#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyDouble : public RavenAnimationDataPropertyBase<Double> {
            SCLASS_SEALED(RavenAnimationDataPropertyDouble);

        public:
            RavenAnimationDataPropertyDouble();
        };
    } // namespace Raven
} // namespace Starlite
#endif