#ifdef STARLITE
#include "RavenPropertyEvent.h"
#include "RavenPropertyEvent.cs"

#include <RavenSequence.h>
#include <TrackComponents/Properties/Base/RavenAnimationPropertyComponentBase.h>

namespace Starlite {
    namespace Raven {
        RavenPropertyEvent::RavenPropertyEvent() {
        }

        bool RavenPropertyEvent::IsEventLockedAtOneFrame() const {
            return m_IsSetProperty;
        }

        bool RavenPropertyEvent::IsSetProperty() const {
            return m_IsSetProperty;
        }

        bool RavenPropertyEvent::IsValid() const {
            if (!RavenContinuousEvent::IsValid()) {
                return false;
            }

            if (!m_Property) {
                return false;
            }

            return m_Property->IsValid();
        }

        const RavenAnimationDataComponentBase* RavenPropertyEvent::GetAnimationDataEditorOnly() {
            return m_Property ? m_Property->GetAnimationData() : nullptr;
        }

        const RavenAnimationPropertyComponentBase* RavenPropertyEvent::GetProperty() const {
            return m_Property;
        }

        const RavenPropertyComponent* RavenPropertyEvent::GetPropertyComponent() const {
            return m_Property;
        }

        const Ref<SceneObject>& RavenPropertyEvent::GetTriggerTarget() const {
            return m_TriggerTarget;
        }

        void RavenPropertyEvent::DestroyEditor(Ptr<RavenSequence> sequence) {
            RavenContinuousEvent::DestroyEditor(sequence);
            if (m_Property) {
                m_Property->DestroyEditor(sequence);
            }
        }

        void RavenPropertyEvent::Initialize(Ptr<RavenSequence> sequence) {
            RavenContinuousEvent::Initialize(sequence);
            m_Property->Initialize(sequence);
        }

        void RavenPropertyEvent::SetIsSetProperty(bool value) {
            m_IsSetProperty = value;
        }

        void RavenPropertyEvent::SetProperty(Ref<RavenAnimationPropertyComponentBase>& value) {
            m_Property = value;
        }

        void RavenPropertyEvent::SetTriggerTarget(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
            m_TriggerTarget = target;

            if (m_Property && m_Property->GetTriggerProperty()) {
                auto& triggerProperty = m_Property->GetTriggerProperty();
                triggerProperty->SetTarget(target);
                triggerProperty->Initialize(sequence);
            }
        }

        void RavenPropertyEvent::OnEndCallback(int frame) {
            m_Property->OnExit();
        }

        void RavenPropertyEvent::OnEnterCallback(int frame) {
            m_Property->OnEnter();
            if (m_IsSetProperty) {
                m_Property->EvaluateAtTime(1.0, 1.0);
            }
        }

        void RavenPropertyEvent::OnPauseCallback(int frame) {
        }

        void RavenPropertyEvent::OnProcessCallback(int frame, double frameInterpolationTime) {
            if (m_IsSetProperty) {
                return;
            }

            m_Property->EvaluateAtTime(GetTimeForFrame(frame) + frameInterpolationTime, m_Duration);
        }

        void RavenPropertyEvent::OnResumeCallback(int frame) {
        }

        void RavenPropertyEvent::OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
            if (m_Property) {
                m_Property->SetTarget(target);
                m_Property->Initialize(sequence);
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif