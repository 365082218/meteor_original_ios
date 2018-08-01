#ifdef STARLITE
#pragma once

#include <Tracks/RavenTrack.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenBookmarkTrack : public RavenTrack {
            SCLASS_SEALED(RavenBookmarkTrack);

        public:
            RavenBookmarkTrack();
            ERavenTrackType GetTrackType() override;
            void InitializeEditor(Ref<RavenSequence>& sequence) override;
        };
    } // namespace Raven
} // namespace Starlite
#endif