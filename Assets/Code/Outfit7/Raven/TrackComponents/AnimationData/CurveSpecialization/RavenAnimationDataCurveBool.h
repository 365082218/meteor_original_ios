#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveBool : public RavenAnimationDataCurve<bool> {
            SCLASS_SEALED(RavenAnimationDataCurveBool);

        public:
            RavenAnimationDataCurveBool();
            bool EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const bool& values) final;
            void SyncStartingValues(const bool& values) final;
        };
    } // namespace Raven
} // namespace Starlite
#endif