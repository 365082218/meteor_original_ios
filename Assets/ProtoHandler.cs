
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using protocol;
using UnityEngine;
using System.Net.Sockets;
using Assets.Code.Idevgame.Common.Util;
using ProtoBuf;
//仅本机的，因为tcp链接一部分代码在其他线程返回，故这里要推到数据结构等主线程来取.
public enum LocalMsgType
{
    Connect,
    TimeOut,
    DisConnect,
    SendFTPLogComplete,//全部日志发送完成
    GameStart,//下载中断，进入Startup场景
    SaveRecord,//保存录像结束.
}

public class EResult {
    public const int PlayerInRoom = 30;//玩家正在房间中
    public const int RoomMaxed = 29;//服务器无法创建更多的房间
    public const int Succeed = 1;//成功
    public const int RoomInvalid = 28;//房间不存在
    public const int PlayerMax = 27;//房间人数已满
    public const int PasswordError = 26;//密码不匹配
    public const int ModelMiss = 25;//缺少模型
    public const int Skicked = 24;//被禁止加入房间.
    public const int VersionInvalid = 23;//流星版本不一致
    public const int CampMax = 22;//阵营人数已满
    public const int Timeup = 21;//时间即将结束
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

//处理TCP协议报文
public class TcpProtoHandler : ProtoHandler {
    private TcpProtoHandler() {

    }
    private static TcpProtoHandler _Ins;
    public static TcpProtoHandler Ins {
        get {
            if (_Ins == null) {
                _Ins = new TcpProtoHandler();
            }
            return _Ins;
        }
    }
    MemoryStream ms = new MemoryStream();
    public override void Update() {
        for (int i = 0; i < packets.Count; i++) {
            SortedDictionary<int, byte[]> pack = packets[i];
            lock (pack) {
                
                try {
                    foreach (var each in pack) {
                        ms.SetLength(0);
                        ms.Write(each.Value, 0, each.Value.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        TcpClientProxy.Ins.endPing = Time.time;
                        TcpClientProxy.Ins.ping = Mathf.FloorToInt(1000 * (TcpClientProxy.Ins.endPing - TcpClientProxy.Ins.startPing));
                        Main.Ins.EventBus.Fire(EventId.PingChanged);
                        switch (each.Key) {
                            case (int)MeteorMsg.MsgType.ProtocolVerify:
                                //ms = new MemoryStream(each.Value);
                                ProtocolVerifyRsp rspVer = Serializer.Deserialize<ProtocolVerifyRsp>(ms);
                                OnVerifyResult(rspVer);
                                break;
                            case (int)MeteorMsg.MsgType.AliveUpdate:
                                TcpClientProxy.Ins.HeartBeat();
                                break;
                            case (int)MeteorMsg.MsgType.GetRoomRsp:
                                //ms = new MemoryStream(each.Value);
                                GetRoomRsp rspG = Serializer.Deserialize<GetRoomRsp>(ms);
                                OnGetRoomRsp(rspG);
                                break;
                            case (int)MeteorMsg.MsgType.JoinRoomRsp:
                                //ms = new MemoryStream(each.Value);
                                JoinRoomRsp rspJ = Serializer.Deserialize<JoinRoomRsp>(ms);
                                ClientJoinRoomRsp(rspJ);
                                break;
                            case (int)MeteorMsg.MsgType.CreateRoomRsp:
                                //ms = new MemoryStream(each.Value);
                                CreateRoomRsp rspC = Serializer.Deserialize<CreateRoomRsp>(ms);
                                OnCreateRoomRsp(rspC);
                                break;
                            case (int)MeteorMsg.MsgType.OnPlayerJoinRoom:
                                //ms = new MemoryStream(each.Value);
                                OnPlayerJoinRoom rspOE = Serializer.Deserialize<OnPlayerJoinRoom>(ms);
                                OnPlayerEnterRoomRsp(rspOE);
                                break;
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
                            case (int)MeteorMsg.MsgType.OnPlayerEnterLevel://其他玩家进入关卡
                                //ms = new MemoryStream(each.Value);
                                PlayerEvent rspEnterLevel = Serializer.Deserialize<PlayerEvent>(ms);
                                OnPlayerEnterLevel(rspEnterLevel);
                                break;
                            case (int)MeteorMsg.MsgType.EnterLevelRsp://自己进入关卡,拉取到场景里所有已知角色
                                //ms = new MemoryStream(each.Value);
                                OnEnterLevelRsp EnterLevel = Serializer.Deserialize<OnEnterLevelRsp>(ms);
                                OnEnterLevel(EnterLevel);
                                break;
                            case (int)MeteorMsg.MsgType.ChatInRoomRsp:
                                //ms = new MemoryStream(each.Value);
                                ChatMsg chatRsp = Serializer.Deserialize<ChatMsg>(ms);
                                OnReceiveChatMsg(chatRsp);
                                break;
                            //case (int)MeteorMsg.MsgType.AudioChat:
                            //    //ms = new MemoryStream(each.Value);
                            //    AudioChatMsg audioRsp = Serializer.Deserialize<AudioChatMsg>(ms);
                            //    OnReceiveAudioMsg(audioRsp);
                            //    break;
                            case (int)MeteorMsg.MsgType.OnPlayerLeaveLevel://其他玩家离开关卡
                                //ms = new MemoryStream(each.Value);
                                PlayerEvent rspLeaveLevel = Serializer.Deserialize<PlayerEvent>(ms);
                                OnPlayerLeaveLevel(rspLeaveLevel);
                                break;
                            case (int)MeteorMsg.MsgType.SyncRate://KCP识别出错，关闭旧的KCP，创建新的
                                SyncMsg syncRate = Serializer.Deserialize<SyncMsg>(ms);
                                FrameSyncServer.Ins.ChangeSyncRate(syncRate.syncrate);
                                break;
                        }
                    }
                } catch (Exception exp) {
                    UnityEngine.Debug.LogError(exp.Message + exp.StackTrace);
                } finally {
                    pack.Clear();
                }
            }
        }

        lock (messageQueue) {
            int length = messageQueue.Count;
            for (int i = 0; i < length; i++) {
                switch (messageQueue[i].Message) {
                    case (short)LocalMsgType.TimeOut: OnTimeOut(messageQueue[i].Result, messageQueue[i].message);break;
                    case (short)LocalMsgType.Connect: OnConnect(messageQueue[i].Result, messageQueue[i].message); break;
                    case (short)LocalMsgType.DisConnect: OnDisconnect(); break;
                    case (short)LocalMsgType.SendFTPLogComplete: OnSendComplete(messageQueue[i].Result, messageQueue[i].Param); break;
                    case (short)LocalMsgType.SaveRecord: OnSaveRecord(messageQueue[i]); break;
                }
            }
            messageQueue.Clear();
        }
    }

    Timer verifyTimeOut;
    public bool VerifySuccess;
    void OnVerifyResult(ProtocolVerifyRsp rsp) {
        if (verifyTimeOut != null) {
            verifyTimeOut.Stop();
            verifyTimeOut = null;
        }
        WaitDialogState.State.WaitExit(0.5f);
        VerifySuccess = rsp.result == 1;
        if (rsp.result == 1) {
            U3D.InsertSystemMsg("验证成功，拉取房间列表");
            if (MainLobbyDialogState.Exist) {
                MainLobbyDialogState.Instance.OnSelectService();
            }
            NetWorkBattle.Ins.PlayerId = (int)rsp.player;
            TcpClientProxy.Ins.UpdateGameServer();//取得房间列表
        } else {
            NetWorkBattle.Ins.PlayerId = -1;
            U3D.PopupTip(rsp.message);
            TcpClientProxy.Ins.Exit();
        }
    }

    //void OnReceiveAudioMsg(AudioChatMsg msg) {
    //    RoomChatDialogState.State.Open();
    //    RoomChatDialogState.Instance.Add((int)msg.playerId, msg.audio_data);
    //}

    void OnReceiveChatMsg(ChatMsg msg) {
        RoomChatDialogState.State.Open();
        RoomChatDialogState.Instance.Add((int)msg.playerId, msg.chatMessage);
    }

    void OnPlayerEnterRoomRsp(OnPlayerJoinRoom rsp) {
        U3D.InsertSystemMsg(rsp.nick + "进入了房间");
        //Debug.LogError(rsp.nick + "进入了房间");
    }

    void OnPlayerLeaveLevel(PlayerEvent rsp) {
        U3D.OnDestroyNetPlayer(rsp);
    }

    //其他玩家进入战场,要判断当前是否在战场
    //这个玩家也可能是自己
    void OnPlayerEnterLevel(PlayerEvent rsp) {
        //Debug.LogError(rsp.name + "进入了战场");
        if (FrameReplay.Ins.Started)//一定是新角色进入.
            U3D.OnCreateNetPlayer(rsp);
        else {
            //进来的玩家是自己，加载场景
            if (rsp.playerId == NetWorkBattle.Ins.PlayerId) {
                //战斗场景已经存在
                if (Main.Ins.GameBattleEx != null) {
                    //重新设置主角的模型和阵营,原来的模型还在
                    MonsterEx mon = Main.Ins.LocalPlayer.Attr;
                    mon.Weapon = (int)rsp.weapon;
                    mon.Model = (int)rsp.model;
                    Main.Ins.LocalPlayer.Camp = (EUnitCamp)rsp.camp;
                    Main.Ins.LocalPlayer.Init((int)rsp.model, mon, true);
                    //重设
                    Main.Ins.LocalPlayer.ResetPosition();
                    Main.Ins.GameBattleEx.NetGameStart();
                    return;
                }
                FrameSyncLocal.Ins.OnSelfEnterLevel(rsp);
                NetWorkBattle.Ins.Load();
            } else {
                //自己还没有进入战场，看到别人进入战场的信息
                U3D.InsertSystemMsg(U3D.GetCampEnterLevelStr((EUnitCamp)rsp.camp, rsp.name));
                FrameSyncLocal.Ins.OnPlayerEnterLevel(rsp);
            }
        }
    }

    //得到了场景里所有人的信息.有位置的.
    void OnEnterLevel(OnEnterLevelRsp rsp) {
        if (rsp.result == 1) {
            FrameSyncLocal.Ins.OnEnterLevel(rsp);
        } else {
            if (rsp.reason == EResult.CampMax) {
                U3D.PopupTip("阵营人数已满,无法进入");
                if (WeaponSelectDialogState.Exist) {
                    WeaponSelectDialogState.Instance.ShowBack();
                }
            } else if (rsp.reason == EResult.Timeup) {
                U3D.PopupTip("当前轮次即将结束，请稍后再进入");
                if (WeaponSelectDialogState.Exist) {
                    WeaponSelectDialogState.Instance.ShowTimeup((int)rsp.result);
                }
            }
        }
    }

    //自己建房间成功，则转到选人界面.与加入房间一个德行
    void OnCreateRoomRsp(CreateRoomRsp rsp) {
        //U3D.PopupTip("创建房间回应");
        if (rsp.result == 1) {
            //GameOverlayWnd.Instance.InsertSystemMsg(string.Format("创建房间 编号:{0}", rsp.roomId));
            RoomMng.Ins.Register((int)rsp.roomId, true);
            ClientAutoJoinRoom(rsp);
        } else {
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
    //自身就是房主，不需要判断密码
    void ClientAutoJoinRoom(CreateRoomRsp rsp) {
        if (NetWorkBattle.Ins.RoomId == -1) {
            TcpClientProxy.Ins.JoinRoom((int)rsp.roomId);
        }
    }

    void OnSaveRecord(LocalMsg msg) {
        WaitDialogState.State.WaitExit(1.0f);//关闭Pending框.
    }

    //自己进入房间的消息被服务器处理，返回给自己
    void ClientJoinRoomRsp(JoinRoomRsp rsp) {
        //如果规则是暗杀或者死斗
        //自己进入房间成功时的信息,跳转到选角色界面，角色选择，就跳转到武器选择界面
        //到最后一步确认后，开始同步服务器场景数据.
        if (rsp.result == 1) {
            if (NetWorkBattle.Ins.RoomId == -1) {
                //选人，或者阵营，或者
                RoomInfo r = RoomMng.Ins.GetRoom((int)rsp.roomId);
                NetWorkBattle.Ins.OnEnterRoomSuccessed((int)rsp.roomId, (int)rsp.levelIdx, NetWorkBattle.Ins.PlayerId, (int)rsp.port);
                KcpClient.Ins.Connect((int)rsp.port, (int)NetWorkBattle.Ins.PlayerId);
                //如果是盟主模式，无需选择阵营
                if (r.rule == RoomInfo.RoomRule.MZ)
                    Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoleSelectDialogState);
                else
                    Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.CampSelectDialogState);
            }
        } else {
            //显示各种错误信息框
            //rsp.reason
            //2未找到
            //3需要退出当前房间
            //1房间满
            UnityEngine.Debug.LogError(string.Format("error:{0}", rsp.reason));
            switch (rsp.reason) {
                case EResult.PlayerMax: U3D.PopupTip("此房间已满，无法进入"); break;
                case EResult.RoomInvalid: U3D.PopupTip("房间已解散"); break;
                case EResult.PlayerInRoom: U3D.PopupTip("需要先退出之前所在的房间"); break;
                //密码不正确
                case EResult.PasswordError:
                    PsdEditDialogState.State.Open(); PsdEditDialogState.Instance.OnConfirm = () => {
                        TcpClientProxy.Ins.JoinRoom((int)RoomMng.Ins.wantJoin, PsdEditDialogState.Instance.Control("PsdField").GetComponent<UnityEngine.UI.InputField>().text);
                        PsdEditDialogState.Instance.OnBackPress();
                    };
                    break;
                case EResult.ModelMiss:
                    ModelItem model = DlcMng.Ins.GetModelMeta((int)rsp.result);
                    U3D.PopupTip(model == null ? string.Format("缺少模型:id:{0}", rsp.result) : string.Format("缺少模型:{0}", model.Name));
                    break;
                case EResult.Skicked:
                    U3D.PopupTip("你已被禁止加入此房间");
                    break;
                case EResult.VersionInvalid:
                    U3D.PopupTip("流星版本不匹配,无法进入,请先在设置里切换版本后再进入此房间");
                    break;
            }
        }
    }

    void OnGetRoomRsp(GetRoomRsp rsp) {
        RoomMng.Ins.RefreshRooms(rsp.Rooms);
        Main.Ins.EventBus.Fire(EventId.RoomUpdate);
    }

    //超时信息.
    void OnTimeOut(int retult, string message) {
        U3D.InsertSystemMsg(message);
        ConnectServerDialogState.State.Close();
    }

    void OnConnect(int result, string message) {
        ConnectServerDialogState.State.Close();
        //取得房间信息
        if (result == 1) {
            WaitDialogState.State.Open("验证服务器是否可用");
            VerifySuccess = false;
            if (verifyTimeOut != null) {
                verifyTimeOut.Stop();
                verifyTimeOut = null;
            }
            verifyTimeOut = Timer.once(3.0f, OnVerifyTimeOut);
            TcpClientProxy.Ins.AutoLogin();//验证客户端的合法性
        } else {
            //链接失败,重置对战
            U3D.InsertSystemMsg(message);
            NetWorkBattle.Ins.OnDisconnect();
            TcpClientProxy.Ins.Exit();//重试3次，等待切换服务器再激活链接服务器的定时器.
        }
    }

    private void OnVerifyTimeOut() {
        U3D.InsertSystemMsg("服务器验证超时，此服务器不兼容");
        WaitDialogState.State.WaitExit(0.5f);
        if (verifyTimeOut != null) {
            verifyTimeOut.Stop();
            verifyTimeOut = null;
        }
    }

    void OnDisconnect() {
        if (LevelHelper.Ins != null)
            LevelHelper.Ins.Stop();
        NetWorkBattle.Ins.OnDisconnect();
    }

    void OnSendComplete(int result, int sendFileCount) {
        if (result == 1)
            U3D.PopupTip("日志上传完毕 成功发送" + sendFileCount + "个文件");
        else
            U3D.PopupTip("日志上传失败");
    }
}

public class UdpProtoHandler : ProtoHandler {
    private UdpProtoHandler() {

    }

