using System;
using System.Collections.Generic;
using ProtoBuf;

namespace CoClass
{
    [ProtoContract]
    //[ProtoInclude(200, typeof(RegData))]
    //[ProtoInclude(201, typeof(UserInfo))]
    //[ProtoInclude(202, typeof(SimpleMessage))]
    //[ProtoInclude(203, typeof(ChatAck))]
    //[ProtoInclude(204, typeof(ResponGetServerList))]
    //[ProtoInclude(205, typeof(RoleInfo))]
    //[ProtoInclude(206, typeof(AnotherLogined))]
    //[ProtoInclude(207, typeof(EnterGameSvr))]
    //[ProtoInclude(208, typeof(EnterLevel))]
    //[ProtoInclude(209, typeof(EnterMap))]
    //[ProtoInclude(210, typeof(BroadCast))]
    //[ProtoInclude(211, typeof(PlayerEnterMap))]
    //[ProtoInclude(212, typeof(FailedDetail))]
    //[ProtoInclude(213, typeof(ItemReq))]
    //[ProtoInclude(214, typeof(BattleResultReq))]
    //[ProtoInclude(215, typeof(BattleResultRsp))]
    public class RBase
    {
        [ProtoMember(1)]
        public short id;
        [ProtoMember(2)]
        public short cmd;
    }

    //[ProtoContract]
    //public class AnotherLogined : RBase
    //{
    //    [ProtoMember(1)]
    //    public string account;
    //}

    //[ProtoContract]
    //public class Account
    //{
    //    [ProtoMember(1)]
    //    public string lastAccount;
    //    [ProtoMember(2)]
    //    public string lastPassword;
    //    [ProtoMember(3)]
    //    public int lastLine;
    //    [ProtoMember(4)]
    //    public List<AccountInfo> account = new List<AccountInfo>();
    //}

    //[ProtoContract]
    //public class AccountInfo
    //{
    //    [ProtoMember(1)]
    //    public string curAccount;
    //    [ProtoMember(2)]
    //    public string curPassword;
    //}


    //[ProtoContract]
    //public class ResponGetServerList:RBase
    //{
    //    [ProtoMember(1)]
    //    public List<ServerInfo> lst;
    //}

    //[ProtoContract]
    //public class EnterMap:RBase
    //{
    //    [ProtoMember(1)]
    //    public int uid;//中心服分配的ID
    //    [ProtoMember(2)]
    //    public string account;//注册的账号.
    //    [ProtoMember(3)]
    //    public int roleId;//角色ID
    //    [ProtoMember(4)]
    //    public EntryPoint map;
    //    [ProtoMember(5)]
    //    public Dictionary<int, Dictionary<int, SimpleBase>> otherPlayerGroup = new Dictionary<int, Dictionary<int, SimpleBase>>();//此地图其他组队玩家.
    //    [ProtoMember(6)]
    //    public Dictionary<int, SimpleBase> otherPlayer = new Dictionary<int, SimpleBase>();//此地图其他散玩家.
    //    //此地图其他怪物,所有玩家共享的.是分组的,分组的意义在于,单挑打怪,其实很没意思.如果是打一组怪,那么当其他玩家进入时,可以明显的感觉是3V3或者2V2,这样打怪,玩法多一点
    //    [ProtoMember(7)]
    //    public Dictionary<int, Dictionary<int, SimpleBase>> otherMonsterGroup = new Dictionary<int, Dictionary<int, SimpleBase>>();//组怪
    //    [ProtoMember(8)]
    //    public Dictionary<int, SimpleBase> otherMonster = new Dictionary<int, SimpleBase>();//散怪
    //    [ProtoMember(9)]
    //    public Dictionary<int, SimpleBase> forthisMonster = new Dictionary<int, SimpleBase>();//此玩家才可以看见的怪物.剧情怪.
    //    [ProtoMember(10)]
    //    public Dictionary<int, InventoryItem> sceneItems = new Dictionary<int, InventoryItem>();//场景上的物品.
    //    [ProtoMember(11)]
    //    public Dictionary<int, SimpleBase> npc = new Dictionary<int, SimpleBase>();//所有玩家共享的NPC
    //}

