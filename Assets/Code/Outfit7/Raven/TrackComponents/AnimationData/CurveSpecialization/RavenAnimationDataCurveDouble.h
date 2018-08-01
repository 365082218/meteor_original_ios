#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveDouble : public RavenAnimationDataCurve<Double> {
            SCLASS_SEALED(RavenAnimationDataCurveDouble);

        public:
            RavenAnimationDataCurveDouble();
            Double EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const Double& values) final;
            void SyncStartingValues(const Double& values) final;
        };
    } // namespace Raven
} // namespace Starlite
#endif