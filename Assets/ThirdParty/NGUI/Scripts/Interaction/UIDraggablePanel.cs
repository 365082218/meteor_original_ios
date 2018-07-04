//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// This script, when attached to a panel allows dragging of the said panel's contents efficiently by using UIDragPanelContents.
/// </summary>

[ExecuteInEditMode]
[RequireComponent(typeof(UIPanel))]
[AddComponentMenu("NGUI/Interaction/Draggable Panel")]
public class UIDraggablePanel : IgnoreTimeScale
{
	public enum DragEffect
	{
		None,
		Momentum,
		MomentumAndSpring,
	}

	public enum ShowCondition
	{
		Always,
		OnlyIfNeeded,
		WhenDragging,
	}

	public delegate void OnDragFinished ();

	/// <summary>
	/// Whether the dragging will be restricted to be within the parent panel's bounds.
	/// </summary>

	public bool restrictWithinPanel = true;

	/// <summary>
	/// Whether dragging will be disabled if the contents fit.
	/// </summary>

	public bool disableDragIfFits = false;

	/// <summary>
	/// Effect to apply when dragging.
	/// </summary>

	public DragEffect dragEffect = DragEffect.MomentumAndSpring;

	/// <summary>
	/// Whether the drag operation will be started smoothly, or if if it will be precise (but will have a noticeable "jump").
	/// </summary>

	public bool smoothDragStart = true;

	/// <summary>
	/// Scale value applied to the drag delta. Set X or Y to 0 to disallow dragging in that direction.
	/// </summary>

	public Vector3 scale = Vector3.one;

	/// <summary>
	/// Effect the scroll wheel will have on the momentum.
	/// </summary>

	public float scrollWheelFactor = 0f;

	/// <summary>
	/// How much momentum gets applied when the press is released after dragging.
	/// </summary>

	public float momentumAmount = 35f;

	/// <summary>
	/// Starting position of the clipped area. (0, 0) means top-left corner, (1, 1) means bottom-right.
	/// </summary>

	public Vector2 relativePositionOnReset = Vector2.zero;

	/// <summary>
	/// Whether the position will be reset to the 'startingDragAmount'. Inspector-only value.
	/// </summary>

	public bool repositionClipping = false;
	
	/// <summary>
	/// Whether to use iOS drag emulation, where the content only drags at half the speed of the touch/mouse movement when the content edge is within the clipping area.
	/// </summary>	
	
	public bool iOSDragEmulation = true;
	
	/// <summary>
	/// Horizontal scrollbar used for visualization.
	/// </summary>

	public UIScrollBar horizontalScrollBar;

	/// <summary>
	/// Vertical scrollbar used for visualization.
	/// </summary>

	public UIScrollBar verticalScrollBar;

	/// <summary>
	/// Condition that must be met for the scroll bars to become visible.
	/// </summary>

	public ShowCondition showScrollBars = ShowCondition.OnlyIfNeeded;

	/// <summary>
	/// Event callback to trigger when the drag process finished. Can be used for additional effects, such as centering on some object.
	/// </summary>

	public OnDragFinished onDragFinished;

	public UISprite leftArrow;

	public UISprite rightArrow;

	Transform mTrans;
	UIPanel mPanel;
	Plane mPlane;
	Vector3 mLastPos;
	bool mPressed = false;
	Vector3 mMomentum = Vector3.zero;
	float mScroll = 0f;
	Bounds mBounds;
	bool mCalculatedBounds = false;
	bool mShouldMove = false;
	bool mIgnoreCallbacks = false;
	int mDragID = -10;
	Vector2 mDragStartOffset = Vector2.zero;
	bool mDragStarted = false;


	#region God
	public bool GodMove = false;
	bool mStartMove = false;
	bool mMoveBack = true;
	int mCurIndex = 0;
	Vector3 mStartPos;
	Vector3 mStopPos;
	Vector3 mMoveTarget = Vector3.zero;
	UIGrid mGrid;
	public int CurIndex { get { return mCurIndex ;} set { mCurIndex = value ;} }
	public delegate void OnGodDragFinish();
	public OnGodDragFinish OnGodDragFinished;
	#endregion 
	


