using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataTweenDouble {

        protected override bool Remap {
            get {
                return m_Remap;
            }
        }

        protected override double PerformRemap(double startValue, double endValue, double t) {
            return m_Interpolator.Interpolate(m_RemapStart, m_RemapEnd, m_Interpolator.Interpolate(startValue, endValue, t));
        }

        protected override double GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueFloat;
        }
    }
}