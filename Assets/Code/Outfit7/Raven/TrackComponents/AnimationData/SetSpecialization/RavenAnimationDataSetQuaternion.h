#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetQuaternion : public RavenAnimationDataSet<Quaternion> {
            SCLASS_SEALED(RavenAnimationDataSetQuaternion);

        public:
            RavenAnimationDataSetQuaternion() = default;

        protected:
            Quaternion GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return Quaternion(parameter->m_ValueVector);
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
