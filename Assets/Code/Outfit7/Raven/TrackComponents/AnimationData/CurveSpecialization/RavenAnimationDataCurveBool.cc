#ifdef STARLITE
#include "RavenAnimationDataCurveBool.h"
#include "RavenAnimationDataCurveBool.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataCurveBool::RavenAnimationDataCurveBool() {
        }

        bool RavenAnimationDataCurveBool::EvaluateAtTime(double time, double duration) {
            return m_Curves[0]->Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType)) >= 1.f ? true : false;
        }

        void RavenAnimationDataCurveBool::SetStartingValues(const bool& values) {
            m_Curves = Array<Ref<AnimationCurve>>(1);
            m_Curves[0] = Object::New<AnimationCurve>();
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(0, values ? 1.f : 0.f));
            m_Curves[0]->AddKeyframe(AnimationCurve::Keyframe(1, values ? 1.f : 0.f));
        }

        void RavenAnimationDataCurveBool::SyncStartingValues(const bool& values) {
            auto& curve = m_Curves[0];
            auto& key = curve->GetKeyframes()[0];
            key.value = values ? 1.f : 0.f;
        }
    } // namespace Raven
} // namespace Starlite
#endif