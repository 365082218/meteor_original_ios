using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenShaderGlobalPropertyVector4 : RavenShaderGlobalPropertyBase<Vector4> {
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

        protected override void SetValue(Vector4 value, UnityEngine.Object targetComponent) {
            Shader.SetGlobalVector(m_TargetShaderPropertyId, value);
        }
    }
}