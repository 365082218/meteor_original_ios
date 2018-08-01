using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenShaderGlobalPropertyVector4 : RavenShaderGlobalPropertyBase<Vector4> {

        [SerializeField]
        private bool[] m_ApplyValues = new bool[] { true, true, true, true };
    }
}