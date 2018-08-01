using UnityEngine;

namespace Starlite.Raven {

    public sealed partial class RavenShaderGlobalPropertyColor : RavenShaderGlobalPropertyBase<Color> {

        [SerializeField]
        private bool[] m_ApplyValues = new bool[] { true, true, true, true };
    }
}