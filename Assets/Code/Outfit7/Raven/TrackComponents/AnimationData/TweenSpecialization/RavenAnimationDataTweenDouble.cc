#ifdef STARLITE
#include "RavenAnimationDataTweenDouble.h"
#include "RavenAnimationDataTweenDouble.cs"

namespace Starlite {
    namespace Raven {
        RavenAnimationDataTweenDouble::RavenAnimationDataTweenDouble() {
        }

        bool RavenAnimationDataTweenDouble::GetRemap() const {
            return m_Remap;
        }

        Double RavenAnimationDataTweenDouble::GetValueFromParameterCallback(const RavenParameter* parameter) const {
            return (Double)parameter->m_ValueFloat;
        }

        Double RavenAnimationDataTweenDouble::PerformRemap(const double& startValue, const double& endValue, double t) {
            return RavenValueInterpolator<double>::Interpolate(m_RemapStart, m_RemapEnd, RavenValueInterpolator<double>::Interpolate(startValue, endValue, t));
        }

        void RavenAnimationDataTweenDouble::CopyValuesCallback(const RavenAnimationDataComponentBase* other) {
            RavenAnimationDataTween<Double>::CopyValuesCallback(other);

            auto otherReal = reinterpret_cast<const RavenAnimationDataTweenDouble*>(other);

            m_Remap = otherReal->m_Remap;
            m_RemapEnd = otherReal->m_RemapEnd;
            m_RemapStart = otherReal->m_RemapStart;
        }
    } // namespace Raven
} // namespace Starlite
#endif