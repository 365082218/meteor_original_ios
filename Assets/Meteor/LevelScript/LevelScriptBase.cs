using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.Util;
using System.Linq;

//存储了所有稳定关卡的脚本
public class ScriptBase
{
    public ScriptBase() {

    }
    //非经典关卡时，从主角脚本读取.
    public static int Weapon0 = -1;
    public static int Weapon1 = -1;
    public static int MaxHP = -1;
    public static int Model = -1;
    public static string _PlayerName = "";

    public static void InitPlayerVariable()
    {
        ScriptMng.Ins.CallScript("localPlayer");
        Model = (int)(double)ScriptMng.Ins.GetVariable("Model");
        _PlayerName = (string)ScriptMng.Ins.GetVariable("Name");
        Weapon0 = (int)(double)ScriptMng.Ins.GetVariable("Weapon");
        Weapon1 = (int)(double)ScriptMng.Ins.GetVariable("Weapon2");
        MaxHP = (int)(double)ScriptMng.Ins.GetVariable("HP");
    }

    public static int _GetPlayerWeapon() { InitPlayerVariable(); return Weapon0; }
    public static int _GetPlayerWeapon2() { InitPlayerVariable(); return Weapon1; }
    public static int _GetPlayerMaxHp() { InitPlayerVariable(); return MaxHP; }
    public static int _GetPlayerModel() { InitPlayerVariable(); return Model; }
    public static string _GetPlayerName() { InitPlayerVariable(); return _PlayerName; }

    public static int GameCallBack(string key, int Value)
    {
        if (key == "mod")
            return (int)CombatData.Ins.GGameMode;
        if (key == "end")
            GameOver(Value);
        return Value;
    }

    public void CleanSceneParticle()
    {
        if (SnowParticle != null)
        {
            SnowParticle.Pause();
        }
    }

    GameObject SnowEffect;
    ParticleSystem SnowParticle;
    ParticleSystem.ForceOverLifetimeModule SnowForce;
    ParticleSystem.EmissionModule SnowEmission;
    ParticleSystem.MainModule SnowMain;

    public void Snow()
    {
        SetScene("snow", 1);//雪粒子
        SetScene("snowdensity", 2000);//粒子密度
        SetScene("winddir", 50, 0, 0);//风方向
        SetScene("snowspeed", 20, 100);//雪速度
        SetScene("snowsize", 5, 5);//粒子尺寸
    }

    public void SetScene(string fun, int arg)
    {
        if (fun == "snow")
        {
            if (arg == 1)
            {
                if (SnowEffect == null)
                {
                    SnowEffect = GameObject.Find("Snow_Particle");
                    if (SnowEffect != null)
                    {
                        SnowParticle = SnowEffect.GetComponent<ParticleSystem>();
                        SnowMain = SnowParticle.main;
                        SnowForce = SnowParticle.forceOverLifetime;
                        SnowEmission = SnowParticle.emission;
                        SnowParticle.Play();
                    }
                } else {
                    SnowParticle.Play();
                }
            }
            else
            {
                CleanSceneParticle();
            }
        }
        else if (string.Equals(fun, "snowdensity"))
        {
            if (SnowParticle != null)
            {
                SnowMain.maxParticles = arg;
                SnowEmission.rateOverTime = new ParticleSystem.MinMaxCurve(50, 100);
                SnowMain.startLifetime = 10;
            }
        }
    }

    public void SetScene(string fun, int a, int b, int c)
    {
        if (GameStateMgr.Ins.gameStatus.DisableParticle)
            return;
        //SetScene("winddir", 50, 0, 0);
        if (fun == "winddir")
        {
            //设置风速，类似风场
            if (SnowParticle != null)
            {
                SnowForce.enabled = true;
                SnowForce.space = ParticleSystemSimulationSpace.World;
                SnowForce.x = new ParticleSystem.MinMaxCurve(0, a);
                SnowForce.y = new ParticleSystem.MinMaxCurve(0, b);
                SnowForce.z = new ParticleSystem.MinMaxCurve(0, c);
            }
        }
    }

    public void SetScene(string fun, int a, int b)
    {
        if (GameStateMgr.Ins.gameStatus.DisableParticle)
            return;
        //SetScene("snowspeed", 20, 100);
        //SetScene("snowsize", 5, 5);
        if (string.Equals(fun, "snowspeed"))
        {
            if (SnowParticle != null)
            {
                SnowMain.startSpeed = new ParticleSystem.MinMaxCurve(a, b);
                SnowMain.gravityModifier = 0.5f;
            }
        }
        else if (string.Equals(fun, "snowsize"))
        {
            if (SnowParticle != null)
            {
                SnowMain.startSize3D = false;
                SnowMain.startSize = new ParticleSystem.MinMaxCurve(a, b);
            }
        }
    }

    //返回角色是否存在执行动作序列
    public bool IsPerforming(int player)
    {
        if (Main.Ins.GameBattleEx != null)
            return Main.Ins.GameBattleEx.IsPerforming(player);
        return false;
    }

    public bool IsUnitDead(int unit)
    {
        return U3D.IsUnitDead(unit);
    }

    public static void RemoveNPC(int id)
    {
        U3D.RemoveNPC(id);
    }

    public static void GameOver(int nCode)
    {
        Main.Ins.GameBattleEx.GameOver(nCode);
    }

    public static int GetGameTime()
    {
        return Main.Ins.GameBattleEx.GetGameTime();
    }

    public static bool CanUseSkill(int c)
    {
        MeteorUnit u = U3D.GetUnit(c);
        if (u != null)
            return u.ActionMgr.CanDefence;
        return false;
    }
    public static float PlayerDistance(int player1, int player2)
    {
        MeteorUnit unit1 = U3D.GetUnit(player1);
        MeteorUnit unit2 = U3D.GetUnit(player2);
        if (unit1 != null && unit2 != null)
            return Vector3.Distance(unit1.transform.position, unit2.transform.position);
        return float.MaxValue;
    }

    public static float Distance(int targetIdx, int targetIdx2)
    {
        if (targetDict.ContainsKey(targetIdx) && targetDict.ContainsKey(targetIdx2))
        {
            float s = Vector3.Distance(targetDict[targetIdx], targetDict[targetIdx2]);
            return s;
        }
        return 0;
    }

    static SortedDictionary<int, List<int>> randomSeries = new SortedDictionary<int, List<int>>();
    static SortedDictionary<int, Vector3> targetDict = new SortedDictionary<int, Vector3>();

    public static void Clear()
    {
        randomSeries.Clear();
        targetDict.Clear();
    }

    public static Vector3 GetTarget(int idx)
    {
        if (targetDict.ContainsKey(idx))
            return targetDict[idx];
        return Vector3.zero;
    }

    public static void SetTarget(int idx, string style, int param)
    {
        Vector3 vec = Vector3.zero;
        List<WayPoint> wayPoint = CombatData.Ins.wayPoints;
        if (style == "waypoint")
        {
            if (CombatData.Ins.GLevelItem != null && wayPoint.Count > param)
                vec = wayPoint[param].pos;
        }
        else if (style == "char")
        {
            for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
                if (MeteorManager.Ins.UnitInfos[i].InstanceId == param)
                {
                    vec = MeteorManager.Ins.UnitInfos[i].transform.position;
                    break;
                }
        }
        targetDict[idx] = vec;
    }

    public static void PlayerPerform(string action, int param)
    {
        U3D.PlayerPerform(action, param);
    }

    public static void PlayerPerform(string action, string content)
    {
        U3D.PlayerPerform(action, content);
    }

    List<int> generateRandSeries(int start, int end)
    {
        List<int> ret = new List<int>();
        List<int> l = new List<int>();
        for (int i = start; i <= end; i++)
            l.Add(i);
        while (l.Count != 0)
        {
            int rIndex = Utility.Range(0, l.Count - 1);
            int r = l[rIndex];
            l.RemoveAt(rIndex);
            ret.Add(r);
        }
        return ret;
    }

    public void Misc(string fun, int id, string v)
    {
        if (fun == "transfer")
        {
            //把某人传送到某位置去
            MeteorUnit unit = U3D.GetUnit(id);
            GameObject TargetGo = NodeHelper.Find(v, Loader.Instance.gameObject);
            if (TargetGo != null)
                unit.transform.position = TargetGo.transform.position;
            else
                Debug.LogError(string.Format("can not find:{0}", v));
        }
    }
    //游戏状态等
    public int Misc(string fun, int serices_index = 0, int start = 0, int end = 0)
    {
        if (fun == "gettime")
            return Main.Ins.GameBattleEx.GetMiscGameTime();
        else if (fun == "randomseries")
        {
            randomSeries[serices_index] = generateRandSeries(start, end);
        }
        else if (fun == "getseries")
        {
            return randomSeries[serices_index][start];
        }
        return 0;
    }

    public static void SetSceneItem(string a, string b, int c)
    {
        U3D.SetSceneItem(a, b, c);
    }
    public static void SetSceneItem(int a, string b, string c)
    {
        U3D.SetSceneItem(a, b, c);
    }
    public static void SetSceneItem(string a, string b, string c)
    {
        U3D.SetSceneItem(a, b, c);
    }
    public static void SetSceneItem(string a, string b, string c, int d)
    {
        U3D.SetSceneItem(a, b, c, d);
    }
    public static void SetSceneItem(string name, string features, int value1, int value2)
    {
        U3D.SetSceneItem(name, features, value1, value2);
    }
    public static void SetSceneItem(int id, string features, string sub_feature, int value)
    {
        U3D.SetSceneItem(id, features, sub_feature, value);
    }
    public static void SetSceneItem(int id, string features, int value1, int value2)
    {
        U3D.SetSceneItem(id, features, value1, value2);
    }
    public static int GetSceneItem(string name, string feature)
    {
        return U3D.GetSceneItem(name, feature);
    }
    public static int GetSceneItem(int id, string feature)
    {
        return U3D.GetSceneItem(id, feature);
    }
    public static int GetTeam(int characterId)
    {
        return U3D.GetTeam(characterId);
    }
    public static void CreateEffect(int target, string effect)
    {
        //Debug.Log("create effect:" + effect);
        U3D.CreateEffect(target, string.Format("{0}.ef", effect), false);
    }
    public static void CreateEffect(string target, string effect, bool loop)
    {
        U3D.CreateEffect(target, string.Format("{0}.ef", effect), loop);
    }
    public static void CreateEffect(string target, string effect)
    {
        U3D.CreateEffect(target, string.Format("{0}.ef", effect));
    }
    public static void MakeString(ref string s, string b, int i)
    {
        s = b + string.Format("{0:d2}", i);
    }

    public static void Output(string i, int a, int b)
    {
        //Debug.Log(i +  " index:" + a + " value:" + b);
    }

    public static void Output(string s, int id, string e, int evt)
    {
        Debug.Log(s);
    }

    public static void Output(int i, int j)
    {
        //Debug.Log(string.Format("{0}{1}", i, j));
    }

    public static void Output(int i)
    {
        //Debug.Log(i);
    }
    public static void Output(string s, int i = 0)
    {
        //Debug.Log(s + " index" + i);
    }

    public static int AddNPC(string npc)
    {
        return U3D.AddNPC(npc);
    }
    public static int GetChar(string tag)
    {
        return U3D.GetChar(tag);
    }

    public static void ChangeBehavior(int id, string act, params object[] value)
    {
        U3D.ChangeBehaviorEx(id, act, value);
    }

    public static void Perform(int id, string pose)
    {
        U3D.Perform(id, pose);
    }
    public static void Perform(int id, string pose, int fun)
    {
        U3D.Perform(id, pose, fun);
    }

    public static void Perform(int id, string pose, string fun)
    {
        U3D.Perform(id, pose, fun);
    }

    public static int GetEnemy(int characterid)
    {
        return U3D.GetEnemy(characterid);
    }

    public static void StopPerform(int id)
    {
        U3D.StopPerform(id);
    }

    public static void Say(int id, string content)
    {
        U3D.Say(id, content);
    }

    public static int GetAngry(int id)
    {
        return U3D.GetAngry(id);
    }
    public static int GetHP(int id)
    {
        return U3D.GetHP(id);
    }
    public static int GetMaxHP(int id)
    {
        return U3D.GetMaxHP(id);
    }

    public static int GetAnyChar(string n)
    {
        return U3D.GetAnyChar(n);
    }
    public const int g_iBoxMaxHP = 100;
    // 絚HP
    public const int g_iBBoxMaxHP = 200;
    // 慈HP
    public const int g_iChairMaxHP = 100;
    // HP
    public const int g_iDeskMaxHP = 150;
    // 砒HP
    public const int g_iJugMaxHP = 100;
    // ┶皑HP
    public const int g_iGiMaMaxHP = 3000;

    // special parameter for each level
    public const int g_iLevel01StoneMaxHP = 500;

    public const int g_iLevel03DoorWaitTime = 15000; // milliseconds
    public const int g_iLevel03GiMaMaxHP = 3000;
    public const int g_iLevel03StoneDamage = 300;
    public const int g_iLevel03DoorDamage = 50;

    public const int g_iLevel04GiMaMaxHP = 10000;

    public const int g_iLevel07KnifeDamage = 300;
    public const int g_iLevel07PinDamage = 200;

    public const int g_iLevel08StickDamage = 300;

    public const int g_iLevel09StepTime = 1000; // milliseconds

    public const int g_iLevel11DoorMaxHP = 10000;

    public const int g_iLevel12StoveHP = 5000;

    public const int g_iLevel13BridgeHP = 4000;

    //for box
    public const int g_iNumBoxes = 65;
    static int []g_iBoxHP;
    static int []g_bBoxAlive;

    public const int g_iNumBBoxes = 60;
    static int []g_iBBoxHP;
    static int []g_bBBoxAlive;
    public static void SetSceneItem(int id, System.Func<int, int, int, int> act, System.Action<int, int> idle)
    {
        SceneItemAgent it = U3D.GetSceneItem(id);
        if (it != null)
            it.OnAttack(act, idle);
    }

    public static void SetSceneItem(string name, System.Func<int, int, int, int> act, System.Action<int, int> idle)
    {
        SceneItemAgent it = U3D.GetSceneItem(name);
        if (it != null)
            it.OnAttack(act, idle);
    }

    public virtual void InitBoxes(int num)
    {
        g_iBoxHP = new int[g_iNumBoxes];
        g_bBoxAlive = new int[g_iNumBoxes];
        int i;
        string name = "";

        for (i = 1; i <= num; i++)
        {
            g_bBoxAlive[i - 1] = 1;
            g_iBoxHP[i - 1] = g_iBoxMaxHP;

            MakeString(ref name, "D_BBox", i);
            int id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);
            SetSceneItem(id, BoxOnAttack, BoxOnIdle);
            MakeString(ref name, "D_itBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);

            MakeString(ref name, "D_wpBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }
    }

    static int BoxOnAttack(int id, int index, int damage)
    {
        g_iBoxHP[index - 1] = g_iBoxHP[index - 1] - damage;
        if (g_iBoxHP[index - 1] > 0)
        {
            
            //Output("effect:", id, "BoxHit");
            CreateEffect(id, "BoxHIT");
            
            return 0;
        }

        if (GetSceneItem(id, "pose") == 1)
        {
            return 0;
        }

        string itemname = "";
        string weaponname = "";
        MakeString(ref itemname, "D_itBBox", index);
        MakeString(ref weaponname, "D_wpBBox", index);

        
        CreateEffect(id, "BoxBRK");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(weaponname, "attribute", "interactive", 1);
        

        return 1;
    }

    static int RemoveBox(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        
        SetSceneItem(id, "attribute", "active", 0);
        
        Output("remove item", id);
        return 1;
    }

    static  void BoxOnIdle(int id, int index)
    {
        if (g_bBoxAlive[index - 1] == 1)
        {
            if (RemoveBox(id) == 1)
            {
                g_bBoxAlive[index - 1] = 0;
            }
        }
    }

    public virtual void InitBBoxes(int num)
    {
        int i;
        string name = "";
        g_iBBoxHP = new int[g_iNumBBoxes];
        g_bBBoxAlive = new int[g_iNumBBoxes];
        for (i = 1; i <= num; i++)
        {
            g_bBBoxAlive[i - 1] = 1;
            g_iBBoxHP[i - 1] = g_iBBoxMaxHP;

            MakeString(ref name, "D_BBBox", i);
            int id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);
            SetSceneItem(id, BBoxOnAttack, BBoxOnIdle);
            MakeString(ref name, "D_itBBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);

            MakeString(ref name, "D_wpBBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }
    }

    //id子物件序号，index盒子序号，伤害
    public virtual int BBoxOnAttack(int id, int index, int damage)
    {
        g_iBBoxHP[index - 1] = g_iBBoxHP[index - 1] - damage;
        if (g_iBBoxHP[index - 1] > 0)
        {
            //
            CreateEffect(id, "BoxHIT");
            //
            return 0;
        }

        string itemname = "";
        string weaponname = "";
        MakeString(ref itemname, "D_itBBBox", index);
        MakeString(ref weaponname, "D_wpBBBox", index);

        //
        CreateEffect(id, "BoxBRK");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(weaponname, "attribute", "interactive", 1);
        //

        return 1;
    }

    public virtual int RemoveBBox(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        //
        SetSceneItem(id, "attribute", "active", 0);
        //
        Output("remove item", id);
        return 1;
    }

    public virtual void BBoxOnIdle(int id, int index)
    {
        if (g_bBBoxAlive[index - 1] == 1)
        {
            if (RemoveBBox(id) == 1)
            {
                g_bBBoxAlive[index - 1] = 0;
            }
        }
    }

    //for desk
    public static int g_iNumDeskes = 20;
    public static int[] g_iDeskHP;
    public static int[] g_bDeskAlive;
    public static void InitDeskes(int num)
    {
        int i;
        string name = "";
        g_iDeskHP = new int[g_iNumDeskes];
        g_bDeskAlive = new int[g_iNumDeskes];
        for (i = 1; i <= num; i++)
        {
            g_bDeskAlive[i - 1] = 1;
            g_iDeskHP[i - 1] = g_iDeskMaxHP;

            MakeString(ref name, "D_Desk", i);
            int id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, DeskOnAttack, DeskOnIdle);
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);

            MakeString(ref name, "D_itDesk", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);

            MakeString(ref name, "D_wpDesk", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }
    }

    public static int DeskOnAttack(int id, int index, int damage)
    {
        g_iDeskHP[index - 1] = g_iDeskHP[index - 1] - damage;
        if (g_iDeskHP[index - 1] > 0)
        {
            ////网络同步开始，后续调用都要通知网络层
            CreateEffect(id, "BoxHIT");
            //
        }

        if (GetSceneItem(id, "pose") == 1)
        {
            return 0;
        }

        string itemname = "";
        string weaponname = "";
        MakeString(ref itemname, "D_itDesk", index);
        MakeString(ref weaponname, "D_wpDesk", index);

        //
        CreateEffect(id, "BoxBRK");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(weaponname, "attribute", "interactive", 1);
        //

        return 1;
    }

    public static int RemoveDesk(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        //
        SetSceneItem(id, "attribute", "active", 0);
        //
        Output("remove item", id);
        return 1;
    }

    public static void DeskOnIdle(int id, int index)
    {
        if (g_bDeskAlive[index - 1] == 1)
        {
            if (RemoveDesk(id) == 1)
            {
                g_bDeskAlive[index - 1] = 0;
            }
        }
    }

    //for chair
    public const int g_iNumChairs = 20;
    public static int[] g_iChairHP;
    public static int[] g_bChairAlive;

    public static void InitChairs(int num)
    {
        g_iChairHP = new int[g_iNumChairs];
        g_bChairAlive = new int[g_iNumChairs];
        int i;
        string name = "";

        for (i = 1; i <= num; i++)
        {
            g_bChairAlive[i - 1] = 1;
            g_iChairHP[i - 1] = g_iChairMaxHP;

            MakeString(ref name, "D_Chair", i);
            int id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, ChairOnAttack, ChairOnIdle);
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);

            MakeString(ref name, "D_itChair", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);

            MakeString(ref name, "D_wpChair", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }
    }

    public static int ChairOnAttack(int id, int index, int damage)
    {
        g_iChairHP[index - 1] = g_iChairHP[index - 1] - damage;
        if (g_iChairHP[index - 1] > 0)
        {
            //
            CreateEffect(id, "BoxHIT");
            //
            return 0;
        }

        if (GetSceneItem(id, "pose") == 1)
        {
            return 0;
        }

        string itemname = "";
        string weaponname = "";
        MakeString(ref itemname, "D_itChair", index);
        MakeString(ref weaponname, "D_wpChair", index);

        //
        CreateEffect(id, "BoxBRK");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(weaponname, "attribute", "interactive", 1);
        //

        return 1;
    }

    public static int RemoveChair(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        //
        SetSceneItem(id, "attribute", "active", 0);
        //
        Output("remove item", id);
        return 1;
    }

    public static void ChairOnIdle(int id, int index)
    {
        if (g_bChairAlive[index - 1] == 1)
        {
            if (RemoveChair(id) == 1)
            {
                g_bChairAlive[index - 1] = 0;
            }
        }
    }

    //for jugs
    public const int g_iNumJugs = 20;
    public static int[] g_bJugAlive;
    public static int[] g_iJugHP;

    public const int g_iNumRJugs = 20;
    public static int[] g_bRJugAlive;
    public static int[] g_iRJugHP;

    public static void InitJugs(int num)
    {
        g_bJugAlive = new int[g_iNumJugs];
        g_iJugHP = new int[g_iNumJugs];
        g_bRJugAlive = new int[g_iNumRJugs];
        g_iRJugHP = new int[g_iNumRJugs];

        int i;
        string name = "";
        int id;

        for (i = 1; i <= num; i++)
        {
            g_bJugAlive[i - 1] = 1;
            g_iJugHP[i - 1] = g_iJugMaxHP;

            MakeString(ref name, "D_Jug", i);
            id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, JugOnAttack, JugOnIdle);
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);

            MakeString(ref name, "D_itJug", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);

            MakeString(ref name, "D_wpJug", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }

        int j;
        string itemname = "";

        for (i = 1; i <= g_iNumRJugs; i++)
        {
            g_bRJugAlive[i - 1] = 1;
            g_iRJugHP[i - 1] = g_iJugMaxHP;

            MakeString(ref name, "D_RJug", i);
            id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, RJugOnAttack, RJugOnIdle);
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);

            for (j = 1; j <= 4; j++)
            {
                MakeString(ref name, "D_itRJug", i);
                MakeString(ref itemname, name, j);
                SetSceneItem(itemname, "attribute", "active", 0);
                SetSceneItem(itemname, "attribute", "interactive", 0);

                MakeString(ref name, "D_wpRJug", i);
                MakeString(ref itemname, name, j);
                SetSceneItem(itemname, "attribute", "active", 0);
                SetSceneItem(itemname, "attribute", "interactive", 0);
            }
        }
    }

    public static int JugOnAttack(int id, int index, int damage)
    {
        g_iJugHP[index - 1] = g_iJugHP[index - 1] - damage;
        if (g_iJugHP[index - 1] > 0)
        {
            //
            CreateEffect(id, "WonHIT");
            //
            return 0;
        }

        if (GetSceneItem(id, "pose") == 1)
        {
            return 0;
        }

        string itemname = "";
        string weaponname = "" ;
        MakeString(ref itemname, "D_itJug", index);
        MakeString(ref weaponname, "D_wpJug", index);

        //
        CreateEffect(id, "WonBRK");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(weaponname, "attribute", "interactive", 1);
        //

        return 1;
    }

    public static int RemoveJug(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        //
        SetSceneItem(id, "attribute", "active", 0);
        //
        Output("remove item", id);
        return 1;
    }

    public static void JugOnIdle(int id, int index)
    {
        if (g_bJugAlive[index - 1] == 1)
        {
            if (RemoveJug(id) == 1)
            {
                g_bJugAlive[index - 1] = 0;
            }
        }
    }
    public static int rand(int min, int max)
    {
        return Utility.Range(min, max);
    }
    static int RJugOnAttack(int id, int index, int damage)
    {
        g_iRJugHP[index - 1] = g_iRJugHP[index - 1] - damage;
        if (g_iRJugHP[index - 1] > 0)
        {
            
            CreateEffect(id, "WonHIT");
            
            return 0;
        }

        if (GetSceneItem(id, "pose") == 1)
        {
            return 0;
        }

        string itemname = "";
        string weaponname = "";

        MakeString(ref itemname, "D_itRJug", index);
        MakeString(ref weaponname, "D_wpRJug", index);

        int randx = rand(1, 5);
        //Debug.LogError("打碎随机坛子-得到产出序号:" + randx);
        string ritemname = "";
        string rweaponname = "";

        MakeString(ref ritemname, itemname, randx);
        MakeString(ref rweaponname, weaponname, randx);

        
        CreateEffect(id, "WonBRK");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(ritemname, "attribute", "active", 1);
        SetSceneItem(ritemname, "attribute", "interactive", 1);
        SetSceneItem(rweaponname, "attribute", "active", 1);
        SetSceneItem(rweaponname, "attribute", "interactive", 1);
        

        return 1;
    }

    static int RemoveRJug(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        
        SetSceneItem(id, "attribute", "active", 0);
        
        Output("remove item", id);
        return 1;
    }

    static void RJugOnIdle(int id, int index)
    {
        if (g_bRJugAlive[index - 1] == 1)
        {
            if (RemoveRJug(id) == 1)
            {
                g_bRJugAlive[index - 1] = 0;
            }
        }
    }
}


public class LevelScriptBase:ScriptBase {
    public LevelScriptBase() {

    }
    public virtual SortedDictionary<int, string> GetModel() { return new SortedDictionary<int, string>(); }//返回模型定义ID-文件名
    public virtual int GetRoundTime() { return 60000; }
    public virtual int GetPlayerSpawn() { return 0; }
    public virtual int GetPlayerSpawnDir() { return 0; }
    public virtual int GetPlayerWeapon() { return 0; }
    public virtual int GetPlayerWeapon2() { return 0; }
    public virtual int GetPlayerMaxHp() { return 2500; }
    public virtual int GetPlayerModel() { return 0; }
    public virtual string GetPlayerName() { return StringUtils.DefaultPlayer; }
    public virtual void Scene_OnCharacterEvent(int id, int evt) { }
    public virtual void OnSceneEvent(int character, SceneEvent evt) { }
    public virtual GameResult OnUnitDead(MeteorUnit deadUnit)
    {
        if ((int)GameMode.Rob == CombatData.Ins.GLevelItem.LevelType) {
            return GameResult.None;
        }
        if (U3D.AllEnemyDead())
            return GameResult.Win;
        return GameResult.None;
    }
    //负责关卡剧本等
    //额外呼叫怪物脚本
    public virtual void OnStart()
    {
        if (CombatData.Ins.GLevelItem != null && (!string.IsNullOrEmpty(CombatData.Ins.GLevelItem.StartScript)))
        {
            if (Main.Ins.ScriptMng != null)
                ScriptMng.Ins.CallFunc(CombatData.Ins.GLevelItem.StartScript);
        }
    }

