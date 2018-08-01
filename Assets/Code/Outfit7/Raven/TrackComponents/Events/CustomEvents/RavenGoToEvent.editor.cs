using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Starlite.Raven {

    public sealed partial class RavenGoToEvent {
#if UNITY_EDITOR

        public int FrameToJumpTo {
            get {
                return m_FrameToJumpTo;
            }
            set {
                m_FrameToJumpTo = value;
            }
        }

        public sealed override RavenAnimationDataComponentBase AnimationDataEditorOnly {
            get {
                return null;
            }
        }

        public override void OffsetEvent(int nFrames) {
            base.OffsetEvent(nFrames);
            m_FrameToJumpTo += nFrames;
        }

        public override void RecalculateFpsChange(double durationFactor) {
            base.RecalculateFpsChange(durationFactor);
            m_FrameToJumpTo = (int)(m_FrameToJumpTo * durationFactor);
        }

        protected override void OnSetTargetEditor(RavenSequence sequence, GameObject target) {
            m_Sequence = sequence;
        }

#endif
    }
}