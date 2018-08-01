using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Outfit7.Logic.StateMachineInternal;
using System;

namespace Outfit7.Logic {

    [CustomEditor(typeof(StateMachine), true)]
    public abstract partial class StateMachineEditor : BucketUpdateBehaviourEditor {

        // Structs/classes
        protected struct MousePosition {
            public Vector2 Position;
            public Vector2 GlobalPosition;

            public void SetPosition(Vector2 position, Vector2 offset, Vector2 halfWindowSize, float zoom) {
                Position = position;
                GlobalPosition = (position - halfWindowSize) / zoom + offset;
            }
        }

        // Constants
        protected const float TransitionOffset = 0.25f;
        protected const float ArrowSize = 10.0f;
        protected const float ArrowWidth = 8.0f;
        protected const float MaxTransitionDistance = 5.0f;
        protected const string NewStateName = "New State";
        protected const string NewLayerName = "New Layer";
        protected const string NewEnumName = "New Enum";
        protected const string NewParameterName = "New Parameter";
        protected readonly string[] BuildInParameterNames = { "Time", "Attribute Mask" };
        protected readonly ParameterType[] BuildInParameterTypes = { ParameterType.Float, ParameterType.Int };

        protected virtual float SnapSize { get { return 10.0f; } }

        protected virtual float TransitionThickness { get { return 3.0f; } }

        protected virtual float ActiveTransitionThickness { get { return 3.0f; } }

        protected virtual Vector2 StateSize { get { return new Vector2(200, 50); } }

        protected virtual float StateActiveYScale { get { return 0.75f; } }

        protected virtual int WindowUIWidth { get { return 400; } }

        protected virtual Vector2 ZoomLimit { get { return new Vector2(0.3f, 2.0f); } }

        protected virtual Color CustomColor { get { return Color.white; } }

        protected Vector2 positionOffset = Vector2.zero;

        protected float zoom = 1f;

        protected Vector2 PositionOffset {
            get {
                return positionOffset;
            }
            set {
                positionOffset = value;
            }
        }

        protected float Zoom {
            get {
                return zoom;
            }
            set {
                zoom = value;
            }
        }

        protected Vector2 EditorPrefsPositionOffset {
            get {
                return new Vector2(EditorPrefs.GetFloat(EditorPositionOffsetXKey, 0f), EditorPrefs.GetFloat(EditorPositionOffsetYKey, 0f));
            }
            set {
                EditorPrefs.SetFloat(EditorPositionOffsetXKey, value.x);
                EditorPrefs.SetFloat(EditorPositionOffsetYKey, value.y);
            }
        }

        protected float EditorPrefsZoom {
            get {
                return EditorPrefs.GetFloat(EditorPositionOffsetZKey, 1f);
            }
            set {
                EditorPrefs.SetFloat(EditorPositionOffsetZKey, value);
            }
        }

        protected Vector2 ScaledStateSize { get { return StateSize * Zoom; } }

        private string editorPositionOffsetXKey = null;

        protected string EditorPositionOffsetXKey {
            get {
                return editorPositionOffsetXKey ?? (editorPositionOffsetXKey = GetGUID(TargetStateMachine.transform) + "|" + ActiveLayerIndex + "|EditorPositionOffsetX");
            }
        }

        private string editorPositionOffsetYKey = null;

        protected string EditorPositionOffsetYKey {
            get {
                return editorPositionOffsetYKey ?? (editorPositionOffsetYKey = GetGUID(TargetStateMachine.transform) + "|" + ActiveLayerIndex + "|EditorPositionOffsetY");
            }
        }

        private string editorPositionOffsetZKey = null;

        protected string EditorPositionOffsetZKey {
            get {
                return editorPositionOffsetZKey ?? (editorPositionOffsetZKey = GetGUID(TargetStateMachine.transform) + "|" + ActiveLayerIndex + "|EditorPositionOffsetZ");
            }
        }

        private string editorLayerIndexKey = null;

        protected string EditorLayerIndexKey {
            get {
                return editorLayerIndexKey ?? (editorLayerIndexKey = GetGUID(TargetStateMachine.transform) + "|EditorLayerIndex");
            }
        }

        // Members
        protected MousePosition CurrentMousePosition;
        protected MousePosition Button0MousePosition;
        protected MousePosition Button1MousePosition;
        protected MousePosition Button2MousePosition;
        protected bool SnapPosition = false;
        protected Vector2 SelectionOffset = Vector2.zero;
        protected int ActiveLayerIndex = -1;
        protected State ActiveDragState;
        protected State OverState;
        protected Transition ActiveTransition;
        protected Transition ActiveTransitionInGUI;
        protected State ActiveTransitionState;
        protected Transition OverTransition;
        protected State OverTransitionState;
        protected State TransitionFromState;
        protected Vector2 WindowSize = Vector2.one;
        protected Vector2 HalfWindowSize = Vector2.one;
        protected bool WindowInFocus = false;
        protected Stack<Color> ColorStack = new Stack<Color>();
        protected Stack<Color> BackgroundColorStack = new Stack<Color>();

        // Foldouts
        protected bool ShowState = true;
        protected bool ShowLayer = true;
        protected bool ShowEnums = false;
        protected bool ShowParameters = true;
        protected bool ShowTransitions = true;
        protected bool ShowConditions = true;
        protected bool ShowAttributes = false;
        protected bool ShowLayerComment = false;
        protected bool ShowStateComment = false;
        protected bool ShowEnterEvents = false;
        protected bool ShowExitEvents = false;
        protected bool ShowUserDefinedCallbacks = false;

        // Styles
        protected GUIStyle StateDefaultStyle;
        protected GUIStyle StateDefaultSelectedStyle;
        protected GUIStyle StateAnyStyle;
        protected GUIStyle StateAnySelectedStyle;
        protected GUIStyle StateStyle;
        protected GUIStyle StateSelectedStyle;
        protected GUIStyle StateSwitchStyle;
        protected GUIStyle StateSwitchSelectedStyle;
        protected GUIStyle StateActiveBackgroundStyle;
        protected GUIStyle StateActiveProgressStyle;
        protected GUIStyle SeparatorStyle;
        protected GUIStyle WindowUIStyle;
        protected GUIStyle CenteredLabelStyle;

        protected StateMachine TargetStateMachine { get; private set; }

        protected Layer ActiveLayer { get; private set; }

        protected State ActiveState { get; private set; }

        public abstract string TitleName { get; }

        protected abstract Layer AddLayerInternal();

        protected abstract void RemoveLayerInternal(Layer layer);

        protected abstract State AddStateInternal();

        protected abstract void RemoveStateInternal(State state);

        protected T AddGeneric<T>(ref T[] ts) where T : new() {
            if (ts == null) {
                ts = new T[0];
            }
            T t = new T();
            ArrayUtility.Add(ref ts, t);
            return t;
        }

        protected void RemoveGeneric<T>(ref T[] ts, T t) {
            ArrayUtility.Remove(ref ts, t);
        }

        private void InitializeStyles() {
            if (StateDefaultStyle != null) {
                return;
            }
            StateDefaultStyle = (GUIStyle) "flow node 5";
            StateDefaultSelectedStyle = (GUIStyle) "flow node 5 on";
            StateAnyStyle = (GUIStyle) "flow node 2";
            StateAnySelectedStyle = (GUIStyle) "flow node 2 on";
            StateStyle = (GUIStyle) "flow node 0";
            StateSelectedStyle = (GUIStyle) "flow node 0 on";
            StateSwitchStyle = (GUIStyle) "flow node 1";
            StateSwitchSelectedStyle = (GUIStyle) "flow node 1 on";
            StateActiveBackgroundStyle = (GUIStyle) "MeLivePlayBackground";
            StateActiveProgressStyle = (GUIStyle) "MeLivePlayBar";
            SeparatorStyle = (GUIStyle) "WindowBottomResize";
            WindowUIStyle = (GUIStyle) "Button";
            CenteredLabelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            CenteredLabelStyle.name = "Centered Label";
            CenteredLabelStyle.alignment = TextAnchor.UpperCenter;
        }

        private void InitializeAttributes() {
            if (TargetStateMachine.AttributeNames != null && TargetStateMachine.AttributeNames.Length == 32) {
                return;
            }
            TargetStateMachine.AttributeNames = new string[32];
        }

        private void InitializeEnums() {
            if (TargetStateMachine.Enums != null) {
                return;
            }
            TargetStateMachine.Enums = new StateMachineInternal.Enum[0];
        }

