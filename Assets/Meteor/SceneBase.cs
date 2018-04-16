using System.Collections.Generic;
using ProtoBuf;
using System.ComponentModel;
//用于道具记录地点，一共允许记录24个地点,类似饥荒里的路标
    [ProtoContract]
    public class gate
    {
        [ProtoMember(1)]
        public int level;//哪个大地图.
        [ProtoMember(2)]
        public MyVector pos;//哪个坐标.
        [ProtoMember(3)]
        public string name;//保存的名称.
        [ProtoMember(4)]
        public MyQuaternion rotation;//旋转
    }

    //[ProtoContract]
    //public class SceneUnit
    //{
    //    [ProtoMember(1, AsReference = true)]
    //    public Cell Idx;//所属地图.
    //    [ProtoMember(2)]
    //    public string Title = "未定义";//地图单元标题
    //    [ProtoMember(3)]
    //    public string BattleText;//如果有固定战斗,那么有战斗描述前缀.后缀在战斗ID对应的描述文本里.
    //    [ProtoMember(4)]
    //    public string Script;//进入地图,如果没有固定战斗,那么就调用脚本
    //    //[ProtoMember(5)]
    //    //public int Gang;//同盟主帮会ID,注资最大的,2个相同的,看谁先投资的.服务器算,不用填.
    //    //[ProtoMember(6)]
    //    //public Dictionary<int, int> InvestTable = new Dictionary<int, int>();//帮会投资,最大支持5个帮会共同注资一个店铺. 服务器算的,不需要填
    //    [ProtoMember(7)]
    //    public string LvReqExp;//进入地图需要主角色级别.级别需求[关系#级别（>5级别 1#5 =5级别 0#5 <5级别 2#5）]
    //    [ProtoMember(8)]
    //    public Dictionary<int, int> UnitReq = new Dictionary<int, int>();//进入地图需要什么物品多少个.
    //    [ProtoMember(9)]
    //    public int ReqLogic;//2个条件之间的关系
    //    [ProtoMember(10)]
    //    public int BattleId;//固定战斗ID
    //    [ProtoMember(11)]
    //    public List<int> MonsterPool = new List<int>();//怪物池,随机怪物生成,会用怪物池里的设置
    //    [ProtoMember(12)]
    //    public List<int> MapObjects = new List<int>();//NPC列表.
    //    [ProtoMember(13)]
    //    public List<EntryPoint> EntryList = new List<EntryPoint>();//驿站类似,可以连入其他地图,或者自身地图的某个单元格.或者多层楼结构,进入2楼,3楼,每一楼相当于一个小城镇
    //    [ProtoMember(14)]
    //    public Dictionary<int, int> Route = new Dictionary<int, int>();//地图通路.
    //}

    //NPC的功能,就是点NPC,如果NPC可以买,可以卖,可以强化,可以分解,可以对话那么这一系列的功能可以放到NPC的功能列表里,每个NPC填自己需要的即可
    //[ProtoContract]
    //public class NpcFunction
    //{
    //    [ProtoMember(1)]
    //    public int Idx;
    //    [ProtoMember(2)]
    //    public string Name;
    //    [ProtoMember(3)]
    //    public string FullName;//X级 住宿 消耗 50铜钱 回复 150 HP 150 MP 类似. 
    //    [ProtoMember(4)]
    //    public string Script;//功能脚本.
    //    [ProtoMember(5)]
    //    public int Level;//功能等级.每一级,其消耗,产出,限制各不同,比如2级的锻造炉,就一定不能强化过强化次数超过2的物品.
    //                     //住宿(花费金钱,获得HP,MP全满,特技次数全满),
    //                     //打尖(花费少量金钱 HP全满,MP不变,特技次数不变)
    //                     //购买.原价. 售出.
    //                     //点当.70%原价 店铺当
    //                     //强化.
    //                     //分解.
    //                     //合成.
    //    [ProtoMember(6)]
    //    public int Cost;//住宿花费.
    //    [ProtoMember(7)]
    //    public int Hp;//回复满,就是+9999
    //    [ProtoMember(8)]
    //    public int Mp;//回复满,就是+9999
    //    [ProtoMember(9)]
    //    public List<int> items = new List<int>();//功能带的商品列表.售卖功能的时候有用.可以合成的物品,可以分解的物品,可以强化的物品,可以典当的
    //}
    [ProtoContract]
    public class NpcFunction
    {
        [ProtoMember(1)]
        public int FunType;//功能类型
        [ProtoMember(2)]
        public int Idx;//编号
        [ProtoMember(3)]
        public string Name;//名称
        [ProtoMember(4)]
        public string Script;//脚本
    }

    [ProtoContract]
    public class MakeItem
    {
        [ProtoMember(1)]
        public int itemTarget;
        [ProtoMember(2)]
        public Dictionary<int,int> itemSource;
    };

    [ProtoContract]
    public class BuildImproveInfo
    {
        [ProtoMember(1)]
        public int Level;//级别
        //[ProtoMember(2)]
        //public int WorkRound;//生产周期 - 每个周期为20秒 删除
        [ProtoMember(3)]
        public int PerOutput;//单位每人产出倍数 每投入1人 一个完整生产周期生产多少件
        [ProtoMember(4)]
        public int Item;//生产的物品种类.
        //[ProtoMember(5)]
        //public int PerFoodCost;//单位每人每周期 消耗粮食 删除
        //[ProtoMember(6)]
        //public float CostTime;//升级所需时间.1级是直接生成的.只要解锁了一个建筑，那么这个建筑就可以点击弹出界面，可以选择升级 安排工作设置 开启技能等 删除
        [ProtoMember(7)]
        public int CostMoney;//升级或者建造所需金钱
        [ProtoMember(8)]
        public int WorkerCount;//增加囚犯数量
        [ProtoMember(9)]
        public Dictionary<int, int> ItemsToIprove = new Dictionary<int, int>();//升级所需物品
        [ProtoMember(10)]
        public Dictionary<int, int> ItemsToWork = new Dictionary<int, int>();//每一个人消耗物品，需要消耗几个物品.
        [ProtoMember(11)]
        public List<int> EnableArmy = new List<int>();//可提供招募的兵种-招募所需物品在兵种里填写着.
        [ProtoMember(12)]
        public List<int> EnableSkill = new List<int>();//可开启的建筑技能-升级技能所需物品  比如 马厩 - 技能 对所有骑兵 轻甲 AD+10 SPD - 4 重甲 AD+20 SPD - 10
        [ProtoMember(13)]
        public List<int> EnableBuild = new List<int>();//解锁的建筑
        [ProtoMember(14)]
        public List<int> EnableUIFunlist = new List<int>();//解锁的UI功能
        [ProtoMember(15)]
        public List<int> EnableMakelist = new List<int>();//解锁可以制造的物品.

        public BuildImproveInfo Clone()
        {
            BuildImproveInfo obj = MemberwiseClone() as BuildImproveInfo;
            obj.ItemsToIprove.Clear();
            obj.ItemsToWork.Clear();
            obj.EnableArmy.Clear();
            obj.EnableSkill.Clear();
            obj.EnableBuild.Clear();
            obj.EnableMakelist.Clear();
            obj.EnableUIFunlist.Clear();
            foreach (var each in ItemsToWork)
                obj.ItemsToWork.Add(each.Key, each.Value);
            foreach (var each in ItemsToIprove)
                obj.ItemsToIprove.Add(each.Key, each.Value);
            foreach (var each in EnableArmy)
                obj.EnableArmy.Add(each);
            foreach (var each in EnableSkill)
                obj.EnableSkill.Add(each);
            foreach (var each in EnableBuild)
                obj.EnableBuild.Add(each);
            foreach (var each in EnableMakelist)
                obj.EnableMakelist.Add(each);
            foreach (var each in EnableUIFunlist)
                obj.EnableUIFunlist.Add(each);
            return obj;
        }
    }

    //建筑的元数据
    [ProtoContract]
    public class BuildingInfo
    {
        [ProtoMember(1)]
        public int Idx;//建筑元id
        [ProtoMember(2)]
        public bool AllowWork;//允许安排工作
        //[ProtoMember(3)]
        //public bool AllowImprove;//允许升级 - 丢弃
        [ProtoMember(4)]
        public List<BuildImproveInfo> improve;
        [ProtoMember(5)]
        public string Name;//建筑原型名称
        [ProtoMember(6)]
        public string Des;//建筑描述
        [ProtoMember(7)]
        public string Job;//工作名称
    }

    [ProtoContract]
    public class MonsterImproveNode
    {
        [ProtoMember(1)]
        public int targetIdx;//目标职业
        [ProtoMember(2)]
        public Dictionary<int, int> Items;//转职所需物品
        [ProtoMember(3)]
        public int CostMoney;//转职需要金钱
        [ProtoMember(4)]
        public bool needEnableArmy;//转职需要目标职业已解锁.
    }

    [ProtoContract]
    public class MonsterImproveInfo
    {
        [ProtoMember(1)]
        public int sourceIdx;//源职业
        [ProtoMember(2)]
        public List<MonsterImproveNode> target;
    }

    [ProtoContract]
    public class SkillProperty
    {
        [ProtoMember(1)]
        public int SkillIdx;
        [ProtoMember(2)]
        public int Target;//目标
        [ProtoMember(3)]
        public string Name;
        [ProtoMember(4)]
        public int Type;//主动，被动，建筑技能
        [ProtoMember(5)]
        public int ExpOnce;//使用一次技能增加多少经验.
        [ProtoMember(6)]
        public int MaxLv;//最高等级
        [ProtoMember(7)]
        public int Field;//属性项-HP,MP,等
        [ProtoMember(8)]
        public Dictionary<int, LevelSlot> Info;//级别信息
        [ProtoMember(9)]
        public string Description;
    }

    [ProtoContract]
    public class LevelSlot
    {
        [ProtoMember(1)]
        public int Exp;//升级所需经验.
        [ProtoMember(2)]
        public int Round;//回合数 0代表一次性技能，即无持续效果，-1表示状态技能一直持续
        [ProtoMember(3)]
        public int Value;//数值 +200 -200等.
        [ProtoMember(4)]
        public int CostMp;//内力消耗
    }

    [ProtoContract]
    public class ArmyNode
    {
        [ProtoMember(1)]
        public int srcArmy;
        [ProtoMember(2)]
        public List<ArmyConvert> dstArmy = new List<ArmyConvert>();
    }

    [ProtoContract]
    public class ArmyConvert
    {
        [ProtoMember(1)]
        public int targetArmy;
        [ProtoMember(2)]
        public Dictionary<int, int> needItem = new Dictionary<int, int>();
        [ProtoMember(3)]
        public int moneyCost;
    }

    [ProtoContract]
    public class Wanted
    {
        [ProtoMember(1)]
        public int idx;
        [ProtoMember(2)]
        public string Name;
        [ProtoMember(3)]
        public string Info;
        [ProtoMember(4)]
        public int RewardMoney;
        [ProtoMember(5)]
        public int Famous;
        [ProtoMember(6)]
        public int TargetBattle;
    }

    //一个UI按钮上的功能
    [ProtoContract]
    public class UIFunction
    {
        [ProtoMember(1)]
        public int idx;
        [ProtoMember(2)]
        public string Name;
        [ProtoMember(3)]
        public string Script;//脚本内容
    }