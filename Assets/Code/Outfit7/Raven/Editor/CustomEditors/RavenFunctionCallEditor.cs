using System;
using System.Collections.Generic;
using System.Linq;
using Starlite.Raven.Compiler;
using UnityEditor;
using UnityEngine;

namespace Starlite.Raven {

    public class RavenFunctionCallEditor : EditorWindow {

        public class FunctionWrapper {
            public string m_FullName = null;
            public string m_PrettyName = null;

            public override string ToString() {
                return m_PrettyName;
            }
        }

        private static RavenFunctionCallEditor s_EventEditorInstance = null;

        private List<FunctionWrapper> m_Functions = null;
        private List<FunctionWrapper> m_FilteredFunctions = null;

        [SerializeField]
        private RavenSequence m_Sequence = null;

        [SerializeField]
        private RavenEvent m_Event = null;

        [SerializeField]
        private RavenTriggerPropertyComponentBase m_TriggerProperty = null;

        [SerializeField]
        private GameObject m_Target = null;

        [SerializeField]
        private string[] m_TypeConstraints = null;

        [SerializeField]
        private bool m_DisableValueEditor = false;

        [SerializeField]
        private string m_Filter = string.Empty;

        private SerializedObject m_SerializedPropertyObject = null;
        private SerializedProperty[] m_Properties = new SerializedProperty[RavenUtility.c_MaxFunctionParameters];
        private string[] m_Names = new string[RavenUtility.c_MaxFunctionParameters];

        public static void OpenEditor(RavenSequence sequence, GameObject targetGameObject, RavenEvent evnt, RavenTriggerPropertyComponentBase triggerProperty, bool disableValueEditor = false, params Type[] typeConstraints) {
            s_EventEditorInstance = GetWindow<RavenFunctionCallEditor>("Function Call Editor", true, typeof(EditorWindow));
            s_EventEditorInstance.Initialize(sequence, targetGameObject, evnt, triggerProperty, disableValueEditor, typeConstraints.Select(x => x.ToString()).ToArray());
            s_EventEditorInstance.ShowPopup();
        }

        public static void CloseEventEditor() {
            if (s_EventEditorInstance != null) {
                s_EventEditorInstance.Close();
                s_EventEditorInstance = null;
            }
        }

        public static void Refresh() {
            if (s_EventEditorInstance != null) {
                s_EventEditorInstance.CheckForModifications();
            }
        }

        private void OnEnable() {
            Initialize(m_Sequence, m_Target, m_Event, m_TriggerProperty, m_DisableValueEditor, m_TypeConstraints);
        }

        private void Initialize(RavenSequence sequence, GameObject targetGameObject, RavenEvent evnt, RavenTriggerPropertyComponentBase triggerProperty, bool disableValueEditor, string[] typeConstraints) {
            if (evnt == null) {
                return;
            }

            m_Event = evnt;
            m_TriggerProperty = triggerProperty;
            m_Sequence = sequence;
            m_Target = targetGameObject;
            m_TypeConstraints = typeConstraints;
            m_DisableValueEditor = disableValueEditor;

            var packedTypeConstraints = RavenUtility.GetFunctionParameterTypesPacked(m_TypeConstraints);
            m_Functions = new List<FunctionWrapper>(RavenEditorUtility.GetFunctionList(targetGameObject, string.IsNullOrEmpty(packedTypeConstraints) ? null : m_TypeConstraints)
                .Select((x) => {
                    return new FunctionWrapper() {
                        m_FullName = x,
                        m_PrettyName = GetPrettyNameFromFullFunctionName(x)
                    };
                }));

            m_Functions.RemoveAll(x => x == null);
            m_Functions.Sort((x, y) => x.m_PrettyName.CompareTo(y.m_PrettyName));
            m_FilteredFunctions = new List<FunctionWrapper>(m_Functions);
            m_SerializedPropertyObject = null;

            if (evnt != m_Event) {
                m_Filter = string.Empty;
            }
            Filter();

            InitializeProperty(m_TriggerProperty);
        }

        private void InitializeProperty(RavenTriggerPropertyComponentBase property) {
            if (property == null) {
                return;
            }

            m_Names = RavenUtility.GetParameterNamesForFunction(property.ComponentType,
                RavenUtility.GetFunctionNameFromPackedFunctionName(property.FunctionName),
                RavenUtility.GetFunctionParameterTypesUnpacked(RavenUtility.GetFunctionParametersFromPackedFunctionName(property.FunctionName)));

            m_SerializedPropertyObject = new SerializedObject(property);
            for (int i = 0; i < RavenUtility.c_MaxFunctionParameters; ++i) {
                m_Properties[i] = m_SerializedPropertyObject.FindProperty("m_Value" + i.ToString());
            }
        }

