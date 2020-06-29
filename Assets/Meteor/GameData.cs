
using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[ProtoBuf.ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class UpdateFile
{
    [ProtoBuf.ProtoMember(1)]
    public string strFile;
    [ProtoBuf.ProtoMember(2)]
    public string strLocalPath;
    [ProtoBuf.ProtoMember(3)]
    public string strMd5;
    [ProtoBuf.ProtoMember(4)]
    public long Loadbytes;
    [ProtoBuf.ProtoMember(5)]
    public long Totalbytes;
    [ProtoBuf.ProtoMember(6)]
    public bool bHashChecked;
    public UpdateFile()
    {
        strFile = "";//网络文件名
        strMd5 = "";
        strLocalPath = "";//本地临时文件名
        Loadbytes = 0;
        Totalbytes = 0;
        bHashChecked = false;
    }

    public UpdateFile(string file, string md5, long loadbytes, long totalbytes)
    {
        strFile = file;
        strMd5 = md5;
        strLocalPath = "";
        Loadbytes = loadbytes;
        Totalbytes = totalbytes;
        bHashChecked = false;
    }
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class UpdateVersion
{
    public string Version;//当前版本
    public string VersionMax;//目标版本
    public UpdateFile File = new UpdateFile();//1个压缩包，包含全部文件，以免http不断连接，到本地先解压缩，再解UPK，之后加载依赖表，OK后全资源可以由本地加载
    public string Notices;//版本更新信息.
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class RoomSetting
{
    public int Mode;//创建单机时选择的模式
    public int Life;//创建单机时选择的生命上限
    public int Weapon0;//创建单机时选择的武器1
    public int Weapon1;//创建单机时选择的武器2-联机无效
    public int ChapterTemplate;//剧本模板+关卡模板，唯一定义了一个地图.
    public int LevelTemplate;//创建单机时选择的场景
    public int Model;//创建单机时的角色模型
    public int RoundTime;//创建单机时的单轮时长
    public int MaxPlayer;//创建单机时的初始角色个数
    public string RoomName;//房间名称
    public bool DisallowSpecialWeapon;//创建房间时禁用远程武器
    public int Version;//联机时-版本号
    public int Pattern;//普通/录制/播放录像
    public RoomSetting()
    {
        LevelTemplate = 22;
        MaxPlayer = 4;
        RoundTime = 15;
        Weapon0 = 1;
        Weapon1 = 10;
        Model = 0;
        Life = 200;
        Mode = (int)GameMode.MENGZHU;
        DisallowSpecialWeapon = true;
        Version = Main.Ins.AppInfo.MeteorVersion.Equals("9.07") ? (int)protocol.RoomInfo.MeteorVersion.V907 : (int)protocol.RoomInfo.MeteorVersion.V107;
        Pattern = 1;
    }
}


//整个游戏只有一份的开关状态.就是整个
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GameState
{
    public int saveSlot;//默认使用的存档编号
    public float MusicVolume;//设置背景音乐
    public float SoundVolume;//设置声音
    public string ClientId;//IOS GAMECENTER账号。
    public int Quality;//0默认最高,1中_800面,2低_300面
    public int Level;//当前最远通过的关卡
    public int ChapterTemplate;//创建单机适默认剧本
    public string NickName;
    public bool useJoystickOrKeyBoard;//是否使用外设摇杆
    public bool EnableDebugSFX;//战斗UI调试特效是否显示
    public bool EnableDebugStatus;//角色头顶的信息条显示 动作 帧 状态 属性 等信息
    public bool EnableWeaponChoose;//战斗UI控制面板是否显示按钮
    public bool EnableDebugRobot;//调试角色按钮。
    public bool _EnableInfiniteAngry;
    public float UIAlpha = 1.0f;
    public string Account;
    public string Password;
    public bool AutoLogin;
    public bool RememberPassword;
    public bool OnlyWifi;//仅在WIFI下更新和下载模组.
    public RoomSetting Single = new RoomSetting();//单机房间设置
    public RoomSetting NetWork = new RoomSetting();//联机房间设置
    public Dictionary<string, string> LocalMovie = new Dictionary<string, string>();//已更新到本地的
    public List<ModelItem> pluginModel = new List<ModelItem>();//已安装的新模型
    public List<Chapter> pluginChapter = new List<Chapter>();//已安装的新章节-资料片
    public List<NpcTemplate> pluginNpc = new List<NpcTemplate>();//已更新的新NPC定义-资料片内有引用
    public void RegisterModel(ModelItem item)
    {
        if (pluginModel == null)
            pluginModel = new List<ModelItem>();
        pluginModel.Add(item);
    }

    public void UnRegisterModel(ModelItem item)
    {
        if (pluginModel == null)
            return;
        pluginModel.Remove(item);
    }

    public void RegisterDlc(Chapter dlc)
    {
        if (pluginChapter == null)
            pluginChapter = new List<Chapter>();
        pluginChapter.Add(dlc);
        Main.Ins.CombatData.ClearLevel();//需要刷新
    }

    public void UnRegisterDlc(Chapter dlc)
    {
        if (pluginChapter == null)
            return;
        pluginChapter.Remove(dlc);
        Main.Ins.CombatData.ClearLevel();
    }

    public bool IsModelInstalled(ModelItem item)
    {
        if (pluginModel == null)
            return false;
        for (int i = 0; i < pluginModel.Count; i++)
        {
            if (pluginModel[i].ModelId == item.ModelId)
            {
                //找到了指定的模型插件，判定模型的资源是否存在
                pluginModel[i].Check();
                return pluginModel[i].Installed;
            }
        }
        return false;
    }

    public bool IsDlcInstalled(Chapter dlc)
    {
        if (pluginChapter == null)
            return false;
        for (int i = 0; i < pluginChapter.Count; i++)
        {
            if (pluginChapter[i].ChapterId == dlc.ChapterId)
            {
                //找到了指定的模型插件，判定模型的资源是否存在
                pluginChapter[i].Check();
                return pluginChapter[i].Installed;
            }
        }
        return false;
    }

    public bool EnableInfiniteAngry
    {
        get
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _EnableInfiniteAngry;
        }
        set
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            {
                return;
            }
            _EnableInfiniteAngry = value;
        }
    }//无限气.
    public bool EnableItemName;//查看掉落武器的名称.-后续分支可能会加入武器装备养成元素.

    //一击必杀
    bool _EnableGodMode;
    public bool EnableGodMode
    {
        get
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _EnableGodMode;
        }
        set
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            {
                return;
            }
            _EnableGodMode = value;
        }
    }

    bool _Undead = false;
    public bool Undead
    {
        get
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _Undead;
        }
        set
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            {
                return;
            }
            _Undead = value;
        }
    }
    //调试作弊栏
    bool _GodLike;
    public bool GodLike
    {
        get
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _GodLike;
        }
        set
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            {
                return;
            }
            _GodLike = value;
        }
    }
    public MyVector2 JoyAnchor;//摇杆坐标.
    public bool[] HasUIAnchor = new bool[10];//UI按钮坐标被设置的.
    public MyVector2[] UIAnchor = new MyVector2[10];//UI按钮坐标
    public MyVector2 AxisSensitivity;//轴视角转向灵敏度
    public string MeteorVersion;
    public int TargetFrame;//60-30
    public bool ShowBlood;//显示敌方血量.
    public bool ShowFPS;//显示fps
    public bool ShowSysMenu2;//显示左侧复活和战况
    public bool ShowWayPoint;//显示路点
    public bool EnableLog;//HIDEBUG日志
    bool _LevelDebug;
    public bool LevelDebug
    {
        get
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _LevelDebug;
        }
        set
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            {
                return;
            }
            _LevelDebug = value;
        }
    }//关卡内功能
    public bool AutoLock;//无锁定
    public bool DisableParticle;//无粒子特效
    public bool SnowParticle;//雪粒子
    public bool DisableJoystick;//不显示摇杆.
    public bool SkipVideo;//忽略过场视频
    public List<ServerInfo> ServerList;//自定义服务器

    public void FixServerList()
    {
        if (ServerList == null || ServerList.Count == 0)
        {
            ServerInfo svr = new ServerInfo();
            svr.ServerName = "www.idevgame.com";
            svr.ServerPort = 7200;
            svr.type = 0;
            ServerList = new List<ServerInfo>();//用户自定义的服务器列表.
            ServerList.Add(svr);
        }
    }
}

