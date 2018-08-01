using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Sequencer {
    public class SequencerParameterProperty : BaseProperty {
        public int ParameterIndex = -1;
        public string ParameterName;
        public ParameterType ParameterType;

        public override int GetNumberOfValuesUsed() {
            return 1;
        }

        public override void OnApply(Component component, Vector4 value) {
            SequencerSequence sequence = component as SequencerSequence;
            if (sequence == null)
                return;
            int index = sequence.FindParameterIndex(ParameterName);
            if (index == -1)
                return;
            switch (ParameterType) {
                case ParameterType.Bool:
                    sequence.SetBoolParameter(index, ((int) value.x) == 0 ? false : true);
                    break;
                case ParameterType.Float:
                    sequence.SetFloatParameter(index, value.x);
                    break;
                case ParameterType.Enum:
                    //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.Int:
                    sequence.SetIntParameter(index, (int) value.x);
                    break;
                case ParameterType.BoolTrigger:
                    sequence.SetBoolParameter(index, ((int) value.x) == 0 ? false : true);
                    //stateMachine.SetBoolTriggerParameter(index);
                    break;
                case ParameterType.IntTrigger:
                    sequence.SetIntParameter(index, ((int) value.x));
                    break;
                case ParameterType.EnumTrigger:
                    //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.EnumBitMask:
                    //condition.ValueInt = SelectEnumMaskGUI(condition.Parameter, condition.ValueInt);
                    break;
            }
        }

        public override Vector4 OnValue(Component component, out bool success) {
            SequencerSequence sequence = component as SequencerSequence;
            if (sequence == null) {
                success = false;
                return Vector4.zero;
            }
            int index = sequence.FindParameterIndex(ParameterName);
            if (index == -1) {
                success = false;
                return Vector4.zero;
            }

            success = true;
            switch (ParameterType) {
                case ParameterType.Bool:
                    return new Vector4(sequence.GetBoolParameter(index) ? 1 : 0, 0, 0, 0);
                case ParameterType.Float:
                    return new Vector4(sequence.GetFloatParameter(index), 0, 0, 0);
                case ParameterType.Enum:
                    //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.Int:
                    return new Vector4((float) sequence.GetIntParameter(index), 0, 0, 0);
                case ParameterType.BoolTrigger:
                    return new Vector4(sequence.GetBoolParameter(index) ? 1 : 0, 0, 0, 0);
                case ParameterType.IntTrigger:
                    return new Vector4((float) sequence.GetIntParameter(index), 0, 0, 0);
                case ParameterType.EnumTrigger:
                    //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.EnumBitMask:
                    //condition.ValueInt = SelectEnumMaskGUI(condition.Parameter, condition.ValueInt);
                    break;
                default:
                    success = false;
                    return Vector4.zero;
            }
            success = false;
            return Vector4.zero;
        }
    }
}