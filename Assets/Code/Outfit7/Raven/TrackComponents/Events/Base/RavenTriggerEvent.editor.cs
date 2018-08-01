using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenTriggerEvent {
#if UNITY_EDITOR

        public GameObject Target {
            get {
                return m_Target;
            }
        }

        public override void InitializeEditor(RavenSequence sequence, GameObject target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) {
            base.InitializeEditor(sequence, target, startFrame, lastFrame, trackIndex, subTrackIndex);
            m_Target = target;
        }

        public sealed override void SetTargetEditor(RavenSequence sequence, GameObject target) {
            Undo.RecordObject(this, "SetTarget");
            m_Target = target;
            OnSetTargetEditor(sequence, target);
        }

        public sealed override void SetLastFrame(int frame) {
            SetStartFrame(frame);
        }

        public sealed override void SetEndFrame(int frame) {
            SetStartFrame(frame);
        }

        protected abstract void OnSetTargetEditor(RavenSequence sequence, GameObject target);

#endif
    }
}