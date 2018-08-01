#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyQuaternion : public RavenAnimationDataPropertyBase<Quaternion> {
            SCLASS_SEALED(RavenAnimationDataPropertyQuaternion);

        public:
            RavenAnimationDataPropertyQuaternion();

        protected:
            void ProcessValueComponents(Quaternion& value, Object* targetComponent) final;

        private:
            static const int c_ValueCount = 4;

            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;
        };
    } // namespace Raven
} // namespace Starlite
#endif