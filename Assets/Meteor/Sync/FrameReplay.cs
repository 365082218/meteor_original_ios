using ProtoBuf;
using protocol;
using System;
using System.Collections.Generic;
using System.IO;
//using System.Diagnostics;
using UnityEngine;
//帧指令接收器，用于存储从服务器/单机 时发送来的帧指令.FSC=FRAMESYNCCLIENT
public class FSC:Singleton<FSC>
{
    List<GameFrames> KeyFrames = new List<GameFrames>();//指令序列.没一个下标都是一帧的.
    public void OnReceiveCommand(GameFrames turn)
    {
        Debug.LogError("OnReceiveCommand Udp");
        KeyFrames.Add(turn);
    }

    public GameFrames NextKeyFrame(int KeyFrame)
    {
        if (KeyFrames.Count > KeyFrame && KeyFrame >= 0)
        {
            return KeyFrames[KeyFrame];
        }

        return null;
    }

    public void Reset()
    {
        KeyFrames.Clear();
    }

    public void OnDisconnected()
    {
        Reset();
    }

    public List<FrameCommand> GetCommand(int frame)
    {
        cmdCache.Clear();
        for (int i = 0; i < KeyFrames[frame].commands.Count; i++)
        {
            cmdCache.Add(KeyFrames[frame].commands[i]);
        }
        return cmdCache;
    }

    List<FrameCommand> cmdCache = new List<FrameCommand>();
}

//帧指令发送器，用于把客户端的操作发送到服务器/或FrameClient FSS=FRAMESYNCSERVER
//存储客户端操作序列.
public class FSS:Singleton<FSS>
{
    List<GameFrames> KeyFrames = new List<GameFrames>();
    public void OnDisconnected()
    {
        Reset();
    }

    public void Reset()
    {
        KeyFrames.Clear();
    }

    /// <summary>
    /// 向服务器同步本地上一个关键帧的操作
    /// </summary>
    int KeyFrameIndex = 0;
    public void SyncKeyFrame()
    {
        //如果是回放历史帧状态，不上传操作.
        if (FrameReplay.Instance.bReplay)
            return;
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            //联机时客户端并没有操作,可以不向服务器发送之前一个回合的帧指令。但是服务器会生成默认的空操作
            if (KeyFrameIndex >= KeyFrames.Count)
                return;
            GameFrames t = KeyFrames[KeyFrameIndex];
            UdpClientProxy.Exec((int)MeteorMsg.Command.SyncToSvr, t);
        }
        else
        {
            //如果是单机下，所有更新者都没有操作.生成默认的空操作，填充进来
            if (KeyFrameIndex >= KeyFrames.Count)
            {
                GameFrames frame = new GameFrames();
                KeyFrames.Add(frame);
            }
            GameFrames f = KeyFrames[KeyFrameIndex];
            FSC.Instance.OnReceiveCommand(f);
        }
        KeyFrameIndex++;
    }

    //在当前帧推入指令-鼠标相对上次的偏移，会导致角色绕Y轴旋转
    public void PushMouseDelta(int playerId, float x, float y)
    {
        GameFrames t = GetFrame(FrameReplay.Instance.KeyFrameIndex + 1);
        FrameCommand cmd = new FrameCommand();
        cmd.command = MeteorMsg.Command.JoyStickMove;
        cmd.fillFrameIndex = (uint)FrameReplay.Instance.FillFrameIndex;
        cmd.playerId = (uint)playerId;
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        Vector2_ vec = new Vector2_();
        vec.x = (int)x * 1000;
        vec.y = (int)y * 1000;
        ProtoBuf.Serializer.Serialize<Vector2_>(ms, vec);
        cmd.data = ms.ToArray();
        t.commands.Add(cmd);
    }

    public void Push(int action)
    {
        GameFrames t = GetFrame(FrameReplay.Instance.KeyFrameIndex + 1);
        FrameCommand cmd = new FrameCommand();
        cmd.command = (MeteorMsg.Command)action;
        cmd.fillFrameIndex = (uint)FrameReplay.Instance.FillFrameIndex;
        cmd.playerId = (uint)NetWorkBattle.Instance.PlayerId;                  
        t.commands.Add(cmd);
    }

    public void PushAction<T>(MeteorMsg.Command command, T req)
    {
        GameFrames t = GetFrame(FrameReplay.Instance.KeyFrameIndex + 1);
        FrameCommand cmd = new FrameCommand();
        cmd.command = command;
        cmd.fillFrameIndex = (uint)FrameReplay.Instance.FillFrameIndex;
        cmd.playerId = (uint)NetWorkBattle.Instance.PlayerId;
        MemoryStream ms = new MemoryStream();
        Serializer.Serialize(ms, req);
        byte[] coreData = ms.ToArray();
        cmd.data = coreData;
        t.commands.Add(cmd);
    }

    //补齐从过去到未来的帧号中间的帧
    public GameFrames GetFrame(int Key)
    {
        if (KeyFrames.Count <= Key)
        {
            int min = KeyFrames.Count;
            for (int i = min; i < Key + 1; i++)
            {
                GameFrames t = new GameFrames();
                KeyFrames.Add(t);
            }
        }
        return KeyFrames[Key];
    }
}