    ////请求获取角色的实际物品数据。做好进入游戏的准备.
    //[ProtoContract]
    //public class FailedDetail:RBase
    //{
    //    [ProtoMember(1)]
    //    public string account;//账号，留给游戏服发给网关的时候带上，网关知道客户端是谁.
    //    [ProtoMember(2)]
    //    public int failcode;//错误码，比如地图不相连，那么判断为非法的请求，因为客户端本身有判断。到服务端的只会是非法的.或者正在战斗，还没完就发一个进入地图的请求.
    //    [ProtoMember(3)]
    //    public int lv;      //级别太低，无法挑战，所需级别
    //    [ProtoMember(4)]
    //    public Dictionary<int, int> needItemCount = new Dictionary<int, int>();//需要物品元件 IDX - COUNT ，key是元件ID，value是数量.
    //}

    ////请求获取角色的实际物品数据。做好进入游戏的准备.
    //[ProtoContract]
    //public class EnterLevel:RBase
    //{
    //    [ProtoMember(1)]
    //    public int uid;//中心服分配的ID
    //    [ProtoMember(2)]
    //    public string account;//注册的账号.
    //    [ProtoMember(3)]
    //    public int roleId;//角色ID
    //    [ProtoMember(4)]
    //    public List<InventoryItem> Items = new List<InventoryItem>();//该角色拥有的全部物品.
    //    [ProtoMember(5)]
    //    public List<TaskInfo> tasks = new List<TaskInfo>();//该角色相关的任务状态.
    //}

    ////请求取区角色的所有基础属性.
    //[ProtoContract]
    //public class EnterGameSvr:RBase
    //{
    //    [ProtoMember(1)]
    //    public int uid;//中心服分配的ID
    //    [ProtoMember(2)]
    //    public string account;//注册的账号.
    //    [ProtoMember(3)]
    //    public int line;//要进入的游戏服。
    //}

    //[ProtoContract]
    //public class RoleInfo:RBase
    //{
    //    [ProtoMember(1)]
    //    public int uid;
    //    [ProtoMember(2)]
    //    public string account;
    //    [ProtoMember(3)]
    //    public int line;
    //    [ProtoMember(4)]
    //    public List<RoleInfoDetail> character = new List<RoleInfoDetail>();
    //}

    [ProtoContract]
    public class Building
    {
        //跟建筑出产 - 升级 - 设置工人相关的
        [ProtoMember(1)]
        public uint Idx;//建筑实例ID. 现在Idx == BuildingPropertyIdx 原本是想做可以同时创建一个建筑的多个实例的，现在是一个建筑只能创建一个，然后不断升级
        [ProtoMember(2)]
        public int BuildingPropertyIdx;//建筑原型ID 绑定到建筑元数据
        [ProtoMember(3)]
        public int Level;//建筑的当前等级.
        [ProtoMember(4)]
        public int Worker;//工人数量.当是建筑的时候，点NPC按钮，有升级和加工人 按钮 或者加工人按钮 就会使该建筑的工人数量+1
        [ProtoMember(5)]
        public float WorkTime;//人数不为0时工作就计时.15秒走一个周期，周期数满，产一次物品.
    }

    [ProtoContract]
    public class Role
    {
        [ProtoMember(1)]
        public int roleId;
        [ProtoMember(2)]
        public DefaultRole baseInfo;
        [ProtoMember(3)]
        public List<uint> Inventory = new List<uint>();//背包,仓库产生的所有物品
        [ProtoMember(5)]
        public int lastLevel;
        [ProtoMember(18)]
        public MyVector lastPos;
        [ProtoMember(19)]
        public MyQuaternion lastRotate;
        [ProtoMember(6)]
        public Dictionary<int, List<int>> BattleRecord = new Dictionary<int, List<int>>();//场景的战斗记录.记录了哪个场景，完成了哪几个战斗
        [ProtoMember(7)]
        public Dictionary<int, int> MyArmy = new Dictionary<int, int>();//我方全部部队人员ID-数量，
        [ProtoMember(10)]
        public Dictionary<int, int> Army = new Dictionary<int, int>();//战斗队伍编成,同时出战，不可以大过职业限定
        [ProtoMember(11)]
        public Dictionary<int, List<SkillInfo>> SkillItems = new Dictionary<int, List<SkillInfo>>();//部队原型技能.一支指定的部队
        [ProtoMember(12)]
        public Dictionary<int, List<SkillInfo>> SkillItemsEx = new Dictionary<int, List<SkillInfo>>();
        //大类部队，比如骑兵（1002，轻骑兵，1003，重骑兵),大类部队的技能通过建筑开启，当一个大类技能开启时，所有新招募的小兵都会遍历一次大类，然后增加大类技能.
        [ProtoMember(13)]
        public List<int> OpenedArmyType = new List<int>();//开启了的职业，供招募，只要招募到初级士兵，那么他的转职链是可以完全使用的，这个由转职所需物品限制.
        [ProtoMember(14)]
        public Dictionary<int, bool> EnableUIFunc = new Dictionary<int, bool>();//开启了的功能按钮ID - 点击过还是没有点击过.
        [ProtoMember(15)]
        public List<int> EnableBuild = new List<int>();//开启了的建筑列表.
        [ProtoMember(16)]
        public List<int> EnableMakeList = new List<int>();//可以合成的物品.
        [ProtoMember(17)]
        public List<int> Backpack = new List<int>();//行军物资
        [ProtoMember(7)]
        public Dictionary<int, int> ArmyDead = new Dictionary<int, int>();//死亡的，可以在NPC处复活。
        public void ChangeLang()
        {
            //baseInfo.Name = unit.Name;
        }
    }

