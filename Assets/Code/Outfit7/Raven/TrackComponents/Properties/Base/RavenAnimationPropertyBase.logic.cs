using System;
using System.Collections.Generic;
using UnityEngine;

namespace Starlite.Raven {

    public abstract partial class RavenAnimationPropertyBase<T> {
        protected RavenAnimationDataBase<T> m_AnimationDataCast;
        protected RavenValueInterpolatorBase<T> m_Interpolator;

        private T m_EnterValue;
        private T[] m_OverridenEnterValues;

        public sealed override int ParameterIndex {
            get {
                return -1;
            }
        }

        public override void Initialize(RavenSequence sequence) {
            base.Initialize(sequence);
            m_AnimationDataCast = m_AnimationData as RavenAnimationDataBase<T>;
            m_Interpolator = RavenInterpolatorOverseer.GetInterpolator<T>();
        }

        public override void OnEnter() {
            if (HasOverridenTargetComponents()) {
                for (int i = 0; i < m_OverridenTargetComponents.Count; ++i) {
                    m_OverridenEnterValues[i] = GetValue(m_OverridenTargetComponents[i]);
                }
            } else {
                m_EnterValue = GetValue(m_TargetComponent);
                m_AnimationDataCast.TrySyncStartingValues(m_EnterValue);
            }
            m_AnimationDataCast.OnEnter();
        }

        public sealed override void EvaluateAtTime(double time, double duration) {
            var hasOverridenTargetComponents = HasOverridenTargetComponents();
            var iterationCount = hasOverridenTargetComponents ? m_OverridenTargetComponents.Count : 1;

            for (int i = 0; i < iterationCount; ++i) {
                T enterValue;
                UnityEngine.Object targetComponent;
                if (hasOverridenTargetComponents) {
                    // have to do this because animation data holds only 1 data and objects might have different starting data points
                    m_AnimationDataCast.TrySyncStartingValues(m_OverridenEnterValues[i]);
                    enterValue = m_OverridenEnterValues[i];
                    targetComponent = m_OverridenTargetComponents[i];
                } else {
                    enterValue = m_EnterValue;
                    targetComponent = m_TargetComponent;
                }

                var value = m_AnimationDataCast.EvaluateAtTime(time, duration);
                if (m_ApplyMultiplier) {
                    value = m_Interpolator.MultiplyScalar(value, m_Multiplier);
                }
                if (m_ApplyOffset) {
                    value = m_Interpolator.Add(value, m_Offset);
                }

                switch (m_PropertyType) {
                    case ERavenAnimationPropertyType.Set:
                        value = ProcessValueComponents(value, targetComponent);
                        break;

                    case ERavenAnimationPropertyType.Add:
                        value = m_Interpolator.Add(enterValue, value);
                        break;

                    case ERavenAnimationPropertyType.RelativeAdd:
                        value = m_Interpolator.Add(GetValue(targetComponent), value);
                        break;

                    case ERavenAnimationPropertyType.Multiply:
                        value = m_Interpolator.Multiply(enterValue, value);
                        break;

                    case ERavenAnimationPropertyType.RelativeMultiply:
                        value = m_Interpolator.Multiply(GetValue(targetComponent), value);
                        break;

                    default:
                        throw new Exception(string.Format("{0} not handled", m_PropertyType));
                }

                m_AnimationDataCast.PostprocessFinalValue(value, targetComponent);
                // This should not modify the value any further!
                PostEvaluateAtTime(time, duration, value, targetComponent);
            }
        }

        public override void OnExit() {
            m_AnimationDataCast.OnExit();
        }

        public sealed override Type GetPropertyType() {
            return typeof(T);
        }

        public abstract T GetValue(UnityEngine.Object targetComponent);

        protected abstract void SetValue(T value, UnityEngine.Object targetComponent);

        protected abstract void PostEvaluateAtTime(double time, double duration, T value, UnityEngine.Object targetComponent);

        protected virtual T ProcessValueComponents(T value, UnityEngine.Object targetComponent) {
            return value;
        }

        protected override void OnSetTargets(List<GameObject> gameObjects) {
            base.OnSetTargets(gameObjects);
            if (m_OverridenEnterValues == null || m_OverridenEnterValues.Length != gameObjects.Count) {
                var tmp = m_OverridenEnterValues;
                m_OverridenEnterValues = new T[gameObjects.Count];
                if (tmp != null) {
                    Array.Copy(tmp, m_OverridenEnterValues, Math.Min(tmp.Length, m_OverridenEnterValues.Length));
                }
            }
        }
    }
}