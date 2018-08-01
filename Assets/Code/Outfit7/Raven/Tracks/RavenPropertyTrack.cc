#ifdef STARLITE
#include "RavenPropertyTrack.h"
#include "RavenPropertyTrack.cs"

namespace Starlite {
    namespace Raven {
        RavenPropertyTrack::RavenPropertyTrack() {
        }

        ERavenTrackType RavenPropertyTrack::GetTrackType() {
            return ERavenTrackType::PropertyTrack;
        }

        void RavenPropertyTrack::InitializeEditor(Ref<RavenSequence>& sequence) {
        }
    } // namespace Raven
} // namespace Starlite
#endif