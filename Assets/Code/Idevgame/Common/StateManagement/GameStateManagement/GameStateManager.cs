//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Idevgame.Util;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace Idevgame.StateManagement.GameStateManagement {
    public abstract class GameStateManager<S, A> : StateManager<S, A> where S : StateManager<S, A>.State {
        public S EntryState{ get; protected set; }
        public virtual void Init() {
            EnterInitialState();
        }

        public override void OnUpdate() {
            base.OnUpdate();

            if (CurrentState == null)
                return;
            if (StateManager.StateChanging)
                return;

            CurrentState.OnUpdate();
        }

        protected override bool BlockStateChange(S newState) {
            if (newState == null) {
                return true;
            }
            return base.BlockStateChange(newState);
        }

        protected override void StartStateChange() {
            StateManager.StateChanging = true;
            StateManager.StateChangedInternal = true;
            base.StartStateChange();
        }

        public void EnterInitialState() {
            Pair<S,object> stateAndData = GetEntryStateAndData();

            Data = stateAndData.Second;
            EntryState = stateAndData.First;
            NextState = EntryState;

            if (PreviousState == null) {//When entering the state for the first time
                OnStatePreEnterEvent(NextState, PreviousState, Data);
            }
        }

        protected abstract Pair<S,object> GetEntryStateAndData();

        public override bool FireAction(A gameAction, object data) {
            if (StateManager.ActionTriggeredInUpdate)//This is not in SM because this can be completely valid (You can trigger actions on all 3 SMs in the same frame (multitouching buttons and the scene)
                return false;

            return base.FireAction(gameAction, data);
        }

        protected override void OnStateExitEvent(S currentState, S state, object data) {
            base.OnStateExitEvent(currentState, state, data);
        }
    }
}
