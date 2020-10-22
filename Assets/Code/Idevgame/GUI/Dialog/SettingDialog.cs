using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Net;
using Idevgame.GameState.DialogState;
using Idevgame.GameState;
using DG.Tweening;
using Excel2Json;

public class SettingDialogState:CommonDialogState<SettingDialog>
{
    public override string DialogName { get { return "SettingDialog"; } }
    public SettingDialogState(MainDialogMgr stateMgr):base(stateMgr)
    {
    }
}

public class SettingDialog : Dialog {
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    GameObject DebugRoot;
    Toggle lowPerfor;
    Toggle highPerfor;
    Toggle superHighPerfor;
    Toggle LowMiddle;
    void Init()
    {
        Control("Return").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameStateMgr.Ins.SaveState();
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.MainMenuState);
        });

        Control("DeleteState").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameStateMgr.Ins.ResetState();
            Init();
        });

        Control("ChangeLog").GetComponent<Text>().text = Resources.Load<TextAsset>("ChangeLog").text;
        Control("AuthorText").GetComponent<Text>().text = Resources.Load<TextAsset>("Author").text;
        Text cheat = Control("CheatCodeList").GetComponent<Text>();
        cheat.text = Resources.Load<TextAsset>("CheatCodeList").text;
        cheat.fontSize = 33;
        Control("AppVerText").GetComponent<Text>().text = Main.Ins.AppInfo.AppVersion();
        Control("MeteorVerText").GetComponent<Text>().text = Main.Ins.AppInfo.MeteorVersion;
        Control("DoScript").GetComponent<Button>().onClick.AddListener(()=> { U3D.DoScript(); });
        Control("Nick").GetComponentInChildren<Text>().text = GameStateMgr.Ins.gameStatus.NickName;
        Control("Nick").GetComponent<Button>().onClick.AddListener(
            () =>
            {
                NickNameDialogState.State.Open();
            }
        );

        lowPerfor = Control("LowPerformance").GetComponent<Toggle>();
        lowPerfor.isOn = GameStateMgr.Ins.gameStatus.TargetFrame == 30;
        lowPerfor.onValueChanged.AddListener(OnChangePerformance);

        LowMiddle = Control("LowMiddle").GetComponent<Toggle>();
        LowMiddle.isOn = GameStateMgr.Ins.gameStatus.TargetFrame == 60;
        LowMiddle.onValueChanged.AddListener(OnChangePerformance);

        highPerfor = Control("HighPerformance").GetComponent<Toggle>();
        highPerfor.isOn = GameStateMgr.Ins.gameStatus.TargetFrame == 90;
        highPerfor.onValueChanged.AddListener(OnChangePerformance);

        superHighPerfor = Control("SuperHighPerformance").GetComponent<Toggle>();
        superHighPerfor.isOn = GameStateMgr.Ins.gameStatus.TargetFrame == 120;
        superHighPerfor.onValueChanged.AddListener(OnChangePerformance);

        Toggle High = Control("High").GetComponent<Toggle>();
        Toggle Medium = Control("Medium").GetComponent<Toggle>();
        Toggle Low = Control("Low").GetComponent<Toggle>();
        High.isOn = GameStateMgr.Ins.gameStatus.Quality == 0;
        Medium.isOn = GameStateMgr.Ins.gameStatus.Quality == 1;
        Low.isOn = GameStateMgr.Ins.gameStatus.Quality == 2;
        High.onValueChanged.AddListener((bool selected) => { if (selected) GameStateMgr.Ins.gameStatus.Quality = 0; });
        Medium.onValueChanged.AddListener((bool selected) => { if (selected) GameStateMgr.Ins.gameStatus.Quality = 1; });
        Low.onValueChanged.AddListener((bool selected) => { if (selected) GameStateMgr.Ins.gameStatus.Quality = 2; });
        Toggle ShowTargetBlood = Control("ShowTargetBlood").GetComponent<Toggle>();
        ShowTargetBlood.isOn = GameStateMgr.Ins.gameStatus.ShowBlood;
        ShowTargetBlood.onValueChanged.AddListener((bool selected) => { GameStateMgr.Ins.gameStatus.ShowBlood = selected; });
        Toggle ShowFPS = Control("ShowFPS").GetComponent<Toggle>();
        ShowFPS.isOn = GameStateMgr.Ins.gameStatus.ShowFPS;
        ShowFPS.onValueChanged.AddListener((bool selected) => { GameStateMgr.Ins.gameStatus.ShowFPS = selected; Main.Ins.ShowFps(selected); });

        if (Main.Ins != null)
        {
            Control("BGMSlider").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.MusicVolume;
            Control("EffectSlider").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.SoundVolume;
        }
        Control("BGMSlider").GetComponent<Slider>().onValueChanged.AddListener(OnMusicVolumeChange);
        Control("EffectSlider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectVolumeChange);
        Control("SetJoyPosition").GetComponent<Button>().onClick.AddListener(OnSetUIPosition);

        Toggle EnableGamePad = Control("EnableGamePad").GetComponent<Toggle>();
        EnableGamePad.isOn = GameStateMgr.Ins.gameStatus.UseGamePad;
        EnableGamePad.onValueChanged.AddListener((bool selected) => { GameStateMgr.Ins.gameStatus.UseGamePad = selected; Main.Ins.JoyStick.enabled = selected; });

        Toggle EnableMouse = Control("EnableMouse").GetComponent<Toggle>();
        EnableMouse.isOn = GameStateMgr.Ins.gameStatus.UseMouse;
        EnableMouse.onValueChanged.AddListener((bool selected) => { GameStateMgr.Ins.gameStatus.UseMouse = selected;});
        
        //显示战斗界面的调试按钮
        Toggle toggleDebug = Control("EnableSFX").GetComponent<Toggle>();
        toggleDebug.isOn = GameStateMgr.Ins.gameStatus.EnableDebugSFX;
        toggleDebug.onValueChanged.AddListener(OnEnableDebugSFX);
        //显示战斗界面的调试按钮
        Toggle toggleRobot = Control("EnableRobot").GetComponent<Toggle>();
        toggleRobot.isOn = GameStateMgr.Ins.gameStatus.EnableDebugRobot;
        toggleRobot.onValueChanged.AddListener(OnEnableDebugRobot);

        //显示武器挑选按钮
        Toggle toggleEnableFunc = Control("EnableWeaponChoose").GetComponent<Toggle>();
        toggleEnableFunc.isOn = GameStateMgr.Ins.gameStatus.EnableWeaponChoose;
        toggleEnableFunc.onValueChanged.AddListener(OnEnableWeaponChoose);
        //无限气
        Toggle toggleEnableInfiniteAngry = Control("EnableInfiniteAngry").GetComponent<Toggle>();
        toggleEnableInfiniteAngry.isOn = GameStateMgr.Ins.gameStatus.EnableInfiniteAngry;
        toggleEnableInfiniteAngry.onValueChanged.AddListener(OnEnableInfiniteAngry);

        //无锁定
        Toggle toggleDisableLock = Control("CameraLock").GetComponent<Toggle>();
        toggleDisableLock.isOn = GameStateMgr.Ins.gameStatus.AutoLock;
        toggleDisableLock.onValueChanged.AddListener(OnDisableLock);

        Toggle toggleEnableGodMode = Control("EnableGodMode").GetComponent<Toggle>();
        toggleEnableGodMode.isOn = GameStateMgr.Ins.gameStatus.EnableGodMode;
        toggleEnableGodMode.onValueChanged.AddListener(OnEnableGodMode);

        Toggle toggleHidePlayer = Control("HidePlayer").GetComponent<Toggle>();
        toggleHidePlayer.isOn = GameStateMgr.Ins.gameStatus.HidePlayer;
        toggleHidePlayer.onValueChanged.AddListener(OnHidePlayer);
        
        Toggle toggleEnableUndead = Control("EnableUnDead").GetComponent<Toggle>();
        toggleEnableUndead.isOn = GameStateMgr.Ins.gameStatus.Undead;
        toggleEnableUndead.onValueChanged.AddListener(OnEnableUndead);

        Toggle toggleShowWayPoint = Control("ShowWayPoint").GetComponent<Toggle>();
        toggleShowWayPoint.isOn = GameStateMgr.Ins.gameStatus.ShowWayPoint;
        toggleShowWayPoint.onValueChanged.AddListener(OnShowWayPoint);
        Control("ChangeV107").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("1.07"); });
        Control("ChangeV907").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("9.07"); });
        Control("UnlockAll").GetComponent<Button>().onClick.AddListener(() => { U3D.UnlockLevel(); });

        //粒子特效
        Toggle toggleDisableParticle = Control("Particle").GetComponent<Toggle>();
        toggleDisableParticle.isOn = GameStateMgr.Ins.gameStatus.DisableParticle;
        toggleDisableParticle.onValueChanged.AddListener(OnDisableParticle);
        OnDisableParticle(toggleDisableParticle.isOn);

        Toggle toggleJoyEnable = Control("EnableJoy").GetComponent<Toggle>();
        toggleJoyEnable.isOn = GameStateMgr.Ins.gameStatus.JoyEnable;
        toggleJoyEnable.onValueChanged.AddListener(OnJoyEnable);

        Toggle toggleJoyOnlyRotate = Control("JoyOnlyRotate").GetComponent<Toggle>();
        toggleJoyOnlyRotate.isOn = GameStateMgr.Ins.gameStatus.JoyRotateOnly;
        toggleJoyOnlyRotate.onValueChanged.AddListener(OnJoyRotateOnly);


        GameObject debugTab = Control("DebugTab", WndObject);
        Toggle debugToggle = Control("Debug", WndObject).GetComponent<Toggle>();
        Toggle cheatToggle = Control("Cheat", WndObject).GetComponent<Toggle>();

        DebugRoot = Control("Content", debugTab);

        if (Main.Ins.AppInfo.AppVersionIsSmallThan(Main.Ins.GameNotice.newVersion))
        {
            //需要更新，设置好服务器版本号，设置好下载链接
            Control("NewVersionSep", WndObject).SetActive(true);
            Control("NewVersion", WndObject).GetComponent<Text>().text = string.Format("最新版本号:{0}", Main.Ins.GameNotice.newVersion);
            Control("NewVersion", WndObject).SetActive(true);
            Control("GetNewVersion", WndObject).GetComponent<LinkLabel>().URL = Main.Ins.GameNotice.apkUrl;
            Control("GetNewVersion", WndObject).SetActive(true);
            Control("Flag", WndObject).SetActive(true);
        }

        UITab[] tabs = WndObject.GetComponentsInChildren<UITab>();
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].onValueChanged.AddListener(OnTabShow);
        }

        //把一些模式禁用，例如作弊之类的.
        if (GameStateMgr.Ins.gameStatus.CheatEnable) {
            debugToggle.gameObject.SetActive(true);
            cheatToggle.gameObject.SetActive(true);
        } else {
            Control("EnableRobot").SetActive(false);//屏蔽可添加电脑
            Control("EnableWeaponChoose").SetActive(false);
            Control("ShowWayPoint").SetActive(false);
            Control("EnableUnDead").SetActive(false);
            Control("EnableGodMode").SetActive(false);
            Control("EnableInfiniteAngry").SetActive(false);
            Control("EnableSFX").SetActive(false);
        }

        LoadDebugLevel();
        //起始页显示
        OnTabShow(true);

        Button JoyW = Control("JoyW").GetComponent<Button>();
        JoyW.onClick.AddListener(()=> { FlashButton(JoyW, EKeyList.KL_KeyW, "上:[{0}]"); });

        Button JoyS = Control("JoyS").GetComponent<Button>();
        JoyS.onClick.AddListener(() => { FlashButton(JoyS, EKeyList.KL_KeyS, "下:[{0}]"); });

        Button JoyA = Control("JoyA").GetComponent<Button>();
        JoyA.onClick.AddListener(() => { FlashButton(JoyA, EKeyList.KL_KeyA, "左:[{0}]"); });

        Button JoyD = Control("JoyD").GetComponent<Button>();
        JoyD.onClick.AddListener(() => { FlashButton(JoyD, EKeyList.KL_KeyD, "右:[{0}]"); });

        Button JoyCW = Control("JoyCW").GetComponent<Button>();
        JoyCW.onClick.AddListener(() => { FlashButton(JoyCW, EKeyList.KL_CameraAxisYU, "视角上:[{0}]"); });

        Button JoyCS = Control("JoyCS").GetComponent<Button>();
        JoyCS.onClick.AddListener(() => { FlashButton(JoyCS, EKeyList.KL_CameraAxisYD, "视角下:[{0}]"); });

        Button JoyCA = Control("JoyCA").GetComponent<Button>();
        JoyCA.onClick.AddListener(() => { FlashButton(JoyCA, EKeyList.KL_CameraAxisXL, "视角左:[{0}]"); });

        Button JoyCD = Control("JoyCD").GetComponent<Button>();
        JoyCD.onClick.AddListener(() => { FlashButton(JoyCD, EKeyList.KL_CameraAxisXR, "视角右:[{0}]"); });

        Button JoyAttack = Control("JoyAttack").GetComponent<Button>();
        JoyAttack.onClick.AddListener(() => { FlashButton(JoyAttack, EKeyList.KL_Attack, "攻击:[{0}]"); });

        Button JoyDefence = Control("JoyDefence").GetComponent<Button>();
        JoyDefence.onClick.AddListener(() => { FlashButton(JoyDefence, EKeyList.KL_Defence, "防守:[{0}]"); });

        Button JoyJump = Control("JoyJump").GetComponent<Button>();
        JoyJump.onClick.AddListener(() => { FlashButton(JoyJump, EKeyList.KL_Jump, "跳跃:[{0}]"); });

        Button JoyBurst = Control("JoyBurst").GetComponent<Button>();
        JoyBurst.onClick.AddListener(() => { FlashButton(JoyBurst, EKeyList.KL_BreakOut, "爆气:[{0}]"); });

        Button JoyChangeWeapon = Control("JoyChangeWeapon").GetComponent<Button>();
        JoyChangeWeapon.onClick.AddListener(() => { FlashButton(JoyChangeWeapon, EKeyList.KL_ChangeWeapon, "切换武器:[{0}]"); });

        Button JoyDrop = Control("JoyDrop").GetComponent<Button>();
        JoyDrop.onClick.AddListener(() => { FlashButton(JoyDrop, EKeyList.KL_DropWeapon, "丢弃武器:[{0}]"); });

        Button JoyCrouch = Control("JoyCrouch").GetComponent<Button>();
        JoyCrouch.onClick.AddListener(() => { FlashButton(JoyCrouch, EKeyList.KL_Crouch, "蹲下:[{0}]"); });

        Button JoyUnlock = Control("JoyUnlock").GetComponent<Button>();
        JoyUnlock.onClick.AddListener(() => { FlashButton(JoyUnlock, EKeyList.KL_KeyQ, "锁定:[{0}]"); });

        Button JoyHelp = Control("JoyHelp").GetComponent<Button>();
        JoyHelp.onClick.AddListener(() => { FlashButton(JoyHelp, EKeyList.KL_Help, "救助:[{0}]"); });
    }

    //按照最新的设置，读取显示出来.
    void Refresh() {
        Button JoyW = Control("JoyW").GetComponent<Button>();
        JoyW.GetComponentInChildren<Text>().text = string.Format("上:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_KeyW].key.ToString());
        Button JoyS = Control("JoyS").GetComponent<Button>();
        JoyS.GetComponentInChildren<Text>().text = string.Format("下:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_KeyS].key.ToString());
        Button JoyA = Control("JoyA").GetComponent<Button>();
        JoyA.GetComponentInChildren<Text>().text = string.Format("左:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_KeyA].key.ToString());
        Button JoyD = Control("JoyD").GetComponent<Button>();
        JoyD.GetComponentInChildren<Text>().text = string.Format("右:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_KeyD].key.ToString());

        Button JoyCW = Control("JoyCW").GetComponent<Button>();
        JoyCW.GetComponentInChildren<Text>().text = string.Format("视角上:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_CameraAxisYU].key.ToString());

        Button JoyCS = Control("JoyCS").GetComponent<Button>();
        JoyCS.GetComponentInChildren<Text>().text = string.Format("视角下:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_CameraAxisYD].key.ToString());

        Button JoyCA = Control("JoyCA").GetComponent<Button>();
        JoyCA.GetComponentInChildren<Text>().text = string.Format("视角左:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_CameraAxisXL].key.ToString());

        Button JoyCD = Control("JoyCD").GetComponent<Button>();
        JoyCD.GetComponentInChildren<Text>().text = string.Format("视角右:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_CameraAxisXR].key.ToString());

        Button JoyAttack = Control("JoyAttack").GetComponent<Button>();
        JoyAttack.GetComponentInChildren<Text>().text = string.Format("攻击:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_Attack].key.ToString());

        Button JoyDefence = Control("JoyDefence").GetComponent<Button>();
        JoyDefence.GetComponentInChildren<Text>().text = string.Format("防守:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_Defence].key.ToString());

        Button JoyJump = Control("JoyJump").GetComponent<Button>();
        JoyJump.GetComponentInChildren<Text>().text = string.Format("跳跃:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_Jump].key.ToString());

        Button JoyBurst = Control("JoyBurst").GetComponent<Button>();
        JoyBurst.GetComponentInChildren<Text>().text = string.Format("爆气:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_BreakOut].key.ToString());

        Button JoyChangeWeapon = Control("JoyChangeWeapon").GetComponent<Button>();
        JoyChangeWeapon.GetComponentInChildren<Text>().text = string.Format("切换武器:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_ChangeWeapon].key.ToString());

        Button JoyDrop = Control("JoyDrop").GetComponent<Button>();
        JoyDrop.GetComponentInChildren<Text>().text = string.Format("丢弃武器:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_DropWeapon].key.ToString());

        Button JoyCrouch = Control("JoyCrouch").GetComponent<Button>();
        JoyCrouch.GetComponentInChildren<Text>().text = string.Format("蹲下:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_Crouch].key.ToString());

        Button JoyUnlock = Control("JoyUnlock").GetComponent<Button>();
        JoyUnlock.GetComponentInChildren<Text>().text = string.Format("锁定:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_KeyQ].key.ToString());

        Button JoyHelp = Control("JoyHelp").GetComponent<Button>();
        JoyHelp.GetComponentInChildren<Text>().text = string.Format("救助:[{0}]", Main.Ins.JoyStick.keyMapping[EKeyList.KL_Help].key.ToString());
    }

    bool flashing = false;
    EKeyList flashKey = EKeyList.KL_None;
    Color initializeColor;
    Tweener colorFade;
    Button flashButton;
    string formatString = "";
    private void Update() {
        if (flashing) {
            string currentButton = "无";
            KeyCode k = KeyCode.None;
            var values = Enum.GetValues(typeof(KeyCode));//存储所有的按键
            for (int x = 0; x < values.Length; x++) {
                //KeyCode j = (KeyCode)values.GetValue(x);
                if (Input.GetKeyDown((KeyCode)values.GetValue(x))) {
                    currentButton = values.GetValue(x).ToString();//遍历并获取当前按下的按键
                    k = (KeyCode)values.GetValue(x);
                    break;
                }
            }
            if (k != KeyCode.None) {
                flashButton.GetComponentInChildren<Text>().text = string.Format(formatString, currentButton);
                KillFade();
                flashing = false;
                flashButton = null;
                formatString = "";
                Main.Ins.JoyStick.Register(flashKey, k);
                Refresh();
            } else {

            }
        }
    }

    void KillFade() {
        if (colorFade != null) {
            colorFade.Pause();
            colorFade.Kill();
            colorFade = null;
            if (flashButton != null) {
                flashButton.GetComponent<Image>().color = initializeColor;
            }
        }
    }

    //闪亮显示一个要设置的按键.当按下任意一个按键时，与对应的动作绑定.
    public void FlashButton(Button btn, EKeyList vkey, string format) {
        if (flashing) {
            KillFade();
        }
        flashButton = btn;
        formatString = format;
        flashKey = vkey;
        flashing = true;

        initializeColor = flashButton.GetComponent<Image>().color;
        MyPingPong(flashButton.GetComponent<Image>(), initializeColor, Color.white, 0.5f);
    }

    void MyPingPong(Image img, Color from, Color to, float duration) {
        colorFade = img.DOColor(to, duration);
        colorFade.OnComplete(() => MyPingPong(img, to, from, duration));
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
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.UIAdjustDialogState);
    }

    void OnJoyEnable(bool enable) {
        GameStateMgr.Ins.gameStatus.JoyEnable = enable;
    }

    void OnJoyRotateOnly(bool only) {
        GameStateMgr.Ins.gameStatus.JoyRotateOnly = only;
    }

    void OnDisableParticle(bool disable)
    {
        GameStateMgr.Ins.gameStatus.DisableParticle = disable;
        if (disable)
        {
            if (CombatData.Ins.GScript != null)
            {
                CombatData.Ins.GScript.CleanSceneParticle();
            }
        }
    }

    void OnChangeVer(string ver)
    {
        if (Main.Ins.AppInfo.MeteorVersion == ver)
        {
            U3D.PopupTip(string.Format("当前流星版本已为{0}", ver));
            return;
        }
        Main.Ins.AppInfo.MeteorVersion = ver;
        GameStateMgr.Ins.gameStatus.MeteorVersion = Main.Ins.AppInfo.MeteorVersion;
        GameStateMgr.Ins.SaveState();
        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        OnBackPress();
        U3D.ReStart();
    }

    //允许在战斗UI选择武器.
    void OnEnableWeaponChoose(bool on)
    {
        GameStateMgr.Ins.gameStatus.EnableWeaponChoose = on;
    }

    void OnDisableLock(bool on)
    {
        GameStateMgr.Ins.gameStatus.AutoLock = on;
        if (Main.Ins.CameraFollow != null)
        {
            if (on)
                Main.Ins.CameraFollow.EnableLock();
            else
                Main.Ins.CameraFollow.DisableLock();
        }

        if (Main.Ins.GameBattleEx != null)
        {
            if (on)
            {
                Main.Ins.GameBattleEx.EnableLock();
            }
            else
            {
                Main.Ins.GameBattleEx.Unlock();
                Main.Ins.GameBattleEx.DisableLock();
            }
        }

        if (on)
        {
            if (FightState.Exist())
                FightState.Instance.ShowCameraBtn();
        }
        else
        {
            if (FightState.Exist())
                FightState.Instance.HideCameraBtn();
        }
    }

    void OnEnableUndead(bool on)
    {
        GameStateMgr.Ins.gameStatus.Undead = on;
    }

    void OnEnableGodMode(bool on)
    {
        GameStateMgr.Ins.gameStatus.EnableGodMode = on;
    }

    void OnHidePlayer(bool hide) {
        GameStateMgr.Ins.gameStatus.HidePlayer = hide;
    }

    void OnEnableInfiniteAngry(bool on)
    {
        GameStateMgr.Ins.gameStatus.EnableInfiniteAngry = on;
    }

    void OnEnableDebugRobot(bool on)
    {
        GameStateMgr.Ins.gameStatus.EnableDebugRobot = on;
    }

    void OnEnableDebugSFX(bool on)
    {
        GameStateMgr.Ins.gameStatus.EnableDebugSFX = on;
    }

    void OnChangePerformance(bool on)
    {
        if (on) {
            if (lowPerfor.isOn)
                GameStateMgr.Ins.gameStatus.TargetFrame = 30;
            if (LowMiddle.isOn)
                GameStateMgr.Ins.gameStatus.TargetFrame = 60;
            if (highPerfor.isOn)
                GameStateMgr.Ins.gameStatus.TargetFrame = 90;
            if (superHighPerfor.isOn)
                GameStateMgr.Ins.gameStatus.TargetFrame = 120;
        }
        Application.targetFrameRate = GameStateMgr.Ins.gameStatus.TargetFrame;
    }

    void OnShowWayPoint(bool on)
    {
        GameStateMgr.Ins.gameStatus.ShowWayPoint = on;
    }


    void OnMusicVolumeChange(float vo)
    {
        SoundManager.Ins.SetMusicVolume(vo);
        if (Main.Ins != null)
            GameStateMgr.Ins.gameStatus.MusicVolume = vo;
    }

    void OnEffectVolumeChange(float vo)
    {
        SoundManager.Ins.SetSoundVolume(vo);
        GameStateMgr.Ins.gameStatus.SoundVolume = vo;
    }

    void OnTabShow(bool show)
    {
        if (Control("JoyStick", WndObject).activeInHierarchy && show) {
            Refresh();
        }
    }

    public override void OnRefresh(int message, object param)
    {
        if (message == 0)
        {
            Control("Nick").GetComponentInChildren<Text>().text = GameStateMgr.Ins.gameStatus.NickName;
        }
    }

    public override void OnClose()
    {
        if (flashing) {
            flashing = false;
            KillFade();
        }
    }

    void LoadDebugLevel() {
        GameObject prefab = Control("Level_debug");
        List<LevelData> levels = DataMgr.Ins.GetDebugLevelDatas();
        for (int i = 1; i < levels.Count; i++) {
            LevelData lev = levels[i];
            GameObject btn = GameObject.Instantiate(prefab, prefab.transform.parent);

            btn.GetComponent<Button>().onClick.AddListener(() => {
                EnterLevel(lev);
            });
            btn.GetComponentInChildren<Text>().text = lev.Name;
            btn.SetActive(true);
        }
    }

    void EnterLevel(LevelData lev) {
        U3D.LoadLevel(lev, LevelMode.SinglePlayerTask, (GameMode)lev.LevelType);
    }
}
