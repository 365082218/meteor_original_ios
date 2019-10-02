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
    Button Quit;
    [SerializeField]
    Button UploadLog;
    [SerializeField]
    public AudioSource menu;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Global.Instance.Chapter = null;
        //进入主界面，创建全局
        Main.Instance.EnterState(Main.Instance.GameOverlay);
        Init();
        Main.Instance.listener.enabled = true;
        menu.volume = GameData.Instance.gameStatus.MusicVolume;
        //每次进入主界面，触发一次更新APP信息的操作，如果
        Main.Instance.UpdateAppInfo();
    }

    void Init()
    {
        Version.text = AppInfo.Instance.MeteorVersion;
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
        Quit.onClick.AddListener(() =>
        {
            GameData.Instance.SaveState();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
        if (GameData.Instance.gameStatus.GodLike)
        {
            UploadLog.gameObject.SetActive(true);
        }
        UploadLog.onClick.AddListener(() => { FtpLog.UploadStart(); });
        TcpClientProxy.Exit();
    }

    void OnSinglePlayer()
    {
        //打开单机关卡面板
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.LevelDialogState, true);
    }

    void OnDlcWnd()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.DlcDialogState);
    }

    //教学关卡.
    void OnTeachingLevel()
    {
        U3D.LoadLevel(LevelMng.Instance.GetAllItem()[30], LevelMode.Teach, GameMode.SIDOU);
    }

    void OnCreateRoom()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.WorldTemplateDialogState);
    }

    void OnlineGame()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.MainLobbyDialogState);
    }

    //关卡外的设置面板和关卡内的设置面板并非同一个页面.
    void OnSetting()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.SettingDialogState);
    }
}
