#ifdef STARLITE
#pragma once

#include <TrackComponents/Events/Base/RavenContinuousEvent.h>
#include <TrackComponents/Properties/Base/RavenTriggerPropertyComponentBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenCallFunctionContinuousEvent : public RavenContinuousEvent {
            SCLASS_SEALED(RavenCallFunctionContinuousEvent);

        public:
            RavenCallFunctionContinuousEvent();
            bool IsBarrier() const final;
            bool IsEventLockedAtOneFrame() const final;
            bool IsValid() const final;
            const RavenAnimationDataComponentBase* GetAnimationDataEditorOnly() final;
            const RavenPropertyComponent* GetPropertyComponent() const override;
            const RavenTriggerPropertyComponentBase* GetProperty() const;
            void DestroyEditor(Ptr<RavenSequence> sequence) override;
            void Initialize(Ptr<RavenSequence> sequence) final;
            void SetProperty(Ref<RavenTriggerPropertyComponentBase>& value);

        protected:
            void OnEndCallback(int frame) final;
            void OnEnterCallback(int frame) final;
            void OnPauseCallback(int frame) final;
            void OnProcessCallback(int frame, double frameInterpolationTime) final;
            void OnResumeCallback(int frame) final;
            void OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) override;

        private:
            SPROPERTY(CustomAttributes : [ "UnityEngine.HeaderAttribute(\"Settings\")", "Raven.VisibleConditionAttribute(\"m_TrackIndex == 0 && m_SubTrackIndex == 0\")" ], Access : "private");
            bool m_IsBarrier;

            SPROPERTY(Access : "private");
            Ref<RavenTriggerPropertyComponentBase> m_Property;
        };
    } // namespace Raven
} // namespace Starlite
#endif