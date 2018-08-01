#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenRect : public RavenAnimationDataTween<Rectangle> {
            SCLASS_SEALED(RavenAnimationDataTweenRect);

        public:
            RavenAnimationDataTweenRect() = default;

        protected:
            Rectangle GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return Rectangle(parameter->m_ValueVector);
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif