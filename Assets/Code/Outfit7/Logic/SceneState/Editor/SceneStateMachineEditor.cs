using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Outfit7.Util;
using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Logic.SceneStateMachineInternal {

    [CustomEditor(typeof(SceneStateMachine), true)]
    public class SceneStateMachineEditor : StateMachineEditor {

        private string[] ScenePaths;
        private List<MonoScript> SceneStateBehaviourMonoScripts;
        private SceneStateMachine SceneStateMachine;
        private UnityEditorInternal.ReorderableList ScenePathsReorderableList = null;

        public override string TitleName { get { return "Scene State Machine"; } }

        protected override Color CustomColor { get { return Color.yellow; } }

        protected override Vector2 StateSize {
            get {
                return new Vector2(180, 60);
            }
        }

        protected override float SnapSize {
            get {
                return StateSize.y;
            }
        }

        private static List<MonoScript> GetScriptAssetsOfType<T>() {
            MonoScript[] scripts = (MonoScript[]) Resources.FindObjectsOfTypeAll(typeof(MonoScript));         
            List<MonoScript> result = new List<MonoScript>();         
            for (int i = 0; i < scripts.Length; ++i) {
                MonoScript m = scripts[i];
                if (m.GetClass() != null && m.GetClass().IsSubclassOf(typeof(T))) {
                    result.Add(m);
                }
            }
            return result;
        }

        private void FillScripts() {
            SceneStateBehaviourMonoScripts = GetScriptAssetsOfType<SceneStateBehaviour>();
        }

        private void FillScenes() {
            // Fill scenes
            List<string> enabledScenePaths = new List<string>();
            for (int i = 0; i < UnityEditor.EditorBuildSettings.scenes.Length; ++i) {
                EditorBuildSettingsScene scene = UnityEditor.EditorBuildSettings.scenes[i];
                if (!scene.enabled) {
                    continue;
                }
                string name = SceneStateMachine.GetScenePath(scene.path);
                enabledScenePaths.Add(name);
            }
            if (enabledScenePaths.Count <= 1) {
                ScenePaths = new string[0];
                return;
            }
            ScenePaths = new string[enabledScenePaths.Count - 1];
            for (int i = 0; i < ScenePaths.Length; ++i) {
                ScenePaths[i] = enabledScenePaths[i + 1];
            }
        }

        protected override Layer AddLayerInternal() {
            SceneStateMachine sceneStateMachine = target as SceneStateMachine;
            return AddGeneric(ref sceneStateMachine.InternalLayers);
        }

        protected override void RemoveLayerInternal(Layer layer) {
            SceneStateMachine sceneStateMachine = target as SceneStateMachine;
            RemoveGeneric(ref sceneStateMachine.InternalLayers, layer as SceneStateMachineLayer);
        }

        protected override State AddStateInternal() {
            SceneStateMachine sceneStateMachine = target as SceneStateMachine;
            return AddGeneric(ref sceneStateMachine.InternalStates);
        }

        protected override void RemoveStateInternal(State state) {
            SceneStateMachine sceneStateMachine = target as SceneStateMachine;
            RemoveGeneric(ref sceneStateMachine.InternalStates, state as SceneStateMachineState);
        }

        protected override void OnUpdateStateIndices(Layer layer, State state) {
            SceneStateMachineState sceneState = state as SceneStateMachineState;
            sceneState.ParameterLoadMaskIndex = FindParameterIndex(sceneState.ParameterLoadMask);
        }

        protected override void OnSelectState() {
            base.OnSelectState();
            SceneStateMachineState sceneState = ActiveState as SceneStateMachineState;
            ScenePathsReorderableList = new UnityEditorInternal.ReorderableList(sceneState.ScenePaths, typeof(SceneStateSceneEntry), true, false, true, true);
            ScenePathsReorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                if (index < 0 || index >= ScenePathsReorderableList.list.Count) {
                    return;
                }
                int scenePathIndex = EditorGUI.Popup(rect, ArrayUtility.FindIndex(ScenePaths, p => p == sceneState.ScenePaths[index].ScenePath), ScenePaths);
                sceneState.ScenePaths[index].ScenePath = scenePathIndex == -1 ? string.Empty : ScenePaths[scenePathIndex];
            };
        }

        protected override void OnEnable() {
            base.OnEnable();
            SceneStateMachine = TargetStateMachine as SceneStateMachine;
            // Force disable bucket
            if (!Application.isPlaying) {
                SceneStateMachine.BucketIndex = -1;
            }
            FillScenes();
            FillScripts();
        }

        protected override void OnDrawState(State state, Rect rect) {
            base.OnDrawState(state, rect);
            SceneStateMachineState sceneState = state as SceneStateMachineState;
            SceneStateMachineLayer sceneLayer = ActiveLayer as SceneStateMachineLayer;
            if (sceneState.SceneStateLoadMode == SceneStateLoadMode.Async) {
                Rect asyncRect = rect;
                PushColor(Color.black);
                EditorGUI.LabelField(asyncRect, "Async");
                PopColor();
            }
            if (!Application.isPlaying) {
                return;
            }
            if (state.Switch) {
                return;
            }
            bool isLoaded = SceneStateManager.Instance.GetRegisteredState(sceneState.SceneStateScriptName) || sceneState.SceneStateBehaviour != null;
            rect.y += StateSize.y * 0.5f;
            PushColor(isLoaded ? Color.green : Color.yellow);
            EditorGUI.LabelField(rect, isLoaded ? "Loaded" : "Not loaded", CenteredLabelStyle);
            PopColor();
            if (sceneLayer.CurrentState == state && sceneLayer.Progress < 1.0f) {
                rect.y += StateSize.y * 0.2f;
                DrawProgress(rect, sceneLayer.Progress);
            }
        }

        protected override void OnLayerCustomGUI() {
            base.OnLayerCustomGUI();
        }

        protected override void OnStateCustomGUI() {
            base.OnStateCustomGUI();
            SceneStateMachineState state = ActiveState as SceneStateMachineState;
            if (IsDefaultState(ActiveLayer, ActiveState) && ActiveLayerIndex == 0) {
                state.SceneStateType = SceneStateType.Scene;
                state.SceneStateLoadMode = SceneStateLoadMode.Default;
            } else {
                state.SceneStateType = (SceneStateType) EditorGUILayout.EnumPopup("Type", state.SceneStateType);
                state.SceneStateLoadMode = (SceneStateLoadMode) EditorGUILayout.EnumPopup("Load Mode", state.SceneStateLoadMode);
            }
            state.SceneStateFlags = (SceneStateFlags) EditorGUILayout.EnumMaskPopup(new GUIContent("Flags"), state.SceneStateFlags);
            switch (state.SceneStateType) {
                case SceneStateType.Scene:
                    // Select scene
                    ScenePathsReorderableList.DoLayoutList();
                    // Select script
                    MonoScript monoScript = SceneStateBehaviourMonoScripts.Find(ms => ms.name == state.SceneStateScriptName || ms.GetClass().FullName == state.SceneStateScriptName);
                    monoScript = (MonoScript) EditorGUILayout.ObjectField("SceneStateBehaviour", monoScript, typeof(MonoScript), false);
                    state.SceneStateScriptName = monoScript != null ? monoScript.GetClass().FullName : string.Empty;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Parameter Load Mask");
                    int index;
                    string[] parameterNamesWithNone = GetParameterNames(p => p.IsIndexer, true);
                    if (ParameterGUI(state.ParameterLoadMask, parameterNamesWithNone, out index)) {
                        if (index == 0) {
                            state.ParameterLoadMask = null;
                            state.ParameterLoadMaskIndex = -1;
                            ;
                        } else {
                            state.ParameterLoadMaskIndex = TargetStateMachine.FindParameterIndex(parameterNamesWithNone[index]);
                            state.ParameterLoadMask = TargetStateMachine.Parameters[state.ParameterLoadMaskIndex];
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    state.CanHaveEmptySceneToStillBeValid = EditorGUILayout.ToggleLeft("Can Have Empty Scene To Still Be Valid", state.CanHaveEmptySceneToStillBeValid);

                    // Reset
                    state.Prefab = new AssetReference();
                    break;
                case SceneStateType.Prefab:
                    // Select prefab
                    AssetReferenceEditor.Field("Prefab", state.Prefab, typeof(GameObject));
                    // Reset
                    state.ScenePaths.Clear();
                    state.SceneStateScriptName = string.Empty;
                    break;
            }
        }
    }

}