//存档数据 类似于设置.
public class GameStateMgr
{
    public GameState gameStatus;
    public InventoryItem MakeEquip(int unitIdx)
    {
        ItemDatas.ItemDatas info = FindItemByIdx(unitIdx);
        if (info == null)
            return null;
        InventoryItem item = new InventoryItem();
        item.Count = 1;
        item.Idx = info.ID;
        return item;
    }

    public ItemDatas.ItemDatas FindItemByIdx(int itemid)
    {
        ItemDatas.ItemDatas ItemProperty = Main.Ins.DataMgr.GetData<ItemDatas.ItemDatas>(itemid);
        //缺失读取外部加载的部分
        //if (ItemProperty == null)
        //    ItemProperty = Main.Instance..GetItem(itemid);
        if (ItemProperty != null)
            return ItemProperty;
        return null;
    }

    string state_path_;
    public string state_path
    {
        get
        {
            if (string.IsNullOrEmpty(state_path_))
                state_path_ = string.Format("{0}/game_state.dat", Application.persistentDataPath);
            return state_path_;
        }
    }

    public void LoadState()
    {
        FileStream save = null;
        try
        {
            save = File.Open(state_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch
        {

        }
        if (save != null && save.Length != 0)
        {
            try
            {
                gameStatus = Serializer.Deserialize<GameState>(save);
            }
            catch
            {
                gameStatus = null;
            }

            if (gameStatus != null && gameStatus.MeteorVersion == null)
                gameStatus = null;
            save.Close();
            save = null;
        }
        if (gameStatus == null)
        {
            gameStatus = new GameState();
            gameStatus.Level = 1;
            gameStatus.saveSlot = 0;//默认使用0号存档.
            gameStatus.MusicVolume = 50;
            gameStatus.SoundVolume = 50;
            gameStatus.NickName = "昱泉杀手";
            gameStatus.JoyAnchor = new MyVector2(390,340);
            gameStatus.AxisSensitivity = new MyVector2(0.5f, 0.5f);
            gameStatus.MeteorVersion = "9.07";
#if UNITY_STANDALONE_WIN
            gameStatus.TargetFrame = 120;//PC上120帧
#else
            gameStatus.TargetFrame = 60;
#endif
            ServerInfo svr = new ServerInfo();
            svr.ServerName = "www.idevgame.com";
            svr.ServerHost = "www.idevgame.com";
            svr.ServerPort = 7200;
            svr.type = 0;
            gameStatus.ServerList = new List<ServerInfo>();//用户自定义的服务器列表.
            gameStatus.ServerList.Add(svr);
            gameStatus.GodLike = false;
            gameStatus.Undead = false;
            gameStatus.ShowBlood = false;
            gameStatus.Quality = 0;
            gameStatus.DisableJoystick = true;
            gameStatus.DisableParticle = true;
            gameStatus.AutoLock = true;
            gameStatus.SkipVideo = true;
            gameStatus.OnlyWifi = true;
            SaveState();
        }
        else
        {
            //如果成功加载了存档，检查其中的一些成员是否不符合要求
            gameStatus.FixServerList();
        }
        
        Main.Ins.AppInfo.MeteorVersion = gameStatus.MeteorVersion;
    }

    public void SaveState()
    {
        try
        {
            FileStream save = File.Open(state_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            save.SetLength(0);
            Serializer.Serialize(save, gameStatus);
            save.Close();
            save = null;
        }
        catch (System.Exception exp)
        {
            Log.WriteError(exp.Message);
        }
    }

    public void ResetState()
    {
        if (File.Exists(Application.persistentDataPath + "/" + "game_state.dat"))
            File.Delete(Application.persistentDataPath + "/" + "game_state.dat");
        gameStatus = null;
        LoadState();
    }
    
    //通过配置表得到武器ItemId;//若一个模型被2个武器引用，则返回前者
    public int GetWeaponCode(string model)
    {
        int unitId = -1;
        List<WeaponDatas.WeaponDatas> wItems = Main.Ins.DataMgr.GetDatasArray<WeaponDatas.WeaponDatas>();
        for (int i = 0; i < wItems.Count; i++)
        {
            if (wItems[i].WeaponR == model)
            {
                unitId = wItems[i].ID;
                break;
            }
        }

        //if (unitId == -1)
        //{
        //    WeaponBase[] wItems2 = PluginWeaponMng.Instance.GetAllItem();
        //    for (int i = 0; i < wItems2.Length; i++)
        //    {
        //        if (wItems2[i].WeaponR == model)
        //        {
        //            unitId = wItems2[i].ID;
        //            break;
        //        }
        //    }
        //}

        List<ItemDatas.ItemDatas> items = Main.Ins.DataMgr.GetDatasArray<ItemDatas.ItemDatas>();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].UnitId == unitId && items[i].MainType == 1)
                return items[i].ID;
        }
        return -1;
    }
}

