#ifdef STARLITE
#pragma once

#include <TrackComponents/Events/Base/RavenTriggerEvent.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenGoToEvent : public RavenTriggerEvent {
            SCLASS_SEALED(RavenGoToEvent);

        public:
            RavenGoToEvent();
            bool IsBarrier() const final;
            int GetFrameToJumpTo() const;
            const RavenAnimationDataComponentBase* GetAnimationDataEditorOnly() final;
            const RavenPropertyComponent* GetPropertyComponent() const override;
            void Initialize(Ptr<RavenSequence> sequence) override;
            void OffsetEvent(int nFrames) override;
            void RecalculateFpsChange(double durationFactor) override;
            void SetFrameToJumpTo(int value);

        protected:
            void OnEnterCallback(int frame) final;
            void OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) override;

        private:
            SPROPERTY(CustomAttributes : ["UnityEngine.HeaderAttribute(\"Settings\")"], Access : "private");
            int m_FrameToJumpTo = 0;

            Ref<RavenSequence> m_Sequence;
        };
    } // namespace Raven
} // namespace Starlite
#endif