	/// <summary>
	/// Panel that's being dragged.
	/// </summary>

	public UIPanel panel { get { return mPanel; } }

	/// <summary>
	/// Calculate the bounds used by the widgets.
	/// </summary>

	public Bounds bounds
	{
		get
		{
			if (!mCalculatedBounds)
			{
				mCalculatedBounds = true;
				mBounds = NGUIMath.CalculateRelativeWidgetBounds(mTrans, mTrans);
			}
			return mBounds;
		}
	}

	/// <summary>
	/// Whether the panel should be able to move horizontally (contents don't fit).
	/// </summary>

	public bool shouldMoveHorizontally
	{
		get
		{
			float size = bounds.size.x;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip) size += mPanel.clipSoftness.x * 2f;
			return size > mPanel.clipRange.z;
		}
	}

	/// <summary>
	/// Whether the panel should be able to move vertically (contents don't fit).
	/// </summary>

	public bool shouldMoveVertically
	{
		get
		{
			float size = bounds.size.y;
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip) size += mPanel.clipSoftness.y * 2f;
			return size > mPanel.clipRange.w;
		}
	}

	/// <summary>
	/// Whether the contents of the panel should actually be draggable depends on whether they currently fit or not.
	/// </summary>

	bool shouldMove
	{
		get
		{
			if (!disableDragIfFits) return true;

			if (mPanel == null) mPanel = GetComponent<UIPanel>();
			Vector4 clip = mPanel.clipRange;
			Bounds b = bounds;

			float hx = (clip.z == 0f) ? Screen.width  : clip.z * 0.5f;
			float hy = (clip.w == 0f) ? Screen.height : clip.w * 0.5f;

			if (!Mathf.Approximately(scale.x, 0f))
			{
				if (b.min.x < clip.x - hx) return true;
				if (b.max.x > clip.x + hx) return true;
			}

			if (!Mathf.Approximately(scale.y, 0f))
			{
				if (b.min.y < clip.y - hy) return true;
				if (b.max.y > clip.y + hy) return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Current momentum, exposed just in case it's needed.
	/// </summary>

	public Vector3 currentMomentum { get { return mMomentum; } set { mMomentum = value; mShouldMove = true; } }

	/// <summary>
	/// Cache the transform and the panel.
	/// </summary>

	void Awake ()
	{
		mTrans = transform;
		mPanel = GetComponent<UIPanel>();
		mGrid = mPanel.GetComponentInChildren<UIGrid>();
	}

	/// <summary>
	/// Set the initial drag value and register the listener delegates.
	/// </summary>

	void Start ()
	{
		UpdateScrollbars(true);

		if (horizontalScrollBar != null)
		{
			horizontalScrollBar.onChange += OnHorizontalBar;
			horizontalScrollBar.alpha = ((showScrollBars == ShowCondition.Always) || shouldMoveHorizontally) ? 1f : 0f;
		}

		if (verticalScrollBar != null)
		{
			verticalScrollBar.onChange += OnVerticalBar;
			verticalScrollBar.alpha = ((showScrollBars == ShowCondition.Always) || shouldMoveVertically) ? 1f : 0f;
		}
	}

	/// <summary>
	/// Restrict the panel's contents to be within the panel's bounds.
	/// </summary>

	public bool RestrictWithinBounds (bool instant)
	{
		Vector3 constraint = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);

		if (constraint.magnitude > 0.001f)
		{
			if (!instant && dragEffect == DragEffect.MomentumAndSpring)
			{
				// Spring back into place
//				if(mPanel.clipping==UIDrawCall.Clipping.SoftClip)
//				{
//					if((bounds.max.y-bounds.min.y)<mPanel.clipRange.w)
//					{
//						constraint.y =mPanel.clipRange.w/2+ mPanel.clipSoftness.y;
//					}
//				}


				#region God
				if(!GodMove)
					SpringPanel.Begin(mPanel.gameObject, mTrans.localPosition + constraint, 13f);
			    else{
				    float leftPos = - mGrid.cellWidth * (mGrid.transform.childCount - 1);
					float rightPos = 0.0f;
					bool flag = Mathf.Abs(leftPos - (mTrans.localPosition + constraint).x)>
						        Mathf.Abs(rightPos - (mTrans.localPosition + constraint).x);
					
					Vector3 target = mMoveTarget;
				    target.x = flag ? rightPos : leftPos;
					target.y = mTrans.localPosition.y+constraint.y;
					SpringPanel.Begin(mPanel.gameObject, target ,13f);				
				}
				#endregion
			}
			else
			{
				// Jump back into place
				MoveRelative(constraint);
				mMomentum = Vector3.zero;
				mScroll = 0f;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Disable the spring movement.
	/// </summary>

	public void DisableSpring ()
	{
		SpringPanel sp = GetComponent<SpringPanel>();
		if (sp != null) sp.enabled = false;
	}

	/// <summary>
	/// Update the values of the associated scroll bars.
	/// </summary>

	public void UpdateScrollbars (bool recalculateBounds)
	{
		if (mPanel == null) return;

		if (horizontalScrollBar != null || verticalScrollBar != null)
		{
			if (recalculateBounds)
			{
				mCalculatedBounds = false;
				mShouldMove = shouldMove;
			}

			Bounds b = bounds;
			Vector2 bmin = b.min;
			Vector2 bmax = b.max;

			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				Vector2 soft = mPanel.clipSoftness;
				bmin -= soft;
				bmax += soft;
			}

			if (horizontalScrollBar != null && bmax.x > bmin.x)
			{
				Vector4 clip = mPanel.clipRange;
				float extents = clip.z * 0.5f;
				float min = clip.x - extents - b.min.x;
				float max = b.max.x - extents - clip.x;

				float width = bmax.x - bmin.x;
				min = Mathf.Clamp01(min / width);
				max = Mathf.Clamp01(max / width);

				float sum = min + max;
				mIgnoreCallbacks = true;
				horizontalScrollBar.barSize = 1f - sum;
				horizontalScrollBar.scrollValue = (sum > 0.001f) ? min / sum : 0f;
				mIgnoreCallbacks = false;
			}

			if (verticalScrollBar != null && bmax.y > bmin.y)
			{
				Vector4 clip = mPanel.clipRange;
				float extents = clip.w * 0.5f;
				float min = clip.y - extents - bmin.y;
				float max = bmax.y - extents - clip.y;

				float height = bmax.y - bmin.y;
				min = Mathf.Clamp01(min / height);
				max = Mathf.Clamp01(max / height);
				float sum = min + max;

				mIgnoreCallbacks = true;
				verticalScrollBar.barSize = 1f - sum;
				verticalScrollBar.scrollValue = (sum > 0.001f) ? 1f - min / sum : 0f;
				mIgnoreCallbacks = false;
			}
		}
		else if (recalculateBounds)
		{
			mCalculatedBounds = false;
		}
	}

	void UpdataArrow()
	{
		if (mPanel == null) return;
		if(leftArrow!=null||rightArrow!=null)
		{

			
			Bounds b = bounds;
			Vector2 bmin = b.min;
			Vector2 bmax = b.max;
			
			if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				Vector2 soft = mPanel.clipSoftness;
				bmin -= soft;
				bmax += soft;
			}

			if(leftArrow!=null)
			{
				if(bounds.size.x>mPanel.clipRange.z)
				{
					Vector4 clip = mPanel.clipRange;
					float extents = clip.z * 0.5f;
					float min = clip.x - extents - b.min.x;
					float max = b.max.x - extents - clip.x;
					
					float width = bmax.x - bmin.x;
					min = Mathf.Clamp01(min / width);
					max = Mathf.Clamp01(max / width);
					
					float sum = min + max;
//					Debug.Log(sum+"-- "+min+"--"+max);
//					mIgnoreCallbacks = true;
//					horizontalScrollBar.barSize = 1f - sum;
//					horizontalScrollBar.scrollValue = (sum > 0.001f) ? min / sum : 
					leftArrow.gameObject.SetActive(true);
				}
				else
				{
					leftArrow.gameObject.SetActive(false);
				}
			}
		}
		
	}


	/// <summary>
	/// Changes the drag amount of the panel to the specified 0-1 range values.
	/// (0, 0) is the top-left corner, (1, 1) is the bottom-right.
	/// </summary>

	public void SetDragAmount (float x, float y, bool updateScrollbars)
	{
		DisableSpring();

		Bounds b = bounds;
		if (b.min.x == b.max.x || b.min.y == b.max.x) return;
		Vector4 cr = mPanel.clipRange;

		float hx = cr.z * 0.5f;
		float hy = cr.w * 0.5f;
		float left = b.min.x + hx;
		float right = b.max.x - hx;
		float bottom = b.min.y + hy;
		float top = b.max.y - hy;

		if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
		{
			left -= mPanel.clipSoftness.x;
			right += mPanel.clipSoftness.x;
			bottom -= mPanel.clipSoftness.y;
			top += mPanel.clipSoftness.y;
		}

		// Calculate the offset based on the scroll value
		float ox = Mathf.Lerp(left, right, x);
		float oy = Mathf.Lerp(top, bottom, y);

		// Update the position
		if (!updateScrollbars)
		{
			Vector3 pos = mTrans.localPosition;
			if (scale.x != 0f) pos.x += cr.x - ox;
			if (scale.y != 0f) pos.y += cr.y - oy;
			mTrans.localPosition = pos;
		}

		// Update the clipping offset
		cr.x = ox;
		cr.y = oy;
		mPanel.clipRange = cr;

		// Update the scrollbars, reflecting this change
		if (updateScrollbars) UpdateScrollbars(false);
	}

	/// <summary>
	/// Reset the panel's position to the top-left corner.
	/// It's recommended to call this function before AND after you re-populate the panel's contents (ex: switching window tabs).
	/// Another option is to populate the panel's contents, reset its position, then call this function to reposition the clipping.
	/// </summary>

	public void ResetPosition()
	{
		// Invalidate the bounds
		mCalculatedBounds = false;

		// First move the position back to where it would be if the scroll bars got reset to zero
		SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, false);

		// Next move the clipping area back and update the scroll bars
		SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, true);
	}

	/// <summary>
	/// Triggered by the horizontal scroll bar when it changes.
	/// </summary>

	void OnHorizontalBar (UIScrollBar sb)
	{
		if (!mIgnoreCallbacks)
		{
			float x = (horizontalScrollBar != null) ? horizontalScrollBar.scrollValue : 0f;
			float y = (verticalScrollBar != null) ? verticalScrollBar.scrollValue : 0f;
			SetDragAmount(x, y, false);
		}
	}

	/// <summary>
	/// Triggered by the vertical scroll bar when it changes.
	/// </summary>

	void OnVerticalBar (UIScrollBar sb)
	{
		if (!mIgnoreCallbacks)
		{
			float x = (horizontalScrollBar != null) ? horizontalScrollBar.scrollValue : 0f;
			float y = (verticalScrollBar != null) ? verticalScrollBar.scrollValue : 0f;
			SetDragAmount(x, y, false);
		}
	}

	/// <summary>
	/// Move the panel by the specified amount.
	/// </summary>

	public void MoveRelative (Vector3 relative)
	{		
		mTrans.localPosition += relative;
		Vector4 cr = mPanel.clipRange;
		cr.x -= relative.x;
		cr.y -= relative.y;
		mPanel.clipRange = cr;				
		UpdateScrollbars(false);		
	}

	/// <summary>
	/// Move the panel by the specified amount.
	/// </summary>

	public void MoveAbsolute (Vector3 absolute)
	{
		Vector3 a = mTrans.InverseTransformPoint(absolute);
		Vector3 b = mTrans.InverseTransformPoint(Vector3.zero);
		MoveRelative(a - b);
	}

	/// <summary>
	/// Create a plane on which we will be performing the dragging.
	/// </summary>

	public void Press (bool pressed)
	{
		if (smoothDragStart && pressed)
		{
			mDragStarted = false;
			mDragStartOffset = Vector2.zero;
		}

		if (enabled && NGUITools.GetActive(gameObject))
		{
			if (!pressed && mDragID == UICamera.currentTouchID) mDragID = -10;

			mCalculatedBounds = false;
			mShouldMove = shouldMove;
			if (!mShouldMove) return;
			mPressed = pressed;

			if (pressed)
			{
				// Remove all momentum on press
				mMomentum = Vector3.zero;
				mScroll = 0f;
				#region God
				
				if(mMoveBack)
				{
					mStartPos = mTrans.localPosition;
					mMoveBack = false;
				}
				Debug.Log(mStartPos.x);
				mStartMove = true;
				
				#endregion 
				// Disable the spring movement
				DisableSpring();

				// Remember the hit position
				mLastPos = UICamera.lastHit.point;

				// Create the plane to drag along
				mPlane = new Plane(mTrans.rotation * Vector3.back, mLastPos);
			}
			else
			{
				if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None && dragEffect == DragEffect.MomentumAndSpring)
				{
					RestrictWithinBounds(false);
				}
				if (onDragFinished != null) onDragFinished();
			}
		}
	}

	/// <summary>
	/// Drag the object along the plane.
	/// </summary>

	public void Drag ()
	{
		if (enabled && NGUITools.GetActive(gameObject) && mShouldMove)
		{
			if (mDragID == -10) mDragID = UICamera.currentTouchID;
			UICamera.currentTouch.clickNotification = UICamera.ClickNotification.BasedOnDelta;

			// Prevents the drag "jump". Contributed by 'mixd' from the Tasharen forums.
			if (smoothDragStart && !mDragStarted)
			{
				mDragStarted = true;
				mDragStartOffset = UICamera.currentTouch.totalDelta;
			}

			Ray ray = smoothDragStart ?
				UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos - mDragStartOffset) :
				UICamera.currentCamera.ScreenPointToRay(UICamera.currentTouch.pos);

			float dist = 0f;

			if (mPlane.Raycast(ray, out dist))
			{
				Vector3 currentPos = ray.GetPoint(dist);
				Vector3 offset = currentPos - mLastPos;
				mLastPos = currentPos;

				if (offset.x != 0f || offset.y != 0f)
				{
					offset = mTrans.InverseTransformDirection(offset);
					offset.Scale(scale);
					offset = mTrans.TransformDirection(offset);
				}

				// Adjust the momentum
				mMomentum = Vector3.Lerp(mMomentum, mMomentum + offset * (0.01f * momentumAmount), 0.67f);

				// Move the panel
				if (!iOSDragEmulation)
				{
					MoveAbsolute(offset);	
				}
				else
				{
					Vector3 constraint = mPanel.CalculateConstrainOffset(bounds.min, bounds.max);

					if (constraint.magnitude > 0.001f)
					{
						MoveAbsolute(offset * 0.5f);
						mMomentum *= 0.5f;
					}
					else
					{
						MoveAbsolute(offset);
					}
				}

				// We want to constrain the UI to be within bounds
				if (restrictWithinPanel &&
					mPanel.clipping != UIDrawCall.Clipping.None &&
					dragEffect != DragEffect.MomentumAndSpring)
				{
					RestrictWithinBounds(true);
				}
			}
		}
	}

	/// <summary>
	/// If the object should support the scroll wheel, do it.
	/// </summary>

	public void Scroll (float delta)
	{
		if (enabled && NGUITools.GetActive(gameObject) && scrollWheelFactor != 0f)
		{
			DisableSpring();
			mShouldMove = shouldMove;
			if (Mathf.Sign(mScroll) != Mathf.Sign(delta)) mScroll = 0f;
			mScroll += delta * scrollWheelFactor;
		}
	}

	/// <summary>
	/// Apply the dragging momentum.
	/// </summary>


	void LateUpdate ()
	{
		// If the panel's geometry changed, recalculate the bounds
		if (mPanel.changedLastFrame) UpdateScrollbars(true);
		UpdataArrow();
		// Inspector functionality
		if (repositionClipping)
		{
			repositionClipping = false;
			mCalculatedBounds = false;
			SetDragAmount(relativePositionOnReset.x, relativePositionOnReset.y, true);
		}

		if (!Application.isPlaying) return;
		float delta = UpdateRealTimeDelta();

		// Fade the scroll bars if needed
		if (showScrollBars != ShowCondition.Always)
		{
			bool vertical = false;
			bool horizontal = false;

			if (showScrollBars != ShowCondition.WhenDragging || mDragID != -10 || mMomentum.magnitude > 0.01f)
			{
				vertical = shouldMoveVertically;
				horizontal = shouldMoveHorizontally;
			}

			if (verticalScrollBar)
			{
				float alpha = verticalScrollBar.alpha;
				alpha += vertical ? delta * 6f : -delta * 3f;
				alpha = Mathf.Clamp01(alpha);
				if (verticalScrollBar.alpha != alpha) verticalScrollBar.alpha = alpha;
			}

			if (horizontalScrollBar)
			{
				float alpha = horizontalScrollBar.alpha;
				alpha += horizontal ? delta * 6f : -delta * 3f;
				alpha = Mathf.Clamp01(alpha);
				if (horizontalScrollBar.alpha != alpha) horizontalScrollBar.alpha = alpha;
			}
		}

		// Apply momentum
		if (mShouldMove && !mPressed)
		{
			mMomentum -= scale * (mScroll * 0.05f);

			if (mMomentum.magnitude > 0.0001f)
			{
				mScroll = NGUIMath.SpringLerp(mScroll, 0f, 20f, delta);

				// Move the panel
				Vector3 offset = NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
				MoveAbsolute(offset);

				// Restrict the contents to be within the panel's bounds
				if (restrictWithinPanel && mPanel.clipping != UIDrawCall.Clipping.None) RestrictWithinBounds(false);
				return;
			}
			else
			{
				mScroll = 0f;
				mMomentum = Vector3.zero;
				#region GOD
				if( mStartMove && GodMove ){	
					StopMakeChoose();
				}
				#endregion
			}
		}
		else mScroll = 0f;

		// Dampen the momentum
		NGUIMath.SpringDampen(ref mMomentum, 9f, delta);
	}
	
	#region GOD
	void StopMakeChoose()
	{
		Debug.Log("stop");
		mStartMove = false;	
		mStopPos = mTrans.localPosition;	
		Debug.Log(mStopPos.x);
		float diff = Mathf.Abs(mStopPos.x - mStartPos.x);
		int sign = mStopPos.x - mStartPos.x > 0 ? 1 : -1;
		Debug.Log(diff);		
		
		SpringPanel sp = GetComponent<SpringPanel>();
		if(sp == null )  sp = gameObject.AddComponent<SpringPanel>();
	    int moveCount = Mathf.RoundToInt(diff/mGrid.cellWidth);
				
	    float xPos = mMoveTarget.x + moveCount * sign * mGrid.cellWidth;
		Debug.Log(xPos);
		if(xPos  < -mGrid.cellWidth * (mGrid.transform.childCount - 1))
			mMoveTarget.x = -mGrid.cellWidth * (mGrid.transform.childCount -1);	
		else if(xPos > 0.0f)
			mMoveTarget.x = 0.0f;	
		else 
			mMoveTarget.x = xPos;
		
		Debug.Log(mMoveTarget.x);
		mMoveTarget.y = mTrans.localPosition.y;
		mCurIndex = (int)Mathf.Abs( mMoveTarget.x/mPanel.clipRange.z  );
		
		sp.target = mMoveTarget;
		sp.strength = 13f;
		sp.onFinished = null;
		if( OnGodDragFinished != null){
			OnGodDragFinished();
		}
		Debug.Log("move back");
		sp.enabled = true;
		mMoveBack = true;
	}
	#endregion
	
	
#if UNITY_EDITOR

	/// <summary>
	/// Draw a visible orange outline of the bounds.
	/// </summary>

	void OnDrawGizmos ()
	{
		if (mPanel != null && mPanel.debugInfo == UIPanel.DebugInfo.Gizmos)
		{
			Bounds b = bounds;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = new Color(1f, 0.4f, 0f);
			Gizmos.DrawWireCube(new Vector3(b.center.x, b.center.y, b.min.z), new Vector3(b.size.x, b.size.y, 0f));
		}
	}
#endif
}
