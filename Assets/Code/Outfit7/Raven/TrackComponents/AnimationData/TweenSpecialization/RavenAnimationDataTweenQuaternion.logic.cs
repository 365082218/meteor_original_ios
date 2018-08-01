using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataTweenQuaternion {

        protected override Quaternion GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueVector.ToQuaternion();
        }
    }
}