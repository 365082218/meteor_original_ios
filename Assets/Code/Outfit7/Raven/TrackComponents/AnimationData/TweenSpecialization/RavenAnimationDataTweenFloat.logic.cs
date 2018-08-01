using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataTweenFloat {

        protected override bool Remap {
            get {
                return m_Remap;
            }
        }

        protected override float PerformRemap(float startValue, float endValue, double t) {
            return m_Interpolator.Interpolate(m_RemapStart, m_RemapEnd, m_Interpolator.Interpolate(startValue, endValue, t));
        }

        protected override float GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueFloat;
        }
    }
}