using UnityEditor;
using UnityEditorInternal;
using Outfit7.Logic;

namespace Outfit7.Sequencer {
    [CustomEditor(typeof(SequencerEvent), true)]
    public class SequencerEventEditor : UnityEditor.Editor {
        ReorderableList ConditionList = null;

        public void OnEnable() {
            SequencerEvent myTarget = (SequencerEvent) target;
            SequencerSequence sequence = myTarget.gameObject.GetComponent<SequencerSequence>();
            if (sequence == null)
                return;

            ConditionList = StateMachineConditionEditor.InitReorderableList(myTarget.Conditions, sequence.Parameters);
        }

        public override void OnInspectorGUI() {
            ConditionList.DoLayoutList();
            DrawDefaultInspector();
        }
    }
}