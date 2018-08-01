using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetMaterial {

        protected override Material GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueObject as Material;
        }
    }
}