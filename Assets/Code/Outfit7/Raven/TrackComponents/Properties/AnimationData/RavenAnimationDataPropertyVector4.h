#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyVector4 : public RavenAnimationDataPropertyBase<Vector4> {
            SCLASS_SEALED(RavenAnimationDataPropertyVector4);

        public:
            RavenAnimationDataPropertyVector4();

        protected:
            void ProcessValueComponents(Vector4& value, Object* targetComponent) final;

        private:
            static const int c_ValueCount = 4;

            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;
        };
    } // namespace Raven
} // namespace Starlite
#endif