        private void ValidateLayer(Layer layer) {
            Dictionary<State, Transition> RemoveTransitions = new Dictionary<State, Transition>();
            for (int i = 0; i < layer.StateIndices.Length; ++i) {
                int sourceStateIndex = layer.StateIndices[i];
                int[] transitionIndices = TargetStateMachine.States[sourceStateIndex].TransitionIndices;
                for (int j = 0; j < transitionIndices.Length; ++j) {
                    int stateIndex = TargetStateMachine.Transitions[transitionIndices[j]].DestinationStateIndex;
                    if (ArrayUtility.FindIndex(layer.StateIndices, si => stateIndex == si) == -1 || stateIndex == -1) {
                        Debug.LogErrorFormat("Transition from state {0} pointing to an invalid state {1}!", TargetStateMachine.States[sourceStateIndex].Name, stateIndex != -1 ? TargetStateMachine.States[stateIndex].Name : "");
                        RemoveTransitions.Add(TargetStateMachine.States[sourceStateIndex], TargetStateMachine.Transitions[transitionIndices[j]]);
                    }
                }
            }
            foreach (var pair in RemoveTransitions) {
                RemoveTransition(pair.Key, pair.Value);
            }
        }

        private void InitializeLayer() {
            if (TargetStateMachine.Layers == null || TargetStateMachine.Layers.Length == 0) {
                AddLayer("Base");
            }
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                ValidateLayer(TargetStateMachine.Layers[i]);
                TargetStateMachine.Layers[i].UpdateReferences(TargetStateMachine);
            }
        }

        protected Vector2 GetGlobalPosition(Vector2 position) {
            if (SnapPosition) {
                return GetSnappedPosition(position);
            }
            return position;
        }

        private void OpenWindowGUI() {
            StateMachineEditorWindow.StateMachineEditor = this;
            EditorWindow.FocusWindowIfItsOpen<StateMachineEditorWindow>();
        }

        public void OnWindowFocus() {
            WindowInFocus = true;
        }

        public void OnWindowLostFocus() {
            WindowInFocus = false;
        }

        protected virtual void OnPreInitialize() {
        }

        protected virtual void OnPostInitialize() {
        }

        protected virtual void OnEnable() {
            TargetStateMachine = target as StateMachine;
            ActiveLayer = null;
            ActiveState = null;
            ActiveDragState = null;
            OverState = null;
            ActiveTransition = null;
            ActiveTransitionInGUI = null;
            ActiveTransitionState = null;
            OverTransition = null;
            OverTransitionState = null;
            TransitionFromState = null;
            ActiveLayerIndex = -1;
            OnPreInitialize();
            InitializeAttributes();
            InitializeEnums();
            InitializeLayer();
            OnPostInitialize();
            SetActiveLayer(EditorPrefs.GetInt(EditorLayerIndexKey, 0));
            Zoom = EditorPrefsZoom;
            PositionOffset = EditorPrefsPositionOffset;
            OpenWindowGUI();
        }

        protected virtual void OnDisable() {
            TargetStateMachine = null;
            StateMachineEditorWindow.StateMachineEditor = null;
            EditorPrefsZoom = Zoom;
            EditorPrefsPositionOffset = PositionOffset;
        }

        new void SetDirty() {
            EditorUtility.SetDirty(TargetStateMachine);
        }

        protected int FindParameterIndex(Parameter parameter) {
            return parameter != null ? ArrayUtility.FindIndex(TargetStateMachine.Parameters, p => p == parameter) : -1;
        }

        protected int FindStateIndex(State state) {
            return state != null ? ArrayUtility.FindIndex(TargetStateMachine.States, s => s == state) : -1;
        }

        protected virtual void OnUpdateStateIndices(Layer layer, State state) {
        }

        private void UpdateStateAndParameterIndices() {
            for (int l = 0; l < TargetStateMachine.Layers.Length; l++) {
                Layer layer = TargetStateMachine.Layers[l];
                layer.DefaultStateIndex = FindStateIndex(layer.DefaultState);
                for (int i = 0; i < layer.StateIndices.Length; i++) {
                    State state = TargetStateMachine.States[layer.StateIndices[i]];
                    OnUpdateStateIndices(layer, state);
                    for (int j = 0; j < state.TransitionIndices.Length; j++) {
                        Transition transition = TargetStateMachine.Transitions[state.TransitionIndices[j]];
                        transition.DestinationStateIndex = FindStateIndex(transition.DestinationState);
                        for (int k = 0; k < transition.Conditions.Length; k++) {
                            Condition condition = transition.Conditions[k];
                            condition.ParameterIndex = FindParameterIndex(condition.Parameter);
                            condition.ValueIndex = FindParameterIndex(condition.ValueParameter);
                        }
                    }
                }
            }
        }

