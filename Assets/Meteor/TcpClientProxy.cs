
using ProtoBuf;
using protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

//处理与服务器的TCP连接.
class TcpClientProxy:Singleton<TcpClientProxy>
{
    public int ping;
    public float startPing;
    public float endPing;
    public bool quit = false;
    //public static AutoResetEvent logicEvent = new AutoResetEvent(false);//负责收到服务器后的响应线程的激活.
    public IPEndPoint server;
    public PacketProxy proxy;
    public TcpProtoHandler TcpHandler;
    public SortedDictionary<int, byte[]> Packet;
    static Timer tConn;

    public void Init()
    {
        Packet = new SortedDictionary<int, byte[]>();//消息ID和字节流
        TcpHandler = TcpProtoHandler.Ins;
        TcpHandler.RegisterPacket(Packet);
    }
    const int delay = 10000;
    public void ReStart()
    {
        quit = false;

        if (sProxy == null)
            sProxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        if (proxy == null)
            proxy = new PacketProxy();

        InitServerCfg();
        if (tConn == null)
            tConn = new Timer(TryConn, null, 0, 5000);
    }

    bool canceled = false;
    public void TryConn(object param)
    {
        if (sProxy != null && sProxy.Connected)
        {
            if (tConn != null)
                tConn.Change(Timeout.Infinite, Timeout.Infinite);
            return;
        }
        canceled = false;

        if (tConn != null)
            tConn.Change(Timeout.Infinite, Timeout.Infinite);
        IAsyncResult ar = sProxy.BeginConnect(server, OnTcpConnect, sProxy);
        bool success = ar.AsyncWaitHandle.WaitOne(2000);
        if (!success) {
            LocalMsg result = new LocalMsg();
            result.Message = (int)LocalMsgType.TimeOut;
            result.message = "服务器未响应";
            result.Result = 0;
            TcpHandler.PostMessage(result);
            canceled = true;//超时连接取消重试.
            if (tConn != null)
                tConn.Change(Timeout.Infinite, Timeout.Infinite);
            return;
        }
    }

    public void OnTcpConnect(IAsyncResult ret)
    {
        LocalMsg result = new LocalMsg();
        try
        {
            if (sProxy != null)
                sProxy.EndConnect(ret);
            
        }
        catch (Exception exp)
        {
            if (canceled) {
                return;
            }
            //Debug.LogError(exp.Message);
            result.Message = (int)LocalMsgType.Connect;
            result.message = exp.Message;
            result.Result = 0;
            TcpHandler.PostMessage(result);
            return;
        }

        //被关闭了的.
        if (sProxy == null)
            return;

        if (canceled) {
            sProxy.Close();
            sProxy = null;
            return;
        }
        result.Message = (int)LocalMsgType.Connect;
        result.Result = 1;
        lock (Packet) {
            Packet.Clear();
        }
        TcpHandler.PostMessage(result);
        try
        {
            sProxy.BeginReceive(proxy.GetBuffer(), 0, PacketProxy.PacketSize, SocketFlags.None, OnReceivedData, sProxy);
        }
        catch
        {
            result.Message = (int)LocalMsgType.DisConnect;
            result.Result = 0;
            TcpHandler.PostMessage(result);
            sProxy.Close();
            proxy.Reset();
        }
    }

    void OnReceivedData(IAsyncResult ar)
    {
        int len = 0;
        try
        {
            len = sProxy.EndReceive(ar);
        }
        catch
        {
                
        }
        if (len <= 0)
        {
            if (!quit)
            {
                LocalMsg msg = new LocalMsg();
                msg.Message = (int)LocalMsgType.DisConnect;
                msg.Result = 1;
                TcpHandler.PostMessage(msg);
                if (sProxy.Connected)
                    sProxy.Close();
                proxy.Reset();
            }
            if (proxy != null)
                proxy.Reset();
            return;
        }

        lock (Packet)
        {
            if (!proxy.Analysis(len, Packet))
            {
                sProxy.Close();
                proxy.Reset();
                return;
            }
        }
        //logicEvent.Set();

        if (!quit)
        {
            try
            {
                sProxy.BeginReceive(proxy.GetBuffer(), 0, PacketProxy.PacketSize, SocketFlags.None, OnReceivedData, sProxy);
            }
            catch
            {
                LocalMsg msg = new LocalMsg();
                msg.Message = (int)LocalMsgType.DisConnect;
                msg.Result = 1;
                TcpHandler.PostMessage(msg);
                sProxy.Close();
                proxy.Reset();
            }
        }
    }

