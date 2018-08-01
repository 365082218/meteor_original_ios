#ifdef STARLITE
#pragma once

#include <TrackComponents/Events/Base/RavenEvent.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenContinuousEvent : public RavenEvent {
            SCLASS_ABSTRACT(RavenContinuousEvent);

        public:
            RavenContinuousEvent();
            bool GetActive() const;
            bool GetInterpolate() const;
            bool IsBarrier() const override;
            bool GetPaused() const;
            bool ShouldEndEvent(int frame) const;
            bool ShouldEndEventAfterJump(int frame) const;
            bool ShouldProcessEvent(int frame) const;
            double GetDuration() const;
            ERavenEventType GetEventType() const final;
            int GetEndFrame() const final;
            int GetLastFrame() const final;
            Ref<SceneObject>& GetTarget();
            virtual bool IsEventLockedAtOneFrame() const = 0;
            void Initialize(Ptr<RavenSequence> sequence) override;
            void InitializeEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target, int startFrame, int lastFrame, int trackIndex, int subTrackIndex) override;
            void OffsetEvent(int nFrames) override;
            void OnEnd(int frame);
            void OnEnter(int frame) final;
            void OnPause(int frame);
            void OnProcess(int frame, double frameInterpolationTime);
            void RecalculateFpsChange(double durationFactor) override;
            void SetEndFrame(int frame) final;
            void SetInterpolate(bool value);
            void SetLastFrame(int frame) final;
            void SetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) final;

        protected:
            double GetTimeForFrame(int frame) const;
            virtual void OnEndCallback(int frame) = 0;
            virtual void OnEnterCallback(int frame) = 0;
            virtual void OnPauseCallback(int frame) = 0;
            virtual void OnProcessCallback(int frame, double frameInterpolationTime) = 0;
            virtual void OnResumeCallback(int frame) = 0;
            virtual void OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) = 0;
            void OnResume(int frame);

        private:
            void ResetLastProcessedFrame();

        protected:
            SPROPERTY(Access : "protected");
            int m_LastFrame = 30;

            SPROPERTY(Access : "protected");
            Ref<SceneObject> m_Target;

            bool m_Active = false;
            bool m_Paused = false;
            Double m_Duration = 0;
            Double m_FrameDuration = 0;
            int m_LastProcessedFrame = 0;

        private:
            SPROPERTY(Access : "private");
            bool m_Interpolate = true;

            bool m_ConditionsMet = false;
        };
    } // namespace Raven
} // namespace Starlite
#endif
