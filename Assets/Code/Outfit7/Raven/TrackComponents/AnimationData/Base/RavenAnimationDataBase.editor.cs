#if UNITY_EDITOR
using Starlite.Raven.Internal;
using System;
using UnityEditor;
#endif

namespace Starlite.Raven {

    public abstract partial class RavenAnimationDataBase<T> {
#if UNITY_EDITOR

        public sealed override void SetStartingValuesEditor(object values) {
            Undo.RecordObject(this, "SetStartingValues");
            SetStartingValues(values == null ? default(T) : (T)values);
        }

        public double GetTimeForRepeatableMirrorEditor(double time, double duration, int repeatCount, bool mirror) {
            return GetTimeForRepeatableMirror(time, duration, repeatCount, mirror);
        }

        protected abstract void SetStartingValues(T values);

        protected T GetVaryingValueEditorInternal(T startValue, T endValue, ERavenValueType valueType, ref int parameterIndex, bool isObjectLink, UnityEngine.Object objectLink, RavenValueInterpolatorBase<T> interpolator, RavenSequence sequence) {
            var param = sequence.GetParameterAtIndex(parameterIndex);
            if (param == null && parameterIndex >= 0) {
                RavenLog.Error("Parameter index {0} set for {1} but there is no parameter in sequence at that index! Clearing the value.", parameterIndex, this);
                parameterIndex = -1;
            } else if (param != null) {
                try {
                    return GetValueFromParameter(param);
                } catch (Exception e) {
                    RavenLog.Error(e, "Failed getting value from parameter! Check if parameter component type ({0}) matches the required type {1}!", param.m_ValueObject == null ? "null" : param.m_ValueObject.GetType().ToString(), m_Property.TargetComponent.GetType().ToString());
                }
            }

            if (isObjectLink) {
                if (objectLink != null) {
                    return m_Property.GetValue(objectLink);
                }
                return default(T);
            }

            switch (valueType) {
                case ERavenValueType.Constant:
                    return startValue;

                case ERavenValueType.Current:
                    var value = m_Property.GetValueEditor(sequence);
                    return value == null ? default(T) : (T)value;

                case ERavenValueType.Range:
                    return interpolator.MultiplyScalar(interpolator.Add(startValue, endValue), 0.5);
            }
            return startValue;
        }

#endif
    }
}