using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenPropertyEvent {

        public override bool IsEventLockedAtOneFrame {
            get {
                return m_IsSetProperty;
            }
        }

        public override RavenPropertyComponent PropertyComponent {
            get {
                return m_Property;
            }
        }

        public sealed override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_Property.Initialize(sequence);
        }

        public sealed override bool IsValid() {
            if (!base.IsValid()) {
                return false;
            }

            if (m_Property == null) {
                return false;
            }
            return m_Property.IsValid();
        }

        protected sealed override void OnProcessCallback(int frame, double frameInterpolationTime) {
            if (m_IsSetProperty) {
                return;
            }

            m_Property.EvaluateAtTime(GetTimeForFrame(frame) + frameInterpolationTime, m_Duration);
        }

        protected sealed override void OnEnterCallback(int frame) {
            m_Property.OnEnter();
            if (m_IsSetProperty) {
                m_Property.EvaluateAtTime(1, 1);
            }
        }

        protected sealed override void OnEndCallback(int frame) {
            m_Property.OnExit();
        }

        protected sealed override void OnPauseCallback(int frame) {
        }

        protected sealed override void OnResumeCallback(int frame) {
        }
    }
}