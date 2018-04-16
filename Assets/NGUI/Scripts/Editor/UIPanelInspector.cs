//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright 漏 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(UIPanel))]
public class UIPanelInspector : Editor
{
	/// <summary>
	/// Handles & interaction.
	/// </summary>

	public void OnSceneGUI ()
	{
		//Tools.current = Tool.View;

		Event e = Event.current;

		switch (e.type)
		{
			case EventType.MouseUp:
			{
				UIWidget[] widgets = NGUIEditorTools.Raycast(target as UIPanel, e.mousePosition);
				if (widgets.Length > 0) Selection.activeGameObject = widgets[0].gameObject;
			}
			break;

			case EventType.KeyDown:
			{
				if (e.keyCode == KeyCode.Escape)
				{
					Tools.current = Tool.Move;
					Selection.activeGameObject = null;
					e.Use();
				}
			}
			break;
		}
	}

	/// <summary>
	/// Draw the inspector widget.
	/// </summary>

	public override void OnInspectorGUI ()
	{
		UIPanel panel = target as UIPanel;
		BetterList<UIDrawCall> drawcalls = panel.drawCalls;
		EditorGUIUtility.LookLikeControls(80f);

		//NGUIEditorTools.DrawSeparator();
		EditorGUILayout.Space();

		float alpha = EditorGUILayout.Slider("Alpha", panel.alpha, 0f, 1f);

		if (alpha != panel.alpha)
		{
			NGUIEditorTools.RegisterUndo("Panel Alpha", panel);
			panel.alpha = alpha;
		}

		if (panel.showInPanelTool != EditorGUILayout.Toggle("Panel Tool", panel.showInPanelTool))
		{
			panel.showInPanelTool = !panel.showInPanelTool;
			EditorUtility.SetDirty(panel);
			EditorWindow.FocusWindowIfItsOpen<UIPanelTool>();
		}

		panel.isTop = EditorGUILayout.Toggle("IsTop",panel.isTop);
		panel.isLeft = EditorGUILayout.Toggle("IsLeft",panel.isLeft);
		GUILayout.BeginHorizontal();
		bool norms = EditorGUILayout.Toggle("Normals", panel.generateNormals, GUILayout.Width(100f));
		GUILayout.Label("Needed for lit shaders");
		GUILayout.EndHorizontal();

		if (panel.generateNormals != norms)
		{
			panel.generateNormals = norms;
			panel.UpdateDrawcalls();
			EditorUtility.SetDirty(panel);
		}

		GUILayout.BeginHorizontal();
		bool depth = EditorGUILayout.Toggle("Depth Pass", panel.depthPass, GUILayout.Width(100f));
		GUILayout.Label("Doubles draw calls, saves fillrate");
		GUILayout.EndHorizontal();

		if (panel.depthPass != depth)
		{
			panel.depthPass = depth;
			panel.UpdateDrawcalls();
			EditorUtility.SetDirty(panel);
		}

		if (depth)
		{
			UICamera cam = UICamera.FindCameraForLayer(panel.gameObject.layer);

			if (cam == null || cam.GetComponent<Camera>().orthographic)
			{
				EditorGUILayout.HelpBox("Please note that depth pass will only save fillrate when used with 3D UIs, and only UIs drawn by the game camera. If you are using a separate camera for the UI, you will not see any benefit!", MessageType.Warning);
			}
		}

		GUILayout.BeginHorizontal();
		bool stat = EditorGUILayout.Toggle("Static", panel.widgetsAreStatic, GUILayout.Width(100f));
		GUILayout.Label("Check if widgets won't move");
		GUILayout.EndHorizontal();

		if (panel.widgetsAreStatic != stat)
		{
			panel.widgetsAreStatic = stat;
			panel.UpdateDrawcalls();
			EditorUtility.SetDirty(panel);
		}

		EditorGUILayout.LabelField("Widgets", panel.widgets.size.ToString());
		EditorGUILayout.LabelField("Draw Calls", drawcalls.size.ToString());

		UIPanel.DebugInfo di = (UIPanel.DebugInfo)EditorGUILayout.EnumPopup("Debug Info", panel.debugInfo);

		if (panel.debugInfo != di)
		{
			panel.debugInfo = di;
			EditorUtility.SetDirty(panel);
		}

		UIDrawCall.Clipping clipping = (UIDrawCall.Clipping)EditorGUILayout.EnumPopup("Clipping", panel.clipping);

		if (panel.clipping != clipping)
		{
			panel.clipping = clipping;
			EditorUtility.SetDirty(panel);
		}

		if (panel.clipping != UIDrawCall.Clipping.None)
		{
			Vector4 range = panel.clipRange;

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			Vector2 pos = EditorGUILayout.Vector2Field("Center", new Vector2(range.x, range.y));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Space(80f);
			Vector2 size = EditorGUILayout.Vector2Field("Size", new Vector2(range.z, range.w));
			GUILayout.EndHorizontal();

			if (size.x < 0f) size.x = 0f;
			if (size.y < 0f) size.y = 0f;

			range.x = pos.x;
			range.y = pos.y;
			range.z = size.x;
			range.w = size.y;

			if (panel.clipRange != range)
			{
				NGUIEditorTools.RegisterUndo("Clipping Change", panel);
				panel.clipRange = range;
				EditorUtility.SetDirty(panel);
			}

			if (panel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(80f);
				Vector2 soft = EditorGUILayout.Vector2Field("Softness", panel.clipSoftness);
				GUILayout.EndHorizontal();

				if (soft.x < 1f) soft.x = 1f;
				if (soft.y < 1f) soft.y = 1f;

				if (panel.clipSoftness != soft)
				{
					NGUIEditorTools.RegisterUndo("Clipping Change", panel);
					panel.clipSoftness = soft;
					EditorUtility.SetDirty(panel);
				}
			}

#if UNITY_ANDROID || UNITY_IPHONE
			if (PlayerSettings.targetGlesGraphics == TargetGlesGraphics.OpenGLES_1_x)
			{
				EditorGUILayout.HelpBox("Clipping requires shader support!\n\nOpen File -> Build Settings -> Player Settings -> Other Settings, then set:\n\n- Graphics Level: OpenGL ES 2.0.", MessageType.Error);
			}
#endif
		}

		if (clipping == UIDrawCall.Clipping.HardClip)
		{
			EditorGUILayout.HelpBox("Hard clipping has been removed due to major performance issues on certain Android devices. Alpha clipping will be used instead.", MessageType.Warning);
		}

		if (clipping != UIDrawCall.Clipping.None && !NGUIEditorTools.IsUniform(panel.transform.lossyScale))
		{
			EditorGUILayout.HelpBox("Clipped panels must have a uniform scale, or clipping won't work properly!", MessageType.Error);
			
			if (GUILayout.Button("Auto-fix"))
			{
				NGUIEditorTools.FixUniform(panel.gameObject);
			}
		}

		foreach (UIDrawCall dc in drawcalls)
		{
			NGUIEditorTools.DrawSeparator();
			EditorGUILayout.ObjectField("Material", dc.material, typeof(Material), false);
			EditorGUILayout.LabelField("Triangles", dc.triangles.ToString());

			if (clipping != UIDrawCall.Clipping.None && !dc.isClipped)
			{
				EditorGUILayout.HelpBox("You must switch this material's shader to Unlit/Transparent Colored or Unlit/Premultiplied Colored in order for clipping to work.",
					MessageType.Warning);
			}
		}
	}
}
