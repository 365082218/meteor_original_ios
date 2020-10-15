using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerListDialogState : CommonDialogState<ServerListDialog>
{
    public override string DialogName { get { return "ServerListDialog"; } }
    public ServerListDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class ServerListDialog : Dialog
{
    public const int ADD = 1;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    public override void OnRefresh(int message, object param)
    {
        switch (message)
        {
            case ADD:
                ServerInfo info = param as ServerInfo;
                GameObject prefab = Resources.Load("UI/Dialogs/SelectListItem") as GameObject;
                InsertServerItem(info, prefab);
                break;
        }
    }

    GameObject ServerListRoot;
    void Init()
    {
        ServerListRoot = Control("ServerListRoot");
        GameObject prefab = Resources.Load("UI/Dialogs/SelectListItem") as GameObject;
        for (int i = 0; i < serverList.Count; i++)
        {
            GameObject.Destroy(serverList[i]);
        }
        serverList.Clear();
        for (int i = 0; i < GameStateMgr.Ins.gameStatus.ServerList.Count; i++)
        {
            InsertServerItem(GameStateMgr.Ins.gameStatus.ServerList[i], prefab);
        }
        GameObject defaultServer = Control("SelectListItem");
        Text text = Control("Text", defaultServer).GetComponent<Text>();
        Control("Delete").GetComponent<Button>().onClick.AddListener(() =>
        {
            //不能删除默认
            if (selectServer != null)
            {
                int selectServerId = GameStateMgr.Ins.gameStatus.ServerList.IndexOf(selectServer);
                if (selectServerId != -1)
                {
                    GameObject.Destroy(serverList[selectServerId]);
                    serverList.RemoveAt(selectServerId);
                    CombatData.Ins.OnServiceChanged(-1, GameStateMgr.Ins.gameStatus.ServerList[selectServerId]);
                    GameStateMgr.Ins.gameStatus.ServerList.RemoveAt(selectServerId);
                }
                if (selectServerId >= serverList.Count)
                    selectServerId = 0;
                if (selectServerId < GameStateMgr.Ins.gameStatus.ServerList.Count) {
                    selectServer = GameStateMgr.Ins.gameStatus.ServerList[selectServerId];
                }
                selectedBtn = null;
            }
        });
        Control("Close").GetComponent<Button>().onClick.AddListener(() => { Close(); });
        Control("AddHost").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.HostEditDialogState);
        });

        text.text = CombatData.Ins.Server.ServerName + string.Format(":{0}", CombatData.Ins.Server.ServerPort);
    }

    List<GameObject> serverList = new List<GameObject>();
    Button selectedBtn;
    ServerInfo selectServer;
    public void InsertServerItem(ServerInfo server, GameObject prefab)
    {
        GameObject host = GameObject.Instantiate(prefab, ServerListRoot.transform);
        host.layer = ServerListRoot.layer;
        host.transform.localPosition = Vector3.zero;
        host.transform.localScale = Vector3.one;
        host.transform.rotation = Quaternion.identity;
        host.transform.SetParent(ServerListRoot.transform);
        Control("Text", host).GetComponent<Text>().text = server.ServerName + string.Format(":{0}", server.ServerPort);
        Button btn = host.GetComponent<Button>();
        btn.onClick.AddListener(() => { OnSelectServer(server, btn); });
        serverList.Add(host);
    }

    void OnSelectServer(ServerInfo svr, Button btn)
    {
        if (selectedBtn != null)
        {
            selectedBtn.image.color = new Color(1, 1, 1, 0);
            selectedBtn = null;
        }
        btn.image.color = new Color(144.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f, 104.0f / 255.0f);
        selectedBtn = btn;
        selectServer = svr;
    }

    void Close() {
        GameStateMgr.Ins.SaveState();
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainLobbyDialogState);
    }
}
