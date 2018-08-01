using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;
using Outfit7.Logic;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Logic.SceneStateMachineInternal {

    public enum SceneStateType {
        Scene,
        Prefab,
    };

    public enum SceneStateLoadMode {
        Default,
        Async,
    };

    [Flags]
    public enum SceneStateFlags {
        Preload = 1,
        Persistent = 2,
        KeepActive = 4,
        FreeResources = 8,
        StartPreload = 16,
    }

    [Serializable]
    public class SceneStateSceneEntry {
        public string ScenePath;
    };

    [Serializable]
    public class SceneStateMachineState : State {
        public SceneStateType SceneStateType = SceneStateType.Scene;
        public List<SceneStateSceneEntry> ScenePaths = new List<SceneStateSceneEntry>();
        public AssetReference Prefab;
        public string SceneStateScriptName;
        public SceneStateLoadMode SceneStateLoadMode = SceneStateLoadMode.Default;
        public SceneStateFlags SceneStateFlags = SceneStateFlags.FreeResources;
        public int ParameterLoadMaskIndex = -1;

        public bool CanHaveEmptySceneToStillBeValid = false;

        // Internal
        [NonSerialized] public SceneStateBehaviour SceneStateBehaviour;
        [NonSerialized] public SceneStatePersistentData PersistentData;
        [NonSerialized] public Parameter ParameterLoadMask;
        [NonSerialized] public Type SceneStateBehaviourType;

        private List<AsyncOperation> AsyncOperations = new List<AsyncOperation>();

        internal bool IsLoaded {
            get {
                if (AsyncOperations.Count > 0) {
                    for (int i = 0; i < AsyncOperations.Count; ++i) {
                        if (AsyncOperations[i].progress < 0.9f) {
                            return false;
                        }
                    }
                    bool isActivated = false;
                    for (int i = 0; i < AsyncOperations.Count; ++i) {
                        if (!AsyncOperations[i].allowSceneActivation) {
                            AsyncOperations[i].allowSceneActivation = true;
                        } else {
                            isActivated = true;
                        }
                    }
                    if (isActivated) {
                        AsyncOperations.Clear();
                    } else {
                        return false;
                    }
                }
                if (SceneStateBehaviour != null) {
                    return true;
                }
                if (SceneStateType == SceneStateType.Prefab) {
                    return false;
                }
                if (ScenePaths.Count == 0) {
                    return true;
                }
                return SceneStateManager.Instance.GetRegisteredState(SceneStateScriptName) != null;
            }
        }

        internal float Progress {
            get {
                if (AsyncOperations.Count == 0) {
                    return 0.0f;
                }
                float progress = 0.0f;
                for (int i = 0; i < AsyncOperations.Count; ++i) {
                    progress += AsyncOperations[i].progress / 0.9f;
                }
                progress /= (float) AsyncOperations.Count;
                return progress;
            }
        }

        internal bool Loading {
            get { 
                return AsyncOperations.Count > 0;
            }
        }

        internal bool Preload {
            get {
                return (SceneStateFlags & SceneStateFlags.Preload) > 0;
            }
        }

        internal bool Persistent {
            get {
                return (SceneStateFlags & SceneStateFlags.Persistent) > 0;
            }
        }

        internal bool KeepActive {
            get {
                return (SceneStateFlags & SceneStateFlags.KeepActive) > 0;
            }
        }

        internal bool FreeResources {
            get {
                return (SceneStateFlags & SceneStateFlags.FreeResources) > 0;
            }
        }

        internal bool StartPreload {
            get {
                return (SceneStateFlags & SceneStateFlags.StartPreload) > 0;
            }
        }

        internal bool LoadState(SceneStateMachine sceneStateMachine, bool forceAsync) {
            if (SceneStateBehaviour != null || AsyncOperations.Count > 0) {
                AsyncOperations.Clear();
                return false;
            }
            O7Log.DebugT(sceneStateMachine.Tag, "State Load {0} {1}", Name, forceAsync);
            if (SceneStateType == SceneStateType.Scene) {
                int loadMask = ParameterLoadMask != null ? ParameterLoadMask.ValueInt : -1;
                for (int i = 0; i < ScenePaths.Count; ++i) {
                    // Check mask
                    if ((loadMask & (1 << i)) == 0) {
                        continue;
                    }
                    sceneStateMachine.IncrementSceneLoadCount(ScenePaths[i].ScenePath);
                    if (forceAsync || SceneStateLoadMode == SceneStateLoadMode.Async) {
                        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(ScenePaths[i].ScenePath, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                        Assert.IsTrue(asyncOperation != null);
                        asyncOperation.allowSceneActivation = forceAsync ? false : true;
                        AsyncOperations.Add(asyncOperation);
                    } else {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(ScenePaths[i].ScenePath, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    }
                }
            } else {
                if (forceAsync || SceneStateLoadMode == SceneStateLoadMode.Async) {
                    AsyncOperation asyncOperation = Resources.LoadAsync<GameObject>(Prefab.PrefabPath);
                    Assert.IsTrue(asyncOperation != null);
                    AsyncOperations.Add(asyncOperation);
                } else {
                    Prefab.Load(typeof(GameObject));
                }
            }
            return true;
        }

        internal void UnloadState(SceneStateMachine sceneStateMachine, SceneStateMachineState nextSceneState, out bool unloadedCompletely) {
            unloadedCompletely = false;
            if (Switch) {
                return;
            }
            if (SceneStateType == SceneStateType.Scene) {
                // If same scene or preload/persistent don't unload scene (but just potentially disable it)
                if ((nextSceneState.ScenePaths.Count > 0 && ScenePaths[0].ScenePath == nextSceneState.ScenePaths[0].ScenePath) || Persistent || CanHaveEmptySceneToStillBeValid) {
                    // Disable state
                    if (!KeepActive) {
                        SceneStateBehaviour.gameObject.SetActive(false);
                    }
                } else {
                    // Unload state
                    O7Log.DebugT(sceneStateMachine.Tag, "State Unload {0}", Name);
                    for (int i = 0; i < ScenePaths.Count; ++i) {
                        UnityEngine.SceneManagement.SceneManager.UnloadScene(ScenePaths[i].ScenePath);
                    }
                    unloadedCompletely = true;
                    SceneStateBehaviour = null;
                }
            } else {
                // If preload/persistent keep the instance
                if (Persistent) {
                    if (!KeepActive) {
                        SceneStateBehaviour.gameObject.SetActive(false);
                    }
                } else {
                    O7Log.DebugT(sceneStateMachine.Tag, "State Unload {0}", Name);
                    GameObject.Destroy(SceneStateBehaviour.gameObject);
                    unloadedCompletely = true;
                    SceneStateBehaviour = null;
                }
            }
            if (FreeResources) {
                Resources.UnloadUnusedAssets();
            }
        }

        public override void OnUpdateReferernces(StateMachine stateMachine, Layer layer) {
            base.OnUpdateReferernces(stateMachine, layer);

            ParameterLoadMask = stateMachine.GetParameterByIndex(ParameterLoadMaskIndex);
            if (!string.IsNullOrEmpty(SceneStateScriptName)) {
                SceneStateBehaviourType = Type.GetType(SceneStateScriptName);
                if (SceneStateBehaviourType == null) {
                    O7Log.ErrorT(stateMachine.Tag, "Scene State '{0}' Type '{0}' not found!", Name, SceneStateScriptName);
                }
            }
        }

    }

}