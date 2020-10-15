using ProtoBuf;
using protocol;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Assets.Code.Idevgame.Common.Util;
using Idevgame.GameState.DialogState;

//联机上传给服务端的数据,只提交设备输入
public class FrameSyncServer:Singleton<FrameSyncServer> {
    public GameFrame Frame;//当前这一帧的输入.
    int DelayTime = 67;
    public int Tick = 67;//
    public const int Retry = 15000;//还能抢救一下
    public const int Lost = 25000;//还是算了吧
    public bool Synced = true;
    public void OnSynced() {
        Synced = true;
        Tick = DelayTime;
        if (ReconnectDialogState.Exist())
            ReconnectDialogState.State.Close();
    }
    public void OnDisconnected() {
        Synced = true;
        Tick = DelayTime;
    }

    public void ChangeSyncRate(int syncRate) {
        DelayTime = 1000 / syncRate;
        DelayTime = Mathf.Clamp(DelayTime, 16, 100);//60次-10次每秒
        U3D.InsertSystemMsg(string.Format("同步速率调整为:{0}", 1000 / DelayTime));
    }

    //在下一帧立即发出指令不要等同步
    public void NetEvent<T>(MeteorMsg.Command cmd, T req) {
        KcpClient.Ins.Exec((int)cmd, req);
    }

    int actionPrev = 0;//上一个动作
    public void SyncMainPlayer() {
        Frame = new GameFrame();
        FrameCommand cmd = new FrameCommand();
        cmd.playerId = (uint)Main.Ins.LocalPlayer.InstanceId;
        cmd.command = MeteorMsg.Command.ClientSync;
        MemoryStream ms = new MemoryStream();
        PlayerSync sync = new PlayerSync();
        MeteorUnit player = Main.Ins.LocalPlayer;
        sync.playerId = (uint)player.InstanceId;
        sync.position = new _Vector3();
        sync.position.x = Mathf.FloorToInt(player.transform.position.x * 1000);
        sync.position.y = Mathf.FloorToInt(player.transform.position.y * 1000);
        sync.position.z = Mathf.FloorToInt(player.transform.position.z * 1000);
        sync.rotation = new _Quaternion();
        sync.rotation.x = Mathf.FloorToInt(player.transform.rotation.x * 1000);
        sync.rotation.y = Mathf.FloorToInt(player.transform.rotation.y * 1000);
        sync.rotation.z = Mathf.FloorToInt(player.transform.rotation.z * 1000);
        sync.rotation.w = Mathf.FloorToInt(player.transform.rotation.w * 1000);
        sync.model = (uint)player.ModelId;
        sync.weapon = (uint)player.Attr.Weapon;
        sync.weapon1 = (uint)player.Attr.Weapon2;
        sync.hp = (uint)player.Attr.hpCur;
        sync.ang = (uint)player.AngryValue;
        sync.action = -1;
        
        if (actionPrev != player.ActionMgr.mActiveAction.Idx) {
            sync.action = player.ActionMgr.mActiveAction.Idx;
            actionPrev = player.ActionMgr.mActiveAction.Idx;
        }

        List<int> buffs = U3D.GetUnitBuffs(player);
        for (int i = 0; i < buffs.Count; i++) {
            sync.buff.Add((uint)buffs[i]);
        }
        
        ProtoBuf.Serializer.Serialize<PlayerSync>(ms, sync);
        cmd.Data = ms.ToArray();
        Frame.commands.Add(cmd);
    }

    public void Pause() {
        pause = true;
        Synced = true;
    }

    public void Resume() {
        pause = false;
        Synced = true;
    }

    bool pause = false;
    //向服务器上传这一帧的操作
    public void SyncCommand(int delta) {
        if (pause)//处于新轮次阶段，不要在这个阶段同步信息
            return;
        Tick -= delta;
        if (Tick < 0) {
            if (Synced) {
                //同步一个逻辑帧的操作到服务器
                SyncMainPlayer();
                KcpClient.Ins.Exec((int)MeteorMsg.Command.ClientSync, Frame);
                Synced = false;
                Tick = DelayTime;
                return;
            }
            //10S内没有收到UDP包了.
            if (Tick < -Lost) {
                NetWorkBattle.Ins.OnDisconnect();
                Tick = DelayTime;
            } else if (Tick < -Retry){
                ReconnectDialogState.State.Open();
            }
        }
    }
}

//存储从服务器拉取到的数据
public class FrameSyncLocal : Singleton<FrameSyncLocal> {
    public OnEnterLevelRsp netPlayers;//场景进入时，拉取到的正在场地内战斗的角色信息，不包括自己的

    PlayerEvent mainPlayer;
    public void OnSelfEnterLevel(PlayerEvent player) {
        mainPlayer = player;
    }

    public List<PlayerEvent> comingPlayers = new List<PlayerEvent>();//在加载场景这段时间内，进入到我的房间的新玩家.因为场景还未
    public void OnPlayerEnterLevel(PlayerEvent player) {
        comingPlayers.Add(player);
    }

    public void Reset() {
        mainPlayer = null;
        netPlayers = null;
        comingPlayers.Clear();
    }

