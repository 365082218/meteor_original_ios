#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyRect : public RavenAnimationDataPropertyBase<Rectangle> {
            SCLASS_SEALED(RavenAnimationDataPropertyRect);

        public:
            RavenAnimationDataPropertyRect();

        protected:
            void ProcessValueComponents(Rectangle& value, Object* targetComponent) override;

        private:
            static const int c_ValueCount = 4;

            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;
        };
    } // namespace Raven
} // namespace Starlite
#endif