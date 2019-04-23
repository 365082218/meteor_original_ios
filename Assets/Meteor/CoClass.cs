using System;
using System.Collections.Generic;
using ProtoBuf;

namespace CoClass
{
    [ProtoContract]
    public class RBase
    {
        [ProtoMember(1)]
        public short id;
        [ProtoMember(2)]
        public short cmd;
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

        public ItemBase Info()
        {
            return GameData.Instance.FindItemByIdx(this.Idx);
        }
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
    

    [ProtoContract]
    public class ServerInfo
    {
        public ServerInfo()
        {

        }
        [ProtoMember(1)]
        public string ServerName;
        [ProtoMember(2)]
        public string ServerHost;
        [ProtoMember(3)]
        public string ServerIP;
        [ProtoMember(4)]
        public int ServerPort;
        [ProtoMember(5)]
        public int type;//0：域名 1：ip
    }
}