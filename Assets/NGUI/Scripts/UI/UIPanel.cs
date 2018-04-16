//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright 漏 2011-2013 Tasharen Entertainment
//----------------------------------------------

#if UNITY_FLASH || UNITY_WP8 || UNITY_METRO
#define USE_SIMPLE_DICTIONARY
#endif

using UnityEngine;
using System.Collections.Generic;

#if !USE_SIMPLE_DICTIONARY
using System.Collections.Specialized;
#endif

/// <summary>
/// UI Panel is responsible for collecting, sorting and updating widgets in addition to generating widgets' geometry.
/// </summary>

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Panel")]
public class UIPanel : MonoBehaviour
{
	public enum DebugInfo
	{
		None,
		Gizmos,
		Geometry,
	}

	/// <summary>
	/// Whether this panel will show up in the panel tool (set this to 'false' for dynamically created temporary panels)
	/// </summary>

	public bool showInPanelTool = true;

	public bool isTop=false;

	public bool isLeft=false;

	/// <summary>
	/// Whether normals and tangents will be generated for all meshes
	/// </summary>
	
	public bool generateNormals = false;

	/// <summary>
	/// Whether the panel will create an additional pass to write to depth.
	/// Turning this on will double the number of draw calls, but will reduce fillrate.
	/// In order to make the most out of this feature, move your widgets on the Z and minimize the amount of visible transparency.
	/// </summary>

	public bool depthPass = false;

	/// <summary>
	/// Whether widgets drawn by this panel are static (won't move). This will improve performance.
	/// </summary>

	public bool widgetsAreStatic = false;

	// Panel's alpha (affects the alpha of all widgets)
	[HideInInspector][SerializeField] float mAlpha = 1f;

	// Whether generated geometry is shown or hidden
	[HideInInspector][SerializeField] DebugInfo mDebugInfo = DebugInfo.Gizmos;

	// Clipping rectangle
	[HideInInspector][SerializeField] UIDrawCall.Clipping mClipping = UIDrawCall.Clipping.None;
	[HideInInspector][SerializeField] Vector4 mClipRange = Vector4.zero;
	[HideInInspector][SerializeField] Vector2 mClipSoftness = new Vector2(40f, 40f);

	// List of managed transforms
#if USE_SIMPLE_DICTIONARY
	Dictionary<Transform, UINode> mChildren = new Dictionary<Transform, UINode>();
#else
	OrderedDictionary mChildren = new OrderedDictionary();
#endif

	// List of all widgets managed by this panel
	BetterList<UIWidget> mWidgets = new BetterList<UIWidget>();

	// Widgets using these materials will be rebuilt next frame
	BetterList<Material> mChanged = new BetterList<Material>();

	// List of UI Screens created on hidden and invisible game objects
	BetterList<UIDrawCall> mDrawCalls = new BetterList<UIDrawCall>();

	// Cached in order to reduce memory allocations
	BetterList<Vector3> mVerts = new BetterList<Vector3>();
	BetterList<Vector3> mNorms = new BetterList<Vector3>();
	BetterList<Vector4> mTans = new BetterList<Vector4>();
	BetterList<Vector2> mUvs = new BetterList<Vector2>();
	BetterList<Color32> mCols = new BetterList<Color32>();

	Transform mTrans;
	Camera mCam;
	int mLayer = -1;
	bool mDepthChanged = false;
	bool mRebuildAll = false;
	bool mChangedLastFrame = false;
	bool mWidgetsAdded = false;




	float mUpdateTime = 0f;
	float mMatrixTime = 0f;
	Matrix4x4 mWorldToLocal = Matrix4x4.identity;

	// Values used for visibility checks
	static float[] mTemp = new float[4];
	Vector2 mMin = Vector2.zero;
	Vector2 mMax = Vector2.zero;

	// When traversing through the child dictionary, deleted values are stored here
	List<Transform> mRemoved = new List<Transform>();

	// Used for SetAlphaRecursive()
	UIPanel[] mChildPanels;

	// Whether the panel should check the visibility of its widgets (set when the clip range changes).
	bool mCheckVisibility = false;
	float mCullTime = 0f;
	bool mCulled = false;

#if UNITY_EDITOR
	// Screen size, saved for gizmos, since Screen.width and Screen.height returns the Scene view's dimensions in OnDrawGizmos.
	Vector2 mScreenSize = Vector2.one;
#endif

	/// <summary>
	/// Cached for speed.
	/// </summary>

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	/// <summary>
	/// Whether the panel's geometry has changed in the past (or current) frame.
	/// </summary>

	public bool changedLastFrame { get { return mChangedLastFrame; } }

