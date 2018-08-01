//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Util;

namespace Outfit7.StateManagement {
    public abstract class StateManager {

        public static bool ActionTriggeredInUpdate { get; set; }

        public static bool StateChanging{ get; set; }

        public static bool StateChangedInternal{ get; set; }

        public static bool IsUpdateBlocked { get; set; }

        public static bool IsActionBlocked { get; set; }

        public static bool IsActionAndUpdateBlocked {
            get {
                return IsUpdateBlocked && IsActionBlocked;
            }
            set {
                IsActionBlocked = value;
                IsUpdateBlocked = value;
            }
        }

        public static void AfterUpdate() {
            ActionTriggeredInUpdate = false;

            if (!StateChangedInternal) {
                StateChanging = false;//State changing flag that blocks all fire actions and changeState methods is delayed to after all touches have been processed in the first frame
                //So if a "fake" cancel event is registered we ignore it!
            }

        }

        public abstract bool IsActive();

        public virtual void OnGameStateExit(object state, object data) {
        }

        public virtual void OnGameStateEnter(object state, object data) {
        }

    }

    public abstract class StateManager<S,A> : StateManager where S : StateManager<S,A>.State {

        public abstract class State {

            private S EnqueuedState;
            private object EnqueuedData;

            protected StateManager<S,A> StateManager;

            public State(StateManager<S,A> stateManager) {
                StateManager = stateManager;
            }

            abstract public void OnEnter(S previousState, object data);

            abstract public void OnExit(S nextState, object data);

            abstract public void OnAction(A gameAction, object data);

            public virtual S OnPreEnter(S previousState, object data) {
                return this as S;
            }

            public virtual void OnPreExit(S nextState, object data) {
            }

            public virtual void OnAppResume() {
            }

            public virtual void OnAppPause() {
            }

            public virtual void OnUpdate() {
                if (EnqueuedState != null) {
                    if (ChangeState(EnqueuedState, EnqueuedData)) {
                        EnqueuedState = null;
                        EnqueuedData = null;
                    }
                }
            }

            public virtual void OnLateUpdate() {
            }

            protected bool EnqueueStateChange(S newState) {
                return EnqueueStateChange(newState, null);
            }

            protected bool EnqueueStateChange(S newState, object data) {
                if (EnqueuedState != null)
                    return false;

                EnqueuedState = newState;
                EnqueuedData = data;
                if (ChangeState(newState, data)) {
                    EnqueuedState = null;
                    EnqueuedData = null;
                }
                return true;
            }

            protected virtual bool ChangeState(S newState) {
                return ChangeState(newState, null);
            }

            protected virtual bool ChangeState(S newState, object data) {
                return StateManager.ChangeState(newState, data);
            }

        }

        protected virtual string Tag { get { return GetType().Name; } }

        public delegate void StateChangeEvent(S state, object data);

        public StateChangeEvent OnStatePreEnter;
        public StateChangeEvent OnStatePreExit;
        public StateChangeEvent OnStateExit;
        public StateChangeEvent OnStateEnter;

        protected bool ActionProcessing;

        public S CurrentState { get; protected set; }

        public S PreviousState { get; protected set; }

        public S NextState { get; protected set; }

        protected object Data;

        //Data is automatically passed forward to any changeState call inside OnAction methods in the state:
        private object PassForwardData;

        public bool IsFirstRoomLoaded { get; private set; }

        public bool ForceStateReload { get; set; }

        protected bool OnActionExecuting;

        private const string MsgInvalidToFireAction = "It's invalid to fire an action inside any of the State's methods. Fired action {0} in current state {1}.";
        private const string MsgCantCallOnActionTwiceFromTheSameStack = "Can't call OnAction from the same stack twice or more...";

        public override bool IsActive() {
            return CurrentState != null;
        }

        public virtual void OnAppResume() {
            O7Log.InfoT(Tag, "OnAppResume {0}", CurrentState);

            if (CurrentState != null) {
                CurrentState.OnAppResume();
            }
        }

        public virtual void OnAppPause() {
            O7Log.InfoT(Tag, "OnAppPause {0}", CurrentState);

            if (CurrentState != null) {
                CurrentState.OnAppPause();
            }
        }

        public virtual void OnLateUpdate() {
            if (CurrentState != null) {
                CurrentState.OnLateUpdate();
            }
        }

        public bool FireAction(A gameAction) {
            return FireAction(gameAction, null);
        }

