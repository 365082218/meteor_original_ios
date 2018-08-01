using System.Collections.Generic;
using UnityEngine;

namespace Outfit7.Logic.StateMachineInternal {

    public struct Callbacks {
        public delegate void ResetEvent();

        public delegate void StateChangeEvent(StateMachine stateMachine, int layerIndex, int previousStateIndex, int newStateIndex);

        public delegate void UpdateEvent();

        public delegate void UpdateLayerEvent(int layerIndex);

        public event ResetEvent OnResetEvent;
        public event StateChangeEvent OnPreStateChangeEvent;
        public event StateChangeEvent OnPostStateChangeEvent;
        public event UpdateEvent OnPreUpdateEvent;
        public event UpdateEvent OnPostUpdateEvent;
        public event UpdateLayerEvent OnPreUpdateLayerEvent;
        public event UpdateLayerEvent OnPostUpdateLayerEvent;

        public void Reset() {
            if (OnResetEvent != null) {
                OnResetEvent();
            }
        }

        public void PreStateChange(StateMachine stateMachine, int layerIndex, int previousStateIndex, int newStateIndex) {
            if (OnPreStateChangeEvent != null) {
                OnPreStateChangeEvent(stateMachine, layerIndex, previousStateIndex, newStateIndex);
            }
        }

        public void PostStateChange(StateMachine stateMachine, int layerIndex, int previousStateIndex, int newStateIndex) {
            if (OnPostStateChangeEvent != null) {
                OnPostStateChangeEvent(stateMachine, layerIndex, previousStateIndex, newStateIndex);
            }
        }

        public void PreUpdate() {
            if (OnPreUpdateEvent != null) {
                OnPreUpdateEvent();
            }
        }

        public void PostUpdate() {
            if (OnPostUpdateEvent != null) {
                OnPostUpdateEvent();
            }
        }

        public void PreUpdateLayer(int layerIndex) {
            if (OnPreUpdateLayerEvent != null) {
                OnPreUpdateLayerEvent(layerIndex);
            }
        }

        public void PostUpdateLayer(int layerIndex) {
            if (OnPostUpdateLayerEvent != null) {
                OnPostUpdateLayerEvent(layerIndex);
            }
        }
    }

}