    //部分关卡属于暗杀模式，会在角色脚下加上队长圈圈
    public virtual void OnLateStart()
    {

    }

    public virtual int OnUpdate() { return 0; }
    //负责场景初始化，物件等设置
    public virtual void Scene_OnLoad() { }
    public virtual void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
    }

    public virtual int Scene_OnIdle() { return 0; }
}

//钟乳洞
public class LevelScript_sn01:LevelScriptBase
{
    int g_iStoneMaxHP = 500;
    const int g_iNumStones = 32;
    int []g_iStoneHP;
    int []g_bStoneAlive;

    public override void Scene_OnLoad()
    {

        int i;
        string name = "";

        for (i = 1; i <= g_iNumStones; i++)
        {
            MakeString(ref name, "D_St", i);
            SetSceneItem(name, "name", "machine");
            SetSceneItem(name, "attribute", "damagevalue", 50);
        }

        g_iStoneMaxHP = g_iLevel01StoneMaxHP;
    }

    public override void Scene_OnInit()
    {
        int i;
        string name = "";

        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
        g_iStoneHP = new int[g_iNumStones];
        g_bStoneAlive = new int[g_iNumStones];
        for (i = 1; i <= g_iNumStones; i++)
        {
            g_iStoneHP[i - 1] = g_iStoneMaxHP;
            g_bStoneAlive[i - 1] = 1;

            Output(i, g_iStoneHP[i - 1]);
            MakeString(ref name, "D_St", i);
            SetSceneItem(name, "attribute", "active", 1);
            SetSceneItem(name, "attribute", "damage", 0);
            SetSceneItem(name, "attribute", "collision", 1);
            SetSceneItem(name, "pose", 0, 0);
            SetSceneItem(name, StoneOnAttack, StoneOnIdle);

            MakeString(ref name, "D_itSt", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
            MakeString(ref name, "D_wpSt", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }
    }

    public int RemoveItem(int id)
    {
        int pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }
        int state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }
        
        SetSceneItem(id, "attribute", "active", 0);
        
        return 1;
    }

    public int ActiveStoneItem(int index)
    {
        string stonename = "";
        string itemname = "";
        string weaponname = "";

        MakeString(ref stonename, "D_St", index);
        int pose = GetSceneItem(stonename, "pose");
        if (pose == 1)
        {
            return 0;
        }

        MakeString(ref itemname, "D_itSt", index);
        MakeString(ref weaponname, "D_wpSt", index);

        Output("Active Stone", index);
        
        CreateEffect(stonename, "StoneBRK");
        SetSceneItem(stonename, "pose", 1, 0);
        SetSceneItem(stonename, "attribute", "collision", 0);
        SetSceneItem(stonename, "attribute", "damage", 1);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        
        return 1;
    }

    public int StoneOnAttack(int id, int index, int damage)
    {
        string name = "";
        g_iStoneHP[index - 1] = g_iStoneHP[index - 1] - damage;
        Output(g_iStoneHP[index - 1]);
        MakeString(ref name, "D_st", index);

        
        CreateEffect(name, "StoneHIT");
        

        if (g_iStoneHP[index - 1] <= 0)
        {
            ActiveStoneItem(index);
        }
        return 0;
    }

    public void StoneOnIdle(int id, int index)
    {
        if (g_bStoneAlive[index - 1] == 1)
        {
            if (RemoveItem(id) == 1)
            {
                g_bStoneAlive[index - 1] = 0;
            }
        }
    }

    int RoundTime = 10;
    int PlayerSpawn = 9;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 1;
    int PlayerHP = 1500;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp(){ return PlayerHP;}
    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;
    int trg5 = 0;
    int trg6 = 0;


    public override void OnStart()
    {
        AddNPC("npc01_01");
        AddNPC("npc01_02");
        AddNPC("npc01_03");
        base.OnStart();
    }

    public int GotoLeader(int c)
    {
        int c2 = GetChar("军枪哨兵长");
        if (c2 >= 0)
        {
            ChangeBehavior(c, "follow", c2);
            SetTarget(0, "char", c2);
            ChangeBehavior(c, "attacktarget", 0);
            return 1;
        }

        return 0;
    }

    public int Report(int c1, int c2, int c3)
    {
        if (c1 >= 0 && c3 >= 0)
        {
            SetTarget(0, "char", c1);
            SetTarget(1, "char", c3);

            if (Distance(0, 1) < 100)
            {
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", c3);
                }

                ChangeBehavior(c1, "follow", c3);
                Perform(c1, "pause", 4);
                Perform(c1, "say", "报告哨兵长！发现入侵者！！");
                Perform(c1, "faceto", c3);

                int player = GetChar("player");
                ChangeBehavior(c3, "follow", player);
                Perform(c3, "say", "在那？所有人跟我来！");
                Perform(c3, "pause", 3);
                Perform(c3, "faceto", c1);

                return 1;
            }
        }

        return 0;
    }


    public override int OnUpdate()
    {
        int player = GetChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;
        int c2;
        int c3;

        if (trg0 == 0)
        {
            c = GetChar("军枪哨兵长");
            c2 = GetChar("铁枪哨兵﹒甲");
            c3 = GetChar("铁枪哨兵﹒乙");

            if (c >= 0 && c2 >= 0 && c3 >= 0)
            {
                Perform(c, "say", "你们都听到萧老大说的了，给我注意四周的动静！有任何风吹草动立刻回报！");
                Perform(c, "faceto", c2);

                ChangeBehavior(c2, "patrol", 0, 1, 4, 2, 3);
                Perform(c2, "say", "是！");
                Perform(c2, "faceto", c);

                ChangeBehavior(c3, "patrol", 18, 19, 75, 55, 79, 38, 77, 78, 14, 20);
                Perform(c3, "say", "是！");
                Perform(c3, "pause", 3);
                Perform(c3, "faceto", c);

                trg0 = 1;
            }
        }
        if (trg0 == 1 && trg3 == 0)
        {
            c = GetChar("军枪哨兵长");
            if (c >= 0 && GetEnemy(c) == player)
            {
                c2 = GetChar("铁枪哨兵﹒甲");
                c3 = GetChar("铁枪哨兵﹒乙");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", c);
                    Perform(c2, "say", "是！！");
                    Perform(c2, "pause", 2);
                    Perform(c2, "faceto", c);
                }
                if (c3 >= 0)
                {
                    ChangeBehavior(c3, "follow", c);
                    Perform(c3, "say", "是！！");
                    Perform(c3, "pause", 2);
                    Perform(c2, "faceto", c);
                }

                if (c2 < 0 && c3 < 0)
                {
                    Perform(c, "say", "人呢！？可恶！我就不信我一个人对付不了你！");
                    Perform(c, "pause", 5);
                }

                Perform(c, "say", "来人呀！！");
                Perform(c, "faceto", player);

                trg0 = 2;
            }
        }

        if (trg1 == 0 && trg0 == 1 && trg3 == 0)
        {
            c = GetChar("铁枪哨兵﹒甲");
            if (c >= 0 && GetEnemy(c) == player)
            {
                Perform(c, "guard", 3);
                Perform(c, "say", "你﹒﹒你﹒﹒你是谁！竟敢擅自闯入禁地！找死！");
                Perform(c, "faceto", player);
                trg1 = 1;
            }
        }
        if (trg1 == 1)
        {
            c = GetChar("铁枪哨兵﹒甲");
            if (c >= 0 && GetEnemy(c) != player)
            {
                Perform(c, "say", "奇怪？人跑到那了？？？");
                trg1 = 2;
            }
        }
        if (trg1 == 2 && trg3 == 0)
        {
            c = GetChar("铁枪哨兵﹒甲");
            if (c >= 0 && GetEnemy(c) == player)
            {
                GotoLeader(c);

                Perform(c, "say", "又是你！这下子你跑不掉了！你给我等着！");
                Perform(c, "faceto", player);
                trg1 = 3;
                trg3 = 1;
            }
        }

        if (trg2 == 0 && trg0 == 1 && trg3 == 0)
        {
            c = GetChar("铁枪哨兵﹒乙");
            if (c >= 0 && GetEnemy(c) == player)
            {
                Perform(c, "aggress");
                Perform(c, "say", "哈！正愁着没乐子！让我来试试你的身手吧！");
                Perform(c, "faceto", player);
                trg2 = 1;
            }
        }
        if (trg2 == 1)
        {
            c = GetChar("铁枪哨兵﹒乙");
            if (c >= 0 && GetEnemy(c) != player)
            {
                Perform(c, "say", "奇怪？人跑到那了？？？");
                trg2 = 2;
            }
        }
        if (trg2 == 2 && trg3 == 0)
        {
            c = GetChar("铁枪哨兵﹒乙");
            if (c >= 0 && GetEnemy(c) == player)
            {
                GotoLeader(c);

                Perform(c, "say", "又是你！这下子你跑不掉了！你给我等着！");
                Perform(c, "faceto", player);
                trg2 = 3;
                trg3 = 1;
            }
        }

        if (trg4 == 0 && trg3 == 0)
        {
            c = GetChar("铁枪哨兵﹒甲");
            if (c >= 0 && GetHP(c) <= GetMaxHP(c) / 4)
            {
                GotoLeader(c);
                Perform(c, "say", "可恶！你给我等着！");
                trg4 = 1;
                trg3 = 1;
                trg1 = 3;
            }
        }
        if (trg5 == 0 && trg3 == 0)
        {
            c = GetChar("铁枪哨兵﹒乙");
            if (c >= 0 && GetHP(c) <= GetMaxHP(c) / 4)
            {
                GotoLeader(c);
                Perform(c, "say", "可恶！你给我等着！");
                trg5 = 1;
                trg3 = 1;
                trg2 = 3;
            }
        }

        if (trg3 == 1)
        {
            c = GetChar("军枪哨兵长");
            c2 = GetChar("铁枪哨兵﹒甲");
            c3 = GetChar("铁枪哨兵﹒乙");
            if (Report(c2, c3, c) != 0 || Report(c3, c2, c) != 0)
            {
                trg3 = 2;
            }
        }

        if (trg6 == 0)
        {
            c = GetChar("军枪哨兵长");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "guard", 4);
                Perform(c, "say", "你这小子还真有两下子，来头可不小！！");
                Perform(c, "faceto", player);
                trg6 = 1;
            }
        }
        if (trg6 == 1)
        {
            c = GetAnyChar("军枪哨兵长");
            if (GetHP(c) <= 0)
            {
                Say(c, "呜﹒﹒﹒你别以为﹒﹒你能够杀的了﹒﹒﹒﹒﹒﹒");
                trg6 = 2;
            }
        }
        return 0;
    }
}
//秦皇陵
public class LevelScript_sn02: LevelScriptBase
{
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    int PlayerHP = 3000;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override void Scene_OnLoad()
    {
        int i;
        string name = "";

        // knife	
        for (i = 1; i <= 3; i++)
        {
            MakeString(ref name, "D_knife", i);
            SetSceneItem(name, "name", "machine");

            SetSceneItem(name, "attribute", "damage", 1);
            SetSceneItem(name, "attribute", "damagevalue", 300);
        }

        // stone step
        for (i = 1; i <= 3; i++)
        {
            MakeString(ref name, "D_sn02st", i);
            SetSceneItem(name, "name", "machine");
            SetSceneItem(name, "attribute", "collision", 1);
        }
    }

    public override void Scene_OnInit()
    {
        SetSceneItem("D_knife01", "pose", 1, 1);
        SetSceneItem("D_knife01", "frame", 0);

        SetSceneItem("D_knife02", "pose", 1, 1);
        SetSceneItem("D_knife02", "frame", 30);

        SetSceneItem("D_knife03", "pose", 1, 1);
        SetSceneItem("D_knife03", "frame", 60);

        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);

        SetSceneItem("D_sn02st01", "attribute", "active", 0);
        SetSceneItem("D_sn02st02", "attribute", "active", 0);
        SetSceneItem("D_sn02st03", "attribute", "active", 0);

        SetSceneItem("D_IPItem01", "attribute", "active", 1);
    }

    void D_IPItem01_OnPickUp()
    {
        //

        SetSceneItem("D_sn02st01", "attribute", "active", 1);
        SetSceneItem("D_sn02st02", "attribute", "active", 1);
        SetSceneItem("D_sn02st03", "attribute", "active", 1);

        CreateEffect("D_sn02st01", "FuTi-UP", true);
        SetSceneItem("D_sn02st01", "pose", 1, 1);
        SetSceneItem("D_sn02st01", "frame", 0);

        SetSceneItem("D_sn02st02", "pose", 1, 1);
        SetSceneItem("D_sn02st02", "frame", 60);

        SetSceneItem("D_sn02st03", "pose", 1, 1);
        SetSceneItem("D_sn02st03", "frame", 120);
    }
}

//一线天
public class LevelScript_sn03 : LevelScriptBase
{
    // 一线天
    int RoundTime = 15;
    int PlayerSpawn = 53;
    int PlayerSpawnDir = 180;
    int PlayerWeapon = 15;//ItemId=>UnitID模型
    int PlayerWeapon2 = 0;
    int PlayerHP = 1500;

    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;
    int trg5 = 0;
    int trg8 = 0;
    int trg9 = 0;
    int timer0 = 0;
    int timer1 = 0;
    int gameover = 0;
    int now = 0;

    public override void OnStart()
    {
        AddNPC("npc03_01");
        AddNPC("npc03_02");
        AddNPC("npc03_03");
        AddNPC("npc03_04");
        AddNPC("npc03_05");
        AddNPC("npc03_06");
        base.OnStart();
    }

    int FindEnemy(int c, int p)
    {
        int c2;

        if (c < 0)
        {
            return 0;
        }

        if (GetEnemy(c) == p)
        {
            c2 = GetChar("疾剑哨兵长");
            if (c2 >= 0)
            {
                Perform(c, "say", "报告哨兵长！发现入侵者！");

                ChangeBehavior(c2, "follow", p);
                Perform(c2, "say", "所有人给我拿下入侵者！！别让他闯关了！");
                Perform(c2, "pause", 3);
            }
            return 1;
        }

        return 0;
    }

    public override int OnUpdate()
    {
        int player = GetAnyChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;
        int c2;
        int flag;

        if (trg9 == 0)
        {
            PlayerPerform("say", "（刚才那巨响是﹒﹒﹒﹒？）");
            trg9 = 1;
        }

        if (trg0 == 0)
        {
            c = GetChar("夜猫子");
            if (c >= 0)
            {
                SetTarget(0, "waypoint", 119);      // near flag position
                SetTarget(1, "char", player);
                if (Distance(0, 1) < 400)
                {
                    //Debug.LogError("distance < 400");
                    ChangeBehavior(c, "follow", "flag");
                    Perform(c, "say", "哈哈哈哈，压成肉酱了吧！！");
                    SetTarget(0, "waypoint", 83);   // stone position
                    ChangeBehavior(c, "attacktarget", 0, 3);
                    trg0 = 1;
                }
            }
        }

        if (trg1 == 0)
        {
            c = GetChar("土匪﹒铁胡子");
            if (c >= 0)
            {
                if (GetChar("flag") == c)
                {
                    Perform(c, "say", "马上来看看这里面放着啥麽好玩意儿～");
                    Perform(c, "pause", 8);
                    Perform(c, "say", "哇哈哈哈！这个宝物得来完全不费工夫呀！！");
                    trg1 = 1;
                }
            }
        }
        if (trg1 == 1)
        {
            c = GetChar("土匪﹒铁胡子");
            if (c >= 0 && GetEnemy(c) == player)
            {
                ChangeBehavior(c, "patrol", 92);
                Perform(c, "guard", 5);
                Perform(c, "say", "你是谁，该不会是想要来抢我的宝物吧！门都没有，只有死路一条！");
                Perform(c, "faceto", player);
                trg1 = 2;
            }
        }
        if (trg1 == 2)
        {
            c = GetChar("土匪﹒铁胡子");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2 && GetEnemy(c) == player)
            {
                SetTarget(0, "waypoint", 92);
                ChangeBehavior(c, "attacktarget", 0);

                Perform(c, "say", "难得见到你这好身手，改天再跟你玩儿！後会有期了！");
                Perform(c, "faceto", player);
                trg1 = 3;
                timer0 = GetGameTime() + 60;
            }
        }
        if (trg1 == 3)
        {
            c = GetChar("土匪﹒铁胡子");
            if (c >= 0 && GetGameTime() > timer0 && GetEnemy(c) == player)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", player);
                if (Distance(0, 1) < 100)
                {
                    ChangeBehavior(c, "follow", player);
                    Perform(c, "guard", 3);
                    Perform(c, "say", "你﹒﹒﹒竟然追的上我，看样子得跟你来硬的了！");
                    Perform(c, "faceto", player);
                    trg1 = 4;
                }
            }
        }
        if (trg1 > 0 && trg1 != 5)
        {
            c = GetAnyChar("土匪﹒铁胡子");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "我﹒﹒我的宝物﹒﹒﹒﹒");
                trg1 = 5;
            }
        }

        if (trg2 == 0)
        {
            c = GetChar("夜猫子");
            c2 = GetChar("土匪﹒铁胡子");
            if (c >= 0 && c2 >= 0 && GetEnemy(c) == c2 && GetEnemy(c2) == c)
            {
                flag = GetChar("flag");
                if (flag == c)
                {
                    Perform(c2, "aggress");
                    Perform(c2, "say", "看我不把你剁成碎片不成！");
                    Perform(c2, "faceto", c);
                    trg2 = 1;
                }

                if (flag == c2)
                {
                    Perform(c2, "pause", 4);
                    Perform(c, "aggress");
                    Perform(c, "say", "你还是乖乖把宝物交出来吧！");
                    Perform(c, "faceto", c2);

                    Perform(c2, "aggress");
                    Perform(c2, "say", "你这副贱嘴脸！看了就讨打！！");
                    Perform(c2, "pause", 4);
                    Perform(c2, "faceto", c);
                    trg2 = 1;
                }
            }
        }

        if (trg3 == 0)
        {
            c = GetChar("夜猫子");
            if (c >= 0 && GetEnemy(c) == player)
            {
                Perform(c, "aggress");
                Perform(c, "say", "嘻嘻嘻！");
                Perform(c, "faceto", player);

                PlayerPerform("say", "﹒﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 4);

                trg3 = 1;
            }
        }
        if (trg3 == 1)
        {
            c = GetChar("夜猫子");
            if (c >= 0 && GetEnemy(c) == player)
            {
                if (GetChar("flag") == c)
                {
                    ChangeBehavior(c, "run");
                    Perform(c, "say", "宝物在我身上有本事就过来抢吧！");
                    Perform(c, "faceto", player);

                    PlayerPerform("say", "﹒﹒﹒﹒﹒﹒");
                    PlayerPerform("pause", 5);

                    trg3 = 2;
                }
            }
        }

        if (trg4 == 0)
        {
            c = GetChar("flag");
            if (c == player)
            {
                c2 = GetChar("夜猫子");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", "flag");
                }

                PlayerPerform("say", "（看样子要通过这里的关卡不简单！只好先开启机关门，再迅速闯入）");
                PlayerPerform("pause", 3);
                PlayerPerform("say", "（这应该就是'高老大'，所说的通关信物吧！）");
                trg4 = 1;
            }
        }
        if (trg4 == 1)
        {
            c = GetChar("土匪﹒铁胡子");
            if (c >= 0 && GetEnemy(c) == player)
            {
                Perform(c, "attack");
                Perform(c, "say", "乖乖留下你身上的所有东西当作买路财吧！否则讨打！");
                Perform(c, "faceto", player);
                trg4 = 2;
            }
        }

        if (trg5 == 0)
        {
            c = GetChar("火枪哨兵﹒甲");
            if (FindEnemy(c, player) != 0)
            {
                trg5 = 1;
            }
        }

        if (trg5 == 0)
        {
            c = GetChar("火枪哨兵﹒乙");
            if (FindEnemy(c, player) != 0)
            {
                trg5 = 1;
            }
        }

        if (trg5 == 0)
        {
            c = GetChar("火枪哨兵﹒丙");
            if (FindEnemy(c, player) != 0)
            {
                trg5 = 1;
            }
        }

        if (trg5 == 0 && trg8 == 0)
        {
            c = GetChar("疾剑哨兵长");
            if (c >= 0 && GetEnemy(c) == player)
            {
                ChangeBehavior(c, "follow", player);
                Perform(c, "say", "所有人给我拿下入侵者！！别让他闯关了！");
                Perform(c, "pause", 3);
                Perform(c, "faceto", player);
                trg8 = 1;
            }
        }


        now = GetGameTime();
        if (gameover == 0 && GetHP(player) <= 0)
        {
            gameover = -1;
            timer1 = now + 2;
        }
        if ((gameover == 1 || gameover == -1) && now > timer1)
        {
            GameOver(gameover);
            gameover = 2;
        }

        return 0;
    }

    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override GameResult OnUnitDead(MeteorUnit deadUnit)
    {
        //通关由拾取了镖物后走到过关区
        return GameResult.None;
    }
    //负责关卡物件属性等
    int g_bStone01Active;
    int g_bStone02Active;

    int g_iADoor02OpenTime;
    int g_iBDoor01OpenTime;
    int g_iDoorWaitTime = 12000;

    int g_iPdoorMaxHP = 2000;
    int g_iPdoorState1HP;
    int g_iPdoorState2HP;
    int g_iPdoorState3HP;
    int g_iPdoorState4HP;
    int g_iPdoorState5HP;

    int g_bAPdoorAlive;
    int g_iAPdoorState;
    int g_iAPdoorShakePose;
    int g_iAPdoorHP;

    int g_bBPdoorAlive;
    int g_iBPdoorState;
    int g_iBPdoorShakePose;
    int g_iBPdoorHP;

    public override void Scene_OnLoad()
    {
        int i;
        string name = "";

        g_iDoorWaitTime = g_iLevel03DoorWaitTime;
        g_iPdoorMaxHP = g_iLevel03GiMaMaxHP;

        g_iPdoorState1HP = (g_iPdoorMaxHP * 3) / 4;
        g_iPdoorState2HP = (g_iPdoorMaxHP * 2) / 4;
        g_iPdoorState3HP = (g_iPdoorMaxHP * 1) / 4;

        SetSceneItem("D_ston01", "name", "machine");
        SetSceneItem("D_ston02", "name", "machine");

        for (i = 1; i <= 10; i++)
        {
            MakeString(ref name, "D_sn03t", i);
            SetSceneItem(name, "name", "machine");
            SetSceneItem(name, "attribute", "damage", 1);
        }

        SetSceneItem("D_Abutton01", "name", "machine");
        SetSceneItem("D_Abutton02", "name", "machine");
        SetSceneItem("D_ADoor01", "name", "machine");
        SetSceneItem("D_ADoor01", "attribute", "collision", 1);
        SetSceneItem("D_ADoor01", "attribute", "damagevalue", g_iLevel03DoorDamage);
        SetSceneItem("D_ADoor01", "attribute", "damage", 1);

        SetSceneItem("D_Bbutton01", "name", "machine");
        SetSceneItem("D_Bbutton02", "name", "machine");
        SetSceneItem("D_BDoor01", "name", "machine");
        SetSceneItem("D_BDoor01", "attribute", "collision", 1);
        SetSceneItem("D_BDoor01", "attribute", "damagevalue", g_iLevel03DoorDamage);
        SetSceneItem("D_BDoor01", "attribute", "damage", 1);

        SetSceneItem("D_APdoor01", "name", "machine");
        SetSceneItem("D_APdoor01", "attribute", "damagevalue", 200000);

        SetSceneItem("D_BPdoor01", "name", "machine");
        SetSceneItem("D_BPdoor01", "attribute", "damagevalue", 200000);

        SetSceneItem("D_APd02Box01", "name", "machine");
        SetSceneItem("D_BPd02Box01", "name", "machine");
    }

    public override void Scene_OnInit()
    {
        g_bStone01Active = 1;
        g_bStone02Active = 1;

        SetSceneItem("D_ston01", "pose", 0, 0);
        SetSceneItem("D_ston01", "attribute", "active", 1);
        SetSceneItem("D_ston01", "attribute", "collision", 1);
        SetSceneItem("D_ston01", "attribute", "damage", 0);
        SetSceneItem("D_ston01", "attribute", "damagevalue", g_iLevel03StoneDamage);

        SetSceneItem("D_ston02", "pose", 0, 0);
        SetSceneItem("D_ston02", "attribute", "active", 1);
        SetSceneItem("D_ston02", "attribute", "collision", 1);
        SetSceneItem("D_ston02", "attribute", "damage", 0);
        SetSceneItem("D_ston02", "attribute", "damagevalue", g_iLevel03StoneDamage);

        SetSceneItem("D_Abutton01", "pose", 0, 0);
        SetSceneItem("D_Abutton02", "pose", 0, 0);
        SetSceneItem("D_ADoor01", "pose", 0, 0);

        SetSceneItem("D_Bbutton01", "pose", 0, 0);
        SetSceneItem("D_Bbutton02", "pose", 0, 0);
        SetSceneItem("D_BDoor01", "pose", 0, 0);

        SetSceneItem("D_APdoor01", "pose", 0, 0);
        SetSceneItem("D_APdoor01", "attribute", "collision", 0);
        SetSceneItem("D_APdoor01", "attribute", "damage", 0);
        SetSceneItem("D_APd02Box01", "attribute", "collision", 1);

        SetSceneItem("D_BPdoor01", "pose", 0, 0);
        SetSceneItem("D_BPdoor01", "attribute", "collision", 0);
        SetSceneItem("D_BPdoor01", "attribute", "damage", 0);
        SetSceneItem("D_BPd02Box01", "attribute", "collision", 1);

        g_iAPdoorHP = g_iPdoorMaxHP;
        g_bAPdoorAlive = 1;
        g_iAPdoorState = 1;
        g_iAPdoorShakePose = 1;

        g_iBPdoorHP = g_iPdoorMaxHP;
        g_bBPdoorAlive = 1;
        g_iBPdoorState = 1;
        g_iBPdoorShakePose = 1;

        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
    }

    public int D_APdoor01_OnAttack(int id, int character, int damage)
    {
        if (GetTeam(character) == 1)
        {
            return 0;
        }

        int state;

        g_iAPdoorHP = g_iAPdoorHP - damage;

        if (g_iAPdoorState == 1 && g_iAPdoorHP < g_iPdoorState1HP)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            g_iAPdoorShakePose = 3;
            
            SetSceneItem(id, "pose", 2, 0);
            
        }

        if (g_iAPdoorState == 2 && g_iAPdoorHP < g_iPdoorState2HP)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            g_iAPdoorShakePose = 5;
            
            SetSceneItem(id, "pose", 4, 0);
            
        }

        if (g_iAPdoorState == 3 && g_iAPdoorHP < g_iPdoorState3HP)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            g_iAPdoorShakePose = 7;
            
            SetSceneItem(id, "pose", 6, 0);
            
        }

        if (g_iAPdoorState == 4 && g_iAPdoorHP < 0)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            
            CreateEffect(id, "GiMaBRK");
            SetSceneItem(id, "attribute", "interactive", 0);
            SetSceneItem(id, "attribute", "collision", 0);
            SetSceneItem(id, "pose", 8, 0);
            SetSceneItem("D_APd02Box01", "attribute", "active", 0);
            
        }

        state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", g_iAPdoorShakePose, 0);
            
        }
        return 0;
    }

    public int D_APdoor01_OnIdle(int id)
    {
        if (g_iAPdoorState == 5 && g_bAPdoorAlive == 1)
        {
            int pose;
            pose = GetSceneItem(id, "pose");
            if (pose != 8)
            {
                return 0;
            }
            int state;
            state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bAPdoorAlive = 0;
                
                SetSceneItem("D_APdoor01", "attribute", "active", 0);
                
            }
        }
        return 0;
    }

    public int D_BPdoor01_OnAttack(int id, int character, int damage)
    {
        if (GetTeam(character) == 2)
        {
            return 0;
        }

        int state;

        g_iBPdoorHP = g_iBPdoorHP - damage;

        if (g_iBPdoorState == 1 && g_iBPdoorHP < g_iPdoorState1HP)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            g_iBPdoorShakePose = 3;
            
            SetSceneItem(id, "pose", 2, 0);
            
        }

        if (g_iBPdoorState == 2 && g_iBPdoorHP < g_iPdoorState2HP)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            g_iBPdoorShakePose = 5;
            Output("Change State 3");
            
            SetSceneItem(id, "pose", 4, 0);
            
        }

        if (g_iBPdoorState == 3 && g_iBPdoorHP < g_iPdoorState3HP)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            g_iBPdoorShakePose = 7;
            Output("Change State 4");
            
            SetSceneItem(id, "pose", 6, 0);
            
        }

        if (g_iBPdoorState == 4 && g_iBPdoorHP < g_iPdoorState4HP)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            
            CreateEffect(id, "GiMaBRK");
            SetSceneItem(id, "attribute", "interactive", 0);
            SetSceneItem(id, "attribute", "collision", 0);
            SetSceneItem(id, "pose", 8, 0);
            SetSceneItem("D_BPd02Box01", "attribute", "active", 0);
            
        }

        state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", g_iBPdoorShakePose, 0);
            
        }
        return 0;
    }

    public int D_BPdoor01_OnIdle(int id)
    {
        if (g_iBPdoorState == 5 && g_bBPdoorAlive == 1)
        {
            int pose = GetSceneItem(id, "pose");
            if (pose != 8)
            {
                return 0;
            }
            int state;
            state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bBPdoorAlive = 0;
                
                SetSceneItem("D_BPdoor01", "attribute", "active", 0);
                
            }
        }
        return 0;
    }

    public int D_ston01_OnAttack(int id, int character, int damage)
    {
        int pose = GetSceneItem(id, "pose");
        if (pose == 1)
        {
            return 0;
        }

        
        CreateEffect(id, "StoneFIL");
        CreateEffect("D_Sston01", "StoneFIL");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(id, "attribute", "damage", 1);
        
        return 0;
    }

    public int D_ston01_OnIdle(int id)
    {
        if (g_bStone01Active == 1)
        {
            int pose = GetSceneItem(id, "pose");
            if (pose == 0)
            {
                return 0;
            }
            int state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bStone01Active = 0;
                
                SetSceneItem(id, "attribute", "active", 0);
                
            }
        }
        return 0;
    }

    public int D_ston02_OnAttack(int id, int character, int damage)
    {
        int pose = GetSceneItem(id, "pose");
        if (pose == 1)
        {
            return 0;
        }

        
        CreateEffect(id, "StoneFIL");
        CreateEffect("D_Sston02", "StoneFIL");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(id, "attribute", "damage", 1);
        
        return 0;
    }

    public int D_ston02_OnIdle(int id)
    {
        if (g_bStone02Active == 1)
        {
            int pose = GetSceneItem(id, "pose");
            if (pose == 0)
            {
                return 0;
            }
            int state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bStone02Active = 0;
                
                SetSceneItem(id, "attribute", "active", 0);
                
            }
        }
        return 0;
    }

    public int D_Abutton01_OnAttack(int id, int character, int damage)
    {
        int pose = GetSceneItem("D_ADoor01", "pose");
        if (pose != 0)
        {
            return 0;
        }
        g_iADoor02OpenTime = Misc("gettime");
        
        SetSceneItem("D_ADoor01", "pose", 1, 0);
        SetSceneItem(id, "pose", 1, 0);
        
        return 0;
    }

    public int D_Abutton02_OnAttack(int id, int character, int damage)
    {
        int pose = GetSceneItem("D_ADoor01", "pose");
        if (pose != 0)
        {
            return 0;
        }
        g_iADoor02OpenTime = Misc("gettime");
        
        SetSceneItem("D_ADoor01", "pose", 1, 0);
        SetSceneItem(id, "pose", 1, 0);
        
        return 0;
    }

    public int D_ADoor01_OnIdle(int id)
    {
        int pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }
        int state = GetSceneItem(id, "state");
        if (pose == 1 && state == 3)
        {
            int diff = Misc("gettime") - g_iADoor02OpenTime;
            if (diff > g_iDoorWaitTime)
            {
                Output("Close Door");
                
                SetSceneItem(id, "pose", 2, 0);
                
            }
            return 1;
        }
        if (pose == 2 && state == 3)
        {
            
            SetSceneItem(id, "pose", 0, 0);
            
            return 1;
        }
        return 0;
    }

    public int D_Bbutton01_OnAttack(int id, int character, int damage)
    {
        int pose = GetSceneItem("D_BDoor01", "pose");
        if (pose != 0)
        {
            return 0;
        }
        g_iBDoor01OpenTime = Misc("gettime");
        
        SetSceneItem("D_BDoor01", "pose", 1, 0);
        SetSceneItem(id, "pose", 1, 0);
        
        return 0;
    }

    public int D_Bbutton02_OnAttack(int id, int character, int damage)
    {
        int pose = GetSceneItem("D_BDoor01", "pose");
        if (pose != 0)
        {
            return 0;
        }
        g_iBDoor01OpenTime = Misc("gettime");
        
        SetSceneItem("D_BDoor01", "pose", 1, 0);
        SetSceneItem(id, "pose", 1, 0);
        
        return 0;
    }

    public int D_BDoor01_OnIdle(int id)
    {
        int pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }
        int state = GetSceneItem(id, "state");
        if (pose == 1 && state == 3)
        {
            int diff = Misc("gettime") - g_iBDoor01OpenTime;
            if (diff > g_iDoorWaitTime)
            {
                Output("Close Door");
                
                SetSceneItem(id, "pose", 2, 0);
                
            }
        }
        if (pose == 2 && state == 3)
        {
            
            SetSceneItem("D_BDoor01", "pose", 0, 0);
            
        }
        return 0;
    }
}

