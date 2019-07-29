using protocol;
using System;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
//帧指令接收器，用于存储从服务器/单机 时发送来的帧指令.FSC=FRAMESYNCCLIENT
public class FSC:Singleton<FSC>
{
    List<GameFrames> frames = new List<GameFrames>();//指令序列.没一个下标都是一帧的.
    public void OnReceiveCommand(GameFrames turn)
    {
        frames.Add(turn);
        //收到服务端的帧同步的回应.
        //UdpClientProxy.Exec<int>((int)MeteorMsg.MsgType.SyncCommand, FrameReplay.Instance.LogicFrameIndex);
    }

    public GameFrames NextFrame(int logicTurn)
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            if (frames.Count > logicTurn && logicTurn >= 0)
            {
                return frames[logicTurn];
            }
            return null;
        }
        else
        {
            int i = frames.Count;
            for (;i <= logicTurn; i++)
            {
                GameFrames f = new GameFrames();
                frames.Add(f);
            }
            return frames[logicTurn];
        }
    }

    public void Reset()
    {
        frames.Clear();
    }

    public void OnDisconnected()
    {
        Reset();
    }

    public List<FrameCommand> GetCommand(int frame)
    {
        cmdCache.Clear();
        if (frames.Count > frame)
        {
            for (int i = 0; i < frames[frame].commands.Count; i++)
            {
                cmdCache.Add(frames[frame].commands[i]);
            }
        }
        return cmdCache;
    }

    List<FrameCommand> cmdCache = new List<FrameCommand>();
}

//帧指令发送器，用于把客户端的操作发送到服务器/或FrameClient FSS=FRAMESYNCSERVER
//存储客户端操作序列.
public class FSS:Singleton<FSS>
{
    List<GameFrames> frames = new List<GameFrames>();
    public void OnDisconnected()
    {
        Reset();
    }

