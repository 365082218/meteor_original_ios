#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetInt : public RavenAnimationDataSet<int> {
            SCLASS_SEALED(RavenAnimationDataSetInt);

        public:
            RavenAnimationDataSetInt() = default;

        protected:
            int GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return parameter->m_ValueInt;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
