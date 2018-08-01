using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenMaterialPropertyVector4 : RavenMaterialPropertyBase<Vector4> {
        private const int c_ValueCount = 4;

        protected override Vector4 ProcessValueComponents(Vector4 value, UnityEngine.Object targetComponent) {
            var synced = false;
            var syncedValue = default(Vector4);

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

        public override Vector4 GetValue(UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return new Vector4();
            }
            var material = GetMaterial(targetComponent);
#if DEVEL_BUILD
            RavenAssert.IsTrue(material.HasProperty(m_TargetMaterialPropertyId), "Material {0} does not have property {1} on {2}, sequence {3}", material, m_TargetMaterialProperty, targetComponent, this);
#endif
            return material.GetVector(m_TargetMaterialPropertyId);
        }

        protected override void SetValue(Vector4 value, UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return;
            }
            var material = GetMaterial(targetComponent);
#if DEVEL_BUILD
            RavenAssert.IsTrue(material.HasProperty(m_TargetMaterialPropertyId), "Material {0} does not have property {1} on {2}, sequence {3}", material, m_TargetMaterialProperty, targetComponent, this);
#endif
            material.SetVector(m_TargetMaterialPropertyId, value);
        }
    }
}