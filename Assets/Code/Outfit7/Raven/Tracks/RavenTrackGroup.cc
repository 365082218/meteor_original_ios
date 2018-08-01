#ifdef STARLITE
#include "RavenTrackGroup.h"
#include "RavenTrackGroup.cs"

namespace Starlite {
    namespace Raven {
        RavenTrackGroup::RavenTrackGroup() {
        }

        int RavenTrackGroup::GetAudioTrackIndex() {
            return m_TrackIndex + 1;
        }

        int RavenTrackGroup::GetPropertyTrackIndex() {
            return m_TrackIndex;
        }

        void RavenTrackGroup::Initialize(Ref<RavenSequence>& sequence) {
        }

        void RavenTrackGroup::SetTrackIndex(int value) {
            if (m_TrackIndex == value) {
                return;
            }

            m_TrackIndex = value;

            if (m_PropertyTrack) {
                for (auto& event : m_PropertyTrack->GetEvents()) {
                    event->SetTrackIndex(GetPropertyTrackIndex());
                }
            }

            if (m_AudioTrack) {
                for (auto& event : m_AudioTrack->GetEvents()) {
                    event->SetTrackIndex(GetAudioTrackIndex());
                }
            }
        }
    } // namespace Raven
} // namespace Starlite
#endif