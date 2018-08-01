#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetBool : public RavenAnimationDataSet<bool> {
            SCLASS_SEALED(RavenAnimationDataSetBool);

        public:
            RavenAnimationDataSetBool() = default;

        protected:
            bool GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return parameter->m_ValueInt == 1;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
