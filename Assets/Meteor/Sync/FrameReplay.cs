using protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public interface INetUpdate
{
    void GameFrameTurn(int gameFramesPerSecond, List<FrameCommand> actions);
    bool Finished { get; }
}

//帧指令接收器，用于存储从服务器/单机 时发送来的帧指令.FSC=FRAMESYNCCLIENT
public class FSC:Singleton<FSC>
{
    List<TurnFrames> frameCommand = new List<TurnFrames>();//指令序列.
    
    public void OnReceiveCommand(TurnFrames turn)
    {
        frameCommand.Add(turn);
    }

    public TurnFrames NextTurn(int turnIndex)
    {
        if (frameCommand.Count > turnIndex && turnIndex >= 0)
        {
            return frameCommand[turnIndex];
        }

        return null;
    }

    public void OnDisconnected()
    {
        frameCommand.Clear();
    }
}

//帧指令发送器，用于把客户端的操作发送到服务器/或FrameClient FSS=FRAMESYNCSERVER
public class FSS:Singleton<FSS>
{
    List<TurnFrames> frameCommand = new List<TurnFrames>();
    public void OnDisconnected()
    {
        frameCommand.Clear();
    }

    public void SyncTurn()
    {
        TurnFrames t = frameCommand[FrameReplay.Instance.TurnIndex];
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            Common.SyncTurn(t);
        else
            FSC.Instance.OnReceiveCommand(t);
    }

    //在指定帧推入数据.
    public void Command(int frame, MeteorMsg.MsgType message, MeteorMsg.Command command)
    {
        PushAction(frame, message, command);
    }

    //在当前帧推入指令
    public void Push(int action)
    {
        PushAction(FrameReplay.Instance.NextFrame, MeteorMsg.MsgType.SyncCommand, (MeteorMsg.Command)action);
    }

    public void PushAction(int frame, MeteorMsg.MsgType message, MeteorMsg.Command command)
    {
        TurnFrames t = GetTurnByFrame(frame);
        FrameCommand cmd = new FrameCommand();
        cmd.message = message;
        cmd.command = command;
        cmd.LogicFrame = (uint)frame;
        cmd.playerId = 0;
        t.commands.Add(cmd);
    }

    public TurnFrames GetTurnByFrame(int frame)
    {
        int TurnIndex = frame / FrameReplay.TurnMaxFrame;
        if (frameCommand.Count <= TurnIndex + 1)
        {
            int min = frameCommand.Count;
            for (int i = min; i < TurnIndex + 1; i++)
            {
                TurnFrames t = new TurnFrames();
                t.turnIndex = (uint)i;
                frameCommand.Add(t);
            }
        }
        return frameCommand[TurnIndex];
    }
}

//帧重播器.
public class FrameReplay : MonoBehaviour {
    public static FrameReplay Instance;
    public const int TurnMaxFrame = 5;//每个Turn5帧
    //角色更新顺序是由playerId由小到大跑.
    //场景物件顺序由物件ID由小到大跑.
    private Stopwatch gameTurnSW;
    public bool TurnStarted;
    //返回下一帧,单机插入指令，都需要在下一帧插入
    public int NextFrame
    {
        get
        {
            return LogicFrameIndex + 1;
        }
    }
    private int LogicFrameIndex = 0;
    private int AccumilatedTime = 0;
    private int LogicFrameLength = 50;
    public int TurnIndex = 0;
    TurnFrames nowTurn;//当前的Turn
    /// <summary>
    /// 包括所有动态物体
    /// 所有需要使用网络时间驱动的游戏对象.需要实现接口IHasGameFrame，由该组件按网络时间，顺序执行每个对象的更新.
    /// </summary>
    public List<INetUpdate> GameObjects = new List<INetUpdate>();
    public List<INetUpdate> TotalObjects = new List<INetUpdate>();
    public void RegisterObject(INetUpdate objNetUpdate)
    {
        TotalObjects.Add(objNetUpdate);
    }

    public void UnRegisterObject(INetUpdate objNetUpdate)
    {
        TotalObjects.Remove(objNetUpdate);
    }

