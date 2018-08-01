using System;
using System.Collections.Generic;
using Outfit7.Logic.SceneStateMachineInternal;
using Outfit7.Util;
using UnityEngine;

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
using System.Text;
#endif

namespace Outfit7.Logic {

    public class SceneStateManager : Manager<SceneStateManager> {

        const string Tag = "SceneStateManager";

        [SerializeField]
        private SceneStateMachine SceneStateMachine;

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
        private List<Pair<Type, Type>> PreviousAndCurrentTypeOnLayer = new List<Pair<Type, Type>>(10);
#endif

        private List<SceneStateBehaviour> RegisteredSceneStateBehaviours = new List<SceneStateBehaviour>(8);
        private Queue<SceneStateBehaviour> RegisterSceneStateBehaviourQueue = new Queue<SceneStateBehaviour>();

        public SceneStateMachine.OnStateChangeDelegate OnStateChange;

        public SceneStateMachine SceneSM {
            get {
                return SceneStateMachine;
            }
        }

        // Registration
        private int FindSceneStateBehaviour(SceneStateBehaviour sceneStateBehaviour) {
            for (int i = 0; i < RegisteredSceneStateBehaviours.Count; ++i) {
                if (RegisteredSceneStateBehaviours[i] == sceneStateBehaviour) {
                    return i;
                }
            }
            return -1;
        }

        private int FindSceneStateBehaviourByName(string name) {
            for (int i = 0; i < RegisteredSceneStateBehaviours.Count; ++i) {
                if (RegisteredSceneStateBehaviours[i] != null && (RegisteredSceneStateBehaviours[i].GetType().FullName == name || RegisteredSceneStateBehaviours[i].GetType().BaseType.FullName == name)) {
                    return i;
                }
            }
            return -1;
        }

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
        private void OnMessageEventInfo(MessageEvent msgEvent, StringBuilder sb) {
            sb.AppendFormat("SM:{0} Layer(InTransition) :: Previous -> Current", SceneStateMachine.name);
            Type previousType, nextType;
            for (int i = 0; i < PreviousAndCurrentTypeOnLayer.Count; i++) {
                sb.AppendLine();
                previousType = PreviousAndCurrentTypeOnLayer[i].First;
                nextType = PreviousAndCurrentTypeOnLayer[i].Second;
                sb.AppendFormat("{0}({1}) :: {2} -> {3}",
                    SceneStateMachine.InternalLayers[i].Name,
                    IsLayerInTransition(i),
                    previousType != null ? previousType.Name : "null",
                    nextType != null ? nextType.Name : "null");
            }
        }
#endif

        private void OnRegisterState(SceneStateBehaviour sceneStateBehaviour) {
            int index = FindSceneStateBehaviour(sceneStateBehaviour);
            if (index != -1) throw new Exception(string.Format("State '{0}' already registered!", sceneStateBehaviour.GetType().FullName));
            RegisteredSceneStateBehaviours.Add(sceneStateBehaviour);
            O7Log.DebugT(Tag, "RegisterState: {0}", sceneStateBehaviour.GetType().FullName);
        }

        public void RegisterState(SceneStateBehaviour sceneStateBehaviour) {
            if (sceneStateBehaviour.IsInstantiatable) {
                return;
            }
            lock (RegisterSceneStateBehaviourQueue) {
                RegisterSceneStateBehaviourQueue.Enqueue(sceneStateBehaviour);
            }
        }

        public void UnregisterState(SceneStateBehaviour sceneStateBehaviour) {
            if (sceneStateBehaviour.IsInstantiatable) {
                return;
            }
            int index = FindSceneStateBehaviour(sceneStateBehaviour);
            if (index == -1) throw new Exception(string.Format("State '{0}' not registered!", sceneStateBehaviour.GetType().FullName));
            RegisteredSceneStateBehaviours.RemoveAt(index);
            O7Log.DebugT(Tag, "UnregisterState: {0}", sceneStateBehaviour.GetType().FullName);
        }

        public SceneStateBehaviour GetRegisteredState(string name) {
            int index = FindSceneStateBehaviourByName(name);
            if (index == -1) {
                return null;
            }
            return RegisteredSceneStateBehaviours[index];
        }

        public void RegisterSceneLoaded(string scenePath) {
            var path = SceneStateMachine.GetScenePath(scenePath);
            SceneStateMachine.DecrementSceneLoadCount(path);
        }

        // States
        public bool IsLayerInTransition(int layerIndex) {
            return SceneStateMachine.IsLayerInTransition(layerIndex);
        }

        public bool IsAnyLayerInTransition() {
            return SceneStateMachine.IsAnyLayerInTransition();
        }

        public float GetLayerPreloadProgress(int layerIndex) {
            return SceneStateMachine.GetLayerPreloadProgress(layerIndex);
        }

