namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataTweenDouble {
#if UNITY_EDITOR

        protected override void CopyValuesCallback(RavenAnimationDataComponentBase other) {
            base.CopyValuesCallback(other);
            var otherReal = other as RavenAnimationDataTweenDouble;

            m_Remap = otherReal.m_Remap;
            m_RemapEnd = otherReal.m_RemapEnd;
            m_RemapStart = otherReal.m_RemapStart;
        }

#endif
    }
}