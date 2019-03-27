using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;

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
            Update = Startup.ins.StartCoroutine(UpdateAppInfo());
            PluginUpdate = Startup.ins.StartCoroutine(UpdatePluginInfo());
        }
    }

    IEnumerator UpdateAppInfo()
    {
        UnityWebRequest vFile = new UnityWebRequest();
        vFile.url = string.Format(Main.strVFile, Main.strHost, Main.strProjectUrl, Main.strPlatform, Main.strNewVersionName);
        vFile.timeout = 2;
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        vFile.downloadHandler = dH;
        yield return vFile.Send();
        if (vFile.isError || vFile.responseCode != 200)
        {
            Debug.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
            vFile.Dispose();
            Update = null;
            yield break;
        }

        LitJson.JsonData js = LitJson.JsonMapper.ToObject(dH.text);
        string serverVersion = js["newVersion"].ToString();
        Debug.Log(serverVersion);
        if (AppInfo.Instance.AppVersionIsSmallThan(serverVersion))
        {
            //需要更新，设置好服务器版本号，设置好下载链接
            Control("NewVersionSep", WndObject).SetActive(true);
            Control("NewVersion", WndObject).GetComponent<Text>().text = string.Format("最新版本号:{0}", serverVersion);
            Control("NewVersion", WndObject).SetActive(true);
            Control("GetNewVersion", WndObject).GetComponent<LinkLabel>().URL = string.Format(Main.strVFile, Main.strHost, Main.strProjectUrl, Main.strPlatform, "Meteor" + serverVersion + ".apk");
            Control("GetNewVersion", WndObject).SetActive(true);
            Control("Flag", WndObject).SetActive(true);
        }
        else
        {
            //Debug.Log("无需更新");
        }
        Update = null;
    }

    IEnumerator UpdatePluginInfo()
    {
        UnityWebRequest vFile = new UnityWebRequest();
        vFile.url = string.Format(Main.strSourcePath, Main.strHost, Main.strProjectUrl, Main.strPlugins);
        vFile.timeout = 2;
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        vFile.downloadHandler = dH;
        yield return vFile.Send();
        if (vFile.isError || vFile.responseCode != 200)
        {
            Debug.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
            vFile.Dispose();
            Update = null;
            yield break;
        }

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

        ModelPlugin.Instance.ClearModel();
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
            Info.IcoPath = Info.Path.Substring(0,  Info.Path.Length - 4) + ".jpg";
            ModelPlugin.Instance.AddModel(Info);
        }

        CleanModelList();
        for (int i = 0; i < ModelPlugin.Instance.Models.Count; i++)
        {
            InsertModel(ModelPlugin.Instance.Models[i]);
        }

        for (int i = 0; i < js["Npc"].Count; i++)
        {
            //Global.Instance.Servers.Add(s);
        }

        for (int i = 0; i < js["Dlc"].Count; i++)
        {
            //for (int j = 0; j < js["Dlc"][i]["Level"].Count; j++)
            //{

            //}
            //Global.Instance.Servers.Add(s);
        }

        PluginUpdate = null;
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

}
