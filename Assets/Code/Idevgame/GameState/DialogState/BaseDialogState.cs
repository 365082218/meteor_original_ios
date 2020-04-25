
using Idevgame.GameState.DialogState;
using Idevgame.StateManagement;
using UnityEngine;

namespace Idevgame.GameState.DialogState {
    public abstract class BaseDialogState : StateManager<BaseDialogState,DialogAction>.State {

        public BaseDialogStateManager DialogStateManager { get; private set; }
        protected static GameObject mRootUI;
        protected static GameObject mCanvasRoot;

        public virtual string Tag { get { return this.GetType().Name; } }

        public abstract string DialogName { get; }

        public virtual bool CanOpen() {
            return true;
        }

        public virtual bool AutoClear() {
            return false;
        }

        public BaseDialogState(BaseDialogStateManager stateManager) : base(stateManager) {
            DialogStateManager = stateManager;
        }
    }
}