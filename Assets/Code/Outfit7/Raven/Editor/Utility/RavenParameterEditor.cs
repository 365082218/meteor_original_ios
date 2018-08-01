using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Starlite.Raven {

    public static class RavenParameterEditor {

        private static MethodInfo GradientDraw;

        public static ReorderableList InitReorderableList(List<RavenParameter> parameters) {
            GradientDraw = typeof(EditorGUI).GetMethod("GradientField", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(string), typeof(Rect), typeof(Gradient) }, null);

            ReorderableList parametersList = new ReorderableList(parameters, typeof(RavenParameter), true, true, true, true);
            List<ReorderableList> componentReorderableList = new List<ReorderableList>();
            ReconstructComponentReorderableList(componentReorderableList, parameters);

            parametersList.elementHeightCallback = (index) => {
                RavenParameter param = parameters[index];
                if (param.m_ParameterType == ERavenParameterType.ActorList) {
                    return param.m_ValueGameObjectList.Count * 20 + 60;
                }
                return 20;
            };
            parametersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                RavenParameter param = parameters[index];
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 20, rect.height), param.m_ParameterIndex + ".");
                param.m_Name = EditorGUI.TextField(new Rect(rect.x + 20, rect.y, rect.width - 220, rect.height), param.m_Name);
                param.m_ParameterType = (ERavenParameterType)EditorGUI.EnumPopup(new Rect(rect.xMax - 200, rect.y, 60f, rect.height), param.m_ParameterType);
                param.m_ParameterIndex = index;
                switch (param.m_ParameterType) {
                    case ERavenParameterType.Float:
                        param.m_ValueFloat = EditorGUI.FloatField(new Rect(rect.xMax - 140f, rect.y, 140f, rect.height), param.m_ValueFloat);
                        break;

                    case ERavenParameterType.Int:
                        param.m_ValueInt = EditorGUI.IntField(new Rect(rect.xMax - 140f, rect.y, 140f, rect.height), param.m_ValueInt);
                        break;

                    case ERavenParameterType.Bool:
                    case ERavenParameterType.BoolTrigger:
                        param.m_ValueInt = EditorGUI.Toggle(new Rect(rect.xMax - 140f, rect.y, 140f, rect.height), param.m_ValueInt == 1) ? 1 : 0;
                        break;

                    case ERavenParameterType.Object:
                        param.m_ValueObject = (UnityEngine.Object)EditorGUI.ObjectField(new Rect(rect.xMax - 140f, rect.y, 140f, rect.height), param.m_ValueObject, typeof(UnityEngine.Object), true);
                        break;

                    case ERavenParameterType.ActorList:
                        if (index >= componentReorderableList.Count)
                            break;
                        if (componentReorderableList[index] == null)
                            break;
                        componentReorderableList[index].DoList(new Rect(rect.xMax - 140f, rect.y, 140f, rect.height));
                        break;

                    case ERavenParameterType.Vector4:
                        param.m_ValueVector = EditorGUI.Vector4Field(new Rect(rect.xMax - 140f, rect.y - 15f, 140f, rect.height), "", param.m_ValueVector);
                        break;

                    case ERavenParameterType.Gradient:
                        param.m_ValueGradient = (Gradient)GradientDraw.Invoke(null, new object[] { "", new Rect(rect.xMax - 140f, rect.y - 15f, 140f, rect.height), param.m_ValueGradient });
                        break;
                }
            };
            parametersList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Parameters");
            };
            parametersList.onRemoveCallback = (ReorderableList list) => {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                ReconstructComponentReorderableList(componentReorderableList, parameters);
            };
            parametersList.onAddCallback = (ReorderableList list) => {
                ReorderableList.defaultBehaviours.DoAddButton(list);
                ReconstructComponentReorderableList(componentReorderableList, parameters);
            };
            parametersList.onReorderCallback = (ReorderableList list) => {
                ReconstructComponentReorderableList(componentReorderableList, parameters);
            };

            return parametersList;
        }

        public static void ReconstructComponentReorderableList(List<ReorderableList> componentReorderableList, List<RavenParameter> parameters) {
            componentReorderableList.Clear();
            for (int i = 0; i < parameters.Count; i++) {
                RavenParameter p = parameters[i];
                ReorderableList componentsList = new ReorderableList(p.m_ValueGameObjectList, typeof(Component), false, false, true, true);
                componentsList.drawElementCallback = (Rect compRect, int compIndex, bool compIsActive, bool compIsFocused) => {
                    if (compIndex < 0 || compIndex > componentsList.list.Count)
                        return;
                    p.m_ValueGameObjectList[compIndex] = EditorGUI.ObjectField(new Rect(compRect.x + 5f, compRect.y, compRect.width - 5f, 15f), p.m_ValueGameObjectList[compIndex], typeof(GameObject), true) as GameObject;
                };
                componentsList.onAddCallback = x => {
                    x.list.Add(null);
                };
                componentReorderableList.Add(componentsList);
            }
        }

        public static bool DrawParameter(Rect rect, RavenParameter parameter, string[] parameterNames, out int index, int selected = -1) {
            string parameterName = parameter != null ? parameter.m_Name : string.Empty;
            int oldIndex = ArrayUtility.FindIndex(parameterNames, p => p == parameterName);
            if (oldIndex == -1 && selected != -1)
                oldIndex = selected;
            int newIndex = EditorGUI.Popup(rect, oldIndex == -1 ? 0 : oldIndex, parameterNames);
            if (oldIndex != newIndex) {
                index = newIndex;
                return true;
            }
            index = -1;
            return false;
        }

        public static bool DrawNonSerializedParameter(Rect rect, int oldIndex, string[] parameterNames, out int index, int selected = -1) {
            if (oldIndex == -1 && selected != -1)
                oldIndex = selected;
            int newIndex = EditorGUI.Popup(rect, oldIndex == -1 ? 0 : oldIndex, parameterNames);
            if (oldIndex != newIndex) {
                index = newIndex;
                return true;
            }
            index = -1;
            return false;
        }

        public static string[] GetParameterNames(List<RavenParameter> parameters, System.Predicate<RavenParameter> match, bool canBeNone) {
            string[] names = new string[canBeNone ? 1 : 0];
            if (canBeNone) {
                names[0] = "None";
            }

            for (int i = 0; i < parameters.Count; i++) {
                if (match == null || match.Invoke(parameters[i])) {
                    ArrayUtility.Add(ref names, parameters[i].m_Name);
                }
            }
            return names;
        }

        public static string[] GetParameterNames(List<RavenParameter> parameters, System.Predicate<RavenParameter> match, string[] additionalNames) {
            string[] names = additionalNames;
            for (int i = 0; i < parameters.Count; i++) {
                if (match == null || match.Invoke(parameters[i])) {
                    ArrayUtility.Add(ref names, parameters[i].m_Name);
                }
            }
            return names;
        }

        public static bool DrawParameterField(Rect rect, System.Predicate<RavenParameter> match, string[] predefinedValues, ref int parameterIndex, ref int selectedIndex) {
            var parameters = RavenSequenceEditor.Instance.Sequence.Parameters;
            var parameterNames = GetParameterNames(parameters, match, false);

            string[] names = new string[0];
            if (predefinedValues != null) {
                ArrayUtility.AddRange(ref names, predefinedValues);
            } else {
                ArrayUtility.Add(ref names, "None");
            }
            var predefinedValuesLength = names.Length;

            ArrayUtility.AddRange(ref names, parameterNames);
            int currentIndex;
            if (selectedIndex < 0) {
                currentIndex = 0;
            } else {
                currentIndex = parameterIndex < 0 ? selectedIndex : Array.IndexOf(names, parameters[parameterIndex].m_Name);
            }

            if (DrawParameter(rect, null, names, out currentIndex, currentIndex)) {
                selectedIndex = currentIndex;
                parameterIndex = currentIndex < predefinedValuesLength ? -1 : parameters.FindIndex(x => x.m_Name == names[currentIndex]);
                return true;
            }
            return false;
        }

        public static void DrawOverrideTargetsParameterFieldForTriggerProperty(RavenTriggerPropertyComponentBase triggerProperty, Rect rect, List<RavenParameter> parameters) {
            if (triggerProperty != null) {
                var selectedIndex = triggerProperty.ParameterIndex;
                int parameterIndex = selectedIndex;
                if (RavenParameterEditor.DrawParameterField(rect, x => x.m_ParameterType == ERavenParameterType.ActorList, null, ref parameterIndex, ref selectedIndex)) {
                    triggerProperty.SetParameterIndexEditor(parameterIndex);
                }
            }
        }
    }
}