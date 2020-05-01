using SLua;
using UnityEngine;
using protocol;
using Idevgame.GameState;

public class SceneMng
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
        //Main.Instance.CombatData.wayPoints = 
        Main.Ins.CombatData.GLevelSpawn = new Vector3[16];
        Main.Ins.CombatData.GCampASpawn = new Vector3[8];
        Main.Ins.CombatData.GCampBSpawn = new Vector3[8];

        for (int i = 0; i < 16; i++)
        {
            GameObject obj = NodeHelper.Find(string.Format("D_user{0:d2}", i + 1), Loader.Instance.gameObject);
            Main.Ins.CombatData.GLevelSpawn[i] = obj == null ? Vector3.zero : obj.transform.position;
        }

        for (int i = 0; i < 8; i++)
        {
            GameObject objA = NodeHelper.Find(string.Format("D_teamA{0:d2}", i + 1), Loader.Instance.gameObject);
            Main.Ins.CombatData.GCampASpawn[i] = objA == null ? Vector3.zero :objA.transform.position;
            GameObject objB = NodeHelper.Find(string.Format("D_teamB{0:d2}", i + 1), Loader.Instance.gameObject);
            Main.Ins.CombatData.GCampBSpawn[i] = objB == null ? Vector3.zero : objB.transform.position;
        }

        if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            return;

        //GameObject objWayPoint = new GameObject("wayPoint");
        //objWayPoint.transform.position = Vector3.zero;
        //objWayPoint.transform.rotation = Quaternion.identity;
        //objWayPoint.transform.localScale = Vector3.one;
        //objWayPoint.layer = LayerMask.NameToLayer("WayPoint");
        //for (int i = 0; i < Main.Ins.CombatData.wayPoints.Count; i++)
        //{
        //    GameObject wayPoint = new GameObject(string.Format("WayPoint{0}", i));
        //    wayPoint.tag = "WayPoint";
        //    wayPoint.transform.SetParent(objWayPoint.transform);
        //    wayPoint.transform.position = Main.Ins.CombatData.wayPoints[i].pos;
        //    wayPoint.layer = objWayPoint.layer;
        //    wayPoint.transform.rotation = Quaternion.identity;
        //    wayPoint.transform.localScale = Vector3.one;
        //    BoxCollider box = wayPoint.AddComponent<BoxCollider>();
        //    box.isTrigger = true;
        //    box.size = Vector3.one * (Main.Ins.CombatData.wayPoints[i].size);
        //    box.center = Vector3.zero;
        //}
    }

    //指明进入一张地图,地图上所有的道具，建筑，陷阱，传送门，Npc,怪物,障碍物都需要保存下来，以便下次进入场景恢复
    public void OnEnterLevel()
    {
        string sceneItems = Main.Ins.CombatData.GLevelItem.sceneItems;
        string items = Main.Ins.CombatData.GScript.GetDesName();
        if (!string.IsNullOrEmpty(items))
            sceneItems = items;

        OnEnterLevel(Main.Ins.CombatData.GScript, sceneItems);
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
        UnitTopState unitTopState = new UnitTopState(unit);
        Main.Ins.EnterState(unitTopState);
        Main.Ins.MeteorManager.OnGenerateUnit(unit);
        LuaFunction onInit = mon.sState["OnInit"] as LuaFunction;
        onInit.call(mon.sState, unit.InstanceId);
        unit.SetGround(false);
        if (Main.Ins.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (Main.Ins.CombatData.wayPoints.Count != 0)
                unit.transform.position = Main.Ins.CombatData.wayPoints.Count > mon.SpawnPoint ? Main.Ins.CombatData.wayPoints[mon.SpawnPoint].pos : Main.Ins.CombatData.wayPoints[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
        }
        else if (Main.Ins.CombatData.GLevelMode == LevelMode.CreateWorld)
        {
            if (Main.Ins.CombatData.GGameMode == GameMode.Normal)
            {
                unit.transform.position = Main.Ins.CombatData.wayPoints.Count > mon.SpawnPoint ? Main.Ins.CombatData.wayPoints[mon.SpawnPoint].pos : Main.Ins.CombatData.wayPoints[0].pos;//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            }
            else if (Main.Ins.CombatData.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.transform.position = Main.Ins.CombatData.GLevelSpawn[Main.Ins.CombatData.SpawnIndex];
                Main.Ins.CombatData.SpawnIndex++;
                Main.Ins.CombatData.SpawnIndex %= 16;
            }
            else if (Main.Ins.CombatData.GGameMode == GameMode.ANSHA || Main.Ins.CombatData.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.transform.position = Main.Ins.CombatData.GCampASpawn[Main.Ins.CombatData.CampASpawnIndex];
                    Main.Ins.CombatData.CampASpawnIndex++;
                    Main.Ins.CombatData.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.transform.position = Main.Ins.CombatData.GCampASpawn[Main.Ins.CombatData.CampBSpawnIndex];
                    Main.Ins.CombatData.CampBSpawnIndex++;
                    Main.Ins.CombatData.CampBSpawnIndex %= 8;
                }
            }
        }
        
        unit.transform.rotation = new Quaternion(0, 0, 0, 1);
        U3D.InsertSystemMsg(U3D.GetCampEnterLevelStr(unit));
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

    public MonsterEx InitNetPlayer(PlayerEventData player)
    {
        MonsterEx ret = new MonsterEx();
        ret.HpMax = (int)Main.Ins.RoomMng.GetRoom(Main.Ins.NetWorkBattle.RoomId).hpMax;
        ret.hpCur = ret.HpMax;
        ret.AngryValue = 0;
        ret.Model = (int)player.model;
        ret.Weapon = (int)player.weapon;
        ret.Weapon2 = (int)0;
        ret.name = player.name;

        ret.SpawnPoint = U3D.Rand(16);
        ret.Speed = 1000;
        ret.IsPlayer = player.playerId == Main.Ins.NetWorkBattle.PlayerId;
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

