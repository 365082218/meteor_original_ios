#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveColor : public RavenAnimationDataCurve<Color> {
            SCLASS_SEALED(RavenAnimationDataCurveColor);

        public:
            RavenAnimationDataCurveColor();
            Color EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const Color& values) final;
            void SyncStartingValues(const Color& values) final;
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