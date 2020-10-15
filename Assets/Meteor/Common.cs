using System;
using System.Text;
using System.Net.Sockets;

using System.IO;
using ProtoBuf;
using System.Net;
using protocol;
using Excel2Json;
using UnityEngine;
public enum GetItemType {
    SceneItem = 1,
    PickupItem = 2,
}

public enum OperateType {
    Kill = 1,
    Kick = 2,
    Skick = 3,
}
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

    public ItemData Info()
    {
        return GameStateMgr.Ins.FindItemByIdx(this.Idx);
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
public enum EKeyList
{
    KL_None = -1,
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
    KL_CameraAxisXL,//视角左旋转
    KL_CameraAxisXR,//视角右旋转
    KL_CameraAxisYU,//视角上旋转
    KL_CameraAxisYD,//视角下旋转
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
    EIT_PressingEnough,//按下超过一定时间
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
    EUC_Meteor = EUC_FRIEND,
    EUC_Butterfly = EUC_ENEMY,
};

public enum UnitType
{
    Weapon = 1,//武器
    Item = 2,//道具
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

//所有发出请求都在这里.
public class Common
{
    

    
}
    