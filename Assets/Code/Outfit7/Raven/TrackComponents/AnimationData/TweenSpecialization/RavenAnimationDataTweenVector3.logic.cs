using UnityEngine;

namespace Starlite.Raven {

    public partial class RavenAnimationDataTweenVector3 {

        protected override Vector3 GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueVector;
        }
    }
}