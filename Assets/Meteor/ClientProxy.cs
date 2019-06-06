
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

//处理与服务器的Udp链接，当进入房间服务器时
class UdpClientProxy
{
    private static IPEndPoint remote;
    private static UdpClient udp;
    private static KCP kcp;
    static PacketProxy packetProxy;
    private static float updateTime;
    private static bool willDisconnect;
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

    public static bool Connect(int port)
    {
        if (Connected)
        {
            return false;
        }

        udp = new UdpClient(TcpClientProxy.server.Address.ToString(), port);
        kcp = new KCP(1, SendWrap);
        kcp.NoDelay(1, 10, 2, 1);
        kcp.WndSize(128, 128);
        Receive();
        updateTime = 0;
        Connected = true;
        
        return true;
    }

    public static void Update()
    {
        if (!Connected)
        {
            return;
        }

        if (willDisconnect)
        {
            Disconnect();
            willDisconnect = false;

            return;
        }

        updateTime += FrameReplay.deltaTime;
        kcp.Update((uint)Mathf.FloorToInt(updateTime * 1000));

        for (var size = kcp.PeekSize(); size > 0; size = kcp.PeekSize())
        {
            var buffer = new byte[size];
            int n = kcp.Recv(buffer);
            if (n > 0)
            {
                packetProxy.SetBuffer(buffer);
                packetProxy.Analysis(n, Packet);
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
        udp.Close();
        return true;
    }

    private static void Receive()
    {
        udp.BeginReceive(ReceiveCallback, null);
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            var data = udp.EndReceive(ar, ref remote);

            if (data != null)
            {
                kcp.Input(data);
            }

            Receive();
        }
        catch (SocketException)
        {
            willDisconnect = true;
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        udp.EndSend(ar);
    }

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
            kcp.Send(data);
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
            udp.BeginSend(data, size, SendCallback, null);
        }
        catch (SocketException)
        {
            Disconnect();
        }
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
                tConn.Change(5000, 5000);
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
                port = 7201;//Global.Instance.Server.ServerPort;
                if (Global.Instance.Server.type == 1)
                {
                    IPAddress address = IPAddress.Parse(Global.Instance.Server.ServerIP);
                    server = new IPEndPoint(address, port);
                }
                else
                {
                    IPAddress[] addr = Dns.GetHostAddresses(Global.Instance.Server.ServerHost);
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

    public static void EnterLevel(int model, int weapon, int camp)
    {
        Common.SendEnterLevel(model, weapon, camp);
    }

    public static void LeaveLevel()
    {
        Common.SendLeaveLevel();
        NetWorkBattle.Instance.OnDisconnect();
    }
}