	/// <summary>
	/// Panel's alpha affects everything drawn by the panel.
	/// </summary>
	/// 
	/// 


//	void Start()
//	{
//
//	}

	public float alpha
	{
		get
		{
			return mAlpha;
		}
		set
		{
			float val = Mathf.Clamp01(value);

			if (mAlpha != val)
			{
				mAlpha = val;
				mCheckVisibility = true;

				for (int i = 0; i < mDrawCalls.size; ++i)
				{
					UIDrawCall dc = mDrawCalls[i];
					MarkMaterialAsChanged(dc.material, false);
				}

				for (int i = 0; i < mWidgets.size; ++i)
				{
					mWidgets[i].MarkAsChangedLite();
				}
			}
		}
	}

	/// <summary>
	/// Recursively set the alpha for this panel and all of its children.
	/// </summary>

	public void SetAlphaRecursive (float val, bool rebuildList)
	{
		if (rebuildList || mChildPanels == null)
			mChildPanels = GetComponentsInChildren<UIPanel>(true);
		for (int i = 0, imax = mChildPanels.Length; i < imax; ++i)
			mChildPanels[i].alpha = val;
	}

	/// <summary>
	/// Whether the panel's generated geometry will be hidden or not.
	/// </summary>

	public DebugInfo debugInfo
	{
		get
		{
			return mDebugInfo;
		}
		set
		{
			if (mDebugInfo != value)
			{
				mDebugInfo = value;
				BetterList<UIDrawCall> list = drawCalls;
				HideFlags flags = (mDebugInfo == DebugInfo.Geometry) ? HideFlags.DontSave | HideFlags.NotEditable : HideFlags.HideAndDontSave;

				for (int i = 0, imax = list.size; i < imax;  ++i)
				{
					UIDrawCall dc = list[i];
					GameObject go = dc.gameObject;
					NGUITools.SetActiveSelf(go, false);
					go.hideFlags = flags;
					NGUITools.SetActiveSelf(go, true);
				}
			}
		}
	}

	/// <summary>
	/// Clipping method used by all draw calls.
	/// </summary>

	public UIDrawCall.Clipping clipping
	{
		get
		{
			return mClipping;
		}
		set
		{
			if (mClipping != value)
			{
				mCheckVisibility = true;
				mClipping = value;
				mMatrixTime = 0f;
				UpdateDrawcalls();
			}
		}
	}

	/// <summary>
	/// Clipping position (XY) and size (ZW).
	/// </summary>

	public Vector4 clipRange
	{
		get
		{
			return mClipRange;
		}
		set
		{
			if (mClipRange != value)
			{
				mCullTime = (mCullTime == 0f) ? 0.001f : Time.realtimeSinceStartup + 0.15f;
				mCheckVisibility = true;
				mClipRange = value;
				mMatrixTime = 0f;
				UpdateDrawcalls();
			}
		}
	}

	/// <summary>
	/// Clipping softness is used if the clipped style is set to "Soft".
	/// </summary>

	public Vector2 clipSoftness { get { return mClipSoftness; } set { if (mClipSoftness != value) { mClipSoftness = value; UpdateDrawcalls(); } } }

	/// <summary>
	/// Widgets managed by this panel.
	/// </summary>

	public BetterList<UIWidget> widgets { get { return mWidgets; } }

	/// <summary>
	/// Retrieve the list of all active draw calls, removing inactive ones in the process.
	/// </summary>

	public BetterList<UIDrawCall> drawCalls
	{
		get
		{
			for (int i = mDrawCalls.size; i > 0; )
			{
				UIDrawCall dc = mDrawCalls[--i];
				if (dc == null) mDrawCalls.RemoveAt(i);
			}
			return mDrawCalls;
		}
	}

	/// <summary>
	/// Helper function to retrieve the node of the specified transform.
	/// </summary>

	UINode GetNode (Transform t)
	{
		UINode node = null;
#if USE_SIMPLE_DICTIONARY
		if (t != null) mChildren.TryGetValue(t, out node);
#else
		if (t != null && mChildren.Contains(t)) node = (UINode)mChildren[t];
#endif
		return node;
	}

	/// <summary>
	/// Returns whether the specified rectangle is visible by the panel. The coordinates must be in world space.
	/// </summary>

