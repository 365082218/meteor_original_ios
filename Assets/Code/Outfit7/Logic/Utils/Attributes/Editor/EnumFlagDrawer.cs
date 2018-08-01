using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Outfit7.Logic.Util {

    [CustomPropertyDrawer(typeof(EnumFlagAttribute))]
    public class EnumFlagDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EnumFlagAttribute flagSettings = (EnumFlagAttribute) attribute;
            Enum enumValue = (Enum) Enum.ToObject(flagSettings.EnumType, property.intValue);
            EditorGUI.BeginProperty(position, label, property);
            Enum enumNew = EditorGUI.EnumMaskField(position, property.name, enumValue);
            property.intValue = (int) ((object) enumNew);
            EditorGUI.EndProperty();
        }
    }
}