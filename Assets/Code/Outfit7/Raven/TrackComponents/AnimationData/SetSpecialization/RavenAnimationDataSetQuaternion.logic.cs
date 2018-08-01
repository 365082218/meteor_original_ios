using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetQuaternion {

        protected override Quaternion GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueVector.ToQuaternion();
        }
    }
}