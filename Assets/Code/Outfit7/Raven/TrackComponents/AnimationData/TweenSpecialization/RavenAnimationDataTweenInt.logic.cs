namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataTweenInt {

        protected override int GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueInt;
        }
    }
}