    private static UdpProtoHandler _Ins;
    public static UdpProtoHandler Ins {
        get {
            if (_Ins == null) {
                _Ins = new UdpProtoHandler();
            }
            return _Ins;
        }
    }

    MemoryStream ms = new MemoryStream();
    public override void Update() {
        for (int i = 0; i < packets.Count; i++) {
            SortedDictionary<int, byte[]> pack = packets[i];
            lock (pack) {
                try {
                    //在这个循环里，不能跑去修改这个packet，所有重要消息，都通过计时器处理.
                    foreach (var each in pack) {
                        ms.SetLength(0);
                        ms.Write(each.Value, 0, each.Value.Length);
                        ms.Seek(0, SeekOrigin.Begin);
                        switch (each.Key) {
                            //UDP消息
                            case (int)MeteorMsg.Command.ServerSync:
                                GameFrame frame = Serializer.Deserialize<GameFrame>(ms);
                                OnSyncPlayers(frame);
                                break;
                            case (int)MeteorMsg.Command.NewTurn:
                                if (!FrameReplay.Ins.Started) {
                                    return;
                                }
                                if (Main.Ins.GameBattleEx != null)
                                    Main.Ins.GameBattleEx.NetGameOver();
                                break;
                            case (int)MeteorMsg.Command.GetItem:
                                //ms = new MemoryStream(each.Value);
                                GetItemMsg getitem = Serializer.Deserialize<GetItemMsg>(ms);
                                OnItemPickuped(getitem);
                                break;
                            case (int)MeteorMsg.Command.Drop:
                                //ms = new MemoryStream(each.Value);
                                DropMsg dropItem = Serializer.Deserialize<DropMsg>(ms);
                                OnItemDropped(dropItem);
                                break;
                            case (int)MeteorMsg.Command.Kill:
                                //ms = new MemoryStream(each.Value);
                                OperateMsg kill = Serializer.Deserialize<OperateMsg>(ms);
                                Kill(kill);
                                break;
                            case (int)MeteorMsg.Command.Kick:
                                //ms = new MemoryStream(each.Value);
                                OperateMsg kick = Serializer.Deserialize<OperateMsg>(ms);
                                OnKicked(kick);
                                break;
                            case (int)MeteorMsg.Command.Skick:
                                //ms = new MemoryStream(each.Value);
                                OperateMsg skick = Serializer.Deserialize<OperateMsg>(ms);
                                OnSkicked(skick);
                                break;
                        }
                    }
                } catch (Exception exp) {
                    UnityEngine.Debug.LogError(exp.Message + exp.StackTrace);
                } finally {
                    pack.Clear();
                }
            }
        }

        //lock (messageQueue) {
        //    int length = messageQueue.Count;
        //    for (int i = 0; i < length; i++) {
        //        switch (messageQueue[i].Message) {
        //            case (short)LocalMsgType.Connect: OnConnect(messageQueue[i].Result, messageQueue[i].message); break;
        //        }
        //    }
        //    messageQueue.Clear();
        //}
    }

