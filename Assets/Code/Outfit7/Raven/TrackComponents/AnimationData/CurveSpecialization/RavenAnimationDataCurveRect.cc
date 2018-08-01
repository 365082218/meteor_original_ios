#ifdef STARLITE
#include "RavenAnimationDataCurveRect.h"
#include "RavenAnimationDataCurveRect.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataCurveRect::RavenAnimationDataCurveRect() {
        }

        Rectangle RavenAnimationDataCurveRect::EvaluateAtTime(double time, double duration) {
            float normalizedTime = (float)GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType);
            Rectangle v;
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

        void RavenAnimationDataCurveRect::SetStartingValues(const Rectangle& values) {
            m_Curves = Array<Ref<AnimationCurve>>(c_ValueCount);
            for (int i = 0; i < c_ValueCount; ++i) {
                m_Curves[i] = Object::New<AnimationCurve>();
                m_Curves[i]->AddKeyframe(AnimationCurve::Keyframe(0, values.data[i]));
                m_Curves[i]->AddKeyframe(AnimationCurve::Keyframe(1, values.data[i]));
            }
        }

        void RavenAnimationDataCurveRect::SyncStartingValues(const Rectangle& values) {
            for (int i = 0; i < c_ValueCount; ++i) {
                auto& curve = m_Curves[i];
                auto& key = curve->GetKeyframes()[0];
                key.value = values.data[i];
            }
        }

        void RavenAnimationDataCurveRect::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            auto otherReal = static_cast<const RavenAnimationDataCurveRect*>(other);

            m_UniformCurves = otherReal->m_UniformCurves;
        }
    } // namespace Raven
} // namespace Starlite
#endif