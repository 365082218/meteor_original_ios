#ifdef STARLITE
#include "RavenTrack.h"
#include "RavenTrack.cs"

namespace Starlite {
    namespace Raven {
        RavenTrack::RavenTrack() {
        }

        bool RavenTrack::IsEnabled() const {
            return m_IsEnabled;
        }

        bool RavenTrack::RemoveEvent(Ref<RavenEvent>& evnt) {
            if (m_Events.Remove(evnt)) {
                return true;
            }

            return false;
        }

        List<Ref<RavenEvent>>& RavenTrack::GetEvents() {
            return m_Events;
        }
    } // namespace Raven
} // namespace Starlite
#endif