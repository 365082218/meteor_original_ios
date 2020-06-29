using protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    public int x;
    public int y;
}

//存储客户端操作序列.
public class FrameSync
{
    public List<GameFrames> Frames { get { return frames; } }
    List<GameFrames> frames = new List<GameFrames>();
    //玩家要同步的状态，只有发生改变时，才提交对应事件
    Dictionary<int, PlayerState> player = new Dictionary<int, PlayerState>();

    public void OnDisconnected()
    {
        Reset();
    }

    public void Reset()
    {
        frames.Clear();
    }

    //播放录像时.指令就是路线里存储的.
    public void OnReceiveCommands(List<GameFrames> frame_from_record) {
        frames = frame_from_record;
    }

    //从服务器收到一个
    public void OnReceiveCommand(GameFrames frame) {
        frames.Add(frame);
    }

    public void SyncTurn()
    {
        if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
        {
            ////联机时客户端并没有操作,可以不向服务器发送之间的帧指令。但是服务器会生成默认的空操作
            //if (FrameReplay.Instance.LogicTurnIndex >= frames.Count)
            //    return;
            //GameFrames t = frames[FrameReplay.Instance.LogicTurnIndex];
            ////同步一个关键帧（一段渲染帧的指令到服务器）
            //UdpClientProxy.Exec((int)MeteorMsg.MsgType.SyncCommand, t);
        }
        else
        {
            //如果是单机下，所有更新者都没有操作.生成默认的空操作，填充进来
            //if (FrameReplay.Instance.LogicTurnIndex >= frames.Count)
            //{
            //    GameFrames frame = new GameFrames();
            //    frames.Add(frame);
            //}
            //GameFrames f = frames[FrameReplay.Instance.LogicTurnIndex];
        }
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
        //GameFrames t = GetFrame(FrameReplay.Instance.NextTurn);
        //for (int i = 0; i < t.commands.Count; i++)
        //{
        //    if ((t.commands[i].command == MeteorMsg.Command.KeyDown ||
        //        t.commands[i].command == MeteorMsg.Command.KeyUp ||
        //        t.commands[i].command == MeteorMsg.Command.KeyLast) && 
        //        playerId == t.commands[i].playerId &&
        //        t.commands[i].LogicFrame == (uint)FrameReplay.Instance.LogicFrameIndex &&
        //        (uint)key == (uint)t.commands[i].data[0])
        //    {
        //        //Debug.LogError("同一帧同一个按键无法响应2次???");
        //        return;
        //    }
        //}
        //FrameCommand cmd = new FrameCommand();
        //cmd.command = command;
        //cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrameIndex;
        //cmd.playerId = (uint)playerId;
        //cmd.data = new byte[1];
        //cmd.data[0] = (byte)key;
        //t.commands.Add(cmd);
    }

    //角色的移动，当状态发生改变时，修改
    public void PushJoyDelta(int playerId, int x, int y)
    {
        //GameFrames t = GetFrame(FrameReplay.Instance.NextTurn);
        //FrameCommand cmd = new FrameCommand();
        //cmd.command = MeteorMsg.Command.JoyStickMove;
        //cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrameIndex;
        //cmd.playerId = (uint)playerId;
        //System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //Vector2_ vec = new Vector2_();
        //vec.x = x;
        //vec.y = y;
        //ProtoBuf.Serializer.Serialize<Vector2_>(ms, vec);
        //cmd.data = ms.ToArray();
        //t.commands.Add(cmd);
    }

