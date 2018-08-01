#if UNITY_5_6_OR_NEWER
using Starlite.Raven.Compiler;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenParticleSystemMainPropertyBase<T> : RavenAnimationPropertyBase<T> {
        private RavenTriggerPropertyBase2<T, ParticleSystem.MainModule> m_TriggerPropertyCast;

        public sealed override bool IsCustom {
            get {
                return true;
            }
        }

        public sealed override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);

            m_TriggerPropertyCast = m_TriggerProperty as RavenTriggerPropertyBase2<T, ParticleSystem.MainModule>;
        }

        protected sealed override void PostEvaluateAtTime(double time, double duration, T value, UnityEngine.Object targetComponent) {
            if (m_TriggerPropertyCast != null) {
                if (!m_ExecuteFunctionOnly) {
                    SetValue(value, targetComponent);
                }
                m_TriggerPropertyCast.ManualExecute(value, GetMainModule(targetComponent));
            } else {
                SetValue(value, targetComponent);
            }
        }

        protected sealed override bool IsCustomValid() {
            return m_TargetComponent is ParticleSystem;
        }

        protected ParticleSystem.MainModule GetMainModule(UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return new ParticleSystem.MainModule();
            }
            return (targetComponent as ParticleSystem).main;
        }
    }
}
#endif