using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(UICharacterSelect_Unit), true)]
	public class UICharacterSelect_UnitEditor : ToggleEditor {
		
		public SerializedProperty nameTextComponentProperty;
		public SerializedProperty nameColorsProperty;
		public SerializedProperty raceTextComponentProperty;
		public SerializedProperty raceColorsProperty;
		public SerializedProperty classTextComponentProperty;
		public SerializedProperty classColorsProperty;
		public SerializedProperty levelTextComponentProperty;
		public SerializedProperty levelLabelTextComponentProperty;
		public SerializedProperty levelColorsProperty;
		public SerializedProperty deleteButtonProperty;
		public SerializedProperty deleteButtonAlwaysVisibleProperty;
		public SerializedProperty deleteButtonFadeDurationProperty;
		
		protected override void OnEnable()
		{
			base.OnEnable();
			
			this.nameTextComponentProperty = this.serializedObject.FindProperty("nameTextComponent");
			this.nameColorsProperty = this.serializedObject.FindProperty("nameColors");
			this.raceTextComponentProperty = this.serializedObject.FindProperty("raceTextComponent");
			this.raceColorsProperty = this.serializedObject.FindProperty("raceColors");
			this.classTextComponentProperty = this.serializedObject.FindProperty("classTextComponent");
			this.classColorsProperty = this.serializedObject.FindProperty("classColors");
			this.levelTextComponentProperty = this.serializedObject.FindProperty("levelTextComponent");
			this.levelLabelTextComponentProperty = this.serializedObject.FindProperty("levelLabelTextComponent");
			this.levelColorsProperty = this.serializedObject.FindProperty("levelColors");
			this.deleteButtonProperty = this.serializedObject.FindProperty("deleteButton");
			this.deleteButtonAlwaysVisibleProperty = this.serializedObject.FindProperty("deleteButtonAlwaysVisible");
			this.deleteButtonFadeDurationProperty = this.serializedObject.FindProperty("deleteButtonFadeDuration");
		}
		
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Name Layout Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(nameTextComponentProperty, new GUIContent("Name Text"));
			if (nameTextComponentProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.nameColorsProperty, true);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Race Layout Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.raceTextComponentProperty, new GUIContent("Race Text"));
			if (this.raceTextComponentProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.raceColorsProperty, true);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Class Layout Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.classTextComponentProperty, new GUIContent("Class Text"));
			if (this.classTextComponentProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.classColorsProperty, true);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Level Layout Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.levelTextComponentProperty, new GUIContent("Level Text"));
			EditorGUILayout.PropertyField(this.levelLabelTextComponentProperty, new GUIContent("Level Label Text"));
			if (this.levelTextComponentProperty.objectReferenceValue != null || this.levelLabelTextComponentProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.levelColorsProperty, true);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Delete Button Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.deleteButtonProperty, new GUIContent("Button"));
			EditorGUILayout.PropertyField(this.deleteButtonAlwaysVisibleProperty, new GUIContent("Always Visible"));
			if (this.deleteButtonAlwaysVisibleProperty.boolValue != true)
				EditorGUILayout.PropertyField(this.deleteButtonFadeDurationProperty, new GUIContent("Fade Duration"));
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			this.serializedObject.ApplyModifiedProperties();
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.LabelField("Toggle Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			base.OnInspectorGUI();
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
	}
}