//帧重播器.
public class FrameReplay : MonoBehaviour {
    public static FrameReplay Instance;
    //角色更新顺序是由playerId由小到大跑.
    //场景物件顺序由物件ID由小到大跑.
    //private Stopwatch gameTurnSW;
    public bool Started;
    public bool bReplay;//开始同步历史帧
    public int LogicFrameIndex = 0;
    //返回下一帧,单机插入指令，都需要在下一帧插入
    public int NextFrame{get{return LogicFrameIndex + 1;}}
    public int KeyFrameIndex{get{return LogicFrameIndex / FillFrame;} }
    public int FillFrameIndex { get { return LogicFrameIndex % FillFrame; } }
    public const int FillFrame = 5;//填充帧-一个关键帧内包含5个数据填充帧.
    private int AccumilatedTime = 0;
    public static float deltaTime = 17.0f / 1000.0f;
    public int LogicFrameLength = 17;
    GameFrames clientFrame;//当前的Turn-每个Turn保留5帧游戏事件数据，服务器每秒发送20个Turn，客户端每个Turn向服务器发送最新的操作，拉取服务器上上次最新的操作.
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

    //加载历史指令中
    int serverKeyFrameIndex = 0;//当前取得的最新的帧
    public void OnLoading()
    {
        bReplay = true;
        LogicFrameLength = 2;//4倍速
        UdpClientProxy.Exec<int>((int)MeteorMsg.Command.FetchCommand, serverKeyFrameIndex);
    }

    public void OnLoaingComplete()
    {
        bReplay = false;
        LogicFrameLength = 17;

        PlayerEventData req = new PlayerEventData();
        req.camp = (uint)NetWorkBattle.Instance.camp;//暂时全部为盟主模式
        req.model = (uint)NetWorkBattle.Instance.heroIdx;
        req.weapon = (uint)NetWorkBattle.Instance.weaponIdx;
        req.playerId = (uint)NetWorkBattle.Instance.PlayerId;
        FSS.Instance.PushAction(MeteorMsg.Command.SpawnPlayer, req);
    }
    
    //当场景以及物件全部加载完成，重新开局时
    public void OnBattleStart()
    {
        clientFrame = null;
        Started = true;
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

            //播放历史帧-追赶上当前速度
            if (bReplay)
            {
                if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
                {
                    AccumilatedTime = AccumilatedTime + Convert.ToInt32((Time.deltaTime * 1000));
                    while (AccumilatedTime > LogicFrameLength)
                    {
                        UdpClientProxy.Update();
                        LogicFrame();
                        AccumilatedTime = AccumilatedTime - LogicFrameLength;
                    }
                }
            }
            return;
        }

        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            //Basically same logic as FixedUpdate, but we can scale it by adjusting FrameLength
            AccumilatedTime = AccumilatedTime + Convert.ToInt32((Time.deltaTime * 1000));
            while (AccumilatedTime > LogicFrameLength)
            {
                UdpClientProxy.Update();
                LogicFrame();
                AccumilatedTime = AccumilatedTime - LogicFrameLength;
            }
        }
        else
        {
            AccumilatedTime = AccumilatedTime + Convert.ToInt32((Time.deltaTime * 1000));
            while (AccumilatedTime > LogicFrameLength)
            {
                LogicFrame();
                AccumilatedTime = AccumilatedTime - LogicFrameLength;
            }
        }
    }

    public delegate void OnUpdate();
    public event OnUpdate OnUpdates;//在战斗还未开始时
    //取得对应逻辑帧的数据
    List<FrameCommand> cacheActions = new List<FrameCommand>();
    List<FrameCommand> GetAction(List<FrameCommand> acts, int fillFrame)
    {
        cacheActions.Clear();
        for (int i = 0; i < acts.Count; i++)
        {
            if (acts[i].fillFrameIndex == fillFrame)
                cacheActions.Add(acts[i]);
        }
        return cacheActions;
    }

    private void LogicFrame()
    {
        //得到当前逻辑帧数据，对普通事件数据，调用对应的事件函数，对按键，在更新每个对象使，应用到每个对象上.
        if (clientFrame == null)
        {
            //等待从服务器收到接下来一帧的信息.
            clientFrame = FSC.Instance.NextKeyFrame(KeyFrameIndex);
            if (clientFrame == null)
                return;
        }
        else
        {

        }

        List<FrameCommand> actions = GetAction(clientFrame.commands, FillFrameIndex);
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

        //当是该关键帧的最后一个填充帧时-向服务器发送自己的消息
        if (FillFrameIndex == (FillFrame - 1))
        {
            FSS.Instance.SyncKeyFrame();
            clientFrame = null;
        }
        LogicFrameIndex++;
    }
}