    void OnKicked(OperateMsg msg) {
        if (msg.KillTarget == NetWorkBattle.Ins.PlayerId) {
            Timer.once(0.1f, OnDisconnected);
        } else {
            MeteorUnit unit = U3D.GetUnit((int)msg.KillTarget);
            if (unit != null) {
                U3D.InsertSystemMsg(string.Format("{0}被踢出房间", unit.name));
            }
        }
    }

    
    void OnSkicked(OperateMsg msg) {
        if (msg.KillTarget == NetWorkBattle.Ins.PlayerId) {
            Timer.once(0.1f, OnDisconnected);
        } else {
            MeteorUnit unit = U3D.GetUnit((int)msg.KillTarget);
            if (unit != null) {
                U3D.InsertSystemMsg(string.Format("{0}被踢出房间，不允许加入", unit.name));
            }
        }
    }

    private void OnDisconnected() {
        NetWorkBattle.Ins.OnDisconnect();
        U3D.PopupTip("被踢出房间");
    }

    void Kill(OperateMsg msg) {
        U3D.Kill((int)msg.KillTarget);
    }

    void OnItemDropped(DropMsg item) {
        DropMng.Ins.DropItem((int)item.item, new Vector3(item.position.x / 1000.0f, item.position.y / 1000.0f, item.position.z / 1000.0f), new Vector3(item.forward.x / 1000.0f, item.forward.y / 1000.0f, item.forward.z / 1000.0f));
    }