        private void SetActiveLayer(int index) {
            if (index == ActiveLayerIndex) {
                return;
            }
            if (index != EditorPrefs.GetInt(EditorLayerIndexKey, 0)) {
                EditorPrefs.SetInt(EditorLayerIndexKey, index);
            }
            if (index < 0 || index >= TargetStateMachine.Layers.Length) {
                index = 0;
            }
            ActiveLayerIndex = index;
            ActiveLayer = TargetStateMachine.Layers[index];
            ActiveState = null;
            ActiveDragState = null;
            ActiveTransition = null;
            OverState = null;
            OverTransition = null;
            ActiveTransitionInGUI = null;
            SetDirty();
        }

        private void SwapLayer(int layerA, int layerB) {
            Layer layer = TargetStateMachine.Layers[layerA];
            TargetStateMachine.Layers[layerA] = TargetStateMachine.Layers[layerB];
            TargetStateMachine.Layers[layerB] = layer;
            Parameter parameter = TargetStateMachine.Parameters[layerA];
            TargetStateMachine.Parameters[layerA] = TargetStateMachine.Parameters[layerB];
            TargetStateMachine.Parameters[layerB] = parameter;
            UpdateStateAndParameterIndices();
        }

        private Vector2 GetSnappedPosition(Vector2 position) {
            return new Vector2(Mathf.Round(position.x / SnapSize) * SnapSize, Mathf.Round(position.y / SnapSize) * SnapSize);
        }

