using System;
using System.Text;
using System.Net.Sockets;
using CoClass;
using System.IO;
using ProtoBuf;
using System.Net;
using protocol;

public enum EKeyList
{
    KL_Defence,//防御
    KL_BreakOut,//爆气
    KL_Crouch,//蹲着
    KL_Help,//救人
    KL_PretendDead,//装死
    KL_Taunt,//嘲讽
    KL_ChangeWeapon,//切换武器1 2
    KL_DropWeapon,//丢弃武器
    KL_KeyW,
    KL_KeyS,
    KL_KeyA,
    KL_KeyD,
    KL_KeyQ,//解除锁定
    KL_Attack,//攻击
    KL_Jump,//跳跃
    KL_Max,
};

/// EInputType
public enum EInputType
{
    EIT_Click = 0,//按下的瞬间触发
    EIT_DoubleClick,
    EIT_Press,//按下后，当按下计时 + 上一帧的时间 > LongPressedTime且按下计时 小于LongPressedTime
    EIT_Release,//不但判断状态，还要判断是不是此帧弹起
    EIT_Pressing,//带ING都只需要判断状态
    EIT_Releasing,
    EIT_ShortRelease,//短按一下后释放 0.1S以内的按键释放，认为是轻按，类似跳，按短一些就是小跳
    EIT_FullPress,//完整按 按下0.1S后 是完整跳
};

public enum EUnitCamp
{
    EUC_KILLALL = 0,	    // 与所有人不和平,盟主模式下，角色的阵营
    EUC_FRIEND = 1,     // 流星雇佣兵或者NPC，帮助流星打Enemy或者KILLALL
    EUC_ENEMY = 2,      // 蝴蝶
    EUC_NONE = 3,    // 与所有人和平 NPC,不攻击，不受击
};

/** 技能伤害类型 */
public enum ESkillHurtType
{
    None = 0,
    /** 1.物理伤害 */
    PhyHurt = 1,
    /** 魔法伤害 */
    MagicHurt = 2,
    /** 绝技伤害 */
    StuntHurt = 3,
    /** Buff伤害 */
    BuffHurt = 4,
    /**治疗*/
    Treat = 5
}

//关卡玩法
public enum ELevelType
{
    Level = 1,//剧情（当前仅实现）
    Challenge = 2,//刷boss副本-当剧情解锁了挑战boss关卡后.挑战一次
    Repeat = 3,//可重复的死斗关卡，除了胜利或者失败无法出去，敌方全灭胜利，我们全灭失败（主角死）
    MiniGame = 4,//限时的小游戏关卡，类似一条道路上有建筑，逐个打破到终点即胜利（或者时间到），或者天上掉下资源，等
    Moba = 5,//MOBA推塔 
    Endless = 6,//千层塔类型，每个此关卡类型的，存档要记录，打到关卡里的哪一层
}

public enum ESceneType
{
    Invalid = 0,
    Update = 1,//更新场景
    Patch = 2,//加载资源场景
    Menu = 3,//主城
    Level = 4,//关卡
    Service = 5,//在服务器上做主机.等其他玩家进来.
}

public enum QualitySprite
{
    Button10_BaseItem_Quality_00,
    Button10_BaseItem_Quality_01,
    Button10_BaseItem_Quality_02,
    Button10_BaseItem_Quality_03,
    Button10_BaseItem_Quality_04,
};

public enum UIFuncType
{
    Build = 1,
    Market = 2,
    Hockshop = 3,
    Produce = 4,
    Train = 5,
}

public enum BuildType
{
    Prison = 0,//牢房
    Quarry = 1,//采石场
    Barrack = 2,//兵营 影响招募 转职等.
    Lumberyard = 3,
    Stable = 4,//马厩 - 商店增加物品 马
    WareHouse = 5,
    Shop,//商店 - 买东西
    PlayerShop,//当铺 - 卖东西
    WeaponMaker,//军器局-开启制作物品.
    WeaponImprover,//强化作坊-可以强化-重铸装备。重铸-会洗去装备的额外属性，以及刷新装备的强化次数.
}

public enum UnitType
{
    Special = 0,//特殊类型，金币 经验
    Equip,//装备
    Key,//钥匙
    Rec,//恢复道具
    Task,//任务道具
    Book,//技能书
    Material,//合成或分解材料
    FunRepeat,//功能材料-调用脚本 无限使用
    FunOnce,//功能材料-调用脚本 使用后数量-1，堆叠数量有用 
}