    //玩家在场景不需要看到其他玩家的更多属性,只有待2者产生交互时,才会请求查看更详细的信息.
    [ProtoContract]
    public class SimpleBase
    {
        public static implicit operator SimpleBase(DefaultRole real)
        {
            SimpleBase dat = new SimpleBase();
            dat.hp = real.hp;
            dat.hpMax = real.hpMax;
            dat.lv = real.lv;
            dat.mp = real.mp;
            dat.mpMax = real.mpMax;
            dat.Name = real.Name;
            return dat;
        }
        [ProtoMember(1)]
        public int lv;
        [ProtoMember(2)]
        public int hp;
        [ProtoMember(3)]
        public int hpMax;
        [ProtoMember(4)]
        public int mp;
        [ProtoMember(5)]
        public int mpMax;
        [ProtoMember(6)]
        public string Name;
    }

    //最原始的属性，装备属性没叠加到角色上.(仅仅在升级的时候可能发生变化，其他的时候一律不变)
    [ProtoContract]
    public class DefaultRole
    {
        [ProtoMember(1)]
        public uint NextLevel;//暂时未用到.
        //基本属性
        [ProtoMember(2)]
        public int lv;
        [ProtoMember(3)]
        public int hp;
        [ProtoMember(4)]
        public int hpMax;//上限基础值，最大值，用此值加上 装备 BUFF 属性点 即可
        [ProtoMember(5)]
        public int mp;
        [ProtoMember(6)]
        public int mpMax;//基础数值
        [ProtoMember(7)]
        public string Name;
        [ProtoMember(8)]
        public Dictionary<int, uint> equipList = new Dictionary<int, uint>();//装备 实物ID
        [ProtoMember(9)]
        public List<SkillInfo> SkillItem = new List<SkillInfo>();//技能列表
        [ProtoMember(10)]
        public int HpArmy;//部队气血
        [ProtoMember(11)]
        public int DefArmy;//部队防御
        [ProtoMember(12)]
        public int PhysicalAttack;//物理伤害 
        [ProtoMember(13)]
        public int PhysicalDefence;//物理防御 
        [ProtoMember(14)]
        public int Speed;//速度
        [ProtoMember(15)]
        public int CriticalRating;//暴击几率
        [ProtoMember(16)]
        public uint Exp;//经验值
        [ProtoMember(17)]
        public int Idx;//怪物原型编号
        [ProtoMember(18)]
        public int JobLevel;//官衔级别
        [ProtoMember(19)]
        public uint Fame;//声望
        [ProtoMember(20)]
        public uint Leadership;//统率力.
        [ProtoMember(21)]
        public uint SkillExp;//技能历练。用于提升技能等级.
        [ProtoMember(22)]
        public List<int> BookList = new List<int>();//阅读过的书.
        [ProtoMember(23)]
        public int ArmyAttack;//部队攻击
        //属性补正值，上一次升级后，若属性比预期低，则记录差值，下次升级时修正，以免成长过低
        [ProtoMember(24)]
        public int hpAdjust;
        [ProtoMember(25)]
        public int mpAdjust;
        [ProtoMember(26)]
        public int attackAdjust;
        [ProtoMember(27)]
        public int defAdjust;
        [ProtoMember(28)]
        public int AbilityPoint;//剩余属性点.用于技能加点
        [ProtoMember(29)]
        public Dictionary<int, int> skillPoint = new Dictionary<int, int>();
        //记录属性提升点,吃属性道具提升的.
        [ProtoMember(30)]
        public int hpPoint;//1-25气血
        [ProtoMember(31)]
        public int mpPoint;//1-8真气
        [ProtoMember(32)]
        public int attackPoint;//1-4攻击
        [ProtoMember(33)]
        public int defPoint;//1-3防御
    }
    [ProtoContract]
    public class InventoryItem
    {
        [ProtoMember(1)]
        public uint ItemId;//服务器编号,单机唯一标识号
        [ProtoMember(2)]
        public int Idx;//表格中的基础数据序号.
        [ProtoMember(3)]
        public uint Count;//如果是非装备物品，且可堆叠的。那么这个就是一堆。//而且后面的数据都会为空.
        [ProtoMember(4)]
        public AttrExtraItem extra;//如果是可装备的物品，有其附加的额外属性.
        [ProtoMember(5)]
        public int WeaponPos;//武器当前姿态。记录乾坤刀的长枪 短柄 分开，当捡到一件物品的时候，要根据外观设置他武器的姿态，只针对乾坤刀这种可切换单双手的
    }
    [ProtoContract]
    public class AttrExtraItem
    {
        [ProtoMember(1)]
        public int ImproveCnt;//强化次数.初始为0
        [ProtoMember(2)]//额外数据，表明装备被重铸过
        public int Hp;
        [ProtoMember(3)]
        public int HpArmy;//增加部队气血
        [ProtoMember(4)]
        public int DefArmy;//增加部队防御
        [ProtoMember(5)]
        public int AttackArmy;//增加部队攻击
        [ProtoMember(6)]
        public int Mp;
        [ProtoMember(7)]
        public int Damage;//增加攻击
        [ProtoMember(8)]
        public string Prefix;
        [ProtoMember(9)]
        public int Def;
        [ProtoMember(14)]
        public int Speed;
        [ProtoMember(17)]
        public int Crit;
        [ProtoMember(18)]
        public int SizePercent;//基础尺寸的百分比增量 20 = %20
        [ProtoMember(22)]
        public int ImproveCntMax;//最大强化次数 = 1 + （随机[0-品质]×1.5） 普通的最多可以 1 + 1 * 1.5,最大3个等级，4舍5入。最好的可以
        //1 + 5 × 1.5 = 8.5等级。小于9级不会太麻烦，也不会太少.
        [ProtoMember(22)]
        public int Coin;//这个额外属性价值.
    }
    //登录取信息.
    [ProtoContract]
    public class UserInfo:RBase
    {
        [ProtoMember(1)]
        public List<int> friend = new List<int>();
        [ProtoMember(2)]
        public int uid;
        [ProtoMember(3)]
        public string account;
        [ProtoMember(4)]
        public string strName;
        [ProtoMember(5)]
        public string strMail;
        [ProtoMember(6)]
        public string strSays;
        [ProtoMember(7)]
        public int Cny;
        [ProtoMember(8)]
        public bool sex;
        [ProtoMember(9)]
        public string detail;

        public UserInfo(string userid)
        {
            account = userid;
        }
        public UserInfo()
        {

        }
    }

    [ProtoContract]
    public class BattleResultReq:RBase
    {
        [ProtoMember(1)]
        public string account;
        [ProtoMember(2)]
        public int roleId;
        [ProtoMember(3)]
        public int result;//0失败-跳跃场景，使用服务器移动不检查场景相连状态 1胜利
        [ProtoMember(4)]
        public int battleIdx;//战斗ID.
        [ProtoMember(5)]
        public List<int> monster;//战斗怪物.
    }

    [ProtoContract]
    public class BattleResultRsp : RBase
    {
        [ProtoMember(1)]
        public string account;
        [ProtoMember(2)]
        public int roleId;
        [ProtoMember(3)]
        public int result;//0失败-跳跃场景，使用服务器移动不检查场景相连状态 1胜利
        [ProtoMember(4)]
        public Dictionary<int, int> reward;//战利品
    }

    [ProtoContract]
    public class ItemReq:RBase
    {
        [ProtoMember(1)]
        public string account;
        [ProtoMember(2)]
        public int roleId;
        [ProtoMember(3)]
        public int itemId;
        [ProtoMember(4)]
        public int targetIdx;//目标，就是 0-自身 1 同伴1 2同伴2 3宠物/培育兵.
        [ProtoMember(5)]
        public int op;//0扔掉=放到地图上. 等一段时间刷掉. 1装备 2卸下 3使用 4分解 5强化
        [ProtoMember(6)]
        public int result;//0 ok 1 failed
    }

    [Serializable]
    [ProtoContract(AsReferenceDefault = true)]
    public class Node
    {
        [ProtoMember(1)]
        public Node parent;//不能用parent来遍历，否则会导致引用的不是同一个对象的问题。
        [ProtoMember(2)]
        public List<Node> child;
        [ProtoMember(3)]
        public string strName;
        [ProtoMember(4)]
        public bool isFolder;
        [ProtoMember(5)]
        public long size;
        [ProtoMember(6)]
        public ulong resId;
        public Node()
        {

        }
    }