    //在当前帧推入指令-鼠标相对上次的偏移，会导致角色绕Y轴旋转
    public void PushMouseDelta(int playerId, int x, int y)
    {
        //GameFrames t = GetFrame(FrameReplay.Instance.NextTurn);
        //FrameCommand cmd = new FrameCommand();
        //cmd.command = MeteorMsg.Command.MouseMove;
        //cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrameIndex;
        //cmd.playerId = (uint)playerId;
        //System.IO.MemoryStream ms = new System.IO.MemoryStream();
        //Vector2_ vec = new Vector2_();
        //vec.x = x;
        //vec.y = y;
        //ProtoBuf.Serializer.Serialize<Vector2_>(ms, vec);
        //cmd.data = ms.ToArray();
        //t.commands.Add(cmd);
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
    const int TurnFrameMax = 8;
    private int AccumilatedTime = 0;
    public float time;
    public static float deltaTime = FrameReplay.delta;
    const float delta = 20.0f / 1000.0f;
    public int LogicFrameLength = 20;
    GameFrames currentFrame;//当前的Turn
    public List<FrameCommand> actions;
    //全部网络更新对象
    List<NetBehaviour> NetObjects = new List<NetBehaviour>();
    public void RegisterObject(NetBehaviour netObject)
    {
        NetObjects.Add(netObject);
    }

    public void UnRegisterObject(NetBehaviour netObject)
    {
        NetObjects.Remove(netObject);
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
        firstFrame = true;
        actions.Clear();
        Main.Ins.FrameSync.Reset();
        if (Main.Ins.CombatData.GRecord != null)
            Main.Ins.FrameSync.OnReceiveCommands(Main.Ins.CombatData.GRecord.frames);
    }

    private void Awake()
    {
        Instance = this;
        TcpClientProxy.Init();
        UdpClientProxy.Init();
        actions = new List<FrameCommand>();
    }

    public void OnBattleFinished()
    {
        Started = false;
        AccumilatedTime = 0;
        LogicFrameIndex = 0;
        LogicTurnIndex = 0;
        Main.Ins.FrameSync.Reset();
        NetObjects.Clear();
    }

    public void OnDisconnected()
    {
        Main.Ins.FrameSync.OnDisconnected();
        //如果重新开始,那么所有指令需要全部清除
        Started = false;
        LogicFrameIndex = 0;
        LogicTurnIndex = 0;
        AccumilatedTime = 0;
        OnBattleFinished();
    }

    //called once per unity frame
    bool firstFrame = true;
    public void Update()
    {
        ProtoHandler.Update();
        if (!Started)
        {
            //if (OnUpdates != null)
            //    OnUpdates();
            return;
        }

        AccumilatedTime = AccumilatedTime + Convert.ToInt32((Time.deltaTime * 1000));
        while (AccumilatedTime > LogicFrameLength) {
            FrameReplay.deltaTime = delta;
            UdpClientProxy.Update();
            LogicFrame();
            AccumilatedTime = AccumilatedTime - LogicFrameLength;
            time += (LogicFrameLength / 1000.0f);
        }

        if (Main.Ins.CombatData.Replay) {
            if (LogicTurnIndex == Main.Ins.CombatData.GRecord.frames.Count) {
                OnBattleFinished();
                U3D.PopupTip("回放结束");
            }
        }
    }

    public void NetUpdate()
    {
        if (NetObjects.Count != 0)
        {
            for (int i = 0; i < NetObjects.Count; i++)
                if (NetObjects[i] != null)
                    NetObjects[i].NetUpdate();
        }
    }

    public void NetLateUpdate()
    {
        if (NetObjects.Count != 0)
        {
            for (int i = 0; i < NetObjects.Count; i++)
                if (NetObjects[i] != null)
                    NetObjects[i].NetLateUpdate();
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
            currentFrame = GetNextTurn(LogicTurnIndex);
            if (currentFrame == null) {
                //如果没有取得下一个回合的指令.能否继续
                //如果是联机或者回放-那么拿不到后面的操作指令是无法继续播放的
                if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer || Main.Ins.CombatData.Replay)
                    return;
            } 
        }
        else
        {

        }

        //update game-物理引擎更新.
        //SceneManager.Manager.TwoDPhysics.Update(GameFramesPerSecond);
        actions.Clear();
        if (currentFrame != null) {
            actions = GetAction(currentFrame.commands, LogicFrameIndex);
            for (int i = 0; i < actions.Count; i++) {
                switch (actions[i].command) {
                    case MeteorMsg.Command.SyncRandomSeed:
                        SyncInitData seed = ProtoBuf.Serializer.Deserialize<SyncInitData>(new System.IO.MemoryStream(actions[i].data));
                        UnityEngine.Random.InitState((int)seed.randomSeed);
                        break;
                    case MeteorMsg.Command.SpawnPlayer:
                        System.IO.MemoryStream ms = new System.IO.MemoryStream(actions[i].data);
                        PlayerEventData evt = ProtoBuf.Serializer.Deserialize<PlayerEventData>(ms);
                        Main.Ins.GameBattleEx.OnCreateNetPlayer(evt);
                        break;
                }
            }
        }

        NetUpdate();
        NetLateUpdate();
        LogicFrameIndex++;
        if (LogicFrameIndex % TurnFrameMax == 0)
        {
            Main.Ins.FrameSync.SyncTurn();
            LogicTurnIndex++;
            currentFrame = null;
            LogicFrameIndex = 0;
        }
        if (firstFrame) {
            Main.Ins.DialogStateManager.ChangeState(null);//关闭loading条
            firstFrame = false;
        }
    }

    //取得当前播放轮次的下一个轮次的帧信息
    GameFrames GetNextTurn(int index) {
        if (Main.Ins.FrameSync.Frames.Count > index)
            return Main.Ins.FrameSync.Frames[index];
        return null;
    }
}
