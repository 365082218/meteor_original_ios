using Starlite.Raven.Compiler;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenParticleSystemEMPropertyBase<T> : RavenAnimationPropertyBase<T> {
        private RavenTriggerPropertyBase2<T, ParticleSystem.EmissionModule> m_TriggerPropertyCast;

        public sealed override bool IsCustom {
            get {
                return true;
            }
        }

        public sealed override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_TriggerPropertyCast = m_TriggerProperty as RavenTriggerPropertyBase2<T, ParticleSystem.EmissionModule>;
        }

        protected sealed override void PostEvaluateAtTime(double time, double duration, T value, UnityEngine.Object targetComponent) {
            if (m_TriggerPropertyCast != null) {
                if (!m_ExecuteFunctionOnly) {
                    SetValue(value, targetComponent);
                }
                m_TriggerPropertyCast.ManualExecute(value, GetEmissionModule(targetComponent));
            } else {
                SetValue(value, targetComponent);
            }
        }

        protected sealed override bool IsCustomValid() {
            return m_TargetComponent is ParticleSystem;
        }

        protected ParticleSystem.EmissionModule GetEmissionModule(UnityEngine.Object targetComponent) {
            if (targetComponent == null) {
                return new ParticleSystem.EmissionModule();
            }
            return (targetComponent as ParticleSystem).emission;
        }
    }
}