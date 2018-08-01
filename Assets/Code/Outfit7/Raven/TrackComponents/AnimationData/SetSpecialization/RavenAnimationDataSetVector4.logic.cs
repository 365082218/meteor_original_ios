using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetVector4 {

        protected override Vector4 GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueVector;
        }
    }
}