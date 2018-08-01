using UnityEditor;

namespace Outfit7.UI {
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AtlasText), true)]
    public class AtlasTextEditor : AbstractAtlasTextEditor {

        private SerializedProperty EditorTestProperty;

        protected override void OnEnable() {
            base.OnEnable();

            EditorTestProperty = serializedObject.FindProperty("EditorTest");
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(EditorTestProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}