using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(UIRealmSelect_Realm), true)]
	public class UIRealmSelect_RealmEditor : SelectableEditor {
		
		private SerializedProperty m_OnValueChangedProperty;
		private SerializedProperty m_GroupProperty;
		private SerializedProperty m_IsOnProperty;
		
		private SerializedProperty m_TitleStringProperty;
		private SerializedProperty m_IsClosedProperty;
		private SerializedProperty m_CurrentStatusProperty;
		private SerializedProperty m_CurrentPopulationProperty;
		
		private SerializedProperty m_ImageProperty;
		private SerializedProperty m_HoverSpriteProperty;
		private SerializedProperty m_ActiveSpriteProperty;
		
		private SerializedProperty m_TitleTextProperty;
		private SerializedProperty m_TitleNormalColorProperty;
		private SerializedProperty m_TitleHoverColorProperty;
		private SerializedProperty m_TitleClosedColorProperty;
		
		private SerializedProperty m_ClosedTextProperty;
		private SerializedProperty m_ClosedNormalColorProperty;
		private SerializedProperty m_ClosedHoverColorProperty;
		
		private SerializedProperty m_StatusTextProperty;
		private SerializedProperty m_StatusOnlineColorProperty;
		private SerializedProperty m_StatusOfflineColorProperty;
		private SerializedProperty m_StatusOnlineStringProperty;
		private SerializedProperty m_StatusOfflineStringProperty;
		
		private SerializedProperty m_PopulationTextProperty;
		private SerializedProperty m_PopulationLowColorProperty;
		private SerializedProperty m_PopulationMediumColorProperty;
		private SerializedProperty m_PopulationHighColorProperty;
		
		private SerializedProperty m_CharactersTextProperty;
				
		protected override void OnEnable()
		{
			base.OnEnable();
			
			this.m_GroupProperty = base.serializedObject.FindProperty("m_Group");
			this.m_IsOnProperty = base.serializedObject.FindProperty("m_IsOn");
			this.m_OnValueChangedProperty = base.serializedObject.FindProperty("onValueChanged");
			
			this.m_TitleStringProperty = this.serializedObject.FindProperty("m_TitleString");
			this.m_IsClosedProperty = this.serializedObject.FindProperty("m_IsClosed");
			this.m_CurrentStatusProperty = this.serializedObject.FindProperty("m_CurrentStatus");
			this.m_CurrentPopulationProperty = this.serializedObject.FindProperty("m_CurrentPopulation");
			
			this.m_ImageProperty = this.serializedObject.FindProperty("m_Image");
			this.m_HoverSpriteProperty = this.serializedObject.FindProperty("m_HoverSprite");
			this.m_ActiveSpriteProperty = this.serializedObject.FindProperty("m_ActiveSprite");
			
			this.m_TitleTextProperty = this.serializedObject.FindProperty("m_TitleText");
			this.m_TitleNormalColorProperty = this.serializedObject.FindProperty("m_TitleNormalColor");
			this.m_TitleHoverColorProperty = this.serializedObject.FindProperty("m_TitleHoverColor");
			this.m_TitleClosedColorProperty = this.serializedObject.FindProperty("m_TitleClosedColor");
			
			this.m_ClosedTextProperty = this.serializedObject.FindProperty("m_ClosedText");
			this.m_ClosedNormalColorProperty = this.serializedObject.FindProperty("m_ClosedNormalColor");
			this.m_ClosedHoverColorProperty = this.serializedObject.FindProperty("m_ClosedHoverColor");
			
			this.m_StatusTextProperty = this.serializedObject.FindProperty("m_StatusText");
			this.m_StatusOnlineColorProperty = this.serializedObject.FindProperty("m_StatusOnlineColor");
			this.m_StatusOfflineColorProperty = this.serializedObject.FindProperty("m_StatusOfflineColor");
			this.m_StatusOnlineStringProperty = this.serializedObject.FindProperty("m_StatusOnlineString");
			this.m_StatusOfflineStringProperty = this.serializedObject.FindProperty("m_StatusOfflineString");
			
			this.m_PopulationTextProperty = this.serializedObject.FindProperty("m_PopulationText");
			this.m_PopulationLowColorProperty = this.serializedObject.FindProperty("m_PopulationLowColor");
			this.m_PopulationMediumColorProperty = this.serializedObject.FindProperty("m_PopulationMediumColor");
			this.m_PopulationHighColorProperty = this.serializedObject.FindProperty("m_PopulationHighColor");
			
			this.m_CharactersTextProperty = this.serializedObject.FindProperty("m_CharactersText");
		}
		
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("General Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_TitleStringProperty, new GUIContent("Realm Title"));
			EditorGUILayout.PropertyField(this.m_IsClosedProperty, new GUIContent("Is Closed"));
			EditorGUILayout.PropertyField(this.m_CurrentStatusProperty, new GUIContent("Status"));
			EditorGUILayout.PropertyField(this.m_CurrentPopulationProperty, new GUIContent("Population"));
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Image Layout Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_ImageProperty, new GUIContent("Target Image"));
			if (this.m_ImageProperty.objectReferenceValue != null)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_HoverSpriteProperty, new GUIContent("Highlight"));
				EditorGUILayout.PropertyField(this.m_ActiveSpriteProperty, new GUIContent("Active"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Title Text Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_TitleTextProperty, new GUIContent("Text Component"));
			if (this.m_TitleTextProperty.objectReferenceValue != null)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_TitleNormalColorProperty, new GUIContent("Normal"));
				EditorGUILayout.PropertyField(this.m_TitleHoverColorProperty, new GUIContent("Highlighted"));
				EditorGUILayout.PropertyField(this.m_TitleClosedColorProperty, new GUIContent("Closed"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Closed Text Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_ClosedTextProperty, new GUIContent("Text Component"));
			if (this.m_ClosedTextProperty.objectReferenceValue != null)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_ClosedNormalColorProperty, new GUIContent("Normal"));
				EditorGUILayout.PropertyField(this.m_ClosedHoverColorProperty, new GUIContent("Highlighted"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Status Text Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_StatusTextProperty, new GUIContent("Text Component"));
			if (this.m_StatusTextProperty.objectReferenceValue != null)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_StatusOnlineColorProperty, new GUIContent("Online"));
				EditorGUILayout.PropertyField(this.m_StatusOnlineStringProperty, new GUIContent(" "));
				EditorGUILayout.PropertyField(this.m_StatusOfflineColorProperty, new GUIContent("Offline"));
				EditorGUILayout.PropertyField(this.m_StatusOfflineStringProperty, new GUIContent(" "));
				
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Population Text Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_PopulationTextProperty, new GUIContent("Text Component"));
			if (this.m_PopulationTextProperty.objectReferenceValue != null)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_PopulationLowColorProperty, new GUIContent("Low"));
				EditorGUILayout.PropertyField(this.m_PopulationMediumColorProperty, new GUIContent("Medium"));
				EditorGUILayout.PropertyField(this.m_PopulationHighColorProperty, new GUIContent("High"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Characters Text Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_CharactersTextProperty, new GUIContent("Text Component"));
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			this.serializedObject.ApplyModifiedProperties();
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Selectable Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			base.OnInspectorGUI();
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Toggle Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			this.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_IsOnProperty, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_GroupProperty, new GUILayoutOption[0]);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.m_OnValueChangedProperty, new GUILayoutOption[0]);
			this.serializedObject.ApplyModifiedProperties();
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
	}
}