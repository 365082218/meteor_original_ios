#ifdef STARLITE
#pragma once

#include <TrackComponents/Properties/RavenAnimationDataPropertyBase.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataPropertyVector3 : public RavenAnimationDataPropertyBase<Vector3> {
            SCLASS_SEALED(RavenAnimationDataPropertyVector3);

        public:
            RavenAnimationDataPropertyVector3();

        protected:
            void ProcessValueComponents(Vector3& value, Object* targetComponent) final;

        private:
            static const int c_ValueCount = 3;

            SPROPERTY(Access : "private");
            Array<bool> m_ApplyValues;
        };
    } // namespace Raven
} // namespace Starlite
#endif