using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;
using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Logic.SceneStateMachineInternal {

    [Serializable]
    public class SceneStateMachineLayer : Layer {

        private enum TransitionType {
            None,
            EmptyState,
            ValidState,
        }

        public enum StateChangedFlags : long {
            None = 0,
            Unloaded = 1 << 0,
        }

        public enum StateChangedState {
            Invalid,
            PreEnter,
            PostEnter,
            PreExit,
            PostExit,
        }

        private SceneStateMachineState LastSceneState;
        private List<SceneStateMachineState> StateSceneLoading = new List<SceneStateMachineState>();
        private object TransitionUserData = null;
        private TransitionType CurrentTransitionType = TransitionType.None;

        public SceneStateMachine SceneStateMachine;

        // TODO: call this multiple times between events and set state
        public delegate void StateChangedDelegate(SceneStateMachineState previousState, SceneStateMachineState currentState, StateChangedFlags flags, StateChangedState state);

        public StateChangedDelegate OnStateChanged;

        public float Progress { get; private set; }

        public bool InTransition { get { return CurrentTransitionType != TransitionType.None; } }

        private void StartPreload(StateMachine stateMachine, SceneStateMachineState currentSceneState) {
            if (!currentSceneState.StartPreload) {
                return;
            }
            for (int i = 0; i < currentSceneState.TransitionIndices.Length; ++i) {
                SceneStateMachineState sceneState = stateMachine.Transitions[currentSceneState.TransitionIndices[i]].DestinationState as SceneStateMachineState;
                if (sceneState.Switch) {
                    StartPreload(stateMachine, sceneState);
                    continue;
                }
                if (!sceneState.Preload) {
                    continue;
                }
                // Check if this scene is already preloading
                if (sceneState.SceneStateType == SceneStateType.Scene) {
                    bool valid = true;
                    for (int j = 0; j < StateSceneLoading.Count; ++j) {
                        if (StateSceneLoading[j].ScenePaths[0].ScenePath == sceneState.ScenePaths[0].ScenePath) {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) {
                        continue;
                    }
                    // Add to loading
                    StateSceneLoading.Add(sceneState);
                }
                // Load
                sceneState.LoadState(SceneStateMachine, true);
            }
        }

        private void LoadTransitionState(SceneStateMachineState sceneState) {

            // Load scene if not registered yet
            if (!sceneState.IsLoaded && !sceneState.Loading) {
                sceneState.LoadState(SceneStateMachine, false);
                Progress = 0.0f;
            } else {
                Progress = 1.0f;
            }
            // Set transition
            if (sceneState.SceneStateType == SceneStateType.Scene && sceneState.ScenePaths.Count == 0 && !sceneState.CanHaveEmptySceneToStillBeValid) {
                CurrentTransitionType = TransitionType.EmptyState;

            } else {
                CurrentTransitionType = TransitionType.ValidState;
            }
            // If we have a state behaviour, trigger transition
            if (LastSceneState != null && LastSceneState.SceneStateBehaviour) {
                LastSceneState.SceneStateBehaviour.TransitionInternal(sceneState.SceneStateBehaviourType);
            }
        }

        private void EnterState(SceneStateMachineState previousStateState, SceneStateMachineState currentSceneState, int layerIndex) {
            if (currentSceneState.SceneStateBehaviourType == null) {
                return;
            }
            Assert.IsTrue(currentSceneState.SceneStateBehaviour != null);
            // Enter
            O7Log.DebugT(SceneStateMachine.Tag, "State Enter {0}.{1}", SceneStateMachine.Layers[layerIndex].Name, currentSceneState.Name);
            // Activate
            if (!currentSceneState.SceneStateBehaviour.gameObject.activeSelf) {
                currentSceneState.SceneStateBehaviour.gameObject.SetActive(true);
            }
            // Call enter
            currentSceneState.SceneStateBehaviour.InitializePersistentData(ref currentSceneState.PersistentData);
            currentSceneState.SceneStateBehaviour.StateEnterInternal(currentSceneState, previousStateState != null ? previousStateState.SceneStateBehaviourType : null, TransitionUserData, layerIndex);
            // Reset user data
            TransitionUserData = null;
        }

        private void ExitState(SceneStateMachineState previousSceneState, SceneStateMachineState currentSceneState, int layerIndex) {
            // Check if we can exit previous state
            if (previousSceneState == null || previousSceneState.SceneStateBehaviour == null) {
                return;
            }
            // Exit
            O7Log.DebugT(SceneStateMachine.Tag, "State Exit {0}.{1}", SceneStateMachine.Layers[layerIndex].Name, previousSceneState.Name);
            // Call exit
            previousSceneState.SceneStateBehaviour.StateExitInternal(currentSceneState.SceneStateBehaviourType);
            // Unload
            bool unloadedCompletely;
            previousSceneState.UnloadState(SceneStateMachine, currentSceneState, out unloadedCompletely);
            if (OnStateChanged != null) {
                OnStateChanged(previousSceneState, currentSceneState, unloadedCompletely ? StateChangedFlags.Unloaded : 0, StateChangedState.PostExit);
            }
            LastSceneState = null;
        }

        // Layer
        protected override bool OnTransition(StateMachine stateMachine, Parameter timeParameter, Transition transition, int index) {
            SceneStateMachineState nextSceneState = transition.DestinationState as SceneStateMachineState;
            // If a switch return
            if (!nextSceneState.Switch) {
                // Reset user data
                TransitionUserData = null;
                // Check for user data
                for (int i = 0; i < transition.Conditions.Length; ++i) {
                    // Take first user data, Assert on multiple maybe?
                    if (transition.Conditions[i].Parameter.UserData != null) {
                        TransitionUserData = transition.Conditions[i].Parameter.UserData;
                        break;
                    }
                }
                // Load state
                LoadTransitionState(nextSceneState);
                MessageEventManager.Instance.LockGlobal(stateMachine.GlobalMessageEventLockIndex);
            }
            base.OnTransition(stateMachine, timeParameter, transition, index);
            return nextSceneState.Switch ? true : false;
        }

        private void DoStateChange(StateMachine stateMachine, SceneStateMachineState previousSceneState, SceneStateMachineState currentSceneState, int index) {
            SceneStateMachine sceneStateMachine = stateMachine as SceneStateMachine;
            if (sceneStateMachine.OnStateChange != null) {
                sceneStateMachine.OnStateChange(previousSceneState != null ? previousSceneState.SceneStateBehaviourType : null, currentSceneState != null ? currentSceneState.SceneStateBehaviourType : null, index);
            }
        }

        public override void Reset(StateMachine stateMachine) {
            base.Reset(stateMachine);

            LastSceneState = null;
            StateSceneLoading.Clear();
            TransitionUserData = null;
            CurrentTransitionType = TransitionType.None;
    }

    public override void OnUpdate(StateMachine stateMachine, float deltaTime, Parameter timeParameter, Parameter attributeParameter, int index) {

            // If we're not in any transition just return
            if (CurrentTransitionType == TransitionType.None) {
                base.OnUpdate(stateMachine, deltaTime, timeParameter, attributeParameter, index);
                return;
            }
            SceneStateMachineState previousSceneState = LastSceneState;
            SceneStateMachineState currentSceneState = CurrentState as SceneStateMachineState;
            // Wait while in transition               
            if (previousSceneState != null && previousSceneState.SceneStateBehaviour != null && previousSceneState.SceneStateBehaviour.InTransition) {
                return;
            }
            // Check if it's preloading
            if (StateSceneLoading.Count > 0) {
                bool allLoaded = true;
                for (int i = 0; i < StateSceneLoading.Count; ++i) {
                    if (!StateSceneLoading[i].IsLoaded) {
                        allLoaded = false;
                        break;
                    }
                }
                if (!allLoaded) {
                    return;
                }
            }
            // Update empty state transition
            if (CurrentTransitionType == TransitionType.EmptyState) {
                MessageEventManager.Instance.UnlockGlobal(stateMachine.GlobalMessageEventLockIndex);
                CurrentTransitionType = TransitionType.None;
                ExitState(previousSceneState, currentSceneState, index);
                // Callback
                DoStateChange(stateMachine, previousSceneState, currentSceneState, index);
            } else {
                // Get scene
                SceneStateBehaviour currentStateBehaviour = null;
                // Check if it's async loaded
                if (!currentSceneState.IsLoaded) {
                    Progress = currentSceneState.Progress;
                    return;
                }
                // Check if load
                bool valid = true;
                if (currentSceneState.SceneStateType == SceneStateType.Scene) {
                    currentStateBehaviour = SceneStateManager.Instance.GetRegisteredState(currentSceneState.SceneStateScriptName);
                    if (currentStateBehaviour == null) {
                        valid = false;
                    }
                }
                // Scene/Prefab loaded
                if (valid) {
                    Progress = 1.0f;
                    CurrentTransitionType = TransitionType.None;
                    // Set state behaviour
                    if (currentSceneState.SceneStateType == SceneStateType.Prefab) {
                        // Instantiate prefab if needed
                        if (currentSceneState.SceneStateBehaviour == null) {
                            GameObject prefab = currentSceneState.Prefab.Load(typeof(GameObject)) as GameObject;
                            Assert.IsTrue(prefab != null);
                            GameObject instance = GameObject.Instantiate(prefab) as GameObject;
                            GameObject.DontDestroyOnLoad(instance);
                            currentSceneState.SceneStateBehaviour = instance.GetComponent<SceneStateBehaviour>();
                            Assert.IsTrue(currentSceneState.SceneStateBehaviour.IsInstantiatable);
                        }
                    } else {
                        currentSceneState.SceneStateBehaviour = currentStateBehaviour;
                    }
                    MessageEventManager.Instance.UnlockGlobal(stateMachine.GlobalMessageEventLockIndex);
                    // Exit previous state
                    ExitState(previousSceneState, currentSceneState, index);
                    // Enter next state
                    EnterState(previousSceneState, currentSceneState, index);
                    StartPreload(stateMachine, currentSceneState);
                    // Callback
                    DoStateChange(stateMachine, previousSceneState, currentSceneState, index);
                    // Store previous scene state
                    LastSceneState = currentSceneState;
                }
            }
        }

        public override void Initialize(StateMachine stateMachine) {
            base.Initialize(stateMachine);
            SceneStateMachine = stateMachine as SceneStateMachine;
        }

        private void GetPreloadProgress(StateMachine stateMachine, SceneStateMachineState currentSceneState, ref float progress, ref int count) {
            for (int i = 0; i < currentSceneState.TransitionIndices.Length; ++i) {
                SceneStateMachineState sceneState = stateMachine.Transitions[currentSceneState.TransitionIndices[i]].DestinationState as SceneStateMachineState;
                if (sceneState.Switch) {
                    GetPreloadProgress(stateMachine, sceneState, ref progress, ref count);
                    continue;
                }
                if (!sceneState.Preload) {
                    continue;
                }
                if (count == StateSceneLoading.Count) {
                    return;
                }
                progress += sceneState.Progress;
                count += 1;
            }
        }

        public float GetPreloadProgress(StateMachine stateMachine) {
            float progress = 0.0f;
            int count = 0;
            SceneStateMachineState currentSceneState = CurrentState as SceneStateMachineState;
            GetPreloadProgress(stateMachine, currentSceneState, ref progress, ref count);
            if (count > 0) {
                return progress / (float) count;
            }
            return 1.0f;
        }

    }

}