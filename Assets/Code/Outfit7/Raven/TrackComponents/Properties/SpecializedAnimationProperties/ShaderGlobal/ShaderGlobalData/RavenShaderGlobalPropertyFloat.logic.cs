using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenShaderGlobalPropertyFloat : RavenShaderGlobalPropertyBase<float> {

        protected override void SetValue(float value, UnityEngine.Object targetComponent) {
            Shader.SetGlobalFloat(m_TargetShaderPropertyId, value);
        }
    }
}