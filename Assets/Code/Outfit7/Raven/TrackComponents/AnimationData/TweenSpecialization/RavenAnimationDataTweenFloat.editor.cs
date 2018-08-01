namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataTweenFloat {
#if UNITY_EDITOR

        protected override void CopyValuesCallback(RavenAnimationDataComponentBase other) {
            base.CopyValuesCallback(other);
            var otherReal = other as RavenAnimationDataTweenFloat;

            m_Remap = otherReal.m_Remap;
            m_RemapEnd = otherReal.m_RemapEnd;
            m_RemapStart = otherReal.m_RemapStart;
        }

#endif
    }
}