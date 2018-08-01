using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenGoToEvent {
        private RavenSequence m_Sequence;

        public sealed override bool IsBarrier {
            get {
                return false;
            }
        }

        public override RavenPropertyComponent PropertyComponent {
            get {
                return null;
            }
        }

        public override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_Sequence = sequence;
        }

        protected sealed override void OnEnterCallback(int frame) {
            m_Sequence.JumpToFrame(m_FrameToJumpTo);
        }
    }
}