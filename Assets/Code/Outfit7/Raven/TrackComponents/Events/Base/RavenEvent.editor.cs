using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenEvent {
#if UNITY_EDITOR

        public class RavenEventComparer : IComparer<RavenEvent> {

            public int Compare(RavenEvent x, RavenEvent y) {
                return x.m_StartFrame.CompareTo(y.m_StartFrame);
            }
        }

        public static readonly RavenEventComparer Comparer = new RavenEventComparer();

        public abstract RavenAnimationDataComponentBase AnimationDataEditorOnly {
            get;
        }

        public int WidthInFrames {
            get {
                return LastFrame - StartFrame + 1;
            }
        }

        public void SetTrackIndex(int trackIndex) {
            if (m_TrackIndex == trackIndex) {
                return;
            }

            Undo.RecordObject(this, "SetTrackIndex");
            m_TrackIndex = trackIndex;
        }

        public void SetSubTrackIndex(int subTrackIndex) {
            if (m_SubTrackIndex == subTrackIndex) {
                return;
            }

            Undo.RecordObject(this, "SetSubTrackIndex");
            m_SubTrackIndex = subTrackIndex;
        }

        public virtual void InitializeEditor(RavenSequence sequence, GameObject target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) {
            Undo.RecordObject(this, "InitializeEditor");

            m_StartFrame = startFrame;
            m_TrackIndex = trackIndex;
            m_SubTrackIndex = subTrackIndex;
        }

        public virtual void RecalculateFpsChange(double durationFactor) {
            m_StartFrame = (int)(m_StartFrame * durationFactor);
        }

        public virtual void OffsetEvent(int nFrames) {
            m_StartFrame += nFrames;
        }

        public virtual void SetStartFrame(int frame) {
            if (frame < 0) {
                return;
            }
            Undo.RecordObject(this, "SetStartFrame");
            m_StartFrame = frame;
        }

        public virtual void SetHideFlags(HideFlags hideFlags) {
            this.hideFlags = hideFlags;
        }

        public virtual void DestroyEditor(RavenSequence sequence) {
            Undo.DestroyObjectImmediate(this);
        }

        public abstract void SetLastFrame(int frame);

        public abstract void SetEndFrame(int frame);

        public abstract void SetTargetEditor(RavenSequence sequence, GameObject target);

#endif
    }
}