	bool IsVisible (Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		UpdateTransformMatrix();

		// Transform the specified points from world space to local space
		a = mWorldToLocal.MultiplyPoint3x4(a);
		b = mWorldToLocal.MultiplyPoint3x4(b);
		c = mWorldToLocal.MultiplyPoint3x4(c);
		d = mWorldToLocal.MultiplyPoint3x4(d);

		mTemp[0] = a.x;
		mTemp[1] = b.x;
		mTemp[2] = c.x;
		mTemp[3] = d.x;

		float minX = Mathf.Min(mTemp);
		float maxX = Mathf.Max(mTemp);

		mTemp[0] = a.y;
		mTemp[1] = b.y;
		mTemp[2] = c.y;
		mTemp[3] = d.y;

		float minY = Mathf.Min(mTemp);
		float maxY = Mathf.Max(mTemp);

		if (maxX < mMin.x) return false;
		if (maxY < mMin.y) return false;
		if (minX > mMax.x) return false;
		if (minY > mMax.y) return false;
		return true;
	}

	/// <summary>
	/// Returns whether the specified world position is within the panel's bounds determined by the clipping rect.
	/// </summary>

	public bool IsVisible (Vector3 worldPos)
	{
		if (mAlpha < 0.001f) return false;
		if (mClipping == UIDrawCall.Clipping.None) return true;
		UpdateTransformMatrix();

		Vector3 pos = mWorldToLocal.MultiplyPoint3x4(worldPos);
		if (pos.x < mMin.x) return false;
		if (pos.y < mMin.y) return false;
		if (pos.x > mMax.x) return false;
		if (pos.y > mMax.y) return false;
		return true;
	}

	/// <summary>
	/// Returns whether the specified widget is visible by the panel.
	/// </summary>

	public bool IsVisible (UIWidget w)
	{
		if (mAlpha < 0.001f) return false;
		if (!w.enabled || !NGUITools.GetActive(w.gameObject) || w.alpha < 0.001f) return false;

		// No clipping? No point in checking.
		if (mClipping == UIDrawCall.Clipping.None) return true;

		Vector2 size = w.relativeSize;
		Vector2 a = Vector2.Scale(w.pivotOffset, size);
		Vector2 b = a;

		a.x += size.x;
		a.y -= size.y;

		// Transform coordinates into world space
		Transform wt = w.cachedTransform;
		Vector3 v0 = wt.TransformPoint(a);
		Vector3 v1 = wt.TransformPoint(new Vector2(a.x, b.y));
		Vector3 v2 = wt.TransformPoint(new Vector2(b.x, a.y));
		Vector3 v3 = wt.TransformPoint(b);
		return IsVisible(v0, v1, v2, v3);
	}

	/// <summary>
	/// Helper function that marks the specified material as having changed so its mesh is rebuilt next frame.
	/// </summary>

	public void MarkMaterialAsChanged (Material mat, bool sort)
	{
		if (mat != null)
		{
			if (sort) mDepthChanged = true;

			if (!mChanged.Contains(mat))
			{
				mChanged.Add(mat);
				mChangedLastFrame = true;
			}
		}
	}

	/// <summary>
	/// Whether the specified transform is being watched by the panel.
	/// </summary>

	public bool WatchesTransform (Transform t)
	{
#if USE_SIMPLE_DICTIONARY
		return t == cachedTransform || mChildren.ContainsKey(t);
#else
		return t == cachedTransform || mChildren.Contains(t);
#endif
	}

	/// <summary>
	/// Add the specified transform to the managed list.
	/// </summary>

	UINode AddTransform (Transform t)
	{
		UINode node = null;
		UINode retVal = null;

		// Add transforms all the way up to the panel
		while (t != null && t != cachedTransform)
		{
#if USE_SIMPLE_DICTIONARY
			if (mChildren.TryGetValue(t, out node))
			{
				if (retVal == null)
					retVal = node;
			}
#else
			if (mChildren.Contains(t))
			{
				if (retVal == null)
					retVal = (UINode)mChildren[t];
			}
#endif
			else
			{
				// The node is not yet managed -- add it to the list
				node = new UINode(t);
				if (retVal == null) retVal = node;
				mChildren.Add(t, node);
			}
			t = t.parent;
		}
		return retVal;
	}

	/// <summary>
	/// Remove the specified transform from the managed list.
	/// </summary>

