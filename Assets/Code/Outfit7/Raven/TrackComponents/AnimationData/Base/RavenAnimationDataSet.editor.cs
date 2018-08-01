using Starlite.Raven.Internal;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataSet<T> {
#if UNITY_EDITOR

        public T GetStartValue(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            return GetVaryingValueEditorInternal(m_StartValueStart, m_StartValueEnd, m_StartValueType, ref m_StartParameterIndex, m_StartValueIsObjectLink, m_StartValueObjectLink, m_Interpolator, sequence);
        }

        protected override void SetStartingValues(T values) {
            m_StartValueStart = values;
            m_StartValueEnd = values;
        }

        protected override void CopyValuesCallback(RavenAnimationDataComponentBase other) {
            var otherReal = other as RavenAnimationDataSet<T>;

            m_StartParameterIndex = otherReal.m_StartParameterIndex;
            m_StartValueEnd = otherReal.m_StartValueEnd;
            m_StartValueStart = otherReal.m_StartValueStart;
            m_StartValueType = otherReal.m_StartValueType;
            m_StartValueIsObjectLink = otherReal.m_StartValueIsObjectLink;
            m_StartValueObjectLink = otherReal.m_StartValueObjectLink;
        }

        public override bool CheckForDependencies() {
            try {
                RavenInterpolatorOverseer.GetInterpolator<T>();
            } catch {
                RavenLog.Error("Interpolator for type {0} does not exist!", typeof(T).ToString());
                return false;
            }

            return true;
        }

#endif
    }
}