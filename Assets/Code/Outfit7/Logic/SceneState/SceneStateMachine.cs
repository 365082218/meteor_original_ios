using System;
using System.Collections;
using UnityEngine;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;
using Outfit7.Logic.SceneStateMachineInternal;
using System.Collections.Generic;

namespace Outfit7.Logic {

    public class SceneStateMachine : StateMachine {

        public override string Tag { get { return "SceneStateMachine"; } }

        public delegate void OnStateChangeDelegate(System.Type previousState, System.Type nextState, int layerIndex);

        public OnStateChangeDelegate OnStateChange;

        public SceneStateMachineLayer[] InternalLayers;
        public SceneStateMachineState[] InternalStates;

        public override Layer[] Layers { get { return InternalLayers; } }

        public override State[] States { get { return InternalStates; } }

        private readonly HashSet<string> SceneLoads = new HashSet<string>();

        internal void IncrementSceneLoadCount(string scenePath) {
            SceneLoads.Add(scenePath);
#if !UNITY_EDITOR
            if (SceneLoads.Count == 1) {
                AssetReference.InSceneLoading = true;
            }
#endif
        }

        internal void DecrementSceneLoadCount(string scenePath) {
            SceneLoads.Remove(scenePath);
#if !UNITY_EDITOR
            if (SceneLoads.Count == 0) {
                AssetReference.InSceneLoading = false;
            }
#endif
        }

        public static string GetScenePath(string unityScenePath) {
            return unityScenePath.Replace("Assets/", "").Replace(".unity", "");
        }

        // States
        public bool IsLayerInTransition(int layerIndex) {
            return InternalLayers[layerIndex].InTransition;
        }

        public bool IsAnyLayerInTransition() {
            for (int i = 0; i < InternalLayers.Length; i++) {
                if (InternalLayers[i].InTransition) return true;
            }
            return false;
        }

        public float GetLayerPreloadProgress(int layerIndex) {
            return InternalLayers[layerIndex].GetPreloadProgress(this);
        }

        public bool IsLayerActive(int layerIndex) {
            SceneStateMachineLayer layer = InternalLayers[layerIndex] as SceneStateMachineLayer;
            if (layer.InTransition) {
                return true;
            }
            SceneStateMachineState state = layer.CurrentState as SceneStateMachineState;
            return state != null && state.SceneStateBehaviourType != null;
        }

        public bool IsAnyLayerActive(int startLayerIndex, int endLayerIndex) {
            for (int i = startLayerIndex; i <= endLayerIndex; ++i) {
                SceneStateMachineLayer layer = InternalLayers[i] as SceneStateMachineLayer;
                if (layer.InTransition) {
                    return true;
                }
                SceneStateMachineState state = layer.CurrentState as SceneStateMachineState;
                if (state != null && state.SceneStateBehaviourType != null) {
                    return true;
                }
            }
            return false;
        }

        public SceneStateBehaviour GetActiveStateBehaviour(int layerIndex) {
            SceneStateMachineLayer layer = InternalLayers[layerIndex];
            SceneStateMachineState state = layer.CurrentState as SceneStateMachineInternal.SceneStateMachineState;
            if (state != null) {
                return state.SceneStateBehaviour;
            }
            return null;
        }

        public string GetActiveState(int index) {
            Layer layer = InternalLayers[index];
            if (layer.CurrentState != null) {
                return layer.CurrentState.Name;
            }
            return string.Empty;
        }

        public string GetActiveState() {
            for (int i = Layers.Length - 1; i >= 0; i--) {
                Layer layer = InternalLayers[i];
                if (layer.CurrentState != null) {
                    return layer.CurrentState.Name;
                }
            }
            return string.Empty;
        }

        public bool IsAllLayersAttributeMask(int mask) {
            for (int i = 0; i < Layers.Length; ++i) {
                if ((GetLayerStateAttributeMask(i) & mask) != mask) {
                    return false;
                }
            }
            return true;
        }

        public bool IsAnyLayersAttributeMask(int mask) {
            for (int i = 0; i < Layers.Length; ++i) {
                if ((GetLayerStateAttributeMask(i) & mask) == mask) {
                    return true;
                }
            }
            return false;
        }

        public bool IsAllLayerAttributeMask(int layer, int mask) {
            if ((GetLayerStateAttributeMask(layer) & mask) == mask) {
                return true;
            }
            return false;
        }

        public bool IsAnyLayerAttributeMask(int layer, int mask) {
            if ((GetLayerStateAttributeMask(layer) & mask) != 0) {
                return true;
            }
            return false;
        }
    }

}