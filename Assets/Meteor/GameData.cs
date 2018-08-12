using CoClass;
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

public enum LanguageType
{
    Ch,
    En,
}

//整个游戏只有一份的开关状态.就是整个
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GameState
{
    public int saveSlot;//默认使用的存档编号
    public int Language;//语言设置. 0 中文 1 英文
    public float MusicVolume;//设置背景音乐
    public float SoundVolume;//设置声音
    public string ClientId;//IOS GAMECENTER账号。
    public int Quality;//0默认最高,1中_800面,2低_300面
    public int Level;//当前最远通过的关卡
    public string NickName;
    public bool useJoystickOrKeyBoard;//是否使用外设摇杆
    public bool EnableDebugSFX;//战斗UI调试特效是否显示
    public bool EnableDebugStatus;//角色头顶的信息条显示 动作 帧 状态 属性 等信息
    public bool EnableWeaponChoose;//战斗UI控制面板是否显示按钮
    public bool EnableDebugRobot;//调试角色按钮。
    public bool EnableInfiniteAngry;//无限气.
    public bool EnableItemName;//查看掉落武器的名称.-后续分支可能会加入武器装备养成元素.
    public bool EnableGodMode;//一击必杀
    public MyVector2 JoyAnchor;//摇杆坐标.
    public MyVector2 AxisSensitivity;//轴视角转向灵敏度
    public string MeteorVersion;
    public int TargetFrame;//60-30
    public bool ShowWayPoint;//显示路点
    public bool EnableLog;//HIDEBUG日志
    public bool LevelDebug;//关卡内功能
    public bool DisableLock;//无锁定
}

public class GameData
{
    public static TblMng<LangBase> langMng = TblMng<LangBase>.Instance.GetTable();
    public static TblMng<ItemBase> itemMng = TblMng<ItemBase>.Instance.GetTable();
    public static TblMng<ActionBase> actionMng = TblMng<ActionBase>.Instance.GetTable();
    public static TblMng<InputBase> inputMng = TblMng<InputBase>.Instance.GetTable();

    public static ClientVersion clientVersion;
    public static GameState gameStatus;

    //必须放在这里，因为其成员会初始化表格类数据
    public static void InitTable()
    {
        TblCore.Instance.Init();
    }

    public static InventoryItem MakeEquip(int unitIdx)
    {
        ItemBase info = FindItemByIdx(unitIdx);
        if (info == null)
            return null;
        InventoryItem item = new InventoryItem();
        item.Count = 1;
        item.Idx = info.Idx;
        //if (Startup.ins == null)
        //    return item;
        return item;
    }

    public static List<InventoryItem> MakeItems(int unit, uint count)
    {
        ItemBase info = FindItemByIdx(unit);
        if (info == null)
            return null;
        List<InventoryItem> ret = new List<InventoryItem>();
        if (info.MainType == (int)UnitType.Equip)
        {
            for (int i = 0; i < count; i++)
            {
                InventoryItem itequip = MakeEquip(unit);
                if (itequip != null)
                    ret.Add(itequip);
            }
            return ret;
        }

        uint num = 0;
        uint left = 0;
        num = count / (uint)info.Stack;
        left = count % (uint)info.Stack;
        if (count > info.Stack)
        {
            //一次生成不能大于最大堆叠值.
            Debug.Log("unit:" + info.Name + " stack is " + info.Stack + " but make it :" + count);
        }
        
        for (int i = 0; i < num; i++)
        {
            InventoryItem it = new InventoryItem();
            it.Idx = info.Idx;
            it.Count = it.Info().Stack;
            //it.ItemId = GetNextSlot();
            //save.Items[it.ItemId] = it;
            //string text = "得到物品:" + it.Name() + ":" + it.Info().Stack;
            //U3D.PopupTip(text);
            ret.Add(it);
        }

        if (left != 0)
        {
            InventoryItem it = new InventoryItem();
            it.Idx = info.Idx;
            it.Count = left;
            //it.ItemId = GetNextSlot();
            //save.Items[it.ItemId] = it;
            //string text = "得到物品:" + it.Name() + ":" + left;
            //U3D.PopupTip(text);
            ret.Add(it);
        }
        return ret;
    }


    //创建初始物品
    public static List<uint> MakeDefaultItems()
    {
        //初始物品包含,300金币,装备直接填到表里即可.
        List<uint> items = new List<uint>();
        List<InventoryItem> it = new List<InventoryItem>();
        List<ItemBase> tbl = itemMng.GetFullRow();
        for (int i = 0; i < tbl.Count; i++)
        {
            if (tbl[i].MainType != (int)UnitType.Equip)
                continue;
            it = MakeItems(tbl[i].Idx, 1);
            for (int j = 0; j < it.Count; j++)
                items.Add(it[j].ItemId);
        }

        return items;
    }


    //暂停所有与游戏有关的定时器，以及
    public static void Pause()
    {
        pause = true;
        if (MeteorManager.Instance.LocalPlayer != null)
            MeteorManager.Instance.LocalPlayer.controller.LockInput(true);
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.Lock(true);
    }

    public static void Resume()
    {
        pause = false;
        if (MeteorManager.Instance.LocalPlayer != null)
            MeteorManager.Instance.LocalPlayer.controller.LockInput(false);
        if (NGUIJoystick.instance != null)
            NGUIJoystick.instance.Lock(false);
    }

