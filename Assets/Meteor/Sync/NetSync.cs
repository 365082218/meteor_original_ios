using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public interface INetUpdate
{
    void GameFrameTurn(int gameFramesPerSecond, List<FrameAction> actions);
    bool Finished { get; }
}

public class NetSync : MonoBehaviour {
    //角色更新顺序是由playerId由小到大跑.
    //场景物件顺序由物件ID由小到大跑.
    private Stopwatch gameTurnSW;
    List<TurnFrames> actionsToSend;//向服务器发送的一个turn内的指令序列
    List<TurnFrames> confirmedActions;//接受到服务器发送来的一个turn内所有玩家的指令序列.
    private int LogicFrameIndex = 0; //Current Game Frame number in the currect lockstep turn
    private int AccumilatedTime = 0; //the accumilated time in Milliseconds that have passed since the last time GameFrame was called
    private int LogicFrameLength = 50;
    public int LockStepTurnID = 0;
    TurnFrames nowTurn;//当前的Turn
    /// <summary>
    /// 包括所有动态物体
    /// 所有需要使用网络时间驱动的游戏对象.需要实现接口IHasGameFrame，由该组件按网络时间，顺序执行每个对象的更新.
    /// </summary>
    public List<INetUpdate> GameObjects = new List<INetUpdate>();
    public void Awake()
    {
        Log.WriteError("FrameSync start");//从这里开始，播放逻辑帧，在取得自己进入场景消息帧时，初始化主角
        LockStepTurnID = 0;
        confirmedActions = new List<TurnFrames>();
        actionsToSend = new List<TurnFrames>();
        gameTurnSW = new Stopwatch();
    }

    //called once per unity frame
    public void Update()
    {
        //Basically same logic as FixedUpdate, but we can scale it by adjusting FrameLength
        AccumilatedTime = AccumilatedTime + Convert.ToInt32((Time.deltaTime * 1000)); //convert sec to milliseconds

        //in case the FPS is too slow, we may need to update the game multiple times a frame
        while (AccumilatedTime > LogicFrameLength)
        {
            LogicFrame();
            AccumilatedTime = AccumilatedTime - LogicFrameLength;
        }
    }

    //取得对应逻辑帧的数据
    List<FrameAction> cacheActions = new List<FrameAction>();
    List<FrameAction> GetAction(List<FrameAction> acts, int logicF)
    {
        cacheActions.Clear();
        for (int i = 0; i < acts.Count; i++)
        {
            if (acts[i].GameLogicFrame == logicF)
                cacheActions.Add(acts[i]);
        }
        if (cacheActions.Count == 0)
            return null;
        return cacheActions;
    }

    private void LogicFrame()
    {
        //得到当前逻辑帧数据，对普通事件数据，调用对应的事件函数，对按键，在更新每个对象使，应用到每个对象上.
        if (nowTurn == null)
        {
            //等待从服务器收到接下来一个turn的信息.
            if (confirmedActions.Count == 0)
                return;
            nowTurn = confirmedActions[0];
            confirmedActions.RemoveAt(0);
        }
        else
        {

        }
        List<FrameAction> actions = GetAction(nowTurn.actions, LogicFrameIndex);
        gameTurnSW.Start();

        //update game
        //SceneManager.Manager.TwoDPhysics.Update (GameFramesPerSecond);

        Log.WriteError(string.Format("Turn:{0}, LogicFrame:{1}", nowTurn.turnIndex, LogicFrameIndex));//从这里开始，播放逻辑帧，在取得自己进入场景消息帧时，初始化主角
        for (int i = 0; i < actions.Count; i++)
        {
            //初始化随机种子.
            if (actions[i].action == MeteorMsg.MsgType.SyncStart)
            {
                
            }
        }

        List<INetUpdate> finished = new List<INetUpdate>();
        foreach (INetUpdate obj in GameObjects)
        {
            obj.GameFrameTurn(LogicFrameLength, actions);
            if (obj.Finished)
            {
                finished.Add(obj);
            }
        }

        foreach (INetUpdate obj in finished)
        {
            GameObjects.Remove(obj);
        }

        LogicFrameIndex++;
        //当逻辑帧为 Turn序号
        if (LogicFrameIndex == nowTurn.turnIndex * 4)
            nowTurn = null;

        gameTurnSW.Stop();
        gameTurnSW.Reset();
    }

    public void NetUpdate()
    {
        //if (TurnIndex != Frame.turnIndex)
        //    return;
        //if (FrameIndex >= Frame.Inputs[0].frames.Count)
        //    return;
        
        //for (int i = 0; i < Frame.Inputs.Count; i++)
        //    OnNetInput(Frame.Inputs[i].playerId, Frame.Inputs[i].frames[FrameIndex]);
        //FrameIndex++;
        GameBattleEx.Instance.NetUpdate();
    }

    //void OnNetInput(uint player, InputFrame fInput)
    //{
    //    MeteorUnit unit = NetWorkBattle.Ins.GetNetPlayer((int)player);
    //    unit.OnNetInput(fInput);
    //}
}
