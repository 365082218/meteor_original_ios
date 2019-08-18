
using Idevgame.GameState.DialogState;
using Idevgame.StateManagement;
namespace Idevgame.GameState.DialogState {
    public abstract class BaseDialogState : StateManager<BaseDialogState,DialogAction>.State {

        public MainDialogStateManager DialogStateManager { get; private set; }

        public virtual string Tag { get { return this.GetType().Name; } }

        public abstract string DialogName { get; }

        public virtual bool IsAngelaVisible { get { return false; } }


        public virtual bool CanOpen() {
            return true;
        }

        public virtual bool AutoClear() {
            return false;
        }

        public BaseDialogState(MainDialogStateManager stateManager) : base(stateManager) {
            DialogStateManager = stateManager;
        }

    }
}