public enum UnitId
{
    CNY = 999,//元宝
    Tael = 1000,//银两
    Exp = 1001,
    Wood = 1002,
    Stone = 1003,
    Food = 1004,
}

public enum EquipType
{
    Armor = 0,//衣服
    Weapon = 1,//武器
    Shoe = 2,//靴子
    Helmet = 3,//头盔
    Ring = 4,//饰品
    Shield = 5,//披风-可消耗装备。有几率被一击破坏.
}

//与原游戏对应的序号.
//0-剑
//1-飞镖-匕首
//2-火枪
//3-飞镖-匕首
//4-锤子
//5-刀
//6-双刺或者血滴子
//7-
public enum EquipWeaponCode
{
    Dart = 1,//飞镖
    Guillotines = 2,
    Gun = 3,
    Brahchthrust = 4,
    Knife = 5,
    Sword = 6,
    Lance = 7,
    Blade = 8,
    Hammer = 9,
    HeavenLanceA = 10,
    HeavenLanceB = 10,
    HeavenLanceC = 10,
    Gloves = 11,
    NinjaSword = 12,
}

//用来算防御动作，与方向有关
public enum EquipWeaponType
{
    Sword = 0,//剑
    Knife = 1,//匕首
    Gun = 2,//火枪
    Dart = 3,//飞镖
    Hammer = 4,//锤子
    Blade = 5,//刀
    Guillotines = 6,//血滴子
    Lance = 7,//长枪
    Brahchthrust = 8,//分水刺
    HeavenLance = 9,//乾坤拔刀
    //HeavenLanceB = 10,//乾坤居合
    //HeavenLanceC = 11,//乾坤太刀
    Gloves = 10,//拳套
    NinjaSword = 11,//忍刀
}

public enum FunctionType
{
    Inn,//住店
    Shop,//商店
    Make,//制造-合成
    Strengthen,//强化
    Match,//切磋-反复的.或者是指定次数限制的.
    Challenge,//挑战-一次性的
    Move,//传送
}

//所有发出请求都在这里.
public class Common
{
    //请求登录.
    //public static string password;
    //public static void SendLoginRequest(string account, string strPassword, Action<RBase> cb = null)
    //{
    //    LoginData data = new LoginData();
    //    data.cmd = (short)CmdAction.AuthReq;
    //    data.Account = Encoding.UTF8.GetBytes(account);
    //    data.Password = new byte[] { };
    //    password = strPassword;
    //    Exec<LoginData>(ClientProxy.sProxy, data, cb);
    //}

    //请求注册，包头后面 留CMD+密码.
    public static void SendRegRequest(string account, string strPassword)
    {
        //RegData reg = new RegData();
        //reg.cmd = (short)CmdAction.RegReq;
        ////加密数据
        //byte[] acc = Encrypt.EncryptArray(Encoding.UTF8.GetBytes(account));
        //reg.Account = acc;
        //byte[] psw = Encrypt.EncryptArray(Encoding.UTF8.GetBytes(strPassword));
        //reg.Password = psw;
        //Exec<RegData>(ClientProxy.sProxy, reg);
    }

    public static void SendLogoutRequest(uint userid)
    {
        //RBase req = new RBase();
        //req.cmd = (short)CmdAction.LoginOutReq;
        //Exec<RBase>(ClientProxy.sProxy, req);
    }

    public static void SendChatMessage(uint user, SimpleMessage message)
    {
        //message.cmd = (short)CmdAction.ChatReq;
        //Exec<SimpleMessage>(ClientProxy.sProxy, message);
    }

    public static void SendHeartBeat()
    {
        RBase rb = new RBase();
        rb.cmd = (short)CmdAction.HeartReq;
        //Exec<RBase>(ClientProxy.sProxy, rb);
    }

