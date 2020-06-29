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
    static bool watchAi = false;//是否在观察AI行为.
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        Main.Ins.GameBattleEx.Pause();
    }

    void Init()
    {
        Control("Continue").GetComponent<Button>().onClick.AddListener(OnClickClose);
        Control("BGMSlider").GetComponent<Slider>().value = Main.Ins.GameStateMgr.gameStatus.MusicVolume;
        Control("EffectSlider").GetComponent<Slider>().value = Main.Ins.GameStateMgr.gameStatus.SoundVolume;
        Control("HSliderBar").GetComponent<Slider>().value = Main.Ins.GameStateMgr.gameStatus.AxisSensitivity.x;
        Control("VSliderBar").GetComponent<Slider>().value = Main.Ins.GameStateMgr.gameStatus.AxisSensitivity.y;
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
        toggleDebug.isOn = Main.Ins.GameStateMgr.gameStatus.EnableDebugSFX;
        toggleDebug.onValueChanged.AddListener(OnEnableDebugSFX);

        Toggle toggleRobot = Control("EnableRobot").GetComponent<Toggle>();
        toggleRobot.isOn = Main.Ins.GameStateMgr.gameStatus.EnableDebugRobot;
        toggleRobot.onValueChanged.AddListener(OnEnableDebugRobot);
        //战斗内显示角色信息
        Toggle toggleDebugStatus = Control("EnableDebugStatus").GetComponent<Toggle>();
        toggleDebugStatus.isOn = Main.Ins.GameStateMgr.gameStatus.EnableDebugStatus;
        toggleDebugStatus.onValueChanged.AddListener(OnEnableDebugStatus);
        //显示武器挑选按钮
        Toggle toggleEnableFunc = Control("EnableWeaponChoose").GetComponent<Toggle>();
        toggleEnableFunc.isOn = Main.Ins.GameStateMgr.gameStatus.EnableWeaponChoose;
        toggleEnableFunc.onValueChanged.AddListener(OnEnableWeaponChoose);
        //无限气
        Toggle toggleEnableInfiniteAngry = Control("EnableInfiniteAngry").GetComponent<Toggle>();
        toggleEnableInfiniteAngry.isOn = Main.Ins.GameStateMgr.gameStatus.EnableInfiniteAngry;
        toggleEnableInfiniteAngry.onValueChanged.AddListener(OnEnableInfiniteAngry);

        //无锁定
        Toggle toggleDisableLock = Control("CameraLock").GetComponent<Toggle>();
        toggleDisableLock.isOn = !Main.Ins.GameStateMgr.gameStatus.AutoLock;
        toggleDisableLock.onValueChanged.AddListener(OnDisableLock);

        Toggle toggleEnableGodMode = Control("EnableGodMode").GetComponent<Toggle>();
        toggleEnableGodMode.isOn = Main.Ins.GameStateMgr.gameStatus.EnableGodMode;
        toggleEnableGodMode.onValueChanged.AddListener(OnEnableGodMode);

        Toggle toggleEnableUndead = Control("EnableUnDead").GetComponent<Toggle>();
        toggleEnableUndead.isOn = Main.Ins.GameStateMgr.gameStatus.Undead;
        toggleEnableUndead.onValueChanged.AddListener(OnEnableUndead);

        Toggle toggleShowWayPoint = Control("ShowWayPoint").GetComponent<Toggle>();
#if !STRIP_DBG_SETTING
        toggleShowWayPoint.isOn = Main.Ins.GameStateMgr.gameStatus.ShowWayPoint;
        toggleShowWayPoint.onValueChanged.AddListener(OnShowWayPoint);
        if (Main.Ins.GameStateMgr.gameStatus.ShowWayPoint)
            OnShowWayPoint(true);
#else
        Destroy(toggleShowWayPoint.gameObject);
#endif
        Toggle ShowTargetBlood = Control("ShowTargetBlood").GetComponent<Toggle>();
        ShowTargetBlood.isOn = Main.Ins.GameStateMgr.gameStatus.ShowBlood;
        ShowTargetBlood.onValueChanged.AddListener((bool selected) => { Main.Ins.GameStateMgr.gameStatus.ShowBlood = selected; });

        Toggle toggleEnableHighPerformance = Control("HighPerformance").GetComponent<Toggle>();
        toggleEnableHighPerformance.isOn = Main.Ins.GameStateMgr.gameStatus.TargetFrame == 60;
        toggleEnableHighPerformance.onValueChanged.AddListener(OnChangePerformance);

#if !STRIP_DBG_SETTING
        Toggle toggleEnableLog = Control("ShowLog").GetComponent<Toggle>();
        toggleEnableLog.isOn = Main.Ins.GameStateMgr.gameStatus.EnableLog;
        toggleEnableLog.onValueChanged.AddListener(OnEnableLog);

        Toggle toggleLevelDebug = Control("ShowLevelDebugButton").GetComponent<Toggle>();
        toggleLevelDebug.isOn = Main.Ins.GameStateMgr.gameStatus.LevelDebug;
        toggleLevelDebug.onValueChanged.AddListener(OnLevelDebug);
#else
        Control("ShowLog").gameObject.SetActive(false);
        Control("ShowLevelDebugButton").gameObject.SetActive(false);
#endif

        Control("ChangeV107").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("1.07"); });
        Control("ChangeV907").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("9.07"); });


        Control("ChangeModel").GetComponent<Button>().onClick.AddListener(() => { Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.ModelSelectDialogState); });
        Control("UnlockAll").GetComponent<Button>().onClick.AddListener(() => { U3D.UnlockLevel(); });

        Control("SpeedFast").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(true); });
        Control("SpeedSlow").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(false); });

        //粒子特效
        Toggle toggleDisableParticle = Control("Particle").GetComponent<Toggle>();
        toggleDisableParticle.isOn = Main.Ins.GameStateMgr.gameStatus.DisableParticle;
        toggleDisableParticle.onValueChanged.AddListener(OnDisableParticle);
        OnDisableParticle(toggleDisableParticle.isOn);

        //关闭摇杆
        Toggle toggleDisableJoyStick = Control("Joystick").GetComponent<Toggle>();
        toggleDisableJoyStick.isOn = Main.Ins.GameStateMgr.gameStatus.DisableJoystick;
        toggleDisableJoyStick.onValueChanged.AddListener(OnDisableJoyStick);
        OnDisableJoyStick(toggleDisableJoyStick.isOn);

        //观察AI行为，调试AI是否存在问题
        Toggle toggleFollowEnemy = Control("FollowEnemy").GetComponent<Toggle>();
        toggleFollowEnemy.isOn = watchAi;
        OnFollowEnemy(watchAi);
        toggleFollowEnemy.onValueChanged.AddListener(OnFollowEnemy);

        //把一些模式禁用，例如作弊之类的.
        if (Main.Ins.GameStateMgr.gameStatus.GodLike)
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
        if (Main.Ins.CameraFree != null && Main.Ins.CameraFree.isActiveAndEnabled)
            return true;
        return false;
    }

    void OnChangeSpeed(bool fast)
    {
        if (Main.Ins.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (Main.Ins.LocalPlayer != null)
            {
                if (fast)
                    Main.Ins.LocalPlayer.SpeedFast();
                else
                    Main.Ins.LocalPlayer.SpeedSlow();
            }
        }
    }

    void OnDisableJoyStick(bool disable)
    {
        Main.Ins.GameStateMgr.gameStatus.DisableJoystick = disable;
        if (NGUIJoystick.instance != null)
        {
            if (Main.Ins.GameStateMgr.gameStatus.DisableJoystick)
                NGUIJoystick.instance.OnDisabled();
            else
                NGUIJoystick.instance.OnEnabled();
        }
    }

    void OnFollowEnemy(bool follow)
    {
        if (watchAi != follow)
        {
            watchAi = follow;
            if (watchAi)
            {
                //找到第一个未死亡的敌对角色
                MeteorUnit watchTarget = null;
                for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
                {
                    if (Main.Ins.MeteorManager.UnitInfos[i].Dead)
                        continue;
                    if (Main.Ins.MeteorManager.UnitInfos[i].SameCamp(Main.Ins.LocalPlayer))
                        continue;
                    watchTarget = Main.Ins.MeteorManager.UnitInfos[i];
                    break;
                }

                Main.Ins.GameBattleEx.InitFreeCamera(watchTarget);
                Main.Ins.GameBattleEx.EnableFollowCamera(false);
                Main.Ins.MainCamera = Main.Ins.CameraFree.m_Camera;
            }
            else
            {
                Main.Ins.GameBattleEx.EnableFollowCamera(true);
                Main.Ins.GameBattleEx.EnableFreeCamera(false);
                Main.Ins.MainCamera = Main.Ins.CameraFollow.m_Camera;
            }
        }
    }

    void OnDisableParticle(bool disable)
    {
        Main.Ins.GameStateMgr.gameStatus.DisableParticle = disable;
        if (disable)
        {
            if (Main.Ins.CombatData.GScript != null)
            {
                Main.Ins.CombatData.GScript.CleanSceneParticle();
            }

        }
    }

