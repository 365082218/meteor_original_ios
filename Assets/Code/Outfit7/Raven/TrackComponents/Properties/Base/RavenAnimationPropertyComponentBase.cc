#ifdef STARLITE
#include "RavenAnimationPropertyComponentBase.h"
#include "RavenAnimationPropertyComponentBase.cs"

#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        RavenAnimationPropertyComponentBase::RavenAnimationPropertyComponentBase() {
        }

        bool RavenAnimationPropertyComponentBase::IsTriggerPropertyValid() const {
            return m_TriggerProperty == nullptr || m_TriggerProperty->GetTargetHash() != RavenUtility::s_InvalidHash;
        }

        bool RavenAnimationPropertyComponentBase::IsValid() const {
            bool propertyValid = m_AnimationData != nullptr && (IsCustom() ? IsCustomValid() : m_TargetHash != RavenUtility::s_InvalidHash);
            return propertyValid && IsTriggerPropertyValid();
        }

        const RavenPropertyComponent* RavenAnimationPropertyComponentBase::GetChildPropertyComponent() const {
            return m_TriggerProperty;
        }

        String RavenAnimationPropertyComponentBase::GetComponentType() const {
            return m_ComponentType;
        }

        String RavenAnimationPropertyComponentBase::GetMemberName() {
            return m_MemberName;
        }

        const String& RavenAnimationPropertyComponentBase::GetMemberName() const {
            return m_MemberName;
        }

        UInt64 RavenAnimationPropertyComponentBase::GetTargetHash() const {
            return m_TargetHash;
        }

        void RavenAnimationPropertyComponentBase::DestroyEditor(Ptr<RavenSequence> sequence) {
        }

        void RavenAnimationPropertyComponentBase::Initialize(Ptr<RavenSequence> sequence) {
            RavenPropertyComponent::Initialize(sequence);
            if (m_TriggerProperty != nullptr) {
                m_TriggerProperty->Initialize(sequence);
            }
            if (m_AnimationData != nullptr) {
                m_AnimationData->Initialize(sequence, this);
            }
        }

        void RavenAnimationPropertyComponentBase::SetComponentType(String value) {
            m_ComponentType = value;
        }

        void RavenAnimationPropertyComponentBase::SetMemberName(String value) {
            m_MemberName = value;
        }

        void RavenAnimationPropertyComponentBase::SetTargetHash(UInt64 value) {
            m_TargetHash = value;
        }
    } // namespace Raven
} // namespace Starlite
#endif