// 炽雪城
public class LevelScript_sn04: LevelScriptBase
{
    int RoundTime = 20;
    int PlayerSpawn = 15;
    int PlayerSpawnDir = 250;
    int PlayerWeapon = 16;
    int PlayerWeapon2 = 13;
    int PlayerHP = 2000;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override GameResult OnUnitDead(MeteorUnit deadUnit)
    {
        //必须打破障碍物，由关卡脚本处理关卡胜利条件
        return GameResult.None;
    }
    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int now = 0;
    int survivor = -1;
    int gameover = 0;
    int timer0 = 0;

    public override void OnStart()
    {
        AddNPC("npc04_01");
        AddNPC("npc04_02");
        AddNPC("npc04_03");
        AddNPC("npc04_04");
        AddNPC("npc04_05");
        AddNPC("npc04_06");
        AddNPC("npc04_07");
        base.OnStart();
    }

    public override int OnUpdate()
    {
        int player = GetAnyChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;
        int c2;
        int c3;
        int c4;
        int c5;

        if (trg0 == 0)
        {
            c = GetChar("禁卫侍官长");
            c2 = GetChar("禁卫士兵﹒甲");
            c3 = GetChar("禁卫士兵﹒乙");
            c4 = GetChar("金枪侍卫﹒甲");
            c5 = GetChar("金枪侍卫﹒乙");

            if (c >= 0 && c2 >= 0 && c3 >= 0 && c4 >= 0 && c5 >= 0)
            {
                Perform(c, "say", "其它人去把那用假信物想通关的小子给我捉来！");
                Perform(c, "say", "所有人眼睛给我睁大一点！好好看守这个关卡！");
                Perform(c, "faceto", c3);

                Perform(c2, "say", "是！");
                Perform(c2, "pause", 3);
                Perform(c2, "faceto", c);

                Perform(c3, "say", "是！");
                Perform(c3, "pause", 3);
                Perform(c3, "faceto", c);

                Perform(c4, "pause", 3);
                Perform(c4, "faceto", c);

                Perform(c5, "pause", 3);
                Perform(c5, "faceto", c);

                PlayerPerform("block", 0);
                PlayerPerform("say", "（﹒﹒﹒﹒还是别多想﹒﹒﹒先闯了关再想﹒﹒）");
                PlayerPerform("pause", 1);
                PlayerPerform("say", "（﹒﹒刚那女子﹒﹒﹒难道也有什麽牵联﹒﹒﹒）");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "（﹒﹒﹒﹒﹒﹒）");
                PlayerPerform("say", "（看样子，现在只有直接破坏这道关卡硬闯了！");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "（幸好高老大的计划周详，单闯一个关卡就为我安排了几个方法！）");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "（现在无法单靠信物安全通关了，看样子只好另寻它法！）");
                PlayerPerform("pause", 1);
                PlayerPerform("say", "（﹒﹒﹒﹒﹒﹒）");
                PlayerPerform("pause", 1);
                PlayerPerform("say", "（﹒﹒还是﹒﹒﹒﹒？？）");
                PlayerPerform("pause", 1);
                PlayerPerform("say", "（难道是那个贼样的夜猫子搞的鬼？）");
                PlayerPerform("pause", 1);
                PlayerPerform("say", "（高老大所说时那个古董商所带的信物，应该是这个没错？）");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "（﹒﹒﹒﹒想不到所取得的信物也成了﹒物﹒﹒）");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "（﹒﹒﹒﹒假信物﹒﹒﹒）");
                PlayerPerform("pause", 7);
                PlayerPerform("block", 1);

                trg0 = 1;
            }
        }

        if (trg0 == 1)
        {
            c2 = GetChar("禁卫士兵﹒甲");
            c3 = GetChar("禁卫士兵﹒乙");

            if (c2 >= 0 && GetEnemy(c2) == player)
            {
                ChangeBehavior(c3, "follow", c2);
                Perform(c2, "say", "发现了！这小子在这，快来帮忙！");
                Perform(c2, "faceto", player);
                trg0 = 2;
            }
            if (c3 >= 0 && GetEnemy(c3) == player)
            {
                ChangeBehavior(c2, "follow", c3);
                Perform(c3, "say", "发现了！这小子在这，快来帮忙！");
                Perform(c3, "faceto", player);
                trg0 = 2;
            }
        }
        if (trg0 == 2)
        {
            c2 = GetAnyChar("禁卫士兵﹒甲");
            c3 = GetAnyChar("禁卫士兵﹒乙");
            survivor = -1;
            if (c2 >= 0 && GetHP(c2) <= 0)
            {
                survivor = c3;
            }
            if (c3 >= 0 && GetHP(c3) <= 0)
            {
                survivor = c2;
            }

            if (GetHP(survivor) > 0)
            {
                c = GetChar("禁卫侍官长");
                if (c >= 0)
                {
                    ChangeBehavior(survivor, "follow", c);
                    Perform(survivor, "say", "可恶！你给我等着！");
                    Perform(survivor, "faceto", player);
                    trg0 = 3;
                }
            }
        }

        if (trg0 == 3)
        {
            c = GetChar("禁卫侍官长");
            if (c >= 0 && GetHP(survivor) > 0)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", survivor);
                if (Distance(0, 1) < 150)
                {
                    Perform(survivor, "say", "报告！发现那位闯关的小子了！");
                    Perform(survivor, "faceto", c);

                    ChangeBehavior(c, "follow", player);
                    Perform(c, "say", "走！所有人跟我来！");
                    Perform(c, "pause", 3);
                    Perform(c, "faceto", survivor);

                    c2 = GetChar("金枪侍卫﹒甲");
                    if (c2 >= 0)
                    {
                        ChangeBehavior(c2, "follow", c);
                        Perform(c2, "faceto", c);
                    }
                    c2 = GetChar("金枪侍卫﹒乙");
                    if (c2 >= 0)
                    {
                        ChangeBehavior(c2, "follow", c);
                        Perform(c2, "faceto", c);
                    }
                    trg0 = 4;
                }
            }
        }

        if (trg1 == 0)
        {
            c = GetChar("禁卫侍官长");
            if (c >= 0 && GetEnemy(c) == player)
            {
                Perform(c, "aggress");
                Perform(c, "say", "你这小子，有我在你就别想通过这里！！");
                Perform(c, "faceto", player);
                if (trg0 != 4)
                {
                    c2 = GetChar("金枪侍卫﹒甲");
                    if (c2 >= 0)
                    {
                        ChangeBehavior(c2, "follow", c);
                        Perform(c2, "guard", 5);
                        Perform(c2, "faceto", player);
                    }
                    c2 = GetChar("金枪侍卫﹒乙");
                    if (c2 >= 0)
                    {
                        ChangeBehavior(c2, "follow", c);
                        Perform(c2, "guard", 5);
                        Perform(c2, "faceto", player);
                    }
                    trg0 = 4;
                }
                trg1 = 1;
            }
        }

        if (trg1 == 1)
        {
            c = GetChar("禁卫侍官长");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                c3 = -1;
                c2 = GetChar("金枪侍卫﹒甲");
                if (c2 >= 0)
                {
                    c3 = c2;
                }
                c2 = GetChar("金枪侍卫﹒乙");
                if (c2 >= 0)
                {
                    c3 = c2;
                }
                c2 = GetChar("禁卫士兵﹒甲");
                if (c2 >= 0)
                {
                    c3 = c2;
                }
                c2 = GetChar("禁卫士兵﹒乙");
                if (c2 >= 0)
                {
                    c3 = c2;
                }

                if (GetHP(c3) > 0)
                {
                    Perform(c, "say", "赶快再去叫人来！！");
                    SetTarget(0, "waypoint", 1);
                    ChangeBehavior(c3, "attacktarget", 0);

                    c4 = GetChar("火铳兵﹒甲");
                    if (c4 >= 0)
                    {
                        ChangeBehavior(c4, "kill", player);
                    }
                    c4 = GetChar("火铳兵﹒乙");
                    if (c4 >= 0)
                    {
                        ChangeBehavior(c4, "kill", player);
                    }
                }

                Perform(c, "say", "好样的！！看我怎麽治你！");
                Perform(c, " faceto", player);

                trg1 = 2;
            }
        }


        now = GetGameTime();
        if (gameover == 0 && GetHP(player) <= 0)
        {
            gameover = -1;
            timer0 = now + 2;
        }
        if ((gameover == 1 || gameover == -1) && now > timer0)
        {
            GameOver(gameover);
            gameover = 2;
        }
        return 1;
    }

    int g_iPdoorMaxHP = 3000;
    int g_iPdoorState1HP;
    int g_iPdoorState2HP;
    int g_iPdoorState3HP;

    int g_bAPdoorAlive;
    int g_iAPdoorState;
    int g_iAPdoorShakePose;
    int g_iAPdoorHP;

    int g_bBPdoorAlive;
    int g_iBPdoorState;
    int g_iBPdoorShakePose;
    int g_iBPdoorHP;

    //string efname = "GunHit";

    public override void Scene_OnLoad()
    {
        SetScene("snow", 1);//雪粒子
        SetScene("snowdensity", 2000);//粒子密度
        SetScene("winddir", 50, 0, 0);//风方向
        SetScene("snowspeed", 20, 100);//雪速度
        SetScene("snowsize", 5, 5);//粒子尺寸

        //string name;
        //int i;

        g_iPdoorMaxHP = g_iLevel04GiMaMaxHP;
        g_iPdoorState1HP = (g_iPdoorMaxHP * 3) / 4;
        g_iPdoorState2HP = (g_iPdoorMaxHP * 2) / 4;
        g_iPdoorState3HP = (g_iPdoorMaxHP * 1) / 4;

        SetSceneItem("D_APdoor", "name", "machine");
        SetSceneItem("D_APdoor", "attribute", "damage", 0);
        SetSceneItem("D_APdoor", "attribute", "damagevalue", 30);
        SetSceneItem("D_APdoor", "attribute", "collision", 0);
        SetSceneItem("D_APd02Box", "name", "machine", 1);
        SetSceneItem("D_APd02Box", "attribute", "collision", 1);

        SetSceneItem("D_BPdoor", "name", "machine");
        SetSceneItem("D_BPdoor", "attribute", "damage", 0);
        SetSceneItem("D_BPdoor", "attribute", "damagevalue", 30);
        SetSceneItem("D_BPdoor", "attribute", "collision", 0);
        SetSceneItem("D_BPd02Box", "name", "machine", 1);
        SetSceneItem("D_BPd02Box", "attribute", "collision", 1);
    }


    public override void Scene_OnInit()
    {
        //int i = 0;

        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);

        g_iAPdoorHP = g_iPdoorMaxHP;
        g_bAPdoorAlive = 1;
        g_iAPdoorState = 1;
        g_iAPdoorShakePose = 1;

        g_iBPdoorHP = g_iPdoorMaxHP;
        g_bBPdoorAlive = 1;
        g_iBPdoorState = 1;
        g_iBPdoorShakePose = 1;

        SetSceneItem("D_APdoor", "pose", 0, 0);
        SetSceneItem("D_BPdoor", "pose", 0, 0);
    }

    public int D_APdoor_OnAttack(int id, int character, int damage)
    {
        int t = GetTeam(character);
        Output("Hit:", character, t);
        if (t == 1)
        {
            return 0;
        }

        int state;
        g_iAPdoorHP = g_iAPdoorHP - damage;
        Output("A", g_iAPdoorHP, g_iAPdoorState);

        if (g_iAPdoorState == 1 && g_iAPdoorHP < g_iPdoorState1HP)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            g_iAPdoorShakePose = 3;
            
            SetSceneItem(id, "pose", 2, 0);
            
        }

        if (g_iAPdoorState == 2 && g_iAPdoorHP < g_iPdoorState2HP)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            g_iAPdoorShakePose = 5;
            
            SetSceneItem(id, "pose", 4, 0);
            
        }

        if (g_iAPdoorState == 3 && g_iAPdoorHP < g_iPdoorState3HP)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            g_iAPdoorShakePose = 7;
            
            SetSceneItem(id, "pose", 6, 0);
            
        }

        if (g_iAPdoorState == 4 && g_iAPdoorHP < 0)
        {
            g_iAPdoorState = g_iAPdoorState + 1;
            
            CreateEffect(id, "GiMaBRK");
            SetSceneItem(id, "attribute", "interactive", 0);
            SetSceneItem(id, "attribute", "collision", 0);
            SetSceneItem(id, "pose", 8, 0);
            
        }

        state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", g_iAPdoorShakePose, 0);
            
        }
        return 1;
    }

    public void D_APdoor_OnIdle(int id)
    {
        if (g_iAPdoorState == 5 && g_bAPdoorAlive == 1)
        {
            int state;
            state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bAPdoorAlive = 0;
                GameCallBack("end", 2);
            }
        }
    }

    public int D_BPdoor_OnAttack(int id, int character, int damage)
    {
        int t = GetTeam(character);
        Output("Hit:", character, t);
        if (t == 2)
        {
            return 0;
        }
        if (g_iBPdoorState == 5 || g_iBPdoorHP < 0)
            return 0;
        int state;

        g_iBPdoorHP = g_iBPdoorHP - damage;
        Output("B", g_iBPdoorHP, g_iBPdoorState);

        if (g_iBPdoorState == 1 && g_iBPdoorHP < g_iPdoorState1HP)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            g_iBPdoorShakePose = 3;
            
            SetSceneItem(id, "pose", 2, 0);
            
        }

        if (g_iBPdoorState == 2 && g_iBPdoorHP < g_iPdoorState2HP)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            g_iBPdoorShakePose = 5;
            Output("Change State 3");
            
            SetSceneItem(id, "pose", 4, 0);
            
        }

        if (g_iBPdoorState == 3 && g_iBPdoorHP < g_iPdoorState3HP)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            g_iBPdoorShakePose = 7;
            Output("Change State 4");
            
            SetSceneItem(id, "pose", 6, 0);
            
        }

        if (g_iBPdoorState == 4 && g_iBPdoorHP < 0)
        {
            g_iBPdoorState = g_iBPdoorState + 1;
            
            CreateEffect(id, "GiMaBRK");
            SetSceneItem(id, "attribute", "interactive", 0);
            SetSceneItem(id, "attribute", "collision", 0);
            SetSceneItem(id, "pose", 8, 0);
            
        }

        

        state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", g_iBPdoorShakePose, 0);
            
        }
        return 1;
    }

    public void D_BPdoor_OnIdle(int id)
    {
        if (g_iBPdoorState == 5 && g_bBPdoorAlive == 1)
        {
            int state;
            state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bBPdoorAlive = 0;
                GameCallBack("end", 1);
            }
        }
    }
}

