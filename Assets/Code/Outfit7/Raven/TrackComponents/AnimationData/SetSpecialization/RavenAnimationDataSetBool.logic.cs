namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetBool {

        protected override bool GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueInt == 1;
        }
    }
}