using Excel2Json;
using Idevgame.GameState.DialogState;
using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public class MainLobbyDialogState : CommonDialogState<MainLobbyDialog>
{
    public override string DialogName { get { return "MainLobby"; } }
    public MainLobbyDialogState(MainDialogMgr stateMgr):base(stateMgr)
    {

    }
}

public class MainLobbyDialog : Dialog
{
    GameObject RoomRoot;
    List<GameObject> rooms = new List<GameObject>();
    int SelectRoomId = -1;
    Button selectedBtn;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        RoomMng.Ins.SelectRoom(-1);
    }

    public override void OnClose()
    {
        Main.Ins.EventBus.UnRegister(EventId.PingChanged, OnPingChanged);
        Main.Ins.EventBus.UnRegister(EventId.RoomUpdate, OnRoomUpdate);
        if (Update != null)
        {
            Main.Ins.StopCoroutine(Update);
            Update = null;
        }
    }

    public void ClearRooms()
    {
        if (RoomRoot == null)
            return;
        SelectRoomId = -1;
        for (int i = 0; i < rooms.Count; i++)
            GameObject.DestroyImmediate(rooms[i]);
        rooms.Clear();
    }

    public void OnGetRoom(List<RoomInfo> rooms)
    {
        ClearRooms();
        int cnt = rooms.Count;
        GameObject prefab = Resources.Load<GameObject>("RoomInfoItem");
        for (int i = 0; i < cnt; i++)
            InsertRoomItem(rooms[i], prefab);
    }

    string[] ruleS = new string[5] { "盟主", "劫镖", "防守", "暗杀", "死斗" };
    public void InsertRoomItem(RoomInfo room, GameObject prefab)
    {
        GameObject roomObj = GameObject.Instantiate(prefab, RoomRoot.transform);
        roomObj.layer = RoomRoot.layer;
        roomObj.transform.localPosition = Vector3.zero;
        roomObj.transform.localScale = Vector3.one;
        roomObj.transform.rotation = Quaternion.identity;
        roomObj.transform.SetParent(RoomRoot.transform);
        Control("Name", roomObj).GetComponent<Text>().text = room.roomName;
        Control("Password", roomObj).GetComponent<Text>().text = room.password == 0 ? "无":"有";
        Control("Rule", roomObj).GetComponent<Text>().text = ruleS[(int)room.rule - 1];//盟主，死斗，暗杀
        Control("LevelName", roomObj).GetComponent<Text>().text = DataMgr.Ins.GetLevelData((int)room.levelIdx).Name;
        Control("Version", roomObj).GetComponent<Text>().text = room.version == RoomInfo.MeteorVersion.V107 ? "107": "907";
        Control("Group1", roomObj).GetComponent<Text>().text = room.Group1.ToString();
        Control("Group2", roomObj).GetComponent<Text>().text = room.Group2.ToString();
        Control("PlayerCount", roomObj).GetComponent<Text>().text = room.playerCount.ToString() + "/" + room.maxPlayer;
        Button btn = roomObj.GetComponent<Button>();
        btn.onClick.AddListener(() => { OnSelectRoom(room.roomId, btn); });
        rooms.Add(roomObj);
    }

    void OnSelectRoom(uint id, Button btn)
    {
        if (selectedBtn != null)
        {
            selectedBtn.image.color = new Color(1, 1, 1, 0);
            selectedBtn = null;
        }
        btn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);
        selectedBtn = btn;
        SelectRoomId = (int)id;
    }

    public void OnSelectService(int ping = 0)
    {
        Control("Server").GetComponent<Text>().text = string.Format("服务器:{0}    IP:{1}    端口:{2}    ping:{3}ms", CombatData.Ins.Server.ServerName,
            TcpClientProxy.Ins.server == null ? "还未取得" : TcpClientProxy.Ins.server.Address.ToString(), TcpClientProxy.Ins.server == null ? "还未取得" : TcpClientProxy.Ins.server.Port.ToString(), ping); 
    }

    Coroutine Update;//更新服务器列表的协程.
    void Init()
    {
        Control("JoinRoom").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnJoinRoom();
        });
        Control("CreateRoom").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnCreateRoom();
        });
        Control("Refresh").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnRefresh();
        });
        Control("Close").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        });

        RoomRoot = Control("RoomRoot");
        Control("Version").GetComponent<Text>().text = GameStateMgr.Ins.gameStatus.MeteorVersion;
        if (CombatData.Ins.Servers.Count == 0 && !serverListUpdate) {
            Update = Main.Ins.StartCoroutine(UpdateServiceList());
            WaitDialogState.State.Open("正在拉取服务器列表，请稍后");
        } else
            OnGetServerListDone();
        Main.Ins.EventBus.Register(EventId.PingChanged, OnPingChanged);
        Main.Ins.EventBus.Register(EventId.RoomUpdate, OnRoomUpdate);
    }

    private void OnRoomUpdate(object sender, TEventArgs e) {
        U3D.InsertSystemMsg("房间信息已刷新");
        OnGetRoom(RoomMng.Ins.rooms);
    }

    private void OnPingChanged(object sender, TEventArgs e) {
        OnSelectService(TcpClientProxy.Ins.ping);
    }

    GameObject serverRoot;
    bool serverListUpdate = false;//是否成功更新了服务器，如果更新了，选择的服务器为第一个服务器，否则不设定服务器
    IEnumerator UpdateServiceList()
    {
        UnityWebRequest vFile = new UnityWebRequest();
        vFile.url = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strServices);
        vFile.timeout = 2;
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        vFile.downloadHandler = dH;
        yield return vFile.Send();
        if (vFile.isNetworkError || vFile.responseCode != 200 || string.IsNullOrEmpty(dH.text))
        {
            Debug.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
            vFile.Dispose();
            Update = null;
            CombatData.Ins.Servers.Clear();
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.ServerList.Count; i++)
                CombatData.Ins.Servers.Add(GameStateMgr.Ins.gameStatus.ServerList[i]);
            serverListUpdate = false;
            OnGetServerListDone();
            yield break;
        }
        serverListUpdate = true;
        CombatData.Ins.Servers.Clear();
        LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
        for (int i = 0; i < js["services"].Count; i++)
        {
            ServerInfo s = new ServerInfo();
            if (!int.TryParse(js["services"][i]["port"].ToString(), out s.ServerPort))
                continue;
            if (!int.TryParse(js["services"][i]["type"].ToString(), out s.type))
                continue;
            if (s.type == 0)
                s.ServerHost = js["services"][i]["addr"].ToString();
            else
                s.ServerIP = js["services"][i]["addr"].ToString();
            s.ServerName = js["services"][i]["name"].ToString();
            CombatData.Ins.Servers.Add(s);
        }
        Update = null;

        //合并所有服务器到全局变量里
        for (int i = 0; i < GameStateMgr.Ins.gameStatus.ServerList.Count; i++)
            CombatData.Ins.Servers.Add(GameStateMgr.Ins.gameStatus.ServerList[i]);

        OnGetServerListDone();
    }

    public void OnGetServerListDone()
    {
        //拉取到服务器列表后
        Control("Servercfg").GetComponent<Button>().onClick.RemoveAllListeners();
        Control("Servercfg").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.ServerListDialogState);
        });
        //保存的列表最少有一项默认的
        if (CombatData.Ins.Server == null)
            CombatData.Ins.Server = CombatData.Ins.Servers[0];
        OnSelectService();
        GameObject Services = Control("Services", WndObject);
        serverRoot = Control("Content", Services);
        int childNum = serverRoot.transform.childCount;
        for (int i = 0; i < childNum; i++)
        {
            GameObject.Destroy(serverRoot.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < CombatData.Ins.Servers.Count; i++)
        {
            InsertServerItem(CombatData.Ins.Servers[i], i);
        }
        TcpClientProxy.Ins.ReStart();
        if (WaitDialogState.Exist())
            WaitDialogState.State.WaitExit(0.5f);
    }

    void InsertServerItem(ServerInfo svr, int i)
    {
        GameObject btn = Instantiate(Resources.Load("ButtonNormal")) as GameObject;
        btn.transform.SetParent(serverRoot.transform);
        btn.transform.localScale = Vector3.one;
        btn.transform.localPosition = Vector3.zero;
        btn.transform.localRotation = Quaternion.identity;
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            ServerInfo s = svr;
            //弹出一个连接框，告知正在与服务器连接
            if (CombatData.Ins.Server == CombatData.Ins.Servers[i])
            {
                if (TcpClientProxy.Ins.CheckNeedReConnect()) {
                    ConnectServerDialogState.State.Open();
                }
                return;
            }
            ConnectServerDialogState.State.Open();
            TcpClientProxy.Ins.Exit();
            ClearRooms();
            CombatData.Ins.Server = s;
            TcpClientProxy.Ins.ReStart();
        });
        btn.GetComponentInChildren<Text>().text = svr.ServerName;
    }

    void OnRefresh()
    {
        if (!TcpProtoHandler.Ins.VerifySuccess) {
            U3D.PopupTip("服务器验证失败 无法拉取房间列表");
            return;
        }
        TcpClientProxy.Ins.UpdateGameServer();
    }

    void OnCreateRoom()
    {
        if (TcpClientProxy.Ins.server == null || TcpClientProxy.Ins.sProxy == null || !TcpClientProxy.Ins.sProxy.Connected) {
            U3D.PopupTip("需要先选择服务器");
            return;
        }

        if (!TcpProtoHandler.Ins.VerifySuccess) {
            U3D.PopupTip("服务器验证失败 无法创建房间");
            return;
        }
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoomOptionDialogState);
    }

    void OnJoinRoom()
    {
        if (SelectRoomId != -1) {
            RoomInfo r = RoomMng.Ins.GetRoom(SelectRoomId);
            for (int i = 0; i < r.models.Count; i++) {
                ModelItem m = null;
                if (!GameStateMgr.Ins.gameStatus.IsModelInstalled((int)r.models[i], ref m)) {
                    if (m == null) {
                        m = DlcMng.Ins.GetModelMeta((int)r.models[i]);
                    }
                    U3D.PopupTip(string.Format("需要安装-模型{0}后方可进入房间", m == null ? r.models[i].ToString():m.Name));
                    return;
                }
            }
            TcpClientProxy.Ins.JoinRoom(SelectRoomId);
        }
    }
}