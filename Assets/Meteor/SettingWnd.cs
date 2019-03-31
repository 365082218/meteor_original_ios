using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net;

public class SettingWnd : Window<SettingWnd> {
    public override string PrefabName{get{return "SettingWnd"; }}
    protected override bool OnOpen()
    {
        Init();
        if (MainWnd.Exist)
            MainWnd.Instance.Close();
        return base.OnOpen();
    }

    Coroutine Update;
    Coroutine PluginUpdate;
    GameObject PluginRoot;
    List<GameObject> Install = new List<GameObject>();
    void Init()
    {
        Control("Return").GetComponent<Button>().onClick.AddListener(() => {
            GameData.Instance.SaveState();
            MainWnd.Instance.Open();
            Close();
        });

        Control("DeleteState").GetComponent<Button>().onClick.AddListener(() => {
            GameData.Instance.ResetState();
            SettingWnd.Instance.Close();
            SettingWnd.Instance.Open();
        });
        Control("DeletePlugins").GetComponent<Button>().onClick.AddListener(() => {
            GameData.Instance.gameStatus.pluginChapter.Clear();
            GameData.Instance.gameStatus.pluginModel.Clear();
            GameData.Instance.gameStatus.pluginNpc.Clear();
            Global.Instance.PluginUpdated = false;
            GameData.Instance.SaveState();
            SettingWnd.Instance.Close();
            SettingWnd.Instance.Open();
        });
        Control("ChangeLog").GetComponent<Text>().text = ResMng.LoadTextAsset("ChangeLog").text;
        Control("AppVerText").GetComponent<Text>().text = AppInfo.Instance.AppVersion();
        Control("MeteorVerText").GetComponent<Text>().text = AppInfo.Instance.MeteorVersion;

        Control("Nick").GetComponentInChildren<Text>().text = GameData.Instance.gameStatus.NickName;
        Control("Nick").GetComponent<Button>().onClick.AddListener(
            () =>
            {
                NickName.Instance.Open();
            }
        );
        Toggle highPerfor = Control("HighPerformance").GetComponent<Toggle>();
        highPerfor.isOn = GameData.Instance.gameStatus.TargetFrame == 60;
        highPerfor.onValueChanged.AddListener(OnChangePerformance);
        Toggle High = Control("High").GetComponent<Toggle>();
        Toggle Medium = Control("Medium").GetComponent<Toggle>();
        Toggle Low = Control("Low").GetComponent<Toggle>();
        High.isOn = GameData.Instance.gameStatus.Quality == 0;
        Medium.isOn = GameData.Instance.gameStatus.Quality == 1;
        Low.isOn = GameData.Instance.gameStatus.Quality == 2;
        High.onValueChanged.AddListener((bool selected) => { if (selected) GameData.Instance.gameStatus.Quality = 0; });
        Medium.onValueChanged.AddListener((bool selected) => { if (selected) GameData.Instance.gameStatus.Quality = 1; });
        Low.onValueChanged.AddListener((bool selected) => { if (selected) GameData.Instance.gameStatus.Quality = 2; });
        Toggle ShowTargetBlood = Control("ShowTargetBlood").GetComponent<Toggle>();
        ShowTargetBlood.isOn = GameData.Instance.gameStatus.ShowBlood;
        ShowTargetBlood.onValueChanged.AddListener((bool selected) => {  GameData.Instance.gameStatus.ShowBlood = selected; });

        if (Startup.ins != null)
        {
            Control("BGMSlider").GetComponent<Slider>().value = GameData.Instance.gameStatus.MusicVolume;
            Control("EffectSlider").GetComponent<Slider>().value = GameData.Instance.gameStatus.SoundVolume;
            //Control("HSliderBar").GetComponent<Slider>().value = GameData.Instance.gameStatus.AxisSensitivity.x;
            //Control("VSliderBar").GetComponent<Slider>().value = GameData.Instance.gameStatus.AxisSensitivity.y;
        }
        Control("BGMSlider").GetComponent<Slider>().onValueChanged.AddListener(OnMusicVolumeChange);
        Control("EffectSlider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectVolumeChange);
        //Control("HSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnXSensitivityChange);
        //Control("VSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnYSensitivityChange);
        Toggle ShowFPS = Control("ShowFPS").GetComponent<Toggle>();
        ShowFPS.isOn = GameData.Instance.gameStatus.ShowFPS;
        ShowFPS.onValueChanged.AddListener((bool selected) => { GameData.Instance.gameStatus.ShowFPS = selected; Startup.ins.ShowFps(selected); });

        Toggle ShowSysMenu2 = Control("ShowSysMenu2").GetComponent<Toggle>();
        ShowSysMenu2.isOn = GameData.Instance.gameStatus.ShowSysMenu2;
        ShowSysMenu2.onValueChanged.AddListener((bool selected) => { GameData.Instance.gameStatus.ShowSysMenu2 = selected; });
        GameObject pluginTab = Control("PluginTab", WndObject);
        PluginRoot = Control("Content", pluginTab);
        if (AppInfo.Instance.AppVersionIsSmallThan(GameConfig.Instance.newVersion))
        {
            //需要更新，设置好服务器版本号，设置好下载链接
            Control("NewVersionSep", WndObject).SetActive(true);
            Control("NewVersion", WndObject).GetComponent<Text>().text = string.Format("最新版本号:{0}", GameConfig.Instance.newVersion);
            Control("NewVersion", WndObject).SetActive(true);
            Control("GetNewVersion", WndObject).GetComponent<LinkLabel>().URL = GameConfig.Instance.apkUrl;
            Control("GetNewVersion", WndObject).SetActive(true);
            Control("Flag", WndObject).SetActive(true);
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            //Debug.Log("1");
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
        {
            //Debug.Log("2");
        }
        else
        if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            PluginUpdate = Startup.ins.StartCoroutine(UpdatePluginInfo());
        }
    }

    IEnumerator UpdatePluginInfo()
    {
        if (!Global.Instance.PluginUpdated)
        {
            UnityWebRequest vFile = new UnityWebRequest();
            vFile.url = string.Format(Main.strSFile, Main.strHost, Main.strProjectUrl, Main.strPlugins);
            vFile.timeout = 2;
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            vFile.downloadHandler = dH;
            yield return vFile.Send();
            if (vFile.isError || vFile.responseCode != 200)
            {
                Debug.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
                Control("Warning").SetActive(true);
                vFile.Dispose();
                Update = null;
                //显示出存档中保存得DLC信息
                DlcMng.Instance.ClearModel();
                for (int i = 0; i < GameData.Instance.gameStatus.pluginModel.Count; i++)
                {
                    DlcMng.Instance.Models.Add(GameData.Instance.gameStatus.pluginModel[i]);
                }
                for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
                {
                    InsertModel(DlcMng.Instance.Models[i]);
                }

                DlcMng.Instance.ClearDlc();
                for (int i = 0; i < GameData.Instance.gameStatus.pluginChapter.Count; i++)
                {
                    DlcMng.Instance.Dlcs.Add(GameData.Instance.gameStatus.pluginChapter[i]);
                }
                for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
                {
                    InsertDlc(DlcMng.Instance.Dlcs[i]);
                }
                yield break;
            }

            Control("Warning").SetActive(false);
            CleanModelList();
            LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
            for (int i = 0; i < js["Scene"].Count; i++)
            {
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

            for (int i = 0; i < js["Weapon"].Count; i++)
            {
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

            DlcMng.Instance.ClearModel();
            for (int i = 0; i < js["Model"].Count; i++)
            {
                int modelIndex = int.Parse(js["Model"][i]["Idx"].ToString());
                Debug.LogError(modelIndex + js["Model"][i]["name"].ToString());
                ModelItem Info = new ModelItem();
                Info.ModelId = modelIndex;
                Info.Name = js["Model"][i]["name"].ToString();
                Info.Path = js["Model"][i]["zip"].ToString();
                if (js["Model"][i]["desc"] != null)
                    Info.Desc = js["Model"][i]["desc"].ToString();
                DlcMng.Instance.AddModel(Info);
            }


            for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
            {
                InsertModel(DlcMng.Instance.Models[i]);
            }

            WebClient web = new WebClient();
            web.DownloadFile(string.Format(Main.strSFile, Main.strHost, Main.strProjectUrl, @"Npc\Npc.zip"), Application.persistentDataPath + @"\Plugins\Npc\Npc.zip");
            ZipUtility.UnzipFile(Application.persistentDataPath + @"\Plugins\Npc\Npc.zip", Application.persistentDataPath + @"\Plugins\Npc\", null, new UnzipCallbackEx(0));

            DlcMng.Instance.ClearDlc();
            for (int i = 0; i < js["Dlc"].Count; i++)
            {
                Chapter c = new Chapter();
                c.Installed = false;
                c.ChapterId = int.Parse(js["Dlc"][i]["Idx"].ToString());
                c.Name = js["Dlc"][i]["name"].ToString();
                c.Path = js["Dlc"][i]["zip"].ToString();
                if (js["Dlc"][i]["desc"] != null)
                    c.Desc = js["Dlc"][i]["desc"].ToString();
                if (js["Dlc"][i]["reference"] != null)
                {
                    Dependence dep = new Dependence();
                    for (int j = 0; j < js["Dlc"][i]["reference"].Count; j++)
                    {
                        if (js["Dlc"][i]["reference"][j]["Scene"] != null)
                        {
                            dep.scene = new List<int>();
                            for (int l = 0; l < js["Dlc"][i]["reference"][j]["Scene"].Count; l++)
                            {
                                dep.scene.Add(int.Parse(js["Dlc"][i]["reference"][j]["Scene"][l].ToString()));
                            }
                        }
                        if (js["Dlc"][i]["reference"][j]["Model"] != null)
                        {
                            dep.model = new List<int>();
                            for (int l = 0; l < js["Dlc"][i]["reference"][j]["Model"].Count; l++)
                            {
                                dep.model.Add(int.Parse(js["Dlc"][i]["reference"][j]["Model"][l].ToString()));
                            }
                        }

                        if (js["Dlc"][i]["reference"][j]["Weapon"] != null)
                        {
                            dep.weapon = new List<int>();
                            for (int l = 0; l < js["Dlc"][i]["reference"][j]["Weapon"].Count; l++)
                            {
                                dep.weapon.Add(int.Parse(js["Dlc"][i]["reference"][j]["Weapon"][l].ToString()));
                            }
                        }
                    }
                    c.Res = dep;
                }
                DlcMng.Instance.AddDlc(c);
            }

            for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
            {
                InsertDlc(DlcMng.Instance.Dlcs[i]);
            }
            Global.Instance.PluginUpdated = true;
            PluginUpdate = null;
        }
        else
        {
            for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
            {
                InsertModel(DlcMng.Instance.Models[i]);
            }
            for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
            {
                InsertDlc(DlcMng.Instance.Dlcs[i]);
            }
        }
    }

    void OnMusicVolumeChange(float vo)
    {
        SoundManager.Instance.SetMusicVolume(vo);
        if (Startup.ins != null)
            GameData.Instance.gameStatus.MusicVolume = vo;
    }

    void OnXSensitivityChange(float v)
    {
        GameData.Instance.gameStatus.AxisSensitivity.x = v;
    }

    void OnYSensitivityChange(float v)
    {
        GameData.Instance.gameStatus.AxisSensitivity.y = v;
    }

    void OnEffectVolumeChange(float vo)
    {
        SoundManager.Instance.SetSoundVolume(vo);
        GameData.Instance.gameStatus.SoundVolume = vo;
    }

    void OnChangePerformance(bool on)
    {
        GameData.Instance.gameStatus.TargetFrame = on ? 60 : 30;
        Application.targetFrameRate = GameData.Instance.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }

    public override void OnRefresh(int message, object param)
    {
        if (message == 0)
        {
            Control("Nick").GetComponentInChildren<Text>().text = GameData.Instance.gameStatus.NickName;
        }
    }

    protected override bool OnClose()
    {
        if (Update != null)
        {
            Startup.ins.StopCoroutine(Update);
            Update = null;
        }
        if (PluginUpdate != null)
        {
            Startup.ins.StopCoroutine(PluginUpdate);
            PluginUpdate = null;
        }

        for (int i = 0; i < Install.Count; i++)
        {
            GameObject.Destroy(Install[i]);
        }

        Install.Clear();
        return base.OnClose();
    }

    void CleanModelList()
    {
        for (int i = 0; i < Install.Count; i++)
        {
            GameObject.Destroy(Install[i]);
        }

        Install.Clear();
    }

    GameObject prefabPluginWnd;
    void InsertModel(ModelItem item)
    {
        if (prefabPluginWnd == null)
            prefabPluginWnd = ResMng.Load("PluginWnd") as GameObject;
        GameObject insert = GameObject.Instantiate(prefabPluginWnd);
        insert.transform.SetParent(PluginRoot.transform);
        insert.transform.localPosition = Vector3.zero;
        insert.transform.localScale = Vector3.one;
        insert.transform.localRotation = Quaternion.identity;
        PluginCtrl ctrl = insert.GetComponent<PluginCtrl>();
        if (ctrl != null)
        {
            ctrl.AttachModel(item);
        }
    }

    void InsertDlc(Chapter item)
    {
        if (prefabPluginWnd == null)
            prefabPluginWnd = ResMng.Load("PluginWnd") as GameObject;
        GameObject insert = GameObject.Instantiate(prefabPluginWnd);
        insert.transform.SetParent(PluginRoot.transform);
        insert.transform.localPosition = Vector3.zero;
        insert.transform.localScale = Vector3.one;
        insert.transform.localRotation = Quaternion.identity;
        PluginCtrl ctrl = insert.GetComponent<PluginCtrl>();
        if (ctrl != null)
        {
            ctrl.AttachDlc(item);
        }
    }

}
