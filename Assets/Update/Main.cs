using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using Idevgame.GameState;
using Idevgame.GameState.DialogState;
using Idevgame.StateManagement;
using Excel2Json;
using System.Net;
using System.ComponentModel;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

public class Main : MonoBehaviour {
	public static Main Ins = null;
    public static string strHost = "www.idevgame.com";
    public static int port = 80;
	public static string strProjectUrl = "meteor";
    public static string strVFileName = "Version.zip";
    public static string strNewVersionName = "Global.json";
    public static string strPlugins = "Plugins.json";
    public static string strServices = "Services.json";
    public static string strVerFile = "Version.json";
    //版本仓库地址
    public static string strFile = "http://{0}:{1}/{2}/{3}";//域名+端口+项目名称+指定文件
    public string localPath;
    //public string replayPath;
    public string userPath;//临时变量存储
    public string baseUrl;//下载资源根目录
    public Font TextFont;
    public GameObject fpsCanvas;
    public AudioSource Music;
    public AudioSource Sound;
    public AudioListener listener;//ui
    public AudioListener playerListener;//相机
    public MainDialogMgr DialogStateManager;
    public MainPopupStateManager PopupStateManager;
    public PersistDialogMgr PersistMgr;
    //下载工具
    public DownloadManager DownloadManager;
    //常驻状态管理
    //主摄像机，当切换时，也一起切换.指向当前战场摄像机.
    public Camera MainCamera;
    //跟随摄像机
    public CameraFollow CameraFollow;
    //自由相机
    public CameraFree CameraFree;

    //全局唯一对象全挂在Main之下
    public PlayerJoyStick JoyStick;
    public GameStateMgr GameStateMgr;
    public AppInfo AppInfo;
    public CombatData CombatData;
    public DlcMng DlcMng;
    public GameNotice GameNotice;
    public Log Log;
    public SoundManager SoundManager;
    public BuffMng BuffMng;
    public NetWorkBattle NetWorkBattle;
    public GameBattleEx GameBattleEx;
    public MeteorManager MeteorManager;
    public ScriptMng ScriptMng;
    //路径查询
    public PathMng PathMng;

    public EventBus EventBus;
    public SceneMng SceneMng;
    public MeteorUnit LocalPlayer;
    //帧同步相关.-改为状态同步，分离表现和逻辑太麻烦
    //public FrameSyncLocal FrameSyncLocal;
    public FrameSyncServer FrameSyncServer;
    //全角色通用逻辑.
    public MeteorBehaviour MeteorBehaviour;
    //物件抛出
    public DropMng DropMng;
    //加载器相关-每个loader加载数据时都先判断是否是处于资料片环境
    //加载器在离开战斗场景时都要把全部数据清理
    public SkcLoader SkcLoader;
    public BncLoader BncLoader;
    public FMCLoader FMCLoader;
    public AmbLoader AmbLoader;
    public GMCLoader GMCLoader;
    public DesLoader DesLoader;
    public SFXLoader SFXLoader;
    public GMBLoader GMBLoader;
    public FMCPoseLoader FMCPoseLoader;
    public MenuResLoader MenuResLoader;
    public ActionInterrupt ActionInterrupt;

    public SfxMeshGenerator SfxMeshGenerator;
    public RoomMng RoomMng;
    public DataMgr DataMgr;
    

    public bool SplashScreenHidden = false;//开屏splash图是否隐藏.隐藏后其他界面才能开始更新
    void OnApplicationQuit()
    {
        //释放日志占用
        Log.Uninit();
        TcpClientProxy.Ins.Exit();
        FtpLog.Uninit();
    }

    //private void OnApplicationPause(bool pause) {
    //    if (pause) {
    //        if (CombatData.Ins.GLevelItem != null && CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {
    //            NetWorkBattle.Ins.OnDisconnect();
    //        }
    //    }    
    //}

