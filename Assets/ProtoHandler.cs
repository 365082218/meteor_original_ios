using CoClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using protocol;
//仅本机的，因为tcp链接一部分代码在其他线程返回，故这里要推到数据结构等主线程来取.
public enum LocalMsgType
{
    Connect,
    DisConnect,
    SendFTPLogComplete,//全部日志发送完成
    GameStart,//下载中断，进入Startup场景
}

public class LocalMsg
{
    public int Message;
    public int Result;
    public int Param;
}

class ProtoHandler
{
    //套接口消息.
    public static void Update()
    {
        //处理网络消息
        lock (ClientProxy.Packet)
        {
            MemoryStream ms = null;
            foreach (var each in ClientProxy.Packet)
            {
                switch (each.Key)
                {
                    case (int)MeteorMsg.MsgType.GetRoomRsp:
                        ms = new MemoryStream(each.Value);
                        GetRoomRsp rspG = ProtoBuf.Serializer.Deserialize<GetRoomRsp>(ms);
                        OnGetRoomRsp(rspG);
                        break;
                    case (int)MeteorMsg.MsgType.JoinRoomRsp:
                        ms = new MemoryStream(each.Value);
                        JoinRoomRsp rspJ = ProtoBuf.Serializer.Deserialize<JoinRoomRsp>(ms);
                        OnJoinRoomRsp(rspJ);
                        break;
                    case (int)MeteorMsg.MsgType.OnJoinRoomRsp:
                        ms = new MemoryStream(each.Value);
                        OnEnterRoomRsp rspE = ProtoBuf.Serializer.Deserialize<OnEnterRoomRsp>(ms);
                        OnEnterRoomRsp_(rspE);
                        break;
                    case (int)MeteorMsg.MsgType.CreateRoomRsp:
                        ms = new MemoryStream(each.Value);
                        CreateRoomRsp rspC = ProtoBuf.Serializer.Deserialize<CreateRoomRsp>(ms);
                        OnCreateRoomRsp(rspC);
                        break;
                    case (int)MeteorMsg.MsgType.EnterLevelRsp:
                        ms = new MemoryStream(each.Value);
                        EnterLevelRsp rspER = ProtoBuf.Serializer.Deserialize<EnterLevelRsp>(ms);
                        EnterLevelRsp_(rspER);
                        break;
                    case (int)MeteorMsg.MsgType.OnEnterLevelRsp:
                        ms = new MemoryStream(each.Value);
                        OnEnterLevelRsp rspOE = ProtoBuf.Serializer.Deserialize<OnEnterLevelRsp>(ms);
                        OnEnterLevelRsp_(rspOE);
                        break;
                    case (int)MeteorMsg.MsgType.OnLeaveRoomRsp:
                        ms = new MemoryStream(each.Value);
                        OnLeaveRoomRsp rspL = ProtoBuf.Serializer.Deserialize<OnLeaveRoomRsp>(ms);
                        OnLevvaRoomRsp_(rspL);
                        break;
                    case (int)MeteorMsg.MsgType.SyncInput:
                        ms = new MemoryStream(each.Value);
                        InputReq InputRsp = ProtoBuf.Serializer.Deserialize<InputReq>(ms);
                        OnSyncInputRsp(InputRsp);
                        break;
                    case (int)MeteorMsg.MsgType.SyncKeyFrame:
                        ms = new MemoryStream(each.Value);
                        KeyFrame KeyFrameRsp = ProtoBuf.Serializer.Deserialize<KeyFrame>(ms);
                        OnSyncKeyFrame(KeyFrameRsp);
                        break;
                }
            }
            ClientProxy.Packet.Clear();
        }

        lock (messageQueue)
        {
            int length = messageQueue.Count;
            for (int i = 0; i < length; i++)
            {
                switch (messageQueue[i].Message)
                {
                    case (short)LocalMsgType.Connect: OnConnect(messageQueue[i].Result);break;
                    case (short)LocalMsgType.DisConnect: OnDisconnect();break;
                    case (short)LocalMsgType.SendFTPLogComplete: OnSendComplete(messageQueue[i].Result, messageQueue[i].Param);break;
                    case (short)LocalMsgType.GameStart:OnGameStart();break;
                }
            }
            messageQueue.Clear();
        }
    }
    //跨线程访问
    public static List<LocalMsg> messageQueue = new List<LocalMsg>();
    public static void PostMessage(LocalMsg msg)
    {
        lock (messageQueue)
            messageQueue.Add(msg);
    }

