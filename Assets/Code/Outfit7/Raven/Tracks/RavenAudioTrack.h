#ifdef STARLITE
#pragma once

#include <Tracks/RavenTrack.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAudioTrack : public RavenTrack {
            SCLASS_SEALED(RavenAudioTrack);

        public:
            RavenAudioTrack();
            ERavenTrackType GetTrackType() final;
            void InitializeEditor(Ref<RavenSequence>& sequence) override;
        };
    } // namespace Raven
} // namespace Starlite
#endif