#ifdef STARLITE
#include "RavenAnimationDataCurveFloat.h"
#include "RavenAnimationDataCurveFloat.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataCurveFloat::RavenAnimationDataCurveFloat() {
        }

        float RavenAnimationDataCurveFloat::EvaluateAtTime(double time, double duration) {
            return m_Curves[0]->Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType));
        }

        void RavenAnimationDataCurveFloat::SetStartingValues(const float& values) {
            m_Curves = Array<Ref<AnimationCurve>>(1);
            m_Curves[0] = Object::New<AnimationCurve>();
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(0, values));
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(1, values));
        }

        void RavenAnimationDataCurveFloat::SyncStartingValues(const float& values) {
            auto& curve = m_Curves[0];
            auto& key = curve->GetKeyframes()[0];
            key.value = values;
        }
    } // namespace Raven
} // namespace Starlite
#endif