    public void OnDisconnected() {
        Reset();
    }

    //把新进来的玩家创建出来
    public void SyncEnterPlayer() {
        for (int i = 0; i < comingPlayers.Count; i++) {
            MeteorUnit player = U3D.GetUnit((int)comingPlayers[i].playerId);
            if (player != null)
                continue;
            U3D.OnCreateNetPlayer(comingPlayers[i]);
        }
        comingPlayers.Clear();
    }
    //TCP拉取场景玩家信息
    public void OnEnterLevel(OnEnterLevelRsp fr) {
        if (FrameReplay.Ins.Started) {
            netPlayers = fr;
            SyncNetPlayer();
        } else {
            netPlayers = fr;
        }
    }

    //
    public void SyncMainPlayer() {
        U3D.OnCreateNetPlayer(mainPlayer);
        mainPlayer = null;
    }

    public void SyncNetPlayer() {
        if (netPlayers != null) {
            for (int i = 0; i < netPlayers.players.Count; i++) {
                PlayerSync player = netPlayers.players[i];
                MeteorUnit unit = U3D.GetUnit((int)player.playerId);
                if (unit == null) {
                    U3D.OnCreateNetPlayer(player);
                }
            }
        }
        if (WeaponSelectDialogState.Exist) {
            Main.Ins.DialogStateManager.ChangeState(null);
        }
        FightState.State.Open();
        FrameSyncServer.Ins.Resume();
        netPlayers = null;
    }
}
//单机保存的所有玩家模拟输入的数据/从服务器拉取的全部操作数据
//public class FrameSyncLocal:Singleton<FrameSyncLocal> {
//    public Dictionary<int, GameFrame> Frames { get { return frames; } }
//    Dictionary<int, GameFrame> frames = new Dictionary<int, GameFrame>();
//    public void OnDisconnected() {
//        Reset();
//    }

//    public void Reset() {
//        frames.Clear();
//    }

//播放录像时.指令就是路线里存储的.
//public void OnReceiveCommands(Dictionary<int, GameFrame> frame_from_record) {
//    frames = frame_from_record;
//}

//从服务器收到一个
//public void OnReceiveCommand(GameFrame frame) {
//}

//public void PushKeyUp(int playerId, EKeyList key) {
//if (!FrameReplay.Instance.bRecord)
//    return;
//PushKeyEvent(MeteorMsg.Command.Key, playerId, key, VirtualKeyState.Release);
//}

//public void PushKeyDown(int playerId, EKeyList key) {
//if (!FrameReplay.Instance.bRecord)
//    return;
//PushKeyEvent(MeteorMsg.Command.Key, playerId, key, VirtualKeyState.Press);
//}

//public void PushKeyEvent(MeteorMsg.Command command, int playerId, EKeyList key, VirtualKeyState state) {
//if (!FrameReplay.Instance.bRecord)
//    return;
////如果这一帧没有数据.添加空数据
////bool checkExist = true;//需要检查这一帧这个按键是否错误的响应多次
//if (!frames.ContainsKey(FrameReplay.Instance.LogicFrames)) {
//    frames.Add(FrameReplay.Instance.LogicFrames, new GameFrame());
//    //checkExist = false;
//}

//一个事件是否在同一帧响应多次
//GameFrame f = frames[FrameReplay.Instance.LogicFrames];
//if (checkExist) {
//    for (int i = 0; i < f.commands.Count; i++) {
//        if (f.commands[i].command == command && f.commands[i].playerId == playerId) {
//            MemoryStream ms2 = new MemoryStream(f.commands[i].Data);
//            KeyEvent k = Serializer.Deserialize<KeyEvent>(ms2);
//            if (k.key == (int)key) {
//                Debug.LogError("frame exist same message:" + FrameReplay.Instance.LogicFrames + " " + command);
//                return;
//            }
//        }
//    }
//}

//往空数据里填充事件
//FrameCommand cmd = new FrameCommand();
//cmd.command = command;
//cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrames;
//cmd.playerId = (uint)playerId;
//KeyEvent kEvent = new KeyEvent();
//kEvent.key = (int)key;
//kEvent.state = (int)state;
//MemoryStream ms = new MemoryStream();
//Serializer.Serialize(ms, kEvent);
//cmd.Data = ms.ToArray();
//f.commands.Add(cmd);
//}