    //把从联机获取的服务器列表和本地自定义的服务器列表合并.
    void InitServerCfg()
    {
        if (server == null)
        {
            try
            {
                int port = 0;
                port = CombatData.Ins.Server.ServerPort;
                if (CombatData.Ins.Server.type == 1)
                {
                    IPAddress address = IPAddress.Parse(CombatData.Ins.Server.ServerIP);
                    server = new IPEndPoint(address, port);
                }
                else
                {
                    IPAddress[] addr = Dns.GetHostAddresses(CombatData.Ins.Server.ServerHost);
                    if (addr.Length != 0)
                        server = new IPEndPoint(addr[0], port);
                }
            }
            catch
            {
                //单机时,或者网址dns无法解析时.
            }
        }
    }

    public Socket sProxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //检查链接是否超过3次失败，超过时
    public bool CheckNeedReConnect()
    {
        //如果没连接上，或者尝试已终止
        if (quit || sProxy == null || !sProxy.Connected)
        {
            Exit();
            ReStart();
            return true;
        }
        return false;
    }

    public void Exit()
    {
        quit = true;

        if (sProxy != null)
        {
            try
            {
                sProxy.Shutdown(SocketShutdown.Both);
                sProxy.Close();
            }
            catch
            {
                
            }
            finally
            {
                sProxy = null;
            }
        }
        if (proxy != null)
            proxy.Reset();

        if (tConn != null)
        {
            tConn.Dispose();
            tConn = null;
        }

        server = null;
    }

    //发出的.
    //网关服务器从中心服务器取得游戏服务器列表.
    public void UpdateGameServer()
    {
        Exec(sProxy, (int)protocol.MeteorMsg.MsgType.GetRoomReq);
    }

    public void AutoLogin()
    {
        ProtocolVerifyReq req = new ProtocolVerifyReq();
        req.version = AppInfo.ProtocolVersion;
        Exec(sProxy, (int)protocol.MeteorMsg.MsgType.ProtocolVerify, req);
    }