    public void Reset()
    {
        frames.Clear();
    }

    
    public void SyncTurn()
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            //联机时客户端并没有操作,可以不向服务器发送之间的帧指令。但是服务器会生成默认的空操作
            if (FrameReplay.Instance.LogicTurnIndex >= frames.Count)
                return;
            GameFrames t = frames[FrameReplay.Instance.LogicTurnIndex];
            UdpClientProxy.Exec((int)MeteorMsg.MsgType.SyncCommand, t);
        }
        else
        {
            //如果是单机下，所有更新者都没有操作.生成默认的空操作，填充进来
            if (FrameReplay.Instance.LogicTurnIndex >= frames.Count)
            {
                GameFrames frame = new GameFrames();
                frames.Add(frame);
            }
            GameFrames f = frames[FrameReplay.Instance.LogicTurnIndex];
            FSC.Instance.OnReceiveCommand(f);
        }
    }

    //在指定帧推入数据.
    public void Command(int frame, MeteorMsg.MsgType message, MeteorMsg.Command command)
    {
        PushAction(frame, message, command);
    }

    public void PushKeyUp(int playerId, EKeyList key)
    {
        PushKeyEvent(MeteorMsg.Command.KeyUp, playerId, key);
    }

    public void PushKeyDown(int playerId, EKeyList key)
    {
        PushKeyEvent(MeteorMsg.Command.KeyDown, playerId, key);
    }

    public void PushKeyEvent(MeteorMsg.Command command, int playerId, EKeyList key)
    {
        GameFrames t = GetFrame(FrameReplay.Instance.NextTurn);
        for (int i = 0; i < t.commands.Count; i++)
        {
            if ((t.commands[i].command == MeteorMsg.Command.KeyDown ||
                t.commands[i].command == MeteorMsg.Command.KeyUp ||
                t.commands[i].command == MeteorMsg.Command.KeyLast) && 
                playerId == t.commands[i].playerId &&
                t.commands[i].LogicFrame == (uint)FrameReplay.Instance.LogicFrameIndex &&
                (uint)key == (uint)t.commands[i].data[0])
            {
                Debug.LogError("同一帧同一个按键无法响应2次???");
                return;
            }
        }
        FrameCommand cmd = new FrameCommand();
        cmd.command = command;
        cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrameIndex;
        cmd.playerId = (uint)playerId;
        cmd.data = new byte[1];
        cmd.data[0] = (byte)key;
        t.commands.Add(cmd);
    }

    public void PushJoyDelta(int playerId, float x, float y)
    {
        GameFrames t = GetFrame(FrameReplay.Instance.NextTurn);
        FrameCommand cmd = new FrameCommand();
        cmd.command = MeteorMsg.Command.JoyStickMove;
        cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrameIndex;
        cmd.playerId = (uint)playerId;
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        Vector2_ vec = new Vector2_();
        vec.x = (int)(x * 1000);
        vec.y = (int)(y * 1000);
        ProtoBuf.Serializer.Serialize<Vector2_>(ms, vec);
        cmd.data = ms.ToArray();
        t.commands.Add(cmd);
    }

    //在当前帧推入指令-鼠标相对上次的偏移，会导致角色绕Y轴旋转
    public void PushMouseDelta(int playerId, float x, float y)
    {
        GameFrames t = GetFrame(FrameReplay.Instance.NextTurn);
        FrameCommand cmd = new FrameCommand();
        cmd.command = MeteorMsg.Command.MouseMove;
        cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrameIndex;
        cmd.playerId = (uint)playerId;
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        Vector2_ vec = new Vector2_();
        vec.x = (int)(x * 1000);
        vec.y = (int)(y * 1000);
        ProtoBuf.Serializer.Serialize<Vector2_>(ms, vec);
        cmd.data = ms.ToArray();
        t.commands.Add(cmd);
    }

    public void Push(int action)
    {
        PushAction(FrameReplay.Instance.NextTurn, MeteorMsg.MsgType.SyncCommand, (MeteorMsg.Command)action);
    }

    public void PushAction(int frame, MeteorMsg.MsgType message, MeteorMsg.Command command)
    {
        GameFrames t = GetFrame(frame);
        FrameCommand cmd = new FrameCommand();
        cmd.command = command;
        cmd.LogicFrame = (uint)frame;
        cmd.playerId = (uint)NetWorkBattle.Instance.PlayerId;
        t.commands.Add(cmd);
    }

    //补齐从过去到未来的帧号中间的帧
    public GameFrames GetFrame(int frame)
    {
        if (frames.Count <= frame)
        {
            int min = frames.Count;
            for (int i = min; i < frame + 1; i++)
            {
                GameFrames t = new GameFrames();
                frames.Add(t);
            }
        }
        return frames[frame];
    }
}

//帧重播器.
public class FrameReplay : MonoBehaviour {
    public static FrameReplay Instance;
    //角色更新顺序是由playerId由小到大跑.
    //场景物件顺序由物件ID由小到大跑.
    //private Stopwatch gameTurnSW;
    public bool Started;
    public bool bSync;//开始同步历史帧
    //返回下一帧,单机插入指令，都需要在下一帧插入
    public int NextTurn
    {
        get
        {
            return LogicTurnIndex + 1;
        }
    }
    public int LogicFrameIndex = 0;
    public int LogicTurnIndex = 0;
    const int TurnFrameMax = 4;
    private int AccumilatedTime = 0;
    public float time;
    public static float deltaTime = 20.0f / 1000.0f;
    public int LogicFrameLength = 20;
    GameFrames currentFrame;//当前的Turn
    public List<FrameCommand> actions;

    public static event Action UpdateEvent;
    public static event Action LateUpdateEvent;

    public static void InvokeLockUpdate()
    {
        if (UpdateEvent != null)
            UpdateEvent();
    }

    public static void InvokeLateUpdate()
    {
        if (LateUpdateEvent != null)
            LateUpdateEvent();
    }

    public void OnSyncCommands()
    {
        bSync = true;
        LogicFrameLength = 5;
    }

