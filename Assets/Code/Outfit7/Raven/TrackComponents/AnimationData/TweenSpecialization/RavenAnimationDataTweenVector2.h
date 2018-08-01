#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenVector2 : public RavenAnimationDataTween<Vector2> {
            SCLASS_SEALED(RavenAnimationDataTweenVector2);

        public:
            RavenAnimationDataTweenVector2() = default;

        protected:
            Vector2 GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return (Vector2)parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif