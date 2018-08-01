#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenQuaternion : public RavenAnimationDataTween<Quaternion> {
            SCLASS_SEALED(RavenAnimationDataTweenQuaternion);

        public:
            RavenAnimationDataTweenQuaternion() = default;

        protected:
            Quaternion GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return Quaternion(parameter->m_ValueVector);
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif