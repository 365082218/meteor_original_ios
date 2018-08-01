#ifdef STARLITE
#pragma once

#include <TrackComponents/RavenComponent.h>
#include <Tracks/RavenAudioTrack.h>
#include <Tracks/RavenPropertyTrack.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenSequence;

        class RavenTrackGroup : public RavenComponent {
            SCLASS_SEALED(RavenTrackGroup);

        public:
            SENUM() enum class ERavenTrackGroupMode { Extended = 0, Optimized, Collapsed };

        public:
            RavenTrackGroup();
            int GetAudioTrackIndex();
            int GetPropertyTrackIndex();
            void Initialize(Ref<RavenSequence>& sequence);
            void SetTrackIndex(int value);

        public:
            SPROPERTY();
            int m_OverrideTargetsParameterIndex = -1;

            SPROPERTY();
            RavenTrackGroup::ERavenTrackGroupMode m_TrackGroupMode = ERavenTrackGroupMode::Extended;

            SPROPERTY();
            Ref<RavenAudioTrack> m_AudioTrack;

            SPROPERTY();
            Ref<RavenPropertyTrack> m_PropertyTrack;

            SPROPERTY();
            Ref<SceneObject> m_Target;

            static const int c_TrackCount = 2;

        private:
            SPROPERTY(Access : "private");
            int m_TrackIndex;
        };
    } // namespace Raven
} // namespace Starlite
#endif