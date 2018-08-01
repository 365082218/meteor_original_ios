#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetVector3 : public RavenAnimationDataSet<Vector3> {
            SCLASS_SEALED(RavenAnimationDataSetVector3);

        public:
            RavenAnimationDataSetVector3() = default;

        protected:
            Vector3 GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return (Vector3)parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