	void RemoveTransform (Transform t)
	{
		if (t != null)
		{
#if USE_SIMPLE_DICTIONARY
			while (mChildren.Remove(t))
			{
#else
			while (mChildren.Contains(t))
			{
				mChildren.Remove(t);
#endif
				t = t.parent;
				if (t == null || t == mTrans || t.childCount > 1) break;
			}
		}
	}

	/// <summary>
	/// Add the specified widget to the managed list.
	/// </summary>

	public void AddWidget (UIWidget w)
	{
		if (w != null)
		{
#if UNITY_EDITOR
			if (w.cachedTransform.parent != null)
			{
				UIWidget parentWidget = NGUITools.FindInParents<UIWidget>(w.cachedTransform.parent.gameObject);

				if (parentWidget != null)
				{
					w.cachedTransform.parent = parentWidget.cachedTransform.parent;
					Debug.LogError("You should never nest widgets! Parent them to a common game object instead. Forcefully changing the parent.", w);
					
					// If the error above gets triggered, it means that you parented one widget to another.
					// If left unchecked, this may lead to odd behavior in the UI. Consider restructuring your UI.
					// For example, if you were trying to do this:

					// Widget #1
					//  |
					//  +- Widget #2

					// You can do this instead, fixing the problem:

					// GameObject (scale 1, 1, 1)
					//  |
					//  +- Widget #1
					//  |
					//  +- Widget #2
				}
			}
#endif

			UINode node = AddTransform(w.cachedTransform);

			if (node != null)
			{
				node.widget = w;
				w.visibleFlag = 1;

				if (!mWidgets.Contains(w))
				{
					mWidgets.Add(w);

					if (!mChanged.Contains(w.material))
					{
						mChanged.Add(w.material);
						mChangedLastFrame = true;
					}
					mDepthChanged = true;
					mWidgetsAdded = true;
				}
			}
			else
			{
				Debug.LogError("Unable to find an appropriate root for " + NGUITools.GetHierarchy(w.gameObject) +
					"\nPlease make sure that there is at least one game object above this widget!", w.gameObject);
			}
		}
	}

	/// <summary>
	/// Remove the specified widget from the managed list.
	/// </summary>

	public void RemoveWidget (UIWidget w)
	{
		if (w != null)
		{
			// Do we have this node? Mark the widget's material as having been changed
			UINode pc = GetNode(w.cachedTransform);

			if (pc != null)
			{
				// Mark the material as having been changed
				if (pc.visibleFlag == 1 && !mChanged.Contains(w.material))
				{
					mChanged.Add(w.material);
					mChangedLastFrame = true;
				}

				// Remove this transform
				RemoveTransform(w.cachedTransform);
			}
			mWidgets.Remove(w);
		}
	}

	/// <summary>
	/// Get or create a UIScreen responsible for drawing the widgets using the specified material.
	/// </summary>

	UIDrawCall GetDrawCall (Material mat, bool createIfMissing)
	{
		for (int i = 0, imax = drawCalls.size; i < imax; ++i)
		{
			UIDrawCall dc = drawCalls.buffer[i];
			if (dc.material == mat) return dc;
		}

		UIDrawCall sc = null;

		if (createIfMissing)
		{
#if UNITY_EDITOR
			// If we're in the editor, create the game object with hide flags set right away
			GameObject go = UnityEditor.EditorUtility.CreateGameObjectWithHideFlags("_UIDrawCall [" + mat.name + "]",
				(mDebugInfo == DebugInfo.Geometry) ? HideFlags.DontSave | HideFlags.NotEditable : HideFlags.HideAndDontSave);
#else
			GameObject go = new GameObject("_UIDrawCall [" + mat.name + "]");
			//go.hideFlags = HideFlags.DontSave;
			DontDestroyOnLoad(go);
#endif
			go.layer = gameObject.layer;
			sc = go.AddComponent<UIDrawCall>();
			sc.material = mat;
			mDrawCalls.Add(sc);
		}
		return sc;
	}

	/// <summary>
	/// Layer is used to ensure that if it changes, widgets get moved as well.
	/// </summary>

	void Start ()
	{
		mLayer = gameObject.layer;
		UICamera uic = UICamera.FindCameraForLayer(mLayer);
		mCam = (uic != null) ? uic.cachedCamera : NGUITools.FindCameraForLayer(mLayer);
		
	}


	/// <summary>
	/// Mark all widgets as having been changed so the draw calls get re-created.
	/// </summary>

	void OnEnable ()
	{
		for (int i = 0, imax = mWidgets.size; i < imax; ++i) AddWidget(mWidgets.buffer[i]);
		mRebuildAll = true;
	}

	/// <summary>
	/// Destroy all draw calls we've created when this script gets disabled.
	/// </summary>

	void OnDisable ()
	{
		for (int i = mDrawCalls.size; i > 0; )
		{
			UIDrawCall dc = mDrawCalls.buffer[--i];
			if (dc != null) NGUITools.DestroyImmediate(dc.gameObject);
		}
		mDrawCalls.Clear();
		mChanged.Clear();
		mChildren.Clear();
	}

	// Temporary list used in GetChangeFlag()
	static BetterList<UINode> mHierarchy = new BetterList<UINode>();

	/// <summary>
	/// Convenience function that figures out the panel's correct change flag by searching the parents.
	/// </summary>

	int GetChangeFlag (UINode start)
	{
		int flag = start.changeFlag;

		if (flag == -1)
		{
			Transform trans = start.trans.parent;
			UINode sub;

			// Keep going until we find a set flag
			for (;;)
			{
				// Check the parent's flag
#if USE_SIMPLE_DICTIONARY
				if (trans != null && mChildren.TryGetValue(trans, out sub))
				{
#else
				if (trans != null && mChildren.Contains(trans))
				{
					sub = (UINode)mChildren[trans];
#endif
					flag = sub.changeFlag;
					trans = trans.parent;

					// If the flag hasn't been set either, add this child to the hierarchy
					if (flag == -1) mHierarchy.Add(sub);
					else break;
				}
				else
				{
					flag = 0;
					break;
				}
			}

			// Update the parent flags
			for (int i = 0, imax = mHierarchy.size; i < imax; ++i)
			{
				UINode pc = mHierarchy.buffer[i];
				pc.changeFlag = flag;
			}
			mHierarchy.Clear();
		}
		return flag;
	}

	/// <summary>
	/// Update the world-to-local transform matrix as well as clipping bounds.
	/// </summary>

	void UpdateTransformMatrix ()
	{
		if (mUpdateTime == 0f || mMatrixTime != mUpdateTime)
		{
			mMatrixTime = mUpdateTime;
			mWorldToLocal = cachedTransform.worldToLocalMatrix;

			if (mClipping != UIDrawCall.Clipping.None)
			{
				Vector2 size = new Vector2(mClipRange.z, mClipRange.w);

				if (size.x == 0f) size.x = (mCam == null) ? Screen.width  : mCam.pixelWidth;
				if (size.y == 0f) size.y = (mCam == null) ? Screen.height : mCam.pixelHeight;

				size *= 0.5f;

				mMin.x = mClipRange.x - size.x;
				mMin.y = mClipRange.y - size.y;
				mMax.x = mClipRange.x + size.x;
				mMax.y = mClipRange.y + size.y;
			}
		}
	}

	/// <summary>
	/// Run through all managed transforms and see if they've changed.
	/// </summary>

	void UpdateTransforms ()
	{
		mChangedLastFrame = false;
		bool transformsChanged = false;
		bool shouldCull = false;

#if UNITY_EDITOR
		shouldCull = (clipping != UIDrawCall.Clipping.None) && (!Application.isPlaying || mUpdateTime > mCullTime);
		if (!Application.isPlaying || !widgetsAreStatic || mWidgetsAdded || shouldCull != mCulled)
#else
		shouldCull = (clipping != UIDrawCall.Clipping.None) && (mUpdateTime > mCullTime);
		if (!widgetsAreStatic || mWidgetsAdded || shouldCull != mCulled)
#endif
		{
#if USE_SIMPLE_DICTIONARY
			foreach (KeyValuePair<Transform, UINode> child in mChildren)
			{
				UINode node = child.Value;
#else
			for (int i = 0, imax = mChildren.Count; i < imax; ++i)
			{
				UINode node = (UINode)mChildren[i];
#endif
				if (node.trans == null)
				{
					mRemoved.Add(node.trans);
					continue;
				}

				if (node.HasChanged())
				{
					node.changeFlag = 1;
					transformsChanged = true;
#if UNITY_EDITOR
					Vector3 s = node.trans.lossyScale;
					float min = Mathf.Abs(Mathf.Min(s.x, s.y));

					if (min == 0f)
					{
						Debug.LogError("Scale of 0 is invalid! Zero cannot be divided by, which causes problems. Use a small value instead, such as 0.01\n" +
						node.trans.lossyScale, node.trans);
					}
#endif
				}
				else node.changeFlag = -1;
			}

			// Clean up the deleted transforms
			for (int i = 0, imax = mRemoved.Count; i < imax; ++i) mChildren.Remove(mRemoved[i]);
			mRemoved.Clear();
		}

		// If the children weren't culled but should be, check their visibility
		if (!mCulled && shouldCull) mCheckVisibility = true;

		// If something has changed, propagate the changes *down* the tree hierarchy (to children).
		// An alternative (but slower) approach would be to do a pc.trans.GetComponentsInChildren<UIWidget>()
		// in the loop above, and mark each one as dirty.

		if (mCheckVisibility || transformsChanged || mRebuildAll)
		{
#if USE_SIMPLE_DICTIONARY
			foreach (KeyValuePair<Transform, UINode> child in mChildren)
			{
				UINode pc = child.Value;
#else
			for (int i = 0, imax = mChildren.Count; i < imax; ++i)
			{
				UINode pc = (UINode)mChildren[i];
#endif
				if (pc.widget != null)
				{
					int visibleFlag = 1;

					// No sense in checking the visibility if we're not culling anything (as the visibility is always 'true')
					if (shouldCull || transformsChanged)
					{
						// If the change flag has not yet been determined...
						if (pc.changeFlag == -1) pc.changeFlag = GetChangeFlag(pc);

						// Is the widget visible?
						if (shouldCull) visibleFlag = (mCheckVisibility || pc.changeFlag == 1) ? (IsVisible(pc.widget) ? 1 : 0) : pc.visibleFlag;
					}

					// If visibility changed, mark the node as changed as well
					if (pc.visibleFlag != visibleFlag) pc.changeFlag = 1;

					// If the node has changed and the widget is visible (or was visible before)
					if (pc.changeFlag == 1 && (visibleFlag == 1 || pc.visibleFlag != 0))
					{
						// Update the visibility flag
						pc.visibleFlag = visibleFlag;
						Material mat = pc.widget.material;

						// Add this material to the list of changed materials
						if (!mChanged.Contains(mat))
						{
							mChanged.Add(mat);
							mChangedLastFrame = true;
						}
					}
				}
			}
		}
		mCulled = shouldCull;
		mCheckVisibility = false;
		mWidgetsAdded = false;
	}

	/// <summary>
	/// Update all widgets and rebuild their geometry if necessary.
	/// </summary>

	void UpdateWidgets ()
	{
#if USE_SIMPLE_DICTIONARY
		foreach (KeyValuePair<Transform, UINode> c in mChildren)
		{
			UINode pc = c.Value;
#else
		for (int i = 0, imax = mChildren.Count; i < imax; ++i)
		{
			UINode pc = (UINode)mChildren[i];
#endif
			UIWidget w = pc.widget;

			// If the widget is visible, update it
			if (pc.visibleFlag == 1 && w != null && w.UpdateGeometry(this, ref mWorldToLocal, (pc.changeFlag == 1), generateNormals))
			{
				// We will need to refill this buffer
				if (!mChanged.Contains(w.material))
				{
					mChanged.Add(w.material);
					mChangedLastFrame = true;
				}
			}
			pc.changeFlag = 0;
		}
	}

	/// <summary>
	/// Update the clipping rect in the shaders and draw calls' positions.
	/// </summary>

	public void UpdateDrawcalls ()
	{
		Vector4 range = Vector4.zero;

		if (mClipping != UIDrawCall.Clipping.None)
		{
			range = new Vector4(mClipRange.x, mClipRange.y, mClipRange.z * 0.5f, mClipRange.w * 0.5f);
		}

		if (range.z == 0f) range.z = Screen.width * 0.5f;
		if (range.w == 0f) range.w = Screen.height * 0.5f;

		RuntimePlatform platform = Application.platform;

		if (platform == RuntimePlatform.WindowsPlayer ||
			platform == RuntimePlatform.WindowsEditor)
		{
			range.x -= 0.5f;
			range.y += 0.5f;
		}

		Transform t = cachedTransform;

		for (int i = 0, imax = mDrawCalls.size; i < imax; ++i)
		{
			UIDrawCall dc = mDrawCalls.buffer[i];
			dc.clipping = mClipping;
			dc.clipRange = range;
			dc.clipSoftness = mClipSoftness;
			dc.depthPass = depthPass && mClipping == UIDrawCall.Clipping.None;

			// Set the draw call's transform to match the panel's.
			// Note that parenting directly to the panel causes unity to crash as soon as you hit Play.
			Transform dt = dc.transform;
			dt.position = t.position;
			dt.rotation = t.rotation;
			dt.localScale = t.lossyScale;
		}
	}

	/// <summary>
	/// Set the draw call's geometry responsible for the specified material.
	/// </summary>

	void Fill (Material mat)
	{
		// Cleanup deleted widgets
		for (int i = mWidgets.size; i > 0; ) if (mWidgets[--i] == null) mWidgets.RemoveAt(i);

		// Fill the buffers for the specified material
		for (int i = 0, imax = mWidgets.size; i < imax; ++i)
		{
			UIWidget w = mWidgets.buffer[i];

			if (w.visibleFlag == 1 && w.material == mat)
			{
				UINode node = GetNode(w.cachedTransform);

				if (node != null)
				{
					if (generateNormals) w.WriteToBuffers(mVerts, mUvs, mCols, mNorms, mTans);
					else w.WriteToBuffers(mVerts, mUvs, mCols, null, null);
				}
				else
				{
					Debug.LogError("No transform found for " + NGUITools.GetHierarchy(w.gameObject), this);
				}
			}
		}

		if (mVerts.size > 0)
		{
			// Rebuild the draw call's mesh
			UIDrawCall dc = GetDrawCall(mat, true);
			dc.depthPass = depthPass && mClipping == UIDrawCall.Clipping.None;
			dc.Set(mVerts, generateNormals ? mNorms : null, generateNormals ? mTans : null, mUvs, mCols);
		}
		else
		{
			// There is nothing to draw for this material -- eliminate the draw call
			UIDrawCall dc = GetDrawCall(mat, false);

			if (dc != null)
			{
				mDrawCalls.Remove(dc);
				NGUITools.DestroyImmediate(dc.gameObject);
			}
		}

		// Cleanup
		mVerts.Clear();
		mNorms.Clear();
		mTans.Clear();
		mUvs.Clear();
		mCols.Clear();
	}

	/// <summary>
	/// Main update function
	/// </summary>
						//int p =5;
	void LateUpdate ()
	{
		//if(Time.frameCount%p!=0)return;
		mUpdateTime = Time.realtimeSinceStartup;
		UpdateTransformMatrix();
		UpdateTransforms();

		// Always move widgets to the panel's layer
		if (mLayer != gameObject.layer)
		{
			mLayer = gameObject.layer;
			UICamera uic = UICamera.FindCameraForLayer(mLayer);
			mCam = (uic != null) ? uic.cachedCamera : NGUITools.FindCameraForLayer(mLayer);
			SetChildLayer(cachedTransform, mLayer);
			for (int i = 0, imax = drawCalls.size; i < imax; ++i) mDrawCalls.buffer[i].gameObject.layer = mLayer;
		}

		UpdateWidgets();

		// If the depth has changed, we need to re-sort the widgets
		if (mDepthChanged)
		{
			mDepthChanged = false;
			mWidgets.Sort(UIWidget.CompareFunc);
		}

		// Fill the draw calls for all of the changed materials
		for (int i = 0, imax = mChanged.size; i < imax; ++i) Fill(mChanged.buffer[i]);

		// Update the clipping rects
		UpdateDrawcalls();
		mChanged.Clear();
		mRebuildAll = false;

#if UNITY_EDITOR
		mScreenSize = new Vector2(Screen.width, Screen.height);
#endif
	}

	/// <summary>
	/// Immediately refresh the panel.
	/// </summary>

	public void Refresh ()
	{
		UIWidget[] wd = GetComponentsInChildren<UIWidget>();
		for (int i = 0, imax = wd.Length; i < imax; ++i) wd[i].Update();
		LateUpdate();
	}

#if UNITY_EDITOR

	// This is necessary because Screen.height inside OnDrawGizmos will return the size of the Scene window,
	// and we need the size of the game window in order to draw the bounds properly.
	int mScreenHeight = 720;
	void Update () { mScreenHeight = Screen.height; }

	/// <summary>
	/// Draw a visible pink outline for the clipped area.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (mDebugInfo == DebugInfo.Gizmos)
		{
			bool clip = (mClipping != UIDrawCall.Clipping.None);
			Vector2 size = clip ? new Vector2(mClipRange.z, mClipRange.w) : Vector2.zero;

			GameObject go = UnityEditor.Selection.activeGameObject;
			bool selected = (go != null) && (NGUITools.FindInParents<UIPanel>(go) == this);

			if (selected || clip || (mCam != null && mCam.orthographic))
			{
				if (size.x == 0f) size.x = mScreenSize.x;
				if (size.y == 0f) size.y = mScreenSize.y;

				if (!clip)
				{
					UIRoot root = NGUITools.FindInParents<UIRoot>(gameObject);
					if (root != null) size *= root.GetPixelSizeAdjustment(mScreenHeight);
				}

				Transform t = clip ? transform : (mCam != null ? mCam.transform : null);

				if (t != null)
				{
					Vector3 pos = new Vector2(mClipRange.x, mClipRange.y);

					Gizmos.matrix = t.localToWorldMatrix;

					if (go != gameObject)
					{
						Gizmos.color = clip ? Color.magenta : new Color(0.5f, 0f, 0.5f);
						Gizmos.DrawWireCube(pos, size);

						// Make the panel selectable
						//Gizmos.color = Color.clear;
						//Gizmos.DrawCube(pos, size);
					}
					else
					{
						Gizmos.color = Color.green;
						Gizmos.DrawWireCube(pos, size);
					}
				}
			}
		}
	}
#endif

	/// <summary>
	/// Calculate the offset needed to be constrained within the panel's bounds.
	/// </summary>

	public Vector3 CalculateConstrainOffset (Vector2 min, Vector2 max)
	{

//		Vector4 clipRange=new Vector4();
//		clipRange.x=this.clipRange.x;
//		clipRange.y=this.clipRange.y;
//		clipRange.w=this.clipRange.w;
//		clipRange.z=this.clipRange.z;
//		if(isTop&&(max.y-min.y)<clipRange.w)
//		{
////			clipRange.w=max.y-min.y;
////			clipRange.y=this.clipRange.y+(this.clipRange.w-clipRange.w);
//			clipRange.y=-clipRange.w*0.05f;
//		}

		float offsetX = clipRange.z * 0.5f;
		float offsetY = clipRange.w * 0.5f;
		
								

		Vector2 minRect = new Vector2(min.x, min.y);
		Vector2 maxRect = new Vector2(max.x, max.y);
		Vector2 minArea = new Vector2(clipRange.x - offsetX, clipRange.y - offsetY);
		Vector2 maxArea = new Vector2(clipRange.x + offsetX, clipRange.y + offsetY);

		if (clipping == UIDrawCall.Clipping.SoftClip)
		{
			minArea.x += clipSoftness.x;
			minArea.y += clipSoftness.y;
			maxArea.x -= clipSoftness.x;
			maxArea.y -= clipSoftness.y;
		}

		Vector2 v2 =NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea);
		if(isTop&&(max.y-min.y)<clipRange.w)
		{
			
			float temY = -clipRange.w/2;
		
			v2.y=-temY+ clipRange.y;
//			Debug.Log("temY:"+temY+" v2.y:"+v2.y+"  clipRqnge.y:"+clipRange.y);
		}
//		Debug.Log("max:"+max+" min:"+min+" clipRange.z:"+clipRange.z+" clipRange.w"+clipRange.w);
		if(isLeft&&(max.x-min.x)<clipRange.z)
		{
			float temX = -clipRange.z/2;
			
			v2.x=temX+ clipRange.x;
//			Debug.Log("temX:"+temX+" v2.x:"+v2.x+"  clipRqnge.z:"+clipRange.z);
		}
		//Debug.Log("vvv2:"+v2);
		return v2;
	}

	/// <summary>
	/// Constrain the current target position to be within panel bounds.
	/// </summary>

	public bool ConstrainTargetToBounds (Transform target, ref Bounds targetBounds, bool immediate)
	{
		Vector3 offset = CalculateConstrainOffset(targetBounds.min, targetBounds.max);

		if (offset.magnitude > 0f)
		{
			if (immediate)
			{
				target.localPosition += offset;
				targetBounds.center += offset;
				SpringPosition sp = target.GetComponent<SpringPosition>();
				if (sp != null) sp.enabled = false;
			}
			else
			{
				SpringPosition sp = SpringPosition.Begin(target.gameObject, target.localPosition + offset, 13f);
				sp.ignoreTimeScale = true;
				sp.worldSpace = false;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Constrain the specified target to be within the panel's bounds.
	/// </summary>

	public bool ConstrainTargetToBounds (Transform target, bool immediate)
	{
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(cachedTransform, target);
		return ConstrainTargetToBounds(target, ref bounds, immediate);
	}

	/// <summary>
	/// Helper function that recursively sets all childrens' game objects layers to the specified value, stopping when it hits another UIPanel.
	/// </summary>

	static void SetChildLayer (Transform t, int layer)
	{
		for (int i = 0; i < t.childCount; ++i)
		{
			Transform child = t.GetChild(i);

			if (child.GetComponent<UIPanel>() == null)
			{
				child.gameObject.layer = layer;
				SetChildLayer(child, layer);
			}
		}
	}

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform.
	/// </summary>

	static public UIPanel Find (Transform trans, bool createIfMissing)
	{
		Transform origin = trans;
		UIPanel panel = null;

		while (panel == null && trans != null)
		{
			panel = trans.GetComponent<UIPanel>();
			if (panel != null) break;
			if (trans.parent == null) break;
			trans = trans.parent;
		}

		if (createIfMissing && panel == null && trans != origin)
		{
			panel = trans.gameObject.AddComponent<UIPanel>();
			SetChildLayer(panel.cachedTransform, panel.gameObject.layer);
		}
		return panel;
	}

	/// <summary>
	/// Find the UIPanel responsible for handling the specified transform, creating a new one if necessary.
	/// </summary>

	static public UIPanel Find (Transform trans) { return Find(trans, true); }
}
