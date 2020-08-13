using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DlcManagerDialogState : CommonDialogState<DlcManagerWnd> {
    public override string DialogName { get { return "DlcManagerWnd"; } }
    public DlcManagerDialogState(MainDialogStateManager stateMgr) : base(stateMgr) {

    }
}

public class DlcManagerWnd : Dialog {
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    Coroutine Update;
    Coroutine PluginPageUpdate;//插件翻页
    Coroutine PluginUpdate;
    GameObject PluginRoot;
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
        Control("Return").GetComponent<Button>().onClick.AddListener(() => {
            Main.Ins.GameStateMgr.SaveState();
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        });

        GameObject pluginTab = Control("ChapterTab", WndObject);
        Control("PluginPrev").GetComponent<Button>().onClick.AddListener(OnPrevPagePlugin);
        Control("PluginNext").GetComponent<Button>().onClick.AddListener(OnNextPagePlugin);
        PluginRoot = Control("Content", pluginTab);

        //模组分页内的功能设定
        Control("DeletePlugin").GetComponent<Button>().onClick.AddListener(() => { U3D.DeletePlugins(); SettingDialogState.Instance.ShowTab(4); });
        Toggle togShowInstallPlugin = Control("ShowInstallToggle").GetComponent<Toggle>();
        togShowInstallPlugin.onValueChanged.AddListener((bool value) => { this.showInstallPlugin = value; Main.Ins.DlcMng.CollectAll(this.showInstallPlugin); this.PluginPageRefreshEx(); });
        togShowInstallPlugin.isOn = true;