    public static void Exec(Socket s, int msg)
    {
        if (s != null && s.Connected)
        {
            byte[] Length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(8));
            byte[] wIdent = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msg));
            byte[] data = new byte[8];
            Buffer.BlockCopy(Length, 0, data, 0, 4);
            Buffer.BlockCopy(wIdent, 0, data, 4, 4);
            try
            {
                s.Send(data, 8, SocketFlags.None);
            }
            catch (Exception exp)
            {
                Log.WriteError(exp.Message);
            }
        }
    }

    public static void Exec<T>(Socket s, int msg, T rsp)
    {
        if (s != null && s.Connected)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize<T>(ms, rsp);
            byte[] coreData = ms.ToArray();
            int length = 8 + coreData.Length;
            byte[] data = new byte[length];
            Buffer.BlockCopy(coreData, 0, data, 8, coreData.Length);
            byte[] Length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length));
            byte[] wIdent = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msg));
            Buffer.BlockCopy(Length, 0, data, 0, 4);
            Buffer.BlockCopy(wIdent, 0, data, 4, 4);
            try
            {
                s.Send(data, length, SocketFlags.None);
            }
            catch (Exception exp)
            {
                Log.WriteError(exp.Message);
            }
        }
    }

    //public static void OnAuthRsp(RBase rsp, Action<RBase> cb = null)
    //{
    //    RegData data = rsp as RegData;
    //    data.cmd = (short)CmdAction.LoginReq;
    //    string strPassword = Encoding.UTF8.GetString(data.Password);
    //    strPassword += password;

    //    byte[] result = new byte[32];
    //    byte[] content = ASCIIEncoding.Unicode.GetBytes(strPassword);
    //    result = SM3Digest.SM3Digest.SM3(content);
    //    string keySm3 = "";
    //    for (int j = 0; j < 32; j++)
    //        keySm3 += result[j].ToString("X2");
    //    data.Password = Encoding.UTF8.GetBytes(keySm3);
    //    Exec<RegData>(ClientProxy.sProxy, data, cb);
    //}

    public static void SendUpdateGameServer()
    {
        Exec(ClientProxy.sProxy, (int)protocol.MeteorMsg.MsgType.GetRoomReq);
    }

    public static void SendJoinRoom(int roomId)
    {
        JoinRoomReq req = new JoinRoomReq();
        req.roomId = (uint)roomId;
        req.userNick = GameData.Instance.gameStatus.NickName;
        Exec(ClientProxy.sProxy, (int)protocol.MeteorMsg.MsgType.JoinRoomReq, req);
    }

    public static void SendLeaveLevel()
    {
        Exec(ClientProxy.sProxy, (int)MeteorMsg.MsgType.LeaveRoomReq);
    }

    public static void SendEnterLevel(int model, int weapon)
    {
        //UnityEngine.Debug.LogError("sendEnterLevel " + UnityEngine.Time.frameCount);
        EnterLevelReq req = new EnterLevelReq();
        req.camp = 0;//暂时全部为盟主模式
        req.model = (uint)model;
        req.weapon = (uint)weapon;
        Exec(ClientProxy.sProxy, (int)MeteorMsg.MsgType.EnterLevelReq, req);
    }

    public static void SyncFrame(KeyFrame k)
    {
        Exec(ClientProxy.sProxy, (int)MeteorMsg.MsgType.KeyFrameReq, k);
    }

    //public static void SendEnterMap(EntryPoint ept)
    //{
    //    EnterMap req = new EnterMap();
    //    req.account = GameData.user.account;
    //    req.cmd = (short)CmdAction.EnterMapReq;
    //    req.roleId = GameData.RoleId;
    //    req.uid = GameData.user.uid;
    //    req.map = ept;
    //    Exec(ClientProxy.sProxy, req);
    //}

    //public static void SendItemReq(int itemid, ItemOp opcode, int targetIdx, Action<RBase> call)
    //{
    //    ItemReq req = new ItemReq();
    //    req.account = GameData.user.account;
    //    req.cmd = (short)CmdAction.ItemReq;
    //    req.itemId = itemid;
    //    req.roleId = GameData.MainRole.roleId;
    //    req.op = (int)opcode;
    //    Exec(ClientProxy.sProxy, req, call);
    //}

    //public static void SendBattleResult(bool result, int battleId, List<int> monster, Action<RBase> cb)
    //{
    //    BattleResultReq req = new BattleResultReq();
    //    req.cmd = (short)CmdAction.BattleResultReq;
    //    req.account = GameData.user.account;
    //    req.roleId = GameData.MainRole.roleId;
    //    req.result = result ? 1 : 0;
    //    req.battleIdx = battleId;
    //    req.monster = monster;
    //    Exec(ClientProxy.sProxy, req, cb);
    //}
}
    