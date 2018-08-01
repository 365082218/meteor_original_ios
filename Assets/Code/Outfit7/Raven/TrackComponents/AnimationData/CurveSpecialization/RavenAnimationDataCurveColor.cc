#ifdef STARLITE
#include "RavenAnimationDataCurveColor.h"
#include "RavenAnimationDataCurveColor.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataCurveColor::RavenAnimationDataCurveColor() {
        }

        Color RavenAnimationDataCurveColor::EvaluateAtTime(double time, double duration) {
            float normalizedTime = (float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType);
            Color v;
            if (m_UniformCurves) {
                auto curve = m_Curves[0].GetObject();
                for (int i = 0; i < c_ValueCount; ++i) {
                    v.data[i] = curve->Evaluate(normalizedTime);
                }
            } else {
                for (int i = 0; i < c_ValueCount; ++i) {
                    v.data[i] = m_Curves[i]->Evaluate(normalizedTime);
                }
            }
            return v;
        }

        void RavenAnimationDataCurveColor::SetStartingValues(const Color& values) {
            m_Curves = Array<Ref<AnimationCurve>>(c_ValueCount);
            for (int i = 0; i < c_ValueCount; ++i) {
                m_Curves[i] = Object::New<AnimationCurve>();
                m_Curves[i]->AddKeyframe(AnimationCurve::Keyframe(0, values.data[i]));
                m_Curves[i]->AddKeyframe(AnimationCurve::Keyframe(1, values.data[i]));
            }
        }

        void RavenAnimationDataCurveColor::SyncStartingValues(const Color& values) {
            for (int i = 0; i < c_ValueCount; ++i) {
                auto& curve = m_Curves[i];
                auto& key = curve->GetKeyframes()[0];
                key.value = values.data[i];
            }
        }

        void RavenAnimationDataCurveColor::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            auto otherReal = static_cast<const RavenAnimationDataCurveColor*>(other);

            m_UniformCurves = otherReal->m_UniformCurves;
        }
    } // namespace Raven
} // namespace Starlite
#endif