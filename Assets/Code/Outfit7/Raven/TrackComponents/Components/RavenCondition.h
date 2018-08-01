#ifdef STARLITE
#pragma once

#include <Enums/RavenConditionMode.h>
#include <TrackComponents/Components/RavenParameter.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenCondition : public SerializableObject {
            SCLASS_SEALED(RavenCondition);

        public:
            RavenCondition() = default;
            bool IsTrue() const;
            void ResetTrigger();

        public:
            SPROPERTY();
            ERavenConditionMode m_ConditionMode = ERavenConditionMode::Equal;

            SPROPERTY();
            float m_ValueFloat = 0;

            SPROPERTY();
            int m_ParameterIndex = -1;

            SPROPERTY();
            int m_ValueIndex = -1;

            SPROPERTY();
            int m_ValueInt = 0;

            Ref<RavenParameter> m_Parameter;
            Ref<RavenParameter> m_ValueParameter;
        };
    } // namespace Raven
} // namespace Starlite
#endif