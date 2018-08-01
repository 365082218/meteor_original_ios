#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyColor : public RavenAnimationDataPropertyBase<Color> {
            SCLASS_SEALED(RavenAnimationDataPropertyColor);

        public:
            RavenAnimationDataPropertyColor();

        protected:
            void ProcessValueComponents(Color& value, Object* targetComponent) final;

        private:
            static const int c_ValueCount = 4;

            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;
        };
    } // namespace Raven
} // namespace Starlite
#endif