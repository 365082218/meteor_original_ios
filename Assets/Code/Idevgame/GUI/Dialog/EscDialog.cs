using Idevgame.GameState;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscDialogState : CommonDialogState<EscDialog>
{
    public override string DialogName { get { return "EscWnd"; } }
    public EscDialogState(MainDialogStateManager dialgState):base(dialgState)
    {

    }
}


public class EscDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        GameBattleEx.Instance.Pause();
    }

    void Init()
    {
        Control("Continue").GetComponent<Button>().onClick.AddListener(OnClickClose);
        Control("BGMSlider").GetComponent<Slider>().value = GameData.Instance.gameStatus.MusicVolume;
        Control("EffectSlider").GetComponent<Slider>().value = GameData.Instance.gameStatus.SoundVolume;
        Control("HSliderBar").GetComponent<Slider>().value = GameData.Instance.gameStatus.AxisSensitivity.x;
        Control("VSliderBar").GetComponent<Slider>().value = GameData.Instance.gameStatus.AxisSensitivity.y;
        Control("BGMSlider").GetComponent<Slider>().onValueChanged.AddListener(OnMusicVolumeChange);
        Control("EffectSlider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectVolumeChange);
        Control("HSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnXSensitivityChange);
        Control("VSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnYSensitivityChange);
        //返回主界面
        Control("QuitGame").GetComponent<Button>().onClick.AddListener(OnClickQuit);
        Control("ResetPosition").GetComponent<Button>().onClick.AddListener(OnResetPosition);
        Control("ReloadTable").GetComponent<Button>().onClick.AddListener(() => { U3D.ReloadTable(); });
        Control("SetPosition").GetComponent<Button>().onClick.AddListener(OnSetJoyPosition);
        Control("DoScript").GetComponent<Button>().onClick.AddListener(OnDoScript);
        Control("Snow").GetComponent<Button>().onClick.AddListener(OnSnow);

        Toggle toggleDebug = Control("EnableSFX").GetComponent<Toggle>();
        toggleDebug.isOn = GameData.Instance.gameStatus.EnableDebugSFX;
        toggleDebug.onValueChanged.AddListener(OnEnableDebugSFX);

        Toggle toggleRobot = Control("EnableRobot").GetComponent<Toggle>();
        toggleRobot.isOn = GameData.Instance.gameStatus.EnableDebugRobot;
        toggleRobot.onValueChanged.AddListener(OnEnableDebugRobot);
        //战斗内显示角色信息
        Toggle toggleDebugStatus = Control("EnableDebugStatus").GetComponent<Toggle>();
        toggleDebugStatus.isOn = GameData.Instance.gameStatus.EnableDebugStatus;
        toggleDebugStatus.onValueChanged.AddListener(OnEnableDebugStatus);
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
        toggleDisableLock.isOn = !GameData.Instance.gameStatus.AutoLock;
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

        Toggle toggleEnableHighPerformance = Control("HighPerformance").GetComponent<Toggle>();
        toggleEnableHighPerformance.isOn = GameData.Instance.gameStatus.TargetFrame == 60;
        toggleEnableHighPerformance.onValueChanged.AddListener(OnChangePerformance);

#if !STRIP_DBG_SETTING
        Toggle toggleEnableLog = Control("ShowLog").GetComponent<Toggle>();
        toggleEnableLog.isOn = GameData.Instance.gameStatus.EnableLog;
        toggleEnableLog.onValueChanged.AddListener(OnEnableLog);

        Toggle toggleLevelDebug = Control("ShowLevelDebugButton").GetComponent<Toggle>();
        toggleLevelDebug.isOn = GameData.Instance.gameStatus.LevelDebug;
        toggleLevelDebug.onValueChanged.AddListener(OnLevelDebug);
#else
        Control("ShowLog").gameObject.SetActive(false);
        Control("ShowLevelDebugButton").gameObject.SetActive(false);
#endif

        Control("ChangeV107").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("1.07"); });
        Control("ChangeV907").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("9.07"); });


        Control("ChangeModel").GetComponent<Button>().onClick.AddListener(() => { Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.ModelSelectDialogState); });
        Control("UnlockAll").GetComponent<Button>().onClick.AddListener(() => { U3D.UnlockLevel(); });

        Control("SpeedFast").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(true); });
        Control("SpeedSlow").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(false); });

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

        //观察AI行为，调试AI是否存在问题
        Toggle toggleFollowEnemy = Control("FollowEnemy").GetComponent<Toggle>();
        toggleFollowEnemy.isOn = false;
        toggleFollowEnemy.onValueChanged.AddListener(OnFollowEnemy);


        //把一些模式禁用，例如作弊之类的.
        if (GameData.Instance.gameStatus.GodLike)
        {

        }
        else
        {
            Control("ChangeModel").SetActive(false);
            Control("UnlockAll").SetActive(false);
            Control("Snow").SetActive(false);
            Control("SpeedFast").SetActive(false);
            Control("SpeedSlow").SetActive(false);
            Control("EnableRobot").SetActive(false);
            Control("EnableWeaponChoose").SetActive(false);
            Control("EnableSFX").SetActive(false);
            Control("EnableDebugStatus").SetActive(false);
            Control("ShowWayPoint").SetActive(false);
            Control("EnableInfiniteAngry").SetActive(false);
            Control("EnableGodMode").SetActive(false);
            Control("ShowLog").SetActive(false);
            Control("ShowLevelDebugButton").SetActive(false);
            Control("EnableUnDead").SetActive(false);
        }

        InitLevel();
    }

    bool IsFreeCameraActive()
    {
        if (CameraFree.Ins != null && CameraFree.Ins.isActiveAndEnabled)
            return true;
        return false;
    }

    void OnChangeSpeed(bool fast)
    {
        if (Global.Instance.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (MeteorManager.Instance.LocalPlayer != null)
            {
                if (fast)
                    MeteorManager.Instance.LocalPlayer.SpeedFast();
                else
                    MeteorManager.Instance.LocalPlayer.SpeedSlow();
            }
        }
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

    bool followEnemy = false;
    void OnFollowEnemy(bool follow)
    {
        followEnemy = follow;
        if (followEnemy)
        {
            //找到第一个未死亡的敌对角色
            MeteorUnit watchTarget = null;
            for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
            {
                if (MeteorManager.Instance.UnitInfos[i].Dead)
                    continue;
                if (MeteorManager.Instance.UnitInfos[i].SameCamp(MeteorManager.Instance.LocalPlayer))
                    continue;
                watchTarget = MeteorManager.Instance.UnitInfos[i];
                break;
            }

            GameBattleEx.Instance.InitFreeCamera(watchTarget);
            GameBattleEx.Instance.EnableFollowCamera(false);
        }
        else
        {
            GameBattleEx.Instance.EnableFollowCamera(true);
            GameBattleEx.Instance.EnableFreeCamera(false);
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

#if !STRIP_DBG_SETTING
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
            U3D.Instance.ShowDbg();
        else
            U3D.Instance.CloseDbg();
    }
#endif

    void InitLevel()
    {
        Transform LevelRoot = Global.ldaControlX("LevelRoot", WndObject).transform;
        Level[] LevelInfo = LevelMng.Instance.GetAllItem();
        for (int i = 0; i < LevelInfo.Length; i++)
        {
            string strKey = LevelInfo[i].Name;
            AddGridItem(LevelInfo[i].ID, strKey, EnterLevel, LevelRoot);
        }
    }

    void AddGridItem(int i, string strTag, UnityEngine.Events.UnityAction<int> call, Transform parent)
    {
        GameObject objPrefab = Resources.Load("LevelItem", typeof(GameObject)) as GameObject;
        GameObject obj = GameObject.Instantiate(objPrefab) as GameObject;
        obj.transform.SetParent(parent);
        obj.name = strTag;
        obj.GetComponent<Button>().onClick.AddListener(() => { call(i); });
        obj.GetComponentInChildren<Text>().text = strTag;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;
    }

    private void EnterLevel(int levelId)
    {
        OnBackPress();
        GameBattleEx.Instance.Pause();
        U3D.LoadLevel(levelId, LevelMode.SinglePlayerTask, (GameMode)LevelMng.Instance.GetItem(levelId).LevelType);
    }

    void OnChangeVer(string ver)
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            U3D.PopupTip("联机中无法切换版本");
            return;
        }

        if (AppInfo.Instance.MeteorVersion == ver)
        {
            U3D.PopupTip(string.Format("当前流星版本已为{0}", ver));
            return;
        }
        AppInfo.Instance.MeteorVersion = ver;
        GameData.Instance.gameStatus.MeteorVersion = AppInfo.Instance.MeteorVersion;
        GameData.Instance.SaveState();
        //提示返回到主场景，然后重新加载数据
        GameBattleEx.Instance.Pause();
        Main.Instance.StopAllCoroutines();
        SoundManager.Instance.StopAll();
        BuffMng.Instance.Clear();
        MeteorManager.Instance.Clear();
        Main.Instance.ExitState(Main.Instance.FightDialogState);
        Main.Instance.DialogStateManager.CheckAndCloseCurrentDialogIfPresent(Main.Instance.DialogStateManager.EscDialogState);

        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        U3D.ReStart();
    }

    //允许在战斗UI选择武器.
    void OnEnableWeaponChoose(bool on)
    {
        GameData.Instance.gameStatus.EnableWeaponChoose = on;
        if (FightDialogState.Exist())
            FightDialogState.Instance.UpdateUIButton();
    }

    //禁止UI上的相机切换按钮
    void OnDisableLock(bool on)
    {
        GameData.Instance.gameStatus.AutoLock = !on;
        if (CameraFollow.Ins != null)
        {
            if (on)
                CameraFollow.Ins.DisableLock();
            else
                CameraFollow.Ins.EnableLock();
        }

        if (GameBattleEx.Instance != null)
        {
            if (on)
            {
                GameBattleEx.Instance.Unlock();
                GameBattleEx.Instance.DisableLock();
            }
            else
            {
                GameBattleEx.Instance.EnableLock();
            }
        }

        if (on)
        {
            if (FightDialogState.Exist())
                FightDialogState.Instance.HideCameraBtn();
        }
        else
        {
            if (FightDialogState.Exist())
                FightDialogState.Instance.ShowCameraBtn();
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

    void OnEnableDebugStatus(bool on)
    {
        GameData.Instance.gameStatus.EnableDebugStatus = on;
        UnitTopUI[] unitsUI = GameObject.FindObjectsOfType(typeof(UnitTopUI)) as UnitTopUI[];
        for (int i = 0; i < unitsUI.Length; i++)
            unitsUI[i].EnableInfo(on);
    }

    void OnEnableDebugRobot(bool on)
    {
        GameData.Instance.gameStatus.EnableDebugRobot = on;
        if (FightDialogState.Exist())
            FightDialogState.Instance.UpdateUIButton();
    }

    void OnEnableDebugSFX(bool on)
    {
        GameData.Instance.gameStatus.EnableDebugSFX = on;
        if (FightDialogState.Exist())
            FightDialogState.Instance.UpdateUIButton();
    }

    void OnChangePerformance(bool on)
    {
        GameData.Instance.gameStatus.TargetFrame = on ? 60 : 30;
        Application.targetFrameRate = GameData.Instance.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 120;
#endif
    }

    void OnShowWayPoint(bool on)
    {
        GameBattleEx.Instance.ShowWayPoint(on);
    }

    void OnSnow()
    {
        if (Global.Instance.GScript != null)
            Global.Instance.GScript.Snow();
    }

    void OnDoScript()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.ScriptInputDialogState);
    }

    void OnSetJoyPosition()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.UIAdjustDialogState);
        //UIAdjustWnd.Instance.Open();
    }

    void OnResetPosition()
    {
        //如果在PVP里，是不能这样的。PVP没有寻路，且使用的路点是场景des文件里的user01-user16等
        if (MeteorManager.Instance.LocalPlayer != null && !MeteorManager.Instance.LocalPlayer.Dead && Global.Instance.GLevelItem != null)
            MeteorManager.Instance.LocalPlayer.transform.position = Global.Instance.GLevelItem.wayPoint[0].pos;
    }

    void OnClickClose()
    {
        GameData.Instance.SaveState();
        GameBattleEx.Instance.Resume();
        OnBackPress();
    }

    void OnMusicVolumeChange(float vo)
    {
        SoundManager.Instance.SetMusicVolume(vo);
        if (Main.Instance != null)
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

    void OnClickQuit()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.EscConfirmDialogState);
    }
}