        Control("InstallAll").GetComponent<Button>().onClick.AddListener(OnInstallAll);
        UITab[] tabs = WndObject.GetComponentsInChildren<UITab>();
        for (int i = 0; i < tabs.Length; i++) {
            tabs[i].onValueChanged.AddListener(OnTabShow);
        }
        OnTabShow(true);
    }

    void OnInstallAll() {
        for (int i = 0; i < pluginList.Count; i++) {
            Main.Ins.DlcMng.AddDownloadTask(pluginList[i]);
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

    void OnTabShow(bool show) {
        if (Control("ChapterTab", WndObject).activeInHierarchy && show) {
            PluginUpdate = Main.Ins.StartCoroutine(UpdatePluginInfo());//下载插件信息，显示插件
        }
    }

    IEnumerator UpdatePluginInfo() {
        if (!Main.Ins.CombatData.PluginUpdated) {
            UnityWebRequest vFile = new UnityWebRequest();
            vFile.url = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strPlugins);
            vFile.timeout = 5;
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            vFile.downloadHandler = dH;
            yield return vFile.Send();
            if (vFile.isError || vFile.responseCode != 200) {
                Debug.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
                Control("Warning").SetActive(true);
                vFile.Dispose();
                Update = null;
                pluginCount = 0;
                //显示出存档中保存得DLC信息
                Main.Ins.DlcMng.ClearModel();
                for (int i = 0; i < Main.Ins.GameStateMgr.gameStatus.pluginModel.Count; i++) {
                    Main.Ins.DlcMng.Models.Add(Main.Ins.GameStateMgr.gameStatus.pluginModel[i]);
                }
                //for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
                //{
                //InsertModel(DlcMng.Instance.Models[i]);
                //}
                pluginCount = Main.Ins.DlcMng.Models.Count;
                Main.Ins.DlcMng.ClearDlc();
                for (int i = 0; i < Main.Ins.GameStateMgr.gameStatus.pluginChapter.Count; i++) {
                    Main.Ins.DlcMng.Dlcs.Add(Main.Ins.GameStateMgr.gameStatus.pluginChapter[i]);
                }
                pluginCount += Main.Ins.DlcMng.Dlcs.Count;
                //for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
                //{
                //    InsertDlc(DlcMng.Instance.Dlcs[i]);
                //}
                pluginPage = 0;
                pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
                Main.Ins.DlcMng.CollectAll(this.showInstallPlugin);
                Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
                PluginPageUpdate = Main.Ins.StartCoroutine(PluginPageRefresh());
                yield break;
            }

            Control("Warning").SetActive(false);
            pluginCount = 0;
            CleanModelList();
            LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
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

            Main.Ins.DlcMng.ClearModel();
            for (int i = 0; i < js["Model"].Count; i++) {
                int modelIndex = int.Parse(js["Model"][i]["Idx"].ToString());
                //Debug.LogError(modelIndex + js["Model"][i]["name"].ToString());
                ModelItem Info = new ModelItem();
                Info.ModelId = modelIndex;
                Info.Name = js["Model"][i]["name"].ToString();
                Info.Path = js["Model"][i]["zip"].ToString();
                if (js["Model"][i]["desc"] != null)
                    Info.Desc = js["Model"][i]["desc"].ToString();
                Info.useFemalePos = js["Model"][i]["gender"] != null;
                Main.Ins.DlcMng.AddModel(Info);
            }

            pluginCount += Main.Ins.DlcMng.Models.Count;
            //for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
            //{
            //    InsertModel(DlcMng.Instance.Models[i]);
            //}
            Main.Ins.DlcMng.ClearDlc();
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
                if (js["Dlc"][i]["reference"] != null) {
                    Dependence dep = new Dependence();
                    for (int j = 0; j < js["Dlc"][i]["reference"].Count; j++) {
                        if (js["Dlc"][i]["reference"][j]["Scene"] != null) {
                            dep.scene = new List<int>();
                            for (int l = 0; l < js["Dlc"][i]["reference"][j]["Scene"].Count; l++) {
                                dep.scene.Add(int.Parse(js["Dlc"][i]["reference"][j]["Scene"][l].ToString()));
                            }
                        }
                        if (js["Dlc"][i]["reference"][j]["Model"] != null) {
                            dep.model = new List<int>();
                            for (int l = 0; l < js["Dlc"][i]["reference"][j]["Model"].Count; l++) {
                                dep.model.Add(int.Parse(js["Dlc"][i]["reference"][j]["Model"][l].ToString()));
                            }
                        }

                        if (js["Dlc"][i]["reference"][j]["Weapon"] != null) {
                            dep.weapon = new List<int>();
                            for (int l = 0; l < js["Dlc"][i]["reference"][j]["Weapon"].Count; l++) {
                                dep.weapon.Add(int.Parse(js["Dlc"][i]["reference"][j]["Weapon"][l].ToString()));
                            }
                        }
                    }
                    c.Res = dep;
                }
                Main.Ins.DlcMng.AddDlc(c);
            }

            pluginCount += Main.Ins.DlcMng.Dlcs.Count;
            pluginPage = 0;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            Main.Ins.DlcMng.CollectAll(this.showInstallPlugin);
            PluginPageUpdate = Main.Ins.StartCoroutine(PluginPageRefresh());
            Main.Ins.CombatData.PluginUpdated = true;
            PluginUpdate = null;
        } else {
            if (PluginPageUpdate != null)
                yield break;
            pluginCount = Main.Ins.DlcMng.Models.Count + Main.Ins.DlcMng.Dlcs.Count;
            pluginPage = 0;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            PluginPageUpdate = Main.Ins.StartCoroutine(PluginPageRefresh());
        }
        Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
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
        for (int i = pluginPage * pluginPerPage; i < Mathf.Min((pluginPage + 1) * pluginPerPage, Main.Ins.DlcMng.allItem.Count); i++) {
            if (Main.Ins.DlcMng.allItem[i] is ModelItem)
                InsertModel(Main.Ins.DlcMng.allItem[i] as ModelItem);
            else
                InsertDlc(Main.Ins.DlcMng.allItem[i] as Chapter);
            yield return 0;
        }
        PluginPageUpdate = null;
    }

    public override void OnRefresh(int message, object param) {
        if (message == 0) {
            Control("Nick").GetComponentInChildren<Text>().text = Main.Ins.GameStateMgr.gameStatus.NickName;
        }
    }

    public override void OnClose() {
        if (Update != null) {
            Main.Ins.StopCoroutine(Update);
            Update = null;
        }
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
    List<MoreGameCtrl> GameList = new List<MoreGameCtrl>();
    List<PluginCtrl> pluginList = new List<PluginCtrl>();
    GameObject prefabPluginWnd;
    GameObject prefabGameItem;
    void InsertModel(ModelItem item) {
        if (prefabPluginWnd == null)
            prefabPluginWnd = ResMng.Load("PluginWnd") as GameObject;
        GameObject insert = GameObject.Instantiate(prefabPluginWnd);
        insert.transform.SetParent(PluginRoot.transform);
        insert.transform.localPosition = Vector3.zero;
        insert.transform.localScale = Vector3.one;
        insert.transform.localRotation = Quaternion.identity;
        PluginCtrl ctrl = insert.GetComponent<PluginCtrl>();
        if (ctrl != null) {
            ctrl.AttachModel(item);
            if (!Main.Ins.GameStateMgr.gameStatus.IsModelInstalled(item))
                Main.Ins.DlcMng.AddPreviewTask(ctrl);
        }
        pluginList.Add(ctrl);
    }

    void InsertDlc(Chapter item) {
        if (prefabPluginWnd == null)
            prefabPluginWnd = ResMng.Load("PluginWnd") as GameObject;
        GameObject insert = GameObject.Instantiate(prefabPluginWnd);
        insert.transform.SetParent(PluginRoot.transform);
        insert.transform.localPosition = Vector3.zero;
        insert.transform.localScale = Vector3.one;
        insert.transform.localRotation = Quaternion.identity;
        PluginCtrl ctrl = insert.GetComponent<PluginCtrl>();
        if (ctrl != null) {
            ctrl.AttachDlc(item);
            Chapter c;
            if (!Main.Ins.GameStateMgr.gameStatus.IsDlcInstalled(item, out c))
                Main.Ins.DlcMng.AddPreviewTask(ctrl);
        }
        pluginList.Add(ctrl);
    }
}

