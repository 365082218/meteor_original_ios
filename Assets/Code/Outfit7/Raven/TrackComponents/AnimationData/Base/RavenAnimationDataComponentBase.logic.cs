using System;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataComponentBase {

        public abstract void Initialize(RavenSequence sequence, RavenAnimationPropertyComponentBase property);

        public abstract Type GetAnimationDataType();
    }
}