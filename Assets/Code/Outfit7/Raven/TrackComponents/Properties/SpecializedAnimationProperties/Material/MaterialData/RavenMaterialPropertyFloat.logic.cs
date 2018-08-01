namespace Starlite.Raven {

    public sealed partial class RavenMaterialPropertyFloat {

        public override float GetValue(UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return 0f;
            }
            var material = GetMaterial(targetComponent);
#if DEVEL_BUILD
            RavenAssert.IsTrue(material.HasProperty(m_TargetMaterialPropertyId), "Material {0} does not have property {1} on {2}, sequence {3}", material, m_TargetMaterialProperty, targetComponent, this);
#endif
            return material.GetFloat(m_TargetMaterialPropertyId);
        }

        protected override void SetValue(float value, UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return;
            }
            var material = GetMaterial(targetComponent);
#if DEVEL_BUILD
            RavenAssert.IsTrue(material.HasProperty(m_TargetMaterialPropertyId), "Material {0} does not have property {1} on {2}, sequence {3}", material, m_TargetMaterialProperty, targetComponent, this);
#endif
            material.SetFloat(m_TargetMaterialPropertyId, value);
        }
    }
}