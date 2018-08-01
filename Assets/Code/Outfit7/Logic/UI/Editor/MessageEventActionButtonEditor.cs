using System.Collections;
using UnityEngine;
using Outfit7.Audio;
using Outfit7.Util;
using Outfit7.UI;
using UnityEditor;

namespace Outfit7.Logic {

    [CustomEditor(typeof(MessageEventActionButton), true)]
    [CanEditMultipleObjects]
    public class MessageEventActionButtonEditor : ActionButtonEditor {

        protected SerializedProperty DownGameActionProperty;
        protected SerializedProperty CancelGameActionProperty;
        protected SerializedProperty ClickGameActionProperty;
        protected SerializedProperty MessageEventDataProperty;
        protected SerializedProperty AudioEventDownProperty;
        protected SerializedProperty AudioEventUpProperty;

        protected override void OnEnable() {
            base.OnEnable();
            DownGameActionProperty = serializedObject.FindProperty("DownGameAction");
            CancelGameActionProperty = serializedObject.FindProperty("CancelGameAction");
            ClickGameActionProperty = serializedObject.FindProperty("ClickGameAction");
            MessageEventDataProperty = serializedObject.FindProperty("MessageEventData");
            AudioEventDownProperty = serializedObject.FindProperty("AudioEventDown");
            AudioEventUpProperty = serializedObject.FindProperty("AudioEventUp");
        }

        private void ActionPopup(SerializedProperty property) {
            int index = ArrayUtility.FindIndex(MessageEventPreferences.MessageEventNames, s => s == property.stringValue);
            if (index == -1) {
                index = 0;
            }
            index = EditorGUILayout.Popup(property.name, index, MessageEventPreferences.MessageEventNames);
            if (index <= 0) {
                property.stringValue = string.Empty;
            } else {
                property.stringValue = MessageEventPreferences.MessageEventNames[index];
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            serializedObject.Update();
            ActionPopup(DownGameActionProperty);
            ActionPopup(CancelGameActionProperty);
            ActionPopup(ClickGameActionProperty);
            EditorGUILayout.PropertyField(MessageEventDataProperty);
            EditorGUILayout.PropertyField(AudioEventDownProperty);
            EditorGUILayout.PropertyField(AudioEventUpProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}