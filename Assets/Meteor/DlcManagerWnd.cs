using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DlcManagerDialogState : CommonDialogState<DlcManagerWnd> {
    public override string DialogName { get { return "DlcManagerWnd"; } }
    public DlcManagerDialogState(MainDialogMgr stateMgr) : base(stateMgr) {

    }
}

public class DlcManagerWnd : Dialog {
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        filter = data == null ? 0 : (int)data;
        Init();
    }

    Coroutine PluginPageUpdate;//插件翻页
    Coroutine PluginUpdate;//重新更新插件列表信息
    GameObject ChapterRoot;
    GameObject ModelRoot;
    GameObject PluginRoot {
        get {
            return filter == 0 ? ChapterRoot : ModelRoot;
        }
    }
    GameObject DebugRoot;
    int gamePage;
    int gamePageMax;
    const int pluginPerPage = 12;//一页最大插件数量
    int pluginPage;//当前插件页
    int pageMax;//最大页
    int pluginCount;//插件数量
    bool showInstallPlugin = true;
    List<GameObject> Install = new List<GameObject>();
    void Init() {
        GameObject chapterTab = Control("ChapterTab", WndObject);
        GameObject modelTab = Control("ModelTab", WndObject);
        Control("Return").GetComponent<Button>().onClick.AddListener(() => {
            Close();
        });
        Control("PluginPrev").GetComponent<Button>().onClick.AddListener(OnPrevPagePlugin);
        Control("PluginNext").GetComponent<Button>().onClick.AddListener(OnNextPagePlugin);
        Control("ResetModel").GetComponent<Button>().onClick.AddListener(()=> {
            GameStateMgr.Ins.gameStatus.UseModel = -1;
            U3D.PopupTip("已设置使用默认角色");
        });
        ChapterRoot = Control("Content", chapterTab);
        ModelRoot = Control("Content", modelTab);
        //模组分页内的功能设定
        Control("DeletePlugin").GetComponent<Button>().onClick.AddListener(() => {
            U3D.DeletePlugins(filter);
            int f = filter;
            Close();
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.DlcManagerDialogState, f);
        });
        Toggle togShowInstallPlugin = Control("ShowInstallToggle").GetComponent<Toggle>();
        togShowInstallPlugin.onValueChanged.AddListener((bool value) => { this.showInstallPlugin = value; DlcMng.Ins.CollectAll(this.showInstallPlugin, filter); this.PluginPageRefreshEx(); });
        togShowInstallPlugin.isOn = true;

        Control("InstallAll").GetComponent<Button>().onClick.AddListener(OnInstallAll);
        UITab select = null;
        UITab[] tabs = WndObject.GetComponentsInChildren<UITab>();
        for (int i = 0; i < tabs.Length; i++) {
            tabs[i].onValueChanged.AddListener(OnTabShow);
            if (filter == i)
                select = tabs[i];
        }
        if (select != null) {
            if (!select.isOn) {
                select.Select();
                return;
            }
        }
        OnTabShow(true);
    }

    void Close() {
        GameStateMgr.Ins.SaveState();
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
    }

    void OnInstallAll() {
        for (int i = 0; i < pluginList.Count; i++) {
            if (pluginList[i].Chapter != null) {
                DownloadManager.Ins.AddTask(TaskType.Chapter, pluginList[i].Chapter.ChapterId);
            }
            else {
                DownloadManager.Ins.AddTask(TaskType.Model, pluginList[i].Model.ModelId);
            }
        }
    }

    public void ShowTab(int tab) {
        GameObject grid = Control("Tabs Grid");
        Transform tabCtrl = grid.transform.GetChild(tab);
        if (tabCtrl != null) {
            UITab t = tabCtrl.GetComponent<UITab>();
            t.isOn = true;
        }
    }

    int filter;
    void OnTabShow(bool show) {
        if (Control("ChapterTab", WndObject).activeInHierarchy && show) {
            filter = 0;
        } else if (Control("ModelTab", WndObject).activeInHierarchy && show) {
            filter = 1;
        }
        if (show) {
            pluginPage = 0;
            OnTabEnter();
        }
    }

    void OnTabEnter() {
        if (CombatData.Ins.PluginUpdated) {
            //已经更新过插件列表
            if (PluginPageUpdate != null)//翻页中
                Main.Ins.StopCoroutine(PluginPageUpdate);
            
            DlcMng.Ins.CollectAll(this.showInstallPlugin, filter);
            pluginCount = DlcMng.Ins.allItem.Count;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            if (PluginPageUpdate != null) {
                Main.Ins.StopCoroutine(PluginPageUpdate);
            }
            PluginPageUpdate = Main.Ins.StartCoroutine(PluginPageRefresh());
            Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
        } else {
            if (PluginUpdate == null)
                PluginUpdate = Main.Ins.StartCoroutine(UpdatePluginInfo());//下载插件信息，显示插件
        }
    }
    IEnumerator UpdatePluginInfo() {
        UnityWebRequest vFile = new UnityWebRequest();
        vFile.url = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strPlugins);
        vFile.timeout = 5;
        WaitDialogState.State.Open("正在拉取模组信息");
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        vFile.downloadHandler = dH;
        yield return vFile.Send();
        WaitDialogState.State.WaitExit(1.0f);
        if (vFile.isNetworkError || vFile.responseCode != 200) {
            Debug.Log(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
            Control("Warning").SetActive(false);//
            U3D.InsertSystemMsg("无法连接至服务器");
            vFile.Dispose();
            pluginCount = 0;
            //显示出存档中保存得DLC信息
            DlcMng.Ins.ClearModel();
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginModel.Count; i++) {
                if (GameStateMgr.Ins.gameStatus.pluginModel[i] == null)
                    continue;
                GameStateMgr.Ins.gameStatus.pluginModel[i].Check();
                if (GameStateMgr.Ins.gameStatus.pluginModel[i].Installed)
                    DlcMng.Ins.Models.Add(GameStateMgr.Ins.gameStatus.pluginModel[i]);
            }
            DlcMng.Ins.ClearDlc();
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginChapter.Count; i++) {
                if (GameStateMgr.Ins.gameStatus.pluginChapter[i] == null)
                    continue;
                GameStateMgr.Ins.gameStatus.pluginChapter[i].Check();
                if (GameStateMgr.Ins.gameStatus.pluginChapter[i].Installed)
                    DlcMng.Ins.Dlcs.Add(GameStateMgr.Ins.gameStatus.pluginChapter[i]);
            }

            pluginPage = 0;
            DlcMng.Ins.CollectAll(this.showInstallPlugin, filter);
            pluginCount = DlcMng.Ins.allItem.Count;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);

            Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
            PluginPageUpdate = Main.Ins.StartCoroutine(PluginPageRefresh());
            PluginUpdate = null;
            yield break;
        } else {
            Control("Warning").SetActive(false);
            CleanModelList();
            LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
            Main.Ins.baseUrl = js["url"].ToString();
            for (int i = 0; i < js["Scene"].Count; i++) {
                //ServerInfo s = new ServerInfo();
                //if (!int.TryParse(js["services"][i]["port"].ToString(), out s.ServerPort))
                //    continue;
                //if (!int.TryParse(js["services"][i]["type"].ToString(), out s.type))
                //    continue;
                //if (s.type == 0)
                //    s.ServerHost = js["services"][i]["addr"].ToString();
                //else
                //    s.ServerIP = js["services"][i]["addr"].ToString();
                //s.ServerName = js["services"][i]["name"].ToString();
                //Global.Instance.Servers.Add(s);
            }

            for (int i = 0; i < js["Weapon"].Count; i++) {
                //ServerInfo s = new ServerInfo();
                //if (!int.TryParse(js["services"][i]["port"].ToString(), out s.ServerPort))
                //    continue;
                //if (!int.TryParse(js["services"][i]["type"].ToString(), out s.type))
                //    continue;
                //if (s.type == 0)
                //    s.ServerHost = js["services"][i]["addr"].ToString();
                //else
                //    s.ServerIP = js["services"][i]["addr"].ToString();
                //s.ServerName = js["services"][i]["name"].ToString();
                //Global.Instance.Servers.Add(s);
            }

            DlcMng.Ins.ClearModel();
            for (int i = 0; i < js["Model"].Count; i++) {
                int modelIndex = int.Parse(js["Model"][i]["Idx"].ToString());
                //Debug.LogError(modelIndex + js["Model"][i]["name"].ToString());
                ModelItem Info = new ModelItem();
                Info.ModelId = modelIndex;
                Info.Name = js["Model"][i]["name"].ToString();
                Info.Path = js["Model"][i]["zip"].ToString();
                if (js["Model"][i]["desc"] != null)
                    Info.Desc = js["Model"][i]["desc"].ToString();
                Info.useFemalePos = js["Model"][i]["gender"] != null ? js["Model"][i]["gender"].ToString() == "1" : false;
                DlcMng.Ins.AddModel(Info);
            }
            DlcMng.Ins.ClearDlc();
            for (int i = 0; i < js["Dlc"].Count; i++) {
                Chapter c = new Chapter();
                c.Installed = false;
                c.ChapterId = int.Parse(js["Dlc"][i]["Idx"].ToString());
                c.Name = js["Dlc"][i]["name"].ToString();
                c.Path = js["Dlc"][i]["zip"].ToString();
                if (js["Dlc"][i]["version"] != null)
                    c.version = js["Dlc"][i]["version"].ToString();
                if (js["Dlc"][i]["desc"] != null)
                    c.Desc = js["Dlc"][i]["desc"].ToString();
                DlcMng.Ins.AddDlc(c);
            }
            pluginPage = 0;
            DlcMng.Ins.CollectAll(this.showInstallPlugin, filter);
            pluginCount = DlcMng.Ins.allItem.Count;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            PluginPageUpdate = Main.Ins.StartCoroutine(PluginPageRefresh());
            CombatData.Ins.PluginUpdated = true;
            PluginUpdate = null;
            Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
        }
    }

    void OnPrevPagePlugin() {
        if (pluginPage != 0)
            pluginPage--;
        else
            return;
        PluginPageRefreshEx();
    }

    void OnNextPagePlugin() {
        if (pluginPage < pageMax - 1)
            pluginPage++;
        else
            return;
        PluginPageRefreshEx();
    }

    void PluginPageRefreshEx() {
        Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
        if (PluginPageUpdate != null)
            Main.Ins.StopCoroutine(PluginPageUpdate);
        PluginPageUpdate = Main.Ins.StartCoroutine(PluginPageRefresh());
    }

    IEnumerator PluginPageRefresh() {
        for (int i = 0; i < pluginList.Count; i++) {
            GameObject.Destroy(pluginList[i].gameObject);
        }
        pluginList.Clear();
        for (int i = pluginPage * pluginPerPage; i < Mathf.Min((pluginPage + 1) * pluginPerPage, DlcMng.Ins.allItem.Count); i++) {
            if (DlcMng.Ins.allItem[i] is ModelItem)
                InsertModel(DlcMng.Ins.allItem[i] as ModelItem);
            else
                InsertDlc(DlcMng.Ins.allItem[i] as Chapter);
            yield return 0;
        }
        PluginPageUpdate = null;
    }

    public override void OnRefresh(int message, object param) {
        for (int i = 0; i < pluginList.Count; i++) {
            pluginList[i].OnStateChange(message, (TaskType)param);
        }
    }

    public override void OnClose() {
        if (PluginUpdate != null) {
            Main.Ins.StopCoroutine(PluginUpdate);
            PluginUpdate = null;
        }

        if (PluginPageUpdate != null) {
            Main.Ins.StopCoroutine(PluginPageUpdate);
            PluginPageUpdate = null;
        }

        for (int i = 0; i < Install.Count; i++) {
            GameObject.Destroy(Install[i]);
        }

        Install.Clear();
    }

    void CleanModelList() {
        for (int i = 0; i < Install.Count; i++) {
            GameObject.Destroy(Install[i]);
        }

        Install.Clear();
    }
    //List<MoreGameCtrl> GameList = new List<MoreGameCtrl>();
    List<PluginCtrl> pluginList = new List<PluginCtrl>();
    GameObject prefabPluginWnd;
    GameObject prefabGameItem;
    void InsertModel(ModelItem item) {
        if (prefabPluginWnd == null)
            prefabPluginWnd = Resources.Load("PluginWnd") as GameObject;
        GameObject insert = GameObject.Instantiate(prefabPluginWnd);
        insert.transform.SetParent(PluginRoot.transform);
        insert.transform.localPosition = Vector3.zero;
        insert.transform.localScale = Vector3.one;
        insert.transform.localRotation = Quaternion.identity;
        PluginCtrl ctrl = insert.GetComponent<PluginCtrl>();
        if (ctrl != null) {
            ctrl.AttachModel(item);
        }
        pluginList.Add(ctrl);
    }

    void InsertDlc(Chapter item) {
        if (prefabPluginWnd == null)
            prefabPluginWnd = Resources.Load("PluginWnd") as GameObject;
        GameObject insert = GameObject.Instantiate(prefabPluginWnd);
        insert.transform.SetParent(PluginRoot.transform);
        insert.transform.localPosition = Vector3.zero;
        insert.transform.localScale = Vector3.one;
        insert.transform.localRotation = Quaternion.identity;
        PluginCtrl ctrl = insert.GetComponent<PluginCtrl>();
        if (ctrl != null) {
            ctrl.AttachDlc(item);
        }
        pluginList.Add(ctrl);
    }
}

