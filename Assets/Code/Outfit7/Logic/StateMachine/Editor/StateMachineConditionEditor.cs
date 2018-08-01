using UnityEngine;
using System.Collections;
using Outfit7.Logic.StateMachineInternal;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEditor;

namespace Outfit7.Logic {

    public static class StateMachineConditionEditor {
        public static ReorderableList InitReorderableList(List<Condition> conditions, List<Parameter> parameters) {
            ReorderableList conditionList = new ReorderableList(conditions, typeof(Condition), false, true, true, true);
            foreach (Condition c in conditions) {
                if (c.ParameterIndex == -1) {
                    c.Parameter = null;
                } else {
                    c.Parameter = parameters[c.ParameterIndex];
                }

                if (c.ValueIndex == -1) {
                    c.ValueParameter = null;
                } else {
                    c.ValueParameter = parameters[c.ValueIndex];
                }
            }
            conditionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                Condition condition = conditions[index];

                string[] parameterNames = StateMachineParameterEditor.GetParameterNames(parameters, null, false);
                string[] parameterNamesWithNone = StateMachineParameterEditor.GetParameterNames(parameters, null, true);

                float split = (condition.ValueIndex == -1) ? 4f : 3f;
                int splitIndex = 0;
                Rect r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);

                int i;
                if (StateMachineParameterEditor.DrawNonSerializedParameter(r, condition.ParameterIndex, parameterNames, out i)) {
                    condition.ParameterIndex = i;
                    condition.Parameter = parameters[condition.ParameterIndex];
                }

                splitIndex++;
                r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);

                condition.ConditionMode = (ConditionMode) EditorGUI.EnumPopup(r, condition.ConditionMode);


                splitIndex++;
                r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);

                if (StateMachineParameterEditor.DrawNonSerializedParameter(r, condition.ValueIndex + 1, parameterNamesWithNone, out i)) {
                    if (i == 0) {
                        condition.ValueIndex = -1;
                        condition.ValueParameter = null;
                    } else {
                        condition.ValueIndex = i - 1;
                        condition.ValueParameter = parameters[condition.ValueIndex];
                    }
                }

                splitIndex++;
                r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);
                if (condition.ValueParameter == null) {
                    DrawConditionValue(r, condition);
                }
            };
            conditionList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Conditions");
            };

            return conditionList;
        }

        public static void DrawConditionValue(Rect rect, Condition condition) {
            if (condition.Parameter == null) {
                return;
            }
            switch (condition.Parameter.ParameterType) {
                case ParameterType.Bool:
                    condition.ValueInt = EditorGUI.Toggle(rect, condition.ValueInt != 0) ? 1 : 0;
                    break;
                case ParameterType.Float:
                    condition.ValueFloat = EditorGUI.FloatField(rect, condition.ValueFloat);
                    break;
                case ParameterType.Enum:
                //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.Int:
                    condition.ValueInt = EditorGUI.IntField(rect, condition.ValueInt);
                    break;
                case ParameterType.BoolTrigger:
                    condition.ValueInt = EditorGUI.Toggle(rect, condition.ValueInt != 0) ? 1 : 0;
                    break;
                case ParameterType.IntTrigger:
                    condition.ValueInt = EditorGUI.IntField(rect, condition.ValueInt);
                    break;
                case ParameterType.EnumTrigger:
                //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.EnumBitMask:
                //condition.ValueInt = SelectEnumMaskGUI(condition.Parameter, condition.ValueInt);
                    break;
            }
        }
    }

}