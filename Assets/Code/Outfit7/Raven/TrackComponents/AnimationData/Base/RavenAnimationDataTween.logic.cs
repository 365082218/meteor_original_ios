using Starlite.Raven.Internal;
using System;

namespace Starlite.Raven {

    [ContinuousAnimationData]
    public abstract partial class RavenAnimationDataTween<T> {

        protected T m_StartValue;
        protected T m_EndValue;

        [NonSerialized]
        protected RavenParameter m_StartParameter;

        [NonSerialized]
        protected RavenParameter m_EndParameter;

        [NonSerialized]
        protected RavenParameter m_StartTangentParameter;

        [NonSerialized]
        protected RavenParameter m_EndTangentParameter;

        protected RavenValueInterpolatorBase<T> m_Interpolator;

        // We'll handle logic in the SyncStartingValues function
        public override bool ShouldSyncStartingValues {
            get {
                return true;
            }
        }

        protected virtual bool Remap {
            get {
                return false;
            }
        }

        public override void Initialize(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            base.Initialize(sequence, property);
            m_Interpolator = RavenInterpolatorOverseer.GetInterpolator<T>();
            m_StartParameter = sequence.GetParameterAtIndex(m_StartParameterIndex);
            m_EndParameter = sequence.GetParameterAtIndex(m_EndParameterIndex);
            m_Property = property as RavenAnimationPropertyBase<T>;

            ValidateParameters();
        }

        public sealed override T EvaluateAtTime(double time, double duration) {
            var startValue = m_StartParameter != null ? GetValueFromParameter(m_StartParameter) : (m_StartValueIsObjectLink ? m_Property.GetValue(m_StartValueObjectLink) : m_StartValue);
            var endValue = m_EndParameter != null ? GetValueFromParameter(m_EndParameter) : (m_EndValueIsObjectLink ? m_Property.GetValue(m_EndValueObjectLink) : m_EndValue);
            var t = m_UseCustomEaseCurve ? m_EaseCurve.Evaluate((float)GetNormalizedTime(GetCurrentTime(time, duration), duration)) : GetNormalizedTime(GetCurrentTime(time, duration), duration, m_EaseType, m_EaseAmplitude, m_EasePeriod);
            if (Remap) {
                return PerformRemap(startValue, endValue, t);
            }
            T value = default(T);
            if (PostEvaluateAtTime(startValue, endValue, t, ref value)) {
                return value;
            }
            return m_Interpolator.Interpolate(startValue, endValue, t);
        }

        protected override void OnEnterCallback() {
            m_StartValue = GetStartValue(m_StartValue, m_StartValueStart, m_StartValueEnd, m_StartValueType, m_StartParameter, m_StartValueIsObjectLink);
            m_EndValue = GetStartValue(m_EndValue, m_EndValueStart, m_EndValueEnd, m_EndValueType, m_EndParameter, m_EndValueIsObjectLink);
        }

        protected override void OnExitCallback() {
        }

        protected override void SyncStartingValues(T values) {
            if (m_StartParameterIndex < 0 && m_StartValueType == ERavenValueType.Current) {
                m_StartValue = values;
            }
            if (m_EndParameterIndex < 0 && m_EndValueType == ERavenValueType.Current) {
                m_EndValue = values;
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

        protected virtual T PerformRemap(T startValue, T endValue, double t) {
            return default(T);
        }

        protected virtual bool PostEvaluateAtTime(T startValue, T endValue, double t, ref T value) {
            return false;
        }

        protected double GetCurrentTime(double time, double duration) {
            if (m_RepeatCount > 1) {
                return GetTimeForRepeatableMirror(time, duration, m_RepeatCount, m_Mirror);
            }

            return time;
        }

        protected virtual void ValidateParameters() {
            if (m_StartParameterIndex >= 0 && m_StartParameter == null) {
                RavenLog.ErrorT(RavenSequence.Tag, "Parameter at index {0} does not exist for {1}! Ignoring.", m_StartParameterIndex, this);
            }
            if (m_EndParameterIndex >= 0 && m_EndParameter == null) {
                RavenLog.ErrorT(RavenSequence.Tag, "Parameter at index {0} does not exist for {1}! Ignoring.", m_EndParameterIndex, this);
            }
        }
    }
}