//处理版本更新相关
public class UpdateHelper
{
    public UpdateVersion updateVersion;
    public void LoadCache()
    {
        FileStream save = null;
        try
        {
            save = File.Open(Application.persistentDataPath + "/" + "game_cache.dat", FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch
        {

        }
        if (save != null)
        {
            updateVersion = Serializer.Deserialize<UpdateVersion>(save);
            save.Close();
            save = null;
        }
    }

    public void SaveCache()
    {
        if (updateVersion != null)
        {
            FileStream save = null;
            try
            {
                save = File.Open(Application.persistentDataPath + "/" + "game_cache.dat", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            }
            catch
            {

            }
            if (save != null)
            {
                save.SetLength(0);
                Serializer.Serialize(save, updateVersion);
                save.Close();
                save = null;
            }
        }
    }

    //应用一个版本
    public void ApplyVersion(VersionItem ver, Main loader)
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.UpdateDialogState);
        UpdateDialogState.Instance.SetNotice(ver.strVersionMax,
            () =>
            {
                UpdateDialogState.Instance.DisableAcceptBtn();
                //如果之前有更新进度
                if (updateVersion != null)
                {
                    if (updateVersion.Version == ver.strVersion && updateVersion.VersionMax == ver.strVersionMax)
                    {
                        //不管更新包大小是否超过剩余磁盘空间
                        //long freeSpace = AppInfo.GetFreeSpace();
                        //if (updateVersion.File.Totalbytes - updateVersion.File.Loadbytes > freeSpace)
                        //{

                        //}
                        //弹一个界面，让玩家决定是否继续更新
                        if (updateVersion.File.Totalbytes != 0)
                            UpdateDialogState.Instance.UpdateProgress((float)updateVersion.File.Loadbytes / (float)updateVersion.File.Totalbytes, "0");
                        loader.StartDownLoad(updateVersion);
                    }
                    else
                    {
                        //说明自从上次升级一半，服务端又升级了，丢弃掉之前升级的内容
                        CleanVersion();
                        DownLoadVersion(ver, loader);
                    }
                }
                else
                {
                    DownLoadVersion(ver, loader);
                }
            }
            ,
        () =>
        {
            //直接进入游戏
            Main.Ins.GameStart();
        });
    }

    public void DownLoadVersion(VersionItem ver, Main loader)
    {
        updateVersion = new UpdateVersion();
        updateVersion.Version = ver.strVersion;
        updateVersion.VersionMax = ver.strVersionMax;
        updateVersion.File = new UpdateFile();
        updateVersion.File.bHashChecked = false;
        updateVersion.File.Loadbytes = 0;
        updateVersion.File.strFile = ver.zip.fileName;
        if (File.Exists(ResMng.GetUpdateTmpPath() + "/" + ver.zip.fileName))
            File.Delete(ResMng.GetUpdateTmpPath() + "/" + ver.zip.fileName);
        updateVersion.File.strLocalPath = ResMng.GetUpdateTmpPath() + "/" + ver.zip.fileName;
        updateVersion.File.Totalbytes = ver.zip.size;
        updateVersion.File.strMd5 = ver.zip.Md5;
        loader.StartDownLoad(updateVersion);
    }

    public void CleanVersion()
    {
        if (updateVersion != null && updateVersion.File != null)
        {
            if (File.Exists(updateVersion.File.strLocalPath))
                File.Delete(updateVersion.File.strLocalPath);
            updateVersion = null;
        }
    }
}

public class CombatData
{
    public bool Logined = false;
    public ServerInfo Server;//当前选择的服务器.
    public List<ServerInfo> Servers = new List<ServerInfo>();
    public float FPS = 1.0f / 30.0f;//动画设计帧率
    public float gGravity = 1000;
    public const float AngularVelocity = 360.0f;
    public const float RebornDelay = 15.0f;//复活队友的CD间隔
    public const float RebornRange = 125.0f;//复活队友的距离最大限制
    public const float RefreshFollowPathDelay = 5.0f;//如果跟随一个动态的目标，那么每5秒刷新一次位置
    public bool useShadowInterpolate = true;//是否使用影子跟随插值
    public bool PluginUpdated = false;//是否已成功更新过资料片配置文件
    public int MaxPlayer;
    public int RoundTime;
    public int MainWeapon;
    public int SubWeapon;
    public int PlayerLife;
    public int PlayerModel;
    public int ComboProbability = 5;//连击率
    public int SpecialWeaponProbability = 98;//100-98=2几率切换到远程武器，每次Think都有2%几率
    public float AimDegree = 30.0f;//夹角超过30度，需要重新瞄准
    public MeteorInput GMeteorInput = null;
    public LevelDatas.LevelDatas GLevelItem = null;//普通关卡
    public LevelMode GLevelMode;//建立房间时选择的类型，从主界面进，都是Normal
    public GameMode GGameMode;//游戏玩法类型
    public Vector3[] GLevelSpawn;
    public Vector3[] GCampASpawn;
    public Vector3[] GCampBSpawn;
    public List<WayPoint> wayPoints;
    public int CampASpawnIndex;
    public int CampBSpawnIndex;
    public int SpawnIndex;
    public LevelScriptBase GScript;
    public GameRecord GRecord;
    public bool Replay { get { return GRecord != null; } }
    public Type GScriptType;
    public long RandSeed = DateTime.Now.ToFileTime();
    bool mPauseAll;
    public Vector3 BodyHeight = new Vector3(0, 28, 0);
    public Chapter Chapter;
    public bool PauseAll
    {
        get { return mPauseAll; }
        set { mPauseAll = value; }
    }

