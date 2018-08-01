#ifdef STARLITE
#include "RavenAudioTrack.h"
#include "RavenAudioTrack.cs"

#include <RavenSequence.h>

namespace Starlite {
    namespace Raven {
        RavenAudioTrack::RavenAudioTrack() {
        }

        ERavenTrackType RavenAudioTrack::GetTrackType() {
            return ERavenTrackType::AudioTrack;
        }

        void RavenAudioTrack::InitializeEditor(Ref<RavenSequence>& sequence) {
            sequence->TrackGroupsChanged();
        }
    } // namespace Raven
} // namespace Starlite
#endif