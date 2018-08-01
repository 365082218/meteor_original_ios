#ifdef STARLITE
#pragma once

#include <Tracks/RavenTrack.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenPropertyTrack : public RavenTrack {
            SCLASS_SEALED(RavenPropertyTrack);

        public:
            RavenPropertyTrack();
            ERavenTrackType GetTrackType() final;
            void InitializeEditor(Ref<RavenSequence>& sequence) override;
        };
    } // namespace Raven
} // namespace Starlite
#endif