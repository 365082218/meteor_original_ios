#ifdef STARLITE
#pragma once

#include <TrackComponents/Events/Base/RavenTriggerEvent.h>
#include <TrackComponents/Properties/Base/RavenTriggerPropertyComponentBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenCallFunctionEvent : public RavenTriggerEvent {
            SCLASS_SEALED(RavenCallFunctionEvent);

        public:
            RavenCallFunctionEvent();
            bool IsBarrier() const final;
            bool IsValid() const final;
            const RavenAnimationDataComponentBase* GetAnimationDataEditorOnly() final;
            const RavenPropertyComponent* GetPropertyComponent() const final;
            const RavenTriggerPropertyComponentBase* GetProperty() const;
            void DestroyEditor(Ptr<RavenSequence> sequence) override;
            void Initialize(Ptr<RavenSequence> sequence) final;
            void SetProperty(Ref<RavenTriggerPropertyComponentBase>& value);

        protected:
            void OnEnterCallback(int frame) final;
            void OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) override;

        private:
            SPROPERTY(CustomAttributes : [ "Raven.VisibleConditionAttribute(\"m_TrackIndex == 0 && m_SubTrackIndex == 0\")", "UnityEngine.HeaderAttribute(\"Settings\")" ], Access : "private");
            bool m_IsBarrier;

            SPROPERTY(Access : "private");
            Ref<RavenTriggerPropertyComponentBase> m_Property;
        };
    } // namespace Raven
} // namespace Starlite
#endif