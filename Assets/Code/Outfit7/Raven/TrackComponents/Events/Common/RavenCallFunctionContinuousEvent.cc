#ifdef STARLITE
#include "RavenCallFunctionContinuousEvent.h"
#include "RavenCallFunctionContinuousEvent.cs"

#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        RavenCallFunctionContinuousEvent::RavenCallFunctionContinuousEvent() {
        }

        bool RavenCallFunctionContinuousEvent::IsBarrier() const {
            return m_IsBarrier && m_TrackIndex == 0 && m_SubTrackIndex == 0;
        }

        bool RavenCallFunctionContinuousEvent::IsEventLockedAtOneFrame() const {
            return false;
        }

        bool RavenCallFunctionContinuousEvent::IsValid() const {
            if (!RavenContinuousEvent::IsValid()) {
                return false;
            }

            if (!m_Property) {
                return false;
            }

            return m_Property->IsValid();
        }

        const RavenAnimationDataComponentBase* RavenCallFunctionContinuousEvent::GetAnimationDataEditorOnly() {
            return nullptr;
        }

        const RavenPropertyComponent* RavenCallFunctionContinuousEvent::GetPropertyComponent() const {
            return m_Property;
        }

        const RavenTriggerPropertyComponentBase* RavenCallFunctionContinuousEvent::GetProperty() const {
            return m_Property;
        }

        void RavenCallFunctionContinuousEvent::DestroyEditor(Ptr<RavenSequence> sequence) {
            RavenContinuousEvent::DestroyEditor(sequence);
            if (m_Property) {
                m_Property->DestroyEditor(sequence);
            }
        }

        void RavenCallFunctionContinuousEvent::Initialize(Ptr<RavenSequence> sequence) {
            RavenContinuousEvent::Initialize(sequence);
            m_Property->Initialize(sequence);
        }

        void RavenCallFunctionContinuousEvent::SetProperty(Ref<RavenTriggerPropertyComponentBase>& value) {
            m_Property = value;
        }

        void RavenCallFunctionContinuousEvent::OnEndCallback(int frame) {
        }

        void RavenCallFunctionContinuousEvent::OnEnterCallback(int frame) {
        }

        void RavenCallFunctionContinuousEvent::OnPauseCallback(int frame) {
        }

        void RavenCallFunctionContinuousEvent::OnProcessCallback(int frame, double frameInterpolationTime) {
            m_Property->OnEnter();
        }

        void RavenCallFunctionContinuousEvent::OnResumeCallback(int frame) {
        }

        void RavenCallFunctionContinuousEvent::OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
            if (m_Property) {
                m_Property->SetTarget(target);
                m_Property->Initialize(sequence);
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif