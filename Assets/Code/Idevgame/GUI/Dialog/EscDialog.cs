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
        Main.Instance.GameBattleEx.Pause();
    }

    void Init()
    {
        Control("Continue").GetComponent<Button>().onClick.AddListener(OnClickClose);
        Control("BGMSlider").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.MusicVolume;
        Control("EffectSlider").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.SoundVolume;
        Control("HSliderBar").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.AxisSensitivity.x;
        Control("VSliderBar").GetComponent<Slider>().value = Main.Instance.GameStateMgr.gameStatus.AxisSensitivity.y;
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
        toggleDebug.isOn = Main.Instance.GameStateMgr.gameStatus.EnableDebugSFX;
        toggleDebug.onValueChanged.AddListener(OnEnableDebugSFX);

        Toggle toggleRobot = Control("EnableRobot").GetComponent<Toggle>();
        toggleRobot.isOn = Main.Instance.GameStateMgr.gameStatus.EnableDebugRobot;
        toggleRobot.onValueChanged.AddListener(OnEnableDebugRobot);
        //战斗内显示角色信息
        Toggle toggleDebugStatus = Control("EnableDebugStatus").GetComponent<Toggle>();
        toggleDebugStatus.isOn = Main.Instance.GameStateMgr.gameStatus.EnableDebugStatus;
        toggleDebugStatus.onValueChanged.AddListener(OnEnableDebugStatus);
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
        toggleDisableLock.isOn = !Main.Instance.GameStateMgr.gameStatus.AutoLock;
        toggleDisableLock.onValueChanged.AddListener(OnDisableLock);

        Toggle toggleEnableGodMode = Control("EnableGodMode").GetComponent<Toggle>();
        toggleEnableGodMode.isOn = Main.Instance.GameStateMgr.gameStatus.EnableGodMode;
        toggleEnableGodMode.onValueChanged.AddListener(OnEnableGodMode);

        Toggle toggleEnableUndead = Control("EnableUnDead").GetComponent<Toggle>();
        toggleEnableUndead.isOn = Main.Instance.GameStateMgr.gameStatus.Undead;
        toggleEnableUndead.onValueChanged.AddListener(OnEnableUndead);

        Toggle toggleShowWayPoint = Control("ShowWayPoint").GetComponent<Toggle>();
#if !STRIP_DBG_SETTING
        toggleShowWayPoint.isOn = GameData.Instance.gameStatus.ShowWayPoint;
        toggleShowWayPoint.onValueChanged.AddListener(OnShowWayPoint);
        if (GameData.Instance.gameStatus.ShowWayPoint)
            OnShowWayPoint(true);
#else
        Destroy(toggleShowWayPoint.gameObject);
#endif
        Toggle ShowTargetBlood = Control("ShowTargetBlood").GetComponent<Toggle>();
        ShowTargetBlood.isOn = Main.Instance.GameStateMgr.gameStatus.ShowBlood;
        ShowTargetBlood.onValueChanged.AddListener((bool selected) => { Main.Instance.GameStateMgr.gameStatus.ShowBlood = selected; });

        Toggle toggleEnableHighPerformance = Control("HighPerformance").GetComponent<Toggle>();
        toggleEnableHighPerformance.isOn = Main.Instance.GameStateMgr.gameStatus.TargetFrame == 60;
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
        toggleDisableParticle.isOn = Main.Instance.GameStateMgr.gameStatus.DisableParticle;
        toggleDisableParticle.onValueChanged.AddListener(OnDisableParticle);
        OnDisableParticle(toggleDisableParticle.isOn);

        //关闭摇杆
        Toggle toggleDisableJoyStick = Control("Joystick").GetComponent<Toggle>();
        toggleDisableJoyStick.isOn = Main.Instance.GameStateMgr.gameStatus.DisableJoystick;
        toggleDisableJoyStick.onValueChanged.AddListener(OnDisableJoyStick);
        OnDisableJoyStick(toggleDisableJoyStick.isOn);

        //观察AI行为，调试AI是否存在问题
        Toggle toggleFollowEnemy = Control("FollowEnemy").GetComponent<Toggle>();
        toggleFollowEnemy.isOn = watchAi;
        OnFollowEnemy(watchAi);
        toggleFollowEnemy.onValueChanged.AddListener(OnFollowEnemy);

        //把一些模式禁用，例如作弊之类的.
        if (Main.Instance.GameStateMgr.gameStatus.GodLike)
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
        if (Main.Instance.CameraFree != null && Main.Instance.CameraFree.isActiveAndEnabled)
            return true;
        return false;
    }

    void OnChangeSpeed(bool fast)
    {
        if (Main.Instance.CombatData.GLevelMode <= LevelMode.SinglePlayerTask)
        {
            if (Main.Instance.MeteorManager.LocalPlayer != null)
            {
                if (fast)
                    Main.Instance.MeteorManager.LocalPlayer.SpeedFast();
                else
                    Main.Instance.MeteorManager.LocalPlayer.SpeedSlow();
            }
        }
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

    void OnFollowEnemy(bool follow)
    {
        if (watchAi != follow)
        {
            watchAi = follow;
            if (watchAi)
            {
                //找到第一个未死亡的敌对角色
                MeteorUnit watchTarget = null;
                for (int i = 0; i < Main.Instance.MeteorManager.UnitInfos.Count; i++)
                {
                    if (Main.Instance.MeteorManager.UnitInfos[i].Dead)
                        continue;
                    if (Main.Instance.MeteorManager.UnitInfos[i].SameCamp(Main.Instance.MeteorManager.LocalPlayer))
                        continue;
                    watchTarget = Main.Instance.MeteorManager.UnitInfos[i];
                    break;
                }

                Main.Instance.GameBattleEx.InitFreeCamera(watchTarget);
                Main.Instance.GameBattleEx.EnableFollowCamera(false);
                Main.Instance.MainCamera = Main.Instance.CameraFree.m_Camera;
            }
            else
            {
                Main.Instance.GameBattleEx.EnableFollowCamera(true);
                Main.Instance.GameBattleEx.EnableFreeCamera(false);
                Main.Instance.MainCamera = Main.Instance.CameraFollow.m_Camera;
            }
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
        Transform LevelRoot = NodeHelper.Find("LevelRoot", WndObject).transform;
        List<LevelDatas.LevelDatas> LevelInfo = Main.Instance.DataMgr.GetDatasArray<LevelDatas.LevelDatas>();
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
        Main.Instance.GameBattleEx.Pause();
        
        U3D.LoadLevel(lev, LevelMode.SinglePlayerTask, (GameMode)lev.LevelType);
    }

    void OnChangeVer(string ver)
    {
        if (Main.Instance.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
        {
            U3D.PopupTip("联机中无法切换版本");
            return;
        }

        if (Main.Instance.AppInfo.MeteorVersion == ver)
        {
            U3D.PopupTip(string.Format("当前流星版本已为{0}", ver));
            return;
        }
        Main.Instance.AppInfo.MeteorVersion = ver;
        Main.Instance.GameStateMgr.gameStatus.MeteorVersion = Main.Instance.AppInfo.MeteorVersion;
        Main.Instance.GameStateMgr.SaveState();
        //提示返回到主场景，然后重新加载数据
        Main.Instance.GameBattleEx.Pause();
        Main.Instance.StopAllCoroutines();
        Main.Instance.SoundManager.StopAll();
        Main.Instance.BuffMng.Clear();
        Main.Instance.MeteorManager.Clear();
        Main.Instance.ExitState(Main.Instance.FightDialogState);
        Main.Instance.DialogStateManager.CheckAndCloseCurrentDialogIfPresent(Main.Instance.DialogStateManager.EscDialogState);

        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        U3D.ReStart();
    }

    //允许在战斗UI选择武器.
    void OnEnableWeaponChoose(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableWeaponChoose = on;
        if (FightDialogState.Exist())
            FightDialogState.Instance.UpdateUIButton();
    }

    //禁止UI上的相机切换按钮
    void OnDisableLock(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.AutoLock = !on;
        if (Main.Instance.CameraFollow != null)
        {
            if (on)
                Main.Instance.CameraFollow.DisableLock();
            else
                Main.Instance.CameraFollow.EnableLock();
        }

        if (Main.Instance.GameBattleEx != null)
        {
            if (on)
            {
                Main.Instance.GameBattleEx.Unlock();
                Main.Instance.GameBattleEx.DisableLock();
            }
            else
            {
                Main.Instance.GameBattleEx.EnableLock();
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

    void OnEnableDebugStatus(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableDebugStatus = on;
        UnitTopUI[] unitsUI = GameObject.FindObjectsOfType(typeof(UnitTopUI)) as UnitTopUI[];
        for (int i = 0; i < unitsUI.Length; i++)
            unitsUI[i].EnableInfo(on);
    }

    void OnEnableDebugRobot(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableDebugRobot = on;
        if (FightDialogState.Exist())
            FightDialogState.Instance.UpdateUIButton();
    }

    void OnEnableDebugSFX(bool on)
    {
        Main.Instance.GameStateMgr.gameStatus.EnableDebugSFX = on;
        if (FightDialogState.Exist())
            FightDialogState.Instance.UpdateUIButton();
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
        GameBattleEx.Instance.ShowWayPoint(on);
    }
#endif

    void OnSnow()
    {
        if (Main.Instance.CombatData.GScript != null)
            Main.Instance.CombatData.GScript.Snow();
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
        if (Main.Instance.MeteorManager.LocalPlayer != null && !Main.Instance.MeteorManager.LocalPlayer.Dead && Main.Instance.CombatData.GLevelItem != null)
            Main.Instance.MeteorManager.LocalPlayer.transform.position = Main.Instance.CombatData.wayPoints[0].pos;
    }

    void OnClickClose()
    {
        Main.Instance.GameStateMgr.SaveState();
        Main.Instance.GameBattleEx.Resume();
        OnBackPress();
    }

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

    void OnClickQuit()
    {
        Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.EscConfirmDialogState);
    }
}
