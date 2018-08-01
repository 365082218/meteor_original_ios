using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetRect {

        protected override Rect GetValueFromParameterCallback(RavenParameter parameter) {
            var vec = parameter.m_ValueVector;
            return new Rect(vec.x, vec.y, vec.z, vec.w);
        }
    }
}