//皇天城
public class LevelScript_sn05: LevelScriptBase
{
    public override void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);

        SetSceneItem("D_Door01", "name", "machine");
        SetSceneItem("D_Door01", "attribute", "collision", 1);

        SetSceneItem("D_Door02", "name", "machine");
        SetSceneItem("D_Door02", "attribute", "collision", 1);

        SetSceneItem("D_Door03", "name", "machine");
        SetSceneItem("D_Door03", "attribute", "collision", 1);
    }

    int RoundTime = 30;
    int PlayerSpawn = 83;
    int PlayerSpawnDir = 135;
    int PlayerWeapon = 24;
    int PlayerWeapon2 = 21;
    int PlayerHP = 2500;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;
    int trg5 = 0;
    int trg6 = 0;
    int trg7 = 0;
    int trg8 = 0;
    int trg9 = 0;
    int timer0 = 0;
    int timer1 = 0;
    int timer2 = 0;

    int hp0 = 0;
    int hp1 = 0;
    int hp2 = 0;
    int hp3 = 0;
    int hp4 = 0;

    public override void OnStart()
    {
        AddNPC("npc05_01");
        AddNPC("npc05_02");

        AddNPC("npc05_05");
        AddNPC("npc05_06");
        AddNPC("npc05_07");
        AddNPC("npc05_08");
        AddNPC("npc05_09");
        base.OnStart();
    }

    public int CallFriend(int c, int c2, int p)
    {
        if (c >= 0 && GetEnemy(c) == p)
        {
            ChangeBehavior(c, "follow", p);
            Perform(c, "guard", 3);
            Perform(c, "say", "有人闯入，快来人呀！");
            Perform(c, "say", "你是谁！竟敢乱闯！！");
            Perform(c, "faceto", p);

            if (c2 >= 0)
            {
                ChangeBehavior(c2, "follow", c);
            }
            return 1;
        }
        return 0;
    }

    public int CallFriend2(int c, int c2, int p, int t)
    {
        if (c >= 0 && GetEnemy(c) == p)
        {
            ChangeBehavior(c, "follow", p);
            if (t == 1)
            {
                Perform(c, "say", "哈！老子看到你了！");
            }
            else
            {
                Perform(c, "say", "唷！好戏上场啦！！");
            }
            Perform(c, "faceto", p);

            if (c2 >= 0)
            {
                ChangeBehavior(c2, "follow", c);
            }
            return 1;
        }
        return 0;
    }

    public int BackGuard(int c, int p, int say)
    {
        if (c >= 0)
        {
            Perform(c, "guard", 100);
            Perform(c, "faceto", p);
            ChangeBehavior(c, "dodge", p);

            if (say == 1)
            {
                Perform(c, "say", "施主保重！");
            }
            if (say == 2)
            {
                Perform(c, "say", "﹒﹒﹒﹒﹒");
            }

            Perform(c, "pause", 4);
            Perform(c, "faceto", p);

            return 1;
        }
        return 0;
    }

    public int PauseAll(int t, int p)
    {
        int c;
        c = GetChar("金枪侍卫");
        if (c >= 0)
        {
            Perform(c, "guard", t);
            Perform(c, "faceto", p);
        }
        c = GetChar("大刀侍卫");
        if (c >= 0)
        {
            Perform(c, "guard", t);
            Perform(c, "faceto", p);
        }
        c = GetChar("野和尚﹒甲");
        if (c >= 0)
        {
            Perform(c, "guard", t);
            Perform(c, "faceto", p);
        }
        c = GetChar("野和尚﹒乙");
        if (c >= 0)
        {
            Perform(c, "guard", t);
            Perform(c, "faceto", p);
        }
        c = GetChar("无名杀手");
        if (c >= 0)
        {
            Perform(c, "guard", t);
            Perform(c, "faceto", p);
        }
        c = GetChar("蒙面人﹒甲");
        if (c >= 0)
        {
            Perform(c, "guard", t);
            Perform(c, "faceto", p);
        }
        c = GetChar("蒙面人﹒乙");
        if (c >= 0)
        {
            Perform(c, "guard", t);
            Perform(c, "faceto", p);
        }

        return 1;
    }

    public override int OnUpdate()
    {
        int player = GetChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;
        int c2;
        int c3;
        int c4;
        int c5;

        if (trg0 == 0)
        {
            PlayerPerform("block", 0);
            PlayerPerform("crouch", 0);
            PlayerPerform("say", "（不该多想的﹒﹒﹒赶紧完成任务，带回范璇身上的代表信物，交差了事好！）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（﹒﹒﹒我怎能会为了这些事情乱了心）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（﹒﹒﹒这一切究竟是﹒﹒﹒﹒﹒）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（而那姓萧所说的话到底是什麽意思？？）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（﹒﹒那奇女子﹒﹒﹒﹒）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（﹒﹒﹒﹒那个蒙面人又是怎麽一回儿事？？）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（难道是高老大担心我﹒﹒﹒还是﹒﹒﹒﹒）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（﹒﹒怎麽感觉一路上都有人在监视着我﹒﹒﹒﹒）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（但﹒﹒难道是我的行踪己经曝露了？）");
            PlayerPerform("pause", 2);
            PlayerPerform("say", "（﹒﹒﹒终於来到这儿了﹒﹒﹒）");
            PlayerPerform("crouch", 1);
            PlayerPerform("block", 1);
            trg0 = 1;
        }

        if (trg1 == 0)
        {
            c = GetChar("金枪侍卫");
            c2 = GetChar("大刀侍卫");

            if (CallFriend(c, c2, player) == 1)
            {
                trg1 = 1;
            }
            if (trg1 == 0 && CallFriend(c2, c, player) == 1)
            {
                trg1 = 1;
            }
        }

        if (trg2 == 0)
        {
            c = GetChar("野和尚﹒甲");
            c2 = GetChar("野和尚﹒乙");
            if (CallFriend2(c, c2, player, 1) == 1)
            {
                trg2 = 1;
            }
            if (trg2 == 0 && CallFriend2(c2, c, player, 0) == 1)
            {
                trg2 = 1;
            }
        }
        if (trg2 == 1)
        {
            c = GetChar("野和尚﹒甲");
            c2 = GetChar("野和尚﹒乙");
            if (c >= 0 && c2 >= 0 && GetEnemy(c) == player && GetEnemy(c2) == player)
            {
                Perform(c, "say", "我们两个就陪你玩玩！！");
                Perform(c, "faceto", player);
                Perform(c2, "say", "我们两个就陪你玩玩！！");
                Perform(c2, "faceto", player);
                trg2 = 2;
            }
        }
        if (trg2 == 2)
        {
            c = GetChar("野和尚﹒甲");
            c2 = GetChar("野和尚﹒乙");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "aggress");
                Perform(c, "say", "施主还是多去练个几年再来吧！");
                Perform(c, "faceto", player);
                trg2 = 3;
            }
            if (c2 >= 0 && GetHP(c2) < GetMaxHP(c2) / 2)
            {
                Perform(c2, "aggress");
                Perform(c2, "say", "施主要是再不滚，休怪老纳不客气了！");
                Perform(c2, "faceto", player);
                trg2 = 3;
            }
        }

        if (trg3 == 0)
        {
            c = GetChar("无名杀手");
            SetTarget(0, "char", player);
            SetTarget(1, "char", c);
            if (c >= 0 && GetEnemy(c) == player && Distance(0, 1) < 100)
            {
                ChangeBehavior(c, "kill", player);

                c2 = GetChar("金枪侍卫");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", player);
                }
                c2 = GetChar("大刀侍卫");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", player);
                }

                c2 = GetAnyChar("野和尚﹒甲");
                c3 = GetAnyChar("野和尚﹒乙");
                if (c2 >= 0 && GetHP(c2) <= 0 && c3 >= 0 && GetHP(c3) <= 0)
                {
                    Perform(c, "aggress");
                    Perform(c, "say", "听说你的功夫了得，我就是特地来试试你的身手！放马过来！");
                    Perform(c, "say", "哼！等了许久终於来了！");
                    Perform(c, "faceto", player);

                    PlayerPerform("say", "﹒﹒﹒﹒﹒﹒");
                    PlayerPerform("pause", 5);
                }
                else
                {
                    Perform(c, "say", "退下，他就交给我吧！");
                    c2 = GetChar("野和尚﹒甲");
                    if (BackGuard(c2, player, 1) == 1)
                    {
                        hp1 = GetHP(c2);
                    }
                    c2 = GetChar("野和尚﹒乙");
                    if (BackGuard(c2, player, 1) == 1)
                    {
                        hp2 = GetHP(c2);
                    }
                    c2 = GetChar("蒙面人﹒甲");
                    if (BackGuard(c2, player, 2) == 1)
                    {
                        hp3 = GetHP(c2);
                    }
                    c2 = GetChar("蒙面人﹒乙");
                    if (BackGuard(c2, player, 2) == 1)
                    {
                        hp4 = GetHP(c2);
                    }
                    c2 = GetChar("金枪侍卫");
                    BackGuard(c2, player, 0);
                    c2 = GetChar("大刀侍卫");
                    BackGuard(c2, player, 0);
                }
                trg3 = 1;
            }
        }

        if (trg3 == 1 || trg3 == 2)
        {
            c5 = -1;
            c = GetChar("野和尚﹒甲");
            c2 = GetChar("野和尚﹒乙");
            c3 = GetChar("蒙面人﹒甲");
            c4 = GetChar("蒙面人﹒乙");

            if (hp1 > 0)
            {
                if (c >= 0 && GetHP(c) < hp1)
                {
                    Perform(c, "say", "你自找的！！");
                    Perform(c, "faceto", player);
                    c5 = c;
                }
            }
            if (hp2 > 0)
            {
                if (c2 >= 0 && GetHP(c2) < hp2)
                {
                    Perform(c2, "say", "找死");
                    Perform(c2, "faceto", player);
                    c5 = c2;
                }
            }
            if (hp3 > 0)
            {
                if (c3 >= 0 && GetHP(c3) < hp3)
                {
                    Perform(c3, "say", "﹒﹒﹒﹒﹒");
                    Perform(c3, "faceto", player);
                    c5 = c3;
                }
            }
            if (hp4 > 0)
            {
                if (c4 >= 0 && GetHP(c4) < hp4)
                {
                    Perform(c, "say", "﹒﹒﹒﹒﹒");
                    Perform(c, "faceto", player);
                    c5 = c4;
                }
            }
            if (c5 >= 0)
            {
                ChangeBehavior(c, "kill", player);
                ChangeBehavior(c2, "kill", player);
                ChangeBehavior(c3, "kill", player);
                ChangeBehavior(c4, "kill", player);
                trg3 = 3;
            }
        }

        if (trg3 == 1)
        {
            c = GetChar("无名杀手");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) * 2 / 3)
            {
                Perform(c, "guard", 4);
                Perform(c, "say", "哼！果然有两下子！看来我得认真了！");
                Perform(c, "faceto", player);

                trg3 = 2;
            }
        }
        if (trg3 == 2)
        {
            c = GetChar("无名杀手");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                c2 = GetChar("野和尚﹒甲");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", c);
                    Perform(c2, "say", "上！");
                    Perform(c2, "pause", 6);
                    Perform(c2, "say", "撑的住吗？？");
                    Perform(c2, "faceto", c);
                    Perform(c2, "pause", 8);
                }
                c2 = GetChar("野和尚﹒乙");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", c);
                    Perform(c2, "say", "我来助你一臂之力！");
                    Perform(c2, "pause", 6);
                    Perform(c2, "say", "撑的住吗？？");
                    Perform(c2, "faceto", c);
                    Perform(c2, "pause", 8);
                }

                trg3 = 3;
            }
        }
        if (trg3 == 3)
        {
            c = GetChar("无名杀手");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 3)
            {
                Perform(c, "say", "想不到武林中竟然有如此的高手﹒﹒");
                Perform(c, "faceto", player);
                trg3 = 4;
            }
        }
        if (trg3 == 4)
        {
            c = GetAnyChar("无名杀手");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "果然厉害﹒﹒﹒能跟你交手，我死也甘心了﹒﹒﹒");

                c2 = GetChar("野和尚﹒甲");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "kill", player);
                }
                c2 = GetChar("野和尚﹒乙");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "kill", player);
                }

                trg3 = 5;
            }
        }

        if (trg4 == 0 && trg6 == 0)
        {
            c = GetChar("屠城");
            SetTarget(0, "char", player);
            SetTarget(1, "char", c);
            if (c >= 0 && GetEnemy(c) == player && Distance(0, 1) < 80)
            {
                c2 = GetChar("范璇");
                if (c2 >= 0)
                {
                    ChangeBehavior(c, "follow", c2);
                    SetTarget(0, "char", c2);
                    ChangeBehavior(c, "attacktarget", 0);
                }
                Perform(c, "say", "来人呀！！给我拿下这个刺客！！");
                Perform(c, "pause", 5);
                Perform(c, "say", "有我在你休通过这里！");
                Perform(c, "say", "你﹒﹒﹒竟敢想要行刺'范主子'！");
                Perform(c, "faceto", player);

                PlayerPerform("block", 0);
                PlayerPerform("say", "﹒﹒﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 3);
                PlayerPerform("block", 1);

                c3 = GetChar("无名杀手");
                if (c3 >= 0)
                {
                    ChangeBehavior(c3, "follow", player);
                }
                c3 = GetChar("蒙面人﹒甲");
                if (c3 >= 0)
                {
                    ChangeBehavior(c3, "follow", player);
                }
                c3 = GetChar("蒙面人﹒乙");
                if (c3 >= 0)
                {
                    ChangeBehavior(c3, "follow", player);
                }
                c3 = GetChar("金枪侍卫");
                if (c3 >= 0)
                {
                    ChangeBehavior(c2, "kill", player);
                }
                c3 = GetChar("大刀侍卫");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "kill", player);
                }

                trg4 = 1;
            }
        }
        if (trg4 == 1)
        {
            c = GetChar("屠城");
            c2 = GetChar("范璇");
            if (c >= 0 && c2 >= 0)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", c2);
                if (Distance(0, 1) < 100)
                {
                    ChangeBehavior(c, "patrol", 56, 66, 65, 61, 67);
                    Perform(c, "say", "哼！！给我杀了他！");
                    Perform(c, "pause", 5);
                    Perform(c, "faceto", player);
                    Perform(c, "say", "是！！");
                    Perform(c, "pause", 6);
                    Perform(c, "say", "报告主子！有刺客想要行刺您，请您先行离开！！");
                    Perform(c, "faceto", c2);

                    Perform(c2, "say", "留下活口，等我回来我要问个清楚，到底是谁指使的！");
                    Perform(c2, "say", "什麽！！！好大的胆子！竟然有人敢行刺我！");
                    Perform(c2, "pause", 3);
                    Perform(c2, "faceto", c);
                    trg4 = 2;
                    timer0 = GetGameTime() + 10;
                }
            }
        }
        if (trg4 == 2 && GetGameTime() > timer0)
        {
            c = GetAnyChar("范璇");
            if (c >= 0)
            {
                RemoveNPC(c);
            }
            trg4 = 3;
        }

        if (trg5 == 0)
        {
            c = GetChar("屠城");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "say", "来人呀！通通给我围住他，把他剁成肉酱喂狗！！");
                Perform(c, "pause", 3);
                Perform(c, "say", "今天你别想活的走着出去了！");
                Perform(c, "say", "好样的﹒﹒﹒竟然能伤的了我！");
                Perform(c, "faceto", player);

                trg5 = 1;
            }
        }

        if (trg5 == 1)
        {
            c = GetAnyChar("屠城");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "呜﹒﹒﹒想不到﹒﹒﹒主子！我对不起你了﹒﹒﹒﹒");
                trg5 = 2;
            }
        }

        if (trg6 == 0 && trg4 == 0)
        {
            c = GetChar("范璇");
            SetTarget(0, "char", player);
            SetTarget(1, "char", c);
            if (c >= 0 && GetEnemy(c) == player && Distance(0, 1) < 100)
            {
                c2 = GetChar("屠城");
                if (c2 >= 0)
                {
                    ChangeBehavior(c, "follow", c2);
                    SetTarget(0, "char", c2);
                    ChangeBehavior(c, "attacktarget", 0);
                }
                Perform(c, "say", "想行刺我！没那麽容易！护驾！有刺客！！");
                Perform(c, "pause", 6);
                Perform(c, "say", "你﹒﹒﹒你是谁！！");
                Perform(c, "faceto", player);

                PlayerPerform("block", 0);
                PlayerPerform("say", "想逃！！");
                PlayerPerform("pause", 6);
                PlayerPerform("say", "﹒﹒﹒我的名字不重要，重要的是，我是来取你狗命的！");
                PlayerPerform("pause", 3);
                PlayerPerform("block", 1);

                PauseAll(10, player);

                trg6 = 1;
            }
        }

        if (trg6 == 1)
        {
            c = GetChar("范璇");
            c2 = GetChar("屠城");
            if (c >= 0 && c2 >= 0)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", c2);
                if (Distance(0, 1) < 120)
                {
                    Perform(c2, "say", "杀了他！！");
                    Perform(c2, "aggress");
                    Perform(c2, "faceto", player);
                    Perform(c2, "say", "是！！");
                    Perform(c2, "pause", 8);
                    Perform(c2, "say", "主子！请您先行离开，这里由我来就好！");
                    Perform(c2, "say", "刺客！！所有人快把刺客拿下！！");
                    Perform(c2, "faceto", c);

                    SetTarget(0, "waypoint", 73);
                    ChangeBehavior(c, "attacktarget", 0);
                    Perform(c, "say", "让他死之前，先给我问出主使者是谁！");
                    Perform(c, "say", "好！我先去避一避，这里就交给你了！");
                    Perform(c, "pause", 4);
                    Perform(c, "faceto", c2);

                    PlayerPerform("say", "﹒﹒﹒﹒﹒﹒﹒");
                    PlayerPerform("pause", 3);

                    c3 = GetChar("无名杀手");
                    if (c3 >= 0)
                    {
                        ChangeBehavior(c3, "follow", player);
                    }
                    c3 = GetChar("蒙面人﹒甲");
                    if (c3 >= 0)
                    {
                        ChangeBehavior(c3, "follow", player);
                    }
                    c3 = GetChar("蒙面人﹒乙");
                    if (c3 >= 0)
                    {
                        ChangeBehavior(c3, "follow", player);
                    }

                    trg6 = 2;
                    timer0 = GetGameTime() + 6;
                }
            }
        }
        if (trg6 == 2 && GetGameTime() > timer0)
        {
            c = GetAnyChar("范璇");
            if (c >= 0)
            {
                RemoveNPC(c);
            }
            trg6 = 3;
        }

        if (trg7 == 0)
        {
            c = GetAnyChar("金枪侍卫");
            c2 = GetAnyChar("大刀侍卫");
            c3 = GetAnyChar("野和尚﹒甲");
            c4 = GetAnyChar("野和尚﹒乙");
            if (c >= 0 && GetHP(c) <= 0 && c2 >= 0 && GetHP(c2) <= 0 && c3 >= 0 && GetHP(c3) <= 0 && c4 >= 0 && GetHP(c4) <= 0)
            {
                RemoveNPC(c);
                RemoveNPC(c2);
                RemoveNPC(c3);
                RemoveNPC(c4);
                trg7 = 1;
                timer1 = GetGameTime() + 5;
            }
        }
        if (trg7 == 1)
        {
            if (GetGameTime() > timer1)
            {
                AddNPC("npc05_03");
                AddNPC("npc05_04");
                trg7 = 2;
            }
        }
        return 1;
    }
}

//四方阵，无脚本
public class LevelScript_sn06: LevelScriptBase
{
    int RoundTime = 15;
    int PlayerSpawn = 12;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }

    public override void OnStart()
    {
        AddNPC("npc02_01");
        base.OnStart();
    }

    int tag1 = 0;
    public override int OnUpdate()
    {
        if (tag1 == 0)
        {
            int c1 = GetChar("皇陵使");
            ChangeBehavior(c1, "patrol", 1, 2, 3);
            tag1 = 1;
        }
        return 0;
    }
}
//死之阵
public class LevelScript_sn07: LevelScriptBase
{
    public override void Scene_OnLoad()
    {
        SetSceneItem("D_Aknife01", "name", "machine");
        SetSceneItem("D_Aknife01", "attribute", "damage", 1);
        SetSceneItem("D_Aknife01", "attribute", "damagevalue", g_iLevel07KnifeDamage);

        SetSceneItem("D_Aknife02", "name", "machine");
        SetSceneItem("D_Aknife02", "attribute", "damage", 1);
        SetSceneItem("D_Aknife02", "attribute", "damagevalue", g_iLevel07KnifeDamage);

        SetSceneItem("D_Bknife03", "name", "machine");
        SetSceneItem("D_Bknife03", "attribute", "damage", 1);
        SetSceneItem("D_Bknife03", "attribute", "damagevalue", g_iLevel07KnifeDamage);

        SetSceneItem("D_Bknife04", "name", "machine");
        SetSceneItem("D_Bknife04", "attribute", "damage", 1);
        SetSceneItem("D_Bknife04", "attribute", "damagevalue", g_iLevel07KnifeDamage);

        SetSceneItem("D_floor01", "name", "machine");
        SetSceneItem("D_floor02", "name", "machine");
        SetSceneItem("D_floor03", "name", "machine");
        SetSceneItem("D_floor04", "name", "machine");
        SetSceneItem("D_floor05", "name", "machine");
        SetSceneItem("D_floor06", "name", "machine");

        SetSceneItem("D_floor01", "attribute", "collision", 1);
        SetSceneItem("D_floor03", "attribute", "collision", 1);
        SetSceneItem("D_floor04", "attribute", "collision", 1);
        SetSceneItem("D_floor05", "attribute", "collision", 1);
        SetSceneItem("D_floor06", "attribute", "collision", 1);

        SetSceneItem("D_sn07t03", "name", "machine");
        SetSceneItem("D_sn07t04", "name", "machine");
        SetSceneItem("D_sn07t05", "name", "machine");
        SetSceneItem("D_sn07t06", "name", "machine");

        SetSceneItem("D_sn07t03", "attribute", "damage", 1);
        SetSceneItem("D_sn07t03", "attribute", "damagevalue", g_iLevel07PinDamage);
        SetSceneItem("D_sn07t04", "attribute", "damage", 1);
        SetSceneItem("D_sn07t04", "attribute", "damagevalue", g_iLevel07PinDamage);
        SetSceneItem("D_sn07t05", "attribute", "damage", 1);
        SetSceneItem("D_sn07t05", "attribute", "damagevalue", g_iLevel07PinDamage);
        SetSceneItem("D_sn07t06", "attribute", "damage", 1);
        SetSceneItem("D_sn07t06", "attribute", "damagevalue", g_iLevel07PinDamage);

        SetSceneItem("D_gd02eye01", "name", "machine");
        SetSceneItem("D_gd03eye02", "name", "machine");
        SetSceneItem("D_gd04eye03", "name", "machine");
        SetSceneItem("D_gd05eye04", "name", "machine");

        SetSceneItem("D_sn07gd02", "name", "machine");
        SetSceneItem("D_sn07gd03", "name", "machine");
        SetSceneItem("D_sn07gd04", "name", "machine");
        SetSceneItem("D_sn07gd05", "name", "machine");
    }

    public override void Scene_OnInit()
    {
        SetSceneItem("D_Aknife01", "pose", 0, 0);
        SetSceneItem("D_Aknife02", "pose", 0, 0);
        SetSceneItem("D_Bknife03", "pose", 0, 0);
        SetSceneItem("D_Bknife04", "pose", 0, 0);

        SetSceneItem("D_floor01", "pose", 0, 0);
        SetSceneItem("D_floor03", "pose", 0, 0);
        SetSceneItem("D_floor04", "pose", 0, 0);
        SetSceneItem("D_floor05", "pose", 0, 0);
        SetSceneItem("D_floor06", "pose", 0, 0);

        SetSceneItem("D_sn07t03", "pose", 0, 0);
        SetSceneItem("D_sn07t04", "pose", 0, 0);
        SetSceneItem("D_sn07t05", "pose", 0, 0);
        SetSceneItem("D_sn07t06", "pose", 0, 0);

        SetSceneItem("D_gd02eye01", "pose", 0, 0);
        SetSceneItem("D_gd03eye02", "pose", 0, 0);
        SetSceneItem("D_gd04eye03", "pose", 0, 0);
        SetSceneItem("D_gd05eye04", "pose", 0, 0);

        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
    }

    public void RandomActivate()
    {
        int r = rand(0, 7);
        Output("Activate", r);
        if (r == 0)
        {
            if (GetSceneItem("D_Aknife01", "state") == 3)
            {
                CreateEffect("D_Aknife01", "DeathKN2");
                CreateEffect("D_SAknife01", "DeathKN2");
                SetSceneItem("D_Aknife01", "pose", 1, 0);
                CreateEffect("D_Aknife02", "DeathKN2");
                CreateEffect("D_SAknife02", "DeathKN2");
                SetSceneItem("D_Aknife02", "pose", 1, 0);
            }
        }

        if (r == 1)
        {
            if (GetSceneItem("D_Bknife03", "state") == 3)
            {
                CreateEffect("D_Bknife03", "DeathKN2");
                CreateEffect("D_SBknife03", "DeathKN2");
                SetSceneItem("D_Bknife03", "pose", 1, 0);
                CreateEffect("D_Bknife04", "DeathKN2");
                CreateEffect("D_SBknife04", "DeathKN2");
                SetSceneItem("D_Bknife04", "pose", 1, 0);
            }
        }
        if (r == 2)
        {
            if (GetSceneItem("D_floor01", "state") == 3)
            {
                CreateEffect("D_floor01", "FloorFI0");
                SetSceneItem("D_floor01", "pose", 1, 0);
            }
        }
        if (r == 3)
        {
            if (GetSceneItem("D_floor03", "state") == 3)
            {
                CreateEffect("D_floor03", "FloorFI0");
                SetSceneItem("D_floor03", "pose", 1, 0);
            }
        }
        if (r == 4)
        {
            if (GetSceneItem("D_floor04", "state") == 3)
            {
                CreateEffect("D_floor04", "FloorFI0");
                SetSceneItem("D_floor04", "pose", 1, 0);
            }
        }
        if (r == 5)
        {
            if (GetSceneItem("D_floor05", "state") == 3)
            {
                CreateEffect("D_floor05", "FloorFI0");
                SetSceneItem("D_floor05", "pose", 1, 0);
            }
        }
        if (r == 6)
        {
            if (GetSceneItem("D_floor06", "state") == 3)
            {
                CreateEffect("D_floor06", "FloorFI0");
                SetSceneItem("D_floor06", "pose", 1, 0);
            }
        }
    }

    public void D_sn07gd02_OnAttack(int id, int characterid, int damage)
    {
        Output("02");
        if (GetSceneItem("D_gd02eye01", "state") == 3)
        {
            
            CreateEffect("D_sn07t06", "FStinger");
            SetSceneItem("D_sn07t06", "pose", 1, 0);
            SetSceneItem("D_gd02eye01", "pose", 1, 0);
            RandomActivate();
            
        }
    }

    public void D_sn07gd03_OnAttack(int id, int characterid, int damage)
    {
        Output("03");
        if (GetSceneItem("D_gd03eye02", "state") == 3)
        {
            
            CreateEffect("D_sn07t03", "FStinger");
            SetSceneItem("D_sn07t03", "pose", 1, 0);
            SetSceneItem("D_gd03eye02", "pose", 1, 0);
            RandomActivate();
            
        }
    }

    public void D_sn07gd04_OnAttack(int id, int characterid, int damage)
    {
        Output("04");
        if (GetSceneItem("D_gd04eye03", "state") == 3)
        {
            
            CreateEffect("D_sn07t04", "FStinger");
            SetSceneItem("D_sn07t04", "pose", 1, 0);
            SetSceneItem("D_gd04eye03", "pose", 1, 0);
            RandomActivate();
            
        }
    }

    public void D_sn07gd05_OnAttack(int id, int characterid, int damage)
    {
        Output("05");
        if (GetSceneItem("D_gd05eye04", "state") == 3)
        {
            
            CreateEffect("D_sn07t05", "FStinger");
            SetSceneItem("D_sn07t05", "pose", 1, 0);
            SetSceneItem("D_gd05eye04", "pose", 1, 0);
            RandomActivate();
            
        }
    }

    int RoundTime = 15;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;

    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
}

//毒牙阵
public class LevelScript_sn08: LevelScriptBase
{
    int RoundTime = 15;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;

    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }

    public override void OnStart()
    {
        base.OnStart();
    }

    public override int OnUpdate()
    {
        return 0;
    }
    public override void Scene_OnLoad()
    {
        SetSceneItem("D_sn08B01", "name", "machine");
        SetSceneItem("D_sn08B02", "name", "machine");
        SetSceneItem("D_sn08B02", "attribute", "damage", 1);
        SetSceneItem("D_sn08B02", "attribute", "damagevalue", g_iLevel08StickDamage);
        SetSceneItem("D_sn08B03", "name", "machine");
        SetSceneItem("D_sn08B03", "attribute", "interactive", 0);
    }
    public override void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);

        SetSceneItem("D_sn08B01", "attribute", "interactive", 0);
        SetSceneItem("D_sn08B03", "attribute", "interactive", 0);

        SetSceneItem("D_sn08B01", "pose", 1, 1);
        SetSceneItem("D_sn08B02", "pose", 1, 1);
        SetSceneItem("D_sn08B03", "pose", 1, 1);
    }
}
//决死阵
public class LevelScript_sn09 : LevelScriptBase
{
    int RoundTime = 5;
    int PlayerSpawn = 20;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 1;
    int PlayerHP = 3000;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override GameResult OnUnitDead(MeteorUnit deadUnit)
    {
        //任意死都算失败
        return GameResult.Fail;
    }
    int trg0 = 0;
    int trg1 = 0;
    int timer0 = 0;
    int timer1 = 0;
    int gameover = 0;

    public override void OnStart()
    {
        AddNPC("npc09_01");
        AddNPC("npc09_02");
        base.OnStart();
    }

