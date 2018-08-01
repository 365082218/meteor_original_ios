#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyVector2 : public RavenAnimationDataPropertyBase<Vector2> {
            SCLASS_SEALED(RavenAnimationDataPropertyVector2);

        public:
            RavenAnimationDataPropertyVector2();

        protected:
            void ProcessValueComponents(Vector2& value, Object* targetComponent) final;

        private:
            static const int c_ValueCount = 2;

            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;
        };
    } // namespace Raven
} // namespace Starlite
#endif