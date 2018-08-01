using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetVector3 {

        protected override Vector3 GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueVector;
        }
    }
}