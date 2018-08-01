#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyMaterial : public RavenAnimationDataPropertyBase<Ref<Material>> {
            SCLASS_SEALED(RavenAnimationDataPropertyMaterial);

        public:
            RavenAnimationDataPropertyMaterial();
        };
    } // namespace Raven
} // namespace Starlite
#endif