    public override int OnUpdate()
    {
        int player = GetAnyChar("player");
        if (player < 0)
        {
            return 0;
        }
        if (gameover == 2)
        {
            return 0;
        }

        int c;
        int c2;
        int now = GetGameTime();


        if (trg0 == 0)
        {
            c = GetChar("冷燕");
            c2 = GetChar("屠城");

            if (c >= 0 && c2 >= 0)
            {
                SetTarget(0, "waypoint", 15);
                ChangeBehavior(c2, "attacktarget", 0);
                Perform(c2, "pause", 2);
                Perform(c2, "say", "看狗咬狗真是快活呀！！");
                Perform(c2, "pause", 2);
                Perform(c2, "say", "现在解药只剩一瓶，就让你们两个去争吧！");
                Perform(c2, "pause", 2);
                Perform(c2, "say", "杀手也会被暗算，很不是滋味是吗？");
                Perform(c2, "pause", 4);
                Perform(c2, "say", "你的目的就是要杀范璇﹒﹒﹒﹒如何？？他真的是死了吧！");
                Perform(c2, "pause", 2);
                Perform(c2, "say", "呵﹒﹒料想不到我会装死吧！");
                Perform(c2, "pause", 5);
                Perform(c2, "say", "哇哈哈哈！");
                Perform(c2, "faceto", player);

                Perform(c, "attack");
                Perform(c, "faceto", player);
                Perform(c, "guard", 4);
                Perform(c, "faceto", player);
                Perform(c, "say", "呵﹒﹒这解药我是要定了！！");
                Perform(c, "pause", 23);
                Perform(c, "faceto", player);
                Perform(c, "use", 14);

                PlayerPerform("block", 0);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 7);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 9);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 6);
                PlayerPerform("say", "你﹒﹒你不是已经被我﹒﹒﹒！？");
                PlayerPerform("pause", 3);
                PlayerPerform("use", 14);
                PlayerPerform("block", 1);

                trg0 = 1;
                timer0 = now + 19;
            }
        }
        if (trg0 == 1 && now > timer0)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                RemoveNPC(c);
                trg0 = 2;
            }
        }

        if (trg0 == 2 && now > 60)
        {
            c = GetChar("冷燕");
            if (c >= 0)
            {
                Perform(c, "guard", 6);
                Perform(c, "say", "﹒﹒﹒﹒﹒﹒﹒");
                Perform(c, "pause", 4);
                Perform(c, "say", "怕了吗？？");
                Perform(c, "faceto", player);
                Perform(c, "pause", 4);
                Perform(c, "say", "来呀！！怎麽？？你就只有这麽一点能耐吗？？");
                Perform(c, "faceto", player);

                PlayerPerform("say", "（想不到一个不注意竟然落得如此田地）");
                PlayerPerform("pause", 4);
                PlayerPerform("say", "﹒﹒﹒我﹒﹒并没有意要跟你争！！");
                PlayerPerform("pause", 4);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 4);

                trg0 = 3;
            }
        }

        if (trg0 == 3 && now > 90)
        {
            c = GetChar("冷燕");
            if (c >= 0)
            {
                Perform(c, "guard", 7);
                Perform(c, "say", "别以为我是个弱女子！就对我手下留情！！");
                Perform(c, "faceto", player);
                Perform(c, "pause", 5);
                Perform(c, "say", "快呀！只有拼个你死活我，只有最後留下的人才能离开！");
                Perform(c, "faceto", player);

                PlayerPerform("say", "（难道一开始就被人所设计﹒﹒﹒）");
                PlayerPerform("pause", 5);
                PlayerPerform("say", "﹒﹒﹒﹒﹒（这一切到底是怎麽一回儿事﹒﹒﹒范璇的死﹒﹒）");
                PlayerPerform("pause", 7);

                trg0 = 4;
            }
        }

        if (trg0 == 4 && now > 120)
        {
            c = GetChar("冷燕");
            if (c >= 0)
            {
                Perform(c, "guard", 7);
                Perform(c, "say", "你分明就是瞧不起我！今天要是不让你死，有辱我杀手的名号！！");
                Perform(c, "faceto", player);
                Perform(c, "pause", 5);
                Perform(c, "say", "﹒﹒﹒﹒﹒出手呀！认真一点！！");
                Perform(c, "faceto", player);

                PlayerPerform("say", "（杀手！﹒﹒﹒这女子也是一位杀手，难道她也是跟我一样，是来杀范璇的？）");
                PlayerPerform("say", "！！");
                PlayerPerform("pause", 5);
                PlayerPerform("say", "﹒﹒﹒﹒﹒（而这女子﹒﹒为什麽会在这﹒﹒）");
                PlayerPerform("pause", 4);

                trg0 = 5;
            }
        }

        if (trg0 == 5 && now > 150)
        {
            c = GetChar("冷燕");
            if (c >= 0)
            {
                Perform(c, "guard", 7);
                Perform(c, "say", "﹒﹒﹒﹒哼！！要不是我身中奇毒，你早就﹒﹒﹒﹒");
                Perform(c, "faceto", player);
                Perform(c, "pause", 5);
                Perform(c, "say", "﹒﹒﹒﹒﹒﹒");
                Perform(c, "faceto", player);
                Perform(c, "pause", 5);
                Perform(c, "say", "怎麽？知道我是杀手你就怕了吗？？");
                Perform(c, "faceto", player);

                PlayerPerform("say", "（她也被屠城下了毒﹒﹒﹒看样子我得想法子离开这里，再另寻他法﹒﹒）");
                PlayerPerform("pause", 5);
                PlayerPerform("say", "﹒﹒呜﹒﹒（体内的毒已经开始侵蚀全身了﹒﹒）");
                PlayerPerform("pause", 5);
                PlayerPerform("say", "（﹒﹒﹒﹒这﹒﹒那她﹒﹒﹒的主使者是谁？？）");
                PlayerPerform("pause", 4);

                trg0 = 6;
            }
        }

        if (trg0 == 6 && now > 180)
        {
            c = GetChar("冷燕");
            if (c >= 0)
            {
                ChangeBehavior(c, "idle");
                Perform(c, "faceto", player);
                Perform(c, "crouch", 0);
                Perform(c, "pause", 10);
                Perform(c, "say", "﹒﹒﹒﹒﹒﹒");
                Perform(c, "faceto", player);
                Perform(c, "pause", 7);
                Perform(c, "say", "快呀！为什麽你不杀了我﹒﹒﹒难道你不想要解药了吗！！");
                Perform(c, "faceto", player);
                Perform(c, "crouch", 1);

                PlayerPerform("say", "﹒﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 8);
                PlayerPerform("say", "﹒﹒﹒你先服下它吧﹒﹒﹒");
                PlayerPerform("pause", 5);

                trg0 = 7;
                timer1 = now + 17;
                gameover = 1;
            }
        }

        if (gameover != -1 && GetHP(player) <= 0)
        {
            gameover = -1;
            timer1 = now + 2;
        }
        c = GetAnyChar("冷燕");
        if (gameover != -1 && c >= 0 && GetHP(c) <= 0)
        {
            gameover = -1;
            timer1 = now + 2;
        }
        if ((gameover == 1 || gameover == -1) && now > timer1)
        {
            GameOver(gameover);
            gameover = 2;
        }

        return 0;
    }
    int g_iCurrentTime;
    int g_iPrevTime;
    int g_iShift;
    int g_iActiveState;
    int g_iActiveCount;
    int g_iMaxFloor;
    int g_iTimeStep;

    public override void Scene_OnLoad()
    {
        int i;
        string name = "";
        g_iMaxFloor = 100;
        g_iTimeStep = g_iLevel09StepTime;
        for (i = 1; i <= g_iMaxFloor; i = i + 1)
        {
            MakeString(ref name, "D_sn09f", i);
            SetSceneItem(name, "name", "machine");
            SetSceneItem(name, "attribute", "collision", 1);
        }
    }

    public override void Scene_OnInit()
    {
        int i;
        string name = "";

        g_iCurrentTime = Misc("gettime");
        g_iPrevTime = Misc("gettime");
        g_iActiveCount = 1;

        for (i = 1; i <= g_iMaxFloor; i = i + 1)
        {
            MakeString(ref name, "D_sn09f", i);
            SetSceneItem(name, "pose", 0, 0);
        }

        Misc("randomseries", 0, 1, 36);
        Misc("randomseries", 1, 37, 64);
        Misc("randomseries", 2, 65, 84);
        Misc("randomseries", 3, 85, 100);
    }

    public override int Scene_OnIdle()
    {
        if (g_iActiveCount > g_iMaxFloor)
        {
            return 0;
        }

        int diff;
        string name = "";
        g_iCurrentTime = Misc("gettime");
        diff = g_iCurrentTime - g_iPrevTime;

        if (diff > g_iTimeStep)
        {
            g_iPrevTime = g_iCurrentTime;

            if (g_iActiveCount <= 36)
            {
                g_iActiveState = 0;
                g_iShift = 1;
            }

            if (g_iActiveCount >= 37 && g_iActiveCount <= 64)
            {
                g_iActiveState = 1;
                g_iShift = 37;
            }

            if (g_iActiveCount >= 65 && g_iActiveCount <= 84)
            {
                g_iActiveState = 2;
                g_iShift = 65;
            }

            if (g_iActiveCount >= 85 && g_iActiveCount <= 100)
            {
                g_iActiveState = 3;
                g_iShift = 85;
            }

            int target = Misc("getseries", g_iActiveState, g_iActiveCount - g_iShift);
            MakeString(ref name, "D_sn09f", target);
            
            CreateEffect(name, "FloorFI1");
            SetSceneItem(name, "pose", 1, 0);
            
            //Output("Active", name, "State", g_iActiveState, g_iActiveCount);

            g_iActiveCount = g_iActiveCount + 1;
        }
        return 0;
    }
}

// 炼铁狱
public class LevelScript_sn10: LevelScriptBase
{
    int RoundTime = 30;
    int PlayerSpawn = 38;
    int PlayerSpawnDir = 100;
    int PlayerWeapon = 31;
    int PlayerWeapon2 = 33;
    int PlayerHP = 3000;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override GameResult OnUnitDead(MeteorUnit deadUnit)
    {
        //屠城死算作游戏结束
        if (deadUnit.Attr.NpcTemplate == "npc10_01")
            return GameResult.Win;
        return GameResult.None;
    }
    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;
    int timer0 = 0;
    int timer1 = 0;
    int timer2 = 0;
    int timer3 = 0;
    int timer4 = 0;
    int reborn = -1;
    string rebornName;

    public override void OnStart()
    {
        AddNPC("npc10_01");
        base.OnStart();
    }

    public override int OnUpdate()
    {
        int player = GetChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;
        int c2 = -1;
        int c3 = -1;
        int c4 = -1;
        int c5 = -1;
        int now = GetGameTime();

        if (trg0 == 0)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                Perform(c, "faceto", player);
                PlayerPerform("use", 14);
                trg0 = 1;
            }
        }

        if (trg0 == 1)
        {
            c = GetChar("屠城");
            SetTarget(0, "char", c);
            SetTarget(1, "char", player);
            if (c >= 0 && GetEnemy(c) == player && Distance(0, 1) < 150)
            {
                Perform(c, "use", 7);
                Perform(c, "use", 4);
                Perform(c, "aggress");
                Perform(c, "say", "现在只有拿你和冷燕的尸首去向帮主请罪！拿命来！！");
                Perform(c, "pause", 2);
                Perform(c, "say", "你不愿听命於我那只有死路一条！");
                Perform(c, "pause", 7);
                Perform(c, "say", "你们这些杀手，说穿了只是任人摆布的棋子！");
                Perform(c, "pause", 5);
                Perform(c, "say", "呵！你不知道的事情可多着！");
                Perform(c, "pause", 5);
                Perform(c, "say", "绝对比你的狗窝快活林还要来的快活多了！");
                Perform(c, "pause", 2);
                Perform(c, "say", "你就作我的左右手，包你有享不尽的容华富贵");
                Perform(c, "pause", 2);
                Perform(c, "say", "看你的功夫还有两下子，我很赏识你！");
                Perform(c, "pause", 5);
                Perform(c, "say", "怎麽？狗咬狗的感觉如何呀？？");
                Perform(c, "pause", 2);
                Perform(c, "say", "想不到你能活到现在，没把你们整死真是可惜");
                Perform(c, "faceto", player);

                PlayerPerform("block", 0);
                PlayerPerform("pause", 10);
                PlayerPerform("say", "（难道﹒﹒这一切的计划﹒﹒﹒）");
                PlayerPerform("pause", 4);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 7);
                PlayerPerform("say", "！？﹒﹒你﹒﹒怎知我是快活林的人！？");
                PlayerPerform("pause", 10);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 6);
                PlayerPerform("block", 1);

                trg0 = 2;
                timer1 = now + 80;
            }
        }

        if (trg0 == 2 && now > timer1)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                Perform(c, "use", 4);
                Perform(c, "use", 7);
                Perform(c, "say", "哈哈哈哈！！凭你这一点能耐是杀不了我的！！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);

                trg0 = 3;
                timer1 = now + 180;
            }
        }
        if (trg0 == 3 && now > timer1)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                Perform(c, "use", 4);
                Perform(c, "use", 7);
                Perform(c, "say", "你尽管放马过来吧！我先来陪你玩一玩再让你死！！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);

                trg0 = 4;
                timer1 = now + 180;
            }
        }
        if (trg0 == 4 && now > timer1)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                Perform(c, "use", 4);
                Perform(c, "use", 7);
                Perform(c, "say", "怎麽？害怕了吗？？");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);

                trg0 = 2;
                timer1 = now + 180;
            }
        }

        if (trg2 == 0)
        {
            c = GetChar("屠城");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 3)
            {
                Perform(c, "attack");
                Perform(c, "faceto", player);
                Perform(c, "say", "来人呀！杀了他！！");
                Perform(c, "pause", 3);
                Perform(c, "say", "你﹒﹒﹒你这家伙！！看我怎麽治你！！");
                Perform(c, "faceto", player);

                trg2 = 1;
                timer0 = now + 6;
            }
        }

        if (trg2 == 1 && now > timer0)
        {
            AddNPC("npc10_02");
            AddNPC("npc10_03");
            AddNPC("npc10_04");
            AddNPC("npc10_05");

            trg2 = 2;
        }
        if (trg2 == 2)
        {
            c = GetAnyChar("屠城");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "﹒﹒﹒﹒想不到﹒﹒﹒我会死在你﹒﹒手中﹒﹒﹒﹒");

                Perform(c, "﹒﹒﹒﹒﹒﹒");
                Perform(c, "pause", 3);

                ChangeBehavior(c2, "idle");
                Perform(c2, "faceto", c);
                ChangeBehavior(c3, "idle");
                Perform(c3, "faceto", c);
                ChangeBehavior(c4, "idle");
                Perform(c4, "faceto", c);
                ChangeBehavior(c5, "idle");
                Perform(c5, "faceto", c);

                trg2 = 3;
            }
        }

        if (trg3 == 0 && trg2 == 2)
        {
            c2 = GetAnyChar("蒙面人﹒甲");
            if (c2 >= 0 && GetHP(c2) <= 0)
            {
                reborn = c2;
                rebornName = "npc10_02";
                trg3 = 1;
                timer4 = now + 25;
            }
            c3 = GetAnyChar("蒙面人﹒乙");
            if (c3 >= 0 && GetHP(c3) <= 0)
            {
                reborn = c3;
                rebornName = "npc10_03";
                trg3 = 1;
                timer4 = now + 25;
            }
            c4 = GetAnyChar("蒙面人﹒丙");
            if (c4 >= 0 && GetHP(c4) <= 0)
            {
                reborn = c4;
                rebornName = "npc10_04";
                trg3 = 1;
                timer4 = now + 25;
            }
            c5 = GetAnyChar("蒙面人﹒丁");
            if (c5 >= 0 && GetHP(c5) <= 0)
            {
                reborn = c5;
                rebornName = "npc10_05";
                trg3 = 1;
                timer4 = now + 25;
            }
        }
        if (trg3 == 1 && now > timer4)
        {
            RemoveNPC(reborn);
            timer4 = now + 5;
            trg3 = 2;
        }
        if (trg3 == 2 && now > timer4)
        {
            AddNPC(rebornName);
            trg3 = 0;
        }
        
        if (trg4 == 0 && trg2 == 2)
        {
            c = GetChar("屠城");
            SetTarget(0, "char", c);
            SetTarget(1, "char", player);
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 5 && GetHP(player) < GetMaxHP(player) / 3 && Distance(0, 1) < 150)
            {
                Perform(c, "use", 15);
                Perform(c, "use", 15);
                Perform(c, "use", 15);
                Perform(c, "use", 15);
                Perform(c, "use", 19);
                Perform(c, "say", "可恶！竟然敬酒不吃，就让我成全你想死的念头吧！");
                Perform(c, "pause", 6);
                Perform(c, "say", "！？");
                Perform(c, "pause", 13);
                Perform(c, "say", "﹒﹒﹒﹒﹒");
                Perform(c, "pause", 10);
                Perform(c, "say", "你们不配当人，只配当狗！！");
                Perform(c, "pause", 6);
                Perform(c, "say", "！？");
                Perform(c, "pause", 5);
                Perform(c, "say", "能有这样的福份，你真该好好把握才是﹒﹒呵");
                Perform(c, "pause", 2);
                Perform(c, "say", "你们这些杀手终日只能处在不见天日的地方生活");
                Perform(c, "pause", 6);
                Perform(c, "say", "想必我要登上分舵主指日可待了！哇哈哈哈！");
                Perform(c, "pause", 2);
                Perform(c, "say", "我就拿她的尸首去跟「帮主﹒万鹏王」交差就可以了！");
                Perform(c, "pause", 2);
                Perform(c, "say", "反正，那女娃儿功夫那麽地差，也不须要她了");
                Perform(c, "pause", 7);
                Perform(c, "say", "还让你当我的左右手，算是很宽宏大量的了");
                Perform(c, "pause", 2);
                Perform(c, "say", "呵！大人有大量，我赦免你一死！");
                Perform(c, "pause", 2);
                Perform(c, "say", "我们「分舵主﹒范璇」一死，我自然就会登上分舵主的宝座了");
                Perform(c, "pause", 6);
                Perform(c, "say", "怎麽？知情後害怕了吗？？？呵﹒﹒");
                Perform(c, "pause", 3);
                Perform(c, "say", "只好由我来亲手解决了﹒﹒");
                Perform(c, "pause", 2);
                Perform(c, "say", "可是那女娃儿功夫差，被范璇逮个正着﹒﹒这下子暗杀行动曝了光");
                Perform(c, "pause", 2);
                Perform(c, "say", "我原本是要让那女娃儿杀掉范璇，然後让从正面闯入的你当个代罪羔羊﹒﹒");
                Perform(c, "pause", 6);
                Perform(c, "say", "我这就告诉你，你跟那女娃儿都是我找来要杀范璇的～");
                Perform(c, "pause", 2);
                Perform(c, "say", "哼！你们这些当人家乖狗儿的杀手，从不会过问委托人是谁");
                Perform(c, "pause", 6);
                Perform(c, "say", "怎麽？？很惊讶吗？？？");
                Perform(c, "pause", 5);
                Perform(c, "say", "这下子我可是找对人了！");
                Perform(c, "pause", 2);
                Perform(c, "say", "功夫自然是在你之下﹒不过，她只要配合好我的行动就足够了～");
                Perform(c, "pause", 2);
                Perform(c, "say", "那个女娃儿无法治的了你！");
                Perform(c, "pause", 5);
                Perform(c, "say", "我可是英雄惜英雄，留条後路给你走﹒﹒");
                Perform(c, "pause", 6);
                Perform(c, "say", "而且还有大把大把的银子可花，何乐而不为呢？");
                Perform(c, "pause", 2);
                Perform(c, "say", "我这还有一瓶解药，只要你点头就不必如此痛苦了");
                Perform(c, "pause", 4);
                Perform(c, "say", "早跟你说了，当我的左右手也没啥不好的嘛！");
                Perform(c, "pause", 4);
                Perform(c, "say", "怎麽啦？？毒性是不是已经侵蚀全身了！？");
                Perform(c, "pause", 2);
                Perform(c, "say", "所有人退下！！");
                Perform(c, "pause", 3);
                Perform(c, "faceto", player);

                PlayerPerform("block", 0);
                PlayerPerform("pause", 6);
                PlayerPerform("use", 19);
                PlayerPerform("say", "我不会让在你死的时候感到一丝痛苦地！！");
                PlayerPerform("pause", 6);
                PlayerPerform("say", "还好﹒﹒﹒我还留有一颗心﹒﹒﹒ 不像你﹒﹒");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "而你﹒﹒﹒竟然如此玩弄别人的性命﹒﹒﹒");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "但是﹒﹒﹒我从不会让目标感到痛苦，就让他死去﹒﹒﹒");
                PlayerPerform("pause", 8);
                PlayerPerform("say", "成天就像行尸走肉一般﹒﹒");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "为了生存，永远只能活在黑暗的地方﹒﹒﹒");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "﹒﹒﹒杀手﹒﹒虽然只是工具﹒﹒");
                PlayerPerform("pause", 6);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 5);
                PlayerPerform("use", 15);
                PlayerPerform("use", 15);
                PlayerPerform("use", 20);
                PlayerPerform("say", "你﹒﹒把人当做是什麽！！");
                PlayerPerform("pause", 9);
                PlayerPerform("say", "你！！﹒﹒﹒﹒");
                PlayerPerform("pause", 11);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 10);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 6);
                PlayerPerform("say", "好阴险的人﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 13);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 8);
                PlayerPerform("say", "难道﹒﹒范璇是你杀的﹒﹒﹒");
                PlayerPerform("pause", 5);
                PlayerPerform("say", "什麽！？");
                PlayerPerform("pause", 10);
                PlayerPerform("say", "﹒﹒﹒﹒﹒");
                PlayerPerform("pause", 5);
                PlayerPerform("say", "不必你的假好意！");
                PlayerPerform("pause", 8);
                PlayerPerform("say", "﹒﹒你！！");
                PlayerPerform("pause", 5);
                PlayerPerform("say", "（可恶﹒﹒﹒再这样下去﹒﹒﹒撑不了多久的﹒﹒）");
                PlayerPerform("pause", 8);
                PlayerPerform("say", "﹒﹒﹒呜﹒﹒﹒﹒");
                PlayerPerform("block", 1);

                c2 = GetChar("蒙面人﹒甲");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 145);
                    Perform(c2, "faceto", player);
                    ChangeBehavior(c2, "dodge", player);
                }
                c2 = GetChar("蒙面人﹒乙");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 145);
                    Perform(c2, "faceto", player);
                    ChangeBehavior(c2, "dodge", player);
                }
                c2 = GetChar("蒙面人﹒丙");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 145);
                    Perform(c2, "faceto", player);
                    ChangeBehavior(c2, "dodge", player);
                }
                c2 = GetChar("蒙面人﹒丁");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 145);
                    Perform(c2, "faceto", player);
                    ChangeBehavior(c2, "dodge", player);
                }

                trg4 = 1;
                trg1 = 1;
                trg0 = 5;
                timer1 = now + 120;
                timer2 = now + 120;
            }
        }

        if (trg4 == 1 && now > timer1)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                Perform(c, "say", "哈哈哈哈！！凭你这一点能耐是杀不了我的！！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);

                trg4 = 2;
                timer1 = now + 180;
            }
        }
        if (trg4 == 2 && now > timer1)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                Perform(c, "say", "你尽管放马过来吧！我先来陪你玩一玩再让你死！！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);

                trg4 = 3;
                timer1 = now + 180;
            }
        }
        if (trg4 == 3 && now > timer1)
        {
            c = GetChar("屠城");
            if (c >= 0)
            {
                Perform(c, "say", "怎麽？害怕了吗？？");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);

                trg4 = 1;
                timer1 = now + 180;
            }
        }

        if (trg1 == 1 && now > timer2)
        {
            c = GetChar("屠城");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 4)
            {
                Perform(c, "use", 15);
                Perform(c, "say", "哼！气死我了！！看我怎麽让你死！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);
                trg1 = 2;
                timer2 = now + 60;
            }
        }
        if (trg1 == 2 && now > timer2)
        {
            c = GetChar("屠城");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 4)
            {
                Perform(c, "use", 15);
                Perform(c, "say", "想不到你真有两下子﹒﹒真是低估了你！！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);
                trg1 = 3;
                timer2 = now + 60;
            }
        }
        if (trg1 == 3 && now > timer2)
        {
            c = GetChar("屠城");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 4)
            {
                Perform(c, "use", 15);
                Perform(c, "say", "﹒﹒﹒可恶！你死定了！！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);
                trg1 = 4;
                timer2 = now + 60;
            }
        }
        if (trg1 == 4 && now > timer2)
        {
            c = GetChar("屠城");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 4)
            {
                Perform(c, "use", 15);
                Perform(c, "say", "﹒﹒﹒可恶！真是气死我了！！");
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                ChangeBehavior(c, "dodge", player);
                trg1 = 1;
                timer2 = now + 60;
            }
        }
        return 1;
    }
}

//五爪峰
public class LevelScript_sn11 : LevelScriptBase
{
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    int g_iDoorMaxHP;
    int DoorState1HP;
    int DoorState2HP;
    int DoorState3HP;

    int ADoorHP;
    int BDoorHP;
    int ADoorAlive;
    int BDoorAlive;

    public override void Scene_OnLoad()
    {
        int i;
        string name = "";

        g_iDoorMaxHP = g_iLevel11DoorMaxHP;
        DoorState1HP = (g_iDoorMaxHP * 3) / 4;
        DoorState2HP = (g_iDoorMaxHP * 2) / 4;
        DoorState3HP = (g_iDoorMaxHP * 1) / 4;

        for (i = 1; i <= 5; i++)
        {
            MakeString(ref name, "D_APdoor", i);
            SetSceneItem(name, "name", "machine");
            SetSceneItem(name, "attribute", "collision", 0);
            SetSceneItem(name, "pose", 0, 0);

            MakeString(ref name, "D_BPdoor", i);
            SetSceneItem(name, "name", "machine");
            SetSceneItem(name, "attribute", "collision", 0);
            SetSceneItem(name, "pose", 0, 0);
        }

        SetSceneItem("D_APdoor06", "name", "machine");
        SetSceneItem("D_APdoor06", "attribute", "visible", 0);
        SetSceneItem("D_APdoor06", "attribute", "collision", 1);
        SetSceneItem("D_APdoor06", "pose", 0, 0);

        SetSceneItem("D_BPdoor06", "name", "machine");
        SetSceneItem("D_BPdoor06", "attribute", "visible", 0);
        SetSceneItem("D_BPdoor06", "attribute", "collision", 1);
        SetSceneItem("D_BPdoor06", "pose", 0, 0);
    }

    public override void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);

        int i;
        string name = "";

        ADoorHP = g_iDoorMaxHP;
        ADoorAlive = 1;
        BDoorHP = g_iDoorMaxHP;
        BDoorAlive = 1;

        SetSceneItem("D_APdoor01", "attribute", "active", 1);
        SetSceneItem("D_APdoor01", "pose", 0, 0);

        SetSceneItem("D_BPdoor01", "attribute", "active", 1);
        SetSceneItem("D_BPdoor01", "pose", 0, 0);

        for (i = 2; i <= 5; i++)
        {
            MakeString(ref name, "D_APdoor", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "pose", 0, 0);

            MakeString(ref name, "D_BPdoor", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "pose", 0, 0);
        }
    }

    public int D_APdoor01_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 1)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        ADoorHP = ADoorHP - damage;
        Output("ADoor 1", ADoorHP);
        if (ADoorHP < DoorState1HP)
        {
            
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_APdoor02", "attribute", "active", 1);
            
        }
        return 1;
    }

    public int D_APdoor02_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 1)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        ADoorHP = ADoorHP - damage;
        Output("ADoor 2", ADoorHP);
        if (ADoorHP < DoorState2HP)
        {
            
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_APdoor03", "attribute", "active", 1);
            
        }
        return 1;
    }

    public int D_APdoor03_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 1)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        ADoorHP = ADoorHP - damage;
        Output("ADoor 3", ADoorHP);
        if (ADoorHP < DoorState3HP)
        {
            
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_APdoor04", "attribute", "active", 1);
            
        }
        return 1;
    }

    public int D_APdoor04_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 1)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        ADoorHP = ADoorHP - damage;
        Output("ADoor 4", ADoorHP);
        if (ADoorHP <= 0)
        {
            
            CreateEffect(id, "GiMaBRK");
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_APdoor05", "attribute", "active", 1);
            SetSceneItem("D_APdoor05", "attribute", "collision", 0);
            SetSceneItem("D_APdoor05", "pose", 1, 0);
            
            GameCallBack("end", 2);
        }
        return 1;
    }

/* team 2 */
    public int D_BPdoor01_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 2)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        BDoorHP = BDoorHP - damage;
        Output("BDoor 1", BDoorHP);
        if (BDoorHP < DoorState1HP)
        {
            
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_BPdoor02", "attribute", "active", 1);
            
        }
        return 1;
    }

    public int D_BPdoor02_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 2)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        BDoorHP = BDoorHP - damage;
        Output("BDoor 2", BDoorHP);
        if (BDoorHP < DoorState2HP)
        {
            
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_BPdoor03", "attribute", "active", 1);
            
        }
        return 1;
    }

    public int D_BPdoor03_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 2)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        BDoorHP = BDoorHP - damage;
        Output("BDoor 3", BDoorHP);
        if (BDoorHP < DoorState3HP)
        {
            
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_BPdoor04", "attribute", "active", 1);
            
        }
        return 1;
    }

    public int D_BPdoor04_OnAttack(int id, int CharacterId, int damage)
    {
        if (GetTeam(CharacterId) == 2)
        {
            return 0;
        }

        int state = GetSceneItem(id, "state");
        if (state == 3)
        {
            
            CreateEffect(id, "GiMaHIT");
            SetSceneItem(id, "pose", 1, 0);
            
        }

        BDoorHP = BDoorHP - damage;
        Output("BDoor 4", BDoorHP);
        if (BDoorHP <= 0)
        {
            
            CreateEffect(id, "GiMaBRK");
            SetSceneItem(id, "attribute", "active", 0);
            SetSceneItem("D_BPdoor05", "attribute", "active", 1);
            SetSceneItem("D_BPdoor05", "pose", 1, 0);
            
            GameCallBack("end", 1);
        }
        return 1;
    }

}
//烽火雷
public class LevelScript_sn12 : LevelScriptBase
{
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    int g_iMaxStoves = 6;
    int g_iMaxStoveHP = 200;
    int []g_bStoveAlive;
    int []g_iStoveHP;

    int g_iTeamAStoves;
    int g_iTeamBStoves;

    public override void Scene_OnLoad()
    {
        int i;
        string name = "";

        g_iMaxStoveHP = g_iLevel12StoveHP;

        for (i = 1; i <= g_iMaxStoves; i++)
        {
            MakeString(ref name, "D_AStove", i);
            SetSceneItem(name, "name", "machine");
            MakeString(ref name, "D_BStove", i);
            SetSceneItem(name, "name", "machine");
        }
    }

    public override void Scene_OnInit()
    {
        int i;
        string name = "";

        g_iTeamAStoves = 3;
        g_iTeamBStoves = 3;
        g_bStoveAlive = new int[g_iMaxStoves];
        g_iStoveHP = new int[g_iMaxStoves];
        for (i = 1; i <= g_iMaxStoves; i++)
        {
            g_bStoveAlive[i - 1] = 1;
            g_iStoveHP[i - 1] = g_iMaxStoveHP;
            MakeString(ref name, "D_AStove", i);
            SetSceneItem(name, StoveOnAttack, null);
            SetSceneItem(name, "pose", 0, 0);
            SetSceneItem(name, "attribute", "active", 1);
            SetSceneItem(name, "attribute", "collision", 1);

            MakeString(ref name, "D_BStove", i);
            SetSceneItem(name, StoveOnAttack, null);
            SetSceneItem(name, "pose", 0, 0);
            SetSceneItem(name, "attribute", "active", 1);
            SetSceneItem(name, "attribute", "collision", 1);
        }

        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
    }

    public int StoveOnAttack(int index, int characterid, int damage)
    {
        if (g_bStoveAlive[index - 1] == 0)
        {
            return 0;
        }
        Output("OnAttack", index, g_iStoveHP[index - 1]);
        string stovename = "";
        if (index <= 3)
        {
            if (GetTeam(characterid) == 1)
            {
                return 0;
            }
            MakeString(ref stovename, "D_AStove", index);
        }
        else
        {
            if (GetTeam(characterid) == 2)
            {
                return 0;
            }
            MakeString(ref stovename, "D_BStove", index - 3);
        }

        int id = GetSceneItem(stovename, "index");
        g_iStoveHP[index - 1] = g_iStoveHP[index - 1] - damage;
        if (g_iStoveHP[index - 1] > 0)
        {
            
            CreateEffect(id, "FireHIT");
            int state = GetSceneItem(id, "state");
            Output("state", state);
            if (state == 3)
            {
                Output("Shake", id);
                SetSceneItem(id, "pose", 1, 0);
            }
            
            return 0;
        }
        g_bStoveAlive[index - 1] = 0;
        if (index >= 1 && index <= 3)
        {
            g_iTeamAStoves = g_iTeamAStoves - 1;
        }
        else
        {
            g_iTeamBStoves = g_iTeamBStoves - 1;
        }

        
        CreateEffect(id, "FireBRK");
        SetSceneItem(id, "pose", 2, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        

        if (g_iTeamAStoves == 0)
        {
            GameCallBack("end", 2);
        }
        if (g_iTeamBStoves == 0)
        {
            GameCallBack("end", 1);
        }

        return 1;
    }
}

//金华城
public class LevelScript_sn13 : LevelScriptBase
{
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override GameResult OnUnitDead(MeteorUnit deadUnit)
    {
        //捕头王强被击杀后，认为游戏胜利.
        if (deadUnit.Attr.NpcTemplate == "npc13_02")
            return GameResult.Win;
        return GameResult.None;
    }
    int RoundTime = 15;
    int PlayerSpawn = 249;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 24;
    int PlayerWeapon2 = 14;
    int PlayerHP = 2000;

    int g_counter = 0;
    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;
    int trg5 = 0;
    int timer0 = 0;
    int survivor = -1;

    public override void OnStart()
    {
        AddNPC("npc13_01");
        AddNPC("npc13_02");
        AddNPC("npc13_03");
        AddNPC("npc13_04");
        AddNPC("npc13_05");
        AddNPC("npc13_06");
        AddNPC("npc13_07");
        AddNPC("npc13_08");
        base.OnStart();
    }

    public override void OnLateStart()
    {
        U3D.ShowLeaderSfx();
    }

    public override int OnUpdate()
    {
        int player = GetChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c = -1;
        int c2 = -1;
        int c3 = -1;
        int c4 = -1;
        int c5 = -1;
        int t = -1;

        g_counter = g_counter + 1;

        if (trg0 == 0)
        {
            c = GetChar("冷燕");
            c2 = GetChar("官差﹒甲");
            c3 = GetChar("官差﹒乙");
            c4 = GetChar("官差﹒丙");
            c5 = GetChar("夜猫子");
            if (c >= 0 && c2 >= 0 && c3 >= 0 && c4 >= 0 && c5 >= 0)
            {
                ChangeBehavior(c, "follow", player);
                Perform(c, "pause", 8);
                Perform(c, "aggress");
                Perform(c, "say", "你有本事就来呀！");
                Perform(c, "pause", 10);
                Perform(c, "faceto", c3);

                ChangeBehavior(c2, "follow", c);
                Perform(c2, "pause", 15);
                Perform(c2, "aggress");
                Perform(c2, "say", "你这臭娘们，总算让我找到了！看你这下子往那跑！");
                Perform(c2, "pause", 2);
                Perform(c2, "faceto", c);

                ChangeBehavior(c3, "follow", player);
                Perform(c3, "pause", 5);
                Perform(c3, "aggress");
                Perform(c3, "say", "在一旁的小子没事就给我快滚开！否则连你也一起押走！");
                Perform(c3, "pause", 15);
                Perform(c3, "faceto", c);

                ChangeBehavior(c4, "follow", player);
                Perform(c4, "pause", 5);
                Perform(c4, "say", "哟～这小子该不会是这女飞贼的帮手，管他是谁，通通讨打！");
                Perform(c4, "pause", 20);
                Perform(c4, "faceto", c);

                ChangeBehavior(c5, "patrol", 172, 127, 133, 127);
                Perform(c5, "pause", 10);
                Perform(c5, "faceto", player);
                Perform(c5, "aggress");
                Perform(c5, "pause", 20);
                Perform(c5, "faceto", player);

                PlayerPerform("block", 0);
                PlayerPerform("pause", 20);
                PlayerPerform("block", 1);

                trg0 = 1;
                timer0 = GetGameTime() + 25;
            }
        }

        if (trg0 == 1)
        {
            c = GetChar("冷燕");
            c2 = GetAnyChar("官差﹒甲");
            c3 = GetAnyChar("官差﹒乙");
            c4 = GetAnyChar("官差﹒丙");
            c5 = -1;

            if (GetHP(c2) > 0 && GetHP(c3) <= 0 && GetHP(c4) <= 0)
            {
                c5 = c2;
            }
            if (GetHP(c3) > 0 && GetHP(c2) <= 0 && GetHP(c4) <= 0)
            {
                c5 = c3;
            }
            if (GetHP(c4) > 0 && GetHP(c3) <= 0 && GetHP(c2) <= 0)
            {
                c5 = c4;
            }
            if (GetHP(c2) <= 0 && GetHP(c3) <= 0 && GetHP(c4) <= 0)
            {
                c5 = 9999;
            }

            if (c5 >= 0)
            {
                if (c5 != 9999)
                {
                    ChangeBehavior(c5, "follow", "vip");
                    Perform(c5, "aggress");
                    Perform(c5, "say", "可恶，你给我记住！！");
                    Perform(c5, "faceto", player);

                    Perform(c, "aggress");
                    Perform(c, "say", "来呀～我就等你，谁怕谁呀！！");
                    Perform(c, "pause", 2);
                    Perform(c, "faceto", c5);

                    PlayerPerform("say", "﹒﹒﹒﹒﹒﹒");
                    PlayerPerform("pause", 5);

                    survivor = c5;
                }

                trg0 = 2;
            }
        }

        if (trg0 == 2 && survivor >= 0)
        {
            c = GetChar("捕头﹒王强");
            if (c >= 0 && GetHP(survivor) > 0)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", survivor);
                if (Distance(0, 1) < 150)
                {
                    Perform(survivor, "say", "是！");
                    Perform(survivor, "pause", 5);
                    Perform(survivor, "say", "那个女飞贼找到了，但是他身边有个高手，我打不蠃他所以﹒﹒");
                    Perform(survivor, "pause", 5);
                    Perform(survivor, "say", "报﹒﹒报报﹒报﹒");
                    Perform(survivor, "faceto", c);

                    ChangeBehavior(c, "follow", player);
                    Perform(c, "say", "什麽！走！！管他是谁，一律捉起来！！");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "报你个头！有话快说有屁快放！！");
                    Perform(c, "pause", 3);
                    Perform(c, "faceto", survivor);

                    c2 = GetChar("军枪官差﹒甲");
                    if (c2 >= 0)
                    {
                        Perform(c2, "say", "是！");
                        Perform(c2, "guard", 12);
                        Perform(c2, "faceto", c);
                    }
                    c2 = GetChar("军枪官差﹒乙");
                    if (c2 >= 0)
                    {
                        Perform(c2, "say", "是！");
                        Perform(c2, "guard", 12);
                        Perform(c2, "faceto", c);
                    }

                    trg0 = 3;
                }
            }
        }

        if (trg1 == 0)
        {
            c = GetChar("捕头﹒王强");
            c2 = GetChar("冷燕");

            SetTarget(0, "char", c);
            SetTarget(1, "char", player);
            if (c >= 0 && c2 >= 0 && GetEnemy(c) == player && Distance(0, 1) < 200)
            {
                ChangeBehavior(c, "follow", player);
                if (trg0 == 3)
                {
                    Perform(c, "say", "哼！！你这女飞贼如此叼蛮！所有人给我拿下他来！");
                    Perform(c, "pause", 8);
                    Perform(c, "say", "炽雪城的通关信物被劫走了，八成也是你干的吧！ ");
                    Perform(c, "pause", 1);
                    Perform(c, "say", "好狂妄的口气！");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "臭小子就是你吧！看样子你跟那女贼是一夥的！");
                    Perform(c, "faceto", player);

                    Perform(c2, "pause", 5);
                    Perform(c2, "say", "跟你们说没拿就是没拿！！要不然你想怎麽样！！");
                    Perform(c2, "pause", 10);
                    Perform(c2, "say", "贼什麽！我叫'冷燕'凭什麽认定我是贼！");
                    Perform(c2, "pause", 2);
                    Perform(c2, "faceto", c);

                    PlayerPerform("block", 0);
                    PlayerPerform("pause", 5);
                    PlayerPerform("say", "（﹒﹒﹒该不会就是我身上的那一个信物？﹒﹒）");
                    PlayerPerform("pause", 6);
                    PlayerPerform("say", "﹒﹒﹒﹒﹒﹒");
                    PlayerPerform("pause", 4);
                    PlayerPerform("block", 1);

                    StopPerform(survivor);

                    t = 17;
                }
                else
                {
                    Perform(c, "say", "管你是谁，只要跟这女飞贼一起的一律捉起来！！");
                    Perform(c, "pause", 3);
                    Perform(c, "say", "好狂妄的口气！竟然还找了个帮手！");
                    Perform(c, "pause", 5);
                    Perform(c, "say", "哈哈哈！你这女飞贼这下子插翅也难飞了！");
                    Perform(c, "faceto", player);

                    Perform(c2, "pause", 5);
                    Perform(c2, "say", "贼什麽！我叫'冷燕'凭什麽认定我是贼！");
                    Perform(c2, "pause", 2);
                    Perform(c2, "faceto", c);

                    PlayerPerform("block", 0);
                    PlayerPerform("pause", 4);
                    PlayerPerform("say", "﹒﹒﹒﹒﹒﹒");
                    PlayerPerform("pause", 4);
                    PlayerPerform("block", 1);

                    survivor = -1;
                    t = 10;
                }

                c3 = GetChar("军枪官差﹒甲");
                if (c3 >= 0)
                {
                    StopPerform(c3);
                    Perform(c3, "say", "是！");
                    Perform(c3, "guard", t);
                    Perform(c3, "faceto", player);
                }
                c3 = GetChar("军枪官差﹒乙");
                if (c3 >= 0)
                {
                    StopPerform(c3);
                    Perform(c3, "say", "是！");
                    Perform(c3, "guard", t);
                    Perform(c3, "faceto", player);
                }
                c3 = GetChar("官差﹒甲");
                if (c3 >= 0)
                {
                    Perform(c3, "guard", t);
                    Perform(c3, "faceto", player);
                }
                c3 = GetChar("官差﹒乙");
                if (c3 >= 0)
                {
                    Perform(c3, "guard", t);
                    Perform(c3, "faceto", player);
                }
                c3 = GetChar("官差﹒丙");
                if (c3 >= 0)
                {
                    Perform(c3, "guard", t);
                    Perform(c3, "faceto", player);
                }
                c3 = GetChar("夜猫子");
                if (c3 >= 0)
                {
                    Perform(c3, "guard", t);
                    Perform(c3, "faceto", player);
                }

                trg1 = 1;
                trg5 = 1;
            }
        }

        if (trg5 == 0)
        {
            c = GetChar("捕头﹒王强");
            if (c >= 0)
            {
                if (g_counter % 45 == 0 && timer0 > 0 && GetGameTime() > timer0 && GetEnemy(c) < 0)
                {
                    Perform(c, "say", "跑那去了？？所有人就算翻了金华城也要给我找出来！！");
                }

                c3 = GetChar("军枪官差﹒甲");
                if (c3 >= 0 && (GetEnemy(c3) == player || GetEnemy(c3) == c2))
                {
                    Perform(c3, "say", "发现了！！");
                    Perform(c3, "faceto", player);
                    ChangeBehavior(c, "follow", player);
                    trg1 = 1;
                }
                c3 = GetChar("军枪官差﹒乙");
                if (c3 >= 0 && (GetEnemy(c3) == player || GetEnemy(c3) == c2))
                {
                    Perform(c3, "say", "发现了！！");
                    Perform(c3, "faceto", player);
                    ChangeBehavior(c, "follow", player);
                    trg1 = 1;
                }
                trg5 = 1;
            }
        }

        if (trg2 == 0)
        {
            c = GetChar("冷燕");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "say", "真对不起，无故地把你牵扯进来﹒真是过意不去﹒﹒");
                Perform(c, "faceto", player);
                trg2 = 1;
            }
        }
        if (trg2 == 1)
        {
            c = GetAnyChar("冷燕");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "我﹒﹒﹒好累﹒﹒我先休﹒﹒﹒息﹒﹒");
                trg2 = 2;
            }
        }

        if (g_counter % 20 == 0 && timer0 > 0 && GetGameTime() > timer0)
        {
            c = GetChar("冷燕");
            if (c >= 0 && !IsPerforming(player) && !IsUnitDead(c))
            {
                SetTarget(0, "char", player);
                SetTarget(1, "char", c);
                if (Distance(0, 1) > 500)
                {
                    Say(c, "等等我！");
                }

                if (g_counter % 40 == 0 && trg2 == 1)
                {
                    Perform(c, "guard", 2);
                    Perform(c, "faceto", player);
                    Perform(c, "pause", 2);
                    Perform(c, "faceto", player);
                    Perform(c, "say", "好累哟～我想先休息一下！");
                }
            }
        }

        if (trg3 == 0)
        {
            c = GetChar("夜猫子");
            if (c >= 0 && GetEnemy(c) == player)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", player);
                if (Distance(0, 1) < 200)
                {
                    ChangeBehavior(c, "kill", player);
                    Perform(c, "aggress");
                    Perform(c, "say", "嘻嘻嘻！放马过来吧！");
                    Perform(c, "say", "呵呵呵！你这臭小子又被我遇到了！这次你可跑不掉了！");
                    Perform(c, "faceto", player);
                    trg3 = 1;
                }
            }
        }
        if (trg3 == 1)
        {
            c = GetChar("夜猫子");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                ChangeBehavior(c, "dodge", player);
                Perform(c, "say", "嘻﹒﹒还真有两下子﹒﹒﹒看我怎麽玩你！");
                Perform(c, "faceto", player);
                trg3 = 2;
            }
        }
        if (trg3 == 2)
        {
            c = GetAnyChar("夜猫子");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "咳﹒﹒别﹒﹒别以为这样子就结束了﹒﹒还会有更多的﹒﹒");
                trg3 = 3;
            }
        }

        if (trg4 == 0)
        {
            c = GetChar("捕头﹒王强");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "say", "可恶！！！你们真是惹脑了我！！");
                Perform(c, "faceto", player);
                trg4 = 1;
            }
        }
        if (trg4 == 1)
        {
            c = GetAnyChar("捕头﹒王强");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "我﹒﹒竟然会哉在你﹒﹒的手下﹒﹒﹒");
                trg4 = 2;
                GameOver(1);
            }
        }
        return 1;
    }

    int g_bBridge01Alive;
    int g_bBridge02Alive;
    int g_iBridge01HP;
    int g_iBridge02HP;

    public override void Scene_OnLoad()
    {
        //int i;
        //string name = "";

        SetSceneItem("D_bridge01", "name", "machine");
        SetSceneItem("D_bridge01", "attribute", "collision", 1);
        SetSceneItem("D_bridge01", "attribute", "damagevalue", 300);

        SetSceneItem("D_bridge02", "name", "machine");
        SetSceneItem("D_bridge02", "attribute", "collision", 1);
        SetSceneItem("D_bridge02", "attribute", "damagevalue", 300);
    }

    public override void Scene_OnInit()
    {
        //int i;
        //string name = "";

        g_bBridge01Alive = 1;
        g_iBridge01HP = g_iLevel13BridgeHP;
        g_bBridge02Alive = 1;
        g_iBridge02HP = g_iLevel13BridgeHP;

        SetSceneItem("D_bridge01", "attribute", "active", 1);
        SetSceneItem("D_bridge01", "attribute", "damage", 0);
        SetSceneItem("D_bridge01", "pose", 0, 0);
        SetSceneItem("D_bridge02", "attribute", "active", 1);
        SetSceneItem("D_bridge02", "attribute", "damage", 0);
        SetSceneItem("D_bridge02", "pose", 0, 0);

        SetSceneItem("D_itbridge01", "attribute", "active", 0);
        SetSceneItem("D_itbridge01", "attribute", "interactive", 0);
        SetSceneItem("D_itbridge02", "attribute", "active", 0);
        SetSceneItem("D_itbridge02", "attribute", "interactive", 0);

        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);

    }

    public int D_bridge01_OnAttack(int id, int characterid, int damage)
    {
        if (g_bBridge01Alive == 1)
        {
            g_iBridge01HP = g_iBridge01HP - damage;
            Output("Bridge 01", g_iBridge01HP);
            if (g_iBridge01HP <= 0)
            {
                int pose = GetSceneItem(id, "pose");
                if (pose == 2)
                {
                    return 0;
                }

                
                CreateEffect(id, "BridgBRK");
                SetSceneItem(id, "pose", 2, 0);
                SetSceneItem(id, "attribute", "damage", 1);
                SetSceneItem("D_itbridge01", "attribute", "active", 1);
                SetSceneItem("D_itbridge01", "attribute", "interactive", 1);
                
            }
            else
            {
                int state = GetSceneItem(id, "state");
                if (state == 3)
                {
                    
                    CreateEffect(id, "BridgHIT");
                    SetSceneItem(id, "pose", 1, 0);
                    
                }
            }
        }
        return 1;
    }

    public void D_bridge01_OnIdle(int id)
    {
        if (g_bBridge01Alive == 1)
        {
            int pose = GetSceneItem(id, "pose");
            if (pose != 2)
            {
                return;
            }
            int state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bBridge01Alive = 0;
                
                SetSceneItem(id, "attribute", "damage", 0);
                SetSceneItem(id, "attribute", "active", 0);
                
            }
        }
        return;
    }

    public int D_bridge02_OnAttack(int id, int characterid, int damage)
    {
        if (g_bBridge02Alive == 1)
        {
            g_iBridge02HP = g_iBridge02HP - damage;
            Output("Bridge 02", g_iBridge02HP);
            if (g_iBridge02HP <= 0)
            {
                int pose = GetSceneItem(id, "pose");
                if (pose == 2)
                {
                    return 0;
                }

                
                CreateEffect(id, "BridgBRK");
                SetSceneItem(id, "pose", 2, 0);
                SetSceneItem(id, "attribute", "damage", 1);
                SetSceneItem("D_itbridge02", "attribute", "active", 1);
                SetSceneItem("D_itbridge02", "attribute", "interactive", 1);
                
            }
            else
            {
                int state = GetSceneItem(id, "state");
                if (state == 3)
                {
                    
                    CreateEffect(id, "BridgHIT");
                    SetSceneItem(id, "pose", 1, 0);
                    
                }
            }
        }
        return 1;
    }

    public void D_bridge02_OnIdle(int id)
    {
        if (g_bBridge02Alive == 1)
        {
            int pose = GetSceneItem(id, "pose");
            if (pose != 2)
            {
                return;
            }
            int state = GetSceneItem(id, "state");
            if (state == 3)
            {
                g_bBridge02Alive = 0;
                
                SetSceneItem(id, "attribute", "damage", 0);
                SetSceneItem(id, "attribute", "active", 0);
                
            }
        }
        return;
    }
}

//炎硫岛
public class LevelScript_sn14 : LevelScriptBase
{
    int RoundTime = 10;
    int PlayerSpawn = 34;
    int PlayerSpawnDir = 0;
    int PlayerWeapon = 6;
    int PlayerWeapon2 = 0;
    int PlayerHP = 1000;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override void OnStart()
    {
	    AddNPC("npc14_01");
	    AddNPC("npc14_02");
        base.OnStart();
    }

    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;

    public override int OnUpdate()
    {
        int player = GetChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;
        int c2;

        if (trg0 == 0)
        {
            c = GetChar("大刀哨兵");
            c2 = GetChar("铁枪哨兵");

            if (c >= 0 && c2 >= 0)
            {
                ChangeBehavior(c, "patrol", 0);
                Perform(c, "say", "好呗，我也懒得听你的冷笑话﹒﹒");
                Perform(c, "pause", 12);
                Perform(c, "say", "真的很冷耶﹒﹒");
                Perform(c, "say", "﹒﹒﹒﹒﹒﹒﹒");
                Perform(c, "aggress");
                Perform(c, "say", "冷个屁呀？四周都是岩浆，热到爆了！真想喝口酒来解热呀！");
                Perform(c, "pause", 12);
                Perform(c, "say", "奇怪了，为啥老大会叫我们来这种鸟不生蛋的地方守！=.=");
                Perform(c, "faceto", c2);

                ChangeBehavior(c2, "patrol", 16, 18, 19, 20, 21);
                Perform(c2, "say", "企企企！");
                Perform(c2, "pause", 8);
                Perform(c2, "say", "下次可能真被会被派到北方冰起来也说不定～");
                Perform(c2, "say", "﹒﹒﹒﹒我还是快回去守着，以免又被发现我们偷懒～");
                Perform(c2, "aggress", 10);
                Perform(c2, "say", "喂～我是在讲个冷笑话来解解闷，你不到你这呆头听不懂呀～");
                Perform(c2, "pause", 8);
                Perform(c2, "say", "才被分派到这种鸟地方把我们 '冷冻' 起来﹒﹒Ｘ滴！");
                Perform(c2, "say", "啊知～看样子是我们上次夜哨时偷溜出去喝酒被捉包！");
                Perform(c2, "pause", 8);
                Perform(c2, "faceto", c);

                trg0 = 1;
            }
        }

        //当大刀哨兵发现主角，停止铁枪和大刀兵之前的行为，进行默认看到敌方时的处理-追击敌方/（丢失视野后会停止追击） 
        //Kill指令无视视野-无论多远都可以看到要追杀的目标
        if (trg1 == 0)
        {
            c = GetChar("大刀哨兵");
            if (c >= 0 && GetEnemy(c) == player)
            {
                StopPerform(c);
                c2 = GetChar("铁枪哨兵");
                StopPerform(c2);

                Perform(c, "say", "好大的胆子竟敢乱闯禁地，没酒喝，火气正大，先拿你来开刀！");
                trg1 = 1;
            }
        }
        if (trg1 == 1)
        {
            c = GetChar("大刀哨兵");
            if (c >= 0 && GetEnemy(c) != player)
            {
                Perform(c, "say", "跑到那儿去了！八成是跌入岩浆中了，哈哈哈哈！");
                trg1 = 2;
            }
        }
        if (trg1 == 2)
        {
            c = GetChar("大刀哨兵");
            if (c >= 0 && GetEnemy(c) == player)
            {
                Perform(c, "say", "好小子，被我发现了吧！这下子你跑不掉了！");
                trg1 = 3;
            }
        }
        if (trg1 == 3)
        {
            c = GetChar("大刀哨兵");
            if (c >= 0 && GetEnemy(c) != player)
            {
                Perform(c, "aggress");
                Perform(c, "say", "哼，又躲起来了！缩头鸟龟一只！");
                trg1 = 4;
            }
        }

        if (trg2 == 0)
        {
            c = GetChar("铁枪哨兵");
            if (c >= 0 && GetEnemy(c) == player)
            {
                StopPerform(c);
                c2 = GetAnyChar("大刀哨兵");
                StopPerform(c2);

                if (c2 >= 0 && GetHP(c2) > 0)
                {
                    ChangeBehavior(c2, "follow", c);
                    Perform(c, "say", "大胡子！！快点过来！有入侵者呀！！");
                }
                else
                {
                    ChangeBehavior(c, "follow", player);
                    Perform(c, "attack");
                    Perform(c, "say", "大胡子人呢？该不会是你把他给杀了！我要你连他的命一起赔上！受死！");
                }

                Perform(c, "guard", 4);
                Perform(c, "say", "谁！你这生面孔八成不是什麽好东西，入侵者一律杀之！");
                Perform(c, "faceto", player);

                trg2 = 1;
            }
        }
        if (trg2 == 1)
        {
            c = GetChar("铁枪哨兵");
            if (c >= 0 && GetEnemy(c) != player)
            {
                Perform(c, "say", "可恶，这臭小还真会躲！再让我见到要你好看！");
                trg2 = 2;
            }
        }
        if (trg2 == 2)
        {
            c = GetChar("铁枪哨兵");
            if (c >= 0 && GetEnemy(c) == player)
            {
                Perform(c, "attack");
                Perform(c, "say", "哈！你是躲不过我的视线的！拿命来！");
                trg2 = 3;
            }
        }
        if (trg2 == 3)
        {
            c = GetChar("铁枪哨兵");
            if (c >= 0 && GetEnemy(c) != player)
            {
                Perform(c, "aggress");
                Perform(c, "say", "又被逃掉了，下次可不会再便宜你了！");
                trg2 = 4;
            }
        }

        if (trg3 == 0)
        {
            c = GetAnyChar("大刀哨兵");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "想不到你这小子还有两三下功夫﹒﹒﹒﹒﹒﹒﹒呜﹒﹒﹒");
                trg3 = 1;
            }
        }
        if (trg3 == 1)
        {
            c = GetChar("铁枪哨兵");
            if (c >= 0 && GetEnemy(c) == player)
            {
                ChangeBehavior(c, "kill", player);
                Perform(c, "say", "你这小子，杀了我的挚友！要你偿命！");
                Perform(c, "faceto", player);
                trg3 = 2;
                trg2 = 4;
            }
        }

        if (trg4 == 0)
        {
            c = GetAnyChar("铁枪哨兵");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "大胡子﹒﹒﹒帮﹒﹒﹒帮我报﹒﹒仇﹒﹒﹒");
                trg4 = 1;
            }
        }
        if (trg4 == 1)
        {
            c = GetChar("大刀哨兵");
            if (c >= 0 && GetEnemy(c) == player)
            {
                ChangeBehavior(c, "kill", player);
                Perform(c, "say", "你死定了！！你这小子！");
                Perform(c, "faceto", player);
                trg4 = 2;
                trg1 = 4;
            }
        }
        return 0;
    }

    public override void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
    }
}

/// <summary>
/// 飞鹏堡
/// </summary>
public class LevelScript_sn15 : LevelScriptBase
{
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override int GetPlayerMaxHp() { return PlayerHP; }
    public override GameResult OnUnitDead(MeteorUnit deadUnit)
    {
        if (deadUnit.Attr.NpcTemplate == "npc15_01")
            return GameResult.Win;
        return GameResult.None;
    }
    public override void OnLateStart()
    {
        U3D.ShowLeaderSfx();
    }
    int RoundTime = 25;
    int PlayerSpawn = 30;
    int PlayerSpawnDir = 80;
    int PlayerWeapon = 13;
    int PlayerWeapon2 = 10;
    int PlayerHP = 2500;

    int trg0 = 0;
    int trg1 = 0;
    int trg2 = 0;
    int trg3 = 0;
    int trg4 = 0;
    int trg5 = 0;
    int trg6 = 0;
    int trg7 = 0;
    int trg8 = 0;
    int trg9 = 0;
    int trg10 = 0;
    int timer0 = 0;
    int timer1 = 0;
    int timer2 = 0;
    int timer3 = 0;
    int gameover = 0;

    public override void OnStart()
    {
        AddNPC("npc15_01");
        AddNPC("npc15_02");
        AddNPC("npc15_03");
        AddNPC("npc15_04");
        AddNPC("npc15_05");
        AddNPC("npc15_06");
        AddNPC("npc15_07");
        base.OnStart();
    }

    public int Report(int c)
    {
        int c2 = GetChar("萧安");
        int c3;
        int c4;
        int player;

        if (c >= 0 && c2 >= 0)
        {
            SetTarget(0, "char", c);
            SetTarget(1, "char", c2);
            if (Distance(0, 1) < 100)
            {
                player = GetChar("player");
                c3 = GetChar("金枪侍卫﹒甲");
                ChangeBehavior(c3, "follow", player);
                c3 = GetChar("金枪侍卫﹒乙");
                ChangeBehavior(c3, "follow", player);
                c3 = GetChar("金枪侍卫﹒丙");
                ChangeBehavior(c3, "follow", player);
                c3 = GetChar("金枪侍卫﹒丁");
                ChangeBehavior(c3, "follow", player);

                Perform(c, "say", "是！！");
                Perform(c, "pause", 10);
                Perform(c, "say", "报﹒﹒报报告！有个刺客闯入﹒﹒");
                Perform(c, "faceto", c2);

                c3 = GetChar("左护法");
                if (c3 >= 0)
                {
                    ChangeBehavior(c3, "follow", c2);
                    Perform(c3, "say", "是！");
                    Perform(c3, "pause", 5);
                    Perform(c3, "say", "在！");
                    Perform(c3, "pause", 15);
                }
                c4 = GetChar("右护法");
                if (c4 >= 0)
                {
                    ChangeBehavior(c4, "follow", c2);
                    Perform(c4, "say", "是！");
                    Perform(c4, "pause", 5);
                    Perform(c4, "say", "在！");
                    Perform(c4, "pause", 15);
                }

                if (c3 >= 0 || c4 >= 0)
                {
                    Perform(c2, "say", "你们就给我守在这儿！看那小子能怎麽样！！");
                    Perform(c2, "pause", 5);
                    Perform(c2, "say", "左右护法！");
                }
                Perform(c2, "pause", 5);
                Perform(c2, "say", "你们去把他给捉来！");
                Perform(c2, "say", "刺客﹒﹒﹒哼﹒﹒果然不出我所料，人还是来了！");
                Perform(c2, "pause", 5);
                Perform(c2, "faceto", c);

                return 1;
            }
        }

        return 0;
    }

    public int Report2(int c)
    {
        int c2 = GetChar("萧安");
        int c3 = -1;
        //int c4 = -1;
        int player;
        if (c >= 0 && c2 >= 0)
        {
            SetTarget(0, "char", c);
            SetTarget(1, "char", c2);
            if (Distance(0, 1) < 100)
            {
                player = GetChar("player");
                c3 = GetChar("金枪侍卫﹒甲");
                ChangeBehavior(c3, "follow", c2);
                c3 = GetChar("金枪侍卫﹒乙");
                ChangeBehavior(c3, "follow", c2);
                c3 = GetChar("金枪侍卫﹒丙");
                ChangeBehavior(c3, "follow", c2);
                c3 = GetChar("金枪侍卫﹒丁");
                ChangeBehavior(c3, "follow", c2);
                c3 = GetChar("右护法");
                ChangeBehavior(c3, "follow", c2);
                c3 = GetChar("左护法");
                ChangeBehavior(c3, "follow", c2);

                Perform(c, "say", "是！！");
                Perform(c, "pause", 8);
                Perform(c, "say", "禀报萧老大！有外人侵入，想必是当时在炽雪城捣乱的家伙！");
                Perform(c, "faceto", c2);

                ChangeBehavior(c2, "follow", player);
                Perform(c2, "say", "呵﹒﹒﹒那小子是不要命了！所有人跟我走！！");
                Perform(c2, "pause", 5);
                Perform(c2, "faceto", c);
                return 1;
            }
        }

        return 0;
    }

    public int UnknownHelp(int c, int p)
    {
        if (c >= 0 && GetEnemy(c) == p && GetHP(p) < GetMaxHP(p) / 3)
        {
            AddNPC("npc15_08");
            return 1;
        }

        return 0;
    }

    public void StopChat()
    {
        int c;
        c = GetChar("金枪侍卫﹒甲");
        if (c >= 0)
        {
            StopPerform(c);
        }
        c = GetChar("金枪侍卫﹒乙");
        if (c >= 0)
        {
            StopPerform(c);
        }
        c = GetChar("player");
        if (c >= 0)
        {
            StopPerform(c);
        }
    }

    public override int OnUpdate()
    {
        int player = GetChar("player");
        if (player < 0)
        {
            return 0;
        }

        int c;
        int c2;
        int c3;
        int c4;
        int now = GetGameTime();

        if (trg0 == 0)
        {
            c = GetChar("金枪侍卫﹒甲");
            c2 = GetChar("金枪侍卫﹒乙");
            c3 = GetChar("金枪侍卫﹒丙");
            c4 = GetChar("金枪侍卫﹒丁");
            if (c >= 0 && c2 >= 0 && c3 >= 0 && c4 >= 0)
            {
                ChangeBehavior(c, "patrol", 0, 1, 2, 3, 4, 5);
                Perform(c, "say", "好呗～还是巡一下好了﹒﹒﹒");
                Perform(c, "pause", 8);
                Perform(c, "say", "不过，那姑娘的姿色很不错耶，要是能够﹒﹒嘻嘻嘻嘻﹒﹒");
                Perform(c, "pause", 8);
                Perform(c, "say", "我看我接下来的假期也没得休了﹒﹒");
                Perform(c, "pause", 8);
                Perform(c, "say", "听说炽雪城被一个不知名的人物一乱，现在搞的大家都紧张起来了！");
                Perform(c, "faceto", c2);

                ChangeBehavior(c2, "patrol", 0, 20, 24, 10, 11, 13, 17, 16);
                Perform(c2, "say", "嗯﹒﹒再聊～");
                Perform(c2, "pause", 4);
                Perform(c2, "say", "小心要是等会儿有人闯进来，假也别放了，被砍头还比较快！");
                Perform(c2, "say", "你是太久没放假解放一下了是吧﹒﹒");
                Perform(c2, "pause", 8);
                Perform(c2, "say", "唉，自从那个女飞贼捉回来之後，我们就得不眠不休地守卫﹒");
                Perform(c2, "pause", 8);
                Perform(c2, "say", "嘿呀﹒﹒害我的假休到一半又被招回来");
                Perform(c2, "pause", 4);
                Perform(c2, "faceto", c);

                Perform(c3, "pause", 10);
                Perform(c3, "faceto", c);
                Perform(c4, "pause", 15);
                Perform(c4, "faceto", c2);

                PlayerPerform("block", 0);
                PlayerPerform("say", "（﹒﹒﹒算了﹒﹒﹒不该理会这种事﹒﹒﹒﹒）");
                PlayerPerform("pause", 2);
                PlayerPerform("say", "（那女子﹒﹒难道她被禁闭在这个座城中？﹒﹒﹒这﹒﹒）");
                PlayerPerform("pause", 15);
                PlayerPerform("say", "（女飞贼！﹒﹒﹒那个女子'冷燕'？﹒﹒）");
                PlayerPerform("pause", 15);
                PlayerPerform("block", 1);

                trg0 = 1;
            }
        }

        if (trg0 == 1)
        {
            if (trg1 == 0)
            {
                c = GetChar("金枪侍卫﹒甲");
                if (c >= 0 && GetEnemy(c) == player)
                {
                    StopChat();
                    Perform(c, "guard", 3);
                    Perform(c, "say", "你是谁！胆敢闯进来！拿命来！");
                    Perform(c, "faceto", player);
                    trg1 = 1;
                }
            }
            if (trg2 == 0)
            {
                c = GetChar("金枪侍卫﹒乙");
                if (c >= 0 && GetEnemy(c) == player)
                {
                    StopChat();
                    Perform(c, "guard", 3);
                    Perform(c, "say", "好大的胆子，竟敢闯入！看我怎麽取你狗命！");
                    Perform(c, "faceto", player);
                    trg2 = 1;
                }
            }
            if (trg3 == 0)
            {
                c = GetChar("金枪侍卫﹒丙");
                if (c >= 0 && GetEnemy(c) == player)
                {
                    c2 = GetChar("金枪侍卫﹒丁");
                    if (c2 >= 0)
                    {
                        ChangeBehavior(c2, "follow", c);
                    }

                    Perform(c, "guard", 3);
                    Perform(c, "say", "快来呀！！有人闯入了！！");
                    Perform(c, "faceto", player);
                    trg3 = 1;
                    trg4 = 1;
                }
            }
            if (trg4 == 0)
            {
                c = GetChar("金枪侍卫﹒丁");
                if (c >= 0 && GetEnemy(c) == player)
                {
                    c2 = GetChar("金枪侍卫﹒丙");
                    if (c2 >= 0)
                    {
                        ChangeBehavior(c2, "follow", c);
                    }

                    Perform(c, "guard", 3);
                    Perform(c, "say", "快来呀！！有人闯入了！！");
                    Perform(c, "faceto", player);
                    trg4 = 1;
                    trg3 = 1;
                }
            }
        }

        if (trg1 == 1)
        {
            c = GetChar("金枪侍卫﹒甲");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                c2 = GetChar("金枪侍卫﹒丙");
                if (c2 >= 0)
                {
                    ChangeBehavior(c, "follow", c2);
                    Perform(c, "say", "有人闯入了！！快来逮住他！");
                    ChangeBehavior(c2, "follow", player);
                    trg1 = 2;
                    trg3 = 1;
                }
            }
        }
        if (trg2 == 1)
        {
            c = GetChar("金枪侍卫﹒乙");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                c2 = GetChar("金枪侍卫﹒丁");
                if (c2 >= 0)
                {
                    ChangeBehavior(c, "follow", c2);
                    Perform(c, "say", "有人闯入了！！快来逮住他！");
                    ChangeBehavior(c2, "follow", player);
                    trg2 = 2;
                    trg4 = 1;
                }
            }
        }

        if (trg3 == 1)
        {
            c = GetChar("金枪侍卫﹒丙");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                ChangeBehavior(c, "follow", "vip");
                trg3 = 2;
            }
        }
        if (trg4 == 1)
        {
            c = GetChar("金枪侍卫﹒丁");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                ChangeBehavior(c, "follow", "vip");
                trg4 = 2;
            }
        }

        if (trg3 == 2)
        {
            c = GetChar("金枪侍卫﹒丙");
            if (Report(c) == 1)
            {
                trg1 = 3;
                trg2 = 3;
                trg3 = 3;
                trg4 = 3;
                trg5 = 4;
                trg6 = 4;
            }
        }
        if (trg4 == 2)
        {
            c = GetChar("金枪侍卫﹒丁");
            if (Report(c) == 1)
            {
                trg1 = 3;
                trg2 = 3;
                trg3 = 3;
                trg4 = 3;
                trg5 = 4;
                trg6 = 4;
            }
        }

        if (trg5 == 0)
        {
            c = GetChar("左护法");
            if (c >= 0 && GetEnemy(c) == player)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", player);
                if (Distance(0, 1) < 200)
                {
                    Perform(c, "aggress");
                    Perform(c, "say", "哼！老子就先来陪你玩玩！");
                    Perform(c, "pause", 4);
                    Perform(c, "say", "呵呵呵呵！该不会就是你把炽雪城搞的天翻地覆吧！");
                    Perform(c, "faceto", player);
                    trg5 = 1;
                }
            }
        }
        if (trg5 == 1)
        {
            c = GetChar("左护法");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "say", "﹒﹒﹒你﹒﹒﹒你这小子真有两下子﹒﹒﹒");
                Perform(c, "faceto", player);
                trg5 = 2;
            }
        }
        if (trg5 == 2)
        {
            c = GetChar("左护法");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 4)
            {
                ChangeBehavior(c, "follow", "vip");
                Perform(c, "say", "﹒﹒识时务者为俊杰﹒溜～");
                Perform(c, "faceto", player);
                trg5 = 3;
            }
        }
        if (trg5 == 3)
        {
            c = GetChar("左护法");
            if (c >= 0 && Report2(c) == 1)
            {
                trg1 = 3;
                trg2 = 3;
                trg3 = 3;
                trg4 = 3;
                trg5 = 4;
                trg6 = 4;
            }
        }
        if (trg5 == 3 || trg5 == 4)
        {
            c = GetAnyChar("左护法");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "萧老大﹒﹒﹒小的先离开了﹒﹒﹒");
                trg5 = 5;
            }
        }

        if (trg6 == 0)
        {
            c = GetChar("右护法");
            if (c >= 0 && GetEnemy(c) == player)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", player);
                if (Distance(0, 1) < 200)
                {
                    Perform(c, "aggress");
                    Perform(c, "say", "就让我来试试你的身手吧！");
                    Perform(c, "pause", 4);
                    Perform(c, "say", "唷！你这小子还真有本事能够闯进来咧！");
                    Perform(c, "faceto", player);
                    trg6 = 1;
                }
            }
        }
        if (trg6 == 1)
        {
            c = GetChar("右护法");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "say", "可﹒﹒恶﹒﹒﹒你这小子惹脑了我了！！");
                Perform(c, "faceto", player);
                trg6 = 2;
            }
        }
        if (trg6 == 2)
        {
            c = GetChar("右护法");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 4)
            {
                ChangeBehavior(c, "follow", "vip");
                Perform(c, "say", "哼！老子就算斗不了你！我这就找人来治你！");
                Perform(c, "faceto", player);
                trg6 = 3;
            }
        }
        if (trg6 == 3)
        {
            c = GetChar("右护法");
            if (c >= 0 && Report2(c) == 1)
            {
                trg1 = 3;
                trg2 = 3;
                trg3 = 3;
                trg4 = 3;
                trg5 = 4;
                trg6 = 4;
            }
        }
        if (trg6 == 3 || trg6 == 4)
        {
            c = GetAnyChar("右护法");
            if (c >= 0 && GetHP(c) <= 0)
            {
                Say(c, "我﹒﹒我﹒﹒不行了﹒﹒﹒");
                trg6 = 5;
            }
        }

        if (trg7 == 0)
        {
            c = GetChar("萧安");
            SetTarget(0, "char", c);
            SetTarget(1, "char", player);
            if (c >= 0 && GetEnemy(c) == player && Distance(0, 1) < 200)
            {
                Perform(c, "guard", 3);
                Perform(c, "say", "那我就送你下地狱去跟﹒罗王问个究竟吧！！受死！！");
                Perform(c, "faceto", player);
                Perform(c, "say", "看你的样子，还不知情吧");
                Perform(c, "pause", 3);
                Perform(c, "say", "在我的看来，你也只是一只只会听命的狗而已！");
                Perform(c, "aggress");
                Perform(c, "say", "你这家伙真是可怜﹒怎麽死的都不知道！");
                Perform(c, "pause", 3);
                Perform(c, "say", "呵呵呵呵！等你许久了，总算让我见识到你了！");
                Perform(c, "faceto", player);


                c2 = GetChar("左护法");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", c);
                    Perform(c2, "guard", 10);
                    Perform(c2, "faceto", player);
                }
                c2 = GetChar("右护法");
                if (c2 >= 0)
                {
                    ChangeBehavior(c2, "follow", c);
                    Perform(c2, "guard", 10);
                    Perform(c2, "faceto", player);
                }

                StopChat();
                c2 = GetChar("金枪侍卫﹒甲");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 10);
                    Perform(c2, "faceto", player);
                }
                c2 = GetChar("金枪侍卫﹒乙");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 10);
                    Perform(c2, "faceto", player);
                }
                c2 = GetChar("金枪侍卫﹒丙");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 10);
                    Perform(c2, "faceto", player);
                }
                c2 = GetChar("金枪侍卫﹒丁");
                if (c2 >= 0)
                {
                    Perform(c2, "guard", 10);
                    Perform(c2, "faceto", player);
                }

                PlayerPerform("block", 0);
                PlayerPerform("pause", 15);
                PlayerPerform("block", 1);

                trg1 = 3;
                trg2 = 3;
                trg3 = 3;
                trg4 = 3;
                trg5 = 4;
                trg6 = 4;
                trg7 = 1;
            }

            if (trg7 == 0)
            {
                if (trg8 == 0)
                {
                    c2 = GetChar("左护法");
                    if (UnknownHelp(c2, player) == 1)
                    {
                        trg8 = 1;
                    }
                }
                if (trg8 == 0)
                {
                    c2 = GetChar("右护法");
                    if (UnknownHelp(c2, player) == 1)
                    {
                        trg8 = 1;
                    }
                }
            }
        }

        if (trg7 == 1)
        {
            c = GetChar("萧安");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 2)
            {
                Perform(c, "say", "不过想通过我这关，你还早的很！！");
                Perform(c, "guard", 5);
                Perform(c, "say", "哼！果然是找对人了！还真有两下子！");
                Perform(c, "faceto", player);

                PlayerPerform("say", "（﹒﹒找对人？？？）");
                PlayerPerform("pause", 3);

                trg7 = 2;
            }
        }
        if (trg7 == 2)
        {
            c = GetChar("萧安");
            if (c >= 0 && GetHP(c) < GetMaxHP(c) / 4)
            {
                ChangeBehavior(c, "wait");
                SetTarget(0, "waypoint", 79);
                ChangeBehavior(c, "attacktarget", 0);

                Perform(c, "say", "﹒﹒﹒你等着吧﹒﹒接下来就有你意想不到的好戏看了﹒﹒﹒");
                Perform(c, "say", "缠住他！我先想法子去！！");
                Perform(c, "say", "可恶！！你这小子！");
                Perform(c, "faceto", player);

                c2 = GetChar("右护法");
                ChangeBehavior(c2, "follow", player);
                c2 = GetChar("左护法");
                ChangeBehavior(c2, "follow", player);

                trg7 = 3;
                timer1 = now + 20;
            }
        }
        if (trg7 == 3 && now > timer1)
        {
            c = GetChar("萧安");
            if (c >= 0)
            {
                SetTarget(0, "waypoint", 107);
                ChangeBehavior(c, "attacktarget", 0);
                trg7 = 4;
                timer1 = now + 15;
            }
        }
        if (trg7 == 4 && now > timer1)
        {
            c = GetChar("萧安");
            if (c >= 0)
            {
                SetTarget(0, "waypoint", 115);
                ChangeBehavior(c, "attacktarget", 0);
                trg7 = 5;
                timer1 = now + 15;
            }
        }
        if (trg7 == 5 && now > timer1)
        {
            c = GetChar("萧安");
            if (c >= 0)
            {
                SetTarget(0, "waypoint", 53);
                ChangeBehavior(c, "attacktarget", 0);
                trg7 = 6;
                timer1 = now + 10;
            }
        }
        if (trg7 == 6 && now > timer1)
        {
            c = GetChar("萧安");
            if (c >= 0)
            {
                SetTarget(0, "waypoint", 116);
                ChangeBehavior(c, "attacktarget", 0);
                Perform(c, "aggress");
                Perform(c, "faceto", player);
                trg7 = 7;
            }
        }
        if (trg7 == 7)
        {
            c = GetChar("萧安");
            if (c >= 0)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "waypoint", 116);
                if (Distance(0, 1) < 80)
                {
                    RemoveNPC(c);
                    gameover = 1;
                    timer0 = now + 3;
                    trg7 = 8;
                }
            }
        }

        if (trg8 == 1)
        {
            c = GetChar("无名");
            if (c >= 0)
            {
                SetTarget(0, "char", c);
                SetTarget(1, "char", player);
                if (Distance(0, 1) < 150)
                {
                    PlayerPerform("say", "你是﹒﹒﹒");

                    c3 = -1;
                    c2 = GetChar("左护法");
                    if (c2 >= 0)
                    {
                        Perform(c2, "say", "你是什麽玩意儿！！找死！！");
                        Perform(c2, "pause", 6);
                        Perform(c2, "faceto", c);

                        c3 = c2;
                    }
                    c2 = GetChar("右护法");
                    if (c2 >= 0)
                    {
                        Perform(c2, "say", "你是什麽玩意儿！！找死！！");
                        Perform(c2, "pause", 6);
                        Perform(c2, "faceto", c);
                        c3 = c2;
                    }

                    if (c3 >= 0)
                    {
                        Perform(c, "aggress");
                        Perform(c, "say", "﹒﹒﹒﹒﹒");
                        Perform(c, "pause", 3);
                        Perform(c, "faceto", c3);
                    }
                    trg8 = 2;
                }
            }
        }

        if (trg8 == 2)
        {
            c = GetChar("无名");

            if (trg9 == 0)
            {
                c2 = GetAnyChar("左护法");
                if (c2 >= 0 && GetHP(c2) <= 0)
                {
                    Say(c2, "要不是你有帮手﹒﹒﹒﹒﹒");

                    Perform(c, "aggress");
                    Perform(c, "say", "嫩！");
                    Perform(c, "faceto", c2);
                    trg9 = 1;
                    timer2 = now + 5;
                }
            }
            if (trg10 == 0)
            {
                c2 = GetAnyChar("右护法");
                if (c2 >= 0 && GetHP(c2) <= 0)
                {
                    Say(c2, "可恶﹒﹒﹒竟然找帮手﹒﹒﹒");

                    Perform(c, "aggress");
                    Perform(c, "say", "嫩！");
                    Perform(c, "faceto", c2);
                    trg10 = 1;
                    timer2 = now + 5;
                }
            }
        }

        if (trg8 != 0)
        {
            c = GetChar("无名");
            if (c >= 0 && now > timer3)
            {
                Perform(c, "use", 4);
                timer3 = now + 180;
            }
        }

        if (trg9 == 1 && trg10 == 1 && now > timer2)
        {
            c = GetChar("无名");
            if (c >= 0)
            {
                SetTarget(0, "waypoint", 53);
                ChangeBehavior(c, "attacktarget", 0);

                Perform(c, "﹒﹒﹒无名﹒﹒﹒");
                Perform(c, "pause", 5);
                Perform(c, "﹒﹒﹒﹒﹒﹒");
                Perform(c, "pause", 5);
                Perform(c, "faceto", player);

                trg9 = 2;
            }
        }

        if (trg9 == 2)
        {
            c = GetChar("无名");
            if (c >= 0)
            {
                SetTarget(0, "waypoint", 53);
                SetTarget(1, "char", c);
                if (Distance(0, 1) < 80)
                {
                    RemoveNPC(c);
                    trg9 = 3;
                }
            }
        }


        if ((gameover == 1) && now > timer0)
        {
            GameOver(1);
            gameover = 2;
        }
        return 1;
    }

    public override void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
    }
}
//五雷塔
public class LevelScript_sn16 : LevelScriptBase
{
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
}

//伏虎山
public class LevelScript_sn17 : LevelScriptBase
{
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
}
//圆满楼
public class LevelScript_sn18 : LevelScriptBase
{
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
}

//洛阳城
public class LevelScript_sn19 : LevelScriptBase
{
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
}
//卧龙窟
public class LevelScript_sn20 : LevelScriptBase
{
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
}

//圣诞夜
public class LevelScript_sn21 : LevelScriptBase
{
    int RoundTime = 30;
    int PlayerSpawn = 5;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
        SetSceneItem("D_sn21st01", "name", "machine");
        SetSceneItem("D_sn21st01", "pose", 0, 0);
        SetSceneItem("D_sn21st01", "attribute", "collision", 0);
        SetSceneItem("D_sn21st01", "attribute", "interactive", 1);
        SetSceneItem("D_sn21st01", D_sn21st01_OnAttack, null);
        int i;
        string name = "";
        for (i = 5; i <= 103; i++)
        {
            MakeString(ref name, "D_Item", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }

        SetScene("snow", 1);
        SetScene("snowdensity", 1000);
        SetScene("winddir", 50, 0, 0);
        SetScene("snowspeed", 20, 100);
        SetScene("snowsize", 5, 5);

        for (i = 1; i <= 136; i++)
        {
            MakeString(ref name, "D_start", i);
            SetSceneItem(name, "name", "machine");
        }
    }

    //public override void Scene_OnClose()
    //{
    //    //SetScene("snow", 0);
    //}

    public int D_sn21st01_OnAttack(int id, int characterid, int damage)
    {
        int state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }
        Output("rotate.....");

        
        CreateEffect(id, "XStar");
        SetSceneItem(id, "pose", 1, 0);
        int i;
        string name = "";
        for (i = 5; i <= 103; i++)
        {
            MakeString(ref name, "D_Item", i);
            SetSceneItem(name, "attribute", "active", 1);
            SetSceneItem(name, "attribute", "interactive", 1);
        }
        
        return 1;
    }
    /*
	PScript file for box Items of all scenes
		by Peter Pon 2002/09/26

	message handle for 60 Boxes &
	message handle for 60 BBoxes
*/

    //int g_iNumBoxes = 65;
    int[] g_iBoxHP;
    int[] g_bBoxAlive;

    //int g_iNumBBoxes = 60;
    int[] g_iBBoxHP;
    int[] g_bBBoxAlive;

    public override void InitBoxes(int num)
    {
        int i;
        string name = "";
        g_bBoxAlive = new int[g_iNumBoxes];
        g_iBoxHP = new int[g_iNumBoxes];
        for (i = 1; i <= num; i++)
        {
            g_bBoxAlive[i - 1] = 1;
            g_iBoxHP[i - 1] = g_iBoxMaxHP;

            MakeString(ref name, "D_BBox", i);
            int id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);
            SetSceneItem(id, BoxOnAttack, BoxOnIdle);
            MakeString(ref name, "D_itBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);

            MakeString(ref name, "D_wpBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }
    }

    public int BoxOnAttack(int id, int index, int damage)
    {
        g_iBoxHP[index - 1] = g_iBoxHP[index - 1] - damage;
        if (g_iBoxHP[index - 1] > 0)
        {
            
            //Output("effect:", id, "BoxHit");
            SetSceneItem(id, "pose", 2, 0);
            CreateEffect(id, "BoxHIT");
            
            return 0;
        }

        string itemname = "";
        string weaponname = "";
        MakeString(ref itemname, "D_itBBox", index);
        MakeString(ref weaponname, "D_wpBBox", index);

        

        if (index < 5)
        {
            CreateEffect(id, "XSnowMan");
        }
        else
        {
            CreateEffect(id, "BoxBRK");
        }

        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(weaponname, "attribute", "interactive", 1);
        

        return 1;
    }

    public int RemoveBox(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0 || pose == 2)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        
        SetSceneItem(id, "attribute", "active", 0);
        
        Output("remove item", id);
        return 1;
    }

    public void BoxOnIdle(int id, int index)
    {
        if (g_bBoxAlive[index - 1] == 1)
        {
            if (RemoveBox(id) == 1)
            {
                g_bBoxAlive[index - 1] = 0;
            }
        }
    }

    public override void InitBBoxes(int num)
    {
        int i;
        string name = "";
        g_bBBoxAlive = new int[g_iNumBBoxes];
        g_iBBoxHP = new int[g_iNumBBoxes];
        for (i = 1; i <= num; i++)
        {
            g_bBBoxAlive[i - 1] = 1;
            g_iBBoxHP[i - 1] = g_iBBoxMaxHP;

            MakeString(ref name, "D_BBBox", i);
            int id = GetSceneItem(name, "index");
            SetSceneItem(id, "name", "machine");
            SetSceneItem(id, "attribute", "collision", 1);
            SetSceneItem(id, "pose", 0, 0);

            MakeString(ref name, "D_itBBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);

            MakeString(ref name, "D_wpBBBox", i);
            SetSceneItem(name, "attribute", "active", 0);
            SetSceneItem(name, "attribute", "interactive", 0);
        }
    }

    public override int BBoxOnAttack(int id, int index, int damage)
    {
        g_iBBoxHP[index - 1] = g_iBBoxHP[index - 1] - damage;
        if (g_iBBoxHP[index - 1] > 0)
        {
            
            CreateEffect(id, "BoxHIT");
            
            return 0;
        }

        string itemname = "";
        string weaponname = "";
        MakeString(ref itemname, "D_itBBBox", index);
        MakeString(ref weaponname, "D_wpBBBox", index);

        
        CreateEffect(id, "BoxBRK");
        SetSceneItem(id, "pose", 1, 0);
        SetSceneItem(id, "attribute", "interactive", 0);
        SetSceneItem(id, "attribute", "collision", 0);
        SetSceneItem(itemname, "attribute", "active", 1);
        SetSceneItem(itemname, "attribute", "interactive", 1);
        SetSceneItem(weaponname, "attribute", "active", 1);
        SetSceneItem(weaponname, "attribute", "interactive", 1);
        

        return 1;
    }

    public override int RemoveBBox(int id)
    {
        int state;
        int pose;

        pose = GetSceneItem(id, "pose");
        if (pose == 0)
        {
            return 0;
        }

        state = GetSceneItem(id, "state");
        if (state != 3)
        {
            return 0;
        }

        
        SetSceneItem(id, "attribute", "active", 0);
        
        Output("remove item", id);
        return 1;
    }

    public override void BBoxOnIdle(int id, int index)
    {
        if (g_bBBoxAlive[index - 1] == 1)
        {
            if (RemoveBBox(id) == 1)
            {
                g_bBBoxAlive[index - 1] = 0;
            }
        }
    }
}

//22-威震八方
public class LevelScript_sn22 : LevelScriptBase
{
    int RoundTime = 60;
    int PlayerSpawn = 15;
    int PlayerSpawnDir = 84;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    int PlayerModel = 10;
    int PlayerHP = 8000;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    int DeathMatch = 1;
    int TeamDeathMatch = 5;
    int GameMod;

    public int EventEnter = 200;//进入门
    public int EventExit = 201;//离开门
    public int EventDeath = 202;//死亡事件

    protected int[] g_CharacterArena = new int[100];//每个角色所在的区域编号,最多200号角色
    protected int[] g_ArenaCharacters = new int[16];//区域拥有的角色数量
    protected int[] g_TeamAArenaCharacters = new int[16];//区域0-15，所含A队人数
    protected int[] g_TeamBArenaCharacters = new int[16];//区域0-15，所含B队人数

    public override void Scene_OnInit()
    {
        InitBoxes(g_iNumBoxes);
        InitBBoxes(g_iNumBBoxes);
        InitChairs(g_iNumChairs);
        InitDeskes(g_iNumDeskes);
        InitJugs(g_iNumJugs);
        GameMod = GameCallBack("mod", 0);
        int i;
        string itemname = "";
        for (i = 0; i < 100; i++) {
            g_CharacterArena[i] = -1;
        }

        for (i = 0; i < 16; i = i + 1)
        {
            g_ArenaCharacters[i] = 0;
            g_TeamAArenaCharacters[i] = 0;
            g_TeamBArenaCharacters[i] = 0;

            MakeString(ref itemname, "D_tpEC", i + 1);
            SetSceneItem(itemname, "name", "machine");
            SetSceneItem(itemname, "attribute", "collision", 0);
            SetSceneItem(itemname, "attribute", "interactive", 1);

            MakeString(ref itemname, "D_tpEC", i + 17);
            SetSceneItem(itemname, "name", "machine");
            SetSceneItem(itemname, "attribute", "collision", 0);
            SetSceneItem(itemname, "attribute", "interactive", 1);
        }

        for (i = 0; i < 8; i = i + 1)
        {
            MakeString(ref itemname, "D_tpAC", i + 1);
            SetSceneItem(itemname, "name", "machine");
            SetSceneItem(itemname, "attribute", "collision", 0);

            MakeString(ref itemname, "D_tpAC", i + 9);
            SetSceneItem(itemname, "name", "machine");
            SetSceneItem(itemname, "attribute", "collision", 0);

            MakeString(ref itemname, "D_tpBC", i + 1);
            SetSceneItem(itemname, "name", "machine");
            SetSceneItem(itemname, "attribute", "collision", 0);

            MakeString(ref itemname, "D_tpBC", i + 9);
            SetSceneItem(itemname, "name", "machine");
            SetSceneItem(itemname, "attribute", "collision", 0);
        }
    }

    public override void Scene_OnCharacterEvent(int id, int evt)
    {
        int arena;
        int team;
        Output("Character:", id, "event:", evt);

	    if (evt == EventExit || evt== EventDeath)
        {
            if (GameMod == DeathMatch)//模式为盟主时
            {
                arena = g_CharacterArena[id];
                if (arena != -1)
                {
                    g_ArenaCharacters[arena] = g_ArenaCharacters[arena] - 1;//区域人数减少1人
                }
                g_CharacterArena[id] = -1;
            }
            if (GameMod == TeamDeathMatch)//死斗
            {
                arena = g_CharacterArena[id];
                team = GetTeam(id);
                if (arena != -1)
                {
                    if (team == 1)
                    {
                        g_TeamAArenaCharacters[arena] = g_TeamAArenaCharacters[arena] - 1;
                    }
                    if (team == 2)
                    {
                        g_TeamBArenaCharacters[arena] = g_TeamBArenaCharacters[arena] - 1;
                    }
                }
                g_CharacterArena[id] = -1;
            }
        }
    }

    public virtual void TransferToArena(int characterid, int tpid)
    {
        string arenaname = "";
        int arena = tpid / 2;
        Output("arena characters", g_ArenaCharacters[arena]);
        if (g_ArenaCharacters[arena] < 2 && g_CharacterArena[characterid] != arena)
        {
            g_ArenaCharacters[arena] = g_ArenaCharacters[arena] + 1;
            g_CharacterArena[characterid] = arena;
            MakeString(ref arenaname, "D_tpED", tpid + 1);
            Misc("transfer", characterid, arenaname);
            Output("transfer", characterid, "to", tpid);
        }
    }

    public virtual void TransferFromArena(int characterid, int tpid)
    {
        string arenaname = "";
        int arena = tpid / 2;
        if (g_ArenaCharacters[arena] == 1 && g_CharacterArena[characterid] == arena)
        {
            g_ArenaCharacters[arena] = g_ArenaCharacters[arena] - 1;
            g_CharacterArena[characterid] = -1;
            MakeString(ref arenaname, "D_tpED", tpid + 17);
            Misc("transfer", characterid, arenaname);
            Output("transfer", characterid, "from", tpid);
        }
    }

    public virtual void TeamATransferToArena(int characterid, int arena)
    {
        string arenaname = "";
        int team;
        team = GetTeam(characterid);
        if (team == 1 && g_TeamAArenaCharacters[arena] == 0)
        {
            g_TeamAArenaCharacters[arena] = 1;
            g_CharacterArena[characterid] = arena;
            MakeString(ref arenaname, "D_tpAD", arena + 1);
            Misc("transfer", characterid, arenaname);
            if (CombatData.Ins.GLevelMode == LevelMode.Teach)
                U3D.OnResumeAI();
            Output("transfer", characterid, "to", arena);
        }
    }

    public virtual void TeamATransferFromArena(int characterid, int arena)
    {
        string arenaname = "";
        int team;
        team = GetTeam(characterid);
        if (team == 1 && g_TeamAArenaCharacters[arena] == 1 && g_TeamBArenaCharacters[arena] == 0)
        {
            g_TeamAArenaCharacters[arena] = 0;
            g_CharacterArena[characterid] = -1;
            MakeString(ref arenaname, "D_tpAD", arena + 9);
            Misc("transfer", characterid, arenaname);
            if (CombatData.Ins.GLevelMode == LevelMode.Teach)
                U3D.OnPauseAI();
            Output("transfer", characterid, "from", arena);
        }
    }

    public virtual void TeamBTransferToArena(int characterid, int arena)
    {
        string arenaname = "";
        int team;
        team = GetTeam(characterid);
        if (team == 2 && g_TeamBArenaCharacters[arena] == 0)
        {
            g_TeamBArenaCharacters[arena] = 1;
            g_CharacterArena[characterid] = arena;
            MakeString(ref arenaname, "D_tpBD", arena + 1);
            Misc("transfer", characterid, arenaname);
            Output("transfer", characterid, "to", arena);
        }
    }

    public virtual void TeamBTransferFromArena(int characterid, int arena)
    {
        string arenaname = "";
        int team;
        team = GetTeam(characterid);
        if (team == 2 && g_TeamBArenaCharacters[arena] == 1 && g_TeamAArenaCharacters[arena] == 0)
        {
            g_TeamBArenaCharacters[arena] = 0;
            g_CharacterArena[characterid] = -1;
            MakeString(ref arenaname, "D_tpBD", arena + 9);
            Misc("transfer", characterid, arenaname);
            Output("transfer", characterid, "from", arena);
        }
    }

    void D_tpEC01_OnTouch(int id, int characterid)
    {
        Output("EC01 Touched");
        TransferToArena(characterid, 0);
    }

    void D_tpEC02_OnTouch(int id, int characterid)
    {
        Output("EC02 Touched");
        TransferToArena(characterid, 1);
    }

    void D_tpEC03_OnTouch(int id, int characterid)
    {
        Output("EC03 Touched");
        TransferToArena(characterid, 2);
    }

    void D_tpEC04_OnTouch(int id, int characterid)
    {
        Output("EC04 Touched");
        TransferToArena(characterid, 3);
    }

    void D_tpEC05_OnTouch(int id, int characterid)
    {
        Output("EC05 Touched");
        TransferToArena(characterid, 4);
    }

    void D_tpEC06_OnTouch(int id, int characterid)
    {
        Output("EC06 Touched");
        TransferToArena(characterid, 5);
    }

    void D_tpEC07_OnTouch(int id, int characterid)
    {
        Output("EC07 Touched");
        TransferToArena(characterid, 6);
    }

    void D_tpEC08_OnTouch(int id, int characterid)
    {
        Output("EC08 Touched");
        TransferToArena(characterid, 7);
    }

    void D_tpEC09_OnTouch(int id, int characterid)
    {
        Output("EC09 Touched");
        TransferToArena(characterid, 8);
    }

    void D_tpEC10_OnTouch(int id, int characterid)
    {
        Output("EC10 Touched");
        TransferToArena(characterid, 9);
    }

    void D_tpEC11_OnTouch(int id, int characterid)
    {
        Output("EC11 Touched");
        TransferToArena(characterid, 10);
    }

    void D_tpEC12_OnTouch(int id, int characterid)
    {
        Output("EC12 Touched");
        TransferToArena(characterid, 11);
    }

    void D_tpEC13_OnTouch(int id, int characterid)
    {
        Output("EC13 Touched");
        TransferToArena(characterid, 12);
    }

    void D_tpEC14_OnTouch(int id, int characterid)
    {
        Output("EC14 Touched");
        TransferToArena(characterid, 13);
    }

    void D_tpEC15_OnTouch(int id, int characterid)
    {
        Output("EC15 Touched");
        TransferToArena(characterid, 14);
    }

    void D_tpEC16_OnTouch(int id, int characterid)
    {
        Output("EC16 Touched");
        TransferToArena(characterid, 15);
    }

    void D_tpEC17_OnTouch(int id, int characterid)
    {
        Output("EC17 Touched");
        TransferFromArena(characterid, 0);
    }

    void D_tpEC18_OnTouch(int id, int characterid)
    {
        Output("EC18 Touched");
        TransferFromArena(characterid, 1);
    }

    void D_tpEC19_OnTouch(int id, int characterid)
    {
        Output("EC19 Touched");
        TransferFromArena(characterid, 2);
    }

    void D_tpEC20_OnTouch(int id, int characterid)
    {
        Output("EC20 Touched");
        TransferFromArena(characterid, 3);
    }

    void D_tpEC21_OnTouch(int id, int characterid)
    {
        Output("EC21 Touched");
        TransferFromArena(characterid, 4);
    }

    void D_tpEC22_OnTouch(int id, int characterid)
    {
        Output("EC22 Touched");
        TransferFromArena(characterid, 5);
    }

    void D_tpEC23_OnTouch(int id, int characterid)
    {
        Output("EC23 Touched");
        TransferFromArena(characterid, 6);
    }

    void D_tpEC24_OnTouch(int id, int characterid)
    {
        Output("EC24 Touched");
        TransferFromArena(characterid, 7);
    }

    void D_tpEC25_OnTouch(int id, int characterid)
    {
        Output("EC25 Touched");
        TransferFromArena(characterid, 8);
    }

    void D_tpEC26_OnTouch(int id, int characterid)
    {
        Output("EC26 Touched");
        TransferFromArena(characterid, 9);
    }

    void D_tpEC27_OnTouch(int id, int characterid)
    {
        Output("EC27 Touched");
        TransferFromArena(characterid, 10);
    }

    void D_tpEC28_OnTouch(int id, int characterid)
    {
        Output("EC28 Touched");
        TransferFromArena(characterid, 11);
    }

    void D_tpEC29_OnTouch(int id, int characterid)
    {
        Output("EC29 Touched");
        TransferFromArena(characterid, 12);
    }

    void D_tpEC30_OnTouch(int id, int characterid)
    {
        Output("EC30 Touched");
        TransferFromArena(characterid, 13);
    }

    void D_tpEC31_OnTouch(int id, int characterid)
    {
        Output("EC31 Touched");
        TransferFromArena(characterid, 14);
    }

    void D_tpEC32_OnTouch(int id, int characterid)
    {
        Output("EC32 Touched");
        TransferFromArena(characterid, 15);
    }

    void D_tpAC01_OnTouch(int id, int characterid)
    {
        Output("AC01 Touched");
        TeamATransferToArena(characterid, 0);
    }

    void D_tpAC02_OnTouch(int id, int characterid)
    {
        Output("AC02 Touched");
        TeamATransferToArena(characterid, 1);
    }

    void D_tpAC03_OnTouch(int id, int characterid)
    {
        Output("AC03 Touched");
        TeamATransferToArena(characterid, 2);
    }

    void D_tpAC04_OnTouch(int id, int characterid)
    {
        Output("AC04 Touched");
        TeamATransferToArena(characterid, 3);
    }

    void D_tpAC05_OnTouch(int id, int characterid)
    {
        Output("AC05 Touched");
        TeamATransferToArena(characterid, 4);
    }

    void D_tpAC06_OnTouch(int id, int characterid)
    {
        Output("AC06 Touched");
        TeamATransferToArena(characterid, 5);
    }

    void D_tpAC07_OnTouch(int id, int characterid)
    {
        Output("AC07 Touched");
        TeamATransferToArena(characterid, 6);
    }

    void D_tpAC08_OnTouch(int id, int characterid)
    {
        Output("AC08 Touched");
        TeamATransferToArena(characterid, 7);
    }

    void D_tpAC09_OnTouch(int id, int characterid)
    {
        Output("AC09 Touched");
        TeamATransferFromArena(characterid, 0);
    }

    void D_tpAC10_OnTouch(int id, int characterid)
    {
        Output("AC10 Touched");
        TeamATransferFromArena(characterid, 1);
    }

    void D_tpAC11_OnTouch(int id, int characterid)
    {
        Output("AC11 Touched");
        TeamATransferFromArena(characterid, 2);
    }

    void D_tpAC12_OnTouch(int id, int characterid)
    {
        Output("AC12 Touched");
        TeamATransferFromArena(characterid, 3);
    }

    void D_tpAC13_OnTouch(int id, int characterid)
    {
        Output("AC13 Touched");
        TeamATransferFromArena(characterid, 4);
    }

    void D_tpAC14_OnTouch(int id, int characterid)
    {
        Output("AC14 Touched");
        TeamATransferFromArena(characterid, 5);
    }

    void D_tpAC15_OnTouch(int id, int characterid)
    {
        Output("AC15 Touched");
        TeamATransferFromArena(characterid, 6);
    }

    void D_tpAC16_OnTouch(int id, int characterid)
    {
        Output("AC16 Touched");
        TeamATransferFromArena(characterid, 7);
    }

    void D_tpBC01_OnTouch(int id, int characterid)
    {
        Output("BC01 Touched by", characterid);
        TeamBTransferToArena(characterid, 0);
    }

    void D_tpBC02_OnTouch(int id, int characterid)
    {
        Output("BC02 Touched by", characterid);
        TeamBTransferToArena(characterid, 1);
    }

    void D_tpBC03_OnTouch(int id, int characterid)
    {
        Output("BC03 Touched by", characterid);
        TeamBTransferToArena(characterid, 2);
    }

    void D_tpBC04_OnTouch(int id, int characterid)
    {
        Output("BC04 Touched by", characterid);
        TeamBTransferToArena(characterid, 3);
    }

    void D_tpBC05_OnTouch(int id, int characterid)
    {
        Output("BC05 Touched by", characterid);
        TeamBTransferToArena(characterid, 4);
    }

    void D_tpBC06_OnTouch(int id, int characterid)
    {
        Output("BC06 Touched by", characterid);
        TeamBTransferToArena(characterid, 5);
    }

    void D_tpBC07_OnTouch(int id, int characterid)
    {
        Output("BC07 Touched by", characterid);
        TeamBTransferToArena(characterid, 6);
    }

    void D_tpBC08_OnTouch(int id, int characterid)
    {
        Output("BC08 Touched by", characterid);
        TeamBTransferToArena(characterid, 7);
    }

    void D_tpBC09_OnTouch(int id, int characterid)
    {
        Output("BC09 Touched by", characterid);
        TeamBTransferFromArena(characterid, 0);
    }

    void D_tpBC10_OnTouch(int id, int characterid)
    {
        Output("BC10 Touched by", characterid);
        TeamBTransferFromArena(characterid, 1);
    }

    void D_tpBC11_OnTouch(int id, int characterid)
    {
        Output("BC11 Touched by", characterid);
        TeamBTransferFromArena(characterid, 2);
    }

    void D_tpBC12_OnTouch(int id, int characterid)
    {
        Output("BC12 Touched by", characterid);
        TeamBTransferFromArena(characterid, 3);
    }

    void D_tpBC13_OnTouch(int id, int characterid)
    {
        Output("BC13 Touched by", characterid);
        TeamBTransferFromArena(characterid, 4);
    }

    void D_tpBC14_OnTouch(int id, int characterid)
    {
        Output("BC14 Touched by", characterid);
        TeamBTransferFromArena(characterid, 5);
    }

    void D_tpBC15_OnTouch(int id, int characterid)
    {
        Output("BC15 Touched by", characterid);
        TeamBTransferFromArena(characterid, 6);
    }

    void D_tpBC16_OnTouch(int id, int characterid)
    {
        Output("BC16 Touched by", characterid);
        TeamBTransferFromArena(characterid, 7);
    }
}

//星梦台
public class LevelScript_sn24 : LevelScriptBase
{
    int Rule = 5;
    int RoundTime = 15;
    int PlayerSpawn = 2;
    int PlayerSpawnDir = 90;
    int PlayerWeapon = 5;
    int PlayerWeapon2 = 0;
    public override int GetRoundTime() { return RoundTime; }
    public override int GetPlayerSpawn() { return PlayerSpawn; }
    public override int GetPlayerSpawnDir() { return PlayerSpawnDir; }
    public override int GetPlayerWeapon() { return PlayerWeapon; }
    public override int GetPlayerWeapon2() { return PlayerWeapon2; }
    public override void OnStart()
    {
        base.OnStart();
    }

    public override int OnUpdate()
    {
        return 0;
    }
}

