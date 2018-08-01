using UnityEditor;
using UnityEngine;

namespace Outfit7.Logic {

    [CustomPropertyDrawer(typeof(MessageEvent))]
    public class MessageEventDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            var height = position.height / 6;
            position.height = height;
            EditorGUI.LabelField(position, label);
            position.y += height;
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("IntData0"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("IntData1"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("FloatData0"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("FloatData1"));
            position.y += height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("ObjectData0"));
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label) * 6;
        }
    }
}