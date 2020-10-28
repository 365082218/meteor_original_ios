using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Excel2Json;

using protocol;

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
    public GameRecord record;//选择的录像
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
        Pattern = (int)RoomInfo.RoomPattern._Normal;
        record = null;
    }
}

//DLC记录备份，主设置要在删除重建后再读取这边的数据
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class DlcState {
    public List<ModelItem> pluginModel = new List<ModelItem>();//已安装的新模型
    public List<Chapter> pluginChapter = new List<Chapter>();//已安装的新章节-资料片
}

//游戏设置
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GameState {
    public float MusicVolume;//设置背景音乐
    public float SoundVolume;//设置声音
    public int Quality;//0默认最高,1中_800面,2低_300面
    public int Level;//当前最远通过的关卡
    public int ChapterTemplate;//创建单机适默认剧本
    public string NickName;
    public bool EnableDebugSFX;//战斗UI调试特效是否显示
    public bool EnableWeaponChoose;//战斗UI控制面板是否显示按钮
    public bool EnableDebugRobot;//调试角色按钮。
    public bool _EnableInfiniteAngry;
    public float UIAlpha = 1.0f;//UI透明度
    public float JoyScale = 1.0f;//方向键缩放;
    public string Account;
    public string Password;
    public bool AutoLogin;
    public bool RememberPassword;
    public RoomSetting Single = new RoomSetting();//单机房间设置
    public RoomSetting NetWork = new RoomSetting();//联机房间设置
    public Dictionary<string, string> LocalMovie = new Dictionary<string, string>();//已更新到本地的
    public List<ModelItem> pluginModel = new List<ModelItem>();//已安装的新模型
    public List<Chapter> pluginChapter = new List<Chapter>();//已安装的新章节-资料片
    public void RegisterModel(ModelItem item) {
        if (item == null)
            return;
        ModelItem m = FindModel(item.ModelId);
        if (m != null)
            return;
        if (pluginModel == null)
            pluginModel = new List<ModelItem>();
        pluginModel.Add(item);
    }

    public void UnRegisterModel(ModelItem item) {
        if (pluginModel == null)
            return;
        ModelItem m = FindModel(item.ModelId);
        if (m != null)
            pluginModel.Remove(m);
    }

    ModelItem FindModel(int model) {
        for (int i = 0; i < pluginModel.Count; i++) {
            if (pluginModel[i].ModelId == model) {
                return pluginModel[i];
            }
        }
        return null;
    }

    Chapter FindChapter(int chapter) {
        for (int i = 0; i < pluginChapter.Count; i++) {
            if (pluginChapter[i].ChapterId == chapter) {
                return pluginChapter[i];
            }
        }
        return null;
    }

    public void RegisterDlc(Chapter dlc) {
        if (dlc == null)
            return;
        Chapter c = FindChapter(dlc.ChapterId);
        if (c != null)
            return;
        if (pluginChapter == null)
            pluginChapter = new List<Chapter>();
        pluginChapter.Add(dlc);
        CombatData.Ins.ClearLevel();//需要刷新
    }

    public void UnRegisterDlc(Chapter dlc) {
        if (pluginChapter == null)
            return;
        Chapter c = FindChapter(dlc.ChapterId);
        if (c != null)
            pluginChapter.Remove(c);
        CombatData.Ins.ClearLevel();
    }

    public bool IsModelInstalled(int item, ref ModelItem m) {
        if (pluginModel == null)
            return false;
        for (int i = 0; i < pluginModel.Count; i++) {
            if (pluginModel[i].ModelId == item) {
                //找到了指定的模型插件，判定模型的资源是否存在
                pluginModel[i].Check();
                bool ret = pluginModel[i].Installed;
                if (ret)
                    m = pluginModel[i];
                return ret;
            }
        }
        return false;
    }

    public bool IsModelInstalled(ModelItem item) {
        if (pluginModel == null)
            return false;
        for (int i = 0; i < pluginModel.Count; i++) {
            if (pluginModel[i].ModelId == item.ModelId) {
                //找到了指定的模型插件，判定模型的资源是否存在
                pluginModel[i].Check();
                return pluginModel[i].Installed;
            }
        }
        return false;
    }

    public bool IsDlcInstalled(Chapter dlc, out Chapter exist) {
        exist = null;
        if (pluginChapter == null)
            return false;
        for (int i = 0; i < pluginChapter.Count; i++) {
            if (pluginChapter[i].ChapterId == dlc.ChapterId) {
                //找到了指定的模型插件，判定模型的资源是否存在
                pluginChapter[i].Check();
                exist = pluginChapter[i];
                return pluginChapter[i].Installed;
            }
        }
        return false;
    }

    public bool EnableInfiniteAngry {
        get {
            if (U3D.IsMultiplyPlayer())
                return false;
            return _EnableInfiniteAngry;
        }
        set {
            if (U3D.IsMultiplyPlayer()) {
                return;
            }
            _EnableInfiniteAngry = value;
        }
    }//无限气.
    public bool EnableItemName;//查看掉落武器的名称.-后续分支可能会加入武器装备养成元素.

    public bool HidePlayer;
    //一击必杀
    bool _EnableGodMode;
    public bool EnableGodMode {
        get {
            if (U3D.IsMultiplyPlayer())
                return false;
            return _EnableGodMode;
        }
        set {
            if (U3D.IsMultiplyPlayer()) {
                return;
            }
            _EnableGodMode = value;
        }
    }

    bool _Undead = false;
    public bool Undead {
        get {
            if (U3D.IsMultiplyPlayer())
                return false;
            return _Undead;
        }
        set {
            if (U3D.IsMultiplyPlayer()) {
                return;
            }
            _Undead = value;
        }
    }
    //调试作弊栏
    bool _CheatEnable;
    public bool CheatEnable {
        get {
            if (U3D.IsMultiplyPlayer())
                return false;
            return _CheatEnable;
        }
        set {
            if (U3D.IsMultiplyPlayer()) {
                return;
            }
            _CheatEnable = value;
        }
    }
    public MyVector2 JoyAnchor;//摇杆坐标.
    public List<bool> HasUIAnchor = new List<bool>();//UI按钮坐标被设置的.
    public List<float>UIScale = new List<float>();//UI按钮缩放值
    public List<MyVector2> UIAnchor = new List<MyVector2>();//UI按钮坐标,设置了的才有效.
    public MyVector2 AxisSensitivity;//轴视角转向灵敏度
    public string MeteorVersion;
    public int TargetFrame;//60-30
    public bool ShowBlood;//显示敌方血量.
    public bool ShowFPS;//显示fps
    public bool ShowWayPoint;//显示路点
    public bool AutoLock;//无锁定
    public bool DisableParticle;//无粒子特效
    public bool SnowParticle;//雪粒子
    public bool UseGamePad;//使用手柄设备-不显示战斗UI的操作部分-当未识别到手柄设备时，不起效
    public bool UseMouse;//使用鼠标控制视角
    public bool JoyEnable;//使用摇杆
    public bool JoyRotateOnly;//摇杆仅控制方向-否则还控制位移
    public int UseModel = -1;//强制使用的主角模型,可能会被特殊关卡设置无效 默认为-1
    public Dictionary<EKeyList, KeyCode> KeyMapping = new Dictionary<EKeyList, KeyCode>();//虚拟键映射关系.
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
public class GameStateMgr:Singleton<GameStateMgr>
{
    public GameState gameStatus;
    public InventoryItem MakeEquip(int unitIdx)
    {
        ItemData info = FindItemByIdx(unitIdx);
        if (info == null)
            return null;
        InventoryItem item = new InventoryItem();
        item.Count = 1;
        item.Idx = info.Key;
        return item;
    }

    public ItemData FindItemByIdx(int itemid)
    {
        ItemData ItemProperty = DataMgr.Ins.GetItemData(itemid);
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
                state_path_ = string.Format("{0}/{1}/game_state.dat", Application.persistentDataPath, AppInfo.Ins.AppVersion());
            return state_path_;
        }
    }

    public void LoadState(bool loadfromfile = true)
    {
        FileStream save = null;
        try
        {
            if (loadfromfile)
                save = File.Open(state_path, FileMode.Open, FileAccess.Read);
        }
        catch (System.Exception exp)
        {
            Debug.Log(exp.Message + "|" + exp.StackTrace);
        }
        if (save != null)
        {
            if (save.Length != 0) {
                try {
                    gameStatus = Serializer.Deserialize<GameState>(save);
                } catch {
                    gameStatus = null;
                }
            }
            //未指定流星版本.
            if (gameStatus != null && gameStatus.MeteorVersion == null)
                gameStatus = null;
            save.Close();
            save = null;
        }
        if (gameStatus == null)
        {
            gameStatus = new GameState();
            gameStatus.Level = 1;
            gameStatus.MusicVolume = 0.5f;
            gameStatus.SoundVolume = 0.5f;
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
            gameStatus.CheatEnable = false;
            gameStatus.Undead = false;
            gameStatus.ShowBlood = false;
            gameStatus.Quality = 0;
            gameStatus.DisableParticle = true;
            gameStatus.AutoLock = true;
            gameStatus.UseGamePad = false;
            gameStatus.UseMouse = false;
            gameStatus.JoyEnable = true;
            gameStatus.JoyRotateOnly = false;
            gameStatus.UseModel = -1;

            int maxNum = 10;
            //UI详细设定
            //是否设定了各个按钮的坐标位置的标志.
            for (int i = 0; i < maxNum; i++) {
                gameStatus.HasUIAnchor.Add(false);
            }
            //每个按钮的具体位置
            for (int i = 0; i < maxNum; i++) {
                gameStatus.UIAnchor.Add(new MyVector2(0, 0));
            }
            //按钮缩放
            for (int i = 0; i < maxNum; i++) {
                gameStatus.UIScale.Add(1);
            }
            //方向盘缩放
            gameStatus.JoyScale = 1;
            gameStatus.UIAlpha = 1;

            //手柄默认按键配置
            gameStatus.KeyMapping.Add(EKeyList.KL_KeyW, KeyCode.JoystickButton4);
            gameStatus.KeyMapping.Add(EKeyList.KL_KeyS, KeyCode.JoystickButton6);
            gameStatus.KeyMapping.Add(EKeyList.KL_KeyA, KeyCode.JoystickButton7);
            gameStatus.KeyMapping.Add(EKeyList.KL_KeyD, KeyCode.JoystickButton5);
            gameStatus.KeyMapping.Add(EKeyList.KL_CameraAxisXL, KeyCode.JoystickButton10);
            gameStatus.KeyMapping.Add(EKeyList.KL_CameraAxisXR, KeyCode.JoystickButton11);
            gameStatus.KeyMapping.Add(EKeyList.KL_CameraAxisYU, KeyCode.JoystickButton8);
            gameStatus.KeyMapping.Add(EKeyList.KL_CameraAxisYD, KeyCode.JoystickButton9);

            gameStatus.KeyMapping.Add(EKeyList.KL_Attack, KeyCode.JoystickButton15);
            gameStatus.KeyMapping.Add(EKeyList.KL_Defence, KeyCode.JoystickButton13);
            gameStatus.KeyMapping.Add(EKeyList.KL_Jump, KeyCode.JoystickButton14);
            gameStatus.KeyMapping.Add(EKeyList.KL_BreakOut, KeyCode.JoystickButton12);
            gameStatus.KeyMapping.Add(EKeyList.KL_ChangeWeapon, KeyCode.JoystickButton0);
            gameStatus.KeyMapping.Add(EKeyList.KL_DropWeapon, KeyCode.JoystickButton3);
            gameStatus.KeyMapping.Add(EKeyList.KL_Crouch, KeyCode.JoystickButton1);
            gameStatus.KeyMapping.Add(EKeyList.KL_KeyQ, KeyCode.JoystickButton2);
            gameStatus.KeyMapping.Add(EKeyList.KL_Help, KeyCode.JoystickButton16);
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
        FileStream save = null;
        try
        {
            save = File.Open(state_path, FileMode.OpenOrCreate, FileAccess.Write);
            save.SetLength(0);
            Serializer.Serialize(save, gameStatus);
            save.Flush();
            save.Close();
            save.Dispose();
            save = null;
        }
        catch (System.Exception exp)
        {
            Debug.Log("save failed:" + exp.Message + "|" + exp.StackTrace);
            if (save != null) {
                save.Close();
            }
        }
    }

    public void ResetState()
    {
        if (File.Exists(state_path))
            File.Delete(state_path);
        gameStatus = null;
        LoadState(false);
    }
    
    //通过配置表得到武器ItemId;//若一个模型被2个武器引用，则返回前者
    public int GetWeaponCode(string model)
    {
        int unitId = -1;
        List<WeaponData> wItems = DataMgr.Ins.GetWeaponDatas();
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

        List<ItemData> items = DataMgr.Ins.GetItemDatas();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].UnitId == unitId && items[i].MainType == (int)UnitType.Weapon)
                return items[i].Key;
        }
        return -1;
    }
}