    void OnItemPickuped(GetItemMsg item) {
        MeteorUnit unit = U3D.GetUnit((int)item.playerId);
        if (item.type == (int)GetItemType.SceneItem) {
            SceneItemAgent sceneItem = U3D.GetSceneItem((int)item.instance);
            if (sceneItem != null) {
                sceneItem.OnNetPickuped(unit);
            }
        } else if (item.type == (int)GetItemType.PickupItem) {
            PickupItemAgent pickup = U3D.GetPickupItem((int)item.instance);
            if (pickup != null) {
                pickup.OnNetPickup(unit);
            }
        }
    }

    //UDP消息
    public void OnSyncPlayers(GameFrame frame) {
        FrameSyncServer.Ins.OnSynced();
        for (int i = 0; i < frame.commands.Count; i++) {
            FrameCommand fmd = frame.commands[i];
            NetWorkBattle.Ins.SyncPlayer(fmd);
        }
        NetWorkBattle.Ins.GameTime = frame.time;
    }

    //关卡重建，角色全部删除.UDP消息
    void OnResetLevel() {
        Main.Ins.GameBattleEx.NetGameOver();
    }
}

public class ProtoHandler
{
    protected List<SortedDictionary<int, byte[]>> packets = new List<SortedDictionary<int, byte[]>>();
    public void RegisterPacket(SortedDictionary<int, byte[]> packet)
    {
        packets.Add(packet);
    }
    public void UnRegisterPacket(SortedDictionary<int, byte[]>packet)
    {
        packets.Remove(packet);
    }

    //套接口消息.
    public virtual void Update()
    {

    }
    //跨线程访问
    public List<LocalMsg> messageQueue = new List<LocalMsg>();
    public void PostMessage(LocalMsg msg)
    {
        lock (messageQueue)
            messageQueue.Add(msg);
    }
}
