#ifdef STARLITE
#include "RavenGoToEvent.h"
#include "RavenGoToEvent.cs"

#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        RavenGoToEvent::RavenGoToEvent() {
        }

        bool RavenGoToEvent::IsBarrier() const {
            return false;
        }

        int RavenGoToEvent::GetFrameToJumpTo() const {
            return m_FrameToJumpTo;
        }

        const RavenAnimationDataComponentBase* RavenGoToEvent::GetAnimationDataEditorOnly() {
            return nullptr;
        }

        const RavenPropertyComponent* RavenGoToEvent::GetPropertyComponent() const {
            return nullptr;
        }

        void RavenGoToEvent::Initialize(Ptr<RavenSequence> sequence) {
            RavenTriggerEvent::Initialize(sequence);
            m_Sequence = sequence;
        }

        void RavenGoToEvent::OffsetEvent(int nFrames) {
            RavenTriggerEvent::OffsetEvent(nFrames);
            m_FrameToJumpTo += nFrames;
        }

        void RavenGoToEvent::RecalculateFpsChange(double durationFactor) {
            RavenTriggerEvent::RecalculateFpsChange(durationFactor);
            m_FrameToJumpTo = (int)(m_FrameToJumpTo * durationFactor);
        }

        void RavenGoToEvent::SetFrameToJumpTo(int value) {
            m_FrameToJumpTo = value;
        }

        void RavenGoToEvent::OnEnterCallback(int frame) {
            m_Sequence->JumpToFrame(m_FrameToJumpTo);
        }

        void RavenGoToEvent::OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
            m_Sequence = sequence;
        }
    } // namespace Raven
} // namespace Starlite
#endif