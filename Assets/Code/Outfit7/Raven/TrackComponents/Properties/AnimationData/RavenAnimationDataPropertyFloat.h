#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyFloat : public RavenAnimationDataPropertyBase<float> {
            SCLASS_SEALED(RavenAnimationDataPropertyFloat);

        public:
            RavenAnimationDataPropertyFloat();
        };
    } // namespace Raven
} // namespace Starlite
#endif
