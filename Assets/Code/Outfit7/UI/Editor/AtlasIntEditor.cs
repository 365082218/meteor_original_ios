using UnityEditor;

namespace Outfit7.UI {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AtlasInt), true)]
    public class AtlasIntEditor : AbstractAtlasTextEditor {

        private SerializedProperty UseNumberGroupSeparatorProperty;
        private SerializedProperty EditorTestProperty;

        protected override void OnEnable() {
            base.OnEnable();

            UseNumberGroupSeparatorProperty = serializedObject.FindProperty("UseNumberGroupSeparator");
            EditorTestProperty = serializedObject.FindProperty("EditorTest");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(UseNumberGroupSeparatorProperty);
            EditorGUILayout.PropertyField(EditorTestProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}