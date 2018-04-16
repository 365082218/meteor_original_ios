using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
	[CanEditMultipleObjects, CustomEditor(typeof(LayoutElementExtended), true)]
	public class LayoutElementExtendedInspector : LayoutElementEditor
	{
		private SerializedProperty m_Margin;

		protected override void OnEnable()
		{
			base.OnEnable();
			this.m_Margin = this.serializedObject.FindProperty("m_Margin");
		}
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_Margin, true);
			this.serializedObject.ApplyModifiedProperties();
		}
	}
}