    public const float ClimbLimit = 1.5f;//爬墙持续提供向上的力
    public const float JumpTimeLimit = 0.15f;//最少要跳跃这么久之后才能攀爬
    public const int LEVELSTART = 1;//初始关卡ID
    public int LEVELMAX = 29;//最大关卡29
    public const int ANGRYMAX = 100;
    public const int ANGRYBURST = 60;
    public const float StopDistance = 1225;//最小约35码
    public const float AttackRange = 8100.0f;//90 * 90换近战武器
    public const float FollowDistanceEnd = 3600.0f;//结束跟随60
    public const float FollowDistanceStart = 6400.0f;//开始跟随80
    public const int BreakChange = 3;//3%爆气几率
    public int MaxModel = 20;//内置角色模型20个
    public void Init()
    {
        LEVELMAX = U3D.GetMaxLevel();
    }

    public void OnServiceChanged(int i, ServerInfo Info)
    {
        if (Servers == null)
            return;
        if (i == -1)
        {
            if (Servers.Contains(Info))
            {
                Servers.Remove(Info);
                if (Server == Info)
                    Server = Servers[0];
            }
        }
        else if (i == 1)
        {
            if (!Servers.Contains(Info))
                Servers.Add(Info);
        }
    }

    private List<LevelDatas.LevelDatas> AllLevel;
    public void ClearLevel()
    {
        AllLevel = null;
    }

