#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenColor : public RavenAnimationDataTween<Color> {
            SCLASS_SEALED(RavenAnimationDataTweenColor);

        public:
            RavenAnimationDataTweenColor() = default;

        protected:
            Color GetValueFromParameterCallback(const RavenParameter* parameter) const final {
                return (Color)parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif