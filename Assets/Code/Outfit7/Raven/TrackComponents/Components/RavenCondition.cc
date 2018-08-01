#ifdef STARLITE
#include "RavenCondition.h"
#include "RavenCondition.cs"

#include <math.h>
#include <Enums/RavenParameterType.h>

namespace Starlite {
    namespace Raven {
        bool RavenCondition::IsTrue() const {
            if (m_Parameter->m_ParameterType == ERavenParameterType::Float) {
                float value = m_ValueParameter != nullptr ? m_ValueParameter->m_ValueFloat : m_ValueFloat;
                switch (m_ConditionMode) {
                case ERavenConditionMode::Less:
                    return m_Parameter->m_ValueFloat < value;

                case ERavenConditionMode::LessOrEqual:
                    return m_Parameter->m_ValueFloat <= value;

                case ERavenConditionMode::Equal:
                    return Math::Abs(m_Parameter->m_ValueFloat - value) < 0.0001f;

                case ERavenConditionMode::GreaterOrEqual:
                    return m_Parameter->m_ValueFloat >= value;

                case ERavenConditionMode::Greater:
                    return m_Parameter->m_ValueFloat > value;

                case ERavenConditionMode::NotEqual:
                    return Math::Abs(m_Parameter->m_ValueFloat - value) >= 0.0001f;

                case ERavenConditionMode::BitSet:
                    Assert(false, "BitSet cannot be used with float parameter!");

                case ERavenConditionMode::BitNotSet:
                    Assert(false, "BitNotSet cannot be used with float parameter!");

                case ERavenConditionMode::BitMaskSet:
                    Assert(false, "BitMaskSet cannot be used with float parameter!");

                case ERavenConditionMode::BitMaskNotSet:
                    Assert(false, "BitMaskNotSet cannot be used with float parameter!");
                }
            } else if (m_Parameter->m_ParameterType == ERavenParameterType::Object || m_Parameter->m_ParameterType == ERavenParameterType::ActorList ||
                       m_Parameter->m_ParameterType == ERavenParameterType::Vector4) {
                Assert(false, "Can't compare Vector4 or Component parameters");
            } else {
                int value = m_ValueParameter != nullptr ? m_ValueParameter->m_ValueInt : m_ValueInt;
                switch (m_ConditionMode) {
                case ERavenConditionMode::Less:
                    return m_Parameter->m_ValueInt < value;

                case ERavenConditionMode::LessOrEqual:
                    return m_Parameter->m_ValueInt <= value;

                case ERavenConditionMode::Equal:
                    return m_Parameter->m_ValueInt == value;

                case ERavenConditionMode::GreaterOrEqual:
                    return m_Parameter->m_ValueInt >= value;

                case ERavenConditionMode::Greater:
                    return m_Parameter->m_ValueInt > value;

                case ERavenConditionMode::NotEqual:
                    return m_Parameter->m_ValueInt != value;

                case ERavenConditionMode::BitSet:
                    return (m_Parameter->m_ValueInt & (1 << value)) > 0;

                case ERavenConditionMode::BitNotSet:
                    return (m_Parameter->m_ValueInt & (1 << value)) == 0;

                case ERavenConditionMode::BitMaskSet:
                    return (m_Parameter->m_ValueInt & value) == value;

                case ERavenConditionMode::BitMaskNotSet:
                    return (m_Parameter->m_ValueInt & value) == 0;
                }
            }
            return false;
        }

        void RavenCondition::ResetTrigger() {
            if (m_Parameter->m_ParameterType != ERavenParameterType::BoolTrigger && m_Parameter->m_ParameterType != ERavenParameterType::IntTrigger &&
                m_Parameter->m_ParameterType != ERavenParameterType::EnumTrigger) {
                return;
            }
            m_Parameter->ResetTrigger();
        }
    } // namespace Raven
} // namespace Starlite
#endif