using Starlite.Raven.Internal;

namespace Starlite.Raven {

    public abstract partial class RavenTriggerEvent {
        private bool m_ConditionsMet = false;

        public sealed override int LastFrame {
            get {
                return m_StartFrame;
            }
        }

        public sealed override int EndFrame {
            get {
                return m_StartFrame;
            }
        }

        public sealed override ERavenEventType EventType {
            get {
                return ERavenEventType.Trigger;
            }
        }

        public sealed override void OnEnter(int frame) {
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} OnEnter {1}", this, frame);
#endif

            m_ConditionsMet = ConditionsMet();

            if (m_ConditionsMet) {
                OnEnterCallback(frame);
            }
        }

        protected abstract void OnEnterCallback(int frame);
    }
}