#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveFloat : public RavenAnimationDataCurve<float> {
            SCLASS_SEALED(RavenAnimationDataCurveFloat);

        public:
            RavenAnimationDataCurveFloat();
            float EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const float& values) final;
            void SyncStartingValues(const float& values) final;
        };
    } // namespace Raven
} // namespace Starlite
#endif