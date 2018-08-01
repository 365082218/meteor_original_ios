namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetInt {

        protected override int GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueInt;
        }
    }
}