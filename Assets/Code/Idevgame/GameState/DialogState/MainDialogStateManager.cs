using System;
using System.Collections.Generic;
using Idevgame.StateManagement;
using Idevgame.StateManagement.DialogStateManagement;
using Idevgame.Util;

namespace Idevgame.GameState.DialogState {
    public class BaseDialogStateManager : DialogStateManager<BaseDialogState, DialogAction>
    {
        private List<DialogStateWrapper> AutoOpenedDialogStates = new List<DialogStateWrapper>();
        public BaseGameState LastNewsShownState { get; set; }
        public MainGameStateManager MtaGameStateManager { get; set; }
        protected float NextDialogsAutoOpenCheckTime = 10f;
        public void AutoOpen(BaseDialogState dialogState)
        {
            AutoOpenedDialogStates.Add(new DialogStateWrapper(dialogState, null));
        }

        public override void OnUpdate()
        {
            if (this.CurrentState == null)
            {
                if (!Main.Ins.SplashScreenHidden) return;
                float time = UnityEngine.Time.timeSinceLevelLoad;
                if (this.NextDialogsAutoOpenCheckTime < time)
                {
                    this.NextDialogsAutoOpenCheckTime = time + 0.2f;
                    this.CheckAndOpenDialogStates(this.AutoOpenedDialogStates);
                }
            }
            else
            {
                this.CurrentState.OnUpdate();
            }
        }

