using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net;
using Idevgame.GameState.DialogState;
using Idevgame.GameState;

public class SettingDialogState:CommonDialogState<SettingDialog>
{
    public override string DialogName { get { return "SettingDialog"; } }
    public SettingDialogState(MainDialogStateManager stateMgr):base(stateMgr)
    {

    }
}

public class SettingDialog : Dialog {
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    Coroutine Update;
    Coroutine PluginPageUpdate;//插件翻页
    Coroutine GamePageUpdate;//游戏推荐页翻页
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
    void Init()
    {
        Control("Return").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Instance.GameStateMgr.SaveState();
            Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.MainMenuState);
        });

        Control("DeleteState").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Instance.GameStateMgr.ResetState();
            Init();
        });

        Control("ChangeLog").GetComponent<Text>().text = ResMng.LoadTextAsset("ChangeLog").text;
        Control("AppVerText").GetComponent<Text>().text = Main.Instance.AppInfo.AppVersion();
        Control("MeteorVerText").GetComponent<Text>().text = Main.Instance.AppInfo.MeteorVersion;

        
        Control("Nick").GetComponentInChildren<Text>().text = Main.Instance.CombatData.Logined ?  Main.Instance.GameStateMgr.gameStatus.NickName:"未登录";
        Control("Nick").GetComponent<Button>().onClick.AddListener(
            () =>
            {
                if (Main.Instance.CombatData.Logined)
                {
                    Main.Instance.EnterState(Main.Instance.NickNameDialogState);
                }
                else
                {
                    
                }
            }
        );
        Toggle highPerfor = Control("HighPerformance").GetComponent<Toggle>();
        highPerfor.isOn = Main.Instance.GameStateMgr.gameStatus.TargetFrame == 60;
        highPerfor.onValueChanged.AddListener(OnChangePerformance);
        Toggle High = Control("High").GetComponent<Toggle>();
        Toggle Medium = Control("Medium").GetComponent<Toggle>();
        Toggle Low = Control("Low").GetComponent<Toggle>();
        High.isOn = Main.Instance.GameStateMgr.gameStatus.Quality == 0;
        Medium.isOn = Main.Instance.GameStateMgr.gameStatus.Quality == 1;
        Low.isOn = Main.Instance.GameStateMgr.gameStatus.Quality == 2;
        High.onValueChanged.AddListener((bool selected) => { if (selected) Main.Instance.GameStateMgr.gameStatus.Quality = 0; });
        Medium.onValueChanged.AddListener((bool selected) => { if (selected) Main.Instance.GameStateMgr.gameStatus.Quality = 1; });
        Low.onValueChanged.AddListener((bool selected) => { if (selected) Main.Instance.GameStateMgr.gameStatus.Quality = 2; });
        Toggle ShowTargetBlood = Control("ShowTargetBlood").GetComponent<Toggle>();
        ShowTargetBlood.isOn = Main.Instance.GameStateMgr.gameStatus.ShowBlood;
        ShowTargetBlood.onValueChanged.AddListener((bool selected) => { Main.Instance.GameStateMgr.gameStatus.ShowBlood = selected; });
        Toggle ShowFPS = Control("ShowFPS").GetComponent<Toggle>();
        ShowFPS.isOn = Main.Instance.GameStateMgr.gameStatus.ShowFPS;
        ShowFPS.onValueChanged.AddListener((bool selected) => { Main.Instance.GameStateMgr.gameStatus.ShowFPS = selected; Main.Instance.ShowFps(selected); });

        Toggle ShowSysMenu2 = Control("ShowSysMenu2").GetComponent<Toggle>();
        ShowSysMenu2.isOn = Main.Instance.GameStateMgr.gameStatus.ShowSysMenu2;
        ShowSysMenu2.onValueChanged.AddListener((bool selected) => { Main.Instance.GameStateMgr.gameStatus.ShowSysMenu2 = selected; });

        if (Main.Instance != null)
        {
            Control("BGMSlider").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.MusicVolume;
            Control("EffectSlider").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.SoundVolume;
            Control("HSliderBar").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.AxisSensitivity.x;
            Control("VSliderBar").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.AxisSensitivity.y;
        }
        Control("BGMSlider").GetComponent<Slider>().onValueChanged.AddListener(OnMusicVolumeChange);
        Control("EffectSlider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectVolumeChange);
        Control("HSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnXSensitivityChange);
        Control("VSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnYSensitivityChange);
        Control("SetJoyPosition").GetComponent<Button>().onClick.AddListener(OnSetUIPosition);

        //显示战斗界面的调试按钮
        Toggle toggleDebug = Control("EnableSFX").GetComponent<Toggle>();
        toggleDebug.isOn = Main.Instance.GameStateMgr.gameStatus.EnableDebugSFX;
        toggleDebug.onValueChanged.AddListener(OnEnableDebugSFX);
        //显示战斗界面的调试按钮
        Toggle toggleRobot = Control("EnableRobot").GetComponent<Toggle>();
        toggleRobot.isOn = Main.Instance.GameStateMgr.gameStatus.EnableDebugRobot;
        toggleRobot.onValueChanged.AddListener(OnEnableDebugRobot);

        //显示武器挑选按钮
        Toggle toggleEnableFunc = Control("EnableWeaponChoose").GetComponent<Toggle>();
        toggleEnableFunc.isOn = Main.Instance.GameStateMgr.gameStatus.EnableWeaponChoose;
        toggleEnableFunc.onValueChanged.AddListener(OnEnableWeaponChoose);
        //无限气
        Toggle toggleEnableInfiniteAngry = Control("EnableInfiniteAngry").GetComponent<Toggle>();
        toggleEnableInfiniteAngry.isOn = Main.Instance.GameStateMgr.gameStatus.EnableInfiniteAngry;
        toggleEnableInfiniteAngry.onValueChanged.AddListener(OnEnableInfiniteAngry);

        //无锁定
        Toggle toggleDisableLock = Control("CameraLock").GetComponent<Toggle>();
        toggleDisableLock.isOn = Main.Instance.GameStateMgr.gameStatus.AutoLock;
        toggleDisableLock.onValueChanged.AddListener(OnDisableLock);

        Toggle toggleEnableGodMode = Control("EnableGodMode").GetComponent<Toggle>();
        toggleEnableGodMode.isOn = Main.Instance.GameStateMgr.gameStatus.EnableGodMode;
        toggleEnableGodMode.onValueChanged.AddListener(OnEnableGodMode);

        Toggle toggleEnableUndead = Control("EnableUnDead").GetComponent<Toggle>();
        toggleEnableUndead.isOn = Main.Instance.GameStateMgr.gameStatus.Undead;
        toggleEnableUndead.onValueChanged.AddListener(OnEnableUndead);

        Toggle toggleShowWayPoint = Control("ShowWayPoint").GetComponent<Toggle>();
        toggleShowWayPoint.isOn = Main.Instance.GameStateMgr.gameStatus.ShowWayPoint;
#if !STRIP_DBG_SETTING
        toggleShowWayPoint.onValueChanged.AddListener(OnShowWayPoint);
        if (Main.Instance.GameStateMgr.gameStatus.ShowWayPoint)
            OnShowWayPoint(true);
#else
        Destroy(toggleShowWayPoint.gameObject);
#endif
        Control("ChangeV107").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("1.07"); });
        Control("ChangeV907").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("9.07"); });
        Control("UnlockAll").GetComponent<Button>().onClick.AddListener(() => { U3D.UnlockLevel(); });

        //粒子特效
        Toggle toggleDisableParticle = Control("Particle").GetComponent<Toggle>();
        toggleDisableParticle.isOn = Main.Instance.GameStateMgr.gameStatus.DisableParticle;
        toggleDisableParticle.onValueChanged.AddListener(OnDisableParticle);
        OnDisableParticle(toggleDisableParticle.isOn);

        //关闭摇杆
        Toggle toggleDisableJoyStick = Control("Joystick").GetComponent<Toggle>();
        toggleDisableJoyStick.isOn = Main.Instance.GameStateMgr.gameStatus.DisableJoystick;
        toggleDisableJoyStick.onValueChanged.AddListener(OnDisableJoyStick);
        OnDisableJoyStick(toggleDisableJoyStick.isOn);

        Toggle toggleSkipVideo = Control("SkipVideo").GetComponent<Toggle>();
        toggleSkipVideo.isOn = Main.Instance.GameStateMgr.gameStatus.SkipVideo;
        toggleSkipVideo.onValueChanged.AddListener(OnSkipVideo);

        Toggle toggleOnlyWifi = Control("OnlyWifi").GetComponent<Toggle>();
        toggleOnlyWifi.isOn = Main.Instance.GameStateMgr.gameStatus.OnlyWifi;
        toggleOnlyWifi.onValueChanged.AddListener(OnOnlyWifi);

        GameObject pluginTab = Control("PluginTab", WndObject);
        GameObject debugTab = Control("DebugTab", WndObject);
        Control("PluginPrev").GetComponent<Button>().onClick.AddListener(OnPrevPagePlugin);
        Control("PluginNext").GetComponent<Button>().onClick.AddListener(OnNextPagePlugin);
        Control("AnimationDebug").GetComponent<Button>().onClick.AddListener(() => { OnBackPress(); UnityEngine.SceneManagement.SceneManager.LoadScene("DebugScene0"); });
        Control("SfxDebug").GetComponent<Button>().onClick.AddListener(() => { OnBackPress(); UnityEngine.SceneManagement.SceneManager.LoadScene("DebugScene1"); });
        PluginRoot = Control("Content", pluginTab);
        DebugRoot = Control("Content", debugTab);

        //模组分页内的功能设定
        Control("DeletePlugin").GetComponent<Button>().onClick.AddListener(() => { U3D.DeletePlugins(); SettingDialogState.Instance.ShowTab(4);});
        Toggle togShowInstallPlugin = Control("ShowInstallToggle").GetComponent<Toggle>();
        togShowInstallPlugin.onValueChanged.AddListener((bool value) => { this.showInstallPlugin = value; Main.Instance.DlcMng.CollectAll(this.showInstallPlugin); this.PluginPageRefreshEx(); });
        togShowInstallPlugin.isOn = true;

        //透明度设定
        Control("AlphaSliderBar").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.UIAlpha;
        Control("AlphaSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnUIAlphaChange);
        Control("InstallAll").GetComponent<Button>().onClick.AddListener(OnInstallAll);

        if (Main.Instance.AppInfo.AppVersionIsSmallThan(Main.Instance.GameNotice.newVersion))
        {
            //需要更新，设置好服务器版本号，设置好下载链接
            Control("NewVersionSep", WndObject).SetActive(true);
            Control("NewVersion", WndObject).GetComponent<Text>().text = string.Format("最新版本号:{0}", Main.Instance.GameNotice.newVersion);
            Control("NewVersion", WndObject).SetActive(true);
            Control("GetNewVersion", WndObject).GetComponent<LinkLabel>().URL = Main.Instance.GameNotice.apkUrl;
            Control("GetNewVersion", WndObject).SetActive(true);
            Control("Flag", WndObject).SetActive(true);
        }

        UITab[] tabs = WndObject.GetComponentsInChildren<UITab>();
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].onValueChanged.AddListener(OnTabShow);
        }

    }

    void OnInstallAll()
    {
        for (int i = 0; i < pluginList.Count; i++)
        {
            Main.Instance.DlcMng.AddDownloadTask(pluginList[i]);
        }
    }

    public void OnUIAlphaChange(float v)
    {
        Main.Instance.GameStateMgr.gameStatus.UIAlpha = v;
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
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.UIAdjustDialogState);
    }

    void OnOnlyWifi(bool only)
    {
        Main.Instance.GameStateMgr.gameStatus.OnlyWifi = only;
    }

    void OnSkipVideo(bool skip)
    {
        Main.Instance.GameStateMgr.gameStatus.SkipVideo = skip;
    }

    void OnDisableJoyStick(bool disable)
    {
        Main.Instance.GameStateMgr.gameStatus.DisableJoystick = disable;
        if (NGUIJoystick.instance != null)
        {
            if (Main.Instance.GameStateMgr.gameStatus.DisableJoystick)
                NGUIJoystick.instance.OnDisabled();
            else
                NGUIJoystick.instance.OnEnabled();
        }
    }

    void OnDisableParticle(bool disable)
    {
        Main.Instance.GameStateMgr.gameStatus.DisableParticle = disable;
        if (disable)
        {
            if (Main.Instance.CombatData.GScript != null)
            {
                Main.Instance.CombatData.GScript.CleanSceneParticle();
            }

        }
    }

    void OnChangeVer(string ver)
    {
        if (Main.Instance.AppInfo.MeteorVersion == ver)
        {
            U3D.PopupTip(string.Format("当前流星版本已为{0}", ver));
            return;
        }
        Main.Instance.AppInfo.MeteorVersion = ver;
        Main.Instance.GameStateMgr.gameStatus.MeteorVersion = Main.Instance.AppInfo.MeteorVersion;
        Main.Instance.GameStateMgr.SaveState();
        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        OnBackPress();
        U3D.ReStart();
    }

    //允许在战斗UI选择武器.
    void OnEnableWeaponChoose(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableWeaponChoose = on;
    }

    void OnDisableLock(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.AutoLock = on;
        if (Main.Instance.CameraFollow != null)
        {
            if (on)
                Main.Instance.CameraFollow.EnableLock();
            else
                Main.Instance.CameraFollow.DisableLock();
        }

        if (Main.Instance.GameBattleEx != null)
        {
            if (on)
            {
                Main.Instance.GameBattleEx.EnableLock();
            }
            else
            {
                Main.Instance.GameBattleEx.Unlock();
                Main.Instance.GameBattleEx.DisableLock();
            }
        }

        if (on)
        {
            if (FightDialogState.Exist())
                FightDialogState.Instance.ShowCameraBtn();
        }
        else
        {
            if (FightDialogState.Exist())
                FightDialogState.Instance.HideCameraBtn();
        }
    }

    void OnEnableUndead(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.Undead = on;
    }

    void OnEnableGodMode(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableGodMode = on;
    }

    void OnEnableInfiniteAngry(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableInfiniteAngry = on;
    }

    void OnEnableDebugRobot(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableDebugRobot = on;
    }

    void OnEnableDebugSFX(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableDebugSFX = on;
    }

    void OnChangePerformance(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.TargetFrame = on ? 60 : 30;
        Application.targetFrameRate = Main.Instance.GameStateMgr.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 120;
#endif
    }

