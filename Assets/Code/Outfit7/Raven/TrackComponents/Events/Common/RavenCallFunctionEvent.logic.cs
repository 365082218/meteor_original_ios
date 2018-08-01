namespace Starlite.Raven {

    public sealed partial class RavenCallFunctionEvent {

        public sealed override bool IsBarrier {
            get {
                return m_IsBarrier && m_TrackIndex == 0 && m_SubTrackIndex == 0;
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

        protected sealed override void OnEnterCallback(int frame) {
            m_Property.OnEnter();
        }
    }
}