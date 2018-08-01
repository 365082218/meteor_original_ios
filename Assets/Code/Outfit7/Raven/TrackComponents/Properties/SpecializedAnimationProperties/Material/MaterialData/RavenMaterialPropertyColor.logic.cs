using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenMaterialPropertyColor {
        private const int c_ValueCount = 4;

        protected override Color ProcessValueComponents(Color value, UnityEngine.Object targetComponent) {
            var synced = false;
            var syncedValue = default(Color);

            for (int i = 0; i < c_ValueCount; ++i) {
                // a choice between call + get vs if, I choose if
                if (!m_ApplyValues[i]) {
                    if (!synced) {
                        synced = true;
                        syncedValue = GetValue(targetComponent);
                    }

                    value[i] = syncedValue[i];
                }
            }

            return value;
        }

        public override Color GetValue(UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return new Color();
            }
            var material = GetMaterial(targetComponent);
#if DEVEL_BUILD
            RavenAssert.IsTrue(material.HasProperty(m_TargetMaterialPropertyId), "Material {0} does not have property {1} on {2}, sequence {3}", material, m_TargetMaterialProperty, targetComponent, this);
#endif
            return material.GetColor(m_TargetMaterialPropertyId);
        }

        protected override void SetValue(Color value, UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return;
            }
            var material = GetMaterial(targetComponent);
#if DEVEL_BUILD
            RavenAssert.IsTrue(material.HasProperty(m_TargetMaterialPropertyId), "Material {0} does not have property {1} on {2}, sequence {3}", material, m_TargetMaterialProperty, targetComponent, this);
#endif
            material.SetColor(m_TargetMaterialPropertyId, value);
        }
    }
}