        public bool IsLayerActive(int layerIndex) {
            return SceneStateMachine.IsLayerActive(layerIndex);
        }

        public bool IsAnyLayerActive(int startLayerIndex, int endLayerIndex) {
            return SceneStateMachine.IsAnyLayerActive(startLayerIndex, endLayerIndex);
        }

        public SceneStateBehaviour GetActiveStateBehaviour(int layerIndex) {
            return SceneStateMachine.GetActiveStateBehaviour(layerIndex);
        }

        public string GetActiveState(int index) {
            return SceneStateMachine.GetActiveState(index);
        }

        public string GetActiveState() {
            return SceneStateMachine.GetActiveState();
        }

        public bool IsAllLayersAttributeMask(int mask) {
            return SceneStateMachine.IsAllLayersAttributeMask(mask);
        }

        public bool IsAnyLayersAttributeMask(int mask) {
            return SceneStateMachine.IsAnyLayersAttributeMask(mask);
        }

        public bool IsAllLayerAttributeMask(int layer, int mask) {
            return SceneStateMachine.IsAllLayerAttributeMask(layer, mask);
        }

        public bool IsAnyLayerAttributeMask(int layer, int mask) {
            return SceneStateMachine.IsAnyLayerAttributeMask(layer, mask);
        }

        // Parameters
        public void SetParameterBool(int index, bool value) {
            SceneStateMachine.SetBoolParameter(index, value);
        }

        public bool GetParameterBool(int index) {
            return SceneStateMachine.GetBoolParameter(index);
        }

        public void SetParameterFloat(int index, float value) {
            SceneStateMachine.SetFloatParameter(index, value);
        }

        public float GetParameterFloat(int index) {
            return SceneStateMachine.GetFloatParameter(index);
        }

        public void SetParametertInt(int index, int value) {
            SceneStateMachine.SetIntParameter(index, value);
        }

        public int GetParameterInt(int index) {
            return SceneStateMachine.GetIntParameter(index);
        }

        public void SetParameterTrigger(int index, object userData = null) {
            SceneStateMachine.SetBoolTriggerParameter(index, userData);
        }

        public void SetParameterIntTrigger(int index, int value, object userData = null) {
            if (SceneStateMachine.GetIntParameter(index) != value) {
                O7Log.DebugT(Tag, "SetParameterIntTrigger Index: {0} Previous:{1}, New:{2}", index, SceneStateMachine.GetIntParameter(index), value);
            }
            SceneStateMachine.SetIntTriggerParameter(index, value, userData);
        }

        public void ResetParameterTrigger(int index) {
            SceneStateMachine.ResetTriggerParameter(index);
        }

        // Manager
        public override bool OnInitialize() {
            SceneStateMachine.DebugInfo = Outfit7.Logic.StateMachineInternal.DebugInfoType.Detailed;
#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
            for (int i = 0; i < SceneStateMachine.Layers.Length; i++) {
                PreviousAndCurrentTypeOnLayer.Add(new Pair<Type, Type>(null, null));
            }
            MessageEventManager.Instance.OnMessageEventInfo += OnMessageEventInfo;
#endif
            // Add state change delegate
            SceneStateMachine.OnStateChange = (System.Type previousState, System.Type nextState, int layerIndex) => {
#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
                PreviousAndCurrentTypeOnLayer[layerIndex].First = previousState;
                PreviousAndCurrentTypeOnLayer[layerIndex].Second = nextState;
#endif
                if (OnStateChange == null) {
                    return;
                }
                OnStateChange(previousState, nextState, layerIndex);
            };
            return true;
        }

        public override void OnTerminate() {
        }

        public override void OnPreUpdate(float deltaTime) {
            if (RegisterSceneStateBehaviourQueue.Count > 0) {
                lock (RegisterSceneStateBehaviourQueue) {
                    while (RegisterSceneStateBehaviourQueue.Count > 0) {
                        OnRegisterState(RegisterSceneStateBehaviourQueue.Dequeue());
                    }
                }
                // Remove null behaviours, because OnDestroy didn't get called on them because they are disabled
                for (int i = 0; i < RegisteredSceneStateBehaviours.Count; ++i) {
                    if (RegisteredSceneStateBehaviours[i] == null) {
                        RegisteredSceneStateBehaviours.RemoveAt(i--);
                    }
                }
            }
            // Update scene state
            if (SceneStateMachine == null) {
                Assert.IsTrue(false);
            }
            if (SceneStateMachine.isActiveAndEnabled) {
                SceneStateMachine.CustomUpdate(deltaTime);
            }
        }

        public override void OnBucketUpdate(int index, float deltaTime) {
        }

        public override void OnPostUpdate(float deltaTime) {
        }
    }
}
