namespace Starlite.Raven {

    public sealed partial class RavenParticleSystemEMPropertyEnabled {

        public override bool GetValue(UnityEngine.Object targetComponent) {
            return GetEmissionModule(targetComponent).enabled;
        }

        protected override void SetValue(bool value, UnityEngine.Object targetComponent) {
            var em = GetEmissionModule(targetComponent);
            em.enabled = value;
        }
    }
}