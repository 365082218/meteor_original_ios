using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.UI
{
	[CanEditMultipleObjects, CustomEditor(typeof(UISelectField_Arrow), true)]
	public class UISelectField_ArrowEditor : Editor {

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(this.serializedObject.FindProperty("transitionType"), new GUIContent("Transition"));
			
			Selectable.Transition transition = (Selectable.Transition)this.serializedObject.FindProperty("transitionType").enumValueIndex;
			Graphic graphic = this.serializedObject.FindProperty("targetGraphic").objectReferenceValue as Graphic;

			// Check if the transition requires a graphic
			if (transition == Selectable.Transition.ColorTint || transition == Selectable.Transition.SpriteSwap)
			{
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("targetGraphic"), new GUIContent("Target Graphic"));
				
				if (transition == Selectable.Transition.ColorTint)
				{
					if (graphic == null)
					{
						EditorGUILayout.HelpBox("You must have a Graphic target in order to use a color transition.", MessageType.Info);
					}
					else
					{
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(serializedObject.FindProperty("colors"), new GUIContent("Colors"), true);
						if (EditorGUI.EndChangeCheck())
							graphic.canvasRenderer.SetColor(this.serializedObject.FindProperty("colors").FindPropertyRelative("m_NormalColor").colorValue);
					}
				}
				else if (transition == Selectable.Transition.SpriteSwap)
				{
					if (graphic as Image == null)
					{
						EditorGUILayout.HelpBox("You must have a Image target in order to use a sprite swap transition.", MessageType.Info);
					}
					else
					{
						EditorGUILayout.PropertyField(this.serializedObject.FindProperty("spriteState"), new GUIContent("Sprites"), true);
					}
				}
			}
			else if (transition == Selectable.Transition.Animation)
			{
				EditorGUILayout.PropertyField(this.serializedObject.FindProperty("animationTriggers"), true);
				
				Animator animator = (target as UISelectField_Arrow).gameObject.GetComponent<Animator>();
				
				if (animator == null || animator.runtimeAnimatorController == null)
				{
					Rect controlRect = EditorGUILayout.GetControlRect();
					controlRect.xMin = (controlRect.xMin + EditorGUIUtility.labelWidth);
					
					if (GUI.Button(controlRect, "Auto Generate Animation", EditorStyles.miniButton))
					{
						// Generate the animator controller
						UnityEditor.Animations.AnimatorController animatorController = UIAnimatorControllerGenerator.GenerateAnimatorContoller(this.serializedObject.FindProperty("animationTriggers"), this.target.name);
						
						if (animatorController != null)
						{
							if (animator == null)
							{
								animator = (target as UISelectField_Arrow).gameObject.AddComponent<Animator>();
							}
							UnityEditor.Animations.AnimatorController.SetAnimatorController(animator, animatorController);
						}
					}
				}
			}

			this.serializedObject.ApplyModifiedProperties();
		}
	}
}