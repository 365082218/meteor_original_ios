using System;
using System.Collections.Generic;
using SLua;
using UnityEngine;

using System.IO;
using protocol;

class SceneMng:Singleton<SceneMng>
{
    public void OnEnterLevel(LevelScriptBase levelScript, string sceneItems)
    {
        if (levelScript == null)
            return;
        if (Loader.Instance != null)
        {
            Loader.Instance.LoadFixedScene(sceneItems);
            Loader.Instance.LoadDynamicTrigger(sceneItems);
        }
        else
        {
            Debug.LogError("Loader not exist");
        }

        Global.Instance.GLevelSpawn = new Vector3[16];
        Global.Instance.GCampASpawn = new Vector3[8];
        Global.Instance.GCampBSpawn = new Vector3[8];

        if (WayMng.Instance == null)
        {
            for (int i = 0; i < 16; i++)
            {
                Global.Instance.GLevelSpawn[i] = Global.ldaControlX(string.Format("D_user{0:d2}", i + 1), Loader.Instance.gameObject).transform.position;
            }

            for (int i = 0; i < 8; i++)
            {
                Global.Instance.GCampASpawn[i] = Global.ldaControlX(string.Format("D_teamA{0:d2}", i + 1), Loader.Instance.gameObject).transform.position;
                Global.Instance.GCampBSpawn[i] = Global.ldaControlX(string.Format("D_teamB{0:d2}", i + 1), Loader.Instance.gameObject).transform.position;
            }
        }
        else
        {
            for (int i = 0; i < 16; i++)
            {
                Global.Instance.GLevelSpawn[i] = WayMng.Instance.wayPoints[i >= WayMng.Instance.wayPoints.Count ? 0 : i].pos;
            }

            for (int i = 0; i < 8; i++)
            {
                Global.Instance.GCampASpawn[i] = WayMng.Instance.wayPoints[i >= WayMng.Instance.wayPoints.Count ? 0 : i].pos;
                Global.Instance.GCampBSpawn[i] = WayMng.Instance.wayPoints[i >= WayMng.Instance.wayPoints.Count ? 0 : i].pos;
            }
        }

        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            return;

        GameObject objWayPoint = new GameObject("wayPoint");
        objWayPoint.transform.position = Vector3.zero;
        objWayPoint.transform.rotation = Quaternion.identity;
        objWayPoint.transform.localScale = Vector3.one;
        objWayPoint.layer = LayerMask.NameToLayer("WayPoint");
        for (int i = 0; i < Global.Instance.GLevelItem.wayPoint.Count; i++)
        {
            GameObject wayPoint = new GameObject(string.Format("WayPoint{0}", i));
            wayPoint.tag = "WayPoint";
            wayPoint.transform.SetParent(objWayPoint.transform);
            wayPoint.transform.position = Global.Instance.GLevelItem.wayPoint[i].pos;
            wayPoint.layer = objWayPoint.layer;
            wayPoint.transform.rotation = Quaternion.identity;
            wayPoint.transform.localScale = Vector3.one;
            BoxCollider box = wayPoint.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = Vector3.one * (Global.Instance.GLevelItem.wayPoint[i].size);
            box.center = Vector3.zero;
            WayPointTrigger trigger = wayPoint.AddComponent<WayPointTrigger>();
            trigger.WayIndex = i;
        }
    }

    //指明进入一张地图,地图上所有的道具，建筑，陷阱，传送门，Npc,怪物,障碍物都需要保存下来，以便下次进入场景恢复
    public void OnEnterLevel()
    {
        string sceneItems = Global.Instance.GLevelItem.sceneItems;
        string items = Global.Instance.GScript.GetDesName();
        if (!string.IsNullOrEmpty(items))
            sceneItems = items;

        OnEnterLevel(Global.Instance.GScript, sceneItems);
    }

    //生成指定怪物,这个是从脚本入口来的，是正式关卡中生成NPC的
    public MeteorUnit Spawn(string script)
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
        LuaFunction onInit = mon.sState["OnInit"] as LuaFunction;
        onInit.call(mon.sState, unit.InstanceId);
        unit.SetGround(false);
        if (Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (Global.Instance.GLevelItem.DisableFindWay == 1)
            {
                //不许寻路，无寻路点的关卡，使用
                unit.transform.position = Global.Instance.GLevelSpawn[mon.SpawnPoint >= Global.Instance.GLevelSpawn.Length ? 0 : mon.SpawnPoint];
            }
            else
            {
                unit.transform.position = Global.Instance.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.Instance.GLevelItem.wayPoint[mon.SpawnPoint].pos : Global.Instance.GLevelItem.wayPoint[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            }
        }
        else if (Global.Instance.GLevelMode == LevelMode.CreateWorld)
        {
            if (Global.Instance.GGameMode == GameMode.Normal)
            {
                if (Global.Instance.GLevelItem.DisableFindWay == 1)
                {
                    //不许寻路，无寻路点的关卡，使用
                    unit.transform.position = Global.Instance.GLevelSpawn[mon.SpawnPoint >= Global.Instance.GLevelSpawn.Length ? 0 : mon.SpawnPoint];
                }
                else
                {
                    unit.transform.position = Global.Instance.GLevelItem.wayPoint.Count > mon.SpawnPoint ? Global.Instance.GLevelItem.wayPoint[mon.SpawnPoint].pos : Global.Instance.GLevelItem.wayPoint[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
                }
            }
            else if (Global.Instance.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Global.Instance.GLevelSpawn[Global.Instance.SpawnIndex];
                Global.Instance.SpawnIndex++;
                Global.Instance.SpawnIndex %= 16;
            }
            else if (Global.Instance.GGameMode == GameMode.ANSHA || Global.Instance.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Global.Instance.GCampASpawn[Global.Instance.CampASpawnIndex];
                    Global.Instance.CampASpawnIndex++;
                    Global.Instance.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Global.Instance.GCampASpawn[Global.Instance.CampBSpawnIndex];
                    Global.Instance.CampBSpawnIndex++;
                    Global.Instance.CampBSpawnIndex %= 8;
                }
            }
        }
        
        unit.transform.rotation = new Quaternion(0, 0, 0, 1);
        //OnStart.call();
        U3D.InsertSystemMsg(unit.name + " 加入游戏");
        mon.OnStart();
        return unit;
    }

    public MonsterEx InitMon(string Script)
    {
        MonsterEx ret = new MonsterEx();
        if (ret.InitMonster(Script))
            return ret;
        return null;
    }

    public MonsterEx InitNetPlayer(Player_ player)
    {
        MonsterEx ret = new MonsterEx();
        ret.hpCur = player.hp;
        ret.HpMax = (int)RoomMng.Instance.GetRoom(NetWorkBattle.Instance.RoomId).hpMax;
        ret.AngryValue = 0;
        ret.Model = player.model;
        ret.Weapon = (int)player.weapon;
        ret.Weapon2 = (int)0;
        ret.name = player.name;
        ret.SpawnPoint = player.spawnpoint;
        ret.Speed = 1000;
        ret.IsPlayer = player.id == NetWorkBattle.Instance.PlayerId;
        return ret;
    }

    public MonsterEx InitPlayer(LevelScriptBase script)
    {
        MonsterEx ret = new MonsterEx();
        ret.InitPlayer(script);
        return ret;
    }
    //index指定多个对手的站位。NPC呼叫其他NPC帮忙时候，都是把战场已死NPC剔除，然后
    public MonsterEx InitNpc(string Script)
    {
        return InitMon(Script);
    }
}