//单位，毫秒，毫米
//用大数字替代浮点数
public class CombatData:Singleton<CombatData>
{
    public ServerInfo Server;//当前选择的服务器.
    public List<ServerInfo> Servers = new List<ServerInfo>();
    public int gGravity = 1000;//重力加速度
    public int gOnGroundCheck = 100;//如果在地面，也给一个向地面的移动测试（每1帧重置，如果离地则继续叠加），避免上下坡时没有向下移动导致的抖动
    public static float AngularVelocity = 360.0f;
    public static float RebornChance = 3.0f;
    public static float RebornDelay = 15.0f;//复活队友的CD间隔
    public static float RebornRange = 90;//复活队友的距离最大限制
    public static float RefreshFollowPath = 15.0f;//如果跟随一个动态的目标，那么每15秒刷新一次目标所在位置
    public bool useShadowInterpolate = true;//是否使用影子跟随插值
    public bool PluginUpdated = false;//是否已成功更新过资料片配置文件
    public bool GameFinished = false;
    public int MaxPlayer;
    public int RoundTime;
    public int MainWeapon;
    public int SubWeapon;
    public int PlayerLife;
    public int PlayerModel;
    public int NormalWeaponProbability = 75;//100-98=2几率切换到远程武器，每次Think都有2%几率
    public static float JumpLimit = 68.0f;//跳跃高度上限.
    public static float AimDegree = 30.0f;//夹角超过30度，需要重新瞄准
    public MeteorInput GMeteorInput = null;
    public LevelData GLevelItem = null;//普通关卡
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
    public long RandSeed;
    public static float DamageDetectDelay = 0.25f;//伤害检测间隔，持续时间长的攻击盒，可以伤害更多次敌人
    bool mPauseAll;
    public Vector3 BodyHeight = new Vector3(0, 28, 0);
    public Chapter Chapter;
    public bool PauseAll
    {
        get { return mPauseAll; }
        set { mPauseAll = value; }
    }
    public static int Jump2Velocity = 100;//蹬腿反弹速度-正后方
    public static int JumpVelocityForward = 180;//向前跳跃速度
    public static int JumpVelocityOther = 100;//其他方向上的速度

