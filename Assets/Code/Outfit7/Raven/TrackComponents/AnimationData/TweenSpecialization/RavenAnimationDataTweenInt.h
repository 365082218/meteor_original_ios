#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataTween.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenInt : public RavenAnimationDataTween<int> {
            SCLASS_SEALED(RavenAnimationDataTweenInt);

        public:
            RavenAnimationDataTweenInt() = default;

        protected:
            int GetValueFromParameterCallback(const RavenParameter* parameter) const final {
                return parameter->m_ValueInt;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif