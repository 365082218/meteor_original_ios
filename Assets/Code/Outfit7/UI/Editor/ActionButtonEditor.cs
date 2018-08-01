using UnityEditor;
using UnityEditor.UI;

namespace Outfit7.UI {
    [CustomEditor(typeof(AbstractActionButton), true)]
    [CanEditMultipleObjects]
    public class ActionButtonEditor : SelectableEditor {
        SerializedProperty m_OnClickProperty;

        protected override void OnEnable() {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_OnClickProperty);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("DelayedTouch"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
