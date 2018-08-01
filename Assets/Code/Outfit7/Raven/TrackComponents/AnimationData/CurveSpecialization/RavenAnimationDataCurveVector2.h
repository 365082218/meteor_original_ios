#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveVector2 : public RavenAnimationDataCurve<Vector2> {
            SCLASS_SEALED(RavenAnimationDataCurveVector2);

        public:
            RavenAnimationDataCurveVector2();
            Vector2 EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const Vector2& values) final;
            void SyncStartingValues(const Vector2& values) final;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;

        protected:
            SPROPERTY(Access : "protected");
            bool m_UniformCurves = false;

        private:
            static const int c_ValueCount = 2;
        };
    } // namespace Raven
} // namespace Starlite
#endif