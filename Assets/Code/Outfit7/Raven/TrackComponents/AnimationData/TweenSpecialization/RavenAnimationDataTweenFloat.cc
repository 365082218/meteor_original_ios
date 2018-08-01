#ifdef STARLITE
#include "RavenAnimationDataTweenFloat.h"
#include "RavenAnimationDataTweenFloat.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataTweenFloat::RavenAnimationDataTweenFloat() {
        }

        bool RavenAnimationDataTweenFloat::GetRemap() const {
            return m_Remap;
        }

        float RavenAnimationDataTweenFloat::GetValueFromParameterCallback(const RavenParameter* parameter) const {
            return parameter->m_ValueFloat;
        }

        float RavenAnimationDataTweenFloat::PerformRemap(const float& startValue, const float& endValue, double t) {
            return RavenValueInterpolator<float>::Interpolate(m_RemapStart, m_RemapEnd, RavenValueInterpolator<float>::Interpolate(startValue, endValue, t));
        }

        void RavenAnimationDataTweenFloat::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            RavenAnimationDataTween<float>::CopyValuesCallback(other);

            auto otherReal = reinterpret_cast<const RavenAnimationDataTweenFloat*>(other);

            m_Remap = otherReal->m_Remap;
            m_RemapEnd = otherReal->m_RemapEnd;
            m_RemapStart = otherReal->m_RemapStart;
        }
    } // namespace Raven
} // namespace Starlite
#endif