#if !STRIP_DBG_SETTING
    void OnShowWayPoint(bool on)
    {
        if (Main.Instance.GameBattleEx != null)
            Main.Instance.GameBattleEx.ShowWayPoint(on);
    }
#endif

    void OnMusicVolumeChange(float vo)
    {
        Main.Instance.SoundManager.SetMusicVolume(vo);
        if (Main.Instance != null)
            Main.Instance.GameStateMgr.gameStatus.MusicVolume = vo;
    }

    void OnXSensitivityChange(float v)
    {
        Main.Instance.GameStateMgr.gameStatus.AxisSensitivity.x = v;
    }

    void OnYSensitivityChange(float v)
    {
        Main.Instance.GameStateMgr.gameStatus.AxisSensitivity.y = v;
    }

    void OnEffectVolumeChange(float vo)
    {
        Main.Instance.SoundManager.SetSoundVolume(vo);
        Main.Instance.GameStateMgr.gameStatus.SoundVolume = vo;
    }

    void OnTabShow(bool show)
    {
        if (Control("PluginTab", WndObject).activeInHierarchy && show)
        {
            PluginUpdate = Main.Instance.StartCoroutine(UpdatePluginInfo());//下载插件信息，显示插件
        }
    }

    IEnumerator UpdatePluginInfo()
    {
        if (!Main.Instance.CombatData.PluginUpdated)
        {
            UnityWebRequest vFile = new UnityWebRequest();
            vFile.url = string.Format(Main.strFile, Main.strHost, Main.port, Main.strProjectUrl, Main.strPlugins);
            vFile.timeout = 5;
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
                Main.Instance.DlcMng.ClearModel();
                for (int i = 0; i < Main.Instance.GameStateMgr.gameStatus.pluginModel.Count; i++)
                {
                    Main.Instance.DlcMng.Models.Add(Main.Instance.GameStateMgr.gameStatus.pluginModel[i]);
                }
                //for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
                //{
                //InsertModel(DlcMng.Instance.Models[i]);
                //}
                pluginCount = Main.Instance.DlcMng.Models.Count;
                Main.Instance.DlcMng.ClearDlc();
                for (int i = 0; i < Main.Instance.GameStateMgr.gameStatus.pluginChapter.Count; i++)
                {
                    Main.Instance.DlcMng.Dlcs.Add(Main.Instance.GameStateMgr.gameStatus.pluginChapter[i]);
                }
                pluginCount += Main.Instance.DlcMng.Dlcs.Count;
                //for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
                //{
                //    InsertDlc(DlcMng.Instance.Dlcs[i]);
                //}
                pluginPage = 0;
                pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
                Main.Instance.DlcMng.CollectAll(this.showInstallPlugin);
                Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
                PluginPageUpdate = Main.Instance.StartCoroutine(PluginPageRefresh());
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

            Main.Instance.DlcMng.ClearModel();
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
                Info.useFemalePos = js["Model"][i]["gender"] != null;
                Main.Instance.DlcMng.AddModel(Info);
            }

            pluginCount += Main.Instance.DlcMng.Models.Count;
            //for (int i = 0; i < DlcMng.Instance.Models.Count; i++)
            //{
            //    InsertModel(DlcMng.Instance.Models[i]);
            //}
            Main.Instance.DlcMng.ClearDlc();
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
                Main.Instance.DlcMng.AddDlc(c);
            }

            pluginCount += Main.Instance.DlcMng.Dlcs.Count;
            //for (int i = 0; i < DlcMng.Instance.Dlcs.Count; i++)
            //{
            //    InsertDlc(DlcMng.Instance.Dlcs[i]);
            //}
            pluginPage = 0;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            Main.Instance.DlcMng.CollectAll(this.showInstallPlugin);
            PluginPageUpdate = Main.Instance.StartCoroutine(PluginPageRefresh());
            Main.Instance.CombatData.PluginUpdated = true;
            PluginUpdate = null;
        }
        else
        {
            if (PluginPageUpdate != null)
                yield break;
            pluginCount = Main.Instance.DlcMng.Models.Count + Main.Instance.DlcMng.Dlcs.Count;
            pluginPage = 0;
            pageMax = pluginCount / pluginPerPage + ((pluginCount % pluginPerPage == 0) ? 0 : 1);
            PluginPageUpdate = Main.Instance.StartCoroutine(PluginPageRefresh());
        }
        Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
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
        Control("Pages").GetComponent<Text>().text = (pluginPage + 1) + "/" + pageMax;
        if (PluginPageUpdate != null)
            Main.Instance.StopCoroutine(PluginPageUpdate);
        PluginPageUpdate = Main.Instance.StartCoroutine(PluginPageRefresh());
    }

    IEnumerator PluginPageRefresh()
    {
        for (int i = 0; i < pluginList.Count; i++)
        {
            GameObject.Destroy(pluginList[i].gameObject);
        }
        pluginList.Clear();
        for (int i = pluginPage * pluginPerPage; i < Mathf.Min((pluginPage + 1) * pluginPerPage, Main.Instance.DlcMng.allItem.Count); i++)
        {
            if (Main.Instance.DlcMng.allItem[i] is ModelItem)
                InsertModel(Main.Instance.DlcMng.allItem[i] as ModelItem);
            else
                InsertDlc(Main.Instance.DlcMng.allItem[i] as Chapter);
            yield return 0;
        }
        PluginPageUpdate = null;
    }

    public override void OnRefresh(int message, object param)
    {
        if (message == 0)
        {
            Control("Nick").GetComponentInChildren<Text>().text = Main.Instance.GameStateMgr.gameStatus.NickName;
        }
    }

    public override void OnClose()
    {
        if (Update != null)
        {
            Main.Instance.StopCoroutine(Update);
            Update = null;
        }
        if (PluginUpdate != null)
        {
            Main.Instance.StopCoroutine(PluginUpdate);
            PluginUpdate = null;
        }

        if (PluginPageUpdate != null)
        {
            Main.Instance.StopCoroutine(PluginPageUpdate);
            PluginPageUpdate = null;
        }

        if (GamePageUpdate != null)
        {
            Main.Instance.StopCoroutine(GamePageUpdate);
            GamePageUpdate = null;
        }

        for (int i = 0; i < Install.Count; i++)
        {
            GameObject.Destroy(Install[i]);
        }

        Install.Clear();
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
            if (!Main.Instance.GameStateMgr.gameStatus.IsModelInstalled(item))
                Main.Instance.DlcMng.AddPreviewTask(ctrl);
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
            if (!Main.Instance.GameStateMgr.gameStatus.IsDlcInstalled(item))
                Main.Instance.DlcMng.AddPreviewTask(ctrl);
        }
        pluginList.Add(ctrl);
    }
}
