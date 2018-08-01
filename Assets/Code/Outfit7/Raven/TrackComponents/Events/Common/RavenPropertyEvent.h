#ifdef STARLITE
#pragma once

#include <TrackComponents/Events/Base/RavenContinuousEvent.h>
#include <TrackComponents/Properties/Base/RavenAnimationPropertyComponentBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenPropertyEvent : public RavenContinuousEvent {
            SCLASS_SEALED(RavenPropertyEvent);

        public:
            RavenPropertyEvent();
            bool IsEventLockedAtOneFrame() const override;
            bool IsSetProperty() const;
            bool IsValid() const final;
            const RavenAnimationDataComponentBase* GetAnimationDataEditorOnly() override;
            const RavenAnimationPropertyComponentBase* GetProperty() const;
            const RavenPropertyComponent* GetPropertyComponent() const override;
            const Ref<SceneObject>& GetTriggerTarget() const;
            void DestroyEditor(Ptr<RavenSequence> sequence) override;
            void Initialize(Ptr<RavenSequence> sequence) final;
            void SetIsSetProperty(bool value);
            void SetProperty(Ref<RavenAnimationPropertyComponentBase>& value);
            void SetTriggerTarget(Ptr<RavenSequence> sequence, Ref<SceneObject>& target);

        protected:
            void OnEndCallback(int frame) final;
            void OnEnterCallback(int frame) final;
            void OnPauseCallback(int frame) final;
            void OnProcessCallback(int frame, double frameInterpolationTime) final;
            void OnResumeCallback(int frame) final;
            void OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) override;

        private:
            SPROPERTY(CustomAttributes : ["UnityEngine.HideInInspector"], Access : "private");
            bool m_IsSetProperty;

            SPROPERTY(Access : "private");
            Ref<RavenAnimationPropertyComponentBase> m_Property;

            SPROPERTY(Access : "private");
            Ref<SceneObject> m_TriggerTarget;
        };
    } // namespace Raven
} // namespace Starlite
#endif