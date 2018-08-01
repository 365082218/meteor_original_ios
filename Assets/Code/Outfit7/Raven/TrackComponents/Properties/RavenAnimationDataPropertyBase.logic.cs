using Starlite.Raven.Compiler;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataPropertyBase<T> {
        private RavenPropertyGetter<T> e_SyncCallback;
        private RavenPropertySetter<T> e_ExecuteCallback;
        private RavenTriggerPropertyBase1<T> m_TriggerPropertyCast;

        public sealed override bool IsCustom {
            get {
                return false;
            }
        }

        public sealed override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_TriggerPropertyCast = m_TriggerProperty as RavenTriggerPropertyBase1<T>;
            PropertyReflectionCompiledOutput.ConfigureRavenAnimationProperty<T>(this);
        }

        public void SetOnExecuteCallback(RavenPropertySetter<T> callback) {
            e_ExecuteCallback = callback;
        }

        public void SetOnSyncCallback(RavenPropertyGetter<T> callback) {
            e_SyncCallback = callback;
        }

        public sealed override T GetValue(UnityEngine.Object targetComponent) {
            return e_SyncCallback(targetComponent);
        }

        protected sealed override void SetValue(T value, UnityEngine.Object targetComponent) {
            e_ExecuteCallback(targetComponent, value);
        }

        protected sealed override void PostEvaluateAtTime(double time, double duration, T value, UnityEngine.Object targetComponent) {
            if (m_TriggerPropertyCast != null) {
                if (!m_ExecuteFunctionOnly) {
                    SetValue(value, targetComponent);
                }
                m_TriggerPropertyCast.ManualExecute(value);
            } else {
                SetValue(value, targetComponent);
            }
        }

        protected sealed override bool IsCustomValid() {
            return false;
        }
    }
}