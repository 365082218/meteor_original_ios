using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.UI
{
	[CanEditMultipleObjects, CustomEditor(typeof(UIWindow))]
	public class UIWindowEditor : Editor {
		
		private SerializedProperty m_WindowIdProperty;
		private SerializedProperty m_CustomWindowIdProperty;
		private SerializedProperty m_StartingStateProperty;
		private SerializedProperty m_EscapeKeyActionProperty;
		private SerializedProperty m_TransitionProperty;
		private SerializedProperty m_TransitionEasingProperty;
		private SerializedProperty m_TransitionDurationProperty;
		private SerializedProperty onTransitionBeginProperty;
		private SerializedProperty onTransitionCompleteProperty;
		
		protected virtual void OnEnable()
		{
			this.m_WindowIdProperty = this.serializedObject.FindProperty("m_WindowId");
			this.m_CustomWindowIdProperty = this.serializedObject.FindProperty("m_CustomWindowId");
			this.m_StartingStateProperty = this.serializedObject.FindProperty("m_StartingState");
			this.m_EscapeKeyActionProperty = this.serializedObject.FindProperty("m_EscapeKeyAction");
			this.m_TransitionProperty = this.serializedObject.FindProperty("m_Transition");
			this.m_TransitionEasingProperty = this.serializedObject.FindProperty("m_TransitionEasing");
			this.m_TransitionDurationProperty = this.serializedObject.FindProperty("m_TransitionDuration");
			this.onTransitionBeginProperty = this.serializedObject.FindProperty("onTransitionBegin");
			this.onTransitionCompleteProperty = this.serializedObject.FindProperty("onTransitionComplete");
		}
		
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			EditorGUILayout.Separator();
			this.DrawGeneralProperties();
			EditorGUILayout.Separator();
			this.DrawTransitionProperties();
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(this.onTransitionBeginProperty, new GUIContent("On Transition Begin"), true);
			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(this.onTransitionCompleteProperty, new GUIContent("On Transition Complete"), true);
			
			serializedObject.ApplyModifiedProperties();
		}
		
		protected void DrawGeneralProperties()
		{
			UIWindowID id = (UIWindowID)this.m_WindowIdProperty.enumValueIndex;
			
			EditorGUILayout.LabelField("General Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			
			EditorGUILayout.PropertyField(this.m_WindowIdProperty, new GUIContent("ID"));
			if (id == UIWindowID.Custom)
			{
				EditorGUILayout.PropertyField(this.m_CustomWindowIdProperty, new GUIContent("Custom ID"));
			}
			EditorGUILayout.PropertyField(this.m_StartingStateProperty, new GUIContent("Starting State"));
			EditorGUILayout.PropertyField(this.m_EscapeKeyActionProperty, new GUIContent("Escape Key Action"));
			
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		protected void DrawTransitionProperties()
		{
			EditorGUILayout.LabelField("Transition Properties", EditorStyles.boldLabel);
			EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
			
			EditorGUILayout.PropertyField(this.m_TransitionProperty, new GUIContent("Transition"));
			
			// Get the transition
			UIWindow.Transition transition = (UIWindow.Transition)this.m_TransitionProperty.enumValueIndex;
			
			if (transition == UIWindow.Transition.Fade)
			{
				EditorGUI.indentLevel = (EditorGUI.indentLevel + 1);
				EditorGUILayout.PropertyField(this.m_TransitionEasingProperty, new GUIContent("Easing"));
				EditorGUILayout.PropertyField(this.m_TransitionDurationProperty, new GUIContent("Duration"));
				EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
			}
			
			EditorGUI.indentLevel = (EditorGUI.indentLevel - 1);
		}
		
		protected void SetWindowID(int id)
		{
		}
	}
}