#if !STRIP_DBG_SETTING
    void OnEnableLog(bool toggle)
    {
        Main.Ins.GameStateMgr.gameStatus.EnableLog = toggle;
        if (toggle)
            WSDebug.Ins.OpenLogView();
        else
            WSDebug.Ins.CloseLogView();
    }


    void OnLevelDebug(bool toggle)
    {
        Main.Ins.GameStateMgr.gameStatus.LevelDebug = toggle;
        if (toggle)
            U3D.Instance.ShowDbg();
        else
            U3D.Instance.CloseDbg();
    }
#endif

    void InitLevel()
    {
        Transform LevelRoot = NodeHelper.Find("LevelRoot", WndObject).transform;
        List<LevelDatas.LevelDatas> LevelInfo = Main.Ins.DataMgr.GetDatasArray<LevelDatas.LevelDatas>();
        for (int i = 0; i < LevelInfo.Count; i++)
        {
            string strKey = LevelInfo[i].Name;
            AddGridItem(LevelInfo[i], strKey, EnterLevel, LevelRoot);
        }
    }

    void AddGridItem(LevelDatas.LevelDatas i, string strTag, UnityEngine.Events.UnityAction<LevelDatas.LevelDatas> call, Transform parent)
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

    private void EnterLevel(LevelDatas.LevelDatas lev)
    {
        OnBackPress();
        Main.Ins.GameBattleEx.Pause();
        
        U3D.LoadLevel(lev, LevelMode.SinglePlayerTask, (GameMode)lev.LevelType);
    }

    void OnChangeVer(string ver)
    {
        if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
        {
            U3D.PopupTip("联机中无法切换版本");
            return;
        }

        if (Main.Ins.AppInfo.MeteorVersion == ver)
        {
            U3D.PopupTip(string.Format("当前流星版本已为{0}", ver));
            return;
        }
        Main.Ins.AppInfo.MeteorVersion = ver;
        Main.Ins.GameStateMgr.gameStatus.MeteorVersion = Main.Ins.AppInfo.MeteorVersion;
        Main.Ins.GameStateMgr.SaveState();
        //提示返回到主场景，然后重新加载数据
        Main.Ins.GameBattleEx.Pause();
        Main.Ins.StopAllCoroutines();
        Main.Ins.SoundManager.StopAll();
        Main.Ins.BuffMng.Clear();
        Main.Ins.MeteorManager.Clear();
        Main.Ins.ExitState(Main.Ins.FightState);
        Main.Ins.DialogStateManager.CheckAndCloseCurrentDialogIfPresent(Main.Ins.DialogStateManager.EscDialogState);

        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        U3D.ReStart();
    }

    //允许在战斗UI选择武器.
    void OnEnableWeaponChoose(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.EnableWeaponChoose = on;
        if (FightState.Exist())
            FightState.Instance.UpdateUIButton();
    }

    //禁止UI上的相机切换按钮
    void OnDisableLock(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.AutoLock = !on;
        if (Main.Ins.CameraFollow != null)
        {
            if (on)
                Main.Ins.CameraFollow.DisableLock();
            else
                Main.Ins.CameraFollow.EnableLock();
        }

        if (Main.Ins.GameBattleEx != null)
        {
            if (on)
            {
                Main.Ins.GameBattleEx.Unlock();
                Main.Ins.GameBattleEx.DisableLock();
            }
            else
            {
                Main.Ins.GameBattleEx.EnableLock();
            }
        }

        if (on)
        {
            if (FightState.Exist())
                FightState.Instance.HideCameraBtn();
        }
        else
        {
            if (FightState.Exist())
                FightState.Instance.ShowCameraBtn();
        }
    }

    void OnEnableUndead(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.Undead = on;
    }

    void OnEnableGodMode(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.EnableGodMode = on;
    }

    void OnEnableInfiniteAngry(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.EnableInfiniteAngry = on;
    }

    void OnEnableDebugStatus(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.EnableDebugStatus = on;
        UnitTopUI[] unitsUI = GameObject.FindObjectsOfType(typeof(UnitTopUI)) as UnitTopUI[];
        for (int i = 0; i < unitsUI.Length; i++)
            unitsUI[i].EnableInfo(on);
    }

    void OnEnableDebugRobot(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.EnableDebugRobot = on;
        if (FightState.Exist())
            FightState.Instance.UpdateUIButton();
    }

    void OnEnableDebugSFX(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.EnableDebugSFX = on;
        if (FightState.Exist())
            FightState.Instance.UpdateUIButton();
    }

    void OnChangePerformance(bool on)
    {
        Main.Ins.GameStateMgr.gameStatus.TargetFrame = on ? 60 : 30;
        Application.targetFrameRate = Main.Ins.GameStateMgr.gameStatus.TargetFrame;
#if UNITY_EDITOR
        Application.targetFrameRate = 120;
#endif
    }

