#ifdef STARLITE
#include "RavenAnimationDataCurveDouble.h"
#include "RavenAnimationDataCurveDouble.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataCurveDouble::RavenAnimationDataCurveDouble() {
        }

        Double RavenAnimationDataCurveDouble::EvaluateAtTime(double time, double duration) {
            return (Double)m_Curves[0]->Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType));
        }

        void RavenAnimationDataCurveDouble::SetStartingValues(const Double& values) {
            m_Curves = Array<Ref<AnimationCurve>>(1);
            m_Curves[0] = Object::New<AnimationCurve>();
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(0, (float)values));
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(1, (float)values));
        }

        void RavenAnimationDataCurveDouble::SyncStartingValues(const Double& values) {
            auto& curve = m_Curves[0];
            auto& key = curve->GetKeyframes()[0];
            key.value = (float)values;
        }
    } // namespace Raven
} // namespace Starlite
#endif