    public void JoinRoom(int roomId, string sec = "")
    {
        RoomMng.Ins.SelectRoom(roomId);
        JoinRoomReq req = new JoinRoomReq();
        req.roomId = (uint)roomId;
        req.version = Main.Ins.AppInfo.MeteorV2();
        req.password = sec;
        req.nick = GameStateMgr.Ins.gameStatus.NickName;
        for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginModel.Count; i++) {
            GameStateMgr.Ins.gameStatus.pluginModel[i].Check();
            if (GameStateMgr.Ins.gameStatus.pluginModel[i].Installed)
                req.models.Add((uint)GameStateMgr.Ins.gameStatus.pluginModel[i].ModelId);
        }
        Exec(sProxy, (int)protocol.MeteorMsg.MsgType.JoinRoomReq, req);//进入房间-还未进入战场，战前准备阶段
    }


    public void SendChatMessage(string message) {
        ChatMsg msg = new ChatMsg();
        msg.chatMessage = message;
        msg.playerId = (uint)NetWorkBattle.Ins.PlayerId;
        Exec(sProxy, (int)MeteorMsg.MsgType.ChatInRoomReq, msg);
    }

    //public void SendAudioMessage(byte[] data) {
    //    AudioChatMsg msg = new AudioChatMsg();
    //    msg.type = 0;
    //    msg.audio_data = data;
    //    msg.playerId = (uint)NetWorkBattle.Ins.PlayerId;
    //    Exec(sProxy, (int)MeteorMsg.MsgType.AudioChat, msg);
    //    //客户端直接收到自己的发言，其他人的发言通过网络
    //    RoomChatDialogState.State.Open();
    //    RoomChatDialogState.Instance.Add((int)msg.playerId, msg.audio_data);
    //}

    public void Exec(Socket s, int msg) {
        if (s != null && s.Connected) {
            startPing = Time.time;
            byte[] Length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(8));
            byte[] wIdent = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msg));
            byte[] data = new byte[8];
            Buffer.BlockCopy(Length, 0, data, 0, 4);
            Buffer.BlockCopy(wIdent, 0, data, 4, 4);
            try {
                s.Send(data, 8, SocketFlags.None);
            } catch (Exception exp) {
                Log.WriteError(exp.Message);
            }
        }
    }

    public void Exec<T>(Socket s, int msg, T rsp) {
        //UnityEngine.Debug.LogError("send msg:" + msg);
        if (s != null && s.Connected) {
            startPing = Time.time;
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize<T>(ms, rsp);
            byte[] coreData = ms.ToArray();
            int length = 8 + coreData.Length;
            byte[] data = new byte[length];
            Buffer.BlockCopy(coreData, 0, data, 8, coreData.Length);
            byte[] Length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length));
            byte[] wIdent = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msg));
            Buffer.BlockCopy(Length, 0, data, 0, 4);
            Buffer.BlockCopy(wIdent, 0, data, 4, 4);
            try {
                //UnityEngine.Debug.LogError("send msg 2:" + msg);
                s.Send(data, length, SocketFlags.None);
            } catch (Exception exp) {
                Log.WriteError(exp.Message);
            }
        }
    }

    public void HeartBeat() {
        //U3D.InsertSystemMsg("收到心跳包，开始应答");
        Exec(sProxy, (int)protocol.MeteorMsg.MsgType.AliveUpdate);
    }

    //这个消息是进入战场前最后一个TCP消息，之后只有UDP包会收到消息.
    public void EnterLevel(int model, int weapon, int camp) {
        PlayerEvent req = new PlayerEvent();
        req.camp = (uint)camp;//暂时全部为盟主模式
        req.model = (uint)model;
        req.weapon = (uint)weapon;
        req.playerId = (uint)NetWorkBattle.Ins.PlayerId;
        req.name = GameStateMgr.Ins.gameStatus.NickName;
        Exec(sProxy, (int)MeteorMsg.MsgType.EnterLevelReq, req);//TCP
    }

    public void LeaveLevel() {
        PlayerEvent req = new PlayerEvent();
        req.camp = (uint)NetWorkBattle.Ins.camp;//暂时全部为盟主模式
        req.model = (uint)NetWorkBattle.Ins.heroIdx;
        req.weapon = (uint)NetWorkBattle.Ins.weaponIdx;
        req.playerId = (uint)NetWorkBattle.Ins.PlayerId;
        Exec(sProxy, (int)MeteorMsg.MsgType.LeaveLevelReq, req);
        NetWorkBattle.Ins.OnDisconnect();
    }

    //创建房间.
    public void CreateRoom(string name, string sec) {
        CreateRoomReq req = new CreateRoomReq();
        req.hpMax = (uint)GameStateMgr.Ins.gameStatus.NetWork.Life;
        //章节编号×1000 + 关卡序号
        req.levelIdx = (uint)GameStateMgr.Ins.gameStatus.NetWork.ChapterTemplate * 1000 + (uint)GameStateMgr.Ins.gameStatus.NetWork.LevelTemplate;
        req.maxPlayer = (uint)GameStateMgr.Ins.gameStatus.NetWork.MaxPlayer;
        req.roomName = name;
        req.roundTime = (uint)GameStateMgr.Ins.gameStatus.NetWork.RoundTime;
        req.rule = (RoomInfo.RoomRule)GameStateMgr.Ins.gameStatus.NetWork.Mode;
        req.secret = sec;
        req.version = Main.Ins.AppInfo.MeteorV2();//107 907
        req.pattern = (RoomInfo.RoomPattern)GameStateMgr.Ins.gameStatus.NetWork.Pattern;
        if (req.pattern == RoomInfo.RoomPattern._Replay) {
            //向服务器发送某个录像文件
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, GameStateMgr.Ins.gameStatus.NetWork.record);
            req.replay_data = ms.ToArray();
        } else if (req.pattern == RoomInfo.RoomPattern._Normal) {
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginModel.Count; i++) {
                GameStateMgr.Ins.gameStatus.pluginModel[i].Check();
                if (GameStateMgr.Ins.gameStatus.pluginModel[i].Installed)
                    req.models.Add((uint)GameStateMgr.Ins.gameStatus.pluginModel[i].ModelId);
            }
        }
        //把本地的武器ID，模型ID传过去，其他人进入房间后，选择角色或者武器，就受到房间此信息限制
        //req.weapons.Add();
        //只包含外接模型-基础0-19无论如何都可以使用.
        //int total = GameStateMgr.Ins.gameStatus.pluginModel.Count;
        //for (int i = 0; i < total; i++)
        //    req.models.Add((uint)GameStateMgr.Ins.gameStatus.pluginModel[i].ModelId);
        Exec(sProxy, (int)MeteorMsg.MsgType.CreateRoomReq, req);
        //1,人数上限
        //2.关卡模式
        //3.时长
        //4.地图模板
        //5.生命上限
        //6.禁用远程武器
    }
}