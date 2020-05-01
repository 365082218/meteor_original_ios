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



public class Main : MonoBehaviour {
	public static Main Ins = null;
#if LOCALHOST
    public static string strHost = "127.0.0.1";
    public static int port = 80;
#else
    public static string strHost = "www.idevgame.com";
    public static int port = 80;
#endif
	public static string strProjectUrl = "meteor";
#if UNITY_ANDROID
    public static string strPlatform { get { return RuntimePlatform.Android.ToString(); } }
#elif UNITY_IOS
    private static string strPlatform { get { return RuntimePlatform.IPhonePlayer.ToString(); } }
#else
    private static string strPlatform { get { return RuntimePlatform.WindowsPlayer.ToString(); } }
#endif
    public static string strVFileName = "Version.zip";
    public static string strNewVersionName = "Global.json";
    public static string strPlugins = "Plugins.json";
    public static string strServices = "Services.json";
    public static string strVerFile = "Version.json";
    //版本仓库地址
    public static string strFile = "http://{0}:{1}/{2}/{3}";//域名+端口+项目名称+指定文件

    public Font TextFont;
    public GameObject fpsCanvas;
    public AudioSource Music;
    public AudioSource Sound;
    public AudioListener listener;
    public AudioListener playerListener;

    public static HttpClient UpdateClient = null;

    //public MainGameStateManager GameStateManager;
    public MainDialogStateManager DialogStateManager;
    public MainPopupStateManager PopupStateManager;

    //常驻状态管理
    public GameOverlayDialogState GameOverlay;//进入主界面叠加
    public FightState FightState;//战斗界面叠加
    public ReplayState ReplayState;//回放控制界面叠加.
    public HostEditDialogState HostEditDialogState;//服务器主机编辑界面.
    public NickNameDialogState NickNameDialogState;//昵称界面.
    public BattleStatusDialogState BattleStatusDialogState;//当局战斗信息界面
    public PlayerDialogState PlayerDialogState;
    public ChatDialogState ChatDialogState;
    public PsdEditDialogState PsdEditDialogState;
    public RoomChatDialogState RoomChatDialogState;
    public LoadingEXDialogState LoadingEx;
    public ItemInfoDialogState ItemInfoDialogState;
    public GunShootDialogStatus GunShootDialogStatus;
    List<PersistState> ActiveState;
    Dictionary<MonoBehaviour, PersistState> StateHash = new Dictionary<MonoBehaviour, PersistState>();

    //主摄像机，当切换时，也一起切换.指向当前战场摄像机.
    public Camera MainCamera;
    //跟随摄像机
    public CameraFollow CameraFollow;
    //自由相机
    public CameraFree CameraFree;

    //全局唯一对象全挂在Main之下
    public GameStateMgr GameStateMgr;
    public UpdateHelper UpdateHelper;
    public AppInfo AppInfo;
    public CombatData CombatData;
    public DlcMng DlcMng;
    public GameNotice GameNotice;
    public GMBLoader GMBLoader;
    public Log Log;
    public SoundManager SoundManager;
    public BuffMng BuffMng;
    public NetWorkBattle NetWorkBattle;
    public GameBattleEx GameBattleEx;
    public MeteorManager MeteorManager;
    public ResMng ResMng;
    public ScriptMng ScriptMng;
    public SFXLoader SFXLoader;
    public PathMng PathMng;
    public ActionInterrupt ActionInterrupt;
    public EventBus EventBus;
    public GMCLoader GMCLoader;
    public DesLoader DesLoader;
    public SceneMng SceneMng;
    public MeteorUnit LocalPlayer;
    //帧同步相关
    public FrameSync FrameSync;

    public MeteorBehaviour MeteorBehaviour;
    public MenuResLoader MenuResLoader;
    public DropMng DropMng;
    public SkcLoader SkcLoader;
    public BncLoader BncLoader;
    public FMCLoader FMCLoader;
    public FMCPoseLoader FMCPoseLoader;

    public SfxMeshGenerator SfxMeshGenerator;
    public RoomMng RoomMng;
    public DataMgr DataMgr;

    public void EnterState(PersistState state)
    {
        if (ActiveState.Contains(state))
            return;
        state.OnStateEnter();
        ActiveState.Add(state);
        if (state.Owner != null)
            StateHash.Add(state.Owner, state);
    }

    public void ExitStateByOwner(UnityEngine.MonoBehaviour Owner)
    {
        if (StateHash.ContainsKey(Owner))
        {
            PersistState state = StateHash[Owner];
            ExitState(state);
        }
    }

    public void ExitState(PersistState state)
    {
        if (!ActiveState.Contains(state))
            return;
        if (state.Owner != null)
            StateHash.Remove(state.Owner);
        state.OnStateExit();
        ActiveState.Remove(state);
    }

    public bool StateActive(PersistState state)
    {
        return ActiveState.Contains(state);
    }

    public bool SplashScreenHidden = false;//开屏splash图是否隐藏.隐藏后其他界面才能开始更新
    void OnApplicationQuit()
    {
        //主线程阻塞到下载线程结束.
        HttpManager.Instance.Quit();
        //释放日志占用
        Log.Uninit();
        TcpClientProxy.Exit();
        FtpLog.Uninit();
        UpdateHelper.SaveCache();
    }

    private void Awake()
    {
        Ins = this;
        Log = new Log();
        ActiveState = new List<PersistState>();
        GameOverlay = new GameOverlayDialogState();
        FightState = new FightState();
        ReplayState = new ReplayState();
        NickNameDialogState = new NickNameDialogState();
        BattleStatusDialogState = new BattleStatusDialogState();
        PlayerDialogState = new PlayerDialogState();
        ChatDialogState = new ChatDialogState();
        PsdEditDialogState = new PsdEditDialogState();
        RoomChatDialogState = new RoomChatDialogState();
        LoadingEx = new LoadingEXDialogState();
        ItemInfoDialogState = new ItemInfoDialogState();
        GunShootDialogStatus = new GunShootDialogStatus();
        //面板管理器.
        DialogStateManager = new MainDialogStateManager();
        //顺序排队弹出框.
        PopupStateManager = new MainPopupStateManager();
        //各类游戏数据.
        GameStateMgr = new GameStateMgr();
        UpdateHelper = new UpdateHelper();
        AppInfo = new AppInfo();
        CombatData = new CombatData();
        GameNotice = new GameNotice();
        MeteorManager = new MeteorManager();
        ScriptMng = new ScriptMng();
        SFXLoader = new SFXLoader();
        ActionInterrupt = new ActionInterrupt();
        
        BuffMng = new BuffMng();
        EventBus = new EventBus();
        NetWorkBattle = new NetWorkBattle();
        SceneMng = new SceneMng();
        FrameSync = new FrameSync();
        MeteorBehaviour = new MeteorBehaviour();
        DropMng = new DropMng();
        //原版相关资源的加载器.
        MenuResLoader = new MenuResLoader();
        SkcLoader = new SkcLoader();
        BncLoader = new BncLoader();
        FMCLoader = new FMCLoader();
        GMBLoader = new GMBLoader();
        GMCLoader = new GMCLoader();
        DesLoader = new DesLoader();
        FMCPoseLoader = new FMCPoseLoader();
        DataMgr = new DataMgr();
        SfxMeshGenerator = new SfxMeshGenerator();
        RoomMng = new RoomMng();
        SoundManager = new SoundManager();
        ResMng = new ResMng();
        DlcMng = new DlcMng();

        DontDestroyOnLoad(gameObject);
        Log.WriteError(string.Format("GameStart AppVersion:{0}", Main.Ins.AppInfo.AppVersion()));
    }

    public void ShowFps(bool active)
    {
        fpsCanvas.SetActive(active);
    }

    Coroutine checkUpdate;
    void Start()
    {
        UpdateHelper.LoadCache();
        GameStateMgr.LoadState();
        DataMgr.LoadAllData();
        ResMng.Reload();
        SoundManager.Init();
        DialogStateManager.Init();
        PopupStateManager.Init();
        DialogUtils.Ins.Init();
        UnityEngine.Random.InitState((int)System.DateTime.UtcNow.Ticks);
        DialogStateManager.ChangeState(DialogStateManager.ConnectDialogState);
        if (checkUpdate == null)
            checkUpdate = StartCoroutine(CheckNeedUpdate());
    }

    public void PlayEndMovie(bool play)
    {
        if (!string.IsNullOrEmpty(CombatData.GLevelItem.sceneItems) && play && CombatData.GLevelMode == LevelMode.SinglePlayerTask && CombatData.Chapter == null)
        {
            string num = CombatData.GLevelItem.sceneItems.Substring(2);
            int number = 0;
            if (int.TryParse(num, out number))
            {
                if (CombatData.GLevelItem.ID >= 0 && CombatData.GLevelItem.ID <= 9)
                {
                    string movie = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, "mmv/" + "v" + number + ".mv");
                    U3D.PlayMovie(movie);
                }
            }
        }

        GotoMenu();
    }

    void GotoMenu()
    {
        MeteorManager.Clear();
        if (CombatData.GLevelMode == LevelMode.Teach || CombatData.GLevelMode == LevelMode.CreateWorld)
            U3D.GoBack();
        else 
        {
            U3D.GoToLevelMenu();
        }
    }

    public void GameStart()
	{
        UpdateHelper.SaveCache();
        if (checkUpdate != null)
            StopCoroutine(checkUpdate);
        checkUpdate = null;

        //加载StartUp场景，成功后，打开开始面板
        U3D.LoadScene("Startup", ()=> {
            DialogStateManager.ChangeState(DialogStateManager.StartupDialogState);
        });
    }
	
    //这个是热更新打包系统支持的更新，由每一次新打包与上次打包的文件对比组成的
    //这个热更新系统不好维护，仅支持一些资源文件的更新等
	IEnumerator CheckNeedUpdate()
	{
        //仅在局域网下可用
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //不可用
            GameStart();
            yield break;
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork && GameStateMgr.gameStatus.OnlyWifi)
        {
            //3G-4G流量套餐
            //别更新
            GameStart();
            yield break;
        }
        else
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork || !GameStateMgr.gameStatus.OnlyWifi)
        {
            //Debug.LogError("download:" + string.Format(strVFile, strHost, Main.port, strProjectUrl, strPlatform, strVFileName));
            UnityWebRequest vFile = new UnityWebRequest();
            vFile.url = string.Format(strFile, strHost, Main.port, strProjectUrl, strVFileName);
            vFile.timeout = 5;
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            vFile.downloadHandler = dH;
            yield return vFile.Send();
            if (vFile.isError || vFile.responseCode != 200)
            {
                Debug.LogError(string.Format("update version file:{0} error:{1} or responseCode:{2}", vFile.url, vFile.error, vFile.responseCode));
                vFile.Dispose();
                GameStart();
                yield break;
            }

            if (vFile.downloadHandler.data != null && vFile.downloadedBytes != 0)
            {
                if (File.Exists(ResMng.GetResPath() + "/" + strVFileName))
                    File.Delete(ResMng.GetResPath() + "/" + strVFileName);
                if (File.Exists(ResMng.GetResPath() + "/" + strVerFile))
                    File.Delete(ResMng.GetResPath() + "/" + strVerFile);
                Debug.Log(ResMng.GetResPath() + "/" + strVFileName);
                File.WriteAllBytes(ResMng.GetResPath() + "/" + strVFileName, vFile.downloadHandler.data);
                ZipUtility.UnzipFile(ResMng.GetResPath() + "/" + strVFileName, ResMng.GetResPath() + "/");
                vFile.Dispose();
            }
            else
            {
                GameStart();
                yield break;
            }
            List<VersionItem> v = ReadVersionJson(File.ReadAllText(ResMng.GetResPath() + "/" + strVerFile));
            //从版本信息下载指定压缩包
            for (int i = 0; i < v.Count; i++)
            {
                if (v[i].strVersion == AppInfo.AppVersion() && v[i].strVersionMax != v[i].strVersion)
                {
                    UpdateHelper.ApplyVersion(v[i], this);
                    yield break;
                }
            }

            GameStart();
            yield break;
        }
        else
		{
            Log.WriteError("Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork");
            GameStart();
            yield break;
		}
	}

    public static List<VersionItem> ReadVersionJson(string json)
    {
        JsonData jd = LitJson.JsonMapper.ToObject(json);
        List<VersionItem> v = new List<VersionItem>();
        for (int i = 0; i < jd.Count; i++)
        {
            VersionItem it = new VersionItem();
            it.strFilelist = jd[i]["strFilelist"].ToString();
            it.strVersion = jd[i]["strVersion"].ToString();
            it.strVersionMax = jd[i]["strVersionMax"].ToString();
            if (jd[i]["zip"] != null)
            {
                it.zip = new UpdateZip();
                it.zip.fileName = jd[i]["zip"]["fileName"].ToString();
                it.zip.Md5 = jd[i]["zip"]["Md5"].ToString();
                it.zip.size = long.Parse(jd[i]["zip"]["size"].ToString());
            }
            v.Add(it);
        }
        return v;
    }

    bool CheckUpdateCompleted(UpdateFile file)
	{
		MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();
        //check md5 and file total
        if (file.bHashChecked)
            return true;
		string strFileName = file.strFile;
		string strMD5 = file.strMd5;
		long LoadByte = file.Loadbytes;
		long TotalByte = file.Totalbytes;

		if (LoadByte != TotalByte || LoadByte == 0)
			return false;

		if (!File.Exists(file.strLocalPath))
			return false;

		FileStream filestream = new FileStream(file.strLocalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		if (filestream == null)
			return false;

		byte[] hash = md5Generator.ComputeHash(filestream);
		string strFileMD5 = System.BitConverter.ToString(hash);
		filestream.Close();

		if (strMD5 != strFileMD5)
		{
			File.Delete(ResMng.GetResPath() + "/" + strFileName);
			return false;
		}
		file.bHashChecked = true;
		return true;
	}


	void Update()
	{
        if (Complete != null)
        {
            ExtractUPK(Complete);
            Complete = null;
        }

        if (GameBattleEx != null && !GameBattleEx.BattleFinished() && CombatData.GLevelMode <= LevelMode.CreateWorld)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                //DialogStateManager.OpenDialog(DialogStateManager.escapeDialogState);
            }
        }

        DialogStateManager.Update();
        PopupStateManager.Update();
        for (int i = 0; i < ActiveState.Count; i++)
        {
            ActiveState[i].OnUpdate();
        }
        //模组管理器下载
        if (CombatData.GLevelItem == null)
            DlcMng.Update();
    }

    private void LateUpdate()
    {
        StateManager.AfterUpdate();
        DialogStateManager.OnLateUpdate();
        PopupStateManager.OnLateUpdate();
        for (int i = 0; i < ActiveState.Count; i++)
        {
            ActiveState[i].OnLateUpdate();
        }
    }

    UpdateVersion Complete = null;
    public void StartDownLoad(UpdateVersion zipInfo)
    {
        //先检查一次该下载是否完成
        if (CheckUpdateCompleted(zipInfo.File))
        {
            ExtractUPK(zipInfo);
        }
        else
        {
            StartDownload(zipInfo);
        }
    }

    IEnumerator UpdateProgress()
    {
        long target = 0;
        long source = 0;
        while (true)
        {
            HttpManager.Instance.UpdateInfo();
            if (HttpManager.Instance.TotalBytes != 0)
            {
                target = HttpManager.Instance.LoadBytes;
                string str = "Checking Update..." + string.Format("{0:d}KB/s    {1:d}/{2:d}", HttpManager.Instance.Kbps, 0, 1);
                while (source < target)
                {
                    //LoadingNotice.Instance.UpdateProgress((float)source / (float)HttpManager.Instance.TotalBytes, str);
                    source += (target / 10);
                    yield return 0;
                }
                yield return 0;
                if (HttpManager.Instance.LoadBytes == HttpManager.Instance.TotalBytes)
                {
                    //LoadingNotice.Instance.UpdateProgress(1.0f, str);
                    yield break;
                }
            }
            yield return 0;
        }
    }

    void StartDownload(UpdateVersion zipInfo)  
	{
        UpdateClient = HttpManager.Instance.Alloc();
        StartCoroutine(UpdateProgress());
        UpdateClient.AddRequest(string.Format(strFile, strHost, Main.port, strProjectUrl, zipInfo.File.strFile), zipInfo.File.strLocalPath, 
            (ref HttpRequest req)=> 
            {
                zipInfo.File.Loadbytes = req.loadBytes;
                zipInfo.File.Totalbytes = req.totalBytes;
                if (req.loadBytes == req.totalBytes && CheckUpdateCompleted(zipInfo.File))
                {
                    if (req.fs != null)
                    {
                        req.fs.Close();
                        req.fs = null;
                    }
                    req.bDone = true;
                    req.Status = TaskStatus.Done;
                    Complete = zipInfo;
                    HttpManager.Instance.Quit();
                }
                else if (req.error != null)
                {
                    //中断连接、断网，服务器关闭，或者http 404 /或者 http1.1发 thunk 或者其他，直接进入游戏。
                    HttpManager.Instance.Quit();
                    LocalMsg msg = new LocalMsg();
                    msg.Message = (int)LocalMsgType.GameStart;
                    ProtoHandler.PostMessage(msg);
                }
            }
            , zipInfo.File.Loadbytes, zipInfo.File);
        UpdateClient.StartDownload();
	}

    void ExtractUPK(UpdateVersion zipInfo)
    {
        try
        {
            string localPak = ResMng.GetUpdateTmpPath() + "/" + Guid.NewGuid().ToString() + ".pak";
            if (File.Exists(localPak))
                File.Delete(localPak);
            LZMAHelper.DeCompressFile(zipInfo.File.strLocalPath, localPak);
            UPKExtra.ExtraUPK(localPak, ResMng.GetResPath());
            AppInfo.SetAppVersion(zipInfo.VersionMax);
            UpdateHelper.CleanVersion();
            GameStart();
        }
        catch (Exception exp)
        {
            Log.WriteError(exp.Message + "|" + exp.StackTrace);
        }
    }

    //拉取Global.json,得到新版本信息和地址.
    Coroutine GlobalJsonUpdate;
    bool GlobalJsonLoaded = false;
    public void UpdateAppInfo()
    {
        if (GlobalJsonLoaded)
            return;
        GlobalJsonUpdate = StartCoroutine(UpdateAppInfoCoroutine());
    }

    IEnumerator UpdateAppInfoCoroutine()
    {
        UnityWebRequest vFile = new UnityWebRequest();
        vFile.url = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strNewVersionName);
        vFile.timeout = 20;
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        vFile.downloadHandler = dH;
        yield return vFile.Send();
        if (vFile.isError || vFile.responseCode != 200)
        {
            Debug.LogError(string.Format("update version file:{0} error:{1} or responseCode:{2}", vFile.url, vFile.error, vFile.responseCode));
            vFile.Dispose();
            GlobalJsonUpdate = null;
            GlobalJsonLoaded = true;
            yield break;
        }
        Debug.Log("download:" + vFile.url);
        LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
        GameNotice.LoadGrid(js);
        GlobalJsonLoaded = true;
    }
}
