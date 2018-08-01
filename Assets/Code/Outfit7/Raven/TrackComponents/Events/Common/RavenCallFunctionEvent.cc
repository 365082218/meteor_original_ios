#ifdef STARLITE
#include "RavenCallFunctionEvent.h"
#include "RavenCallFunctionEvent.cs"

#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        RavenCallFunctionEvent::RavenCallFunctionEvent() {
        }

        bool RavenCallFunctionEvent::IsBarrier() const {
            return m_IsBarrier && m_TrackIndex == 0 && m_SubTrackIndex == 0;
        }

        bool RavenCallFunctionEvent::IsValid() const {
            if (!RavenTriggerEvent::IsValid()) {
                return false;
            }

            if (!m_Property) {
                return false;
            }

            return m_Property->IsValid();
        }

        const RavenAnimationDataComponentBase* RavenCallFunctionEvent::GetAnimationDataEditorOnly() {
            return nullptr;
        }

        const RavenPropertyComponent* RavenCallFunctionEvent::GetPropertyComponent() const {
            return m_Property;
        }

        const RavenTriggerPropertyComponentBase* RavenCallFunctionEvent::GetProperty() const {
            return m_Property;
        }

        void RavenCallFunctionEvent::DestroyEditor(Ptr<RavenSequence> sequence) {
            RavenTriggerEvent::DestroyEditor(sequence);
            if (m_Property) {
                m_Property->DestroyEditor(sequence);
            }
        }

        void RavenCallFunctionEvent::Initialize(Ptr<RavenSequence> sequence) {
            RavenTriggerEvent::Initialize(sequence);
            m_Property->Initialize(sequence);
        }

        void RavenCallFunctionEvent::SetProperty(Ref<RavenTriggerPropertyComponentBase>& value) {
            m_Property = value;
        }

        void RavenCallFunctionEvent::OnEnterCallback(int frame) {
            m_Property->OnEnter();
        }

        void RavenCallFunctionEvent::OnSetTargetEditor(Ptr<RavenSequence> sequence, Ref<SceneObject>& target) {
            if (m_Property) {
                m_Property->SetTarget(target);
                m_Property->Initialize(sequence);
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif