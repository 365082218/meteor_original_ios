using Starlite.Raven.Internal;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataTween<T> {
#if UNITY_EDITOR

        public ERavenEaseType EaseType {
            get {
                return m_EaseType;
            }
            set {
                m_EaseType = value;
            }
        }

        public double EaseAmplitude {
            get {
                return m_EaseAmplitude;
            }
            set {
                m_EaseAmplitude = value;
            }
        }

        public double EasePeriod {
            get {
                return m_EasePeriod;
            }
            set {
                m_EasePeriod = value;
            }
        }

        public bool UseCustomEaseCurve {
            get {
                return m_UseCustomEaseCurve;
            }
            set {
                m_UseCustomEaseCurve = value;
            }
        }

        public AnimationCurve CustomCurve {
            get {
                return m_EaseCurve;
            }
            set {
                m_EaseCurve = value;
            }
        }

        public int RepeatCount {
            get {
                return m_RepeatCount;
            }
        }

        public bool Mirror {
            get {
                return m_Mirror;
            }
        }

        public T GetStartValueEditor(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            return GetVaryingValueEditorInternal(m_StartValueStart, m_StartValueEnd, m_StartValueType, ref m_StartParameterIndex, m_StartValueIsObjectLink, m_StartValueObjectLink, m_Interpolator, sequence);
        }

        public T GetEndValueEditor(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            return GetVaryingValueEditorInternal(m_EndValueStart, m_EndValueEnd, m_EndValueType, ref m_EndParameterIndex, m_EndValueIsObjectLink, m_EndValueObjectLink, m_Interpolator, sequence);
        }

        public Vector2 GetMinMax(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            if (m_Interpolator == null) {
                m_Interpolator = RavenInterpolatorOverseer.GetInterpolator<T>();
            }
            var start = GetStartValueEditor(sequence, property);
            var end = GetEndValueEditor(sequence, property);
            return new Vector2((float)m_Interpolator.MinValue(start, end), (float)m_Interpolator.MaxValue(start, end));
        }

        protected override void SetStartingValues(T values) {
            m_StartValueStart = values;
            m_StartValueEnd = values;
            m_EndValueStart = values;
            m_EndValueEnd = values;
        }

        protected override void CopyValuesCallback(RavenAnimationDataComponentBase other) {
            var otherReal = other as RavenAnimationDataTween<T>;

            m_Mirror = otherReal.m_Mirror;
            m_EaseAmplitude = otherReal.m_EaseAmplitude;
            m_EaseCurve = new AnimationCurve(otherReal.m_EaseCurve.keys);
            m_EasePeriod = otherReal.m_EasePeriod;
            m_EaseType = otherReal.m_EaseType;
            m_EndParameter = otherReal.m_EndParameter;
            m_EndParameterIndex = otherReal.m_EndParameterIndex;
            m_EndValueEnd = otherReal.m_EndValueEnd;
            m_EndValueStart = otherReal.m_EndValueStart;
            m_EndValueType = otherReal.m_EndValueType;
            m_EndValueIsObjectLink = otherReal.m_EndValueIsObjectLink;
            m_EndValueObjectLink = otherReal.m_EndValueObjectLink;
            m_RepeatCount = otherReal.m_RepeatCount;
            m_StartParameterIndex = otherReal.m_StartParameterIndex;
            m_StartValueEnd = otherReal.m_StartValueEnd;
            m_StartValueStart = otherReal.m_StartValueStart;
            m_StartValueType = otherReal.m_StartValueType;
            m_StartValueIsObjectLink = otherReal.m_StartValueIsObjectLink;
            m_StartValueObjectLink = otherReal.m_StartValueObjectLink;
            m_UseCustomEaseCurve = otherReal.m_UseCustomEaseCurve;
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