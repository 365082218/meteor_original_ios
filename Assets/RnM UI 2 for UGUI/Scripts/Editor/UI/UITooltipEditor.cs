using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(UITooltip), true)]
	public class UITooltipEditor : Editor {
		
		private SerializedProperty m_DefaultWidthProperty;
		private SerializedProperty m_AnchorGraphicProperty;
		private SerializedProperty m_AnchorGraphicOffsetProperty;
		private SerializedProperty m_followMouseProperty;
		private SerializedProperty m_OffsetProperty;
		private SerializedProperty m_AnchoredOffsetProperty;
		private SerializedProperty m_TransitionProperty;
		private SerializedProperty m_TransitionEasingProperty;
		private SerializedProperty m_TransitionDurationProperty;
		private SerializedProperty m_TitleFontProperty;
		private SerializedProperty m_TitleFontStyleProperty;
		private SerializedProperty m_TitleFontSizeProperty;
		private SerializedProperty m_TitleFontLineSpacingProperty;
		private SerializedProperty m_TitleFontColorProperty;
		private SerializedProperty m_TitleTextEffectProperty;
		private SerializedProperty m_TitleTextEffectColorProperty;
		private SerializedProperty m_TitleTextEffectDistanceProperty;
		private SerializedProperty m_TitleTextEffectUseGraphicAlphaProperty;
		private SerializedProperty m_DescriptionFontProperty;
		private SerializedProperty m_DescriptionFontStyleProperty;
		private SerializedProperty m_DescriptionFontSizeProperty;
		private SerializedProperty m_DescriptionFontLineSpacingProperty;
		private SerializedProperty m_DescriptionFontColorProperty;
		private SerializedProperty m_DescriptionTextEffectProperty;
		private SerializedProperty m_DescriptionTextEffectColorProperty;
		private SerializedProperty m_DescriptionTextEffectDistanceProperty;
		private SerializedProperty m_DescriptionTextEffectUseGraphicAlphaProperty;
		private SerializedProperty m_AttributeFontProperty;
		private SerializedProperty m_AttributeFontStyleProperty;
		private SerializedProperty m_AttributeFontSizeProperty;
		private SerializedProperty m_AttributeFontLineSpacingProperty;
		private SerializedProperty m_AttributeFontColorProperty;
		private SerializedProperty m_AttributeTextEffectProperty;
		private SerializedProperty m_AttributeTextEffectColorProperty;
		private SerializedProperty m_AttributeTextEffectDistanceProperty;
		private SerializedProperty m_AttributeTextEffectUseGraphicAlphaProperty;
				
		protected virtual void OnEnable()
		{
			this.m_DefaultWidthProperty = this.serializedObject.FindProperty("m_DefaultWidth");
			this.m_AnchorGraphicProperty = this.serializedObject.FindProperty("m_AnchorGraphic");
			this.m_AnchorGraphicOffsetProperty = this.serializedObject.FindProperty("m_AnchorGraphicOffset");
			this.m_followMouseProperty = this.serializedObject.FindProperty("m_followMouse");
			this.m_OffsetProperty = this.serializedObject.FindProperty("m_Offset");
			this.m_AnchoredOffsetProperty = this.serializedObject.FindProperty("m_AnchoredOffset");
			this.m_TransitionProperty = this.serializedObject.FindProperty("m_Transition");
			this.m_TransitionEasingProperty = this.serializedObject.FindProperty("m_TransitionEasing");
			this.m_TransitionDurationProperty = this.serializedObject.FindProperty("m_TransitionDuration");
			this.m_TitleFontProperty = this.serializedObject.FindProperty("m_TitleFont");
			this.m_TitleFontStyleProperty = this.serializedObject.FindProperty("m_TitleFontStyle");
			this.m_TitleFontSizeProperty = this.serializedObject.FindProperty("m_TitleFontSize");
			this.m_TitleFontLineSpacingProperty = this.serializedObject.FindProperty("m_TitleFontLineSpacing");
			this.m_TitleFontColorProperty = this.serializedObject.FindProperty("m_TitleFontColor");
			this.m_TitleTextEffectProperty = this.serializedObject.FindProperty("m_TitleTextEffect");
			this.m_TitleTextEffectColorProperty = this.serializedObject.FindProperty("m_TitleTextEffectColor");
			this.m_TitleTextEffectDistanceProperty = this.serializedObject.FindProperty("m_TitleTextEffectDistance");
			this.m_TitleTextEffectUseGraphicAlphaProperty = this.serializedObject.FindProperty("m_TitleTextEffectUseGraphicAlpha");
			this.m_DescriptionFontProperty = this.serializedObject.FindProperty("m_DescriptionFont");
			this.m_DescriptionFontStyleProperty = this.serializedObject.FindProperty("m_DescriptionFontStyle");
			this.m_DescriptionFontSizeProperty = this.serializedObject.FindProperty("m_DescriptionFontSize");
			this.m_DescriptionFontLineSpacingProperty = this.serializedObject.FindProperty("m_DescriptionFontLineSpacing");
			this.m_DescriptionFontColorProperty = this.serializedObject.FindProperty("m_DescriptionFontColor");
			this.m_DescriptionTextEffectProperty = this.serializedObject.FindProperty("m_DescriptionTextEffect");
			this.m_DescriptionTextEffectColorProperty = this.serializedObject.FindProperty("m_DescriptionTextEffectColor");
			this.m_DescriptionTextEffectDistanceProperty = this.serializedObject.FindProperty("m_DescriptionTextEffectDistance");
			this.m_DescriptionTextEffectUseGraphicAlphaProperty = this.serializedObject.FindProperty("m_DescriptionTextEffectUseGraphicAlpha");
			this.m_AttributeFontProperty = this.serializedObject.FindProperty("m_AttributeFont");
			this.m_AttributeFontStyleProperty = this.serializedObject.FindProperty("m_AttributeFontStyle");
			this.m_AttributeFontSizeProperty = this.serializedObject.FindProperty("m_AttributeFontSize");
			this.m_AttributeFontLineSpacingProperty = this.serializedObject.FindProperty("m_AttributeFontLineSpacing");
			this.m_AttributeFontColorProperty = this.serializedObject.FindProperty("m_AttributeFontColor");
			this.m_AttributeTextEffectProperty = this.serializedObject.FindProperty("m_AttributeTextEffect");
			this.m_AttributeTextEffectColorProperty = this.serializedObject.FindProperty("m_AttributeTextEffectColor");
			this.m_AttributeTextEffectDistanceProperty = this.serializedObject.FindProperty("m_AttributeTextEffectDistance");
			this.m_AttributeTextEffectUseGraphicAlphaProperty = this.serializedObject.FindProperty("m_AttributeTextEffectUseGraphicAlpha");
		}
		
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			EditorGUILayout.Separator();
			this.DrawGeneralProperties();
			EditorGUILayout.Separator();
			this.DrawAnchorProperties();
			EditorGUILayout.Separator();
			this.DrawTransitionProperties();
			EditorGUILayout.Separator();
			this.DrawTitleProperties();
			EditorGUILayout.Separator();
			this.DrawDescriptionProperties();
			EditorGUILayout.Separator();
			this.DrawAttributeProperties();
			EditorGUILayout.Separator();
			this.serializedObject.ApplyModifiedProperties();
		}
		
		protected void DrawGeneralProperties()
		{
			EditorGUILayout.LabelField("General Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUIUtility.labelWidth = 150f;
			EditorGUILayout.PropertyField(this.m_followMouseProperty, new GUIContent("Follow Mouse"));
			EditorGUILayout.PropertyField(this.m_OffsetProperty, new GUIContent("Offset"));
			EditorGUILayout.PropertyField(this.m_AnchoredOffsetProperty, new GUIContent("Anchored Offset"));
			EditorGUILayout.PropertyField(this.m_DefaultWidthProperty, new GUIContent("Default Width"));
			EditorGUIUtility.labelWidth = 120f;
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		protected void DrawTransitionProperties()
		{
			EditorGUILayout.LabelField("Transition Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			
			EditorGUILayout.PropertyField(this.m_TransitionProperty, new GUIContent("Transition"));
			
			UITooltip.Transition transition = (UITooltip.Transition)this.m_TransitionProperty.enumValueIndex;
			
			if (transition != UITooltip.Transition.None)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				
				if (transition == UITooltip.Transition.Fade)
				{
					EditorGUILayout.PropertyField(this.m_TransitionEasingProperty, new GUIContent("Easing"));
					EditorGUILayout.PropertyField(this.m_TransitionDurationProperty, new GUIContent("Duration"));
				}
				
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		protected void DrawAnchorProperties()
		{
			EditorGUILayout.LabelField("Anchor Graphic Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_AnchorGraphicProperty, new GUIContent("Graphic"));
			EditorGUILayout.PropertyField(this.m_AnchorGraphicOffsetProperty, new GUIContent("Offset"));
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		protected void DrawTitleProperties()
		{
			UITooltip.TextEffectType textEffect = (UITooltip.TextEffectType)this.m_TitleTextEffectProperty.enumValueIndex;
			
			EditorGUILayout.LabelField("Title Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_TitleFontProperty, new GUIContent("Font"));
			EditorGUILayout.PropertyField(this.m_TitleFontStyleProperty, new GUIContent("Font Style"));
			EditorGUILayout.PropertyField(this.m_TitleFontSizeProperty, new GUIContent("Font Size"));
			EditorGUILayout.PropertyField(this.m_TitleFontLineSpacingProperty, new GUIContent("Font Line Spacing"));
			EditorGUILayout.PropertyField(this.m_TitleFontColorProperty, new GUIContent("Color"));
			EditorGUILayout.PropertyField(this.m_TitleTextEffectProperty, new GUIContent("Text Effect"));
			if (textEffect == UITooltip.TextEffectType.Shadow || textEffect == UITooltip.TextEffectType.Outline)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_TitleTextEffectColorProperty, new GUIContent("Effect Color"));
				EditorGUILayout.PropertyField(this.m_TitleTextEffectDistanceProperty, new GUIContent("Effect Distance"));
				EditorGUILayout.PropertyField(this.m_TitleTextEffectUseGraphicAlphaProperty, new GUIContent("Use Graphic Alpha"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		protected void DrawDescriptionProperties()
		{
			UITooltip.TextEffectType textEffect = (UITooltip.TextEffectType)this.m_DescriptionTextEffectProperty.enumValueIndex;
			
			EditorGUILayout.LabelField("Description Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_DescriptionFontProperty, new GUIContent("Font"));
			EditorGUILayout.PropertyField(this.m_DescriptionFontStyleProperty, new GUIContent("Font Style"));
			EditorGUILayout.PropertyField(this.m_DescriptionFontSizeProperty, new GUIContent("Font Size"));
			EditorGUILayout.PropertyField(this.m_DescriptionFontLineSpacingProperty, new GUIContent("Font Line Spacing"));
			EditorGUILayout.PropertyField(this.m_DescriptionFontColorProperty, new GUIContent("Color"));
			EditorGUILayout.PropertyField(this.m_DescriptionTextEffectProperty, new GUIContent("Text Effect"));
			if (textEffect == UITooltip.TextEffectType.Shadow || textEffect == UITooltip.TextEffectType.Outline)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_DescriptionTextEffectColorProperty, new GUIContent("Effect Color"));
				EditorGUILayout.PropertyField(this.m_DescriptionTextEffectDistanceProperty, new GUIContent("Effect Distance"));
				EditorGUILayout.PropertyField(this.m_DescriptionTextEffectUseGraphicAlphaProperty, new GUIContent("Use Graphic Alpha"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		protected void DrawAttributeProperties()
		{
			UITooltip.TextEffectType textEffect = (UITooltip.TextEffectType)this.m_AttributeTextEffectProperty.enumValueIndex;
			
			EditorGUILayout.LabelField("Attribute Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			EditorGUILayout.PropertyField(this.m_AttributeFontProperty, new GUIContent("Font"));
			EditorGUILayout.PropertyField(this.m_AttributeFontStyleProperty, new GUIContent("Font Style"));
			EditorGUILayout.PropertyField(this.m_AttributeFontSizeProperty, new GUIContent("Font Size"));
			EditorGUILayout.PropertyField(this.m_AttributeFontLineSpacingProperty, new GUIContent("Font Line Spacing"));
			EditorGUILayout.PropertyField(this.m_AttributeFontColorProperty, new GUIContent("Color"));
			EditorGUILayout.PropertyField(this.m_AttributeTextEffectProperty, new GUIContent("Text Effect"));
			if (textEffect == UITooltip.TextEffectType.Shadow || textEffect == UITooltip.TextEffectType.Outline)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_AttributeTextEffectColorProperty, new GUIContent("Effect Color"));
				EditorGUILayout.PropertyField(this.m_AttributeTextEffectDistanceProperty, new GUIContent("Effect Distance"));
				EditorGUILayout.PropertyField(this.m_AttributeTextEffectUseGraphicAlphaProperty, new GUIContent("Use Graphic Alpha"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
	}
}