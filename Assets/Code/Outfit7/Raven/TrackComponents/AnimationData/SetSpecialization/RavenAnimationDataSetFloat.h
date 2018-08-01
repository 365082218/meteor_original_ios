#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetFloat : public RavenAnimationDataSet<float> {
            SCLASS_SEALED(RavenAnimationDataSetFloat);

        public:
            RavenAnimationDataSetFloat() = default;

        protected:
            float GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return parameter->m_ValueFloat;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