    //当场景以及物件全部加载完成，重新开局时
    public void OnBattleStart()
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            //什么都不干，等从服务器拉取到帧后播放即可.
        }
        else
        {
            //发送同步随机种子事件
            FSS.Instance.Command(1, MeteorMsg.MsgType.SyncCommand, MeteorMsg.Command.SyncRandomSeed);
            //发送创建主角色事件.
            FSS.Instance.Command(2, MeteorMsg.MsgType.SyncCommand, MeteorMsg.Command.CreatePlayer);
            //发送场景脚本初始化事件.
            FSS.Instance.Command(3, MeteorMsg.MsgType.SyncCommand, MeteorMsg.Command.LevelStart);
            //向客户端发送首包.
            FSS.Instance.SyncTurn();
        }
        TurnIndex = 0;
        nowTurn = null;
        TurnStarted = true;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void OnBattleFinished()
    {
        TurnStarted = false;
    }

    public void OnDisconnected()
    {
        FSC.Instance.OnDisconnected();
        FSS.Instance.OnDisconnected();
        //如果重新开始,那么所有指令需要全部清除
        TurnStarted = false;
        TurnIndex = 0;
        LogicFrameIndex = 0;
    }

    //called once per unity frame
    public void Update()
    {
        ProtoHandler.Update();

        if (!TurnStarted)
            return;
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
    List<FrameCommand> cacheActions = new List<FrameCommand>();
    List<FrameCommand> GetAction(List<FrameCommand> acts, int logicF)
    {
        cacheActions.Clear();
        for (int i = 0; i < acts.Count; i++)
        {
            if (acts[i].LogicFrame == logicF)
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
            if (FSC.Instance.NextTurn(TurnIndex) == null)
                return;
            nowTurn = FSC.Instance.NextTurn(TurnIndex);
        }
        else
        {

        }

        List<FrameCommand> actions = GetAction(nowTurn.commands, LogicFrameIndex);
        gameTurnSW.Start();

        //update game
        //SceneManager.Manager.TwoDPhysics.Update(GameFramesPerSecond);

        Log.WriteError(string.Format("Turn:{0}, LogicFrame:{1}", nowTurn.turnIndex, LogicFrameIndex));//从这里开始，播放逻辑帧，在取得自己进入场景消息帧时，初始化主角
        for (int i = 0; i < actions.Count; i++)
        {
            switch (actions[i].message)
            {
                case MeteorMsg.MsgType.SyncCommand:
                    switch (actions[i].command)
                    {
                        case MeteorMsg.Command.SyncRandomSeed:
                            //UnityEngine.Random.InitState((int)actions[i].data1);
                            break;
                        case MeteorMsg.Command.CreatePlayer:
                            GameBattleEx.Instance.OnCreatePlayer();
                            break;
                        //关卡开始.
                        case MeteorMsg.Command.LevelStart:
                            if (Global.Instance.GScript != null)
                                Global.Instance.GScript.OnStart();

                            for (int j = 0; j < MeteorManager.Instance.UnitInfos.Count; j++)
                            {
                                if (MeteorManager.Instance.UnitInfos[j].Attr != null)
                                    MeteorManager.Instance.UnitInfos[j].Attr.OnStart();//AI脚本必须在所有角色都加载完毕后再调用
                            }
                            break;
                    }
                    break;
            }
        }

        //在更新过程中会新增/移除某元素
        GameObjects.Clear();
        GameObjects.AddRange(TotalObjects);

        List<INetUpdate> finished = new List<INetUpdate>();
        foreach (INetUpdate obj in GameObjects)
        {
            obj.GameFrameTurn(LogicFrameLength, actions);
            if (obj.Finished)
                finished.Add(obj);
        }

        foreach (INetUpdate obj in finished)
        {
            TotalObjects.Remove(obj);
        }

        GameBattleEx.Instance.NetUpdate();

        LogicFrameIndex++;
        //当逻辑帧为 Turn序号
        if (LogicFrameIndex == nowTurn.turnIndex * TurnMaxFrame)
        {
            nowTurn = null;
            FSS.Instance.SyncTurn();
            TurnIndex++;
        }
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
        
    }

    //void OnNetInput(uint player, InputFrame fInput)
    //{
    //    MeteorUnit unit = NetWorkBattle.Ins.GetNetPlayer((int)player);
    //    unit.OnNetInput(fInput);
    //}
}
