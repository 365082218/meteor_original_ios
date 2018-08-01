namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetFloat {

        protected override float GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueFloat;
        }
    }
}