        private bool CheckAndOpenDialogStates(List<DialogStateWrapper> dialogStates)
        {
            OnActionExecuting = true;

            DialogStateWrapper removeDialogStateWrapper = null;
            bool opened = false;
            for (int i = 0; i < dialogStates.Count; i++)
            {
                DialogStateWrapper dialogStateWrapper = dialogStates[i];
                BaseDialogState dialogState = dialogStateWrapper.DialogState;
                if (dialogState.CanOpen())
                {

                    if (CurrentState != null)
                        continue;

                    if (OpenDialog(dialogState, dialogStateWrapper.Data))
                    {
                        if (dialogState.AutoClear())
                        {
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
        protected override bool BlockStateChange(BaseDialogState nextState)
        {
            if (StateManager.StateChangedInternal && nextState != null)
            {
                return true;
            }
            return false;
        }

        public override bool ChangeState(BaseDialogState newState, object data = null)
        {
            BaseDialogState exitState = CurrentState;
            bool changedState = base.ChangeState(newState, data);

            if (changedState)
            {
                UnityEngine.Resources.UnloadUnusedAssets();
            }
            return changedState;
        }

        protected override void HandleFireAction(DialogAction gameAction, object data)
        {
            base.HandleFireAction(gameAction, data);
        }

        public override bool CloseCurrentDialogIfPresent(object data)
        {
            bool closed = base.CloseCurrentDialogIfPresent(data);
            return closed;
        }

        public override void OnGameStateExit(object state, object data)
        {
            base.OnGameStateExit(state, data);
        }
    }

    public class MainDialogStateManager:BaseDialogStateManager  {

        public ConnectDialogState ConnectDialogState;
        //主界面
        public MainMenuState MainMenuState;
        //退出面板
        public EscDialogState EscDialogState;
        //加载场景面板
        public LoadingDialogState LoadingDialogState;
        //主角模型切换面板
        public ModelSelectDialogState ModelSelectDialogState;
        public ScriptInputDialogState ScriptInputDialogState;
        public UIAdjustDialogState UIAdjustDialogState;
        public EscConfirmDialogState EscConfirmDialogState;
        public StartupDialogState StartupDialogState;
        public LevelDialogState LevelDialogState;//关卡选择页面,单机/剧本
        public DlcDialogState DlcDialogState;//剧本选择界面
        public WorldTemplateDialogState WorldTemplateDialogState;
        public MainLobbyDialogState MainLobbyDialogState;
        public SettingDialogState SettingDialogState;
        public UpdateDialogState UpdateDialogState;
        public RoomOptionDialogState RoomOptionDialogState;
        public ServerListDialogState ServerListDialogState;
        public RecordDialogState RecordDialogState;
        public HostEditDialogState HostEditDialogState;
        public LoginDialogState LoginDialogState;
        public WeaponDialogState WeaponDialogState;
        public RoleSelectDialogState RoleSelectDialogState;
        public WeaponSelectDialogState WeaponSelectDialogState;
        public MatchDialogState MatchDialogState;
        public BattleResultDialogState BattleResultDialogState;
        public SfxDialogState SfxDialogState;
        public RobotDialogState RobotDialogState;
        public MainDialogStateManager() {
            ConnectDialogState = new ConnectDialogState(this);
            EscDialogState = new EscDialogState(this);
            LoadingDialogState = new LoadingDialogState(this);
            ModelSelectDialogState = new ModelSelectDialogState(this);
            MainMenuState = new MainMenuState(this);
            EscConfirmDialogState = new EscConfirmDialogState(this);
            UIAdjustDialogState = new UIAdjustDialogState(this);
            ScriptInputDialogState = new ScriptInputDialogState(this);
            StartupDialogState = new StartupDialogState(this);
            LevelDialogState = new LevelDialogState(this);
            UpdateDialogState = new UpdateDialogState(this);
            DlcDialogState = new DlcDialogState(this);
            WorldTemplateDialogState = new WorldTemplateDialogState(this);
            MainLobbyDialogState = new MainLobbyDialogState(this);
            RoomOptionDialogState = new RoomOptionDialogState(this);
            ServerListDialogState = new ServerListDialogState(this);
            RecordDialogState = new RecordDialogState(this);
            HostEditDialogState = new HostEditDialogState(this);
            SettingDialogState = new SettingDialogState(this);
            LoginDialogState = new LoginDialogState(this);
            WeaponSelectDialogState = new WeaponSelectDialogState(this);
            MatchDialogState = new MatchDialogState(this);
            BattleResultDialogState = new BattleResultDialogState(this);
            SfxDialogState = new SfxDialogState(this);
            RobotDialogState = new RobotDialogState(this);
            WeaponDialogState = new WeaponDialogState(this);
        }

        public virtual void Init()
        {

        }
    }

    /// <summary>
    /// 自动弹出框PopUp样式，指代需要一定处理时间的弹出式面板
    /// </summary>
    public class MainPopupStateManager : BaseDialogStateManager
    {
        public NoticeDialogState NoticeDialogState;
        public MainPopupStateManager()
        {
            NoticeDialogState = new NoticeDialogState(this);
        }

        public void Init()
        {
            AutoPopup(NoticeDialogState);
        }

        private readonly List<DialogStateWrapper> AllPopupStates = new List<DialogState.DialogStateWrapper>();
        //public static void PopupState(List<RewardData> rewards)
        //{
        //    for (int i = 0; i < rewards.length; i++)
        //    {
        //        var popup:RewardPopupState = new RewardPopupState(Main.Instance.PopupStateManager);
        //        Main.Instance.PopupStateManager.AutoPopup(popup, rewards[i]);
        //    }
        //}

        public void AutoPopup(BaseDialogState dialogState, object data = null)
        {
            if (this.ExistPopup(dialogState))
                return;
            AllPopupStates.Add(new DialogStateWrapper(dialogState, data));
        }

        public bool ExistPopup(BaseDialogState dialogState)
        {
            for (int i = 0; i < this.AllPopupStates.Count; i++)
            {
                if (this.AllPopupStates[i].DialogState == dialogState)
                    return true;
            }
            if (this.CurrentState == dialogState)
                return true;
            return false;
        }

        public bool OnBackPress()
        {
            return this.FireAction(DialogAction.BackButton, null);
        }

        //不要运行基类的
        public override void OnUpdate()
        {
            if (this.CurrentState == null)
            {
                if (!Main.Ins.SplashScreenHidden) return;
                this.CheckAndOpenPopupStates(this.AllPopupStates);
            }
            else
            {
                this.CurrentState.OnUpdate();
            }
        }

        private bool CheckAndOpenPopupStates(List<DialogStateWrapper> dialogStates = null)
        {
            bool opened = false;
            if (dialogStates.Count > 0)
            {
                this.OnActionExecuting = true;
                DialogStateWrapper removeDialogStateWrapper = null;
                for (int i = 0; i < dialogStates.Count; i++)
                {
                    DialogStateWrapper dialogStateWrapper = dialogStates[i];
                    BaseDialogState dialogState = dialogStateWrapper.DialogState;
                    if (!dialogState.CanOpen())
                        continue;
                    if (!this.OpenDialog(dialogState, dialogStateWrapper.Data)) continue;
                    if (dialogState.AutoClear())
                    {
                        removeDialogStateWrapper = dialogStateWrapper;
                    }
                    opened = true;
                    break;
                }
                this.OnActionExecuting = false;
                if (removeDialogStateWrapper != null)
                    dialogStates.Remove(removeDialogStateWrapper);
            }
            return opened;
        }

        protected override void HandleFireAction(DialogAction gameAction, Object data)
        {
            base.HandleFireAction(gameAction, data);
        }
    }
}
