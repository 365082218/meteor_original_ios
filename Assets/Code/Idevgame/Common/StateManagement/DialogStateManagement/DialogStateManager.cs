
using System;

namespace Idevgame.StateManagement.DialogStateManagement {
    public abstract class DialogStateManager<S,A>  : StateManager<S,A> where S : StateManager<S,A>.State {
        protected override string Tag { get { return "DialogStateManager"; } }

        public override void OnUpdate() {
            if (CurrentState == null)
                return;

            CurrentState.OnUpdate();
        }

        public override bool FireAction(A action, object data) {
            if (StateManager.ActionTriggeredInUpdate)//This is not in SM because this can be completely valid (You can trigger actions on all 3 SMs in the same frame (multitouching buttons and the scene)
                return false;

            return base.FireAction(action, data);
        }

        public bool CheckAndCloseCurrentDialogIfPresent(S dialog) {
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
            return closed;
        }

        protected bool OpenDialog(S dialogState) {
            return OpenDialog(dialogState, null);
        }

        protected virtual bool OpenDialog(S dialogState, object data) {
            if (CurrentState != null)
                return false;

            return ChangeState(dialogState, data);
        }
    }
}

