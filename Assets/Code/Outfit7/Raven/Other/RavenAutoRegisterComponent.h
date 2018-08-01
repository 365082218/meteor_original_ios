#ifdef STARLITE
#pragma once

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAutoRegisterComponent : public SceneObjectComponent {
            SCLASS_SEALED(RavenAutoRegisterComponent);

        public:
            RavenAutoRegisterComponent() = default;

        public:
            SPROPERTY();
            String m_Parameter;
        };
    } // namespace Raven
} // namespace Starlite
#endif