#ifdef STARLITE
#include "RavenTriggerPropertyComponentBase.h"
#include "RavenTriggerPropertyComponentBase.cs"

#include <Utility/RavenUtility.h>
#include <Utility/RavenLog.h>
#include <RavenSequence.h>

using namespace Starlite::Raven::Internal;

namespace Starlite {
    namespace Raven {
        RavenTriggerPropertyComponentBase::RavenTriggerPropertyComponentBase() {
        }

        bool RavenTriggerPropertyComponentBase::CheckForDependencies() const {
            return true;
        }

        bool RavenTriggerPropertyComponentBase::IsValid() const {
            return m_TargetHash != RavenUtility::s_InvalidHash;
        }

        int RavenTriggerPropertyComponentBase::GetParameterIndex() const {
            return m_ParameterIndex;
        }

        const RavenPropertyComponent* RavenTriggerPropertyComponentBase::GetChildPropertyComponent() const {
            return nullptr;
        }

        String RavenTriggerPropertyComponentBase::GetComponentType() const {
            return m_ComponentType;
        }

        String RavenTriggerPropertyComponentBase::GetFullFunctionName() const {
            return RavenUtility::CombineComponentTypeAndFunctionName(m_ComponentType, m_FunctionName);
        }

        String RavenTriggerPropertyComponentBase::GetFunctionName() const {
            return m_FunctionName;
        }

        UInt64 RavenTriggerPropertyComponentBase::GetTargetHash() const {
            return m_TargetHash;
        }

        void RavenTriggerPropertyComponentBase::Initialize(Ptr<RavenSequence> sequence) {
            auto type = pReflection->GetType(m_TargetComponent->GetObjectTypeId());
            auto index = type->FindFunctionIndex(m_FunctionName);
            if (index == (Size_T)-1) {
                index = type->FindFunctionIndex(m_StrippedFunctionName);
                if (index == (Size_T)-1) {
                    pRavenLog->ErrorT(RavenSequence::Tag.GetCString(), "%s failed to find reflection function <%s> or <%s> on type %s(%u)", this, m_FunctionName, m_StrippedFunctionName,
                                      m_TargetComponent->GetObjectTypeName(), m_TargetComponent->GetObjectTypeId());
                }
            }
            int function_index = (int)index;
            m_Function = type->GetFunctions()[function_index];
        }

        bool RavenTriggerPropertyComponentBase::OnBuild(int pass) {
            m_StrippedFunctionName = RavenUtility::GetFunctionNameFromPackedFunctionName(m_FunctionName.GetCString()).c_str();
            return true;
        }

        void RavenTriggerPropertyComponentBase::SetComponentType(String value) {
            m_ComponentType = value;
        }

        void RavenTriggerPropertyComponentBase::SetFunctionName(String value) {
            m_FunctionName = value;
        }

        void RavenTriggerPropertyComponentBase::SetParameterIndexEditor(int parameterIndex) {
            m_ParameterIndex = parameterIndex;
        }

        void RavenTriggerPropertyComponentBase::SetTargetHash(UInt64 value) {
            m_TargetHash = value;
        }
    } // namespace Raven
} // namespace Starlite

#endif