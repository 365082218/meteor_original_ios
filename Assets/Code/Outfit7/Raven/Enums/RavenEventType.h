#ifdef STARLITE
#pragma once

namespace Starlite {
    namespace Raven {
        SENUM()
        enum class ERavenEventType {
            Bookmark,
            Trigger,
            Continuous,
        };
    } // namespace Raven
} // namespace Starlite
#endif