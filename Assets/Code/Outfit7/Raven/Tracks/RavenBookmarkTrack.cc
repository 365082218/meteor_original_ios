#ifdef STARLITE
#include "RavenBookmarkTrack.h"
#include "RavenBookmarkTrack.cs"

namespace Starlite {
    namespace Raven {
        RavenBookmarkTrack::RavenBookmarkTrack() {
        }

        ERavenTrackType RavenBookmarkTrack::GetTrackType() {
            return ERavenTrackType::BookmarkTrack;
        }

        void RavenBookmarkTrack::InitializeEditor(Ref<RavenSequence>& sequence) {
        }
    } // namespace Raven
} // namespace Starlite
#endif