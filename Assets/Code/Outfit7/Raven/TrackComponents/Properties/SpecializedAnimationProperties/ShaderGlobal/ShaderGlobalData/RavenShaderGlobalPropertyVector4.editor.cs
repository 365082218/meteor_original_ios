namespace Starlite.Raven {

    public sealed partial class RavenShaderGlobalPropertyVector4 {
#if UNITY_EDITOR

        public RavenShaderGlobalPropertyVector4() {
            m_ApplyValues = new bool[c_ValueCount];
            for (int i = 0; i < c_ValueCount; ++i) {
                m_ApplyValues[i] = true;
            }
        }

        public override bool[] ApplyValues {
            get {
                return m_ApplyValues;
            }
        }

#endif
    }
}