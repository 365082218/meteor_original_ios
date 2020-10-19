using cocosocket4unity;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public interface KcpListener {
    void handleReceive(ByteBuf bb);
    void handleException(Exception exp);
}

//处理与服务器的Udp链接，当进入房间服务器时
class KcpClient : Singleton<KcpClient>, KcpListener {
    private IPEndPoint Remote;
    private KcpOnUdp kcp;
    PacketProxy PacketProxy;
    UdpProtoHandler UdpHandler;
    private bool WillDisconnect;
    public SortedDictionary<int, byte[]> Packet;
    public void Init() {
        Packet = new SortedDictionary<int, byte[]>();
        PacketProxy = new PacketProxy();
        UdpHandler = UdpProtoHandler.Ins;
        //注册解析udp数据包.
        UdpHandler.RegisterPacket(Packet);
    }

    public static bool Connected {
        get;
        private set;
    }

    public bool Connect(int port, int playerId) {
        if (Connected) {
            return false;
        }

        kcp = new KcpOnUdp(TcpClientProxy.Ins.server.Address.ToString(), port, playerId, this);
        kcp.Connect(TcpClientProxy.Ins.server.Address.ToString(), port);
        Connected = true;
        return true;
    }

    public void Update() {
        if (!Connected) {
            return;
        }
        if (WillDisconnect) {
            Disconnect();
            return;
        }
        kcp.Update();
    }

    public bool Disconnect() {
        if (!Connected) {
            return false;
        }
        
        Connected = false;
        kcp.DisConnect();
        Packet.Clear();
        return true;
    }

    public void Exec(int msg) {
        byte[] Length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(8));
        byte[] wIdent = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(msg));
        byte[] data = new byte[8];
        Buffer.BlockCopy(Length, 0, data, 0, 4);
        Buffer.BlockCopy(wIdent, 0, data, 4, 4);
        ByteBuf bb = new ByteBuf(data);
        try {
            lock (this) {
                kcp.Send(bb);
            }
        } catch (Exception exp) {
            Log.WriteError(exp.Message);
        }
    }

    //发送UDP帧同步包-主消息synccommand
    public void Exec<T>(int msg, T rsp) {
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
        ByteBuf bb = new ByteBuf(data);
        try {
            lock (this) {
                kcp.Send(bb);
            }
        } catch (Exception exp) {
            Log.WriteError(exp.Message);
        }
    }

    public void handleReceive(ByteBuf bb) {
        PacketProxy.SetBuffer(bb.GetRaw());
        PacketProxy.Analysis(bb.ReadableBytes(), Packet);
    }

    public void handleException(Exception exp) {
        WillDisconnect = true;
    }

    public void handleClose(KcpOnUdp kcp, int errorCode) {

    }
}