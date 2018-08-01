//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Ad;
using Outfit7.Util;
using UnityEngine.SceneManagement;

namespace Outfit7.StateManagement.GameStateManagement {

    public abstract class GameStateManager<S, A> : StateManager<S, A> where S : StateManager<S, A>.State {

        public StateChangeEvent OnStatePreExitPostRender;

        public S EntryState{ get; protected set; }

        public S InclusiveCurrentState { get; protected set; }

        private List<Func<bool>> PendingDialogOpenChecks = new List<Func<bool>>();

        public InterstitialAdManager InterstitialAdManager { get; set; }

        public StateManager DialogStateManager { get; set; }

        public virtual void Init() {
            EnterInitialState();
        }

        public override void OnUpdate() {
            base.OnUpdate();

            if (CurrentState == null) {
                return;
            }
            //If a new level is loading don't call update on the state
            //This will force update not to get called on the first frame
            if (StateManager.StateChanging)
                return;

            PendingDialogOpenChecks.Clear();
            CurrentState.OnUpdate();
            CheckAndOpenAllDialogs();
            PendingDialogOpenChecks.Clear();

        }

        protected override bool BlockStateChange(S newState) {
            if (newState == null && InterstitialAdManager.QuitWithPostitial()) {
                return true;
            }
            return base.BlockStateChange(newState);
        }

        private void CheckAndOpenAllDialogs() {
            for (int i = 0; i < PendingDialogOpenChecks.Count; i++) {
                Func<bool> function = PendingDialogOpenChecks[i];
                if (!CanAutoOpen())
                    return;

                if (function())
                    return;
            }
        }

        public void TryToAutoOpen(Func<bool> openingMethods) {
            PendingDialogOpenChecks.Add(openingMethods);
        }

        public virtual bool CanAutoOpen() {
            if (StateManager.ActionTriggeredInUpdate)//If any actions have already trigered don't try to auto open a dialog in the current update
                return false;

            if (StateManager.StateChanging)//If a new level is loading don't try to open the dialog
                return false;

            return true;
        }

        public bool CheckAndOpenStates(List<IAutoOpenState> autoOpenStates) {
            S openGameState = null;

            //Currently we don't allow the restore state to close any dialogs (enter your name dialog)
            if (DialogStateManager != null && DialogStateManager.IsActive())
                return false;

            IAutoOpenState removeState = null;

            for (int i = 0; i < autoOpenStates.Count; i++) {
                IAutoOpenState gameState = autoOpenStates[i];
                if (gameState.CanOpen()) {
                    openGameState = gameState as S;
                    if (gameState.AutoClear()) {
                        removeState = gameState;
                    }
                    break;
                }
            }

            if (removeState != null) {
                autoOpenStates.Remove(removeState);
            }

            bool changedState = false;
            if (openGameState != null) {
                OnActionExecuting = true;
                changedState = ChangeState(openGameState, this);
                OnActionExecuting = false;
            }
            return changedState;
        }

        protected abstract bool CanLoadLevel();

        protected abstract string LoadLevelName();

        protected override void StartStateChange() {
            InclusiveCurrentState = NextState;
            StateManager.StateChanging = true;
            StateManager.StateChangedInternal = true;
            if (CanLoadLevel()) {
                O7Log.InfoT(Tag, "Application.LoadLevel started");
                SceneManager.LoadScene(LoadLevelName(), LoadSceneMode.Single);
                O7Log.InfoT(Tag, "Application.LoadLevel ended.");
            } else {
                base.StartStateChange();
            }
        }

        public void EnterInitialState() {
            Pair<S,object> stateAndData = GetEntryStateAndData();

            Data = stateAndData.Second;
            EntryState = stateAndData.First;
            Assert.IsTrue(EntryState != null, "Entry state must never be null");

            O7Log.DebugT(Tag, "Done, EntryState: {0}", EntryState);
            NextState = EntryState;
            InclusiveCurrentState = NextState;//Has to be set here because we load the first level async from Startup

            if (PreviousState == null) {//When entering the state for the first time
                OnStatePreEnterEvent(NextState, PreviousState, Data);
            }
        }

        protected abstract Pair<S,object> GetEntryStateAndData();

        public virtual void OnLevelWasLoaded(int lvl) {
            O7Log.InfoT(Tag, "OnLevelWasLoaded {0}", lvl);

            OnStateChanged();
        }

        public override bool FireAction(A gameAction, object data) {
            if (StateManager.ActionTriggeredInUpdate)//This is not in SM because this can be completely valid (You can trigger actions on all 3 SMs in the same frame (multitouching buttons and the scene)
                return false;

            return base.FireAction(gameAction, data);
        }

        public void OnStatePreExitPostRenderEvent() {
            Assert.IsTrue(StateManager.StateChanging);
            O7Log.InfoT(Tag, "OnStateChangePostRender: {0} {1} {2}", CurrentState, NextState, Data);

            if (CurrentState == null)
                return;

            if (OnStatePreExitPostRender != null) {
                OnStatePreExitPostRender(NextState, Data);
            }
        }

        protected override void OnStateExitEvent(S currentState, S state, object data) {
            if (DialogStateManager != null)
                DialogStateManager.OnGameStateExit(currentState, data);

            base.OnStateExitEvent(currentState, state, data);
        }

        public abstract bool OnBackPress();
    }
}
