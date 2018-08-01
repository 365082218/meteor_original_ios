using Starlite.Raven.Internal;
using System;
using UnityEngine;

namespace Starlite.Raven {

    [TriggerAnimationData]
    public abstract partial class RavenAnimationDataSet<T> {

        protected T m_StartValue;

        [NonSerialized]
        protected RavenParameter m_StartParameter;

        protected RavenValueInterpolatorBase<T> m_Interpolator;

        public override bool ShouldSyncStartingValues {
            get {
                return m_StartValueType == ERavenValueType.Current;
            }
        }

        public override void Initialize(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            base.Initialize(sequence, property);
            m_Interpolator = RavenInterpolatorOverseer.GetInterpolator<T>();
            m_StartParameter = sequence.GetParameterAtIndex(m_StartParameterIndex);

            ValidateParameters();
        }

        public override T EvaluateAtTime(double time, double duration) {
            var startValue = m_StartParameter != null ? GetValueFromParameter(m_StartParameter) : (m_StartValueIsObjectLink ? m_Property.GetValue(m_StartValueObjectLink) : m_StartValue);
            return startValue;
        }

        protected override void OnEnterCallback() {
            m_StartValue = GetStartValue(m_StartValue, m_StartValueStart, m_StartValueEnd, m_StartValueType, m_StartParameter, m_StartValueIsObjectLink);
        }

        protected override void OnExitCallback() {
        }

        protected override void SyncStartingValues(T values) {
            if (m_StartParameterIndex < 0 && m_StartValueType == ERavenValueType.Current) {
                m_StartValue = values;
            }
        }

        protected T GetStartValue(T currentValue, T valueStart, T valueEnd, ERavenValueType valueType, RavenParameter parameter, bool isObjectLink) {
            if (parameter != null) {
                return currentValue;
            }

            if (isObjectLink) {
                return currentValue;
            }

            switch (valueType) {
                case ERavenValueType.Constant:
                    return valueStart;

                case ERavenValueType.Range:
                    return m_Interpolator.Random(valueStart, valueEnd);

                case ERavenValueType.Current:
                    return currentValue;
            }

            return default(T);
        }

        private void ValidateParameters() {
            if (m_StartParameterIndex >= 0 && m_StartParameter == null) {
                RavenLog.ErrorT(RavenSequence.Tag, "Parameter at index {0} does not exist for {1}! Ignoring.", m_StartParameterIndex, this);
            }
        }
    }
}