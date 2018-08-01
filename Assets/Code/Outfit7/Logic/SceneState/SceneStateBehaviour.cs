using System;
using System.Collections.Generic;
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Logic {

    public class SceneStatePersistentData {
    };

    public abstract class SceneStateBehaviour : MessageEventBehaviour {

        private SceneStateMachineInternal.SceneStateMachineState InternalState;

        public virtual bool IsInstantiatable { get { return false; } }

        public virtual bool InTransition { get { return false; } }

        public int LayerIndex { get; private set; }

        public string Name { get { return InternalState.Name; } }

        public SceneStateBehaviour() {
#if UNITY_EDITOR
            if (SceneStateManager.Instance != null) {
#endif
                O7Log.DebugT("SceneStateBehaviour", "Constructor called; registering {0}.", GetType());
                SceneStateManager.Instance.RegisterState(this);
#if UNITY_EDITOR
            }
#endif
        }

        protected override void Awake() {
            base.Awake();
        }

        protected virtual void OnDestroy() {
#if UNITY_EDITOR
            if (SceneStateManager.Instance != null)
#endif
                SceneStateManager.Instance.UnregisterState(this);
        }

        public virtual void InitializePersistentData(ref SceneStatePersistentData persistentData) {
        }

        internal void StateEnterInternal(SceneStateMachineInternal.SceneStateMachineState currentState, Type previousStateType, object userData, int layerIndex) {
            InternalState = currentState;
            LayerIndex = layerIndex;
            OnStateEnter(previousStateType, userData);
        }

        internal void StateExitInternal(Type nextStateType) {
            OnStateExit(nextStateType);
        }

        internal void TransitionInternal(Type nextStateType) {
            OnTransition(nextStateType);
        }

        protected virtual void OnTransition(Type nextStateType) {
        }

        protected virtual void OnStateEnter(Type previousStateType, object userData) {
        }

        protected virtual void OnStateExit(Type nextStateType) {
        }
    }

}