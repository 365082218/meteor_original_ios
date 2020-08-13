using Idevgame.GameState;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Excel2Json;

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
        Control("SetPosition").GetComponent<Button>().onClick.AddListener(OnSetJoyPosition);

        Control("Snow").GetComponent<Button>().onClick.AddListener(OnSnow);
        Control("DoScript").GetComponent<Button>().onClick.AddListener(() => { U3D.DoScript(); });

        Control("ChangeModel").GetComponent<Button>().onClick.AddListener(() => { Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.ModelSelectDialogState); });

        Control("SpeedFast").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(true); });
        Control("SpeedSlow").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(false); });

        //观察AI行为，调试AI是否存在问题
        Toggle toggleFollowEnemy = Control("FollowEnemy").GetComponent<Toggle>();
        toggleFollowEnemy.isOn = watchAi;
        OnFollowEnemy(watchAi);
        toggleFollowEnemy.onValueChanged.AddListener(OnFollowEnemy);

        Control("PrevRobot").GetComponent<Button>().onClick.AddListener(WatchPrevRobot);
        Control("NextRobot").GetComponent<Button>().onClick.AddListener(WatchNextRobot);
        //把一些模式禁用，例如作弊之类的.
        if (Main.Ins.GameStateMgr.gameStatus.CheatEnable)
        {

        }
        else
        {
            Control("ChangeModel").SetActive(false);
            Control("Snow").SetActive(false);
            Control("SpeedFast").SetActive(false);
            Control("SpeedSlow").SetActive(false);
        }
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

    void WatchPrevRobot() {
        if (!watchAi) {
            U3D.InsertSystemMsg("需要先[观察电脑]");
            return;
        }
        MeteorUnit watchTarget = Main.Ins.CameraFree.Watched;
        List<MeteorUnit> allow = new List<MeteorUnit>();
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++) {
            if (Main.Ins.MeteorManager.UnitInfos[i].Dead)
                continue;
            if (Main.Ins.MeteorManager.UnitInfos[i] == Main.Ins.LocalPlayer)
                continue;
            allow.Add(Main.Ins.MeteorManager.UnitInfos[i]);
        }

        int j = allow.IndexOf(watchTarget);
        if (j == 0) {
            watchTarget = allow[allow.Count - 1];
        } else {
            watchTarget = allow[j - 1];
        }
        Main.Ins.CameraFree.Init(watchTarget);
    }

    void WatchNextRobot() {
        if (!watchAi) {
            U3D.InsertSystemMsg("需要先[观察电脑]");
            return;
        }
        MeteorUnit watchTarget = Main.Ins.CameraFree.Watched;
        List<MeteorUnit> allow = new List<MeteorUnit>();
        for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++) {
            if (Main.Ins.MeteorManager.UnitInfos[i].Dead)
                continue;
            if (Main.Ins.MeteorManager.UnitInfos[i] == Main.Ins.LocalPlayer)
                continue;
            allow.Add(Main.Ins.MeteorManager.UnitInfos[i]);
        }

        int j = allow.IndexOf(watchTarget);
        if (j == allow.Count - 1) {
            watchTarget = allow[0];
        } else {
            watchTarget = allow[j + 1];
        }
        Main.Ins.CameraFree.Init(watchTarget);
    }

    void OnFollowEnemy(bool follow)
    {
        if (watchAi != follow)
        {
            watchAi = follow;
            if (watchAi)
            {
                //找到第一个未死亡的角色
                MeteorUnit watchTarget = null;
                for (int i = 0; i < Main.Ins.MeteorManager.UnitInfos.Count; i++)
                {
                    if (Main.Ins.MeteorManager.UnitInfos[i].Dead)
                        continue;
                    if (Main.Ins.MeteorManager.UnitInfos[i] == Main.Ins.LocalPlayer)
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

    void OnSnow()
    {
        if (Main.Ins.CombatData.GScript != null)
            Main.Ins.CombatData.GScript.Snow();
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