    static void OnPlayerInsert(RBase req)
    {
        PlayerEnterMap insert = req as PlayerEnterMap;
        if (insert != null)
        {
            //把场景上的此玩家删除掉.
            string strTip = "玩家:";
            foreach (var each in insert.insertPlayer)
            {
                strTip += each.Value.Name + " ";
            }
            strTip += "进入";
            U3D.PopupTip(strTip);
            //insert.insertPlayer;
        }
    }

    static void OnPlayerLeave(RBase req)
    {
        PlayerEnterMap remove = req as PlayerEnterMap;
        if (remove != null)
        {
            //把场景上的此玩家删除掉.
            string strTip = "玩家:";
            foreach (var each in remove.insertPlayer)
            {
                strTip += each.Value.Name + " ";
            }
            strTip += "离开";
            U3D.PopupTip(strTip);
            //insert.insertPlayer;
        }
    }

    //同步关键帧，全部角色的属性，都设置一次
    static void OnSyncKeyFrame(KeyFrame frame)
    {

    }

    //同步服务端发来的其他玩家的按键
    static void OnSyncInputRsp(InputReq rsp)
    {

    }

    static void OnLevvaRoomRsp_(OnLeaveRoomRsp rsp)
    {
        //rsp.playerId
    }

    static void OnEnterLevelRsp_(OnEnterLevelRsp rsp)
    {

    }

    static void EnterLevelRsp_(EnterLevelRsp rsp)
    {
        //进入到房间内,开始加载场景设置角色初始化位置
        U3D.LoadNetLevel(rsp.scene.items, rsp.scene.players);
    }

    //自己建房间成功，则转到选人界面.与加入房间一个德行
    static void OnCreateRoomRsp(CreateRoomRsp rsp)
    {
        if (rsp.result == 1)
        {
            GameOverlayWnd.Instance.InsertSystemMsg(string.Format("创建房间 编号:{0}", rsp.roomId));
            RoleSelectWnd.Instance.Open();
        }
        else
        {
            GameOverlayWnd.Instance.InsertSystemMsg("创建房间失败");
        }
    }

    //其他人进入我所在的房间
    static void OnEnterRoomRsp_(OnEnterRoomRsp rsp)
    {
        //显示某某进入房间的文字
        GameOverlayWnd.Instance.InsertSystemMsg(string.Format("{0} 进入房间", rsp.playerNick));
    }

    static void OnJoinRoomRsp(JoinRoomRsp rsp)
    {
        //如果规则是暗杀或者死斗
        //自己进入房间成功时的信息,跳转到选角色界面，角色选择，就跳转到武器选择界面
        //到最后一步确认后，开始同步服务器场景数据.
        if (rsp.result == 1)
        {
            if (NetWorkBattle.Ins.RoomId == -1)
            {
                //选人，或者阵营，或者
                UnityEngine.Debug.LogError("OnJoinRoom successful");
                if (MainLobby.Instance != null)
                    MainLobby.Instance.Close();
                NetWorkBattle.Ins.OnEnterRoomSuccessed((int)rsp.roomId, (int)rsp.levelIdx, (int)rsp.playerId);
                RoleSelectWnd.Instance.Open();//阵营选择,最后一步，调用进入Level,那个时候再加载场景之类.
            }
            //U3D.LoadNetLevel((int)rsp.levelIdx, LevelMode.MultiplyPlayer, GameMode.MENGZHU);
        }
        else
        {
            //显示各种错误信息框
            //rsp.reason
            //2未找到
            //3需要退出当前房间
            //1房间满
            UnityEngine.Debug.LogError(string.Format("error:{0}", rsp.reason));
            switch (rsp.reason)
            {
                case 1:U3D.PopupTip("此房间已满，无法进入");break;
                case 2:U3D.PopupTip("房间已解散");break;
                case 3:U3D.PopupTip("需要先退出房间");break;
            }
        }
    }

    static void OnGetRoomRsp(GetRoomRsp rsp)
    {
        if (MainLobby.Exist)
            MainLobby.Instance.OnGetRoom(rsp);
    }

    static void OnConnect(int result)
    {
        //取得房间信息
        if (result == 1)
            ClientProxy.UpdateGameServer();
        else
        {
            //链接失败,重置对战
            NetWorkBattle.Ins.OnDisconnect();
        }
    }

    //断开链接,退出场景，返回
    static void OnDisconnect()
    {
        NetWorkBattle.Ins.OnDisconnect();
        U3D.GoBack();
    }

    static void OnSendComplete(int result, int sendFileCount)
    {
        if (result == 1)
            U3D.PopupTip("日志上传完毕 成功发送" + sendFileCount + "个文件");
        else
            U3D.PopupTip("日志上传失败");
    }

    static void OnGameStart()
    {
        Main.Ins.GameStart();
    }
}
