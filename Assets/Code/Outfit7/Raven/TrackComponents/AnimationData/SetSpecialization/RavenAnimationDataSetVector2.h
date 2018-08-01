#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetVector2 : public RavenAnimationDataSet<Vector2> {
            SCLASS_SEALED(RavenAnimationDataSetVector2);

        public:
            RavenAnimationDataSetVector2() = default;

        protected:
            Vector2 GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return (Vector2)parameter->m_ValueVector;
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
