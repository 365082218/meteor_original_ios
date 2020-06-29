
using ProtoBuf;
using protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

//处理与服务器的Udp链接，当进入房间服务器时
class UdpClientProxy
{
    private static IPEndPoint Remote;
    private static UdpClient Client;
    private static KCP Kcp;
    static PacketProxy PacketProxy;
    private static float UpdateTime;
    private static bool WillDisconnect;
    public static Dictionary<int, byte[]> Packet;
    public static void Init()
    {
        Packet = new Dictionary<int, byte[]>();
        //注册解析udp数据包.
        ProtoHandler.RegisterPacket(Packet);
    }

    public static bool Connected
    {
        get;
        private set;
    }

    public static bool Connect(int port, int playerId)
    {
        if (Connected)
        {
            return false;
        }

        Client = new UdpClient(TcpClientProxy.server.Address.ToString(), port);
        Kcp = new KCP((uint)playerId, SendWrap);
        Kcp.NoDelay(1, 10, 2, 1);
        Kcp.WndSize(128, 128);
        Receive();
        UpdateTime = 0;
        Connected = true;
        return true;
    }

    public static void Update()
    {
        if (!Connected)
        {
            return;
        }

        if (WillDisconnect)
        {
            Disconnect();
            WillDisconnect = false;

            return;
        }

        UpdateTime += FrameReplay.deltaTime;
        Kcp.Update((uint)Mathf.FloorToInt(UpdateTime * 1000));

        for (var size = Kcp.PeekSize(); size > 0; size = Kcp.PeekSize())
        {
            var buffer = new byte[size];
            int n = Kcp.Recv(buffer);
            if (n > 0)
            {
                PacketProxy.SetBuffer(buffer);
                PacketProxy.Analysis(n, Packet);
            }
        }
    }

    public static bool Disconnect()
    {
        if (!Connected)
        {
            return false;
        }
        Connected = false;
        Client.Close();
        return true;
    }

    private static void Receive()
    {
        Client.BeginReceive(ReceiveCallback, null);
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            var data = Client.EndReceive(ar, ref Remote);

            if (data != null)
            {
                Kcp.Input(data);
            }

            Receive();
        }
        catch (SocketException)
        {
            WillDisconnect = true;
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        Client.EndSend(ar);
    }

    public static void Exec(int msg)
    {
        byte[] Length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(8));
        byte[] wIdent = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msg));
        byte[] data = new byte[8];
        Buffer.BlockCopy(Length, 0, data, 0, 4);
        Buffer.BlockCopy(wIdent, 0, data, 4, 4);
        try
        {
            Kcp.Send(data);
        }
        catch (Exception exp)
        {
            Log.WriteError(exp.Message);
        }
    }

    //发送UDP帧同步包-主消息synccommand
    public static void Exec<T>(int msg, T rsp)
    {
        MemoryStream ms = new MemoryStream();
        Serializer.Serialize(ms, rsp);
        byte[] coreData = ms.ToArray();
        int length = 8 + coreData.Length;
        byte[] data = new byte[length];
        Buffer.BlockCopy(coreData, 0, data, 8, coreData.Length);
        byte[] Length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(length));
        byte[] wIdent = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msg));
        Buffer.BlockCopy(Length, 0, data, 0, 4);
        Buffer.BlockCopy(wIdent, 0, data, 4, 4);
        try
        {
            Kcp.Send(data);
        }
        catch (Exception exp)
        {
            Log.WriteError(exp.Message);
        }
    }

    private static void SendWrap(byte[] data, int size)
    {
        try
        {
            Client.BeginSend(data, size, SendCallback, null);
        }
        catch (SocketException)
        {
            Disconnect();
        }
    }

    public static void EnterLevel(int model, int weapon, int camp)
    {
        PlayerEventData req = new PlayerEventData();
        req.camp = (uint)camp;//暂时全部为盟主模式
        req.model = (uint)model;
        req.weapon = (uint)weapon;
        req.playerId = (uint)Main.Ins.NetWorkBattle.PlayerId;
        Exec((int)MeteorMsg.Command.SpawnPlayer, req);
    }

    public static void LeaveLevel()
    {
        PlayerEventData req = new PlayerEventData();
        req.camp = (uint)Main.Ins.NetWorkBattle.camp;//暂时全部为盟主模式
        req.model = (uint)Main.Ins.NetWorkBattle.heroIdx;
        req.weapon = (uint)Main.Ins.NetWorkBattle.weaponIdx;
        req.playerId = (uint)Main.Ins.NetWorkBattle.PlayerId;
        Exec((int)MeteorMsg.Command.DestroyPlayer, req);
        Main.Ins.NetWorkBattle.OnDisconnect();
    }
}

