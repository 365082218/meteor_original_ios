using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using protocol;
using System;
using UnityEngine.SceneManagement;
using Excel2Json;

//只能缓存一下房间战斗相关的数据,与联机战斗/房间相关的这里处理.
public class NetWorkBattle:Singleton<NetWorkBattle>
{
    private float mLogicTempTime = 0;
    //在房间的玩家.
    SortedDictionary<int, MeteorUnit> player = new SortedDictionary<int, MeteorUnit>();
    public int RoomId = -1;//房间在服务器的编号
    public int LevelId = -1;//房间场景关卡编号
    public int PlayerId = -1;//主角在服务器的角色编号.
    public int heroIdx;//选择的模型编号.
    public int camp;//选择的阵营编号
    public int weaponIdx;
    public int KcpPort;//KCP服务器端口
    string RoomName;
    int FrameIndex;
    int ServerFrameIndex;
    public int GameTime;//剩余总时间.

    public void SyncPlayer(FrameCommand fmd) {
        //不同步自己的信息，因为客户端是最新数据，服务器的是旧数据
        if (fmd.playerId == PlayerId)
            return;
        //同步角色的位置，旋转，动作，速度，武器，气血，模型，怒气，buff
        if (fmd.command == MeteorMsg.Command.ServerSync) {
            MeteorUnit unit = U3D.GetUnit((int)fmd.playerId);
            if (unit != null) {
                unit.synced = true;
                System.IO.MemoryStream ms = new System.IO.MemoryStream(fmd.Data);
                PlayerSync sync = ProtoBuf.Serializer.Deserialize<PlayerSync>(ms);
                unit.ShadowPosition = new Vector3(sync.position.x / 1000.0f, sync.position.y / 1000.0f, sync.position.z / 1000.0f);
                unit.ShadowRotation = new Quaternion(sync.rotation.x / 1000.0f, sync.rotation.y / 1000.0f, sync.rotation.z / 1000.0f, sync.rotation.w / 1000.0f);
                unit.ActionMgr.SetAction(sync.action);
                unit.angle = sync.ang;
                unit.Attr.hpCur = (int)sync.hp;
                if (unit.ModelId != sync.model) {
                    ModelItem m = null;
                    if (GameStateMgr.Ins.gameStatus.IsModelInstalled((int)sync.model, ref m)) {
                        U3D.ChangePlayerModel(unit, (int)sync.model);
                    } else {
                        //模型不一样，但是未安装此模型
                    }
                }
                unit.SyncWeapon((int)sync.weapon, (int)sync.weapon1);
                List<int> buffs = new List<int>();
                foreach (var each in BuffMng.Ins.BufDict) {
                    if (each.Value.Units.ContainsKey(unit)) {
                        buffs.Add(each.Key);
                    }
                }
                //把我不该有的buff删除
                for (int i = 0; i < buffs.Count; i++) {
                    if (!sync.buff.Contains((uint)buffs[i])){
                        BuffMng.Ins.BufDict[buffs[i]].ClearBuff(unit);
                    }
                }
                //把我没有的buff加上
                for (int i = 0; i < sync.buff.Count; i++) {
                    if (!buffs.Contains((int)sync.buff[i])) {
                        Option opt = MenuResLoader.Ins.GetItemInfo((int)sync.buff[i]);
                        if (opt != null && opt.IsItem())
                            unit.AddBuf(opt);
                    }
                }
            }
        }
    }

    //断开连接时.
    public void OnDisconnect()
    {
        if (RoomId != -1)
        {
            KcpClient.Ins.Disconnect();
            //在联机战斗场景中.
            Main.Ins.GameBattleEx.Pause();
            SoundManager.Ins.StopAll();
            RoomId = -1;
            RoomName = "";
            FrameReplay.Ins.OnDisconnected();
            FrameIndex = ServerFrameIndex = 0;
            U3D.InsertSystemMsg("与服务器断开链接.");
            if (!MainMenuState.Exist)
                U3D.GoBack();
        }
        RoomId = -1;
        RoomName = "";
    }

    //选择好了角色和武器，向服务器发出进入战场请求.
    public void EnterLevel()
    {
        //加载地图场景-开始和服务器同步历史帧信息.
        TcpClientProxy.Ins.EnterLevel(heroIdx, weaponIdx, camp);
    }

    //进入房间，还未进入战场，选阵营/人/武器界面
    public void OnEnterRoomSuccessed(int roomId, int levelid, int playerid, int kcpPort)
    {
        RoomId = roomId;
        LevelId = levelid;
        PlayerId = playerid;
        KcpPort = kcpPort;
        camp = (int)EUnitCamp.EUC_KILLALL;
    }

    public void Load()
    {
        LevelData lev = CombatData.Ins.GetLevel(0, LevelId);
        CombatData.Ins.Chapter = null;
        CombatData.Ins.GLevelItem = lev;
        CombatData.Ins.GLevelMode = LevelMode.MultiplyPlayer;
        RoomInfo r = RoomMng.Ins.GetRoom((int)RoomId);
        RoomMng.Ins.Current = r;
        CombatData.Ins.GGameMode = (GameMode)r.rule;
        CombatData.Ins.wayPoints = null;
        U3D.LoadLevelEx();
    }
}
