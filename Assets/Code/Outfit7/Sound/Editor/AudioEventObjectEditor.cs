using UnityEngine;
using UnityEditor;

namespace Outfit7.Audio {

    [CustomPropertyDrawer(typeof(AudioEventObject), true)]
    public class AudioEventObjectDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var height = position.height / 7;
            position.height = height;
            EditorGUI.LabelField(position, label);
            position.y += height;
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("AudioEventData"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("UserDefinedPlayIndex"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Volume"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Pitch"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("Delay"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("AttachTransform"));
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 7;
        }
    }
}