    [ProtoContract]
    public class RegData: RBase
    {
        public RegData()
        {

        }
        [ProtoMember(1)]
        public byte[] Account;
        [ProtoMember(2)]
        public byte[] Password;
    }

    [ProtoContract]
    public class ChatAck:RBase
    {
        public ChatAck()
        {

        }
        [ProtoMember(1)]
        public string talkid;
    }

    [ProtoContract]
    public class SimpleNode
    {
        public SimpleNode()
        {

        }
        [ProtoMember(1)]
        public List<SimpleNode> child;
        [ProtoMember(2)]
        public string strText;
        [ProtoMember(3)]
        public int ImageIndex;
    }

    [ProtoContract]
    public class SimpleMessage:RBase
    {
        public SimpleMessage()
        {

        }
        [ProtoMember(1)]
        public string userid;
        [ProtoMember(2)]
        public string receiveid;
        [ProtoMember(3)]
        public SimpleNode root;
    }

    [ProtoContract]
    public class UploadData
    {
        public UploadData()
        {

        }
        [ProtoMember(1)]
        public string strCurDirectory;
        //key = SM3_hash
        [ProtoMember(2)]
        public Dictionary<string, string> strFiles;
        [ProtoMember(3)]
        public Dictionary<string, byte[]> fileBytes;
    }

    [ProtoContract]
    public class ServerInfo
    {
        public ServerInfo()
        {

        }
        [ProtoMember(1)]
        public int Idx;
        [ProtoMember(2)]
        public string ServerName;
        [ProtoMember(3)]
        public string ServerHost;
        [ProtoMember(4)]
        public string ServerIP;
        [ProtoMember(5)]
        public int ServerPort;
        [ProtoMember(6)]
        public int type;//0：域名 1：ip
    }

    [ProtoContract]
    public class BroadCast:RBase
    {
        [ProtoMember(1)]
        public int broadCmd;
        [ProtoMember(2)]
        public RBase Context;
    }

    //游戏服务器->网关, 通知网关轮流集合,每一个对象发送一个玩家插入的消息
    //网关->客户端,客户端就显示.
    [ProtoContract]
    public class PlayerEnterMap: RBase
    {
        [ProtoMember(1)]
        public List<string> playerInMap = new List<string>();//原本在此场景的角色.
        [ProtoMember(2)]
        public Dictionary<int, SimpleBase> insertPlayer = new Dictionary<int, SimpleBase>();//新增的玩家角色ID,以及相关的基本属性.
    }

    public enum CmdAction
    {
        BEGIN = 0x13,//一个请求 +1 对一个回应, 必然有去有回.
        RegReq,
        RegRsp,
        AuthReq,//登录第一步
        AuthRsp,
        LoginReq,//登录第二步
        LoginRsp,
        LoginOutReq,
        LoginOutRsp,
        ChatReq,
        ChatRsp,
        HeartReq,
        HeartRsp,
        GetSvrReq,
        GetSvrRsp,
        EnterGameSvrReq,
        EnterGameSvrRsp,
        EnterLevelReq,
        EnterLevelRsp,
        EnterMapReq,
        EnterMapRsp,
        ItemReq,
        ItemRsp,
        BattleResultReq,
        BattleResultRsp,
        END_ = 0x0400,
        AnotherLogined,//单步,可以由于客户端到服务器,也可以由服务器到客户端.
        BroadCastReq,//广播,最少要有 广播集合,被广播集合.发送给网关，结构体是 BroadCast
        InsertPlayer,//场景其他玩家进入.
        RemovePlayer,//场景其他玩家离开.
        EnterMapFailRsp,//无法进入地图
        
    }

    public enum BroadCastAction
    {
        BEGIN = 0x13,
        BroadCastInsertPlayer,//广播玩家进入.
        BroadCastRemovePlayer,//广播玩家离开.
        END_,
    }

    public enum ItemOp
    {
        Remove = 0,
        Equip,
        UnEquip,
        Use,
        Decompose,
        Strengthen,
    }
    public enum SkillTarget
    {
        OneArmy,
        AllArmy,
        EnemyArmy,
        EnemyAll,
        Assassin,//刺客
        Swordman,//剑客
        Blademan,//刀客
        Lanceman,//长枪兵
        Marksman,//狙击手
    }

    public enum ArmyType
    {
        Assassin,//刺客
        Swordman,//剑客
        Blademan,//刀客
        Lanceman,//长枪兵
        Marksman,//狙击手
    }

}