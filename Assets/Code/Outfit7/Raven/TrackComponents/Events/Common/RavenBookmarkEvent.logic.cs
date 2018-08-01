using UnityEngine;
using Starlite.Raven.Internal;

namespace Starlite.Raven {

    public sealed partial class RavenBookmarkEvent {
        private RavenSequence m_Sequence;
        private bool m_ConditionsMet;

        public string BookmarkName {
            get {
                return m_BookmarkName;
            }
#if UNITY_EDITOR
            set {
                m_BookmarkName = value;
            }
#endif
        }

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
                return ERavenEventType.Bookmark;
            }
        }

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

        public sealed override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_Sequence = sequence;
        }

        public sealed override void OnEnter(int frame) {
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} OnEnter {1}", this, frame);
#endif
            m_ConditionsMet = ConditionsMet();

            if (m_ConditionsMet) {
                switch (m_BookmarkType) {
                    case ERavenBookmarkType.Pause:
                        m_Sequence.Pause();
                        break;

                    case ERavenBookmarkType.Stop:
                        m_Sequence.Stop();
                        break;

                    case ERavenBookmarkType.Loop:
                        var prevBookmark = m_Sequence.FindFirstBookmarkBefore(m_StartFrame);
                        if (prevBookmark != null) {
                            m_Sequence.GoToBookmark(prevBookmark);
                        } else {
                            m_Sequence.JumpToFrame(0);
                        }
                        break;
                }
            }
        }
    }
}