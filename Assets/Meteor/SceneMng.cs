using System;
using System.Collections.Generic;
using SLua;
using UnityEngine;
using CoClass;
using System.IO;

class SceneMng
{
    static void BattleInit()
    {
        WsWindow.Close(WsWindow.Dialogue);//进战斗前必须把交互对话框关闭.
        WsWindow.Close(WsWindow.Battle);
        WsWindow.Open(WsWindow.Battle);
    }

    //指明进入一张地图,地图上所有的道具，建筑，陷阱，传送门，Npc,怪物,障碍物都需要保存下来，以便下次进入场景恢复
    public static void OnEnterLevel(int level)
    {
        Level lev = LevelMng.Instance.GetItem(level);
        //加载地图上无论如何都固有的环境,类似瀑布水流声，一些配置点，d_user d_team等
        //Loader load = GameObject.FindObjectOfType<Loader>();
        Loader.Instance.LoadFixedScene(lev);
        Loader.Instance.LoadDynamicTrigger(lev);//可破坏物件暂不处理.
    }

    //生成指定怪物
    public static MeteorUnit Spawn(string script)
    {
        MonsterEx mon = InitMon(script);
        GameObject objPrefab = Resources.Load("MeteorUnit") as GameObject;
        GameObject ins = GameObject.Instantiate(objPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        MeteorUnit unit = ins.GetComponent<MeteorUnit>();
        switch (mon.Team)
        {
            case 0:
                unit.Camp = EUnitCamp.EUC_KILLALL;//与所有人都是敌人
                break;
            case 1:
                unit.Camp = EUnitCamp.EUC_FRIEND;//流星阵营
                break;
            case 2:
                unit.Camp = EUnitCamp.EUC_ENEMY;//蝴蝶阵营
                break;
            default:
                unit.Camp = EUnitCamp.EUC_NONE;//与所有人都是朋友，所有人都无法打我
                break;
        }
        unit.Init(mon.Model, mon);
        MeteorManager.Instance.OnGenerateUnit(unit);
        LuaFunction onInit = ScriptMng.ins.GetFunc("OnInit");
        LuaFunction OnStart = ScriptMng.ins.GetFunc("OnStart");
        onInit.call(unit.InstanceId);
        unit.SetGround(false);
        if (Global.GLevelItem.wayPoint != null)
        {
            if (Global.GLevelItem.wayPoint.Count > mon.SpawnPoint)
                unit.transform.position = Global.GLevelItem.wayPoint[mon.SpawnPoint].pos;
            else if (Global.GLevelItem.wayPoint.Count > 0)
                unit.transform.position = Global.GLevelItem.wayPoint[0].pos;
        }
        unit.transform.rotation = new Quaternion(0, 0, 0, 1);
        OnStart.call();
        U3D.InsertSystemMsg(unit.name + " 加入游戏");
        return unit;
    }
    
    public static void OnLoad()
    {
        GameObject root = GameObject.Find("BattleRoot");
        if (root == null)
        {
            root = new GameObject("BattleRoot");
            root.transform.position = Vector3.zero;
            root.transform.rotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;
        }
        root.AddComponent<GameBattleEx>();
    }

    public static MonsterEx InitMon(string Script)
    {
        MonsterEx ret = new MonsterEx();
        if (ret.InitMonster(Script))
            return ret;
        return null;
    }

    public static MonsterEx InitPlayer(LevelScriptBase script)
    {
        MonsterEx ret = new MonsterEx();
        ret.InitPlayer(script);
        return ret;
    }
    //index指定多个对手的站位。NPC呼叫其他NPC帮忙时候，都是把战场已死NPC剔除，然后
    public static MonsterEx InitNpc(string Script)
    {
        return InitMon(Script);
    }
}

