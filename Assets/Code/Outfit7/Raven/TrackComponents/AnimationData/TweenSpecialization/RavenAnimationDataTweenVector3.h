#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenVector3 : public RavenAnimationDataTween<Vector3> {
            SCLASS(RavenAnimationDataTweenVector3);

        public:
            RavenAnimationDataTweenVector3() = default;

        protected:
            Vector3 GetValueFromParameterCallback(const RavenParameter* parameter) const final {
                return (Vector3)parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif