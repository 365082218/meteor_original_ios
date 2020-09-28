
using UnityEngine;
using ProtoBuf;
using SLua;
using Excel2Json;

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
    public bool IsPlayer;//主玩家
    bool IsDead = false;
    public string Name{get{return name;}set{name = value;}}
    public int Model;
    public int HpMax;//如果是主角色，那么这个值是最大值，如果不是主角色，还需要加上HpArmy一起得到最大值
    public int MpMax;
    public int hpCur;
    public int mpCur;
    public int Speed;
    //public int ActionSpeed;
    public int Crit;
    public int View;//视野直径
    public int Think;
    public int Attack1;
    public int Attack2;
    public int Attack3;
    public int Guard;//防御
    public int Dodge;//逃跑
    public int Jump;//跳跃
    public int Look;//四处看
    public int Burst;//闪避几率-快速移动
    public int Aim;//远程武器命中.火枪-飞镖
    public int GetItem;//夺宝几率，各种镖物 武器 道具 等
    public int SpawnPoint;
    public int AngryValue;
    public int BuffDamage;
    public int BuffDef;
    public string NpcTemplate;
    public void ChangeLang()
    {
        //name 
    }
    public float CombatChance;
    public int Weapon;
    public int Weapon2;
    public int Team;

    public LuaTable sState;

    public MonsterEx(int HPMAX = 2000)
    {
        IsPlayer = false;
        //ScriptMng.ins.DoScript(Script);
        //NpcTemplate = Script;
        //Model = (int)(double)ScriptMng.ins.GetVariable("Model");
        //name = (string)ScriptMng.ins.GetVariable("Name");
        //Weapon = (int)(double)ScriptMng.ins.GetVariable("Weapon");
        //Weapon2 = (int)(double)ScriptMng.ins.GetVariable("Weapon2");
        CombatChance = CombatData.CombatBase + (Random.Range(1, 101) / 100.0f) * CombatData.CombatChance;
        Team = 2;
        View = 600;
        Think = Random.Range(45, 100);
        //默认属性即为机器人属性.
        Attack1 = Random.Range(20, 35);
        Attack2 = Random.Range(30, 45);
        Attack3 = Random.Range(40, 55);
        Guard = Random.Range(5, 12);
        GetItem = Random.Range(1, 5);
        Aim = 80;
        Burst = Random.Range(1, 8);
        Look = Random.Range(1, 8);
        Jump = Random.Range(1, 8);
        HpMax = HPMAX;
        //SpawnPoint = 1;
        Speed = 1000;
        //ActionSpeed = 1;
        hpCur = HpMax;
        AngryValue = 0;
    }

    public void OnStart()
    {
        if (sState != null)
        {
            LuaFunction f = (sState["OnStart"] as LuaFunction);
            f.call(sState);
        }
    }

    public void UpdateAttr()
    {
        //主角属性无法更改
        if (IsPlayer)
            return;
        if (sState == null)
            return;
        Attack1 = (int)(double)sState["Attack1"];
        Attack2 = (int)(double)sState["Attack2"];
        Attack3 = (int)(double)sState["Attack3"];
        Think = (int)(double)sState["Think"];
        Guard = (int)(double)sState["Guard"];
        Jump = (int)(double)sState["Jump"];
        Look = (int)(double)sState["Look"];
        Burst = (int)(double)sState["Burst"];
        Aim = (int)(double)sState["Aim"];
        GetItem = (int)(double)sState["GetItem"];
        View = (int)(double)sState["View"];
    }

    //unit id , 名称, 所在队伍中的下标.
    public bool InitMonster(string Script)
    {
        IsPlayer = false;
        sState = Main.Ins.ScriptMng.DoScript(Script) as LuaTable;
        NpcTemplate = Script;
        Model = (int)(double)sState["Model"];
        name = (string)sState["Name"];
        Weapon = (int)(double)sState["Weapon"];
        Weapon2 = (int)(double)sState["Weapon2"];
        Team = (int)(double)sState["Team"];
        View = (int)(double)sState["View"];
        Think = (int)(double)sState["Think"];
        HpMax = (int)(double)sState["HP"];
        SpawnPoint = (int)(double)sState["Spawn"];
        Speed = 1000;
        Guard = (int)(double)sState["Guard"];
        Jump = (int)(double)sState["Jump"];
        Look = (int)(double)sState["Look"];
        Burst = (int)(double)sState["Burst"];
        Aim = (int)(double)sState["Aim"];
        GetItem = (int)(double)sState["GetItem"];
        Attack1 = (int)(double)sState["Attack1"];
        Attack2 = (int)(double)sState["Attack2"];
        Attack3 = (int)(double)sState["Attack3"];
        CombatChance = CombatData.CombatBase + (Random.Range(1, 101) / 100.0f) * CombatData.CombatChance;
        hpCur = HpMax;
        mpCur = MpMax;
        return true;
    }

    //脱掉一件物品.
    public void UnEquipItem(InventoryItem item)
    {
        ItemData unit = item.Info();
        if (unit != null)
        {
            
        }
    }
    

    //public InventoryItem EquipItems(InventoryItem item)
    //{
    //    InventoryItem itemUnEquip = null;
    //    ItemData unit = item.Info();


    //    if (unit != null)
    //    {
    //        Speed += unit.Speed;
    //        if (Speed > 2500)
    //            Speed = 2500;
    //    }

    //    return itemUnEquip;
    //}

    //仅修改移动速度,非动作速度
    public void MultiplySpeed(float per)
    {
        Speed = (int)((float)Speed * per);
    }

    //public void AddSpeed(int speed)
    //{
    //    Speed += speed;
    //    Speed = Mathf.Clamp(Speed, 1000, 3500);
    //}

    public void AddDefence(int def)
    {
        Debug.Log("add buff def:" + def);
        BuffDef += def;
    }

    public void AddDamage(int damage)
    {
        BuffDamage += damage;   
    }

    public void OnReborn(float max = 0.3f)
    {
        IsDead = false;
        hpCur = Mathf.FloorToInt(HpMax * max);
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
            name = StringUtils.DefaultPlayer;
            SpawnPoint = 0;
            SpawnDir = 0;
            
            Weapon = 51;
            Weapon2 = 47;
            HpMax = hpCur = 10000;
        }
        else
        {
            name = script.GetPlayerName();
            Model = script.GetPlayerModel();
            SpawnPoint = script.GetPlayerSpawn();
            SpawnDir = script.GetPlayerSpawnDir();//一个Z轴朝内，一个Z轴朝外,角度自己来换吧。
            Weapon = (int)(double)script.GetPlayerWeapon();
            Weapon2 = (int)(double)script.GetPlayerWeapon2();
            HpMax = hpCur = (int)(double)script.GetPlayerMaxHp();
            if (HpMax == 0)
                hpCur = HpMax = 1000;
            if (Main.Ins.CombatData.GLevelMode == LevelMode.CreateWorld)
            {
                HpMax = hpCur = 10 * Main.Ins.CombatData.PlayerLife;
                Weapon = U3D.GetWeaponByType(Main.Ins.CombatData.MainWeapon);
                Weapon2 = U3D.GetWeaponByType(Main.Ins.CombatData.SubWeapon);
                Model = Main.Ins.CombatData.PlayerModel;
                name = Main.Ins.DataMgr.GetModelDatas()[Model].Name;
            }
        } 
        View = 500;
        AngryValue = 0;
        IsDead = false;
    }
}

