#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataSet.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataSetRect : public RavenAnimationDataSet<Rectangle> {
            SCLASS_SEALED(RavenAnimationDataSetRect);

        public:
            RavenAnimationDataSetRect() = default;

        protected:
            Rectangle GetValueFromParameterCallback(const RavenParameter* parameter) const override {
                return Rectangle(parameter->m_ValueVector);
            }
        };
    } // namespace Raven
} // namespace Starlite
#endif
