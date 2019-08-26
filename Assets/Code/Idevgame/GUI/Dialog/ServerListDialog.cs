using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerListDialogState : CommonDialogState<ServerListDialog>
{
    public override string DialogName { get { return "ServerListDialog"; } }
    public ServerListDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
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
                GameObject prefab = ResMng.LoadPrefab("SelectListItem") as GameObject;
                InsertServerItem(info, prefab);
                break;
        }
    }

    GameObject ServerListRoot;
    void Init()
    {
        ServerListRoot = Control("ServerListRoot");
        GameObject prefab = ResMng.LoadPrefab("SelectListItem") as GameObject;
        for (int i = 0; i < serverList.Count; i++)
        {
            GameObject.Destroy(serverList[i]);
        }
        serverList.Clear();
        for (int i = 0; i < GameData.Instance.gameStatus.ServerList.Count; i++)
        {
            InsertServerItem(GameData.Instance.gameStatus.ServerList[i], prefab);
        }
        GameObject defaultServer = Control("SelectListItem");
        Text text = Control("Text", defaultServer).GetComponent<Text>();
        Control("Delete").GetComponent<Button>().onClick.AddListener(() =>
        {
            //不能删除默认
            if (selectServer != null)
            {
                int selectServerId = GameData.Instance.gameStatus.ServerList.IndexOf(selectServer);
                if (selectServerId != -1)
                {
                    GameObject.Destroy(serverList[selectServerId]);
                    serverList.RemoveAt(selectServerId);
                    Global.Instance.OnServiceChanged(-1, GameData.Instance.gameStatus.ServerList[selectServerId]);
                    GameData.Instance.gameStatus.ServerList.RemoveAt(selectServerId);
                }
                if (selectServerId >= serverList.Count)
                    selectServerId = 0;
                selectServer = GameData.Instance.gameStatus.ServerList[selectServerId];
                selectedBtn = null;
            }
        });
        Control("Close").GetComponent<Button>().onClick.AddListener(() => { OnPreviousPress(); });
        Control("AddHost").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.HostEditDialogState);
        });

        text.text = Global.Instance.Server.ServerName + string.Format(":{0}", Global.Instance.Server.ServerPort);
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
}
