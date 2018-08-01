using System;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Logic.StateMachineInternal {

    [Serializable]
    public class Condition {
        // Serialized
        public int ParameterIndex = -1;
        public float ValueFloat;
        public int ValueInt;
        public int ValueIndex = -1;
        public ConditionMode ConditionMode;
        // Internal
        [NonSerialized] public Parameter Parameter;
        [NonSerialized] public Parameter ValueParameter;

        public void ResetTrigger() {
            if (Parameter.ParameterType != ParameterType.BoolTrigger && Parameter.ParameterType != ParameterType.IntTrigger && Parameter.ParameterType != ParameterType.EnumTrigger) {
                return;
            }
            Parameter.ResetTrigger();
        }

        public bool IsTrue() {
            if (Parameter.ParameterType == ParameterType.Float) {
                float value = ValueParameter != null ? ValueParameter.ValueFloat : ValueFloat;
                switch (ConditionMode) {
                    case ConditionMode.Less:
                        return Parameter.ValueFloat < value;
                    case ConditionMode.LessOrEqual:
                        return Parameter.ValueFloat <= value;
                    case ConditionMode.Equal:
                        return Mathf.Abs(Parameter.ValueFloat - value) < 0.0001f;
                    case ConditionMode.GreaterOrEqual:
                        return Parameter.ValueFloat >= value;
                    case ConditionMode.Greater:
                        return Parameter.ValueFloat > value;
                    case ConditionMode.NotEqual:
                        return Mathf.Abs(Parameter.ValueFloat - value) >= 0.0001f;
                    case ConditionMode.BitSet:
                        throw new System.Exception("BitSet cannot be used with float parameter!");
                    case ConditionMode.BitNotSet:
                        throw new System.Exception("BitNotSet cannot be used with float parameter!");
                    case ConditionMode.BitMaskSet:
                        throw new System.Exception("BitMaskSet cannot be used with float parameter!");
                    case ConditionMode.BitMaskNotSet:
                        throw new System.Exception("BitMaskNotSet cannot be used with float parameter!");
                }
            } else if (Parameter.ParameterType == ParameterType.Component ||
                       Parameter.ParameterType == ParameterType.ComponentList ||
                       Parameter.ParameterType == ParameterType.Vector4) {
                Assert.IsTrue(true, "Can't compare Vector4 or Component parameters");
            } else {
                int value = ValueParameter != null ? ValueParameter.ValueInt : ValueInt;
                switch (ConditionMode) {
                    case ConditionMode.Less:
                        return Parameter.ValueInt < value;
                    case ConditionMode.LessOrEqual:
                        return Parameter.ValueInt <= value;
                    case ConditionMode.Equal:
                        return Parameter.ValueInt == value;
                    case ConditionMode.GreaterOrEqual:
                        return Parameter.ValueInt >= value;
                    case ConditionMode.Greater:
                        return Parameter.ValueInt > value;
                    case ConditionMode.NotEqual:
                        return Parameter.ValueInt != value;
                    case ConditionMode.BitSet:
                        return (Parameter.ValueInt & (1 << value)) > 0;
                    case ConditionMode.BitNotSet:
                        return (Parameter.ValueInt & (1 << value)) == 0;
                    case ConditionMode.BitMaskSet:
                        return (Parameter.ValueInt & value) == value;
                    case ConditionMode.BitMaskNotSet:
                        return (Parameter.ValueInt & value) == 0;
                }
            }
            return false;
        }

    }

}