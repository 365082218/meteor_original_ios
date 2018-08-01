#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetDouble : public RavenAnimationDataSet<Double> {
            SCLASS_SEALED(RavenAnimationDataSetDouble);

        public:
            RavenAnimationDataSetDouble() = default;

        protected:
            Double GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return (Double)parameter->m_ValueFloat;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
