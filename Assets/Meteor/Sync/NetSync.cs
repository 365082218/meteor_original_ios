using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public interface INetUpdate
{
    void GameFrameTurn(int gameFramesPerSecond);
    bool Finished { get; }
}

public class NetSync : MonoBehaviour {
    //角色更新顺序是由playerId由小到大跑.
    //场景物件顺序由物件ID由小到大跑.
    private Stopwatch gameTurnSW;
    List<TurnFrames> actionsToSend;//向服务器发送的一个turn内的指令序列
    List<TurnFrames> confirmedActions;//接受到服务器发送来的一个turn内所有玩家的指令序列.
    private int GameFrame = 0; //Current Game Frame number in the currect lockstep turn
    private int AccumilatedTime = 0; //the accumilated time in Milliseconds that have passed since the last time GameFrame was called
    private int initialLockStepTurnLength = 200; //每秒5个同步帧-一个同步帧拥有4个逻辑帧，包含4帧所有玩家的指令序列
    private int initialGameFrameTurnLength = 50; //每秒20次逻辑帧
    private int LockstepTurnLength;
    private int GameFrameTurnLength;
    private int GameFramesPerLockstepTurn;
    private int LockstepsPerSecond;
    private int GameFramesPerSecond;
    public int LockStepTurnID = 0;
    /// <summary>
    /// 包括所有动态物体
    /// 所有需要使用网络时间驱动的游戏对象.需要实现接口IHasGameFrame，由该组件按网络时间，顺序执行每个对象的更新.
    /// </summary>
    public List<INetUpdate> GameObjects = new List<INetUpdate>();
    public void Awake()
    {
        Log.WriteError("FrameSync start. My PlayerID: " + NetWorkBattle.Ins.PlayerId);
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
        while (AccumilatedTime > GameFrameTurnLength)
        {
            GameFrameTurn();
            AccumilatedTime = AccumilatedTime - GameFrameTurnLength;
        }
    }

    //动态调整帧率
    private void UpdateGameFrameRate()
    {
        //LockstepTurnLength = (networkAverage.GetMax() * 2/*two round trips*/) + 1/*minimum of 1 ms*/;
        //GameFrameTurnLength = runtimeAverage.GetMax();

        ////lockstep turn has to be at least as long as one game frame
        //if (GameFrameTurnLength > LockstepTurnLength)
        //{
        //    LockstepTurnLength = GameFrameTurnLength;
        //}

        //GameFramesPerLockstepTurn = LockstepTurnLength / GameFrameTurnLength;
        ////if gameframe turn length does not evenly divide the lockstep turn, there is extra time left after the last
        ////game frame. Add one to the game frame turn length so it will consume it and recalculate the Lockstep turn length
        //if (LockstepTurnLength % GameFrameTurnLength > 0)
        //{
        //    GameFrameTurnLength++;
        //    LockstepTurnLength = GameFramesPerLockstepTurn * GameFrameTurnLength;
        //}

        //LockstepsPerSecond = (1000 / LockstepTurnLength);
        //if (LockstepsPerSecond == 0) { LockstepsPerSecond = 1; } //minimum per second

        //GameFramesPerSecond = LockstepsPerSecond * GameFramesPerLockstepTurn;
    }

    private void GameFrameTurn()
    {
        //first frame is used to process actions
        if (GameFrame == 0)
        {
            if (!LockStepTurn())
            {
                //if the lockstep turn is not ready to advance, do not run the game turn
                return;
            }
        }

        //start the stop watch to determine game frame runtime performance
        gameTurnSW.Start();

        //update game
        //SceneManager.Manager.TwoDPhysics.Update (GameFramesPerSecond);

        List<INetUpdate> finished = new List<INetUpdate>();
        foreach (INetUpdate obj in GameObjects)
        {
            obj.GameFrameTurn(GameFramesPerSecond);
            if (obj.Finished)
            {
                finished.Add(obj);
            }
        }

        foreach (INetUpdate obj in finished)
        {
            GameObjects.Remove(obj);
        }

        GameFrame++;
        if (GameFrame == GameFramesPerLockstepTurn)
        {
            GameFrame = 0;
        }

        gameTurnSW.Stop();
        gameTurnSW.Reset();
    }

    private bool LockStepTurn()
    {
        return false;
    }

    private void SendPendingAction()
    {
    }

    private void ProcessActions()
    {
        gameTurnSW.Start();
        gameTurnSW.Stop();
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

    void OnNetInput(uint player, InputFrame fInput)
    {
        MeteorUnit unit = NetWorkBattle.Ins.GetNetPlayer((int)player);
        unit.OnNetInput(fInput);
    }
}
