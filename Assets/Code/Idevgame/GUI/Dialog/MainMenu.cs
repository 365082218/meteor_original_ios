using UnityEngine;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

public class MainMenuState:CommonDialogState<MainMenu>
{
    public override string DialogName { get { return "MainMenu"; } }
    public MainMenuState(MainDialogStateManager dialogMgr):base(dialogMgr)
    {

    }
}

public class MainMenu : Dialog
{
    bool subMenuOpen = false;
    [SerializeField]
    Text Version;
    [SerializeField]
    Button SinglePlayerMode;
    [SerializeField]
    GameObject SubMenu;
    [SerializeField]
    Button SinglePlayer;
    [SerializeField]
    Button DlcLevel;
    [SerializeField]
    Button TeachingLevel;
    [SerializeField]
    Button CreateBattle;
    [SerializeField]
    Button MultiplePlayer;
    [SerializeField]
    Button PlayerSetting;
    [SerializeField]
    Button Replay;
    [SerializeField]
    Button EnterQueue;
    [SerializeField]
    Button UploadLog;
    [SerializeField]
    public AudioSource menu;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Main.Ins.CombatData.Chapter = null;
        //进入主界面，创建全局
        Main.Ins.EnterState(Main.Ins.GameOverlay);
        Init();
        Main.Ins.listener.enabled = true;
        menu.volume = Main.Ins.GameStateMgr.gameStatus.MusicVolume;
        //每次进入主界面，触发一次更新APP信息的操作，如果
        Main.Ins.UpdateAppInfo();
    }

    void Init()
    {
        Version.text = Main.Ins.AppInfo.MeteorVersion;
        SinglePlayerMode.onClick.AddListener(() =>
        {
            subMenuOpen = !subMenuOpen;
            SubMenu.SetActive(subMenuOpen);
        });
        //单机关卡-官方剧情
        SinglePlayer.onClick.AddListener(() =>
        {
            OnSinglePlayer();
        });
        DlcLevel.onClick.AddListener(() =>
        {
            OnDlcWnd();
        });
        //教学关卡-教导使用招式方式
        TeachingLevel.onClick.AddListener(() =>
        {
            OnTeachingLevel();
        });
        //创建房间-各种单机玩法
        CreateBattle.onClick.AddListener(() =>
        {
            OnCreateRoom();
        });
        //多人游戏-联机
        MultiplePlayer.onClick.AddListener(() =>
        {
            OnlineGame();
        });
        //设置面板
        PlayerSetting.onClick.AddListener(() =>
        {
            OnSetting();
        });
        Replay.onClick.AddListener(() =>
        {
            OnReplay();
        });
        EnterQueue.onClick.AddListener(() =>
        {
            OnEnterQueue();
        });
        if (Main.Ins.GameStateMgr.gameStatus.GodLike)
        {
            UploadLog.gameObject.SetActive(true);
        }
        UploadLog.onClick.AddListener(() => { FtpLog.UploadStart(); });
        TcpClientProxy.Exit();
    }

    void OnSinglePlayer()
    {
        //打开单机关卡面板
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LevelDialogState, true);
    }

    void OnDlcWnd()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.DlcDialogState);
    }

    //教学关卡.
    void OnTeachingLevel()
    {
        U3D.LoadLevel(Main.Ins.DataMgr.GetDatasArray<LevelDatas.LevelDatas>()[30], LevelMode.Teach, GameMode.SIDOU);
    }

    void OnCreateRoom()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.WorldTemplateDialogState);
    }

    void OnlineGame()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainLobbyDialogState);
    }

    //关卡外的设置面板和关卡内的设置面板并非同一个页面.
    void OnSetting()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.SettingDialogState);
    }

    void OnReplay()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RecordDialogState);
    }

    void OnEnterQueue()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MatchDialogState);
    }
}
