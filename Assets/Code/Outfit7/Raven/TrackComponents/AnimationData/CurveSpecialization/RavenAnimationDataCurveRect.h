#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveRect : public RavenAnimationDataCurve<Rectangle> {
            SCLASS_SEALED(RavenAnimationDataCurveRect);

        public:
            RavenAnimationDataCurveRect();
            Rectangle EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const Rectangle& values) final;
            void SyncStartingValues(const Rectangle& values) final;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;

        protected:
            SPROPERTY(Access : "protected");
            bool m_UniformCurves = false;

        private:
            static const int c_ValueCount = 4;
        };
    } // namespace Raven
} // namespace Starlite
#endif