//角色的移动，主要是Robot没有按键驱动，必须得存
//Dictionary<int, MouseEvent> lastJoy = new Dictionary<int, MouseEvent>();//存储最新的移动方向
//Dictionary<int, MouseEvent> lastMouse = new Dictionary<int, MouseEvent>();//存储最新的拖拽方向-视角控制
//public void PushDelta(MeteorMsg.Command message, int playerId, int x, int y) {
//if (!FrameReplay.Instance.bRecord)
//    return;
//Dictionary<int, MouseEvent> lastEvent = (message == MeteorMsg.Command.Mouse ? lastMouse : lastJoy);
////判断这个状态是否改变，未改变的，丢弃
//if (lastEvent.ContainsKey(playerId)) {
//    if (lastEvent[playerId].x == x && lastEvent[playerId].y == y)
//        return;
//}
////如果这一帧没有数据.添加空数据
//if (!frames.ContainsKey(FrameReplay.Instance.LogicFrames)) {
//    frames.Add(FrameReplay.Instance.LogicFrames, new GameFrame());
//}
////往空数据里填充事件
//GameFrame t = frames[FrameReplay.Instance.LogicFrames];
//FrameCommand cmd = new FrameCommand();
//cmd.command = message;
//cmd.LogicFrame = (uint)FrameReplay.Instance.LogicFrames;
//cmd.playerId = (uint)playerId;
//System.IO.MemoryStream ms = new System.IO.MemoryStream();
//MouseEvent vec = new MouseEvent();
//vec.x = x;
//vec.y = y;
//ProtoBuf.Serializer.Serialize<MouseEvent>(ms, vec);
//cmd.Data = ms.ToArray();
//t.commands.Add(cmd);
//lastEvent[playerId] = vec;
//}
//}

//帧重播器.
public class FrameReplay : MonoBehaviour {
    public static FrameReplay Ins;
    //角色更新顺序是由playerId由小到大跑.
    //场景物件顺序由物件ID由小到大跑.
    //private Stopwatch gameTurnSW;
    public bool Started;
    Action Logic;
    public static float deltaTime;//单位 毫秒
    //public GameFrame frameData;
    //全部网络更新对象
    [SerializeField]
    List<NetBehaviour> NetObjects = new List<NetBehaviour>();
    public void RegisterObject(NetBehaviour netObject) {
        NetObjects.Add(netObject);
    }

    public void UnRegisterObject(NetBehaviour netObject) {
        NetObjects.Remove(netObject);
    }

    public void OnKcpClosed() {
        KcpClient.Ins.Disconnect();
        KcpClient.Ins.Connect(NetWorkBattle.Ins.KcpPort, NetWorkBattle.Ins.PlayerId);
    }

    [SerializeField]
    int track = 0;
    bool playRecord = false;
    //当场景以及物件全部加载完成，重新开局时
    public void OnBattleStart() {
        time = 0;
        Started = true;
        playRecord = false;
        firstFrame = true;
        Logic = LocalLogic;
        deltaTime = 0;
        //FrameSyncLocal.Ins.Reset();
        if (Main.Ins.JoyStick.isActiveAndEnabled) {
            Main.Ins.JoyStick.OnBattleStart();
        }
        //if (CombatData.Ins.GRecord != null)
        //    FrameSyncLocal.Ins.OnReceiveCommands(CombatData.Ins.GRecord.frames);
    }

    public void Resume() {
        Started = true;
        CombatData.Ins.GameFinished = false;
    }

    //暂停，切换到联机的结算页面
    public void Stop() {
        Started = false;
    }

    private void Awake() {
        Ins = this;
        TcpClientProxy.Ins.Init();
        KcpClient.Ins.Init();
    }

    public void OnBattleFinished() {
        Started = false;
        //FrameSyncLocal.Ins.Reset();
        U3D.CloseBox();
        U3D.CancelWatch();
        PersistDialogMgr.Ins.ExitAllState();
    }

    public void OnDisconnected() {
        FrameSyncLocal.Ins.OnDisconnected();
        FrameSyncServer.Ins.OnDisconnected();
        OnBattleFinished();
    }

    //called once per unity frame
    bool firstFrame = true;
    public void Update() {
        KcpClient.Ins.Update();//KCP客户端
        TcpProtoHandler.Ins.Update();//TCP报文处理
        UdpProtoHandler.Ins.Update();//UDP报文处理
        Timer.Update(Time.deltaTime);//处理同线程定时器
        if (!Started) {
            return;
        }

        if (Logic != null) {
            Logic.Invoke();
        }
    }

    //单机-直接在每一个渲染帧里
    public float time;
    public void LocalLogic() {
        if (firstFrame) {
            Main.Ins.DialogStateManager.ChangeState(null);//关闭loading条
            firstFrame = false;
        }
        deltaTime = Time.deltaTime;//本来之前是固定逻辑帧帧率的，但是失败了
        time = Time.time;
        LogicFrame();
    }

    public void NetUpdate() {
        if (NetObjects.Count != 0) {
            for (int i = 0; i < NetObjects.Count; i++)
                if (NetObjects[i] != null)
                    NetObjects[i].NetUpdate();
        }
    }

    public void NetLateUpdate() {
        if (NetObjects.Count != 0) {
            for (int i = 0; i < NetObjects.Count; i++)
                if (NetObjects[i] != null)
                    NetObjects[i].NetLateUpdate();
        }
    }

    public delegate void OnUpdate();
    public event OnUpdate OnUpdates;//在战斗还未开始时

    private void LogicFrame() {
        NetUpdate();
        NetLateUpdate();

        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {
            FrameSyncServer.Ins.SyncCommand(Mathf.FloorToInt(Time.deltaTime * 1000));
            FrameSyncLocal.Ins.SyncEnterPlayer();
        }
    }
}
