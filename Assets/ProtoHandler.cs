
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using protocol;
using UnityEngine;
//仅本机的，因为tcp链接一部分代码在其他线程返回，故这里要推到数据结构等主线程来取.
public enum LocalMsgType
{
    Connect,
    DisConnect,
    SendFTPLogComplete,//全部日志发送完成
    GameStart,//下载中断，进入Startup场景
    SaveRecord,//保存录像结束.
    PathCalcFinished,//路径计算完成
}

//这里都是联机模块线程的信息，类似网络断开之类的
public class LocalMsg
{
    public int Message;
    public int Result;
    public int Param;
    public string message;
    public object context;
}

class ProtoHandler
{
    static List<Dictionary<int, byte[]>> packets = new List<Dictionary<int, byte[]>>();
    public static void RegisterPacket(Dictionary<int, byte[]> packet)
    {
        packets.Add(packet);
    }
    public static void UnRegisterPacket(Dictionary<int, byte[]>packet)
    {
        packets.Remove(packet);
    }

    //套接口消息.
    public static void Update()
    {
        //处理网络消息
        for (int i = 0; i < packets.Count; i++)
        {
            Dictionary<int, byte[]> pack = packets[i];
            lock (pack)
            {
                MemoryStream ms = null;
                try
                {
                    foreach (var each in pack)
                    {
                        //UnityEngine.Debug.LogError(string.Format("收到:{0}", each.Key));
                        switch (each.Key)
                        {
                            case (int)MeteorMsg.MsgType.ProtocolVerifyRsp:
                                ms = new MemoryStream(each.Value);
                                ProtocolVerifyRsp rspVer = ProtoBuf.Serializer.Deserialize<ProtocolVerifyRsp>(ms);
                                OnVerifyResult(rspVer);
                                break;
                            case (int)MeteorMsg.MsgType.GetRoomRsp:
                                ms = new MemoryStream(each.Value);
                                GetRoomRsp rspG = ProtoBuf.Serializer.Deserialize<GetRoomRsp>(ms);
                                OnGetRoomRsp(rspG);
                                break;
                            case (int)MeteorMsg.MsgType.JoinRoomRsp:
                                ms = new MemoryStream(each.Value);
                                JoinRoomRsp rspJ = ProtoBuf.Serializer.Deserialize<JoinRoomRsp>(ms);
                                ClientJoinRoomRsp(rspJ);
                                break;
                            //case (int)MeteorMsg.MsgType.OnJoinRoomRsp:
                            //    ms = new MemoryStream(each.Value);
                            //    OnEnterRoomRsp rspE = ProtoBuf.Serializer.Deserialize<OnEnterRoomRsp>(ms);
                            //    OnEnterRoomRsp_(rspE);
                            //    break;
                            case (int)MeteorMsg.MsgType.CreateRoomRsp:
                                ms = new MemoryStream(each.Value);
                                CreateRoomRsp rspC = ProtoBuf.Serializer.Deserialize<CreateRoomRsp>(ms);
                                OnCreateRoomRsp(rspC);
                                break;
                            //case (int)MeteorMsg.MsgType.EnterLevelRsp:
                            //    ms = new MemoryStream(each.Value);
                            //    EnterLevelRsp rspER = ProtoBuf.Serializer.Deserialize<EnterLevelRsp>(ms);
                            //    EnterLevelRsp_(rspER);
                            //    break;
                            //case (int)MeteorMsg.MsgType.OnEnterLevelRsp:
                            //    ms = new MemoryStream(each.Value);
                            //    OnEnterLevelRsp rspOE = ProtoBuf.Serializer.Deserialize<OnEnterLevelRsp>(ms);
                            //    OnEnterLevelRsp_(rspOE);
                            //    break;
                            //case (int)MeteorMsg.MsgType.UserRebornSB2C:
                            //    ms = new MemoryStream(each.Value);
                            //    OnEnterLevelRsp rspReborn = ProtoBuf.Serializer.Deserialize<OnEnterLevelRsp>(ms);
                            //    OnUserRebornRsp_(rspReborn);
                            //    break;
                            //case (int)MeteorMsg.MsgType.OnLeaveRoomRsp:
                            //    ms = new MemoryStream(each.Value);
                            //    OnLeaveRoomRsp rspL = ProtoBuf.Serializer.Deserialize<OnLeaveRoomRsp>(ms);
                            //    OnLeaveRoomRsp_(rspL);
                            //    break;
                            case (int)MeteorMsg.MsgType.ChatInRoomRsp:
                                ms = new MemoryStream(each.Value);
                                ChatMsg chatRsp = ProtoBuf.Serializer.Deserialize<ChatMsg>(ms);
                                OnReceiveChatMsg(chatRsp);
                                break;
                            case (int)MeteorMsg.MsgType.AudioChat:
                                ms = new MemoryStream(each.Value);
                                AudioChatMsg audioRsp = ProtoBuf.Serializer.Deserialize<AudioChatMsg>(ms);
                                OnReceiveAudioMsg(audioRsp);
                                break;
                            //case (int)MeteorMsg.MsgType.UserDeadSB2C:
                            //    //Debug.LogError("OnUserDead");
                            //    ms = new MemoryStream(each.Value);
                            //    UserId userDeadRsp = ProtoBuf.Serializer.Deserialize<UserId>(ms);
                            //    OnUserDead(userDeadRsp);
                            //    break;

                            //case (int)MeteorMsg.MsgType.ExitQueueRsp:
                            //    OnExitQueue();
                            //    break;
                            case (int)MeteorMsg.MsgType.EnterQueueRsp:
                                OnEnterQueue();
                                break;

                            //帧同步信息-UDP
                            //收到服务器的帧同步信息.
                            case (int)MeteorMsg.MsgType.SyncCommand:
                                ms = new MemoryStream(each.Value);
                                GameFrames t = ProtoBuf.Serializer.Deserialize<GameFrames>(ms);
                                Main.Ins.FrameSync.OnReceiveCommand(t);
                                break;
                        }
                    }
                }
                catch (Exception exp)
                {
                    UnityEngine.Debug.LogError(exp.Message + exp.StackTrace);
                }
                finally
                {
                    pack.Clear();
                }
            }
        }
        
        lock (messageQueue)
        {
            int length = messageQueue.Count;
            for (int i = 0; i < length; i++)
            {
                switch (messageQueue[i].Message)
                {
                    case (short)LocalMsgType.Connect: OnConnect(messageQueue[i].Result, messageQueue[i].message);break;
                    case (short)LocalMsgType.DisConnect: OnDisconnect();break;
                    case (short)LocalMsgType.SendFTPLogComplete: OnSendComplete(messageQueue[i].Result, messageQueue[i].Param);break;
                    case (short)LocalMsgType.GameStart:OnGameStart();break;
                    case (short)LocalMsgType.SaveRecord:OnSaveRecord(messageQueue[i]);break;
                    case (short)LocalMsgType.PathCalcFinished:OnPathCalcFinished(messageQueue[i]);break;
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

    //进入排队得到了回复
    static void OnEnterQueue()
    {
        //if (MatchWnd.Exist)
        //    MatchWnd.Instance.OnEnterQueue();
    }
    //离开了排队
    static void OnExitQueue()
    {
        //if (MatchWnd.Exist)
        //    MatchWnd.Instance.OnLeaveQueue();
    }

    //某人发来一段语音,显示一个按钮，点击了就播放这段语音即可.
    static void OnReceiveAudioMsg(AudioChatMsg msg)
    {
        //if (!RoomChatWnd.Exist)
        //    RoomChatWnd.Instance.Open();
        //RoomChatWnd.Instance.Add((int)msg.playerId, msg.audio_data);
    }

    static void OnReceiveChatMsg(ChatMsg msg)
    {
        //if (!RoomChatWnd.Exist)
        //    RoomChatWnd.Instance.Open();
        //RoomChatWnd.Instance.Add((int)msg.playerId, msg.chatMessage);
    }

    //看是自己挂了还是其他人挂了
    //static void OnUserDead(UserId id)
    //{
    //    if (id.Player != null)
    //    {
    //        for (int i = 0; i < id.Player.Count; i++)
    //        {
    //            MeteorUnit unit = U3D.GetUnit((int)id.Player[i]);
    //            if (unit == null)
    //                continue;
    //            if (unit != MeteorManager.Instance.LocalPlayer)
    //            {
    //                MeteorManager.Instance.OnNetRemoveUnit(unit);
    //            }
    //            else
    //            {
    //                //摄像机先禁用跟随.禁用
    //                //if (MeteorManager.Instance.LocalPlayer == null)
    //                //    return;
    //                MeteorManager.Instance.OnNetRemoveUnit(unit);
    //                if (CameraFollow.Ins != null)
    //                    CameraFollow.Ins.FreeCamera = true;
    //                MeteorManager.Instance.LocalPlayer = null;
    //                //向服务器发送复活请求.由服务器返回位置,然后在该位置创建角色
    //                ClientProxy.ReqReborn(NetWorkBattle.Ins.PlayerId);
    //            }
    //        }
    //    }
    //}

    //同步关键帧，全部角色的属性，都设置一次
    //在进入房间后开始接收此消息，有可能玩家还没进入战场，因为要选择角色和武器.
    //static void OnSyncKeyFrame(KeyFrame frame)
    //{
    //    if (NetWorkBattle.Ins.RoomId == -1)
    //    {
    //        Debug.LogError("退出房间后仍收到同步消息");
    //        return;
    //    }
    //    if (!NetWorkBattle.Ins.bSync && NetWorkBattle.Ins.TurnStarted)
    //    {
    //        NetWorkBattle.Ins.bSync = true;
    //        if (ReconnectWnd.Exist)
    //            ReconnectWnd.Instance.Close();
    //        if (GameBattleEx.Instance)
    //            GameBattleEx.Instance.Resume();
    //    }

    //    if (NetWorkBattle.Ins.TurnStarted && MeteorManager.Instance.LocalPlayer != null)
    //    {
    //        //在战场更新中,更新其他角色信息，自己的只上传.
    //        for (int i = 0; i < frame.Players.Count; i++)
    //        {
    //            MeteorUnit unit = NetWorkBattle.Ins.GetNetPlayer((int)frame.Players[i].id);
    //            if (unit != null && unit.InstanceId != MeteorManager.Instance.LocalPlayer.InstanceId)
    //            {
    //                //玩家同步所有属性
    //                NetWorkBattle.Ins.ApplyAttribute(unit, frame.Players[i]);
    //            }
    //        }
    //    }
    //    NetWorkBattle.Ins.OnPlayerUpdate((int)frame.frameIndex);
    //}

    //同步服务端发来的其他玩家的按键
    static void OnSyncRsp(GameFrames rsp)
    {

    }

    //static void OnLeaveRoomRsp_(OnLeaveRoomRsp rsp)
    //{
    //    //rsp.playerId
    //    if (NetWorkBattle.Ins != null)
    //    {
    //        MeteorUnit unit = NetWorkBattle.Ins.GetNetPlayer((int)rsp.playerId);
    //        if (unit != null)
    //        {
    //            if (unit.InstanceId != MeteorManager.Instance.LocalPlayer.InstanceId)
    //            {
    //                U3D.InsertSystemMsg(string.Format("{0}离开了房间", NetWorkBattle.Ins.GetNetPlayerName((int)rsp.playerId)));
    //                MeteorManager.Instance.OnNetRemoveUnit(unit);
    //            }
    //        }
    //    }
    //}

    ////角色复活时,返回消息体与其他人进场是一样的
    //static void OnUserRebornRsp_(OnEnterLevelRsp rsp)
    //{
    //    if (NetWorkBattle.Ins != null && rsp != null && rsp.player != null)
    //    {
    //        NetWorkBattle.Ins.waitReborn = false;
    //        U3D.InitNetPlayer(rsp.player);//其他人复活,或者其他人进入战场,或者自己复活.
    //        if (rsp.player.id == NetWorkBattle.Ins.PlayerId && MeteorManager.Instance.LocalPlayer != null)
    //        {
    //            AudioListener au = MeteorManager.Instance.LocalPlayer.gameObject.GetComponent<AudioListener>();
    //            if (au == null)
    //            {
    //                Main.Instance.listener.enabled = false;
    //                Main.Instance.playerListener = MeteorManager.Instance.LocalPlayer.gameObject.AddComponent<AudioListener>();
    //            }
    //            if (FightWnd.Exist)
    //                FightWnd.Instance.UpdatePlayerInfo();
    //        }
    //    }
    //}

    //其他角色进入战场时.
    //static void OnEnterLevelRsp_(OnEnterLevelRsp rsp)
    //{
    //    //其他玩家进入此房间
    //    if (NetWorkBattle.Ins != null && rsp != null && rsp.player != null)
    //        U3D.InitNetPlayer(rsp.player);//其他人复活,或者其他人进入战场,或者自己复活.
    //}

    //public static bool loading = false;
    //static void EnterLevelRsp_(EnterLevelRsp rsp)
    //{
    //    if (loading)
    //        return;
    //    //进入到房间内,开始加载场景设置角色初始化位置
    //    NetWorkBattle.Ins.scene = rsp.scene;
    //    U3D.LoadNetLevel(rsp.scene.items, rsp.scene.players);
    //    loading = true;
    //}

    //自己建房间成功，则转到选人界面.与加入房间一个德行
    static void OnCreateRoomRsp(CreateRoomRsp rsp)
    {
        U3D.PopupTip("创建房间回应");
        if (rsp.result == 1)
        {
            //GameOverlayWnd.Instance.InsertSystemMsg(string.Format("创建房间 编号:{0}", rsp.roomId));
            Main.Ins.RoomMng.Register((int)rsp.roomId, true);
            ClientAutoJoinRoom(rsp);
        }
        else
        {
            U3D.PopupTip("创建房间失败");//无论什么失败，都不开启KCP模块
        }
    }

    ////其他人进入我所在的房间，消息发给我
    //static void OnEnterRoomRsp_(JoinRoomRsp rsp)
    //{
    //    //显示某某进入房间的文字
    //    GameOverlayWnd.Instance.InsertSystemMsg(string.Format("{0} 进入房间", rsp.playerNick));
    //}

    //创建房间OK时自动进入房间.
    static void ClientAutoJoinRoom(CreateRoomRsp rsp)
    {
        if (Main.Ins.NetWorkBattle.RoomId == -1)
        {
            //选人，或者阵营，或者
            //if (MainLobby.Exist)
            //    MainLobby.Instance.Close();
            //if (RoomOptionWnd.Exist)
            //    RoomOptionWnd.Instance.Close();
            Main.Ins.NetWorkBattle.OnEnterRoomSuccessed((int)rsp.roomId, (int)rsp.levelId, (int)rsp.playerId);
            UdpClientProxy.Connect((int)rsp.port, (int)rsp.playerId);
            RoomInfo r = Main.Ins.RoomMng.GetRoom((int)rsp.roomId);
            //如果是盟主模式，无需选择阵营
            //if (r.rule == RoomInfo.RoomRule.MZ)
            //    RoleSelectWnd.Instance.Open();
            //else
            //    CampSelectWnd.Instance.Open();
        }
    }

    //自己进入房间的消息被服务器处理，返回给自己
    static void ClientJoinRoomRsp(JoinRoomRsp rsp)
    {
        //如果规则是暗杀或者死斗
        //自己进入房间成功时的信息,跳转到选角色界面，角色选择，就跳转到武器选择界面
        //到最后一步确认后，开始同步服务器场景数据.
        if (rsp.result == 1)
        {
            if (Main.Ins.NetWorkBattle.RoomId == -1)
            {
                //选人，或者阵营，或者
                //UnityEngine.Debug.LogError("OnJoinRoom successful");
                //if (MainLobby.Exist)
                //    MainLobby.Instance.Close();
                //if (RoomOptionWnd.Exist)
                //    RoomOptionWnd.Instance.Close();
                Main.Ins.NetWorkBattle.OnEnterRoomSuccessed((int)rsp.roomId, (int)rsp.levelIdx, (int)rsp.playerId);
                UdpClientProxy.Connect((int)rsp.port, (int)rsp.playerId);
                RoomInfo r = Main.Ins.RoomMng.GetRoom((int)rsp.roomId);
                //如果是盟主模式，无需选择阵营
                //if (r.rule == RoomInfo.RoomRule.MZ)
                //    RoleSelectWnd.Instance.Open();
                //else
                //    CampSelectWnd.Instance.Open();
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
                //密码不正确
                case 4:
                    Main.Ins.EnterState(Main.Ins.PsdEditDialogState); PsdEditDialogState.Instance.OnConfirm = ()=>
                    {
                        Common.SendJoinRoom((int)rsp.roomId, PsdEditDialogState.Instance.Control("PsdField").GetComponent<UnityEngine.UI.InputField>().text);
                        PsdEditDialogState.Instance.OnBackPress();
                    };
                    break;
            }
        }
    }

    static void OnVerifyResult(ProtocolVerifyRsp rsp)
    {
        if (rsp.result == 1)
        {
            TcpClientProxy.UpdateGameServer();//取得房间列表
        }
        else
        {
            U3D.PopupTip(string.Format("客户端版本过低-无法连接到服务器"));
            TcpClientProxy.Exit();
        }
    }

    static void OnGetRoomRsp(GetRoomRsp rsp)
    {
        Main.Ins.RoomMng.RegisterRooms(rsp.RoomInLobby);
    }

    static int retryNum = 3;
    static void OnConnect(int result, string message)
    {
        //取得房间信息
        if (result == 1)
        {
            retryNum = 3;
            TcpClientProxy.UpdateGameServer();
            //TcpClientProxy.AutoLogin();//验证客户端的合法性
        }
        else
        {
            //链接失败,重置对战
            U3D.PopupTip(message);
            Main.Ins.NetWorkBattle.OnDisconnect();
            retryNum--;
            if (retryNum <= 0)
            {
                TcpClientProxy.Exit();//重试3次，等待切换服务器再激活链接服务器的定时器.
                retryNum = 3;
            }
        }
    }

    //断开链接,退出场景，返回
    static void OnDisconnect()
    {
        Main.Ins.NetWorkBattle.OnDisconnect();
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

    //保存录像结束，把对应的Pending框关闭
    static void OnSaveRecord(LocalMsg msg)
    {
        DialogUtils.Ins.CloseWait();//关闭Pending框.
    }

    static void OnPathCalcFinished(LocalMsg msg)
    {
        if (msg.context is PathPameter)
        {
            PathPameter pameter = msg.context as PathPameter;
            pameter.stateMachine.OnPathCalcFinished(pameter);
        }
    }
}