    //public const float gGravity = 980.0f;//971.4f;//向上0.55秒，向下0.45秒
    public static float groundFriction = 3000.0f;//地面摩擦力，在地面不是瞬间停止下来的。
    public static int SpeedMax = 500;//最大速度
    public static int ClimbLimitMax = 180;//最大爬墙速度
    public static float yClimbEndLimit = -30.0f;//爬墙时,Y速度低于此速度时，速度为负，开始计时，时间到就从墙壁落下
    public static float ClimbFallLimit = 0.5f;//爬墙速度向下持续0.3f认为到达要掉下的临界条件
    public static float ClimbLimit = 2.0f;//爬墙提供向上的力持续时长，表现为在墙壁上停留的时间
    public static float JumpTimeLimit = 0.15f;//最少要跳跃这么久之后才能攀爬
    public const int LEVELSTART = 1;//初始关卡ID
    public const int LEVELMAX = 9;//最大关卡9炼铁狱
    public const int ANGRYMAX = 100;
    public const int ANGRYBURST = 60;
    public static float StopDistance = 1225;//最小约35码
    public static float StopMove = 0.15f;//与路点接近要停止时的测试比例
    public static float AttackRange = 6400;//换近战武器
    public static float PlayerEnter = 3600;//隐身衣状态下，能发现对方的距离
    public static float PlayerLeave = 6400;//隐身衣状态下，视野丢失所需距离
    public static float FollowDistanceEnd = 10000;//距离小于100结束跟随
    public static float FollowDistanceStart = 14400;//距离超过120开始跟随
    public const int BreakChance = 3;//千分之三爆气几率
    public const int CombatChancePerThink = 3;//THINK越大，连击概率越大
    public static float DistanceSkipAngle = 3600;//距离足够近时，不论角度如何都应有感知
    public static float WallForce = 1000;//贴紧墙壁受到的推开速度 
    public int MaxModel = 20;//内置角色模型20个
    public const int DefConvertAngry = 20;//2点伤害的防御=1怒气
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

