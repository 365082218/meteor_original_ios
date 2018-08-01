#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenVector4 : public RavenAnimationDataTween<Vector4> {
            SCLASS_SEALED(RavenAnimationDataTweenVector4);

        public:
            RavenAnimationDataTweenVector4() = default;

        protected:
            Vector4 GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif