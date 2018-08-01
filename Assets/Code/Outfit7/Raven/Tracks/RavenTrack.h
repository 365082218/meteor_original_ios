#ifdef STARLITE
#pragma once

#include <Enums/RavenTrackType.h>
#include <TrackComponents/RavenComponent.h>
#include <TrackComponents/Events/Base/RavenEvent.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenSequence;

        class RavenTrack : public RavenComponent {
            SCLASS_ABSTRACT(RavenTrack);

        public:
            RavenTrack();
            bool IsEnabled() const;
            bool RemoveEvent(Ref<RavenEvent>& evnt);
            List<Ref<RavenEvent>>& GetEvents();
            virtual ERavenTrackType GetTrackType() = 0;
            virtual void InitializeEditor(Ref<RavenSequence>& sequence) = 0;

        private:
            SPROPERTY(Access : "private");
            List<Ref<RavenEvent>> m_Events;

            SPROPERTY(Access : "private");
            bool m_IsEnabled = true;
        };
    } // namespace Raven
} // namespace Starlite
#endif