    public LevelDatas.LevelDatas[] GetAllLevel()
    {
        if (AllLevel != null)
            return AllLevel.ToArray();
        if (AllLevel == null)
            AllLevel = new List<LevelDatas.LevelDatas>();
        List<LevelDatas.LevelDatas> baseLevel = Main.Ins.DataMgr.GetDatasArray<LevelDatas.LevelDatas>();
        for (int i = 0; i < baseLevel.Count; i++)
        {
            AllLevel.Add(baseLevel[i]);
        }

        for (int i = 0; i < Main.Ins.GameStateMgr.gameStatus.pluginChapter.Count; i++)
        {
            baseLevel = Main.Ins.DlcMng.GetDlcLevel(Main.Ins.GameStateMgr.gameStatus.pluginChapter[i].ChapterId);
            for (int j = 0; j < baseLevel.Count; j++)
            {
                AllLevel.Add(baseLevel[j]);
            }
        }
        return AllLevel.ToArray();
    }

    public LevelDatas.LevelDatas GetGlobalLevel(int mix)
    {
        int c = (mix / 1000) * 1000;
        int l = mix % 1000;
        return GetLevel(c, l);
    }

    public LevelDatas.LevelDatas GetLevel(int chapterId, int id)
    {
        if (chapterId == 0)
        {
            LevelDatas.LevelDatas lev = Main.Ins.DataMgr.GetData<LevelDatas.LevelDatas>(id);
            if (lev != null)
                return lev;
        }

        List<LevelDatas.LevelDatas> l = Main.Ins.DlcMng.GetDlcLevel(chapterId);
        for (int i = 0; i < l.Count; i++)
        {
            if (l[i].ID == id)
                return l[i];
        }
        Debug.LogError(string.Format("无法找到指定的剧本{0}关卡{1}", chapterId, id));
        return null;
    }

