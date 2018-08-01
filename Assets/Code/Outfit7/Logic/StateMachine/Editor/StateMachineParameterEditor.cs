using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Logic.StateMachineInternal {

    public static class StateMachineParameterEditor {
        public static ReorderableList InitReorderableList(List<Parameter> parameters) {
            ReorderableList parametersList = new ReorderableList(parameters, typeof(Parameter), true, true, true, true);
            List<ReorderableList> componentReorderableList = new List<ReorderableList>();
            ReconstructComponentReorderableList(componentReorderableList, parameters);

            parametersList.elementHeightCallback = (index) => {
                Parameter param = parameters[index];
                if (param.ParameterType == ParameterType.ComponentList) {
                    return param.ValueComponentList.Count * 20 + 60;
                }
                return 20;
            };
            parametersList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                Parameter param = parameters[index];
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 20, rect.height), param.ParameterIndex + ".");
                param.Name = EditorGUI.TextField(new Rect(rect.x + 20, rect.y, rect.width - 290, rect.height), param.Name);
                param.ParameterType = (ParameterType) EditorGUI.EnumPopup(new Rect(rect.xMax - 270, rect.y, 80f, rect.height), param.ParameterType);
                param.ParameterIndex = index;
                switch (param.ParameterType) {
                    case ParameterType.Float:
                        param.ValueFloat = EditorGUI.FloatField(new Rect(rect.xMax - 190.0f, rect.y, 190.0f, rect.height), param.ValueFloat);
                        break;
                    case ParameterType.Int:
                        param.ValueInt = EditorGUI.IntField(new Rect(rect.xMax - 190.0f, rect.y, 190.0f, rect.height), param.ValueInt);
                        break;
                    case ParameterType.Bool:
                    case ParameterType.BoolTrigger:
                        param.ValueInt = EditorGUI.Toggle(new Rect(rect.xMax - 190.0f, rect.y, 190.0f, rect.height), param.ValueInt == 1) ? 1 : 0;
                        break;
                    case ParameterType.Component:
                        param.ValueComponent = (Component) EditorGUI.ObjectField(new Rect(rect.xMax - 190.0f, rect.y, 190.0f, rect.height), param.ValueComponent, typeof(Component), true);
                        break;
                    case ParameterType.ComponentList:
                        if (index >= componentReorderableList.Count)
                            break;
                        if (componentReorderableList[index] == null)
                            break;
                        componentReorderableList[index].DoList(new Rect(rect.xMax - 190.0f, rect.y, 190.0f, rect.height));
                        break;
                    case ParameterType.Vector4:
                        param.ValueVector = EditorGUI.Vector4Field(new Rect(rect.xMax - 190.0f, rect.y - 15f, 190.0f, rect.height), "", param.ValueVector);
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

        public static void ReconstructComponentReorderableList(List<ReorderableList> componentReorderableList, List<Parameter> parameters) {
            componentReorderableList.Clear();
            for (int i = 0; i < parameters.Count; i++) {
                Parameter p = parameters[i];
                ReorderableList componentsList = new ReorderableList(p.ValueComponentList, typeof(Component), false, false, true, true);
                componentsList.drawElementCallback = (Rect compRect, int compIndex, bool compIsActive, bool compIsFocused) => {
                    if (compIndex < 0 || compIndex > componentsList.list.Count)
                        return;
                    p.ValueComponentList[compIndex] = (Component) EditorGUI.ObjectField(new Rect(compRect.x + 5f, compRect.y, compRect.width - 5f, 15f), p.ValueComponentList[compIndex], typeof(Component), true);  
                };
                componentReorderableList.Add(componentsList);
            }
        }

        public static bool DrawParameter(Rect rect, Parameter parameter, string[] parameterNames, out int index, int selected = -1) {
            string parameterName = parameter != null ? parameter.Name : string.Empty;
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

        public static string[] GetParameterNames(List<Parameter> parameters, System.Predicate<Parameter> match, bool canBeNone) {
            string[] names = new string[canBeNone ? 1 : 0];
            if (canBeNone) {
                names[0] = "None";
            }

            for (int i = 0; i < parameters.Count; i++) {
                if (match == null || match.Invoke(parameters[i])) {
                    ArrayUtility.Add(ref names, parameters[i].Name);
                }
            }
            return names;
        }

        public static string[] GetParameterNames(List<Parameter> parameters, System.Predicate<Parameter> match, string[] additionalNames) {
            string[] names = additionalNames;
            for (int i = 0; i < parameters.Count; i++) {
                if (match == null || match.Invoke(parameters[i])) {
                    ArrayUtility.Add(ref names, parameters[i].Name);
                }
            }
            return names;
        }
    }

}