        private void ClearAttributeFromAllLayers(int attributeIndex) {
            int attributeMask = 1 << attributeIndex;
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                Layer layer = TargetStateMachine.Layers[i];
                for (int j = 0; j < layer.StateIndices.Length; j++) {
                    State state = TargetStateMachine.States[layer.StateIndices[j]];
                    state.AttributeMask &= ~attributeMask;
                }
            }
        }

        private void ShowAtributeOnAllLayers(string attributeName, int attributeIndex) {
            int attributeMask = 1 << attributeIndex;
            string states = string.Empty;
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                Layer layer = TargetStateMachine.Layers[i];
                for (int j = 0; j < layer.StateIndices.Length; j++) {
                    State state = TargetStateMachine.States[layer.StateIndices[j]];
                    if ((state.AttributeMask & attributeMask) > 0) {
                        states += string.Format("{0}\\{1}\n", layer.Name, state.Name);
                    }
                }
            }
            if (states.Length > 0) {
                EditorUtility.DisplayDialog(string.Format("Show states with attribute {0} (Bit {1})", attributeName, attributeIndex), states, "Ok");
            }
        }

        private void ShowParametersOnAllLayers(int parameterIndex, string parameterName) {
            string states = string.Empty;
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                Layer layer = TargetStateMachine.Layers[i];
                for (int j = 0; j < layer.StateIndices.Length; j++) {
                    State state = TargetStateMachine.States[layer.StateIndices[j]];
                    for (int k = 0; k < state.TransitionIndices.Length; k++) {
                        Transition trans = TargetStateMachine.Transitions[state.TransitionIndices[k]];
                        for (int l = 0; l < trans.Conditions.Length; l++) {
                            Condition cond = trans.Conditions[l];
                            if (cond.ParameterIndex == parameterIndex) {
                                states += string.Format("{0}\\{1}\n", layer.Name, state.Name);
                            }
                        }
                    }
                }
            }
            if (states.Length > 0) {
                EditorUtility.DisplayDialog(string.Format("Show states with parameter {0}", parameterName), states, "Ok");
            }
        }

        private void ShowEnumsOnAllLayers(StateMachineInternal.Enum Enum, int value, string desc) {
            string states = string.Empty;
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                Layer layer = TargetStateMachine.Layers[i];
                for (int j = 0; j < layer.StateIndices.Length; j++) {
                    State state = TargetStateMachine.States[layer.StateIndices[j]];
                    for (int k = 0; k < state.TransitionIndices.Length; k++) {
                        Transition trans = TargetStateMachine.Transitions[state.TransitionIndices[k]];
                        for (int l = 0; l < trans.Conditions.Length; l++) {
                            Condition cond = trans.Conditions[l];
                            if (cond.Parameter.IsEnum && 
                                cond.Parameter.Name == Enum.Name && 
                                cond.ValueInt == value) {
                                states += string.Format("{0}\\{1}\n", layer.Name, state.Name);
                            }
                        }
                    }
                }
            }
            if (states.Length > 0) {
                EditorUtility.DisplayDialog(string.Format("Show states with enum {0} value {1}", Enum.Name, desc), states, "Ok");
            }
        }


        private Parameter AddParameter(string name, int insertIndex) {
            Parameter parameter = new Parameter();
            parameter.Name = name;
            if (insertIndex == -1) {
                ArrayUtility.Add(ref TargetStateMachine.Parameters, parameter);
            } else {
                ArrayUtility.Insert(ref TargetStateMachine.Parameters, insertIndex, parameter);
            }
            UpdateStateAndParameterIndices();
            SetDirty();
            return parameter;
        }

        private void RemoveParameter(Parameter parameter) {
            TraverseAllConditions((State state, Transition transition, Condition condition) => {
                if (transition.Conditions.Length <= 1) {
                    return true;
                }
                if (condition.Parameter == parameter) {
                    return false;
                }
                return true;
            });
            ArrayUtility.Remove(ref TargetStateMachine.Parameters, parameter);
            UpdateStateAndParameterIndices();
            SetDirty();
        }

        private void SwapParameter(int parameterA, int parameterB) {
            Parameter parameter = TargetStateMachine.Parameters[parameterA];
            TargetStateMachine.Parameters[parameterA] = TargetStateMachine.Parameters[parameterB];
            TargetStateMachine.Parameters[parameterB] = parameter;
            UpdateStateAndParameterIndices();
        }

        private string GetLayerParameterName(string name, string parameterName) {
            return string.Format("{0} {1}", name, parameterName);
        }

        private StateMachineInternal.Enum AddEnum(string name, int insertIndex) {
            StateMachineInternal.Enum enumList = new StateMachineInternal.Enum();
            enumList.Name = name;
            enumList.Strings = new string[1];
            if (insertIndex == -1) {
                ArrayUtility.Add(ref TargetStateMachine.Enums, enumList);
            } else {
                ArrayUtility.Insert(ref TargetStateMachine.Enums, insertIndex, enumList);
            }
            SetDirty();
            return enumList;
        }

        private void RemoveEnum(int index) {
            TraverseAllConditions((state, transition, condition) => {
                if (!condition.Parameter.IsEnum) {
                    return true;
                }
                if (condition.Parameter.ParameterIndex != index) {
                    return true;
                }
                condition.ValueInt = 0;
                return true;
            });
            for (int i = 0; i < TargetStateMachine.Parameters.Length; i++) {
                Parameter parameter = TargetStateMachine.Parameters[i];
                if (!parameter.IsEnum) {
                    continue;
                }
                if (parameter.ParameterIndex >= index) {
                    parameter.ParameterIndex = Mathf.Max(parameter.ParameterIndex - 1, 0);
                }
            }
            ArrayUtility.RemoveAt(ref TargetStateMachine.Enums, index);
            SetDirty();
        }

        private void AddEnumString(int enumIndex, StateMachineInternal.Enum Enum, string enumString, int insertIndex) {
            if (insertIndex != -1) {
                TraverseAllConditions((state, transition, condition) => {
                    if (!condition.Parameter.IsEnum) {
                        return true;
                    }
                    if (condition.Parameter.ParameterIndex != enumIndex) {
                        return true;
                    }
                    if (condition.ValueInt >= insertIndex) {
                        condition.ValueInt++;
                    }
                    return true;
                });
                for (int i = 0; i < TargetStateMachine.Parameters.Length; i++) {
                    Parameter parameter = TargetStateMachine.Parameters[i];
                    if (!parameter.IsEnum) {
                        continue;
                    }
                    if (parameter.ParameterIndex != enumIndex) {
                        continue;
                    }
                    if (parameter.ValueInt >= insertIndex) {
                        parameter.ValueInt++;
                    }
                }
                ArrayUtility.Insert(ref Enum.Strings, insertIndex, enumString);
            } else {
                ArrayUtility.Add(ref Enum.Strings, enumString);
            }
        }

        private void RemoveEnumString(int enumIndex, StateMachineInternal.Enum Enum, int index) {
            TraverseAllConditions((state, transition, condition) => {
                if (!condition.Parameter.IsEnum) {
                    return true;
                }
                if (condition.Parameter.ParameterIndex != enumIndex) {
                    return true;
                }
                if (condition.ValueInt >= index) {
                    condition.ValueInt = Mathf.Max(condition.ValueInt - 1, 0);
                }
                return true;
            });
            for (int i = 0; i < TargetStateMachine.Parameters.Length; i++) {
                Parameter parameter = TargetStateMachine.Parameters[i];
                if (!parameter.IsEnum) {
                    continue;
                }
                if (parameter.ParameterIndex != enumIndex) {
                    continue;
                }
                if (parameter.ValueInt >= index) {
                    parameter.ValueInt = Mathf.Max(parameter.ValueInt - 1, 0);
                }
            }
            ArrayUtility.RemoveAt(ref Enum.Strings, index);
        }

        private void SwapEnumString(int enumIndex, StateMachineInternal.Enum Enum, int enumA, int enumB) {
            TraverseAllConditions((state, transition, condition) => {
                if (!condition.Parameter.IsEnum) {
                    return true;
                }
                if (condition.Parameter.ParameterIndex != enumIndex) {
                    return true;
                }
                if (condition.ValueInt == enumA) {
                    condition.ValueInt = enumB;
                } else if (condition.ValueInt == enumB) {
                    condition.ValueInt = enumA;
                }
                return true;
            });
            for (int i = 0; i < TargetStateMachine.Parameters.Length; i++) {
                Parameter parameter = TargetStateMachine.Parameters[i];
                if (!parameter.IsEnum) {
                    continue;
                }
                if (parameter.ParameterIndex != enumIndex) {
                    continue;
                }
                if (parameter.ValueInt == enumA) {
                    parameter.ValueInt = enumB;
                } else if (parameter.ValueInt == enumB) {
                    parameter.ValueInt = enumA;
                }
            }
            string enumStringA = Enum.Strings[enumA];
            Enum.Strings[enumA] = Enum.Strings[enumB];
            Enum.Strings[enumB] = enumStringA;
        }

        protected virtual void OnAddLayer(Layer layer) {
        }

        private Layer AddLayer(string name) {
            // Add layer
            if (TargetStateMachine.Layers == null) {
                TargetStateMachine.Parameters = new Parameter[0];
                TargetStateMachine.Transitions = new Transition[0];
            }
            Layer layer = AddLayerInternal();
            layer.Name = name;
            layer.StateIndices = new int[0];
            // Add layer built-in parameters
            for (int i = 0; i < BuildInParameterNames.Length; i++) {
                Parameter timeParameter = AddParameter(GetLayerParameterName(name, BuildInParameterNames[i]), (TargetStateMachine.Layers.Length - 1) * BuildInParameterNames.Length + i);
                timeParameter.ParameterType = BuildInParameterTypes[i];
            }
            OnAddLayer(layer);
            // Add any state
            AddState(layer, "Any State", GetSnappedPosition(StateSize));
            // Add default state
            AddState(layer, NewStateName, GetSnappedPosition(StateSize + new Vector2(0, StateSize.y * 3.0f)));
            SetDirty();
            return layer;
        }

        private void RemoveLayer(int index) {
            Layer layer = TargetStateMachine.Layers[index];
            // Remove states
            while (layer.StateIndices.Length > 0) {
                RemoveState(layer, TargetStateMachine.States[layer.StateIndices[0]]);
            }
            // Remove layer
            RemoveLayerInternal(layer);
            // Remove layer built-in parameters
            for (int i = 0; i < BuildInParameterNames.Length; i++) {
                RemoveParameter(TargetStateMachine.Parameters[index * BuildInParameterNames.Length]);
            }
            SetDirty();
        }

        private void RenameLayer(string name) {
            ActiveLayer.Name = GetUniqueName(TargetStateMachine.Layers, name);
            // Rename layer built-in parameters
            for (int i = 0; i < BuildInParameterNames.Length; i++) {
                TargetStateMachine.Parameters[ActiveLayerIndex * BuildInParameterNames.Length + i].Name = GetLayerParameterName(ActiveLayer.Name, BuildInParameterNames[i]);
            }
        }

        protected delegate void TraverseLayerCallback(Layer layer);

        protected void TraverseAllLayers(TraverseLayerCallback callback) {
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                Layer layer = TargetStateMachine.Layers[i];
                callback(layer);
            }
        }

        protected delegate void TraverseStateCallback(State state);

        protected void TraverseAllStates(TraverseStateCallback callback) {
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                Layer layer = TargetStateMachine.Layers[i];
                for (int j = 0; j < layer.StateIndices.Length; j++) {
                    State state = TargetStateMachine.States[layer.StateIndices[j]];
                    callback(state);
                }
            }
        }

        protected delegate bool TraverseConditionCallback(State state, Transition transition, Condition condition);

        protected void TraverseAllConditions(TraverseConditionCallback callback) {
            for (int i = 0; i < TargetStateMachine.Layers.Length; i++) {
                Layer layer = TargetStateMachine.Layers[i];
                for (int j = 0; j < layer.StateIndices.Length; j++) {
                    State state = TargetStateMachine.States[layer.StateIndices[j]];
                    for (int k = 0; k < state.TransitionIndices.Length; k++) {
                        Transition transition = TargetStateMachine.Transitions[state.TransitionIndices[k]];
                        for (int l = 0; l < transition.Conditions.Length; l++) {
                            Condition condition = transition.Conditions[l];
                            if (condition.Parameter == null) {
                                condition.Parameter = TargetStateMachine.Parameters[0];
                                continue;
                            }
                            if (!callback(state, transition, condition)) {
                                ArrayUtility.RemoveAt(ref transition.Conditions, l--);
                            }
                        }
                    }
                }
            }
        }

        private void SetDefaultState(Layer layer, State state) {
            layer.DefaultState = state;
            layer.DefaultStateIndex = FindStateIndex(state);
            SetDirty();
        }

        protected virtual void OnAddState(State state) {
        }

        private void AddState(Layer layer, string name, Vector2 position) {
            State state = AddStateInternal();
            state.Name = name;
            state.EditorPosition = position;
            state.TransitionIndices = new int[0];
            OnAddState(state);
            int index = FindStateIndex(state);
            ArrayUtility.Add(ref layer.StateIndices, index);
            if (layer.StateIndices.Length > 1 && layer.DefaultState == null) {
                SetDefaultState(layer, state);
            }
            SetDirty();
        }

        private void RemoveState(Layer layer, State state) {
            // Remove transition to this state
            for (int i = 0; i < layer.StateIndices.Length; i++) {
                State layerState = TargetStateMachine.States[layer.StateIndices[i]];
                for (int j = 0; j < layerState.TransitionIndices.Length; j++) {
                    Transition transition = TargetStateMachine.Transitions[layerState.TransitionIndices[j]];
                    if (transition.DestinationState == state) {
                        RemoveTransition(layerState, transition);
                        i--;
                    }
                }
            }
            // Remove transitions from this state
            while (state.TransitionIndices.Length > 0) {
                RemoveTransition(state, TargetStateMachine.Transitions[state.TransitionIndices[0]]);
            }
            // Fix all state indices
            int index = FindStateIndex(state);
            ArrayUtility.Remove(ref layer.StateIndices, index);
            RemoveStateInternal(state);
            TraverseAllLayers(delegate (Layer targetLayer) {
                for (int j = 0; j < targetLayer.StateIndices.Length; j++) {
                    if (targetLayer.StateIndices[j] > index) {
                        targetLayer.StateIndices[j]--;
                    }
                }
            });
            // Fix default state index
            if (layer.DefaultStateIndex == index) {
                layer.DefaultState = null;
                layer.DefaultStateIndex = -1;
                if (layer.StateIndices.Length > 1) {
                    SetDefaultState(layer, TargetStateMachine.States[layer.StateIndices[1]]);
                }
            }
            UpdateStateAndParameterIndices();
            SetDirty();
        }

        Condition AddCondition(Transition transition, Parameter parameter) {
            Condition condition = new Condition();
            SetConditionParameter(condition, parameter);
            ArrayUtility.Add(ref transition.Conditions, condition);
            SetDirty();
            return condition;
        }

        private void RemoveCondition(Transition transition, Condition condition) {
            ArrayUtility.Remove(ref transition.Conditions, condition);
            SetDirty();
        }

        private void SetConditionParameter(Condition condition, Parameter parameter) {
            condition.Parameter = parameter;
            condition.ParameterIndex = parameter != null ? TargetStateMachine.FindParameterIndex(parameter.Name) : -1;
            SetDirty();
        }

        private void AddTransition(Layer layer, State state, State destinationState, Parameter defaultParameter) {
            Transition transition = new Transition();
            transition.DestinationState = destinationState;
            transition.DestinationStateIndex = FindStateIndex(destinationState);
            transition.Conditions = new Condition[0];
            transition.AnimationCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            // Set condition default
            Condition condition = AddCondition(transition, defaultParameter);
            condition.ConditionMode = ConditionMode.GreaterOrEqual;
            condition.ValueFloat = 1.0f;
            ArrayUtility.Add(ref TargetStateMachine.Transitions, transition);
            ArrayUtility.Add(ref state.TransitionIndices, TargetStateMachine.Transitions.Length - 1);
            SetDirty();
        }

        private void RemoveTransition(State state, Transition transition) {
            for (int i = 0; i < state.TransitionIndices.Length; i++) {
                int index = state.TransitionIndices[i];
                Transition stateTransition = TargetStateMachine.Transitions[index];
                if (stateTransition == transition) {
                    // Remove transition from global array
                    ArrayUtility.Remove(ref state.TransitionIndices, index);
                    ArrayUtility.Remove(ref TargetStateMachine.Transitions, transition);
                    // Traverse all states and fix indices
                    TraverseAllStates(delegate (State traverseState) {
                        for (int j = 0; j < traverseState.TransitionIndices.Length; j++) {
                            if (traverseState.TransitionIndices[j] > index) {
                                traverseState.TransitionIndices[j]--;
                            }
                        }
                    });
                    SetDirty();
                    return;
                }
            }
        }

        protected string GetUniqueName<T>(T[] arrayOfT, string name) where T : Base {
            for (int i = 0; i < arrayOfT.Length; i++) {
                if (arrayOfT[i].Name == name) {
                    string nameCopy = name;
                    int index = name.LastIndexOf(" ");
                    int value = 0;
                    if (index != -1) {
                        try {
                            string number = name.Substring(index + 1);
                            value = System.Convert.ToInt32(number) + 1;
                            nameCopy = name.Remove(index + 1).Trim();
                        } catch (System.FormatException) {
                            value = 0;
                        }
                    }
                    return GetUniqueName<T>(arrayOfT, string.Format("{0} {1}", nameCopy, value));
                }
            }
            return name.Trim();
        }

        protected string GetUniqueName<T>(int[] indices, T[] arrayOfT, string name) where T : Base {
            for (int i = 0; i < indices.Length; i++) {
                if (arrayOfT[indices[i]].Name == name) {
                    string nameCopy = name;
                    int index = name.LastIndexOf(" ");
                    int value = 0;
                    if (index != -1) {
                        try {
                            string number = name.Substring(index + 1);
                            value = System.Convert.ToInt32(number) + 1;
                            nameCopy = name.Remove(index + 1).Trim();
                        } catch (System.FormatException) {
                            value = 0;
                        }
                    }
                    return GetUniqueName<T>(indices, arrayOfT, string.Format("{0} {1}", nameCopy, value));
                }
            }
            return name.Trim();
        }

        protected bool IsAnyState(Layer layer, State state) {
            return TargetStateMachine.States[layer.StateIndices[0]] == state;
        }

        protected bool IsDefaultState(Layer layer, State state) {
            return TargetStateMachine.States[layer.DefaultStateIndex] == state;
        }

        private void BeginEdit() {
            EditorGUI.BeginChangeCheck();
        }

        private bool EndEdit(bool force = false) {
            if (Application.isPlaying) {
                return false;
            }
            if (force || EditorGUI.EndChangeCheck()) {
                EditorUtility.SetDirty(TargetStateMachine);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                return true;
            }
            return false;
        }

        private Bounds GetLayerEditorPositionBounds(Layer layer) {
            Bounds bounds = new Bounds();
            for (int i = 0; i < layer.StateIndices.Length; ++i) {
                State state = TargetStateMachine.States[layer.StateIndices[i]];
                if (i == 0) {
                    bounds.SetMinMax((Vector3) state.EditorPosition, (Vector3) state.EditorPosition);
                } else {
                    bounds.Encapsulate((Vector3) state.EditorPosition);
                }
            }
            return bounds;
        }

        public static string GetGUID(Transform t) {
            string guid;
            if (PrefabUtility.GetPrefabType(t) == PrefabType.Prefab) {
                guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(t));
            } else {
                guid = AssetDatabase.AssetPathToGUID(t.gameObject.scene.path) + "|" + GetSceneObjectPath(t);
            }
            return guid;
        }

        public static string GetSceneObjectPath(Transform transform) {
            var sb = new System.Text.StringBuilder();
            var t = transform;
            List<Transform> transforms = new List<Transform>() {
                t
            };

            while (t.parent != null) {
                t = t.parent;
                transforms.Add(t);
            }

            for (int i = transforms.Count - 1; i >= 0; --i) {
                sb.Append(transforms[i].name);
                if (i != 0) {
                    sb.Append("/");
                }
            }
            return sb.ToString();
        }
    }

}
