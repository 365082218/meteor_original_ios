#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveInt : public RavenAnimationDataCurve<int> {
            SCLASS_SEALED(RavenAnimationDataCurveInt);

        public:
            RavenAnimationDataCurveInt();
            int EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const int& values) final;
            void SyncStartingValues(const int& values) final;
        };
    } // namespace Raven
} // namespace Starlite
#endif