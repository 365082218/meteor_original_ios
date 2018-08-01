using System.Collections.Generic;
using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenBookmarkEvent {
#if UNITY_EDITOR

        public class RavenBookmarkComparer : IComparer<RavenBookmarkEvent> {

            public int Compare(RavenBookmarkEvent x, RavenBookmarkEvent y) {
                return Comparer.Compare(x, y);
            }
        }

        public static readonly RavenBookmarkComparer BookmarkComparer = new RavenBookmarkComparer();

        public ERavenBookmarkType BookmarkType {
            get {
                return m_BookmarkType;
            }
            set {
                m_BookmarkType = value;
            }
        }

        public override RavenAnimationDataComponentBase AnimationDataEditorOnly {
            get {
                return null;
            }
        }

        public sealed override void SetLastFrame(int frame) {
            SetStartFrame(frame);
        }

        public sealed override void SetEndFrame(int frame) {
            SetStartFrame(frame);
        }

        public override void RecalculateFpsChange(double durationFactor) {
            m_StartFrame = (int)(m_StartFrame * durationFactor);
        }

        public sealed override void SetTargetEditor(RavenSequence sequence, GameObject target) {
        }

#endif
    }
}