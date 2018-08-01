using System;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataBase<T> {
        protected RavenAnimationPropertyBase<T> m_Property;

        public abstract bool ShouldSyncStartingValues {
            get;
        }

        public void OnEnter() {
            OnEnterCallback();
        }

        public void OnExit() {
            OnExitCallback();
        }

        public void TrySyncStartingValues(T values) {
            if (ShouldSyncStartingValues) {
                SyncStartingValues(values);
            }
        }

        public override void Initialize(RavenSequence sequence, RavenAnimationPropertyComponentBase property) {
            m_Property = property as RavenAnimationPropertyBase<T>;
        }

        public sealed override Type GetAnimationDataType() {
            return typeof(T);
        }

        public virtual void PostprocessFinalValue(T value, UnityEngine.Object targetObject) {
        }

        public abstract T EvaluateAtTime(double time, double duration);

        protected abstract void OnEnterCallback();

        protected abstract void OnExitCallback();

        protected abstract void SyncStartingValues(T values);

        protected abstract T GetValueFromParameterCallback(RavenParameter parameter);

        protected T GetValueFromParameter(RavenParameter parameter) {
            if (parameter.m_ParameterType == ERavenParameterType.Object && IsObjectValueComponent(parameter.m_ValueObject)) {
                return m_Property.GetValue(parameter.m_ValueObject);
            }

            return GetValueFromParameterCallback(parameter);
        }

        protected double GetNormalizedTime(double time, double duration) {
            return time / duration;
        }

        protected double GetNormalizedTime(double time, double duration, ERavenEaseType easeType) {
            var value = RavenEaseUtility.Evaluate(easeType, time, duration, 1, 1);
            return value;
        }

        protected double GetNormalizedTime(double time, double duration, ERavenEaseType easeType, double amplitude, double period) {
            var value = RavenEaseUtility.Evaluate(easeType, time, duration, amplitude, period);
            return value;
        }

        protected double GetTimeForRepeatableMirror(double time, double duration, int repeatCount, bool mirror) {
            var repeatInterval = duration / repeatCount;
            var repeatIndexTime = time / repeatInterval;
            var repeatIndex = (int)repeatIndexTime;
            var repeatIndexRemainder = repeatIndexTime - repeatIndex;
            var newTime = repeatIndexRemainder * duration;
            var flip = mirror ? repeatIndex % 2 == 1 : repeatIndexRemainder == 0 && time != 0;   // mad logic -- last bool check handles case where duration == time because it would return 0 otherwise
            return flip ? duration - newTime : newTime;
        }

        protected bool IsObjectValueComponent(UnityEngine.Object obj) {
            return obj != null && (obj is UnityEngine.GameObject || obj is UnityEngine.Component);
        }
    }
}