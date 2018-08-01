using System;
using UnityEngine;

namespace Starlite.Raven {

    public partial class RavenAnimationDataTweenVector3RectTransformPosition {
#if UNITY_EDITOR

        public override Type TargetType {
            get {
                return typeof(RectTransform);
            }
        }

#endif
    }
}