    bool playRecord = false;
    //当场景以及物件全部加载完成，重新开局时
    public void OnBattleStart()
    {
        currentFrame = null;
        Started = true;
        LogicTurnIndex = 0;
        LogicFrameIndex = 0;
        playRecord = false;
    }

    private void Awake()
    {
        Instance = this;
        TcpClientProxy.Init();
        UdpClientProxy.Init();
    }

    public void OnBattleFinished()
    {
        Started = false;
        AccumilatedTime = 0;
        LogicFrameIndex = 0;
        LogicTurnIndex = 0;
        FSS.Instance.Reset();
        FSC.Instance.Reset();
    }

    public void OnDisconnected()
    {
        FSC.Instance.OnDisconnected();
        FSS.Instance.OnDisconnected();
        //如果重新开始,那么所有指令需要全部清除
        Started = false;
        LogicFrameIndex = 0;
        LogicTurnIndex = 0;
        AccumilatedTime = 0;
    }

    //called once per unity frame
    public void Update()
    {
        ProtoHandler.Update();
        if (!Started)
        {
            if (OnUpdates != null)
                OnUpdates();
            return;
        }
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            //Basically same logic as FixedUpdate, but we can scale it by adjusting FrameLength
            AccumilatedTime = AccumilatedTime + Convert.ToInt32((Time.deltaTime * 1000)); //convert sec to milliseconds
            while (AccumilatedTime > LogicFrameLength)
            {
                UdpClientProxy.Update();
                
                LogicFrame();
                //Debug.LogError("logicframe:" + LogicFrameIndex);
                AccumilatedTime = AccumilatedTime - LogicFrameLength;
                time += (LogicFrameLength / 1000.0f);
            }
        }
        else
        {
            FrameReplay.deltaTime = Time.deltaTime;
            LogicFrame();
            time += Time.deltaTime;
        }
    }

    public delegate void OnUpdate();
    public event OnUpdate OnUpdates;//在战斗还未开始时
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
        return cacheActions;
    }

    private void LogicFrame()
    {
        //得到当前逻辑帧数据，对普通事件数据，调用对应的事件函数，对按键，在更新每个对象使，应用到每个对象上.
        if (currentFrame == null)
        {
            //等待从服务器收到接下来一帧的信息.
            currentFrame = FSC.Instance.NextFrame(LogicTurnIndex);
            if (currentFrame == null)
                return;
        }
        else
        {

        }

        actions = GetAction(currentFrame.commands, LogicFrameIndex);
        //gameTurnSW.Start();

        //update game
        //SceneManager.Manager.TwoDPhysics.Update(GameFramesPerSecond);

        //Log.WriteError(string.Format("Turn:{0}, LogicFrame:{1}", nowTurn.turnIndex, LogicFrameIndex));//从这里开始，播放逻辑帧，在取得自己进入场景消息帧时，初始化主角
        for (int i = 0; i < actions.Count; i++)
        {
            switch (actions[i].command)
            {
                case MeteorMsg.Command.SyncRandomSeed:
                    SyncInitData seed = ProtoBuf.Serializer.Deserialize<SyncInitData>(new System.IO.MemoryStream(actions[i].data));
                    UnityEngine.Random.InitState((int)seed.randomSeed);
                    break;
                case MeteorMsg.Command.SpawnPlayer:
                    System.IO.MemoryStream ms = new System.IO.MemoryStream(actions[i].data);
                    PlayerEventData evt = ProtoBuf.Serializer.Deserialize<PlayerEventData>(ms);
                    GameBattleEx.Instance.OnCreateNetPlayer(evt);
                    break;
            }
        }
        if (UpdateEvent != null)
            UpdateEvent();
        if (LateUpdateEvent != null)
            LateUpdateEvent();
        LogicFrameIndex++;
        if (LogicFrameIndex % TurnFrameMax == 0)
        {
            FSS.Instance.SyncTurn();
            LogicTurnIndex++;
            currentFrame = null;
            LogicFrameIndex = 0;
        }
    }
}
