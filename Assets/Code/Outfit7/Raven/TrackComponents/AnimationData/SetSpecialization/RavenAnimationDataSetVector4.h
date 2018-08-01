#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetVector4 : public RavenAnimationDataSet<Vector4> {
            SCLASS_SEALED(RavenAnimationDataSetVector4);

        public:
            RavenAnimationDataSetVector4() = default;

        protected:
            Vector4 GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
