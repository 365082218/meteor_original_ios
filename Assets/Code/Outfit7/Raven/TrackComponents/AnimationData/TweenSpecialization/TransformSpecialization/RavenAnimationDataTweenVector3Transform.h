#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/TweenSpecialization/RavenAnimationDataTweenVector3.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenVector3Transform : public RavenAnimationDataTweenVector3 {
            SCLASS(RavenAnimationDataTweenVector3Transform);

        public:
            RavenAnimationDataTweenVector3Transform() = default;
        };
    } // namespace Raven
} // namespace Starlite
#endif