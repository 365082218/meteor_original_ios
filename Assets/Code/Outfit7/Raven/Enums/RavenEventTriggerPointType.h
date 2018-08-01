#ifdef STARLITE
#pragma once

namespace Starlite {
    namespace Raven {
        SENUM()
        enum class ERavenEventTriggerPointType {
            Start = 1000,
            Process = 2000,
            Pause = 3000,
            Bookmark = 4000,
            Barrier = 5000,
            End = 6000,
        };
    } // namespace Raven
} // namespace Starlite
#endif