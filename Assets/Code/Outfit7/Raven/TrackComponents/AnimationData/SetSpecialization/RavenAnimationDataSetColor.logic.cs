using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetColor {

        protected override Color GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueVector;
        }
    }
}