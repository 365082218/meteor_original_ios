using System;
using UnityEngine;
using Starlite.Raven.Internal;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenContinuousEvent {
#if UNITY_EDITOR

        public GameObject Target {
            get {
                return m_Target;
            }
        }

        public bool Interpolate {
            get {
                return m_Interpolate;
            }
            set {
                m_Interpolate = value;
            }
        }

        public sealed override void SetTargetEditor(RavenSequence sequence, GameObject target) {
            Undo.RecordObject(this, "SetTarget");
            m_Target = target;
            OnSetTargetEditor(sequence, target);
        }

        public override void InitializeEditor(RavenSequence sequence, GameObject target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) {
            base.InitializeEditor(sequence, target, startFrame, lastFrame, trackIndex, subTrackIndex);
            SetLastFrame(lastFrame);
            m_Target = target;
        }

        public override void RecalculateFpsChange(double durationFactor) {
            var oldDurationInFrames = DurationInFrames;
            base.RecalculateFpsChange(durationFactor);

            if (IsEventLockedAtOneFrame) {
                m_LastFrame = m_StartFrame;
            } else {
                m_LastFrame = Math.Max((int)(EndFrame * durationFactor) - 1, 0);

                if (durationFactor != 1.0) {
                    var expectedDuration = durationFactor * oldDurationInFrames;
                    var missFactor = DurationInFrames / expectedDuration;
                    if (missFactor != 1.0) {
                        RavenLog.WarnT(RavenSequence.Tag, "Event {0}'s duration {1} will last {2} of the expected duration ({3}) in frames after recalculating FPS change!", this, DurationInFrames, missFactor, expectedDuration);
                    }
                }
            }
        }

        public override void OffsetEvent(int nFrames) {
            base.OffsetEvent(nFrames);
            m_LastFrame += nFrames;
        }

        public sealed override void SetLastFrame(int frame) {
            if (frame < m_StartFrame) {
                return;
            }

            if (IsEventLockedAtOneFrame && frame > m_StartFrame) {
                return;
            }

            Undo.RecordObject(this, "SetLastFrame");
            m_LastFrame = frame;
        }

        public sealed override void SetEndFrame(int frame) {
            SetLastFrame(frame - 1);
        }

        protected abstract void OnSetTargetEditor(RavenSequence sequence, GameObject target);

#endif
    }
}