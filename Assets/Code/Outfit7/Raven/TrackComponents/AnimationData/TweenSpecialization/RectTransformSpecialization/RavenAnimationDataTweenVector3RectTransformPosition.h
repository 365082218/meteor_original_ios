#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/TweenSpecialization/TransformSpecialization/RavenAnimationDataTweenVector3TransformPosition.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataTweenVector3RectTransformPosition : public RavenAnimationDataTweenVector3TransformPosition {
            SCLASS_SEALED(RavenAnimationDataTweenVector3RectTransformPosition);

        public:
            RavenAnimationDataTweenVector3RectTransformPosition() = default;
        };
    } // namespace Raven
} // namespace Starlite
#endif
