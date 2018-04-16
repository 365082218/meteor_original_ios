using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
	[CanEditMultipleObjects, CustomEditor(typeof(HorizontalOrVerticalLayoutGroupExtended), true)]
	public class HorizontalOrVerticalLayoutGroupExtendedInspector : HorizontalOrVerticalLayoutGroupEditor
	{
		private SerializedProperty m_SubtractMarginHorizontal;
		private SerializedProperty m_SubtractMarginVertical;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			this.m_SubtractMarginHorizontal = base.serializedObject.FindProperty("m_SubtractMarginHorizontal");
			this.m_SubtractMarginVertical = base.serializedObject.FindProperty("m_SubtractMarginVertical");
		}
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			base.serializedObject.Update();
			EditorGUIUtility.labelWidth = 120f;
			Rect rect = EditorGUILayout.GetControlRect();
			rect = EditorGUI.PrefixLabel(rect, -1, new GUIContent("Subtract Margin"));
			rect.width = Mathf.Max(80f, (rect.width - 4f) / 3f);
			EditorGUIUtility.labelWidth = 80f;
			this.ToggleLeft(rect, this.m_SubtractMarginHorizontal, new GUIContent("Horizontal"));
			rect.x = rect.x + (rect.width + 2f);
			this.ToggleLeft(rect, this.m_SubtractMarginVertical, new GUIContent("Vertical"));
			base.serializedObject.ApplyModifiedProperties();
		}
		
		private void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
		{
			bool flag = property.boolValue;
			EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
			EditorGUI.BeginChangeCheck();
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			flag = EditorGUI.ToggleLeft(position, label, flag);
			EditorGUI.indentLevel = indentLevel;
			if (EditorGUI.EndChangeCheck())
			{
				property.boolValue = (property.hasMultipleDifferentValues || !property.boolValue);
			}
			EditorGUI.showMixedValue = false;
		}
	}
}
