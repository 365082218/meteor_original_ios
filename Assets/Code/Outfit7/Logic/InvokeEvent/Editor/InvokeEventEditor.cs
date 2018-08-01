using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Outfit7.Util;
using System.Linq;
using UnityEditor.SceneManagement;

namespace Outfit7.Logic {

    public class InvokeEventEditor : EditorWindow {

        protected class MethodMan {
            public MethodInfo Info = null;
            public string DisplayName;
            public string LinkName;
            public string SortKey;
        }

        // Internal
        private InvokeEvent ActiveEvent;
        private GameObject ActiveGameObject;
        private MonoBehaviour[] UserDefinedCallbackMonoBehaviours;

        private List<MethodMan> SortedMethods = null;
        private List<string> SortedMethodStrings = null;

        // Public
        public bool Initialize(GameObject gameObject, MonoBehaviour[] userDefinedCallbackMonoBehaviours, InvokeEvent activeEvent) {
            GetAllPublicNonInheritedMethods(gameObject, userDefinedCallbackMonoBehaviours);
            ActiveEvent = activeEvent;
            ActiveGameObject = gameObject;
            UserDefinedCallbackMonoBehaviours = userDefinedCallbackMonoBehaviours;
            return SortedMethods.Count > 0;
        }

        // Private
        private void GetAllPublicNonInheritedMethods(GameObject gameObject, MonoBehaviour[] userDefinedCallbackMonoBehaviours) {
            SortedMethods = new List<MethodMan>();
            MethodMan mmnone = new MethodMan();
            mmnone.LinkName = mmnone.DisplayName = "None";
            mmnone.SortKey = "__" + "None";
            ;
            SortedMethods.Add(mmnone);

            List<MonoBehaviour> monoBehaviours = new List<MonoBehaviour>(gameObject.GetComponents<MonoBehaviour>());
            // Add user defined
            if (userDefinedCallbackMonoBehaviours != null) {
                for (int i = 0; i < userDefinedCallbackMonoBehaviours.Length; i++) {
                    monoBehaviours.Add(userDefinedCallbackMonoBehaviours[i]);
                }
            }
            // Find all methods
            for (int i = 0; i < monoBehaviours.Count; i++) {
                MonoBehaviour monoBehaviour = monoBehaviours[i];
                MethodInfo[] methodInfos = monoBehaviour.GetType().GetMethods();

                for (int j = 0; j < methodInfos.Length; j++) {
                    MethodInfo methodInfo = methodInfos[j];

                    MethodMan mm = new MethodMan();
                    mm.DisplayName = monoBehaviour.gameObject.name + "." + monoBehaviour.GetType().Name + "." + methodInfo.Name;
                    mm.LinkName = methodInfo.Name;
                    mm.SortKey = "_" + mm.DisplayName;
                    mm.Info = methodInfo;

                    if (methodInfo.DeclaringType == typeof(Object) || methodInfo.DeclaringType == typeof(Component) || methodInfo.DeclaringType == typeof(MonoBehaviour)) {
                        continue;
                    }
                    if (methodInfo.IsStatic || !methodInfo.IsPublic || methodInfo.Name.StartsWith("get_") || methodInfo.Name.StartsWith("set_")) {
                        continue;
                    }
                    if (SortedMethods.FindIndex(x => x.SortKey == mm.SortKey) >= 0) {
                        continue;
                    }
                    ParameterInfo[] parameters = methodInfo.GetParameters();
                    bool valid = true;
                    for (int k = 0; k < parameters.Length; k++) {
                        if (GetEventParameterType(parameters[k]) == InvokeEvent.ParameterType.None) {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) {
                        continue;
                    }

                    SortedMethods.Add(mm);
                }
            }

            SortedMethods = SortedMethods.OrderBy(x => x.SortKey).ToList();
            SortedMethodStrings = new List<string>();
            for (int a = 0; a < SortedMethods.Count; a++) {
                SortedMethodStrings.Add(SortedMethods[a].DisplayName);
            }
        }

        private InvokeEvent.ParameterType GetEventParameterType(ParameterInfo pinfo) {
            System.Type type = pinfo.ParameterType;
            if (type == typeof(int) || type == typeof(uint) || type == typeof(char) || type == typeof(byte) ||
                type == typeof(short) || type == typeof(ushort)) {
                return InvokeEvent.ParameterType.Int;
            }
            if (type == typeof(bool)) {
                return InvokeEvent.ParameterType.Bool;
            } else if (type == typeof(float)) {
                return InvokeEvent.ParameterType.Float;
            } else if (type == typeof(string)) {
                return InvokeEvent.ParameterType.String;
            } else if (typeof(Object).IsAssignableFrom(type)) {
                return InvokeEvent.ParameterType.ObjectReference;
            } else if (type == typeof(AnimationCurve)) {
                return InvokeEvent.ParameterType.AnimationCurve;
            } else if (typeof(AssetReference).IsAssignableFrom(type)) {
                return InvokeEvent.ParameterType.AssetReference;
            }
            return InvokeEvent.ParameterType.None;
        }

        private void UpdateEventParameterType(ParameterInfo pinfo, InvokeEvent.Parameter parameter) {
            parameter.ParameterType = GetEventParameterType(pinfo);

            if (parameter.ParameterType == InvokeEvent.ParameterType.Int) {
                parameter.IntOrBoolParameter = pinfo.RawDefaultValue != System.DBNull.Value ? (int) pinfo.RawDefaultValue : 0;
            }

            if (parameter.ParameterType == InvokeEvent.ParameterType.Bool) {
                parameter.IntOrBoolParameter = (pinfo.RawDefaultValue != System.DBNull.Value ? (bool) pinfo.RawDefaultValue : false) == true ? 1 : 0;
            }

            if (parameter.ParameterType == InvokeEvent.ParameterType.Float) {
                parameter.FloatParameter = pinfo.RawDefaultValue != System.DBNull.Value ? (float) pinfo.RawDefaultValue : 0.0f;
            }

            if (parameter.ParameterType == InvokeEvent.ParameterType.String) {
                parameter.StringParameter = pinfo.RawDefaultValue != System.DBNull.Value ? (string) pinfo.RawDefaultValue : string.Empty;
            }

            parameter.ObjectReferenceParameter = null;
            if (parameter.ParameterType == InvokeEvent.ParameterType.AnimationCurve) {
                parameter.AnimationCurveParameter = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            } else {
                parameter.AnimationCurveParameter = null;
            }
            if (parameter.ParameterType == InvokeEvent.ParameterType.AssetReference) {
                parameter.AssetReferenceParameter = System.Activator.CreateInstance(pinfo.ParameterType) as AssetReference;
            } else {
                parameter.AssetReferenceParameter = null;
            }
        }

        private void OnGUI() {
            if (ActiveEvent == null || SortedMethods == null)
                return;
            ActiveEvent.Time = System.Convert.ToSingle(EditorGUILayout.TextField("Time", ActiveEvent.Time.ToString()));
            if (SortedMethods.Count > 0) {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Function");
                int index = SortedMethods.FindIndex(m => m.LinkName == ActiveEvent.Method);
                int lastIndex = index;
                index = EditorGUILayout.Popup(Mathf.Max(index, 0), SortedMethodStrings.ToArray());
                MethodMan mm = SortedMethods[index];
                MethodInfo methodInfo = mm.Info;
                ActiveEvent.Method = methodInfo != null ? methodInfo.Name : string.Empty;
                if (index != lastIndex || ActiveEvent.Parameters == null || methodInfo == null || ActiveEvent.Parameters.Length != methodInfo.GetParameters().Length) {
                    if (methodInfo == null) {
                        ActiveEvent.Parameters = null;
                    } else {
                        ParameterInfo[] pinfos = methodInfo.GetParameters();
                        ActiveEvent.Parameters = new InvokeEvent.Parameter[pinfos.Length];
                        for (int i = 0; i < ActiveEvent.Parameters.Length; i++) {
                            InvokeEvent.Parameter parameter = new InvokeEvent.Parameter();
                            ParameterInfo pinfo = pinfos[i];
                            UpdateEventParameterType(pinfo, parameter);
                            ActiveEvent.Parameters[i] = parameter;
                        }
                    }
                }
                if (methodInfo != null) {
                    for (int i = 0; i < methodInfo.GetParameters().Length; i++) {
                        ParameterInfo parameterInfo = methodInfo.GetParameters()[i];
                        InvokeEvent.Parameter parameter = ActiveEvent.Parameters[i];
                        string parameterName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parameterInfo.Name);
                        switch (parameter.ParameterType) {
                            case InvokeEvent.ParameterType.Int:
                                parameter.IntOrBoolParameter = (int) EditorGUILayout.IntField(parameterName, parameter.IntOrBoolParameter);
                                break;
                            case InvokeEvent.ParameterType.Bool:
                                parameter.IntOrBoolParameter = EditorGUILayout.Toggle(parameterName, parameter.IntOrBoolParameter > 0) ? 1 : 0;
                                break;
                            case InvokeEvent.ParameterType.Float:
                                parameter.FloatParameter = (float) EditorGUILayout.FloatField(parameterName, parameter.FloatParameter);
                                break;
                            case InvokeEvent.ParameterType.String:
                                parameter.StringParameter = EditorGUILayout.TextField(parameterName, parameter.StringParameter);
                                break;
                            case InvokeEvent.ParameterType.ObjectReference:
                                parameter.ObjectReferenceParameter = EditorGUILayout.ObjectField(parameterName, parameter.ObjectReferenceParameter, parameterInfo.ParameterType, false);
                                break;
                            case InvokeEvent.ParameterType.AnimationCurve:
                                parameter.AnimationCurveParameter = EditorGUILayout.CurveField(parameterName, parameter.AnimationCurveParameter);
                                break;
                            case InvokeEvent.ParameterType.AssetReference:
                                if (parameter.AssetReferenceParameter == null) {
                                    parameter.AssetReferenceParameter = new AssetReference();
                                }
                                AssetReferenceEditor.Field(parameterName, parameter.AssetReferenceParameter, parameter.AssetReferenceParameter.GetTypeOfT());
                                break;
                        }
                    }
                }
                // Set dirty if Ui has changed
                if (EditorGUI.EndChangeCheck()) {
                    EditorUtility.SetDirty(ActiveGameObject);
                    if (UserDefinedCallbackMonoBehaviours != null) {
                        for (int i = 0; i < UserDefinedCallbackMonoBehaviours.Length; ++i) {
                            EditorUtility.SetDirty(UserDefinedCallbackMonoBehaviours[i]);
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                }
            }

        }
    }

}