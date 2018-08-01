#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertySprite : public RavenAnimationDataPropertyBase<Ref<Sprite>> {
            SCLASS_SEALED(RavenAnimationDataPropertySprite);

        public:
            RavenAnimationDataPropertySprite();
        };
    } // namespace Raven
} // namespace Starlite
#endif