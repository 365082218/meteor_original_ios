using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataTweenColor {

        protected override Color GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueVector;
        }
    }
}