using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Starlite.Raven {

    public static class RavenConditionEditor {

        public static ReorderableList InitReorderableList(List<RavenCondition> conditions, List<RavenParameter> parameters) {
            ReorderableList conditionList = new ReorderableList(conditions, typeof(RavenCondition), false, true, true, true);
            foreach (RavenCondition c in conditions) {
                if (c.m_ParameterIndex == -1) {
                    c.m_Parameter = null;
                } else {
                    c.m_Parameter = parameters[c.m_ParameterIndex];
                }

                if (c.m_ValueIndex == -1) {
                    c.m_ValueParameter = null;
                } else {
                    c.m_ValueParameter = parameters[c.m_ValueIndex];
                }
            }
            conditionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                RavenCondition condition = conditions[index];

                string[] parameterNames = RavenParameterEditor.GetParameterNames(parameters, null, false);
                string[] parameterNamesWithNone = RavenParameterEditor.GetParameterNames(parameters, null, true);

                float split = (condition.m_ValueIndex == -1) ? 4f : 3f;
                int splitIndex = 0;
                Rect r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);

                int i;
                if (RavenParameterEditor.DrawNonSerializedParameter(r, condition.m_ParameterIndex, parameterNames, out i)) {
                    condition.m_ParameterIndex = i;
                    condition.m_Parameter = parameters[condition.m_ParameterIndex];
                }

                splitIndex++;
                r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);

                condition.m_ConditionMode = (ERavenConditionMode)EditorGUI.EnumPopup(r, condition.m_ConditionMode);

                splitIndex++;
                r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);

                if (RavenParameterEditor.DrawNonSerializedParameter(r, condition.m_ValueIndex + 1, parameterNamesWithNone, out i)) {
                    if (i == 0) {
                        condition.m_ValueIndex = -1;
                        condition.m_ValueParameter = null;
                    } else {
                        condition.m_ValueIndex = i - 1;
                        condition.m_ValueParameter = parameters[condition.m_ValueIndex];
                    }
                }

                splitIndex++;
                r = new Rect(rect.x + (splitIndex * rect.width / split), rect.y, rect.width / split, rect.height);
                if (condition.m_ValueParameter == null) {
                    DrawConditionValue(r, condition);
                }
            };
            conditionList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Conditions");
            };

            return conditionList;
        }

        public static void DrawConditionValue(Rect rect, RavenCondition condition) {
            if (condition.m_Parameter == null) {
                return;
            }
            switch (condition.m_Parameter.m_ParameterType) {
                case ERavenParameterType.Bool:
                    condition.m_ValueInt = EditorGUI.Toggle(rect, condition.m_ValueInt != 0) ? 1 : 0;
                    break;

                case ERavenParameterType.Float:
                    condition.m_ValueFloat = EditorGUI.FloatField(rect, condition.m_ValueFloat);
                    break;

                case ERavenParameterType.Enum:
                    //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;

                case ERavenParameterType.Int:
                    condition.m_ValueInt = EditorGUI.IntField(rect, condition.m_ValueInt);
                    break;

                case ERavenParameterType.BoolTrigger:
                    condition.m_ValueInt = EditorGUI.Toggle(rect, condition.m_ValueInt != 0) ? 1 : 0;
                    break;

                case ERavenParameterType.IntTrigger:
                    condition.m_ValueInt = EditorGUI.IntField(rect, condition.m_ValueInt);
                    break;

                case ERavenParameterType.EnumTrigger:
                    //condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;

                case ERavenParameterType.EnumBitMask:
                    //condition.ValueInt = SelectEnumMaskGUI(condition.Parameter, condition.ValueInt);
                    break;
            }
        }
    }
}