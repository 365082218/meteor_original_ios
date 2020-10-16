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
    public EscDialogState(MainDialogMgr dialgState):base(dialgState)
    {

    }

}


public class EscDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        Main.Ins.GameBattleEx.Pause();
    }

    public override void OnDialogStateExit() {
        base.OnDialogStateExit();
        GameStateMgr.Ins.SaveState();
        Main.Ins.GameBattleEx.Resume();
    }

    void Init()
    {
        Control("Continue").GetComponent<Button>().onClick.AddListener(OnClickClose);
        Control("BGMSlider").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.MusicVolume;
        Control("EffectSlider").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.SoundVolume;
        Control("HSliderBar").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.AxisSensitivity.x;
        Control("VSliderBar").GetComponent<Slider>().value = GameStateMgr.Ins.gameStatus.AxisSensitivity.y;
        Control("HValue").GetComponent<Text>().text = string.Format("{0:f1}", GameStateMgr.Ins.gameStatus.AxisSensitivity.x);
        Control("VValue").GetComponent<Text>().text = string.Format("{0:f1}", GameStateMgr.Ins.gameStatus.AxisSensitivity.y);
        Control("BGMSlider").GetComponent<Slider>().onValueChanged.AddListener(OnMusicVolumeChange);
        Control("EffectSlider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectVolumeChange);
        Control("HSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnXSensitivityChange);
        Control("VSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnYSensitivityChange);
        //返回主界面
        Control("QuitGame").GetComponent<Button>().onClick.AddListener(OnClickQuit);
        Control("ResetPosition").GetComponent<Button>().onClick.AddListener(OnResetPosition);
        Control("SetPosition").GetComponent<Button>().onClick.AddListener(OnSetJoyPosition);

        Control("Snow").GetComponent<Button>().onClick.AddListener(()=> { U3D.Snow(); });
        Control("DoScript").GetComponent<Button>().onClick.AddListener(() => { U3D.DoScript(); OnClickClose(); });

        Control("ChangeModel").GetComponent<Button>().onClick.AddListener(() => { Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.ModelSelectDialogState); });

        //观察AI行为，调试AI是否存在问题
        Toggle toggleFollowEnemy = Control("FollowEnemy").GetComponent<Toggle>();
        toggleFollowEnemy.isOn = U3D.WatchAi;
        OnFollowEnemy(U3D.WatchAi);
        toggleFollowEnemy.onValueChanged.AddListener(OnFollowEnemy);

        Control("PrevRobot").GetComponent<Button>().onClick.AddListener(U3D.WatchPrevRobot);
        Control("NextRobot").GetComponent<Button>().onClick.AddListener(U3D.WatchNextRobot);

        Control("Mission").GetComponent<Text>().text = GetMission();
        Control("LevelDesc").GetComponent<Text>().text = GetLevelDesc();
        Control("MissionTab").gameObject.SetActive(CombatData.Ins.GLevelMode == LevelMode.SinglePlayerTask);
        //把一些模式禁用，例如作弊之类的.
        if (GameStateMgr.Ins.gameStatus.CheatEnable)
        {

        }
        else
        {
            Control("ChangeModel").gameObject.SetActive(CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer);
            Control("Snow").SetActive(CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer);
        }
    }

    bool IsFreeCameraActive()
    {
        if (Main.Ins.CameraFree != null && Main.Ins.CameraFree.isActiveAndEnabled)
            return true;
        return false;
    }

    void OnFollowEnemy(bool follow)
    {
        if (U3D.WatchAi != follow)
        {
            U3D.WatchAi = follow;
            if (U3D.WatchAi)
            {
                //找到第一个未死亡的角色
                MeteorUnit watchTarget = null;
                for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
                {
                    if (MeteorManager.Ins.UnitInfos[i].Dead)
                        continue;
                    if (MeteorManager.Ins.UnitInfos[i] == Main.Ins.LocalPlayer)
                        continue;
                    watchTarget = MeteorManager.Ins.UnitInfos[i];
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

            if (FightState.Exist()) {
                FightState.Instance.UpdateUIButton();
            }
        }
    }


    void OnSetJoyPosition()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.UIAdjustDialogState);
        //UIAdjustWnd.Instance.Open();
    }

    void OnResetPosition()
    {
        //如果在PVP里，是不能这样的。PVP没有寻路，且使用的路点是场景des文件里的user01-user16等
        if (Main.Ins.LocalPlayer != null && !Main.Ins.LocalPlayer.Dead && CombatData.Ins.GLevelItem != null)
            Main.Ins.LocalPlayer.transform.position = CombatData.Ins.wayPoints[0].pos;
    }

    void OnClickClose()
    {
        OnBackPress();
    }

    void OnMusicVolumeChange(float vo)
    {
        SoundManager.Ins.SetMusicVolume(vo);
        if (Main.Ins != null)
            GameStateMgr.Ins.gameStatus.MusicVolume = vo;
    }

    void OnXSensitivityChange(float v)
    {
        GameStateMgr.Ins.gameStatus.AxisSensitivity.x = v;
        Control("HValue").GetComponent<Text>().text = string.Format("{0:f1}", v);
    }

    void OnYSensitivityChange(float v)
    {
        GameStateMgr.Ins.gameStatus.AxisSensitivity.y = v;
        Control("VValue").GetComponent<Text>().text = string.Format("{0:f1}", v);
    }

    void OnEffectVolumeChange(float vo)
    {
        SoundManager.Ins.SetSoundVolume(vo);
        GameStateMgr.Ins.gameStatus.SoundVolume = vo;
    }

    void OnClickQuit()
    {
        Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.EscConfirmDialogState);
    }

    //取得关卡的目标描述
    string GetMission() {
        if (CombatData.Ins.GLevelItem != null) {
            return CombatData.Ins.GLevelItem.Mission;
        }
        return "";
    }

    string GetLevelDesc() {
        if (CombatData.Ins.GLevelItem != null) {
            return CombatData.Ins.GLevelItem.Desc;
        }
        return "";
    }
}
