#ifdef STARLITE
#pragma once

#include <TrackComponents/AnimationData/Base/RavenAnimationDataCurve.h>

using namespace Starlite;

namespace Starlite {
    namespace Raven {
        class RavenAnimationDataCurveVector4 : public RavenAnimationDataCurve<Vector4> {
            SCLASS_SEALED(RavenAnimationDataCurveVector4);

        public:
            RavenAnimationDataCurveVector4();
            Vector4 EvaluateAtTime(double time, double duration) final;

        protected:
            void SetStartingValues(const Vector4& values) final;
            void SyncStartingValues(const Vector4& values) final;
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