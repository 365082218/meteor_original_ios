using UnityEditor;
using UnityEditorInternal;

namespace Starlite.Raven {

    [CustomEditor(typeof(RavenEvent), true)]
    public class RavenEventEditor : UnityEditor.Editor {
        protected ReorderableList m_ConditionList = null;

        protected virtual void OnEnable() {
            var evnt = target as RavenEvent;
            var sequence = evnt.gameObject.GetComponent<RavenSequence>();
            if (sequence == null)
                return;

            m_ConditionList = RavenConditionEditor.InitReorderableList(evnt.Conditions, sequence.Parameters);
        }

        public sealed override void OnInspectorGUI() {
            Undo.RecordObject(target, "OnInspectorGUI");
            DrawInspector();
        }

        protected virtual void DrawInspector() {
            m_ConditionList.DoLayoutList();
            DrawDefaultInspector();
        }
    }
}