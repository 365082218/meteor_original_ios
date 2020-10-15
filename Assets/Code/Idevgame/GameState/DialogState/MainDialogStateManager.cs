using System.Collections.Generic;
using Idevgame.StateManagement;
using Idevgame.StateManagement.DialogStateManagement;
using UnityEngine;

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

    public class MainDialogMgr:BaseDialogStateManager  {

        public ConnectDialogState ConnectDialogState;
        //主界面
        public MainMenuState MainMenuState;
        //退出面板
        public EscDialogState EscDialogState;
        //加载场景面板
        public LoadingDialogState LoadingDialogState;
        //主角模型切换面板
        public ModelSelectDialogState ModelSelectDialogState;
        public UIAdjustDialogState UIAdjustDialogState;
        public EscConfirmDialogState EscConfirmDialogState;
        public StartupDialogState StartupDialogState;
        public LevelDialogState LevelDialogState;//关卡选择页面,单机/剧本
        public DlcDialogState DlcDialogState;//剧本选择界面
        public DlcManagerDialogState DlcManagerDialogState;//模组管理器-从设置面板分离出来的.
        public WorldTemplateDialogState WorldTemplateDialogState;
        public MainLobbyDialogState MainLobbyDialogState;
        public SettingDialogState SettingDialogState;
        public UpdateDialogState UpdateDialogState;
        public CreateRoomDialogState RoomOptionDialogState;
        public ServerListDialogState ServerListDialogState;
        public RecordDialogState RecordDialogState;
        public HostEditDialogState HostEditDialogState;
        public LoginDialogState LoginDialogState;
        public WeaponDialogState WeaponDialogState;
        public RoleSelectDialogState RoleSelectDialogState;
        public CampSelectDialogState CampSelectDialogState;
        public WeaponSelectDialogState WeaponSelectDialogState;
        public MatchDialogState MatchDialogState;
        public BattleResultDialogState BattleResultDialogState;
        public SfxDialogState SfxDialogState;
        public RobotDialogState RobotDialogState;

        public virtual void Init()
        {
            ConnectDialogState = new ConnectDialogState(this);
            EscDialogState = new EscDialogState(this);
            LoadingDialogState = new LoadingDialogState(this);
            ModelSelectDialogState = new ModelSelectDialogState(this);
            MainMenuState = new MainMenuState(this);
            EscConfirmDialogState = new EscConfirmDialogState(this);
            UIAdjustDialogState = new UIAdjustDialogState(this);
            StartupDialogState = new StartupDialogState(this);
            LevelDialogState = new LevelDialogState(this);
            UpdateDialogState = new UpdateDialogState(this);
            DlcDialogState = new DlcDialogState(this);
            DlcManagerDialogState = new DlcManagerDialogState(this);
            WorldTemplateDialogState = new WorldTemplateDialogState(this);
            MainLobbyDialogState = new MainLobbyDialogState(this);
            RoomOptionDialogState = new CreateRoomDialogState(this);
            ServerListDialogState = new ServerListDialogState(this);
            RecordDialogState = new RecordDialogState(this);
            HostEditDialogState = new HostEditDialogState(this);
            SettingDialogState = new SettingDialogState(this);
            LoginDialogState = new LoginDialogState(this);
            WeaponSelectDialogState = new WeaponSelectDialogState(this);
            CampSelectDialogState = new CampSelectDialogState(this);
            MatchDialogState = new MatchDialogState(this);
            BattleResultDialogState = new BattleResultDialogState(this);
            SfxDialogState = new SfxDialogState(this);
            RobotDialogState = new RobotDialogState(this);
            RoleSelectDialogState = new RoleSelectDialogState(this);
            WeaponDialogState = new WeaponDialogState(this);
        }
    }

    /// <summary>
    /// 自动弹出框PopUp样式，指代需要一定处理时间的弹出式面板
    /// </summary>
    public class MainPopupStateManager : BaseDialogStateManager
    {
        public NoticeDialogState NoticeDialogState;
        public void Init()
        {
            NoticeDialogState = new NoticeDialogState(this);
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

        protected override void HandleFireAction(DialogAction gameAction, object data)
        {
            base.HandleFireAction(gameAction, data);
        }
    }

    //管理2级窗口，可多次叠加
    public class PersistDialogMgr:Singleton<PersistDialogMgr> {
        public void Init() {
            ActiveState = new List<PersistState>();
            ReconnectDialogState = new ReconnectDialogState();
            GameOverlay = new GameOverlayDialogState();
            FightState = new FightState();
            ReplayState = new ReplayState();
            NickNameDialogState = new NickNameDialogState();
            BattleStatusDialogState = new BattleStatusDialogState();
            ConnectServerState = new ConnectServerDialogState();
            PlayerDialogState = new PlayerDialogState();
            ChatDialogState = new ChatDialogState();
            PsdEditDialogState = new PsdEditDialogState();
            RoomChatDialogState = new RoomChatDialogState();
            LoadingEx = new LoadingEXDialogState();
            ItemInfoDialogState = new ItemInfoDialogState();
            GunShootDialogState = new GunShootDialogState();
            TipDialogState = new TipDialogState();
            WaitDialogState = new WaitDialogState();
            RecordSelectState = new RecordSelectState();
            ScriptInputDialogState = new ScriptInputDialogState();
        }

        List<PersistState> ActiveState;
        Dictionary<MonoBehaviour, PersistState> StateHash = new Dictionary<MonoBehaviour, PersistState>();
        public ReconnectDialogState ReconnectDialogState;
        public FightState FightState;//战斗界面叠加
        public GameOverlayDialogState GameOverlay;//进入主界面叠加
        public ReplayState ReplayState;//回放控制界面叠加.
        public NickNameDialogState NickNameDialogState;//昵称界面.
        public BattleStatusDialogState BattleStatusDialogState;//当局战斗信息界面
        public ConnectServerDialogState ConnectServerState;//与指定服务器进行连接.
        public PlayerDialogState PlayerDialogState;
        public ChatDialogState ChatDialogState;
        public ScriptInputDialogState ScriptInputDialogState;
        public PsdEditDialogState PsdEditDialogState;
        public RoomChatDialogState RoomChatDialogState;
        public LoadingEXDialogState LoadingEx;
        public ItemInfoDialogState ItemInfoDialogState;
        public GunShootDialogState GunShootDialogState;
        public TipDialogState TipDialogState;
        public WaitDialogState WaitDialogState;
        public RecordSelectState RecordSelectState;

        public void EnterState(PersistState state, object data = null) {
            if (ActiveState.Contains(state))
                return;
            state.OnStateEnter(data);
            ActiveState.Add(state);
            if (state.Owner != null)
                StateHash.Add(state.Owner, state);
        }

        public void ExitStateByOwner(UnityEngine.MonoBehaviour Owner) {
            if (StateHash.ContainsKey(Owner)) {
                PersistState state = StateHash[Owner];
                ExitState(state);
            }
        }

        public void ExitAllState() {
            List<PersistState> actives = new List<PersistState>();
            actives.AddRange(ActiveState);
            for (int i = 0; i < actives.Count; i++) {
                ExitState(actives[i]);
            }
        }

        public void ExitState(PersistState state) {
            if (!ActiveState.Contains(state))
                return;
            if (state.Owner != null)
                StateHash.Remove(state.Owner);
            state.OnStateExit();
            ActiveState.Remove(state);
        }

        public bool StateActive(PersistState state) {
            return ActiveState.Contains(state);
        }

        public void Update() {
            for (int i = 0; i < ActiveState.Count; i++) {
                ActiveState[i].OnUpdate();
            }
        }

        public void OnLateUpdate() {
            for (int i = 0; i < ActiveState.Count; i++) {
                ActiveState[i].OnLateUpdate();
            }
        }
    }
}
