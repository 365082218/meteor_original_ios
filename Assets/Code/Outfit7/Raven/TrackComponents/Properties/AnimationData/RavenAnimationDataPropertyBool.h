#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyBool : public RavenAnimationDataPropertyBase<bool> {
            SCLASS_SEALED(RavenAnimationDataPropertyBool);

        public:
            RavenAnimationDataPropertyBool();
        };
    } // namespace Raven
} // namespace Starlite
#endif