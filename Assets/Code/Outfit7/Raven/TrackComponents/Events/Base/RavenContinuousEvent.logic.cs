using Starlite.Raven.Internal;

namespace Starlite.Raven {

    public abstract partial class RavenContinuousEvent {

        protected bool m_Active;
        protected bool m_Paused;
        protected double m_Duration;
        protected double m_FrameDuration;
        protected int m_LastProcessedFrame;

        private bool m_ConditionsMet;

        public double Duration {
            get {
                return m_Duration;
            }
        }

        public sealed override int LastFrame {
            get {
                return m_LastFrame;
            }
        }

        public sealed override int EndFrame {
            get {
                return m_LastFrame + 1;
            }
        }

        public bool Active {
            get {
                return m_Active;
            }
        }

        public bool Paused {
            get {
                return m_Paused;
            }
        }

        public sealed override ERavenEventType EventType {
            get {
                return ERavenEventType.Continuous;
            }
        }

        public override bool IsBarrier {
            get {
                return false;
            }
        }

        public abstract bool IsEventLockedAtOneFrame {
            get;
        }

        public bool ShouldEndEvent(int frame) {
            return frame > m_LastFrame || frame < m_StartFrame;
        }

        public bool ShouldEndEventAfterJump(int frame) {
            // this one is a bit special because we put end event triggers on EndFrame + 1 frame
            // so if the frame isn't beyond that or before start, we will get end events normally
            return frame < m_StartFrame || (frame > EndFrame);
        }

        public bool ShouldProcessEvent(int frame) {
            return frame <= m_LastFrame && frame > m_StartFrame;
        }

        public override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_FrameDuration = sequence.FrameDuration;
            m_Duration = m_FrameDuration * DurationInFrames;
        }

        public sealed override void OnEnter(int frame) {
            // can be called if already active in case of gotos to the start frame... etc
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} OnEnter {1}", this, frame);
            RavenAssert.IsTrue(m_Active == false, "Continuous event {0} already active when entering!", this);
#endif
            m_Active = true;
            ResetLastProcessedFrame();

            m_ConditionsMet = ConditionsMet();

            if (m_Paused) {
                OnResume(frame);
            }

            if (m_ConditionsMet) {
                OnEnterCallback(frame);
            }
        }

        public void OnProcess(int frame, double frameInterpolationTime) {
            // can be called if not active in case of gotos to thge middle... etc
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} OnProcess {1} {2}", this, frame, frameInterpolationTime);
#endif
            if (!m_Active) {
                OnEnter(frame);
            }

            if (m_Paused) {
                OnResume(frame);
            }

            if (m_ConditionsMet && (m_Interpolate || frame != m_LastProcessedFrame)) {
                // frameInterpolationTime is 0-1 so we need to multiply it by frame duration to get correct time
                // don't add interpolation time if we're not interpolating so we always get the same result
                OnProcessCallback(frame, m_Interpolate ? (frameInterpolationTime * m_FrameDuration) : 0);
            }

            m_LastProcessedFrame = frame;
        }

        public void OnEnd(int frame) {
            // can be called if not active in case of gotos to the end frame... etc
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} OnEnd {1}", this, frame);
            RavenAssert.IsTrue(m_Active == true, "Continuous event {0} not active when ending!", this);
#endif
            OnProcess(frame, 0);
            m_Active = false;

            if (m_ConditionsMet) {
                OnEndCallback(frame);
            }
        }

        public void OnPause(int frame) {
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} OnPause {1}", this, frame);
            RavenAssert.IsTrue(m_Paused == false, "Continuous event {0} already paused when pausing!", this);
#endif
            OnProcess(frame, 0);
            m_Paused = true;
            if (m_ConditionsMet) {
                OnPauseCallback(frame);
            }
        }

        protected void OnResume(int frame) {
#if RAVEN_DEBUG
            RavenLog.DebugT(RavenSequence.Tag, "{0} OnResume {1}", this, frame);
            RavenAssert.IsTrue(m_Paused == true, "Continuous event {0} not paused when resuming!", this);
#endif
            m_Paused = false;
            ResetLastProcessedFrame();
            if (m_ConditionsMet) {
                OnResumeCallback(frame);
            }
        }

        protected abstract void OnEnterCallback(int frame);

        protected abstract void OnProcessCallback(int frame, double frameInterpolationTime);

        protected abstract void OnEndCallback(int frame);

        protected abstract void OnPauseCallback(int frame);

        protected abstract void OnResumeCallback(int frame);

        protected double GetTimeForFrame(int frame) {
            return (frame - m_StartFrame) * m_FrameDuration;
        }

        private void ResetLastProcessedFrame() {
            m_LastProcessedFrame = -1;
        }
    }
}