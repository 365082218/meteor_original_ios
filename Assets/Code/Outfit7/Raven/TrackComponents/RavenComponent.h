#ifdef STARLITE
#pragma once

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenComponent : public SceneObjectComponent {
            SCLASS_ABSTRACT(RavenComponent);

        public:
            RavenComponent() = default;
        };
    } // namespace Raven
} // namespace Starlite
#endif