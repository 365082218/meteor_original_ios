﻿using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.UI
{
	[CanEditMultipleObjects, CustomEditor(typeof(UITab), true)]
	public class UITabEditor : Editor
	{
		private SerializedProperty m_TargetContentProperty;
		private SerializedProperty m_ImageTargetProperty;
		private SerializedProperty m_ImageTransitionProperty;
		private SerializedProperty m_ImageColorsProperty;
		private SerializedProperty m_ImageSpriteStateProperty;
		private SerializedProperty m_ImageAnimationTriggersProperty;
		private SerializedProperty m_TextTargetProperty;
		private SerializedProperty m_TextTransitionProperty;
		private SerializedProperty m_TextColorsProperty;
		private SerializedProperty m_OnValueChangedProperty;
		private SerializedProperty m_TransitionProperty;
		private SerializedProperty m_GraphicProperty;
		private SerializedProperty m_GroupProperty;
		private SerializedProperty m_IsOnProperty;
		private SerializedProperty m_NavigationProperty;
		
		protected virtual void OnEnable()
		{
			this.m_TargetContentProperty = this.serializedObject.FindProperty("m_TargetContent");
			this.m_ImageTargetProperty = this.serializedObject.FindProperty("m_ImageTarget");
			this.m_ImageTransitionProperty = this.serializedObject.FindProperty("m_ImageTransition");
			this.m_ImageColorsProperty = this.serializedObject.FindProperty("m_ImageColors");
			this.m_ImageSpriteStateProperty = this.serializedObject.FindProperty("m_ImageSpriteState");
			this.m_ImageAnimationTriggersProperty = this.serializedObject.FindProperty("m_ImageAnimationTriggers");
			this.m_TextTargetProperty = this.serializedObject.FindProperty("m_TextTarget");
			this.m_TextTransitionProperty = this.serializedObject.FindProperty("m_TextTransition");
			this.m_TextColorsProperty = this.serializedObject.FindProperty("m_TextColors");
			this.m_TransitionProperty = base.serializedObject.FindProperty("toggleTransition");
			this.m_GraphicProperty = base.serializedObject.FindProperty("graphic");
			this.m_GroupProperty = base.serializedObject.FindProperty("m_Group");
			this.m_IsOnProperty = base.serializedObject.FindProperty("m_IsOn");
			this.m_OnValueChangedProperty = base.serializedObject.FindProperty("onValueChanged");
			this.m_NavigationProperty = base.serializedObject.FindProperty("m_Navigation");
		}
		
		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.m_TargetContentProperty, new GUIContent("Target Content"));
			EditorGUILayout.Space();
			this.DrawTargetImageProperties();
			EditorGUILayout.Space();
			this.DrawTargetTextProperties();
			this.serializedObject.ApplyModifiedProperties();
			EditorGUILayout.Space();
			this.DrawToggleProperties();
		}
		
		private void DrawTargetImageProperties()
		{
			EditorGUILayout.LabelField("Image Target Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			
			EditorGUILayout.PropertyField(this.m_ImageTargetProperty);
			
			// Check if image is set
			if (this.m_ImageTargetProperty.objectReferenceValue != null)
			{
				Image image = (this.m_ImageTargetProperty.objectReferenceValue as Image);
				
				EditorGUILayout.PropertyField(this.m_ImageTransitionProperty);
				
				// Get the selected transition
				Selectable.Transition transition = (Selectable.Transition)this.m_ImageTransitionProperty.enumValueIndex;
				
				if (transition != Selectable.Transition.None)
				{
					EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
					if (transition == Selectable.Transition.ColorTint)
					{
						EditorGUILayout.PropertyField(this.m_ImageColorsProperty);
					}
					else if (transition == Selectable.Transition.SpriteSwap)
					{
						EditorGUILayout.PropertyField(this.m_ImageSpriteStateProperty);
					}
					else if (transition == Selectable.Transition.Animation)
					{
						EditorGUILayout.PropertyField(this.m_ImageAnimationTriggersProperty);
						
						Animator animator = image.gameObject.GetComponent<Animator>();
						
						if (animator == null || animator.runtimeAnimatorController == null)
						{
							Rect controlRect = EditorGUILayout.GetControlRect();
							controlRect.xMin = (controlRect.xMin + EditorGUIUtility.labelWidth);
							
							if (GUI.Button(controlRect, "Auto Generate Animation", EditorStyles.miniButton))
							{
								// Generate the animator controller
								UnityEditor.Animations.AnimatorController animatorController = UIAnimatorControllerGenerator.GenerateAnimatorContoller(this.m_ImageAnimationTriggersProperty, this.target.name);
								
								if (animatorController != null)
								{
									if (animator == null)
									{
										animator = image.gameObject.AddComponent<Animator>();
									}
									UnityEditor.Animations.AnimatorController.SetAnimatorController(animator, animatorController);
								}
							}
						}
					}
					EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
				}
			}
			
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		private void DrawTargetTextProperties()
		{
			EditorGUILayout.LabelField("Text Target Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			
			EditorGUILayout.PropertyField(this.m_TextTargetProperty);
			
			// Check if image is set
			if (this.m_TextTargetProperty.objectReferenceValue != null)
			{
				EditorGUILayout.PropertyField(this.m_TextTransitionProperty);
				
				// Get the selected transition
				UITab.TextTransition transition = (UITab.TextTransition)this.m_TextTransitionProperty.enumValueIndex;
				
				if (transition != UITab.TextTransition.None)
				{
					EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
					if (transition == UITab.TextTransition.ColorTint)
					{
						EditorGUILayout.PropertyField(this.m_TextColorsProperty);
					}
					EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
				}
			}
			
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		private void DrawToggleProperties()
		{
			EditorGUILayout.LabelField("Toggle Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			
			base.serializedObject.Update();
			EditorGUILayout.PropertyField(this.m_IsOnProperty, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_TransitionProperty, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_GraphicProperty, new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_GroupProperty, new GUILayoutOption[0]);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(this.m_NavigationProperty);
			EditorGUILayout.Space();
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			EditorGUILayout.PropertyField(this.m_OnValueChangedProperty, new GUILayoutOption[0]);
			base.serializedObject.ApplyModifiedProperties();
		}
	}
}
