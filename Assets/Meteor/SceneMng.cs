using SLua;
using UnityEngine;
using protocol;
using Idevgame.GameState;
using Idevgame.GameState.DialogState;

public class SceneMng:Singleton<SceneMng>
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
        CombatData.Ins.GLevelSpawn = new Vector3[16];
        CombatData.Ins.GCampASpawn = new Vector3[8];
        CombatData.Ins.GCampBSpawn = new Vector3[8];

        for (int i = 0; i < 16; i++)
        {
            GameObject obj = NodeHelper.Find(string.Format("D_user{0:d2}", i + 1), Loader.Instance.gameObject);
            CombatData.Ins.GLevelSpawn[i] = obj == null ? Vector3.zero : obj.transform.position;
        }

        for (int i = 0; i < 8; i++)
        {
            GameObject objA = NodeHelper.Find(string.Format("D_teamA{0:d2}", i + 1), Loader.Instance.gameObject);
            CombatData.Ins.GCampASpawn[i] = objA == null ? Vector3.zero :objA.transform.position;
            GameObject objB = NodeHelper.Find(string.Format("D_teamB{0:d2}", i + 1), Loader.Instance.gameObject);
            CombatData.Ins.GCampBSpawn[i] = objB == null ? Vector3.zero : objB.transform.position;
        }
    }

    //重新加载所有道具等
    public void Reset() {
        string sceneItems = CombatData.Ins.GLevelItem.sceneItems;
        if (Loader.Instance != null) {
            Loader.Instance.LoadDynamicTrigger(sceneItems);
        }
    }
    //指明进入一张地图,地图上所有的道具，建筑，陷阱，传送门，Npc,怪物,障碍物都需要保存下来，以便下次进入场景恢复
    public void OnEnterLevel()
    {
        string sceneItems = CombatData.Ins.GLevelItem.sceneItems;
        OnEnterLevel(CombatData.Ins.GScript, sceneItems);
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
        PersistDialogMgr.Ins.EnterState(unitTopState);
        MeteorManager.Ins.OnGenerateUnit(unit);
        LuaFunction onInit = mon.sState["OnInit"] as LuaFunction;
        onInit.call(mon.sState, unit.InstanceId);
        unit.SetGround(false);
        if (CombatData.Ins.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (CombatData.Ins.wayPoints.Count != 0)
                unit.SetPosition(CombatData.Ins.wayPoints.Count > mon.SpawnPoint ? CombatData.Ins.wayPoints[mon.SpawnPoint].pos : CombatData.Ins.wayPoints[0].pos);//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            else
                unit.SetPosition(CombatData.Ins.GLevelSpawn[mon.SpawnPoint]);
        }
        else if (CombatData.Ins.GLevelMode == LevelMode.CreateWorld)
        {
            if (CombatData.Ins.GGameMode == GameMode.Normal)
            {
                unit.SetPosition(CombatData.Ins.wayPoints.Count > mon.SpawnPoint ? CombatData.Ins.wayPoints[mon.SpawnPoint].pos : CombatData.Ins.wayPoints[0].pos);//等关卡脚本实现之后在设置单机出生点.PlayerEx.Instance.SpawnPoint
            }
            else if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
            {
                //16个点
                unit.SetPosition(CombatData.Ins.GLevelSpawn[CombatData.Ins.SpawnIndex]);
                CombatData.Ins.SpawnIndex++;
                CombatData.Ins.SpawnIndex %= 16;
            }
            else if (CombatData.Ins.GGameMode == GameMode.ANSHA || CombatData.Ins.GGameMode == GameMode.SIDOU)
            {
                //2个队伍8个点.
                if (unit.Camp == EUnitCamp.EUC_FRIEND)
                {
                    unit.SetPosition(CombatData.Ins.GCampASpawn[CombatData.Ins.CampASpawnIndex]);
                    CombatData.Ins.CampASpawnIndex++;
                    CombatData.Ins.CampASpawnIndex %= 8;
                }
                else if (unit.Camp == EUnitCamp.EUC_ENEMY)
                {
                    unit.SetPosition(CombatData.Ins.GCampASpawn[CombatData.Ins.CampBSpawnIndex]);
                    CombatData.Ins.CampBSpawnIndex++;
                    CombatData.Ins.CampBSpawnIndex %= 8;
                }
            }
        }

        unit.SetRotationImmediate(new Quaternion(0, 0, 0, 1));
        U3D.InsertSystemMsg(U3D.GetCampEnterLevelStr(unit.Camp, unit.name));
        if (FrameReplay.Ins.Started) {
            mon.OnStart();
            //Debug.Log("关卡剧情加入NPC自己初始化");
        } else {
            //Debug.Log("关卡默认NPC统一初始化");
        }

        return unit;
    }

    public MonsterEx InitMon(string Script)
    {
        MonsterEx ret = new MonsterEx();
        if (ret.InitMonster(Script))
            return ret;
        return null;
    }

    public MonsterEx InitNetPlayer(PlayerSync player) {
        MonsterEx ret = new MonsterEx();
        RoomInfo room = RoomMng.Ins.GetRoom(NetWorkBattle.Ins.RoomId);
        if (room == null) {
            Debug.LogError("创建联机玩家-找不到房间信息 房间号:" + NetWorkBattle.Ins.RoomId);
        }
        ret.HpMax = room == null ? 200 : (int)room.hpMax * 10;
        ret.hpCur = (int)player.hp;
        ret.AngryValue = (int)player.ang;
        ret.Model = (int)player.model;
        ret.Weapon = (int)player.weapon;
        ret.Weapon2 = (int)0;
        ret.name = player.name;

        ret.SpawnPoint = (int)player.spwanIndex;
        ret.Speed = 1000;
        ret.IsPlayer = false;
        return ret;
    }

    public MonsterEx InitNetPlayer(PlayerEvent player)
    {
        MonsterEx ret = new MonsterEx();
        ret.HpMax = (int)RoomMng.Ins.GetRoom(NetWorkBattle.Ins.RoomId).hpMax * 10;
        ret.hpCur = ret.HpMax;
        ret.AngryValue = 0;
        ret.Model = (int)player.model;
        ret.Weapon = (int)player.weapon;
        ret.Weapon2 = (int)0;
        ret.name = player.name;

        ret.SpawnPoint = (int)player.spawnIndex;
        ret.Speed = 1000;
        ret.IsPlayer = player.playerId == NetWorkBattle.Ins.PlayerId;
        return ret;
    }

    public MonsterEx InitPlayer(LevelScriptBase script)
    {
        MonsterEx ret = new MonsterEx(100, true);
        ret.InitPlayer(script);
        return ret;
    }
    //index指定多个对手的站位。NPC呼叫其他NPC帮忙时候，都是把战场已死NPC剔除，然后
    public MonsterEx InitNpc(string Script)
    {
        return InitMon(Script);
    }
}