    public string GetCharacterName(int id)
    {
        if (id >= Main.Ins.CombatData.MaxModel)
        {
            return DlcMng.GetPluginModel(id).Name;
        }
        return Main.Ins.DataMgr.GetData<ModelDatas.ModelDatas>(id).Name;
    }

    public static List<WayPoint> GetWayPoint(LevelDatas.LevelDatas level)
    {
        List<WayPoint> wayPoint = new List<WayPoint>();
        string items = level.sceneItems;
        if ((wayPoint == null || wayPoint.Count == 0) && !string.IsNullOrEmpty(items)){
            wayPoint = WayLoader.ReLoad(items);
        }
        return wayPoint;
    }
}

public class WayLength
{
    public int mode;//0 run 1 jump
    public float length;
}

[Serializable]
public class WayPoint
{
    public int index;//-1表示仅为一个地点，并不在路点列表中
    public Vector3 pos;
    public int size;
    public Dictionary<int, WayLength> link;
}

public enum GameResult
{
    None = -10,
    Fail = 0,
    Win = 1,
    Win2 = 2,
    TimeOut = 3,
}
//决定了入口是从单机任务来，还是开房间，进房间
public enum LevelMode
{
    Teach,//教学
    SinglePlayerTask,//剧情任务
    CreateWorld,//单机-创建世界
    //小于他的全是单机.
    MultiplyPlayer,//联机-看GameMode
}

public enum GameMode
{
    None,//还未进入关卡.
    MENGZHU = 1,//时间限制回合，不分阵营
    Rob = 2,//劫镖
    Defence = 3,//护城
    ANSHA = 4,//分为蝴蝶和流星阵营`每一边人数一般都是8个才开始玩，暗杀有队长和队友，队长脚下有个圈圈，流星阵营是蓝的，蝴蝶阵营是红的，
    //杀死对方队长算胜利，队友死了队长可以复活队友，复活的对友血量只有一半，以地图上的流星蝴蝶阵营的位置为出生点
    SIDOU = 5,//分为蝴蝶和流星阵营，不分队长和队友，死了不能复活。杀死对方全部敌人才算胜利
    Normal = 6,//单机关卡,以路点作为出生点,剧本关卡.
    GroupRPG = 7,//联机剧本模式.
}