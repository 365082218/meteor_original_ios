using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using protocol;
using System;
using UnityEngine.SceneManagement;

//只能缓存一下房间战斗相关的数据,与联机战斗/房间相关的这里处理.
public class NetWorkBattle
{
    private float mLogicTempTime = 0;
    //在房间的玩家.
    Dictionary<int, MeteorUnit> player = new Dictionary<int, MeteorUnit>();
    public int RoomId = -1;//房间在服务器的编号
    public int LevelId = -1;//房间场景关卡编号
    public int PlayerId = -1;//主角在服务器的角色编号.
    public int heroIdx;//选择的模型编号.
    public int camp;//选择的阵营编号
    public int weaponIdx;
    string RoomName;
    int FrameIndex;
    int ServerFrameIndex;
    public int GameTime;//剩余总时间.

    void Start () {
        RoomId = -1;
        RoomName = "";
        TurnStarted = false;//在进入战场后，对客户端来说才需要
    }

    public MeteorUnit GetNetPlayer(int id)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].InstanceId == id)
                return Main.Ins.MeteorManager.UnitInfos[i];
        }
        return null;
    }

    public string GetNetPlayerName(int id)
    {
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Ins.MeteorManager.UnitInfos[i].InstanceId == id)
                return Main.Ins.MeteorManager.UnitInfos[i].name;
        }
        return "不明身份者";
    }
    uint frameIndex = 0;
    uint turn = 0;
    uint tick = 0;
    public int TurnFrame = 5;
    public bool TurnStarted = false;
    public bool waitReborn = false;
    public bool waitSend = true;
    //void Update () {
        //if (bSync && RoomId != -1 && TurnStarted && MeteorManager.Instance.LocalPlayer != null && !waitReborn)
        //{
        //    if (waitSend)
        //    {
        //        mLogicTempTime += Time.deltaTime;
        //        if (mLogicTempTime > 0.02f)
        //        {
        //            for (int i = 0; i < mFastForwardSpeed; i++)
        //            {
        //                GameTurn();
        //                mLogicTempTime = 0;
        //            }
        //        }

        //        frameIndex++;
        //        tick++;
        //        if (frameIndex % TurnFrame == 0)
        //        {
        //            turn++;
        //            waitSend = false;
        //            SyncInput();
        //            //SyncAttribute(frame.Players[0]);
        //            //Common.SyncFrame(frame);

        //            if (MeteorManager.Instance.LocalPlayer.Dead)
        //            {
        //                //Debug.LogError("waitreborn hp:" + frame.Players[0].hp);

        //                waitReborn = true;
        //            }
        //        }


        //        //36=3秒个turn内没收到服务器回复的同步信息，算作断开连接.
        //        if (tick >= 360)
        //        {
        //            bSync = false;
        //            ReconnectWnd.Instance.Open();
        //            if (GameBattleEx.Instance != null)
        //                GameBattleEx.Instance.NetPause();
        //        }
                
        //    }
        //    if (Global.useShadowInterpolate)
        //        SyncInterpolate();
        //}
	//}

    //public void SyncInterpolate()
    //{
    //    if (NetWorkBattle.Ins.TurnStarted && MeteorManager.Instance.LocalPlayer != null)
    //    {
    //        //在战场更新中,更新其他角色信息，自己的只上传.
    //        //Debug.Log("SyncInterpolate:" + Time.frameCount);
    //        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
    //        {
    //            MeteorUnit unit = MeteorManager.Instance.UnitInfos[i];
    //            if (unit != null && unit != MeteorManager.Instance.LocalPlayer)
    //            {
    //                //玩家同步所有属性
    //                if (unit.ShadowSynced)
    //                    continue;
    //                //float next = Mathf.Clamp01(unit.ShadowDelta + 0.5f);
    //                //Debug.Log("同步角色位置:" + Time.frameCount);
    //                unit.ShadowDelta += 5 * Time.deltaTime;
    //                unit.transform.position = Vector3.Lerp(unit.transform.position, unit.ShadowPosition, unit.ShadowDelta);
    //                unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, unit.ShadowRotation, unit.ShadowDelta);
    //                if (unit.ShadowDelta >= 1.0f)
    //                    unit.ShadowSynced = true;
    //            }
    //        }
    //    }
    //}

    //断开连接时.
    public void OnDisconnect()
    {
        if (RoomId != -1)
        {
            UdpClientProxy.Disconnect();
            //在联机战斗场景中.
            Main.Ins.GameBattleEx.Pause();
            Main.Ins.SoundManager.StopAll();
            Main.Ins.BuffMng.Clear();
            Main.Ins.MeteorManager.Clear();
            //if (FightWnd.Exist)
            //    FightWnd.Instance.Close();
            RoomId = -1;
            RoomName = "";
            FrameReplay.Instance.OnDisconnected();
            FrameIndex = ServerFrameIndex = 0;
            U3D.InsertSystemMsg("与服务器断开链接.");
            //if (!MainWnd.Exist)
            //    U3D.GoBack();            
        }
        RoomId = -1;
        waitReborn = false;
        RoomName = "";
    }

    //选择好了角色和武器，向服务器发出进入战场请求.
    public void EnterLevel()
    {
        //加载地图场景-开始和服务器同步历史帧信息.
        //LoadNetLevel();
        UdpClientProxy.EnterLevel(heroIdx, weaponIdx, camp);
    }

    //进入房间，还未进入战场，选阵营/人/武器界面
    public void OnEnterRoomSuccessed(int roomId, int levelid, int playerid)
    {
        RoomId = roomId;
        LevelId = levelid;
        PlayerId = playerid;
        camp = (int)EUnitCamp.EUC_KILLALL;
    }

    public void Load()
    {
        LevelDatas.LevelDatas lev = Main.Ins.CombatData.GetGlobalLevel(LevelId);
        Main.Ins.CombatData.Chapter = Main.Ins.DlcMng.FindChapter((LevelId / 1000) * 1000);
        Main.Ins.CombatData.GLevelItem = lev;
        Main.Ins.CombatData.GLevelMode = LevelMode.MultiplyPlayer;
        RoomInfo r = Main.Ins.RoomMng.GetRoom((int)RoomId);
        Main.Ins.RoomMng.Current = r;
        Main.Ins.CombatData.GGameMode = (GameMode)r.rule;
        Main.Ins.CombatData.wayPoints = null;
        //LoadingWnd.Instance.Open();
        U3D.LoadLevelEx();
    }
}
