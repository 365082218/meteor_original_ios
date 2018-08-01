using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Logic {

    public partial class StateMachineEditor {
        public static string[] GetParameterNames(StateMachine stateMachine, System.Predicate<Parameter> match, bool canBeNone) {
            string[] names = new string[canBeNone ? 1 : 0];
            if (canBeNone) {
                names[0] = "None";
            }
            for (int i = 0; i < stateMachine.Parameters.Length; i++) {
                if (match == null || match.Invoke(stateMachine.Parameters[i])) {
                    ArrayUtility.Add(ref names, stateMachine.Parameters[i].Name);
                }
            }
            return names;
        }

        protected string[] GetParameterNames(System.Predicate<Parameter> match, bool canBeNone) {
            string[] names = new string[canBeNone ? 1 : 0];
            if (canBeNone) {
                names[0] = "None";
            }
            for (int i = 0; i < TargetStateMachine.Parameters.Length; i++) {
                if (match == null || match.Invoke(TargetStateMachine.Parameters[i])) {
                    ArrayUtility.Add(ref names, TargetStateMachine.Parameters[i].Name);
                }
            }
            return names;
        }

        protected void SeparatorGUI() {
            int identLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("", SeparatorStyle);
            EditorGUI.indentLevel = identLevel;
        }

        protected void PushBackgroundColor(Color newColor) {
            Color color = GUI.backgroundColor;
            BackgroundColorStack.Push(color);
            GUI.backgroundColor = newColor;
        }

        protected void PopBackgroundColor() {
            Assert.IsTrue(BackgroundColorStack.Count > 0);
            GUI.backgroundColor = BackgroundColorStack.Pop();
        }

        protected void PushColor(Color newColor) {
            Color color = GUI.color;
            ColorStack.Push(color);
            GUI.color = newColor;
        }

        protected void PopColor() {
            Assert.IsTrue(ColorStack.Count > 0);
            GUI.color = ColorStack.Pop();
        }

        protected void OnConditionValueGUI(Condition condition) {
            if (condition.Parameter == null) {
                return;
            }
            switch (condition.Parameter.ParameterType) {
                case ParameterType.Bool:
                    condition.ValueInt = EditorGUILayout.Toggle(condition.ValueInt != 0) ? 1 : 0;
                    break;
                case ParameterType.Float:
                    condition.ValueFloat = EditorGUILayout.FloatField(condition.ValueFloat);
                    break;
                case ParameterType.Enum:
                    condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.Int:
                    condition.ValueInt = EditorGUILayout.IntField(condition.ValueInt);
                    break;
                case ParameterType.BoolTrigger:
                    condition.ValueInt = EditorGUILayout.Toggle(condition.ValueInt != 0) ? 1 : 0;
                    break;
                case ParameterType.IntTrigger:
                    condition.ValueInt = EditorGUILayout.IntField(condition.ValueInt);
                    break;
                case ParameterType.EnumTrigger:
                    condition.ValueInt = SelectEnumStringGUI(condition.Parameter, condition.ValueInt);
                    break;
                case ParameterType.EnumBitMask:
                    condition.ValueInt = SelectEnumMaskGUI(condition.Parameter, condition.ValueInt);
                    break;
            }
        }

        private int SelectEnumStringGUI(Parameter parameter, int index) {
            if (parameter.ParameterIndex < 0 || parameter.ParameterIndex >= TargetStateMachine.Enums.Length) {
                return 0;
            }
            string[] strings = TargetStateMachine.Enums[parameter.ParameterIndex].Strings;
            if (index < 0 || index > strings.Length) {
                index = 0;
            }
            return EditorGUILayout.Popup(index, strings);
        }

        private int SelectEnumMaskGUI(Parameter parameter, int index) {
            string[] strings = TargetStateMachine.Enums[parameter.ParameterIndex].Strings;
            return EditorGUILayout.MaskField(index, strings);
        }

        protected  void OnParameterValueGUI(Parameter parameter) {
            if (parameter == null) {
                return;
            }
            switch (parameter.ParameterType) {
                case ParameterType.Bool:
                    parameter.ValueInt = EditorGUILayout.Toggle(parameter.ValueInt != 0) ? 1 : 0;
                    break;
                case ParameterType.Float:
                    parameter.ValueFloat = EditorGUILayout.FloatField(parameter.ValueFloat);
                    break;
                case ParameterType.Int:
                    parameter.ValueInt = EditorGUILayout.IntField(parameter.ValueInt);
                    break;
                case ParameterType.Enum:
                    parameter.ValueInt = SelectEnumStringGUI(parameter, parameter.ValueInt);
                    break;
                case ParameterType.BoolTrigger:
                    parameter.ValueInt = EditorGUILayout.Toggle(parameter.ValueInt != 0) ? 1 : 0;
                    break;
                case ParameterType.IntTrigger:
                    parameter.ValueInt = EditorGUILayout.IntField(parameter.ValueInt);
                    break;
                case ParameterType.EnumTrigger:
                    parameter.ValueInt = SelectEnumStringGUI(parameter, parameter.ValueInt);
                    break;
                case ParameterType.EnumBitMask:
                    parameter.ValueInt = SelectEnumMaskGUI(parameter, parameter.ValueInt);
                    break;
            }
        }

        protected bool ParameterGUI(Parameter parameter, string[] parameterNames, out int index) {
            string parameterName = parameter != null ? parameter.Name : string.Empty;
            int oldIndex = ArrayUtility.FindIndex(parameterNames, p => p == parameterName);
            int newIndex = EditorGUILayout.Popup(oldIndex == -1 ? 0 : oldIndex, parameterNames);
            if (oldIndex != newIndex) {
                index = newIndex;
                return true;
            }
            index = -1;
            return false;
        }

        protected void OnTransitionGUI(State state, Transition transition) {
            ShowTransitions = EditorGUILayout.Foldout(ShowTransitions, string.Format("Transition {0} -> {1}", state.Name, transition.DestinationState.Name));
            if (ShowTransitions) {
                EditorGUI.indentLevel++;
                string[] parameterNames = GetParameterNames(null, false);
                string[] parameterNamesWithNone = GetParameterNames(null, true);
                EditorGUILayout.BeginVertical();
                transition.Enabled = EditorGUILayout.Toggle("Enabled", transition.Enabled);
                transition.Atomic = EditorGUILayout.Toggle("Atomic", transition.Atomic);
                transition.UpdateTime = EditorGUILayout.Toggle("Update Time", transition.UpdateTime);
                EditorGUILayout.BeginHorizontal();
                transition.Duration = EditorGUILayout.FloatField("Duration", transition.Duration);
                transition.AnimationCurve = EditorGUILayout.CurveField(transition.AnimationCurve);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                ShowConditions = EditorGUILayout.Foldout(ShowConditions, "Conditions");
                if (ShowConditions) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < transition.Conditions.Length; i++) {
                        Condition condition = transition.Conditions[i];
                        EditorGUILayout.BeginHorizontal();
                        // Parameter
                        int index;
                        if (ParameterGUI(condition.Parameter, parameterNames, out index)) {
                            condition.ParameterIndex = index;
                            condition.Parameter = TargetStateMachine.Parameters[condition.ParameterIndex];
                        }
                        if (condition.Parameter.ParameterType != ParameterType.Bool && condition.Parameter.ParameterType != ParameterType.BoolTrigger) {
                            condition.ConditionMode = (ConditionMode) EditorGUILayout.EnumPopup(condition.ConditionMode);
                        } else {
                            condition.ConditionMode = ConditionMode.Equal;
                        }
                        // Value parameter
                        if (ParameterGUI(condition.ValueParameter, parameterNamesWithNone, out index)) {
                            if (index == 0) {
                                condition.ValueIndex = -1;
                                condition.ValueParameter = null;
                            } else {
                                condition.ValueIndex = index - 1;
                                condition.ValueParameter = TargetStateMachine.Parameters[condition.ValueIndex];
                            }
                        }
                        if (condition.ValueParameter == null) {
                            // Special attribute mask override
                            // Has the second of the layer built-in parameters
                            if (condition.ParameterIndex < TargetStateMachine.Layers.Length * BuildInParameterNames.Length && (condition.ParameterIndex % BuildInParameterNames.Length) == 1) {
                                condition.ValueInt = EditorGUILayout.MaskField(condition.ValueInt, TargetStateMachine.AttributeNames);
                            } else {
                                // Value
                                OnConditionValueGUI(condition);
                            }
                        }
                        AddRemoveMoveAction addRemove = LogicEditorCommon.AddRemoveGUI(i == transition.Conditions.Length - 1, transition.Conditions.Length > 1);
                        if (addRemove == AddRemoveMoveAction.Add) {
                            AddUndo("AddCondition");
                            AddCondition(transition, TargetStateMachine.Parameters[0]);
                        } else if (addRemove == AddRemoveMoveAction.Remove) {
                            AddUndo("RemoveCondition");
                            RemoveCondition(transition, condition);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                // Events
                ShowEnterEvents = EditorGUILayout.Foldout(ShowEnterEvents, "Pre Enter Events");
                if (ShowEnterEvents) {
                    if (LogicEditorCommon.EditEvents(TargetStateMachine.gameObject, TargetStateMachine.UserDefinedCallbackMonoBehaviours, ref transition.PreEnterEvents)) {
                        GUI.changed = true;
                    }
                }
                ShowExitEvents = EditorGUILayout.Foldout(ShowExitEvents, "Post Enter Events");
                if (ShowExitEvents) {
                    if (LogicEditorCommon.EditEvents(TargetStateMachine.gameObject, TargetStateMachine.UserDefinedCallbackMonoBehaviours, ref transition.PostEnterEvents)) {
                        GUI.changed = true;
                    }
                }
                EditorGUI.indentLevel--;
            }
            SeparatorGUI();
        }

        protected virtual void OnStateCustomLoopGUI() {
        }

        protected virtual void OnStateCustomGUI() {
        }

        protected void OnStateGUI() {
            ShowState = EditorGUILayout.Foldout(ShowState, string.Format("State ({0})", ActiveState.Name));
            if (ShowState) {
                EditorGUI.indentLevel++;
                if (!IsAnyState(ActiveLayer, ActiveState)) {
                    string name = EditorGUILayout.TextField("Name", ActiveState.Name);
                    if (name != ActiveState.Name) {
                        ActiveState.Name = GetUniqueName(ActiveLayer.StateIndices, TargetStateMachine.States, name);
                    }
                    ActiveState.AttributeMask = EditorGUILayout.MaskField("Attributes", ActiveState.AttributeMask, TargetStateMachine.AttributeNames);
                    ActiveState.Switch = EditorGUILayout.Toggle("Switch", ActiveState.Switch);
                    // Only show blend tree if its not a switch state
                    if (!ActiveState.Switch) {
                        ActiveState.Speed = EditorGUILayout.FloatField("Speed", ActiveState.Speed);
                        ActiveState.Weight = EditorGUILayout.Slider("Weight", ActiveState.Weight, 0.0f, 1.0f);
                        ActiveState.UseNormalizedTime = EditorGUILayout.Toggle("Use Normalized Time", ActiveState.UseNormalizedTime);
                        ActiveState.TimeAnimationCurve = EditorGUILayout.CurveField("Time Curve", ActiveState.TimeAnimationCurve);
                        ActiveState.TimeInSeconds = EditorGUILayout.Toggle("Time in Seconds", ActiveState.TimeInSeconds);
                        EditorGUILayout.MinMaxSlider(new GUIContent("Play Range"), ref ActiveState.PlayRange.x, ref ActiveState.PlayRange.y, 0.0f, 1.0f);
                        ActiveState.PlayRange = EditorGUILayout.Vector2Field("", ActiveState.PlayRange);
                        ActiveState.Loop = EditorGUILayout.BeginToggleGroup("Loop", ActiveState.Loop);
                        if (ActiveState.Loop) {
                            EditorGUI.indentLevel++;
                            PushBackgroundColor(CustomColor);
                            OnStateCustomLoopGUI();
                            PopBackgroundColor();
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndToggleGroup();
                        SeparatorGUI();
                        PushBackgroundColor(CustomColor);
                        OnStateCustomGUI();
                        PopBackgroundColor();
                        SeparatorGUI();
                    }
                    // Events
                    ShowEnterEvents = EditorGUILayout.Foldout(ShowEnterEvents, "Enter Events");
                    if (ShowEnterEvents) {
                        if (LogicEditorCommon.EditEvents(TargetStateMachine.gameObject, TargetStateMachine.UserDefinedCallbackMonoBehaviours, ref ActiveState.EnterEvents)) {
                            GUI.changed = true;
                        }
                    }
                    ShowExitEvents = EditorGUILayout.Foldout(ShowExitEvents, "Exit Events");
                    if (ShowExitEvents) {
                        if (LogicEditorCommon.EditEvents(TargetStateMachine.gameObject, TargetStateMachine.UserDefinedCallbackMonoBehaviours, ref ActiveState.ExitEvents)) {
                            GUI.changed = true;
                        }
                    }
                }

                ShowStateComment = EditorGUILayout.Foldout(ShowStateComment, "State Comment");
                if (ShowStateComment) {
                    ActiveState.Comment = EditorGUILayout.TextArea(ActiveState.Comment);
                }

                ShowTransitions = EditorGUILayout.Foldout(ShowTransitions, "Transitions");
                if (ShowTransitions) {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < ActiveState.TransitionIndices.Length; i++) {
                        int index = ActiveState.TransitionIndices[i];
                        Transition transition = TargetStateMachine.Transitions[index];
                        if (ActiveTransitionInGUI == transition) {
                            EditorGUILayout.BeginHorizontal((GUIStyle) "SelectionRect");
                        } else {
                            EditorGUILayout.BeginHorizontal();
                        }
                        if (GUILayout.Button(string.Format("Transition -> {0}", transition.DestinationState.Name))) {
                            ActiveTransitionInGUI = transition;
                        }
                        AddRemoveMoveAction addRemoveAndMove = LogicEditorCommon.AddRemoveAndMoveGUI(false, true, i > 0, i < ActiveState.TransitionIndices.Length - 1);
                        if (addRemoveAndMove == AddRemoveMoveAction.Remove) {
                            AddUndo("RemoveTransition");
                            RemoveTransition(ActiveState, transition);
                            i--;
                        } else if (addRemoveAndMove == AddRemoveMoveAction.MoveUp) {
                            AddUndo("MoveUpTransition");
                            ActiveState.TransitionIndices[i] = ActiveState.TransitionIndices[i - 1];
                            ActiveState.TransitionIndices[i - 1] = index;
                        } else if (addRemoveAndMove == AddRemoveMoveAction.MoveDown) {
                            AddUndo("MoveDownTransition");
                            ActiveState.TransitionIndices[i] = ActiveState.TransitionIndices[i + 1];
                            ActiveState.TransitionIndices[i + 1] = index;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (ActiveTransitionInGUI != null) {
                        OnTransitionGUI(ActiveState, ActiveTransitionInGUI);
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
            }
            SeparatorGUI();
        }

        protected virtual void OnLayerCustomGUI() {
        }

        protected void OnLayerGUI() {
            ShowLayer = EditorGUILayout.Foldout(ShowLayer, string.Format("Layer ({0})", ActiveLayer.Name));
            if (ShowLayer) {
                EditorGUI.indentLevel++;
                // Show just normalized time on base layer
                PushBackgroundColor(CustomColor);
                OnLayerCustomGUI();
                PopBackgroundColor();
                SeparatorGUI();
                ShowLayerComment = EditorGUILayout.Foldout(ShowLayerComment, "Layer Comment");
                if (ShowLayerComment) {
                    ActiveLayer.Comment = EditorGUILayout.TextArea(ActiveLayer.Comment);
                }
                EditorGUI.indentLevel--;
            }
            SeparatorGUI();
        }

        protected void OnEnumsGUI() {
            ShowEnums = EditorGUILayout.Foldout(ShowEnums, "Enums");
            if (ShowEnums) {
                EditorGUI.indentLevel++;
                for (int i = 0; i < TargetStateMachine.Enums.Length; i++) {
                    StateMachineInternal.Enum Enum = TargetStateMachine.Enums[i];
                    EditorGUILayout.BeginVertical();
                    string enumName = EditorGUILayout.TextField("Enum", Enum.Name);
                    if (enumName != Enum.Name) {
                        Enum.Name = GetUniqueName(TargetStateMachine.Enums, enumName);
                    }
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < Enum.Strings.Length; j++) {
                        EditorGUILayout.BeginHorizontal();
                        Enum.Strings[j] = EditorGUILayout.TextField(string.Format("{0}", j), Enum.Strings[j]);
                        AddRemoveMoveAction addRemoveAndMove = LogicEditorCommon.AddRemoveAndMoveGUI(true, true, j > 0, j < Enum.Strings.Length - 1);
                        if (addRemoveAndMove == AddRemoveMoveAction.Add) {
                            AddUndo("AddEnum");
                            AddEnumString(i, Enum, "New Enum String", j);
                        } else if (addRemoveAndMove == AddRemoveMoveAction.Remove) {
                            AddUndo("RemoveEnum");
                            RemoveEnumString(i, Enum, j);
                            j--;
                        } else if (addRemoveAndMove == AddRemoveMoveAction.MoveUp) {
                            AddUndo("MoveUpEnum");
                            SwapEnumString(i, Enum, j, j - 1);
                        } else if (addRemoveAndMove == AddRemoveMoveAction.MoveDown) {
                            AddUndo("MoveDownEnum");
                            SwapEnumString(i, Enum, j, j + 1);
                        }
                        if (GUILayout.Button("?", GUILayout.Width(20.0f))) {
                            ShowEnumsOnAllLayers(Enum, j, Enum.Strings[j]);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (LogicEditorCommon.AddRemoveGUI(true, false) == AddRemoveMoveAction.Add) {
                        AddEnumString(i, Enum, "New Enum String", -1);
                    }
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.BeginHorizontal();
                    AddRemoveMoveAction addRemoveAction = LogicEditorCommon.AddRemoveGUI(i == TargetStateMachine.Enums.Length - 1, true);
                    if (addRemoveAction == AddRemoveMoveAction.Add) {
                        AddUndo("AddEnum");
                        AddEnum(GetUniqueName(TargetStateMachine.Enums, NewEnumName), -1);
                    } else if (addRemoveAction == AddRemoveMoveAction.Remove) {
                        AddUndo("RemoveEnum");
                        RemoveEnum(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (LogicEditorCommon.AddRemoveGUI(true, false) == AddRemoveMoveAction.Add) {
                    AddUndo("AddEnum");
                    AddEnum(GetUniqueName(TargetStateMachine.Enums, NewEnumName), -1);
                }

                EditorGUI.indentLevel--;
            }
            SeparatorGUI();
        }

        protected void OnParameterGUI() {
            ShowParameters = EditorGUILayout.Foldout(ShowParameters, "Parameters");
            if (ShowParameters) {
                EditorGUI.indentLevel++;
                string[] enumTypeStrings = new string[TargetStateMachine.Enums.Length];
                // Fill enums
                for (int i = 0; i < TargetStateMachine.Enums.Length; i++) {
                    enumTypeStrings[i] = TargetStateMachine.Enums[i].Name;
                }
                // GUI
                for (int i = 0; i < TargetStateMachine.Parameters.Length; i++) {
                    Parameter parameter = TargetStateMachine.Parameters[i];
                    EditorGUILayout.BeginHorizontal();
                    if (i < TargetStateMachine.Layers.Length * BuildInParameterNames.Length) {
                        EditorGUILayout.TextField(parameter.Name);
                        // Special attribute mask
                        if ((i % BuildInParameterNames.Length) == 1) {
                            EditorGUILayout.MaskField(parameter.ValueInt, TargetStateMachine.AttributeNames);
                        } else {
                            OnParameterValueGUI(parameter);
                        }
                    } else {
                        string name = EditorGUILayout.TextField(parameter.Name);
                        if (name != parameter.Name) {
                            parameter.Name = GetUniqueName(TargetStateMachine.Parameters, name);
                        }
                        parameter.ParameterType = (ParameterType) EditorGUILayout.EnumPopup(parameter.ParameterType);
                        if (parameter.IsEnum) {
                            parameter.ParameterIndex = EditorGUILayout.Popup(parameter.ParameterIndex, enumTypeStrings);
                        }
                        OnParameterValueGUI(parameter);
                    }
                    AddRemoveMoveAction addRemoveAndMove = LogicEditorCommon.AddRemoveAndMoveGUI(i == TargetStateMachine.Parameters.Length - 1, i >= TargetStateMachine.Layers.Length, i > TargetStateMachine.Layers.Length, i >= TargetStateMachine.Layers.Length && i < TargetStateMachine.Parameters.Length - 1);
                    if (addRemoveAndMove == AddRemoveMoveAction.Add) {
                        AddUndo("AddParameter");
                        AddParameter(GetUniqueName(TargetStateMachine.Parameters, NewParameterName), -1);
                    } else if (addRemoveAndMove == AddRemoveMoveAction.Remove) {
                        AddUndo("RemoveParameter");
                        RemoveParameter(parameter);
                        i--;
                    } else if (addRemoveAndMove == AddRemoveMoveAction.MoveUp) {
                        AddUndo("MoveUpParameter");
                        SwapParameter(i, i - 1);
                    } else if (addRemoveAndMove == AddRemoveMoveAction.MoveDown) {
                        AddUndo("MoveDownParameter");
                        SwapParameter(i, i + 1);
                    }
                    if (GUILayout.Button("?")) {
                        ShowParametersOnAllLayers(i, parameter.Name);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
            SeparatorGUI();
        }

        protected void OnAttributesGUI() {
            ShowAttributes = EditorGUILayout.Foldout(ShowAttributes, "Attributes");
            if (ShowAttributes) {
                for (int i = 0; i < 32; i++) {
                    EditorGUILayout.BeginHorizontal();
                    TargetStateMachine.AttributeNames[i] = EditorGUILayout.TextField(string.Format("Attribute {0}", i), TargetStateMachine.AttributeNames[i]);
                    if (GUILayout.Button("Clear")) {
                        ClearAttributeFromAllLayers(i);
                    }
                    if (GUILayout.Button("Show used")) {
                        ShowAtributeOnAllLayers(TargetStateMachine.AttributeNames[i], i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        protected void OnCallbacksGUI() {
            ShowUserDefinedCallbacks = EditorGUILayout.Foldout(ShowUserDefinedCallbacks, "User defined callbacks");
            if (ShowUserDefinedCallbacks) {
                EditorGUILayout.BeginVertical();
                EditorGUI.indentLevel++;
                for (int j = 0; j < TargetStateMachine.UserDefinedCallbackMonoBehaviours.Length; j++) {
                    EditorGUILayout.BeginHorizontal();
                    TargetStateMachine.UserDefinedCallbackMonoBehaviours[j] = EditorGUILayout.ObjectField(TargetStateMachine.UserDefinedCallbackMonoBehaviours[j], typeof(UnityEngine.MonoBehaviour), true) as UnityEngine.MonoBehaviour;
                    AddRemoveMoveAction addRemoveAndMove = LogicEditorCommon.AddRemoveGUI(true, true);
                    if (addRemoveAndMove == AddRemoveMoveAction.Add) {
                        AddUndo("AddUserDefineCallback");
                        ArrayUtility.Insert(ref TargetStateMachine.UserDefinedCallbackMonoBehaviours, j, null);
                    } else if (addRemoveAndMove == AddRemoveMoveAction.Remove) {
                        AddUndo("RemoveUserDefineCallback");
                        ArrayUtility.RemoveAt(ref TargetStateMachine.UserDefinedCallbackMonoBehaviours, j--);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (LogicEditorCommon.AddRemoveGUI(true, false) == AddRemoveMoveAction.Add) {
                    AddUndo("AddUserDefineCallback");
                    ArrayUtility.Add(ref TargetStateMachine.UserDefinedCallbackMonoBehaviours, null);
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
        }

        protected virtual void OnBaseCustomGUI() {
        }

        protected void OnBaseGUI() {
            PushBackgroundColor(CustomColor);
            OnBaseCustomGUI();
            PopBackgroundColor();
            TargetStateMachine.UpdateIfPaused = EditorGUILayout.Toggle("Update If Paused", TargetStateMachine.UpdateIfPaused);
            TargetStateMachine.GlobalMessageEventLockIndex = EditorGUILayout.IntSlider("Global Message Event Lock Index", TargetStateMachine.GlobalMessageEventLockIndex, 0, 31);
        }

        protected virtual void OnPreCustomGUI() {
        }

        protected virtual void OnPostCustomGUI() {
        }

        protected virtual void OnShowWindow() {
            ShowWindow();
        }

        public override void OnInspectorGUI() {

            bool refresh = false;

            DrawInspectorGUI();
            BeginEdit();
            InitializeStyles();
            PushBackgroundColor(CustomColor);
            OnPreCustomGUI();
            PopBackgroundColor();
            OnBaseGUI();
            OnExportGUI();
            OnCallbacksGUI();
            OnAttributesGUI();
            OnEnumsGUI();
            OnParameterGUI();
            PushBackgroundColor(CustomColor);
            OnPostCustomGUI();
            PopBackgroundColor();
            OnLayerGUI();
            if (ActiveTransition != null) {
                OnTransitionGUI(ActiveTransitionState, ActiveTransition);
            } else if (ActiveState != null) {
                OnStateGUI();
            }
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show window")) {
                OnShowWindow();
            }
            if (GUILayout.Button("Export")) {
                Export();
                refresh = true;
            }

            EditorGUILayout.EndHorizontal();
            EndEdit();

            if (refresh) {
                refresh = false;
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("Outfit7/Logic/State Machine", false, 19)]
        public static void ShowWindow() {
            EditorWindow.GetWindow<StateMachineEditorWindow>();
        }

    }
}
