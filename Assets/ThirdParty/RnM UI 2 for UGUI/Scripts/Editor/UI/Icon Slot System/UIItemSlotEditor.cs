using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using System.Collections;

namespace UnityEditor.UI
{
	[CustomEditor(typeof(UIItemSlot), true)]
	public class UIItemSlotEditor : UISlotBaseEditor {
		
		private SerializedProperty onAssignProperty;
		private SerializedProperty onUnassignProperty;
        private SerializedProperty onClickProperty;
        protected override void OnEnable()
		{
			base.OnEnable();
			this.onAssignProperty = this.serializedObject.FindProperty("onAssign");
			this.onUnassignProperty = this.serializedObject.FindProperty("onUnassign");
            this.onClickProperty = this.serializedObject.FindProperty("onClick");
        }
		
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

            EditorGUILayout.Separator();

            this.serializedObject.Update();
            EditorGUILayout.PropertyField(this.onAssignProperty, new GUIContent("On Assign"), true);
            EditorGUILayout.PropertyField(this.onUnassignProperty, new GUIContent("On Unassign"), true);
            EditorGUILayout.PropertyField(this.onClickProperty, new GUIContent("On Click"), true);
            this.serializedObject.ApplyModifiedProperties();
        }
	}
}