#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyInt : public RavenAnimationDataPropertyBase<int> {
            SCLASS_SEALED(RavenAnimationDataPropertyInt);

        public:
            RavenAnimationDataPropertyInt();
        };
    } // namespace Raven
} // namespace Starlite
#endif