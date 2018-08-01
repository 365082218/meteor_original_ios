#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetColor : public RavenAnimationDataSet<Color> {
            SCLASS_SEALED(RavenAnimationDataSetColor);

        public:
            RavenAnimationDataSetColor() = default;

        protected:
            Color GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return (Color)parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
