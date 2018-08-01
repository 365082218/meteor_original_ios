using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetSprite {

        protected override Sprite GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueObject as Sprite;
        }
    }
}