    private List<LevelData> AllLevel;
    public void ClearLevel()
    {
        AllLevel = null;
    }

    public LevelData[] GetAllLevel()
    {
        if (AllLevel != null)
            return AllLevel.ToArray();
        if (AllLevel == null)
            AllLevel = new List<LevelData>();
        List<LevelData> baseLevel = DataMgr.Ins.GetLevelDatas();
        for (int i = 0; i < baseLevel.Count; i++)
        {
            AllLevel.Add(baseLevel[i]);
        }

        for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginChapter.Count; i++)
        {
            baseLevel = DlcMng.Ins.GetDlcLevel(GameStateMgr.Ins.gameStatus.pluginChapter[i].ChapterId);
            for (int j = 0; j < baseLevel.Count; j++)
            {
                AllLevel.Add(baseLevel[j]);
            }
        }
        return AllLevel.ToArray();
    }

    public LevelData GetLevel(int chapterId, int id)
    {
        if (chapterId == 0)
        {
            LevelData lev = DataMgr.Ins.GetLevelData(id);
            if (lev != null)
                return lev;
        }

        List<LevelData> l = DlcMng.Ins.GetDlcLevel(chapterId);
        for (int i = 0; i < l.Count; i++)
        {
            if (l[i].Id == id)
                return l[i];
        }
        Debug.LogError(string.Format("无法找到指定的剧本{0}关卡{1}", chapterId, id));
        return null;
    }

    public string GetCharacterName(int id)
    {
        if (id >= CombatData.Ins.MaxModel)
        {
            return DlcMng.GetPluginModel(id).Name;
        }
        return DataMgr.Ins.GetModelData(id).Name;
    }

    public static List<WayPoint> GetWayPoint(LevelData level)
    {
        List<WayPoint> wayPoint = new List<WayPoint>();
        string items = level.sceneItems;
        if (!string.IsNullOrEmpty(items)){
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
    public SortedDictionary<int, WayLength> link;
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