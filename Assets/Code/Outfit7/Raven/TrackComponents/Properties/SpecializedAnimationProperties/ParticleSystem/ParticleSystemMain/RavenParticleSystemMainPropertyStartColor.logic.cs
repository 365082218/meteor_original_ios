#if UNITY_5_6_OR_NEWER

using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenParticleSystemMainPropertyStartColor {

        public override ParticleSystem.MinMaxGradient GetValue(UnityEngine.Object targetComponent) {
            return GetMainModule(targetComponent).startColor;
        }

        protected override void SetValue(ParticleSystem.MinMaxGradient value, UnityEngine.Object targetComponent) {
            var m = GetMainModule(targetComponent);
            m.startColor = value;
        }
    }
}

#endif