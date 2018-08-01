using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    [CustomPropertyDrawer(typeof(VisibleConditionAttribute))]
    internal sealed class VisibleConditionDrawer : PropertyDrawer {
        private bool m_Inited = false;
        private PwnCompiler.LogicalExpression m_Expression;
        private bool m_DrawProperty = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!m_Inited) {
                Init(property);
            } else {
                m_DrawProperty = m_Expression.Evaluate(property.serializedObject);
            }

            if (m_DrawProperty) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!m_Inited) {
                Init(property);
            }

            if (m_DrawProperty) {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        private void Init(SerializedProperty property) {
            VisibleConditionAttribute dependencyAtt = (VisibleConditionAttribute)attribute;
            m_Expression = PwnCompiler.PwnLanguageCompiler.ParseLogicalExpression(dependencyAtt.m_DependencyStatement);
            m_DrawProperty = m_Expression.Evaluate(property.serializedObject);
            m_Inited = true;
        }
    }
}