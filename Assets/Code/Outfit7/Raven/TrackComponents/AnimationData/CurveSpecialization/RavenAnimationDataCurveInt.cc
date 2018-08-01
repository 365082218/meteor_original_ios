#ifdef STARLITE
#include "RavenAnimationDataCurveInt.h"
#include "RavenAnimationDataCurveInt.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataCurveInt::RavenAnimationDataCurveInt() {
        }

        int RavenAnimationDataCurveInt::EvaluateAtTime(double time, double duration) {
            return (int)m_Curves[0]->Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType));
        }

        void RavenAnimationDataCurveInt::SetStartingValues(const int& values) {
            m_Curves = Array<Ref<AnimationCurve>>(1);
            m_Curves[0] = Object::New<AnimationCurve>();
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(0, (float)values));
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(1, (float)values));
        }

        void RavenAnimationDataCurveInt::SyncStartingValues(const int& values) {
            auto& curve = m_Curves[0];
            auto& key = curve->GetKeyframes()[0];
            key.value = (float)values;
        }
    } // namespace Raven
} // namespace Starlite
#endif