namespace Starlite.Raven {

    public sealed partial class RavenAnimationDataSetDouble {

        protected override double GetValueFromParameterCallback(RavenParameter parameter) {
            return parameter.m_ValueFloat;
        }
    }
}