#if !STRIP_DBG_SETTING
    void OnShowWayPoint(bool on)
    {
        Main.Ins.GameBattleEx.ShowWayPoint(on);
    }
#endif

    void OnSnow()
    {
        if (Main.Ins.CombatData.GScript != null)
            Main.Ins.CombatData.GScript.Snow();
    }

    void OnDoScript()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.ScriptInputDialogState);
    }

    void OnSetJoyPosition()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.UIAdjustDialogState);
        //UIAdjustWnd.Instance.Open();
    }

    void OnResetPosition()
    {
        //如果在PVP里，是不能这样的。PVP没有寻路，且使用的路点是场景des文件里的user01-user16等
        if (Main.Ins.LocalPlayer != null && !Main.Ins.LocalPlayer.Dead && Main.Ins.CombatData.GLevelItem != null)
            Main.Ins.LocalPlayer.transform.position = Main.Ins.CombatData.wayPoints[0].pos;
    }

    void OnClickClose()
    {
        Main.Ins.GameStateMgr.SaveState();
        Main.Ins.GameBattleEx.Resume();
        OnBackPress();
    }

    void OnMusicVolumeChange(float vo)
    {
        Main.Ins.SoundManager.SetMusicVolume(vo);
        if (Main.Ins != null)
            Main.Ins.GameStateMgr.gameStatus.MusicVolume = vo;
    }

    void OnXSensitivityChange(float v)
    {
        Main.Ins.GameStateMgr.gameStatus.AxisSensitivity.x = v;
    }

    void OnYSensitivityChange(float v)
    {
        Main.Ins.GameStateMgr.gameStatus.AxisSensitivity.y = v;
    }

    void OnEffectVolumeChange(float vo)
    {
        Main.Ins.SoundManager.SetSoundVolume(vo);
        Main.Ins.GameStateMgr.gameStatus.SoundVolume = vo;
    }

    void OnClickQuit()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.EscConfirmDialogState);
    }
}
