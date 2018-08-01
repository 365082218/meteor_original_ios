using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataCurve<T> {
#if UNITY_EDITOR

        public AnimationCurve[] Curves {
            get {
                return m_Curves;
            }
        }

        public ERavenEaseType EaseType {
            get {
                return m_EaseType;
            }
            set {
                m_EaseType = value;
            }
        }

        public bool SyncToCurrent {
            get {
                return m_ValueType == ERavenValueType.Current;
            }
            set {
                if (value) {
                    m_ValueType = ERavenValueType.Current;
                } else {
                    m_ValueType = ERavenValueType.Constant;
                }
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

        public Vector2 GetMinMax() {
            float min = Mathf.Infinity;
            float max = Mathf.NegativeInfinity;

            for (int i = 0; i < m_Curves.Length; i++) {
                var curve = m_Curves[i];
                for (int j = 0; j < curve.keys.Length; j++) {
                    var key = curve.keys[j];
                    if (key.value > max) {
                        max = key.value;
                    }
                    if (key.value < min) {
                        min = key.value;
                    }
                }
            }
            return new Vector2(min, max);
        }

        public virtual bool UniformCurves {
            get {
                return false;
            }
            set {
            }
        }

        protected override void CopyValuesCallback(RavenAnimationDataComponentBase other) {
            var otherReal = other as RavenAnimationDataCurve<T>;
            m_Curves = new AnimationCurve[otherReal.m_Curves.Length];
            for (int i = 0; i < otherReal.m_Curves.Length; ++i) {
                m_Curves[i] = new AnimationCurve(otherReal.m_Curves[i].keys);
            }

            m_EaseType = otherReal.m_EaseType;
            m_ValueType = otherReal.m_ValueType;
            m_RepeatCount = otherReal.m_RepeatCount;
            m_Mirror = otherReal.m_Mirror;
        }

        public override bool CheckForDependencies() {
            return true;
        }

#endif
    }
}