using UnityEditor;
using UnityEditor.EventSystems;

namespace Outfit7.UI {
    [CustomEditor(typeof(TouchEventSystem), true)]
    public class TouchEventSystemEditor : EventSystemEditor {

        public override void OnInspectorGUI() {
            serializedObject.Update();

            SerializedProperty sp = serializedObject.FindProperty("m_DragThreshold");
            sp.intValue = EditorGUILayout.IntField("Drag Threshold", sp.intValue);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