        public virtual bool FireAction(A gameAction, object data) {
            if (IsUpdateBlocked || IsActionBlocked) {
                O7Log.InfoT(Tag, "Action: {0} in state {1} blocked!", gameAction, CurrentState);
                return false;
            }

            if (CurrentState == null) {
                return false;  // no action can be triggered if state is null
            }

            //Fail if two actions are fired in the same stack:
            Assert.IsTrue(!ActionProcessing, MsgInvalidToFireAction, gameAction, CurrentState);

            if (StateManager.StateChanging) {

                O7Log.InfoT(Tag, "fireAction: {0} in state {1} failed due to state changing", gameAction, CurrentState);

                return false;
            }

            ActionProcessing = true;

            O7Log.InfoT(Tag, "fireAction: {0} in state {1}", gameAction, CurrentState);

            Assert.IsTrue(!OnActionExecuting, MsgCantCallOnActionTwiceFromTheSameStack);

            PassForwardData = data;
            HandleFireAction(gameAction, data);
            PassForwardData = null;

            ActionProcessing = false;
            StateManager.ActionTriggeredInUpdate = true;
            return true;
        }

        protected virtual void HandleFireAction(A gameAction, object data) {
            CurrentState.OnAction(gameAction, data);
        }

        protected virtual bool BlockStateChange(S newState) {
            return StateManager.StateChanging && newState != null;//Don't ever block if quitting
        }

        protected virtual bool ChangeState(S newState, object data) {
            if (BlockStateChange(newState))
                return false;

            if (newState == null) {
                O7Log.InfoT(Tag, "new state null -> quit");
                if (CurrentState != null) {
                    data = data ?? PassForwardData;
                    OnStatePreExitEvent(CurrentState, null, data);
                    OnStateExitEvent(CurrentState, null, data);//TODO maybe rethink this for the game state.. (but it's ok for now)
                }
                CurrentState = null;
                ToNullState();
                return true;
            }

            if (newState != CurrentState || ForceStateReload) {
                data = data ?? PassForwardData;
                Data = data;
                NextState = newState;
                PreviousState = CurrentState;
                if (CurrentState != null)
                    OnStatePreExitEvent(CurrentState, NextState, Data);
                OnStatePreEnterEvent(NextState, PreviousState, data);

                StartStateChange();
                ForceStateReload = false;
            }

            return true;
        }

        protected virtual void OnStateChanged() {
            StateManager.StateChangedInternal = false;

            if (PreviousState != null) {
                OnStateExitEvent(PreviousState, NextState, Data);
            }

            CurrentState = NextState;
            OnStateEnterEvent(CurrentState, PreviousState, Data);
            Data = null;
            IsFirstRoomLoaded = true; // Must be after onEnter
        }

        protected virtual void StartStateChange() {
            StateManager.StateChangedInternal = true;
            OnStateChanged();
        }

        protected abstract void ToNullState();

        public void Update() {
            if (IsUpdateBlocked) {
                return;
            }
            OnUpdate();
        }

        public virtual void OnUpdate() {
        }

        protected void OnStatePreEnterEvent(S callState, S state, object data) {
            O7Log.InfoT(Tag, "OnStatePreEnterEvent: {0} {1} {2}", callState, state, data);

            S rerouteState = callState.OnPreEnter(state, data);

            while (rerouteState != callState) {
                PreviousState = callState;
                NextState = rerouteState;
                callState = NextState;

                rerouteState = callState.OnPreEnter(state, data);
                Assert.IsTrue(rerouteState != null, "Exiting the app from state rerouting Not supported yet! / Actually no need for this and this is implemented to aviod accidental closes!");
            }

            if (OnStatePreEnter != null) {
                OnStatePreEnter(state, data);
            }
        }

        private void OnStatePreExitEvent(S currentState, S state, object data) {
            O7Log.InfoT(Tag, "OnStatePreExitEvent: {0} {1} {2}", currentState, state, data);

            currentState.OnPreExit(state, data);
            if (OnStatePreExit != null) {
                OnStatePreExit(state, data);
            }
        }

        protected virtual void OnStateExitEvent(S currentState, S state, object data) {
            O7Log.InfoT(Tag, "OnStateExitEvent: {0} {1} {2}", currentState, state, data);

            currentState.OnExit(state, data);
            if (OnStateExit != null) {
                OnStateExit(state, data);
            }
        }

        private void OnStateEnterEvent(S currentState, S state, object data) {
            O7Log.InfoT(Tag, "OnStateEnterEvent: {0} {1} {2}", currentState, state, data);

            currentState.OnEnter(state, data);
            if (OnStateEnter != null) {
                OnStateEnter(state, data);
            }
        }
    }
}
