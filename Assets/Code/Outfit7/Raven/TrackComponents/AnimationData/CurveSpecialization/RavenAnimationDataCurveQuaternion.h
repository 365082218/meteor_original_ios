#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveQuaternion : public RavenAnimationDataCurve<Quaternion> {
            SCLASS_SEALED(RavenAnimationDataCurveQuaternion);

        public:
            RavenAnimationDataCurveQuaternion();
            Quaternion EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const Quaternion& values) final;
            void SyncStartingValues(const Quaternion& values) final;
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