    static bool pause = false;
    public static bool IsPause { get { return pause; } }

    public static bool anotherLogined = false;
    //其他客户端登录相同账号.
    //public static void OnAnotherLogined(RBase rsp)
    //{
    //    U3D.PopupTip(rdat.account + " 在其他处登录");//考虑存档在服务器的问题，这里的本地存档，可能是需要从服务器下载的.
    //    anotherLogined = true;
    //    //保存角色数据,把roleid清理掉.
    //    SaveRoleState();
    //    WsWindow.CloseAll();
    //}

    /*
    public static bool LoadAccount()
    {
        if (account != null)
            return true;
        //如果上次有注册成功的账号,那么加载出来.
        //解压缩，读取。存储时，加密压缩，保存.
        string file = Application.persistentDataPath + "/" + "account.dat";
        if (System.IO.File.Exists(file))
        {
            byte[] buff = Encrypt.DecryptFile(file);
            MemoryStream ms = new MemoryStream(buff);
            try
            {
                account = Serializer.Deserialize<Account>(ms);
            }
            catch
            {
                
            }
        }
        return account != null;
    }
    */


    /*
    public static void SetCurAccount(UserInfo info, string strAccount, string strPassword)
    {
        if (account == null)
            account = new Account();
        logined = true;
        anotherLogined = false;
        user = info;
        account.lastAccount = strAccount;
        account.lastPassword = strPassword;
        AccountInfo acc = new AccountInfo();
        acc.curAccount = strAccount;
        acc.curPassword = strPassword;
        for (int i = 0; i < account.account.Count; i++)
        {
            if (account.account[i].curAccount == strAccount)
            {
                account.account[i].curPassword = strPassword;
                return;
            }
        }
        account.account.Add(acc);
    }
    */
    //1两黄金=10两白银=10贯铜钱=10000文铜钱
    public static string GetMoneyStr(long count)
    {
        long gold = count / 10000;
        long sliver = (count - (gold * 10000))/ 1000;
        long copper = count % 1000;
        string str = (gold == 0 ? "" : gold + "金") + (sliver == 0 ? "" : sliver + "银") + (copper == 0 ? "" : copper + "文");
        if (str == "")
            str = "-";
        return str; 
    }


    
    public static ItemBase FindItemByIdx(int itemid)
    {
        return itemMng.GetRowByIdx(itemid) as ItemBase ;
    }

    public static void LoadState()
    {
        FileStream save = null;
        try
        {
            save = File.Open(Application.persistentDataPath + "/" + "game_state.dat", FileMode.Open, FileAccess.Read, FileShare.Read);
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
            save.Close();
            save = null;
        }
        if (gameStatus == null)
        {
            gameStatus = new GameState();
#if DEBUG_DEV
            gameStatus.Level = LevelMng.Instance.GetAllItem()[LevelMng.Instance.GetAllItem().Length - 1].ID;
#else
            gameStatus.Level = 1;
#endif
            gameStatus.saveSlot = 0;//默认使用0号存档.
            gameStatus.Language = (int)LanguageType.Ch;//默认使用英文.
            gameStatus.MusicVolume = 50;
            gameStatus.SoundVolume = 50;
            gameStatus.NickName = "昱泉杀手";
            gameStatus.JoyAnchor = new MyVector2(391,340);
            gameStatus.AxisSensitivity = new MyVector2(0.5f, 0.5f);
            gameStatus.MeteorVersion = "9.07";
            gameStatus.TargetFrame = 60;
        }
        AppInfo.MeteorVersion = gameStatus.MeteorVersion;
    }

    public static void SaveState()
    {
        try
        {
            FileStream save = File.Open(Application.persistentDataPath + "/" + "game_state.dat", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            save.SetLength(0);
            Serializer.Serialize(save, gameStatus);
            save.Close();
            save = null;
        }
        catch (System.Exception exp)
        {
            WSLog.LogError(exp.Message);
        }
    }

    public static void ResetState()
    {
        if (File.Exists(Application.persistentDataPath + "/" + "game_state.dat"))
            File.Delete(Application.persistentDataPath + "/" + "game_state.dat");
        gameStatus = null;
        LoadState();
    }
    
    //通过配置表得到武器ItemId;//若一个模型被2个武器引用，则返回前者
    public static int GetWeaponCode(string model)
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
public class GlobalUpdate
{
    public static UpdateVersion updateVersion;
    public static void LoadCache()
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

    public static void SaveCache()
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
    public static void ApplyVersion(VersionItem ver, Main loader)
    {
        ConnectWnd.Instance.Close();
        LoadingNotice.Instance.Open();
        LoadingNotice.Instance.SetNotice(ver.strVersionMax, 
            ()=> 
            {
                LoadingNotice.Instance.DisableAcceptBtn();
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
                            LoadingNotice.Instance.UpdateProgress((float)updateVersion.File.Loadbytes / (float)updateVersion.File.Totalbytes , "0");
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
        ()=> 
        {
            LoadingNotice.Instance.Close();
            Main.Ins.GameStart();
        });
        
    }

    public static void DownLoadVersion(VersionItem ver, Main loader)
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

    public static void CleanVersion()
    {
        if (updateVersion != null && updateVersion.File != null)
        {
            if (File.Exists(updateVersion.File.strLocalPath))
                File.Delete(updateVersion.File.strLocalPath);
            updateVersion = null;
        }
    }
}