    private void InitCertificate() {
        ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
    }

    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
        bool isOk = true;
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None) {
            for (int i = 0; i < chain.ChainStatus.Length; i++) {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
                    continue;
                }
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid) {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }

    private void Awake()
    {
        Ins = this;
        InitCertificate();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        localPath = Application.persistentDataPath;
        
        userPath = string.Format("{0}/State/user.dat", Application.persistentDataPath);
        //replayPath = string.Format("{0}/Replay/", Application.persistentDataPath);
        string dir = string.Format("{0}/{1}", Application.persistentDataPath, AppInfo.Ins.AppVersion());
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        dir = string.Format("{0}/State", Application.persistentDataPath);
        //状态
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        //录像文件
        //if (!Directory.Exists(replayPath))
        //    Directory.CreateDirectory(replayPath);

        UserPref.Ins.Load(userPath);
        Create();
        Init();
#if UNITY_ANDROID && !UNITY_EDITOR
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
#endif
    }

    public void ShowFps(bool active)
    {
        fpsCanvas.SetActive(active);
    }
    [SerializeField]
    [Header("是否直接启动指定关卡")]
    bool launchSingle = false;
    [SerializeField]
    [Header("当从单个关卡启动时直接打开指定关卡")]
    int level = 1;

    void Create() {
        Log = new Log();
        LayerManager.Init();
        //不受状态机控制，仅打开和关闭的
        PersistMgr = PersistDialogMgr.Ins;
        //面板管理器.
        DialogStateManager = new MainDialogMgr();
        //顺序排队弹出框.
        PopupStateManager = new MainPopupStateManager();
        //各类游戏数据.
        GameStateMgr = GameStateMgr.Ins;
        AppInfo = AppInfo.Ins;
        CombatData = CombatData.Ins;
        GameNotice = new GameNotice();
        MeteorManager = MeteorManager.Ins;
        ScriptMng = ScriptMng.Ins;

        ActionInterrupt = ActionInterrupt.Ins;

        BuffMng = BuffMng.Ins;
        EventBus = new EventBus();
        NetWorkBattle = NetWorkBattle.Ins;
        SceneMng = SceneMng.Ins;

        //FrameSyncLocal = FrameSyncLocal.Ins;
        FrameSyncServer = FrameSyncServer.Ins;

        MeteorBehaviour = MeteorBehaviour.Ins;
        DropMng = DropMng.Ins;
        //原版相关资源的加载器.
        MenuResLoader = MenuResLoader.Ins;
        SkcLoader = SkcLoader.Ins;
        BncLoader = BncLoader.Ins;
        FMCLoader = FMCLoader.Ins;
        GMBLoader = GMBLoader.Ins;
        GMCLoader = GMCLoader.Ins;
        DesLoader = DesLoader.Ins;
        FMCPoseLoader = FMCPoseLoader.Ins;
        SFXLoader = SFXLoader.Ins;
        AmbLoader = AmbLoader.Ins;
        DataMgr = DataMgr.Ins;
        SfxMeshGenerator = SfxMeshGenerator.Ins;
        RoomMng = RoomMng.Ins;
        SoundManager = SoundManager.Ins;
        DlcMng = DlcMng.Ins;
        PathMng = PathMng.Ins;
        DownloadManager = DownloadManager.Ins;
        DontDestroyOnLoad(gameObject);
        Log.WriteError(string.Format("GameStart AppVersion:{0}", Main.Ins.AppInfo.AppVersion()));
    }

    void Init()
    {
        GameStateMgr.LoadState();
        DataMgr.LoadAllData();
        SoundManager.Init();
        DialogStateManager.Init();
        PopupStateManager.Init();
        PersistMgr.Init();
        UnityEngine.Random.InitState((int)System.DateTime.UtcNow.Ticks);
        if (!launchSingle) {
            //检查更新，进入主界面
            DialogStateManager.ChangeState(DialogStateManager.ConnectDialogState);
            GameStart();
        } else {
            //如果当前场景就是关卡场景，那么直接调用，否则需要先加载对应的关卡场景
            if (Loader.Instance == null) {
                LevelData lev = Ins.DataMgr.GetLevelData(level);
                U3D.LoadScene(lev.Scene, () => {
                    LevelHelper.OnLoadFinishedSingle(level);
                });
            } else {
                LevelHelper.OnLoadFinishedSingle(level);
            }
        }
    }

    public void GotoMenu()
    {
        if (CombatData.GLevelMode == LevelMode.Teach || CombatData.GLevelMode == LevelMode.CreateWorld)
            U3D.GoBack();
        else 
        {
            U3D.GoToLevelMenu();
        }
    }

    public void GameStart()
	{
        //加载StartUp场景，成功后，打开开始面板
        U3D.LoadScene("Startup", ()=> {
            DialogStateManager.ChangeState(DialogStateManager.StartupDialogState);
        });
    }
	

	void Update()
	{
        DialogStateManager.Update();
        PopupStateManager.Update();
        PersistMgr.Update();
        if (GameBattleEx == null) {
            DownloadManager.Update();
        }
    }

    private void LateUpdate()
    {
        StateManager.AfterUpdate();
        DialogStateManager.OnLateUpdate();
        PopupStateManager.OnLateUpdate();
        PersistMgr.OnLateUpdate();
    }

    //拉取Global.json,得到新版本信息和地址.
    //Coroutine GlobalJsonUpdate;
    //bool GlobalJsonLoaded = false;
    //public void UpdateAppInfo()
    //{
    //    if (GlobalJsonLoaded)
    //        return;
    //    GlobalJsonUpdate = StartCoroutine(UpdateAppInfoCoroutine());
    //}

    //IEnumerator UpdateAppInfoCoroutine()
    //{
    //    UnityWebRequest vFile = new UnityWebRequest();
    //    vFile.url = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strNewVersionName);
    //    vFile.timeout = 20;
    //    DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
    //    vFile.downloadHandler = dH;
    //    yield return vFile.Send();
    //    if (vFile.isNetworkError || vFile.responseCode != 200)
    //    {
    //        Debug.LogWarning(string.Format("update version file:{0} error:{1} or responseCode:{2}", vFile.url, vFile.error, vFile.responseCode));
    //        vFile.Dispose();
    //        GlobalJsonUpdate = null;
    //        GlobalJsonLoaded = true;
    //        yield break;
    //    }
    //    Debug.Log("download:" + vFile.url);
    //    LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
    //    GameNotice.LoadGrid(js);
    //    GlobalJsonLoaded = true;
    //}
}
