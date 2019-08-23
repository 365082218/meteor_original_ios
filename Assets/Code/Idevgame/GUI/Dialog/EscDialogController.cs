using Idevgame.GameState;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscDialogState : CommonDialogState<EscDialogController>
{
    public override string DialogName { get { return "EscWnd"; } }
    public EscDialogState(MainDialogStateManager dialgState):base(dialgState)
    {

    }
}


public class EscDialogController : Dialog
{
    public Button Continue;
    public Slider BGMSlider;
    public Slider EffectSlider;
    public Slider HSliderBar;
    public Slider VSliderBar;
    public Button QuitGame;
    public Button ResetPosition;
    public Button ReloadTable;
    public Button SetPosition;
    public Button DoScript;
    public Button Snow;
    public Toggle EnableSFX;
    public Toggle EnableRobot;
    public Toggle EnableDebugStatus;
    public Toggle EnableWeaponChoose;
    public Toggle EnableInfiniteAngry;
    public Toggle CameraLock;
    public Toggle EnableGodMode;
    public Toggle EnableUnDead;
    public Toggle ShowWayPoint;
    public Toggle HighPerformance;
    public Toggle ShowLog;
    public Toggle ShowLevelDebugButton;
    public Button ChangeV107;
    public Button ChangeV907;
    public Button ChangeModel;
    public Button UnlockAll;
    public Button SpeedFast;
    public Button SpeedSlow;
    public Toggle Particle;
    public Toggle Joystick;
    public Toggle FollowEnemy;
    public Transform LevelRoot;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        GameBattleEx.Instance.Pause();
    }

    void Init()
    {
        Continue.onClick.AddListener(OnClickClose);
        if (Main.Instance != null)
        {
            BGMSlider.value = GameData.Instance.gameStatus.MusicVolume;
            EffectSlider.value = GameData.Instance.gameStatus.SoundVolume;
            HSliderBar.value = GameData.Instance.gameStatus.AxisSensitivity.x;
            VSliderBar.value = GameData.Instance.gameStatus.AxisSensitivity.y;
        }
        BGMSlider.onValueChanged.AddListener(OnMusicVolumeChange);
        EffectSlider.onValueChanged.AddListener(OnEffectVolumeChange);
        HSliderBar.onValueChanged.AddListener(OnXSensitivityChange);
        VSliderBar.GetComponent<Slider>().onValueChanged.AddListener(OnYSensitivityChange);
        //返回主界面
        QuitGame.onClick.AddListener(OnClickQuit);
        ResetPosition.onClick.AddListener(OnResetPosition);
        ReloadTable.onClick.AddListener(() => { U3D.ReloadTable(); });
        SetPosition.onClick.AddListener(OnSetJoyPosition);
        DoScript.onClick.AddListener(OnDoScript);
        Snow.onClick.AddListener(OnSnow);
        //显示战斗界面的调试按钮
        EnableSFX.isOn = GameData.Instance.gameStatus.EnableDebugSFX;
        EnableSFX.onValueChanged.AddListener(OnEnableDebugSFX);
        //显示战斗界面的调试按钮
        EnableRobot.isOn = GameData.Instance.gameStatus.EnableDebugRobot;
        EnableRobot.onValueChanged.AddListener(OnEnableDebugRobot);
        //战斗内显示角色信息
        EnableDebugStatus.isOn = GameData.Instance.gameStatus.EnableDebugStatus;
        EnableDebugStatus.onValueChanged.AddListener(OnEnableDebugStatus);
        //显示武器挑选按钮
        EnableWeaponChoose.isOn = GameData.Instance.gameStatus.EnableWeaponChoose;
        EnableWeaponChoose.onValueChanged.AddListener(OnEnableWeaponChoose);
        //无限气
        EnableInfiniteAngry.isOn = GameData.Instance.gameStatus.EnableInfiniteAngry;
        EnableInfiniteAngry.onValueChanged.AddListener(OnEnableInfiniteAngry);
        //无锁定
        CameraLock.isOn = !GameData.Instance.gameStatus.AutoLock;
        CameraLock.onValueChanged.AddListener(OnDisableLock);

        EnableGodMode.isOn = GameData.Instance.gameStatus.EnableGodMode;
        EnableGodMode.onValueChanged.AddListener(OnEnableGodMode);

        EnableUnDead.isOn = GameData.Instance.gameStatus.Undead;
        EnableUnDead.onValueChanged.AddListener(OnEnableUndead);

        ShowWayPoint.isOn = GameData.Instance.gameStatus.ShowWayPoint;
        ShowWayPoint.onValueChanged.AddListener(OnShowWayPoint);
        if (GameData.Instance.gameStatus.ShowWayPoint)
            OnShowWayPoint(true);

        HighPerformance.isOn = GameData.Instance.gameStatus.TargetFrame == 60;
        HighPerformance.onValueChanged.AddListener(OnChangePerformance);

        ShowLog.isOn = GameData.Instance.gameStatus.EnableLog;
        ShowLog.onValueChanged.AddListener(OnEnableLog);
        OnEnableLog(ShowLog.isOn);

        ShowLevelDebugButton.isOn = GameData.Instance.gameStatus.LevelDebug;
        ShowLevelDebugButton.onValueChanged.AddListener(OnLevelDebug);
        OnLevelDebug(ShowLevelDebugButton.isOn);

        ChangeV107.onClick.AddListener(() => { OnChangeVer("1.07"); });
        //Control("Ver108").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer(108); });
        ChangeV907.onClick.AddListener(() => { OnChangeVer("9.07"); });


        ChangeModel.onClick.AddListener(() => { Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.ModelSelectDialogState); });
        UnlockAll.onClick.AddListener(() => { U3D.UnlockLevel(); });

        SpeedFast.onClick.AddListener(() => { OnChangeSpeed(true); });
        SpeedSlow.onClick.AddListener(() => { OnChangeSpeed(false); });

        //粒子特效
        Particle.isOn = GameData.Instance.gameStatus.DisableParticle;
        Particle.onValueChanged.AddListener(OnDisableParticle);
        OnDisableParticle(Particle.isOn);

        //关闭摇杆
        Joystick.isOn = GameData.Instance.gameStatus.DisableJoystick;
        Joystick.onValueChanged.AddListener(OnDisableJoyStick);
        OnDisableJoyStick(Joystick.isOn);

        //观察AI行为，调试AI是否存在问题
        FollowEnemy.isOn = false;
        FollowEnemy.onValueChanged.AddListener(OnFollowEnemy);

        //把一些模式禁用，例如作弊之类的.
        if (GameData.Instance.gameStatus.GodLike)
        {

        }
        else
        {
            ChangeModel.gameObject.SetActive(false);
            UnlockAll.gameObject.SetActive(false);
            Snow.gameObject.SetActive(false);
            SpeedFast.gameObject.SetActive(false);
            SpeedSlow.gameObject.SetActive(false);
            EnableRobot.gameObject.SetActive(false);
            EnableWeaponChoose.gameObject.SetActive(false);
            EnableSFX.gameObject.SetActive(false);
            EnableDebugStatus.gameObject.SetActive(false);
            ShowWayPoint.gameObject.SetActive(false);
            EnableInfiniteAngry.gameObject.SetActive(false);
            EnableGodMode.gameObject.SetActive(false);
            ShowLog.gameObject.SetActive(false);
            ShowLevelDebugButton.gameObject.SetActive(false);
            EnableUnDead.gameObject.SetActive(false);
        }

        InitLevel();
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
            GamePool.Instance.ShowDbg();
        else
            GamePool.Instance.CloseDbg();
    }

    void InitLevel()
    {
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
