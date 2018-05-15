using CoClass;
using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
//public class UpdateFile
//{
//    public string file;
//    public string path;
//    public bool done;
//}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class UpdateVersion
{
    public int Version;//当前版本
    public int TargetVersion;//目标版本
    public List<UpdateFile> FileList = new List<UpdateFile>();
    public List<string> Notices = new List<string>();
    public int Total;
}

public class GameData
{
#if LOCALHOST
    public static string Domain = "127.0.0.1";
#else
    public static string Domain = "www.idevgame.com";
#endif
    public static ushort GatePort = 7200;
    public static TblMng<LangBase> langMng = TblMng<LangBase>.Instance.GetTable();
    public static TblMng<ItemBase> itemMng = TblMng<ItemBase>.Instance.GetTable();
    public static TblMng<ActionBase> actionMng = TblMng<ActionBase>.Instance.GetTable();
    public static TblMng<InputBase> inputMng = TblMng<InputBase>.Instance.GetTable();

    public static ClientVersion clientVersion;
    public static GameState gameStatus;
    public static UpdateVersion updateVersion;

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

    static void SaveVersion()
    {
        FileStream save = null;
        try
        {
            save = File.Open(Application.persistentDataPath + "/" + "game_version.dat", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
        }
        catch
        {

        }
        if (save != null)
        {
            save.SetLength(0);
            clientVersion = new ClientVersion();
            clientVersion.Version = Global.Version;
            Serializer.Serialize<ClientVersion>(save, clientVersion);
            save.Close();
            save = null;
        }
    }

    public static void LoadVersion()
    {
        FileStream save = null;
        try
        {
            save = File.Open(Application.persistentDataPath + "/" + "game_version.dat", FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch
        {

        }
        if (save != null && save.Length != 0)
        {
            try
            {
                clientVersion = Serializer.Deserialize<ClientVersion>(save);
            }
            catch
            {
                clientVersion = null;
            }
            save.Close();
            save = null;
        }
        //没有加载到版本信息，表示是旧版本
        if (clientVersion == null)
        {
            if (File.Exists(Application.persistentDataPath + "/" + "game_state.dat"))
                File.Delete(Application.persistentDataPath + "/" + "game_state.dat");
            SaveVersion();
        }
        else if (clientVersion.Version < Global.Version)
        {
            //加载到存档版本信息，比当前客户端小.
            if (File.Exists(Application.persistentDataPath + "/" + "game_state.dat"))
                File.Delete(Application.persistentDataPath + "/" + "game_state.dat");
            SaveVersion();
        }
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
            gameStatus.Level = 9;
            gameStatus.saveSlot = 0;//默认使用0号存档.
            gameStatus.Language = (int)LanguageType.Ch;//默认使用英文.
            gameStatus.MusicVolume = 50;
            gameStatus.SoundVolume = 50;
            gameStatus.NickName = "昱泉杀手";
            gameStatus.useJoystickOrKeyBoard = false;
            gameStatus.JoyAnchor = new MyVector2(391,340);
            gameStatus.AxisSensitivity = new MyVector2(0.5f, 0.5f);
            gameStatus.MeteorVersion = "9.07";
        }
        Global.MeteorVersion = gameStatus.MeteorVersion;
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