        private void OnGUI() {
            if (CheckForModifications()) {
                return;
            }

            if (m_Functions != null && m_Functions.Count > 0) {
                EditorGUILayout.LabelField("Function");
                int index = -1;
                if (m_TriggerProperty != null) {
                    index = m_FilteredFunctions.FindIndex(m => m.m_FullName == m_TriggerProperty.FullFunctionName);
                }
                int lastIndex = index;
                index = EditorGUILayout.Popup(index, m_FilteredFunctions.Select(x => x.m_PrettyName).ToArray());

                EditorGUI.BeginChangeCheck();
                m_Filter = EditorGUILayout.TextField("Filter", m_Filter);
                if (EditorGUI.EndChangeCheck()) {
                    Filter();
                }

                EditorGUILayout.Space();
                //EditorGUILayout.LabelField("Parameters");

                if (index != lastIndex) {
                    var fullFunctionName = m_FilteredFunctions[index].m_FullName;
                    GenerateFunctionCallProperty(fullFunctionName, m_Target, m_Event, true, out m_TriggerProperty, m_DisableValueEditor, m_TypeConstraints.Select(x => RavenUtility.GetTypeFromLoadedAssemblies(x)).ToArray());
                }

                DrawProperty();
            }
        }

        private void DrawProperty() {
            if (m_SerializedPropertyObject == null || m_Event == null || m_TriggerProperty == null) {
                return;
            }

            EditorGUI.BeginDisabledGroup(m_DisableValueEditor);
            m_SerializedPropertyObject.Update();

            for (int i = 0; i < RavenUtility.c_MaxFunctionParameters; ++i) {
                var prop = m_Properties[i];
                if (prop != null) {
                    EditorGUILayout.PropertyField(prop, new GUIContent(i < m_Names.Length ? ObjectNames.NicifyVariableName(m_Names[i]) : ObjectNames.NicifyVariableName(prop.name)), true);
                }
            }

            m_SerializedPropertyObject.ApplyModifiedProperties();
            EditorGUI.EndDisabledGroup();
        }

        private void Filter() {
            m_FilteredFunctions = new List<FunctionWrapper>(m_Functions.FindAll(x => x.m_PrettyName.ToLowerInvariant().Contains(m_Filter.ToLowerInvariant())));
        }

        private bool CheckForModifications() {
            if (m_Event != null && m_TriggerProperty != null && m_Target != m_TriggerProperty.Target) {
                Initialize(m_Sequence, m_TriggerProperty.Target, m_Event, m_TriggerProperty, m_DisableValueEditor, m_TypeConstraints);
                Repaint();
                FocusWindowIfItsOpen<RavenFunctionCallEditor>();
                return true;
            }
            return false;
        }

        public static bool GenerateFunctionCallProperty(string fullFunctionName, GameObject target, RavenEvent evnt, bool openEditor, out RavenTriggerPropertyComponentBase triggerProperty, bool disableValueEditor = false, params Type[] typeConstraints) {
            triggerProperty = null;

            var componentTypeName = RavenUtility.GetComponentNameFromFullFunctionName(fullFunctionName);
            var componentType = RavenUtility.GetTypeFromLoadedAssemblies(componentTypeName);
            var component = componentType == typeof(GameObject) ? target : target.GetComponent(componentType) as UnityEngine.Object;

            Type specType;
            if (PropertyReflectionCompiledOutput.HasPropertySpecialization(RavenUtility.GetFunctionParametersFromFullFunctionName(fullFunctionName), out specType)) {
                RavenEventView eventView;
                if (RavenSequenceEditor.Instance.SequenceView.GetEventView(evnt, out eventView)) {
                    triggerProperty = eventView.GenerateFunctionCallProperty(specType, target, component, RavenUtility.GetPackedFunctionNameFromFullFunctionName(fullFunctionName));
                    if (openEditor) {
                        OpenEditor(RavenSequenceEditor.Instance.Sequence, target, evnt, triggerProperty, disableValueEditor, typeConstraints);
                    }
                    return true;
                }
            } else {
                if (openEditor) {
                    OpenEditor(RavenSequenceEditor.Instance.Sequence, target, evnt, triggerProperty, disableValueEditor, typeConstraints);
                    s_EventEditorInstance.m_Filter = GetPrettyNameFromFullFunctionName(fullFunctionName);
                    s_EventEditorInstance.Filter();
                    s_EventEditorInstance.m_SerializedPropertyObject = null;
                }
                var go = new GameObject("TEMP");
                var dummyProperty = go.AddComponent<DummyTriggerNonBase>();
                dummyProperty.Target = target;
                dummyProperty.ComponentType = RavenUtility.GetComponentNameFromFullFunctionName(fullFunctionName);
                dummyProperty.FunctionName = RavenUtility.GetPackedFunctionNameFromFullFunctionName(fullFunctionName);
                dummyProperty.TargetComponent = component;
                EditorUtility.DisplayDialog("New Class", "New data class will be compiled. Please reselect the selected function once it finishes compiling.", "Ok");
                Compiler.RavenCompiler.CompileProperty(dummyProperty);
                GameObject.DestroyImmediate(go);
            }

            return false;
        }

        private static string GetPrettyNameFromFullFunctionName(string fullFunctionName) {
            var split = fullFunctionName.Split('|');
            return RavenUtility.GetTypeWithoutNamespace(split[0]) + "." + split[1] + "(" + split[2] + ")";
        }
    }
}