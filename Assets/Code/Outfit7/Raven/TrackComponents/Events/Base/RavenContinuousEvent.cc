#ifdef STARLITE
#include "RavenContinuousEvent.h"
#include "RavenContinuousEvent.cs"

#include <math.h>
#include <Utility/RavenLog.h>
#include <RavenSequence.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        RavenContinuousEvent::RavenContinuousEvent() {
        }

        bool RavenContinuousEvent::GetActive() const {
            return m_Active;
        }

        bool RavenContinuousEvent::GetInterpolate() const {
            return m_Interpolate;
        }

        bool RavenContinuousEvent::IsBarrier() const {
            return false;
        }

        bool RavenContinuousEvent::GetPaused() const {
            return m_Paused;
        }

        bool RavenContinuousEvent::ShouldEndEvent(int frame) const {
            return frame > m_LastFrame || frame < m_StartFrame;
        }

        bool RavenContinuousEvent::ShouldEndEventAfterJump(int frame) const {
            // this one is a bit special because we put end event triggers on EndFrame + 1 frame
            // so if the frame isn't beyond that or before start, we will get end events normally
            return frame < m_StartFrame || (frame > GetEndFrame());
        }

        bool RavenContinuousEvent::ShouldProcessEvent(int frame) const {
            return frame <= m_LastFrame && frame > m_StartFrame;
        }

        double RavenContinuousEvent::GetDuration() const {
            return m_Duration;
        }

        ERavenEventType RavenContinuousEvent::GetEventType() const {
            return ERavenEventType::Continuous;
        }

        int RavenContinuousEvent::GetEndFrame() const {
            return m_LastFrame + 1;
        }

        int RavenContinuousEvent::GetLastFrame() const {
            return m_LastFrame;
        }

        Ref<SceneObject>& RavenContinuousEvent::GetTarget() {
            return m_Target;
        }

        void RavenContinuousEvent::Initialize(Ptr<RavenSequence> sequence) {
            RavenEvent::Initialize(sequence);
            m_FrameDuration = sequence->GetFrameDuration();
            m_Duration = m_FrameDuration * GetDurationInFrames();
        }

        void RavenContinuousEvent::InitializeEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) {
            RavenEvent::InitializeEditor(sequence, target, startFrame, lastFrame, trackIndex, subTrackIndex);
            SetLastFrame(lastFrame);
            m_Target = target;
        }

        void RavenContinuousEvent::OffsetEvent(int nFrames) {
            RavenEvent::OffsetEvent(nFrames);
            m_LastFrame += nFrames;
        }

        void RavenContinuousEvent::OnEnd(int frame) {
// can be called if not active in case of gotos to the end frame... etc
#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s OnEnd %d", this, frame);
#endif
            DebugAssert(m_Active == true, "Continuous event %s not active when ending!", this);

            OnProcess(frame, 0);
            m_Active = false;

            if (m_ConditionsMet) {
                OnEndCallback(frame);
            }
        }

        void RavenContinuousEvent::OnEnter(int frame) {
// can be called if already active in case of gotos to the start frame... etc
#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s OnEnter %d", this, frame);
#endif
            DebugAssert(m_Active == false, "Continuous event %s already active when entering!", this);

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

        void RavenContinuousEvent::OnPause(int frame) {
#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s OnPause %d", this, frame);
#endif
            DebugAssert(m_Paused == false, "Continuous event %s already paused when pausing!", this);

            OnProcess(frame, 0);
            m_Paused = true;
            if (m_ConditionsMet) {
                OnPauseCallback(frame);
            }
        }

        void RavenContinuousEvent::OnProcess(int frame, double frameInterpolationTime) {
// can be called if not active in case of gotos to thge middle... etc
#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s OnProcess %d %f", this, frame, frameInterpolationTime);
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

        void RavenContinuousEvent::RecalculateFpsChange(double durationFactor) {
            auto oldDurationInFrames = GetDurationInFrames();
            RavenEvent::RecalculateFpsChange(durationFactor);

            if (IsEventLockedAtOneFrame()) {
                m_LastFrame = m_StartFrame;
            } else {
                m_LastFrame = Math::Max((int)(GetEndFrame() * durationFactor) - 1, 0);

                if (durationFactor != 1.0) {
                    auto expectedDuration = durationFactor * oldDurationInFrames;
                    auto missFactor = GetDurationInFrames() / expectedDuration;
                    if (missFactor != 1.0) {
                        pRavenLog->WarningT(RavenSequence::Tag.GetCString(), "Event %s's duration %f will last %f of the expected duration (%f) in frames after recalculating FPS change!", this,
                                            GetDurationInFrames(), missFactor, expectedDuration);
                    }
                }
            }
        }

        void RavenContinuousEvent::SetEndFrame(int frame) {
            SetLastFrame(frame - 1);
        }

        void RavenContinuousEvent::SetInterpolate(bool value) {
            m_Interpolate = value;
        }

        void RavenContinuousEvent::SetLastFrame(int frame) {
            if (frame < m_StartFrame) {
                return;
            }

            if (IsEventLockedAtOneFrame() && frame > m_StartFrame) {
                return;
            }

            m_LastFrame = frame;
        }

        void RavenContinuousEvent::SetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
            m_Target = target;
            OnSetTargetEditor(sequence, target);
        }

        double RavenContinuousEvent::GetTimeForFrame(int frame) const {
            return (frame - m_StartFrame) * m_FrameDuration;
        }

        void RavenContinuousEvent::OnResume(int frame) {
#ifdef RAVEN_DEBUG
            pRavenLog->InfoT(RavenSequence::Tag.GetCString(), "%s OnResume %d", this, frame);
#endif
            DebugAssert(m_Paused == true, "Continuous event %s not paused when resuming!", this);

            m_Paused = false;
            ResetLastProcessedFrame();
            if (m_ConditionsMet) {
                OnResumeCallback(frame);
            }
        }

        void RavenContinuousEvent::ResetLastProcessedFrame() {
            m_LastProcessedFrame = -1;
        }
    } // namespace Raven
} // namespace Starlite

#endif