
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
        Version = AppInfo.Instance.MeteorVersion.Equals("9.07") ? (int)protocol.RoomInfo.MeteorVersion.V907 : (int)protocol.RoomInfo.MeteorVersion.V107;
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

    public void RegisterDlc(Chapter dlc)
    {
        if (pluginChapter == null)
            pluginChapter = new List<Chapter>();
        pluginChapter.Add(dlc);
        Global.Instance.ClearLevel();//需要刷新
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
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _EnableInfiniteAngry;
        }
        set
        {
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
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
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _EnableGodMode;
        }
        set
        {
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
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
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _Undead;
        }
        set
        {
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
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
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _GodLike;
        }
        set
        {
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
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
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
                return false;
            return _LevelDebug;
        }
        set
        {
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
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
}

public class GameData:Singleton<GameData>
{
    public TblMng<ItemBase> itemMng = TblMng<ItemBase>.Instance.GetTable();
    public TblMng<ActionBase> actionMng = TblMng<ActionBase>.Instance.GetTable();
    public TblMng<InputBase> inputMng = TblMng<InputBase>.Instance.GetTable();
    public GameState gameStatus;

    //必须放在这里，因为其成员会初始化表格类数据
    public void InitTable()
    {
        TblCore.Instance.Init();
    }

    public InventoryItem MakeEquip(int unitIdx)
    {
        ItemBase info = FindItemByIdx(unitIdx);
        if (info == null)
            return null;
        InventoryItem item = new InventoryItem();
        item.Count = 1;
        item.Idx = info.Idx;
        return item;
    }

    public ItemBase FindItemByIdx(int itemid)
    {
        object obj = itemMng.GetRowByIdx(itemid);
        if (obj == null)
            obj = PluginItemMng.Instance.GetItem(itemid);
        if (obj != null)
            return obj as ItemBase ;
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
        }
        else
        {
            //如果成功加载了存档，检查其中的一些成员是否不符合要求
            if (gameStatus.ServerList == null || gameStatus.ServerList.Count == 0)
            {
                ServerInfo svr = new ServerInfo();
                svr.ServerName = "www.idevgame.com";
                svr.ServerPort = 7200;
                svr.type = 0;
                gameStatus.ServerList = new List<ServerInfo>();//用户自定义的服务器列表.
                gameStatus.ServerList.Add(svr);
            }
        }
        
        AppInfo.Instance.MeteorVersion = gameStatus.MeteorVersion;
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
        WeaponBase[] wItems = WeaponMng.Instance.GetAllItem();
        for (int i = 0; i < wItems.Length; i++)
        {
            if (wItems[i].WeaponR == model)
            {
                unitId = wItems[i].ID;
                break;
            }
        }

        if (unitId == -1)
        {
            WeaponBase[] wItems2 = PluginWeaponMng.Instance.GetAllItem();
            for (int i = 0; i < wItems2.Length; i++)
            {
                if (wItems2[i].WeaponR == model)
                {
                    unitId = wItems2[i].ID;
                    break;
                }
            }
        }

        List<ItemBase> items = itemMng.GetFullRow();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].UnitId == unitId && items[i].MainType == 1)
                return items[i].Idx;
        }
        return -1;
    }
}

//处理版本更新相关
public class GlobalUpdate:Singleton<GlobalUpdate>
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
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.UpdateDialogState);
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
            Main.Instance.GameStart();
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