using CoClass;
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

//处理与服务器的连接.没有p2p
class ClientProxy
{
    public static bool quit = false;
    //public static AutoResetEvent logicEvent = new AutoResetEvent(false);//负责收到服务器后的响应线程的激活.
    public static IPEndPoint server;
    public static TcpProxy proxy;
    public static Dictionary<int, byte[]> Packet = new Dictionary<int, byte[]>();//消息ID和字节流
    static Timer tConn;

    public static void Init()
    {
        quit = false;

        if (sProxy == null)
            sProxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        if (proxy == null)
            proxy = new TcpProxy();

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

        result.Message = (int)LocalMsgType.Connect;
        result.Result = 1;
        ProtoHandler.PostMessage(result);

        try
        {
            sProxy.BeginReceive(proxy.GetBuffer(), 0, TcpProxy.PacketSize, SocketFlags.None, OnReceivedData, sProxy);
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
                sProxy.BeginReceive(proxy.GetBuffer(), 0, TcpProxy.PacketSize, SocketFlags.None, OnReceivedData, sProxy);
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

    static void InitServerCfg()
    {
        if (server == null)
        {
            try
            {
                int port = 0;
                if (GameData.Instance.gameStatus.defaultServerIdx >= GameData.Instance.gameStatus.ServerList.Count)
                    GameData.Instance.gameStatus.defaultServerIdx = 0;
                port = GameData.Instance.gameStatus.ServerList[GameData.Instance.gameStatus.defaultServerIdx].ServerPort;
                if (GameData.Instance.gameStatus.ServerList[GameData.Instance.gameStatus.defaultServerIdx].type == 1)
                {
                    IPAddress address = IPAddress.Parse(GameData.Instance.gameStatus.ServerList[GameData.Instance.gameStatus.defaultServerIdx].ServerIP);
                    server = new IPEndPoint(address, port);
                }
                else
                {
                    IPAddress[] addr = Dns.GetHostAddresses(GameData.Instance.gameStatus.ServerList[GameData.Instance.gameStatus.defaultServerIdx].ServerHost);
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

    public static void OnLogout(uint userid, Action<RBase> cb)
    {
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

    //加入大厅，与服务器验证，否则一段时间内服务器关闭掉未验证客户端
    public static void JoinLobbyRequest()
    {
        Common.SendJoinLobbyRequest();
    }

    public static void JoinRoom(int roomId)
    {
        Common.SendJoinRoom(roomId);
    }

    public static void ReqReborn(int userId)
    {
        Common.SendRebornRequest(userId);
    }

    public static void EnterLevel(int model, int weapon)
    {
        Common.SendEnterLevel(model, weapon);
    }

    public static void LeaveLevel()
    {
        Common.SendLeaveLevel();
        NetWorkBattle.Ins.OnDisconnect();
    }

    //同步当前帧的输入状态
    public static void SyncInput()
    {

    }

    //同步当前角色状态,定点数，位置 + 旋转 + 帧动画 + 时刻
    public static void SyncFrame()
    {

    }
    //public static void SendBattleResult(bool result, int battleId, List<int> monster, Action<RBase> cb = null)
    //{
    //    try
    //    {
    //        InitServerCfg();
    //        Common.SendBattleResult(result, battleId, monster, cb);
    //    }
    //    catch (Exception exp)
    //    {
    //        Console.WriteLine("socket error");
    //    }
    //}
}