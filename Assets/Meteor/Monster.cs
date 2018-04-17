using CoClass;
using System;
using System.Collections.Generic;
using UnityEngine;
using ProtoBuf;
//光环类技能，属于较麻烦的一个地方。先不做.
//穿戴装备以及初始化对象顺序
//1：初始化所有战斗对象的基础属性，包括装备的附加属性，但是不包括装备的技能光环。
//2：收集友军，敌军的技能光环【目标是友方的光环】，遍历光环，每一个友军计算光环属性增益。每个光环对全体友军有效，收集中光环若相同，则取光环级别最大者，低者被忽略
//3：收集友军，敌军的技能光环【目标是地方的光环】，遍历光环，每一个敌军计算光环属性减益。每个光环对全体敌军有效，收集中光环若相同，则取光环级别最大者，低者被忽略

//穿戴装备或者脱取装备流程
//1：穿戴一个装备，先脱取同部位的装备【若有】
//2：脱去一个装备，先查看装备是否拥有光环，其次光环是否起作用【有更高级别的光环覆盖他了】，若起作用.则对友军和敌军对光环进行减益运算。运算完毕后，对装备进行减益运算，
//之后，查看是否存在其覆盖过的低级光环或者存在相同的光环【2个3级命令光环，去掉一个，应该还有一个】。若存在低级光环，则计算低级光环的增益效果，对友军或敌军

//穿戴一个装备，若不是替换装备，查看是否能够替换低级光环，能，1：对敌军友军，去掉低级光环的效果，2：自身增加装备属性 3：敌军友军添加光环效果.
[ProtoContract]
public class SkillInfo
{
    [ProtoMember(1)]
    public int skillId;
    [ProtoMember(2)]
    public int level;
    [ProtoMember(3)]
    public int exp;
}

[ProtoContract]
public class MonsterEx
{
    public bool Dead { get { return IsDead; } }
    public int CalcBuffDamage()
    {
        return BuffDamage;
    }

    //计算防御力
    public int CalcBuffDef()
    { 
        return BuffDef;
    }

    public int SpawnDir;
    public int TotalHp
    {
        get
        {
            return HpMax;
        }
    }
    public string name;
    public bool pause;
    public bool IsPlayer;//玩家阵营
    bool IsDead = false;
    public string Name{get{return name;}set{name = value;}}
    public int Model;
    public int HpMax;//如果是主角色，那么这个值是最大值，如果不是主角色，还需要加上HpArmy一起得到最大值
    public int MpMax;
    public int hpCur;
    public int mpCur;
    public int Speed;
    public int Crit;
    public int View;
    public int Think;
    public int Attack1;
    public int Attack2;
    public int Attack3;
    public int SpawnPoint;
    public int AngryValue;
    public int BuffDamage;
    public int BuffDef;
    public string NpcTemplate;
    public void ChangeLang()
    {
        //name 
    }

    public int Weapon;
    public int Weapon2;
    public int Team;
    public MonsterEx()
    {
        IsPlayer = false;
        //ScriptMng.ins.DoScript(Script);
        //NpcTemplate = Script;
        //Model = (int)(double)ScriptMng.ins.GetVariable("Model");
        //name = (string)ScriptMng.ins.GetVariable("Name");
        //Weapon = (int)(double)ScriptMng.ins.GetVariable("Weapon");
        //Weapon2 = (int)(double)ScriptMng.ins.GetVariable("Weapon2");
        Team = 2;
        View = 40;
        //Think = (int)ScriptMng.ins.GetVariable("Think");
        HpMax = 200;
        SpawnPoint = 1;
        Speed = 1000;
        hpCur = HpMax;
        AngryValue = 0;
    }

    public void UpdateAttr()
    {
        ScriptMng.ins.DoScript(NpcTemplate);
        Attack1 = (int)(double)ScriptMng.ins.GetVariable("Attack1");
        Attack2 = (int)(double)ScriptMng.ins.GetVariable("Attack2");
        Attack3 = (int)(double)ScriptMng.ins.GetVariable("Attack3");
    }

    //unit id , 名称, 所在队伍中的下标.
    public bool InitMonster(string Script)
    {
        IsPlayer = false;
        ScriptMng.ins.DoScript(Script);
        NpcTemplate = Script;
        Model = (int)(double)ScriptMng.ins.GetVariable("Model");
        name = (string)ScriptMng.ins.GetVariable("Name");
        Weapon = (int)(double)ScriptMng.ins.GetVariable("Weapon");
        Weapon2 = (int)(double)ScriptMng.ins.GetVariable("Weapon2");
        Team = (int)(double)ScriptMng.ins.GetVariable("Team");
        View = (int)(double)ScriptMng.ins.GetVariable("View") / 10;
        //Think = (int)ScriptMng.ins.GetVariable("Think");
        HpMax = (int)(double)ScriptMng.ins.GetVariable("HP") / 10;
        SpawnPoint = (int)(double)ScriptMng.ins.GetVariable("Spawn");
        Speed = 1000;
        /*
         * string Name	= "金枪侍卫﹒丁";
int Model =	8;
int Weapon = 25;
int Weapon2 = 12;
int Team = 2;
int View = 400;
int Think = 70;
int	Attack1	= 10;
int Attack2 = 20;
int Attack3 = 30;
int	Guard =	40;
int	Dodge =	15;
int Jump = 15;
int Look = 50;
int Burst = 10;
int Aim = 70;
int GetItem = 10;
int Spawn = 2;
int HP = 2500;
	

int g_selfAddr;
int g_self;*/


        hpCur = HpMax;
        mpCur = MpMax;
        return true;
    }

    //脱掉一件物品.
    public void UnEquipItem(InventoryItem item)
    {
        ItemBase unit = item.Info();
        if (unit != null)
        {
            
        }
    }
    

    public InventoryItem EquipItems(InventoryItem item)
    {
        InventoryItem itemUnEquip = null;
        ItemBase unit = item.Info();


        if (unit != null)
        {
            HpMax += unit.HP;
            MpMax += unit.MP;
            hpCur += unit.HP;
            mpCur += unit.MP;
            Speed += unit.Speed;
            if (Speed > 2500)
                Speed = 2500;
            Crit += unit.Crit;
            if (item != null && item.extra != null && unit.Quality != 0)
            {
                //减去额外属性.
                HpMax += item.extra.Hp;
                hpCur += item.extra.Hp;
                MpMax += item.extra.Mp;
                mpCur += item.extra.Mp;
                Speed += item.extra.Speed;
                Crit += item.extra.Crit;
            }
        }

        return itemUnEquip;
    }

    //初始速度1000，后面+的速度，就直接加，跟百分比一个样.
    public void AddSpeed(int speed)
    {
        Speed += speed;
        Speed = Mathf.Clamp(Speed, 1000, 3500);
    }

    public void AddDefence(int def)
    {
        BuffDef += def;
    }

    public void AddDamage(int damage)
    {
        BuffDamage += damage;   
    }

    public void AddHP(int hp)
    {
        hpCur += hp;
        if (hpCur <= 0)
            IsDead = true;
        hpCur = Mathf.Clamp(hpCur, 0, TotalHp);
    }

    public void AddMaxHP(int hpMax)
    {
        HpMax += hpMax;
    }

    public void AddMp(int mp)
    {
        int tmp = mpCur;
        if (tmp + mp > MpMax)
            mp = MpMax - tmp;
        mpCur += mp;
    }

    public void ReduceHp(int damage)
    {
        if (Startup.ins.state.EnableGodMode)
            damage = 0;
        int tmp = hpCur;
        tmp -= damage;
        if (tmp <= 0)
        {
            damage += tmp;
            hpCur = 0;
            IsDead = true;
            return;
        }
        hpCur -= damage;
    }


    public void ResetHp()
    {
        hpCur = TotalHp;
    }

    public void InitPlayer(LevelScriptBase script)
    {
        Team = 1;
        IsPlayer = true;
        Speed = 1000;
        if (script == null)
        {
            name = "孟星魂";
            SpawnPoint = 0;
            SpawnDir = 0;
            
            Weapon = 51;
            Weapon2 = 47;
            HpMax = hpCur = 100;
        }
        else
        {
            name = script.GetPlayerName();
            Model = script.GetPlayerModel();
            SpawnPoint = script.GetPlayerSpawn();
            SpawnDir = script.GetPlayerSpawnDir();//一个Z轴朝内，一个Z轴朝外,角度自己来换吧。
            Weapon = (int)(double)script.GetPlayerWeapon();
            Weapon2 = (int)(double)script.GetPlayerWeapon2();
            HpMax = hpCur = (int)(double)script.GetPlayerMaxHp() / 10;
            if (HpMax == 0)
                hpCur = HpMax = 100;
        }
        View = 50;
        AngryValue = 0;
        IsDead = false;
    }
}