//处理与服务器的TCP连接.
class TcpClientProxy
{
    public static bool quit = false;
    //public static AutoResetEvent logicEvent = new AutoResetEvent(false);//负责收到服务器后的响应线程的激活.
    public static IPEndPoint server;
    public static PacketProxy proxy;
    public static Dictionary<int, byte[]> Packet = new Dictionary<int, byte[]>();//消息ID和字节流
    static Timer tConn;

    public static void Init()
    {
        ProtoHandler.RegisterPacket(Packet);
    }
    const int delay = 10000;
    public static void ReStart()
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

    public static void TryConn(object param)
    {
        if (sProxy != null && sProxy.Connected)
        {
            if (tConn != null)
                tConn.Change(Timeout.Infinite, Timeout.Infinite);
            return;
        }
        sProxy.BeginConnect(server, OnTcpConnect, sProxy);
        if (tConn != null)
            tConn.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public static void OnTcpConnect(IAsyncResult ret)
    {
        LocalMsg result = new LocalMsg();
        try
        {
            if (sProxy != null)
                sProxy.EndConnect(ret);
            if (tConn != null)
                tConn.Change(Timeout.Infinite, Timeout.Infinite);
        }
        catch (Exception exp)
        {
            Debug.LogError(exp.Message);
            result.Message = (int)LocalMsgType.Connect;
            result.message = exp.Message;
            result.Result = 0;
            ProtoHandler.PostMessage(result);
            if (tConn != null)
                tConn.Change(delay, delay);
            return;
        }

        //被关闭了的.
        if (sProxy == null)
            return;

        result.Message = (int)LocalMsgType.Connect;
        result.Result = 1;
        ProtoHandler.PostMessage(result);
        try
        {
            sProxy.BeginReceive(proxy.GetBuffer(), 0, PacketProxy.PacketSize, SocketFlags.None, OnReceivedData, sProxy);
        }
        catch
        {
            result.Message = (int)LocalMsgType.DisConnect;
            result.Result = 0;
            ProtoHandler.PostMessage(result);
            sProxy.Close();
            proxy.Reset();
            if (tConn != null)
                tConn.Change(5000, 5000);
        }
    }

    static void OnReceivedData(IAsyncResult ar)
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
                ProtoHandler.PostMessage(msg);
                if (sProxy.Connected)
                    sProxy.Close();
                proxy.Reset();
                if (tConn != null)
                    tConn.Change(5000, 5000);
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
                ProtoHandler.PostMessage(msg);
                sProxy.Close();
                proxy.Reset();
                if (tConn != null)
                    tConn.Change(5000, 5000);
            }
        }
    }

    //把从联机获取的服务器列表和本地自定义的服务器列表合并.
    static void InitServerCfg()
    {
        if (server == null)
        {
            try
            {
                int port = 0;
                port = Main.Ins.CombatData.Server.ServerPort;
                if (Main.Ins.CombatData.Server.type == 1)
                {
                    IPAddress address = IPAddress.Parse(Main.Ins.CombatData.Server.ServerIP);
                    server = new IPEndPoint(address, port);
                }
                else
                {
                    IPAddress[] addr = Dns.GetHostAddresses(Main.Ins.CombatData.Server.ServerHost);
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

    public static Socket sProxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //检查链接是否超过3次失败，超过时
    public static void CheckNeedReConnect()
    {
        //如果没连接上，或者尝试已终止
        if (quit || sProxy == null || !sProxy.Connected)
        {
            ReStart();
        }
    }

    public static void Exit()
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

    public static void UploadRecord(GameRecord record) {
        Common.SendRecord(record);
    }

    //发出的.
    //网关服务器从中心服务器取得游戏服务器列表.
    public static void UpdateGameServer()
    {
        Common.SendUpdateGameServer();
    }

    public static void AutoLogin()
    {
        Common.SendAutoLogin();
    }

    public static void JoinRoom(int roomId)
    {
        Common.SendJoinRoom(roomId);
    }
}