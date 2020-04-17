using Idevgame.GameState.DialogState;
using protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MainLobbyDialogState : CommonDialogState<MainLobbyDialog>
{
    public override string DialogName { get { return "MainLobby"; } }
    public MainLobbyDialogState(MainDialogStateManager stateMgr):base(stateMgr)
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
    }

    public override void OnClose()
    {
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

    public void OnGetRoom(GetRoomRsp rsp)
    {
        ClearRooms();
        int cnt = rsp.RoomInLobby.Count;
        GameObject prefab = Resources.Load<GameObject>("RoomInfoItem");
        for (int i = 0; i < cnt; i++)
            InsertRoomItem(rsp.RoomInLobby[i], prefab);
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
        Control("Pattern", roomObj).GetComponent<Text>().text = "";
        Control("Password", roomObj).GetComponent<Text>().text = "无";
        Control("Rule", roomObj).GetComponent<Text>().text = ruleS[(int)room.rule - 1];//盟主，死斗，暗杀
        Control("LevelName", roomObj).GetComponent<Text>().text = Main.Ins.DataMgr.GetData<LevelDatas.LevelDatas>((int)room.levelIdx).Name;
        Control("Version", roomObj).GetComponent<Text>().text = Main.Ins.AppInfo.MeteorVersion;
        Control("Ping", roomObj).GetComponent<Text>().text = "???";
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

    public void OnSelectService()
    {
        Control("Server").GetComponent<Text>().text = string.Format("服务器:{0}        IP:{1}        端口:{2}", Main.Ins.CombatData.Server.ServerName,
            TcpClientProxy.server == null ? "还未取得" : TcpClientProxy.server.Address.ToString(), TcpClientProxy.server == null ? "还未取得" : TcpClientProxy.server.Port.ToString());
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
            OnPreviousPress();
        });

        RoomRoot = Control("RoomRoot");
        Control("Version").GetComponent<Text>().text = Main.Ins.GameStateMgr.gameStatus.MeteorVersion;
        if (Main.Ins.CombatData.Servers.Count == 0)
            Update = Main.Ins.StartCoroutine(UpdateServiceList());
        else
            OnGetServerListDone();
    }

    GameObject serverRoot;
    IEnumerator UpdateServiceList()
    {
        UnityWebRequest vFile = new UnityWebRequest();
        vFile.url = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strServices);
        vFile.timeout = 5;
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        vFile.downloadHandler = dH;
        yield return vFile.Send();
        if (vFile.isError || vFile.responseCode != 200 || string.IsNullOrEmpty(dH.text))
        {
            Debug.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
            vFile.Dispose();
            Update = null;
            yield break;
        }

        Main.Ins.CombatData.Servers.Clear();
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
            Main.Ins.CombatData.Servers.Add(s);
        }
        Update = null;

        //合并所有服务器到全局变量里
        for (int i = 0; i < Main.Ins.GameStateMgr.gameStatus.ServerList.Count; i++)
            Main.Ins.CombatData.Servers.Add(Main.Ins.GameStateMgr.gameStatus.ServerList[i]);

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

        Main.Ins.CombatData.Server = Main.Ins.CombatData.Servers[0];
        GameObject Services = Control("Services", WndObject);
        serverRoot = Control("Content", Services);
        int childNum = serverRoot.transform.childCount;
        for (int i = 0; i < childNum; i++)
        {
            GameObject.Destroy(serverRoot.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < Main.Ins.CombatData.Servers.Count; i++)
        {
            InsertServerItem(Main.Ins.CombatData.Servers[i], i);
        }
        TcpClientProxy.ReStart();
    }

    void InsertServerItem(ServerInfo svr, int i)
    {
        GameObject btn = GameObject.Instantiate(ResMng.Load("ButtonNormal")) as GameObject;
        btn.transform.SetParent(serverRoot.transform);
        btn.transform.localScale = Vector3.one;
        btn.transform.localPosition = Vector3.zero;
        btn.transform.localRotation = Quaternion.identity;
        btn.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (Main.Ins.CombatData.Server == Main.Ins.CombatData.Servers[i])
            {
                TcpClientProxy.CheckNeedReConnect();
                return;
            }
            TcpClientProxy.Exit();
            ClearRooms();
            Main.Ins.CombatData.Server = svr;
            TcpClientProxy.ReStart();
        });
        btn.GetComponentInChildren<Text>().text = svr.ServerName;
    }

    void OnRefresh()
    {
        TcpClientProxy.UpdateGameServer();
    }

    void OnCreateRoom()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoomOptionDialogState);
    }

    void OnJoinRoom()
    {
        if (SelectRoomId != -1)
        {
            TcpClientProxy.JoinRoom(SelectRoomId);
        }
    }
}