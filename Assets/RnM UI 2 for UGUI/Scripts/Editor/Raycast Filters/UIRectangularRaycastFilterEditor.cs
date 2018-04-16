using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace UnityEditor.UI
{
	[CanEditMultipleObjects, CustomEditor(typeof(UIRectangularRaycastFilter))]
	public class UIRectangularRaycastFilterEditor : Editor {
		
		public const string PREFS_KEY = "UIRectRaycastFilter_DG";
		private bool m_DisplayGeometry = true;
		
		protected void OnEnable()
		{
			this.m_DisplayGeometry = EditorPrefs.GetBool(PREFS_KEY, true);
		}
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			GUI.changed = false;
			
			this.m_DisplayGeometry = EditorGUILayout.Toggle("Display Geometry", this.m_DisplayGeometry);
			
			if (GUI.changed)
			{
				EditorPrefs.SetBool(PREFS_KEY, this.m_DisplayGeometry);
				EditorUtility.SetDirty(target);
			}
		}
		
		
		public Vector3[] scaledWorldCorners
		{
			get
			{
				UIRectangularRaycastFilter filter = this.target as UIRectangularRaycastFilter;
				Canvas canvas = UIUtility.FindInParents<Canvas>(filter.gameObject);
				Rect scaledRect = filter.scaledRect;
				if (canvas != null)
				{
					scaledRect.width *= canvas.scaleFactor;
					scaledRect.height *= canvas.scaleFactor;
					scaledRect.position *= canvas.scaleFactor;
				}
				RectTransform rt = (RectTransform)filter.transform;
				Vector3[] corners = new Vector3[4];
				corners[0] = new Vector3(rt.position.x + scaledRect.x, rt.position.y + scaledRect.y, rt.position.z);
				corners[1] = new Vector3(rt.position.x + scaledRect.x + scaledRect.width, rt.position.y + scaledRect.y, rt.position.z);
				corners[2] = new Vector3(rt.position.x + scaledRect.x + scaledRect.width, rt.position.y + scaledRect.y + scaledRect.height, rt.position.z);
				corners[3] = new Vector3(rt.position.x + scaledRect.x, rt.position.y + scaledRect.y + scaledRect.height, rt.position.z);
				return corners;
			}
		}
		
		protected void OnSceneGUI()
		{
			if (!this.m_DisplayGeometry)
				return;
			
			Vector3[] worldCorners = this.scaledWorldCorners;
			
			Handles.color = Color.green;
			Handles.DrawLine(worldCorners[0], worldCorners[1]); // Left line
			Handles.DrawLine(worldCorners[1], worldCorners[2]); // Top line
			Handles.DrawLine(worldCorners[2], worldCorners[3]); // Right line
			Handles.DrawLine(worldCorners[3], worldCorners[0]); // Bottom line
		}
	}
}