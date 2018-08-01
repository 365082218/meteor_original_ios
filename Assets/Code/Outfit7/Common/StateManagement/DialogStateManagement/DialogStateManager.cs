//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Util;
using System;

namespace Outfit7.StateManagement.DialogStateManagement {
    public abstract class DialogStateManager<S,A>  : StateManager<S,A> where S : StateManager<S,A>.State, IDialogState {
        protected override string Tag { get { return "DialogStateManager"; } }

        private S PendingDialogClose;
        private object PendingDialogCloseData;

        public StateManager GameStateManager;

        private const string MsgDialogMustAlwaysClose = "The dialog must always close!";
        private const string MsgCantOpenANullDialogState = "You can't open a null dialog state!";
        private const string MsgPendingDialogNotClosed = "PendingDialogClose not closed: {0}";

        public override void OnUpdate() {
            if (CurrentState == null)
                return;

            CurrentState.OnUpdate();
        }

        protected override void ToNullState() {

        }

        public abstract bool OnBackPress();

        public override bool FireAction(A action, object data) {
            if (StateManager.ActionTriggeredInUpdate)//This is not in SM because this can be completely valid (You can trigger actions on all 3 SMs in the same frame (multitouching buttons and the scene)
                return false;

            if (CurrentState == null && BuildConfig.IsProdOrDevel) {
                throw new Exception(string.Format("DialogStateManager action=\"{0}\", previousState=\"{1}\", data=\"{2}\"", action, PreviousState, data));
            }

            return base.FireAction(action, data);
        }


        public bool CheckAndCloseCurrentDialogIfPresent(IDialogState dialog) {
            if (dialog != CurrentState)
                return false;

            return CloseCurrentDialogIfPresent(null);
        }

        public bool CloseCurrentDialogIfPresent() {
            return CloseCurrentDialogIfPresent(null);
        }

        public virtual bool CloseCurrentDialogIfPresent(object data) {
            if (CurrentState == null)
                return true;

            bool closed = ChangeState(null, data);
            Assert.IsTrue(closed, MsgDialogMustAlwaysClose);

            return closed;
        }

        public bool OpenDialog(S dialogState) {
            return OpenDialog(dialogState, null);
        }

        public virtual bool OpenDialog(S dialogState, object data) {
            Assert.IsTrue(dialogState != null, MsgCantOpenANullDialogState);

            if (CurrentState != null && CurrentState.BlockOtherDialogs(dialogState))
                return false;

            //Can't open the dialog / conditions are not met
            if (!dialogState.CanOpen())
                return false;

            return ChangeState(dialogState, data);
        }

        public void CloseDialogOnMainStateExit(object data) {
            PendingDialogCloseData = data;
            PendingDialogClose = CurrentState;
        }

        public void CloseDialogOnMainStateExit() {
            CloseDialogOnMainStateExit(null);
        }

        public override void OnGameStateExit(object state, object data) {
            if (PendingDialogClose != null) {
                bool closed = ChangeState(null, PendingDialogCloseData);
                Assert.IsTrue(closed, MsgPendingDialogNotClosed, PendingDialogClose);
                PendingDialogClose = null;
            }
        }

    }
}

