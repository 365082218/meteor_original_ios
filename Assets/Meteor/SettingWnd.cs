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
    Coroutine PluginPageUpdate;//插件翻页
    Coroutine GamePageUpdate;//游戏推荐页翻页
    Coroutine PluginUpdate;
    GameObject PluginRoot;
    GameObject GameRoot;
    int gamePage;
    int gamePageMax;
    const int pluginPerPage = 12;//一页最大插件数量
    int pluginPage;//当前插件页
    int pageMax;//最大页
    int pluginCount;//插件数量
    bool showInstallPlugin = true;
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
        Toggle ShowFPS = Control("ShowFPS").GetComponent<Toggle>();
        ShowFPS.isOn = GameData.Instance.gameStatus.ShowFPS;
        ShowFPS.onValueChanged.AddListener((bool selected) => { GameData.Instance.gameStatus.ShowFPS = selected; Startup.ins.ShowFps(selected); });

        Toggle ShowSysMenu2 = Control("ShowSysMenu2").GetComponent<Toggle>();
        ShowSysMenu2.isOn = GameData.Instance.gameStatus.ShowSysMenu2;
        ShowSysMenu2.onValueChanged.AddListener((bool selected) => { GameData.Instance.gameStatus.ShowSysMenu2 = selected; });

        if (Startup.ins != null)
        {
            Control("BGMSlider").GetComponent<Slider>().value = GameData.Instance.gameStatus.MusicVolume;
            Control("EffectSlider").GetComponent<Slider>().value = GameData.Instance.gameStatus.SoundVolume;
            Control("HSliderBar").GetComponent<Slider>().value = GameData.Instance.gameStatus.AxisSensitivity.x;
            Control("VSliderBar").GetComponent<Slider>().value = GameData.Instance.gameStatus.AxisSensitivity.y;
        }
        Control("BGMSlider").GetComponent<Slider>().onValueChanged.AddListener(OnMusicVolumeChange);
        Control("EffectSlider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectVolumeChange);
        Control("HSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnXSensitivityChange);
        Control("VSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnYSensitivityChange);
        Control("SetJoyPosition").GetComponent<Button>().onClick.AddListener(OnSetUIPosition);
        
        //显示战斗界面的调试按钮
        Toggle toggleDebug = Control("EnableSFX").GetComponent<Toggle>();
        toggleDebug.isOn = GameData.Instance.gameStatus.EnableDebugSFX;
        toggleDebug.onValueChanged.AddListener(OnEnableDebugSFX);
        //显示战斗界面的调试按钮
        Toggle toggleRobot = Control("EnableRobot").GetComponent<Toggle>();
        toggleRobot.isOn = GameData.Instance.gameStatus.EnableDebugRobot;
        toggleRobot.onValueChanged.AddListener(OnEnableDebugRobot);

        //显示武器挑选按钮
        Toggle toggleEnableFunc = Control("EnableWeaponChoose").GetComponent<Toggle>();
        toggleEnableFunc.isOn = GameData.Instance.gameStatus.EnableWeaponChoose;
        toggleEnableFunc.onValueChanged.AddListener(OnEnableWeaponChoose);
        //无限气
        Toggle toggleEnableInfiniteAngry = Control("EnableInfiniteAngry").GetComponent<Toggle>();
        toggleEnableInfiniteAngry.isOn = GameData.Instance.gameStatus.EnableInfiniteAngry;
        toggleEnableInfiniteAngry.onValueChanged.AddListener(OnEnableInfiniteAngry);

        //无锁定
        Toggle toggleDisableLock = Control("CameraLock").GetComponent<Toggle>();
        toggleDisableLock.isOn = GameData.Instance.gameStatus.AutoLock;
        toggleDisableLock.onValueChanged.AddListener(OnDisableLock);

        Toggle toggleEnableGodMode = Control("EnableGodMode").GetComponent<Toggle>();
        toggleEnableGodMode.isOn = GameData.Instance.gameStatus.EnableGodMode;
        toggleEnableGodMode.onValueChanged.AddListener(OnEnableGodMode);

        Toggle toggleEnableUndead = Control("EnableUnDead").GetComponent<Toggle>();
        toggleEnableUndead.isOn = GameData.Instance.gameStatus.Undead;
        toggleEnableUndead.onValueChanged.AddListener(OnEnableUndead);

        Toggle toggleShowWayPoint = Control("ShowWayPoint").GetComponent<Toggle>();
        toggleShowWayPoint.isOn = GameData.Instance.gameStatus.ShowWayPoint;
        toggleShowWayPoint.onValueChanged.AddListener(OnShowWayPoint);
        if (GameData.Instance.gameStatus.ShowWayPoint)
            OnShowWayPoint(true);

        Toggle toggleEnableLog = Control("ShowLog").GetComponent<Toggle>();
        toggleEnableLog.isOn = GameData.Instance.gameStatus.EnableLog;
        toggleEnableLog.onValueChanged.AddListener(OnEnableLog);
        OnEnableLog(toggleEnableLog.isOn);

        Toggle toggleLevelDebug = Control("ShowLevelDebugButton").GetComponent<Toggle>();
        toggleLevelDebug.isOn = GameData.Instance.gameStatus.LevelDebug;
        toggleLevelDebug.onValueChanged.AddListener(OnLevelDebug);
        OnLevelDebug(toggleLevelDebug.isOn);

        Control("ChangeV107").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("1.07"); });
        Control("ChangeV907").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("9.07"); });
        Control("UnlockAll").GetComponent<Button>().onClick.AddListener(() => { U3D.UnlockLevel(); });

        //粒子特效
        Toggle toggleDisableParticle = Control("Particle").GetComponent<Toggle>();
        toggleDisableParticle.isOn = GameData.Instance.gameStatus.DisableParticle;
        toggleDisableParticle.onValueChanged.AddListener(OnDisableParticle);
        OnDisableParticle(toggleDisableParticle.isOn);

        //关闭摇杆
        Toggle toggleDisableJoyStick = Control("Joystick").GetComponent<Toggle>();
        toggleDisableJoyStick.isOn = GameData.Instance.gameStatus.DisableJoystick;
        toggleDisableJoyStick.onValueChanged.AddListener(OnDisableJoyStick);
        OnDisableJoyStick(toggleDisableJoyStick.isOn);

        //宠物猫
        Toggle toggleDisableCat = Control("Cat").GetComponent<Toggle>();
        toggleDisableCat.isOn = GameData.Instance.gameStatus.PetOn;
        toggleDisableCat.onValueChanged.AddListener(OnDisableCat);
        OnDisableCat(toggleDisableCat.isOn);

        Toggle toggleSkipVideo = Control("SkipVideo").GetComponent<Toggle>();
        toggleSkipVideo.isOn = GameData.Instance.gameStatus.SkipVideo;
        toggleSkipVideo.onValueChanged.AddListener(OnSkipVideo);

        GameObject pluginTab = Control("PluginTab", WndObject);
        GameObject gameTab = Control("GameTab", WndObject);
        Control("PluginPrev").GetComponent<Button>().onClick.AddListener(OnPrevPagePlugin);
        Control("PluginNext").GetComponent<Button>().onClick.AddListener(OnNextPagePlugin);
        Control("GamePrev").GetComponent<Button>().onClick.AddListener(OnPrevPageGame);
        Control("GameNext").GetComponent<Button>().onClick.AddListener(OnNextPageGame);
        PluginRoot = Control("Content", pluginTab);
        GameRoot = Control("Content", gameTab);

        //模组分页内的功能设定
        Control("DeletePlugin").GetComponent<Button>().onClick.AddListener(() => { U3D.DeletePlugins();SettingWnd.Instance.Close();SettingWnd.Instance.Open();SettingWnd.Instance.ShowTab(4); });
        Toggle togShowInstallPlugin = Control("ShowInstallToggle").GetComponent<Toggle>();
        togShowInstallPlugin.onValueChanged.AddListener((bool value) => { this.showInstallPlugin = value; DlcMng.Instance.CollectAll(this.showInstallPlugin); this.PluginPageRefreshEx(); });
        togShowInstallPlugin.isOn = true;

        //透明度设定
        Control("AlphaSliderBar").GetComponent<Slider>().value = GameData.Instance.gameStatus.UIAlpha;
        Control("AlphaSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnUIAlphaChange);

        
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

        UITab [] tabs = WndObject.GetComponentsInChildren<UITab>();
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].onValueChanged.AddListener(OnTabShow);
        }
        
    }

    public void OnUIAlphaChange(float v)
    {
        GameData.Instance.gameStatus.UIAlpha = v;
    }

    public void ShowTab(int tab)
    {
        GameObject grid = Control("Tabs Grid");
        Transform tabCtrl = grid.transform.GetChild(tab);
        if (tabCtrl != null)
        {
            UITab t = tabCtrl.GetComponent<UITab>();
            t.isOn = true;
        }
    }

    void OnSetUIPosition()
    {
        UIAdjustWnd.Instance.Open();
    }

    void OnSkipVideo(bool skip)
    {
        GameData.Instance.gameStatus.SkipVideo = skip;
    }

    void OnDisableCat(bool disable)
    {
        GameData.Instance.gameStatus.PetOn = disable;
    }

    void OnDisableJoyStick(bool disable)
    {
        GameData.Instance.gameStatus.DisableJoystick = disable;
        if (NGUIJoystick.instance != null)
        {
            if (GameData.Instance.gameStatus.DisableJoystick)
                NGUIJoystick.instance.OnDisabled();
            else
                NGUIJoystick.instance.OnEnabled();
        }
    }

    void OnDisableParticle(bool disable)
    {
        GameData.Instance.gameStatus.DisableParticle = disable;
        if (disable)
        {
            if (Global.Instance.GScript != null)
            {
                Global.Instance.GScript.CleanSceneParticle();
            }

        }
    }

    void OnEnableLog(bool toggle)
    {
        GameData.Instance.gameStatus.EnableLog = toggle;
        if (toggle)
            WSDebug.Ins.OpenLogView();
        else
            WSDebug.Ins.CloseLogView();
    }

    void OnLevelDebug(bool toggle)
    {
        GameData.Instance.gameStatus.LevelDebug = toggle;
        if (toggle)
            Game.Instance.ShowDbg();
        else
            Game.Instance.CloseDbg();
    }

    void OnChangeVer(string ver)
    {
        if (AppInfo.Instance.MeteorVersion == ver)
        {
            U3D.PopupTip(string.Format("当前流星版本已为{0}", ver));
            return;
        }
        AppInfo.Instance.MeteorVersion = ver;
        GameData.Instance.gameStatus.MeteorVersion = AppInfo.Instance.MeteorVersion;
        GameData.Instance.SaveState();
        Close();
        if (GameOverlayWnd.Exist)
            GameOverlayWnd.Instance.ClearSystemMsg();
        U3D.ReStart();
    }

    //允许在战斗UI选择武器.
    void OnEnableWeaponChoose(bool on)
    {
        GameData.Instance.gameStatus.EnableWeaponChoose = on;
    }

    void OnDisableLock(bool on)
    {
        GameData.Instance.gameStatus.AutoLock = on;
        if (CameraFollow.Ins != null)
        {
            if (on)
                CameraFollow.Ins.EnableLock();
            else
                CameraFollow.Ins.DisableLock();
        }

        if (GameBattleEx.Instance != null)
        {
            if (on)
            {
                GameBattleEx.Instance.EnableLock();
            }
            else
            {
                GameBattleEx.Instance.Unlock();
                GameBattleEx.Instance.DisableLock();
            }
        }

        if (on)
        {
            if (FightWnd.Exist)
                FightWnd.Instance.ShowCameraBtn();
        }
        else
        {
            if (FightWnd.Exist)
                FightWnd.Instance.HideCameraBtn();
        }
    }

    void OnEnableUndead(bool on)
    {
        GameData.Instance.gameStatus.Undead = on;
    }

    void OnEnableGodMode(bool on)
    {
        GameData.Instance.gameStatus.EnableGodMode = on;
    }

    void OnEnableInfiniteAngry(bool on)
    {
        GameData.Instance.gameStatus.EnableInfiniteAngry = on;
    }

    void OnEnableDebugRobot(bool on)
    {
        GameData.Instance.gameStatus.EnableDebugRobot = on;
    }

    void OnEnableDebugSFX(bool on)
    {
        GameData.Instance.gameStatus.EnableDebugSFX = on;
    }

    void OnChangePerformance(bool on)
    {
        GameData.Instance.gameStatus.TargetFrame = on ? 60 : 30;
        Application.targetFrameRate = GameData.Instance.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif
    }

    void OnShowWayPoint(bool on)
    {
        GameBattleEx.Instance.ShowWayPoint(on);
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

    void OnTabShow(bool show)
    {
        if (Control("PluginTab", WndObject).activeInHierarchy && show)
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
            {
                PluginUpdate = Startup.ins.StartCoroutine(UpdatePluginInfo());//下载插件信息，显示插件
            }
        }
        else if (Control("GameTab", WndObject).activeInHierarchy && show)
        {
            GamePageUpdate = Startup.ins.StartCoroutine(GamePageRefresh());
            if (PluginUpdate != null)
            {
                Startup.ins.StopCoroutine(PluginUpdate);
                PluginUpdate = null;
            }
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
                pluginCount = 0;
                //显示出存档中保存得DLC信息
                DlcMng.Instance.ClearModel();
                for (int i = 0; i < GameData.Instance.gameStatus.pluginModel.Count; i++)
                {
                    DlcMng.Instance.Models.Add(GameData.Instance.gameStatus.pluginModel[i]);
                }
                //for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
                //{
                //InsertModel(DlcMng.Instance.Models[i]);
                //}
                pluginCount = DlcMng.Instance.Models.Count;
                DlcMng.Instance.ClearDlc();
                for (int i = 0; i < GameData.Instance.gameStatus.pluginChapter.Count; i++)
                {
                    DlcMng.Instance.Dlcs.Add(GameData.Instance.gameStatus.pluginChapter[i]);
                }
                pluginCount += DlcMng.Instance.Dlcs.Count;
                //for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
                //{
                //    InsertDlc(DlcMng.Instance.Dlcs[i]);
                //}
                pluginPage = 0;
                pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
                DlcMng.Instance.CollectAll(this.showInstallPlugin);
                PluginPageUpdate = Startup.ins.StartCoroutine(PluginPageRefresh());
                yield break;
            }

            Control("Warning").SetActive(false);
            pluginCount = 0;
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

            pluginCount += DlcMng.Instance.Models.Count;
            //for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
            //{
            //    InsertModel(DlcMng.Instance.Models[i]);
            //}

            //更新定义文件
            UnzipCallbackEx cb = new UnzipCallbackEx(0);
            try
            {
                WebClient webDef = new WebClient();
                webDef.DownloadFile(string.Format(Main.strSFile, Main.strHost, Main.strProjectUrl, "Def/Def.zip"), Application.persistentDataPath + "/Plugins/Def.zip");
                ZipUtility.UnzipFile(Application.persistentDataPath + "/Plugins/Def.zip", Application.persistentDataPath + "/Plugins/Def/", null, cb);
            }
            catch (Exception exp)
            {
                cb.OnFinished(false);
            }
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

            pluginCount += DlcMng.Instance.Dlcs.Count;
            //for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
            //{
            //    InsertDlc(DlcMng.Instance.Dlcs[i]);
            //}
            pluginPage = 0;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            DlcMng.Instance.CollectAll(this.showInstallPlugin);
            PluginPageUpdate = Startup.ins.StartCoroutine(PluginPageRefresh());
            Global.Instance.PluginUpdated = true;
            PluginUpdate = null;
        }
        else
        {
            if (PluginPageUpdate != null)
                Startup.ins.StopCoroutine(PluginPageUpdate);
            pluginCount = DlcMng.Instance.Models.Count + DlcMng.Instance.Dlcs.Count;
            pluginPage = 0;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            PluginPageUpdate = Startup.ins.StartCoroutine(PluginPageRefresh());
            //for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
            //{
            //    InsertModel(DlcMng.Instance.Models[i]);
            //}
            //for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
            //{
            //    InsertDlc(DlcMng.Instance.Dlcs[i]);
            //}
        }
    }

    void OnPrevPageGame()
    {
        if (gamePage != 0)
            gamePage--; 
        if (GamePageUpdate != null)
            Startup.ins.StopCoroutine(GamePageUpdate);
        GamePageUpdate = Startup.ins.StartCoroutine(GamePageRefresh());
    }

    void OnNextPageGame()
    {
        if (gamePage < gamePageMax - 1)
            gamePage++;
        if (GamePageUpdate != null)
            Startup.ins.StopCoroutine(GamePageUpdate);
        GamePageUpdate = Startup.ins.StartCoroutine(GamePageRefresh());
    }


    void OnPrevPagePlugin()
    {
        if (pluginPage != 0)
            pluginPage--;
        else
            return;
        PluginPageRefreshEx();
    }

    void OnNextPagePlugin()
    {
        if (pluginPage < pageMax - 1)
            pluginPage++;
        else
            return;
        PluginPageRefreshEx();
    }

    void PluginPageRefreshEx()
    {
        if (PluginPageUpdate != null)
            Startup.ins.StopCoroutine(PluginPageUpdate);
        PluginPageUpdate = Startup.ins.StartCoroutine(PluginPageRefresh());
    }

    IEnumerator PluginPageRefresh()
    {
        for (int i = 0; i < pluginList.Count; i++)
        {
            GameObject.Destroy(pluginList[i].gameObject);
            yield return 0;
        }
        pluginList.Clear();
        for (int i = pluginPage * pluginPerPage; i < Mathf.Min((pluginPage + 1) * pluginPerPage, DlcMng.Instance.allItem.Count); i++)
        {
            if (DlcMng.Instance.allItem[i] is ModelItem)
                InsertModel(DlcMng.Instance.allItem[i] as ModelItem);
            else
                InsertDlc(DlcMng.Instance.allItem[i] as Chapter);
            yield return 0;
        }
        PluginPageUpdate = null;
    }

    IEnumerator GamePageRefresh()
    {
        for (int i = 0; i < GameList.Count; i++)
        {
            GameObject.Destroy(GameList[i].gameObject);
            yield return 0;
        }
        GameList.Clear();
        for (int i = gamePage * pluginPerPage; i < Mathf.Min((gamePage + 1) * pluginPerPage, GameConfig.Instance.moreGame.Count); i++)
        {
            InsertGame(GameConfig.Instance.moreGame[i]);
            yield return 0;
        }
        GamePageUpdate = null;
        yield return 0;
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

        if (PluginPageUpdate != null)
        {
            Startup.ins.StopCoroutine(PluginPageUpdate);
            PluginPageUpdate = null;
        }

        if (GamePageUpdate != null)
        {
            Startup.ins.StopCoroutine(GamePageUpdate);
            GamePageUpdate = null;
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
    List<MoreGameCtrl> GameList = new List<MoreGameCtrl>();
    List<PluginCtrl> pluginList = new List<PluginCtrl>();
    GameObject prefabPluginWnd;
    GameObject prefabGameItem;
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
        pluginList.Add(ctrl);
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
        pluginList.Add(ctrl);
    }

    void InsertGame(AdGame game)
    {
        if (prefabGameItem == null)
            prefabGameItem = ResMng.Load("MoreGameItem") as GameObject;
        GameObject insert = GameObject.Instantiate(prefabGameItem);
        insert.transform.SetParent(GameRoot.transform);
        insert.transform.localPosition = Vector3.zero;
        insert.transform.localScale = Vector3.one;
        insert.transform.localRotation = Quaternion.identity;
        MoreGameCtrl ctrl = insert.GetComponent<MoreGameCtrl>();
        if (ctrl != null)
        {
            ctrl.Load(game);
        }
        GameList.Add(ctrl);
    }
}
