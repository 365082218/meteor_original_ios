#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveVector3 : public RavenAnimationDataCurve<Vector3> {
            SCLASS_SEALED(RavenAnimationDataCurveVector3);

        public:
            RavenAnimationDataCurveVector3();
            Vector3 EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const Vector3& values) final;
            void SyncStartingValues(const Vector3& values) final;
            void CopyValuesCallback(const RavenAnimationDataComponentBase* other) override;

        protected:
            SPROPERTY(Access : "protected");
            bool m_UniformCurves = false;

        private:
            static const int c_ValueCount = 3;
        };
    } // namespace Raven
} // namespace Starlite
#endif