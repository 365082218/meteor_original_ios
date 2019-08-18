using System;
using System.Collections.Generic;
using Idevgame.StateManagement;
using Idevgame.StateManagement.DialogStateManagement;
using Idevgame.Util;

namespace Idevgame.GameState.DialogState {
    public class MainDialogStateManager : DialogStateManager<BaseDialogState, DialogAction> {
        private List<DialogStateWrapper> ComonRoomAutoOpenedDialogStates = new List<DialogStateWrapper>();
        public BaseGameState LastNewsShownState { get; set; }
        public MainGameStateManager MtaGameStateManager { get; set; }
        public readonly Func<bool> CheckAndOpenCleanStartDialogsFunct;
        public readonly Func<bool> CheckAndOpenCommonRoomDialogsFunct;

        public TimeSpan LessImportantDialogDelayAfterAppStart = TimeSpan.FromSeconds(5);
        public TimeSpan LessImportantDialogTimeout = TimeSpan.FromSeconds(30);

        ConnectDialogState connectDialogState;
        public MainDialogStateManager() {
            //GameOverDialogState = new GameOverDialogState(this);
            //HelpDialogState = new HelpDialogState(this);
            //GameModeSelectState = new GameModeSelectDialogState(this);
            CheckAndOpenCommonRoomDialogsFunct = CheckAndOpenCommonRoomDialogs;
        }

        public void Init() {
            //AutoOpenDialogOnCommonRooms(GameCenterSignInDialogState);
            //AutoOpenDialogOnCommonRooms(LevelUpDialogState);
            //AutoOpenDialogOnCommonRooms(UpdateBannerDialogState);
        }

        public void AutoOpenDialogOnCommonRooms(BaseDialogState dialogState) {
            ComonRoomAutoOpenedDialogStates.Add(new DialogStateWrapper(dialogState, null));
        }

        public override void OnUpdate() {
            if (CurrentState == null)
                return;

            CurrentState.OnUpdate();
        }

        private bool CheckAndOpenCommonRoomDialogs() {
            if (StateChanging) return false;
            if (ActionTriggeredInUpdate) return false;
            if (MtaGameStateManager.CurrentState == null) return false;
            bool opened = false;

            if (!opened) {
                opened = CheckAndOpenDialogStates(ComonRoomAutoOpenedDialogStates);
            }

            return opened;
        }


        private bool CheckAndOpenDialogStates(List<DialogStateWrapper> dialogStates) {
            OnActionExecuting = true;

            DialogStateWrapper removeDialogStateWrapper = null;
            bool opened = false;
            for (int i = 0; i < dialogStates.Count; i++) {
                DialogStateWrapper dialogStateWrapper = dialogStates[i];
                BaseDialogState dialogState = dialogStateWrapper.DialogState;
                if (dialogState.CanOpen()) {

                    if (CurrentState != null)
                        continue;

                    if (OpenDialog(dialogState, dialogStateWrapper.Data)) {
                        if (dialogState.AutoClear()) {
                            removeDialogStateWrapper = dialogStateWrapper;
                        }
                        opened = true;
                        break;
                    }
                }
            }

            OnActionExecuting = false;

            if (removeDialogStateWrapper != null)
                dialogStates.Remove(removeDialogStateWrapper);

            return opened;
        }

        //Don't wait to change state on dialogs before the first update happens (Like in GameStateManager), so we can show all the dialogs On MonoBehaviour.Start()
        protected override bool BlockStateChange(BaseDialogState nextState) {
            if (StateManager.StateChangedInternal && nextState != null) {
                return true;
            }
            return false;
        }

        protected override bool ChangeState(BaseDialogState newState, object data) {
            BaseDialogState exitState = CurrentState;
            bool changedState = base.ChangeState(newState, data);

            if (changedState) {
                UnityEngine.Resources.UnloadUnusedAssets();
            }
            return changedState;
        }

        protected override void HandleFireAction(DialogAction gameAction, object data) {
            base.HandleFireAction(gameAction, data);
        }

        public override bool CloseCurrentDialogIfPresent(object data) {
            bool closed = base.CloseCurrentDialogIfPresent(data);
            return closed;
        }

        public override void OnGameStateExit(object state, object data) {
            base.OnGameStateExit(state, data);
        }
    }
}
