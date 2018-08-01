using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetGradient {

        protected override Gradient GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueGradient;
        }
    }
}