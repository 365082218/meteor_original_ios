using System;
using System.Collections.Generic;
using UnityEngine;
using Outfit7.Util;
using Outfit7.Logic.StateMachineInternal;

namespace Outfit7.Logic {

    public abstract class StateMachine : BucketUpdateBehaviour {

        public const int BuiltInParameterCount = 2;

        public virtual string Tag { get { return "StateMachine"; } }
        // Serialized
        public MonoBehaviour[] UserDefinedCallbackMonoBehaviours = new MonoBehaviour[0];
        public Parameter[] Parameters;
        public Transition[] Transitions;
        public string[] AttributeNames;
        public StateMachineInternal.Enum[] Enums;
        public string ExportPath = string.Empty;
        public int GlobalMessageEventLockIndex = 0;

        public float Speed = 1.0f;
        public bool UpdateIfPaused = true;

        // Internal
        [NonSerialized] public List<MonoBehaviour> CallbackMonoBehaviours;
        [NonSerialized] public Callbacks Callbacks;
        [NonSerialized] public int CurrentLayerIndex;

        public abstract Layer[] Layers { get; }

        public abstract State[] States { get; }

        public virtual DebugInfoType DebugInfo { get; set; }

        public static int FindBaseIndex<T>(string name, int[] indices, T[] bases) where T : Base {
            for (int i = 0; i < indices.Length; i++) {
                if (bases[indices[i]].Name == name) {
                    return indices[i];
                }
            }
            return -1;
        }

        public static int FindBaseIndex<T>(string name, T[] bases) where T : Base {
            for (int i = 0; i < bases.Length; i++) {
                if (bases[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public Parameter GetParameterByIndex(int parameterIndex) {
            return parameterIndex != -1 ? Parameters[parameterIndex] : null;
        }

        public State GetStateByIndex(int stateIndex) {
            return stateIndex != -1 ? States[stateIndex] : null;
        }

        public int FindParameterIndex(string name) {
            return FindBaseIndex(name, Parameters);
        }

        public int FindLayerIndex(string name) {
            return FindBaseIndex(name, Layers);
        }

        public int FindLayerStateIndex(int layerIndex, string name) {
            return Layers[layerIndex].FindStateIndex(this, name);
        }

        public int GetLayerCurrentStateIndex(int index) {
            Layer layer = Layers[index];
            return layer.CurrentStateIndex;
        }

        public float GetLayerCurrentStateWeight(int index) {
            Layer layer = Layers[index];
            return layer.CurrentStateWeight;
        }

        public string GetLayerStateName(int layerIndex, int index) {
            return States[Layers[layerIndex].StateIndices[index]].Name;
        }

        public int GetLayerStateAttributeMask(int index) {
            Layer layer = Layers[index];
            return layer.CurrentState != null ? layer.CurrentState.AttributeMask : 0;
        }

        public void SetLayerNormalizedTime(int index, float time) {
            Layers[index].NormalizedTime = time;
        }

        public void SetLayerCurrentNormalizedTime(int index, float time) {
            Layers[index].CurrentNormalizedTime = time;
        }

        public float GetLayerCurrentNormalizedTime(int index) {
            return Layers[index].CurrentNormalizedTime;
        }

        public void SetLayerWeight(int index, float weight) {
            Layers[index].Weight = weight;
        }

        public float GetLayerWeight(int index) {
            return Layers[index].Weight;
        }

        public void EnableLayer(int index, bool enable) {
            Layers[index].Enabled = enable;
        }

        public void SetIntParameter(int index, int parameter) {
            Parameters[index].SetInt(parameter);
        }

        public int GetIntParameter(int index) {
            return Parameters[index].ValueInt;
        }

        public void SetFloatParameter(int index, float parameter) {
            Parameters[index].SetFloat(parameter);
        }

        public float GetFloatParameter(int index) {
            return Parameters[index].ValueFloat;
        }

        public void SetBoolParameter(int index, bool parameter) {
            Parameters[index].SetBool(parameter);
        }

        public void SetIntTriggerParameter(int index, int parameter, object userData = null) {
            Parameters[index].SetIntTrigger(parameter, userData);
        }

        public void SetBoolTriggerParameter(int index, object userData = null) {
            Parameters[index].SetBoolTrigger(userData);
        }

        public bool GetBoolParameter(int index) {
            return Parameters[index].ValueInt != 0;
        }

        public void ResetTriggerParameter(int index) {
            Parameters[index].ResetTrigger();
        }

        public void ResetParameters() {
            for (int i = 0; i < Parameters.Length; i++) {
                Parameters[i].ResetTrigger();
            }
        }

        public void Reset() {
            #if UNITY_EDITOR
            if (Parameters == null || Layers == null) {
                return;
            }
            #endif
            // Reset triggers
            ResetParameters();
            // Restart layers
            for (int i = 0; i < Layers.Length; i++) {
                Layers[i].Reset(this);
            }
            Callbacks.Reset();
        }

        public void UpdateMonoBehaviours() {
            // Find all child MonoBehaviours
            CallbackMonoBehaviours = new List<MonoBehaviour>(GetComponents<MonoBehaviour>());
            // Add user defined
            for (int i = 0; i < UserDefinedCallbackMonoBehaviours.Length; i++) {
                CallbackMonoBehaviours.Add(UserDefinedCallbackMonoBehaviours[i]);
            }
        }

        protected virtual bool OnPreCustomUpdate(float deltaTime) {
            return true;
        }

        protected virtual void OnPostCustomUpdate(float deltaTime) {
        }

        public void CustomUpdate(float deltaTime) {
            if (!OnPreCustomUpdate(deltaTime)) {
                return;
            }
            Callbacks.PreUpdate();
            // Update state machine
            int layerParameterIndex = 0;
            for (int i = 0; i < Layers.Length; i++, layerParameterIndex += BuiltInParameterCount) {
                CurrentLayerIndex = i;
                Callbacks.PreUpdateLayer(i);
                Layers[i].OnUpdate(this, Speed * deltaTime, Parameters[layerParameterIndex + 0], Parameters[layerParameterIndex + 1], i);
                Callbacks.PostUpdateLayer(i);
            }
            // Update transition triggers
            for (int i = 0; i < Layers.Length; i++) {
                Layers[i].OnPostUpdate(i);
            }
            Callbacks.PostUpdate();
            OnPostCustomUpdate(deltaTime);
        }

        // BucketUpdateBehaviour
        public override void OnPreUpdate(float deltaTime) {
            CustomUpdate(deltaTime);
        }

        // MonoBehaviour
        protected override void Awake() {
            base.Awake();
            for (int i = 0; i < Layers.Length; i++) {
                Layer layer = Layers[i];
                layer.UpdateReferences(this);
                layer.Initialize(this);
            }
            UpdateMonoBehaviours();
        }

        protected override void OnEnable() {
            base.OnEnable();
            Reset();
        }

        protected override void OnDisable() {
            base.OnDisable();
            for (int i = 0; i < Layers.Length; i++) {
                Layer layer = Layers[i];
                layer.Reset(this);
            }
        }
    }
}
