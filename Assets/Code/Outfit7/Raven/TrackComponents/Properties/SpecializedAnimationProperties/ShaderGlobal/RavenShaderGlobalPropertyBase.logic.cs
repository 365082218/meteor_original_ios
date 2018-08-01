using Starlite.Raven.Compiler;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenShaderGlobalPropertyBase<T> {
        protected int m_TargetShaderPropertyId;

        private RavenTriggerPropertyBase2<T, string> m_TriggerPropertyCast;

        public sealed override bool IsCustom {
            get {
                return true;
            }
        }

        public override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_TriggerPropertyCast = m_TriggerProperty as RavenTriggerPropertyBase2<T, string>;
            m_TargetShaderPropertyId = Shader.PropertyToID(m_TargetShaderProperty);
        }

        public sealed override T GetValue(UnityEngine.Object targetComponent) {
            return default(T);
        }

        protected override void PostEvaluateAtTime(double time, double duration, T value, UnityEngine.Object targetComponent) {
#if UNITY_EDITOR
            m_TargetShaderPropertyId = Shader.PropertyToID(m_TargetShaderProperty);
#endif
            if (m_TriggerPropertyCast != null) {
                if (!m_ExecuteFunctionOnly) {
                    SetValue(value, targetComponent);
                }
                m_TriggerPropertyCast.ManualExecute(value, m_TargetShaderProperty);
            } else {
                SetValue(value, targetComponent);
            }
        }

        protected override bool IsCustomValid() {
            return true;
        }
    }
}