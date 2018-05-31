using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using ProtoBuf;

//主角模型选择.
public class ModelWnd:Window<ModelWnd>
{
    public GameObject GridViewRoot;
    Coroutine loadModel;
    protected override bool OnOpen()
    {
        GridViewRoot = Control("GridViewRoot");
        Control("Close").GetComponent<Button>().onClick.AddListener(()=> { Close(); });
        if (GameBattleEx.Instance != null)
            loadModel = GameBattleEx.Instance.StartCoroutine(LoadModels());
        return base.OnOpen();
    }

    IEnumerator LoadModels()
    {
        for (int i = 0; i < Global.model.Length; i++)
        {
            AddModel(i);
            yield return 0;
        }
    }

    void AddModel(int Idx)
    {
        UIFunCtrl obj = (GameObject.Instantiate(Resources.Load("UIFuncItem")) as GameObject).GetComponent<UIFunCtrl>();
        obj.SetEvent(ChangeModel, Idx);
        obj.SetText(Global.model[Idx]);
        obj.transform.SetParent(GridViewRoot.transform);
        obj.gameObject.layer = GridViewRoot.layer;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

    void ChangeModel(int id)
    {
        //创建一个角色，去替换掉主角色，
        U3D.ChangePlayerModel(id);
    }

    protected override bool OnClose()
    {
        if (loadModel != null && GameBattleEx.Instance != null)
            GameBattleEx.Instance.StopCoroutine(loadModel);
        return base.OnClose();
    }

    protected override bool FullStretch()
    {
        return false;
    }
}

public class NewSystemWnd : Window<NewSystemWnd>
{
    public override string PrefabName { get { return "NewSystemWnd"; } }
    UITab audioTab;
    UITab saveTab;

    protected override bool OnOpen()
    {
        Init();
        GameBattleEx.Instance.Pause();
        return base.OnOpen();
    }
    protected override bool FullStretch()
    {
        return false;
    }

    public override void OnClick()
    {
        //if (mWindowStyle >= WindowStyle.WS_Ext)
        //    OnClickClose();
    }

    void Init()
    {
        Control("Continue").GetComponent<Button>().onClick.AddListener(OnClickClose);
        audioTab = Control("AudioTab").GetComponent<UITab>();
        saveTab = Control("SaveTab").GetComponent<UITab>();
        if (Startup.ins != null)
        {
            Control("BGMSlider").GetComponent<Slider>().value = GameData.gameStatus.MusicVolume;
            Control("EffectSlider").GetComponent<Slider>().value = GameData.gameStatus.SoundVolume;
            Control("HSliderBar").GetComponent<Slider>().value = GameData.gameStatus.AxisSensitivity.x;
            Control("VSliderBar").GetComponent<Slider>().value = GameData.gameStatus.AxisSensitivity.y;
        }
        Control("BGMSlider").GetComponent<Slider>().onValueChanged.AddListener(OnMusicVolumeChange);
        Control("EffectSlider").GetComponent<Slider>().onValueChanged.AddListener(OnEffectVolumeChange);
        Control("HSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnXSensitivityChange);
        Control("VSliderBar").GetComponent<Slider>().onValueChanged.AddListener(OnYSensitivityChange);
        Control("QuitGame").GetComponent<UIButtonExtended>().onClick.AddListener(OnClickBack);
        Control("ResetPosition").GetComponent<Button>().onClick.AddListener(OnResetPosition);
        Control("ReloadTable").GetComponent<Button>().onClick.AddListener(()=> { U3D.ReloadTable(); });
        Control("LoadLevel").GetComponent<Button>().onClick.AddListener(OnLoadLevel);
        Control("SetJoyPosition").GetComponent<Button>().onClick.AddListener(OnSetJoyPosition);
        Control("DoScript").GetComponent<Button>().onClick.AddListener(OnDoScript);
        Toggle toggleDebug = Control("EnableDebug").GetComponent<Toggle>();
        toggleDebug.isOn = GameData.gameStatus.EnableDebug;
        toggleDebug.onValueChanged.AddListener(OnEnableDebug);

        Toggle toggleEnableFunc = Control("EnableFunc").GetComponent<Toggle>();
        toggleEnableFunc.isOn = GameData.gameStatus.EnableFunc;
        toggleEnableFunc.onValueChanged.AddListener(OnEnableFunc);

        Toggle toggleEnableInfiniteAngry = Control("EnableInfiniteAngry").GetComponent<Toggle>();
        toggleEnableInfiniteAngry.isOn = GameData.gameStatus.EnableInfiniteAngry;
        toggleEnableInfiniteAngry.onValueChanged.AddListener(OnEnableInfiniteAngry);

        Toggle toggleEnableItemName = Control("EnableItemName").GetComponent<Toggle>();
        toggleEnableItemName.isOn = GameData.gameStatus.EnableItemName;
        toggleEnableItemName.onValueChanged.AddListener(OnEnableItemName);

        Toggle toggleEnableGodMode = Control("EnableGodMode").GetComponent<Toggle>();
        toggleEnableGodMode.isOn = GameData.gameStatus.EnableGodMode;
        toggleEnableGodMode.onValueChanged.AddListener(OnEnableGodMode);

        Toggle toggleShowWayPoint = Control("ShowWayPoint").GetComponent<Toggle>();
        toggleShowWayPoint.isOn = false;
        toggleShowWayPoint.onValueChanged.AddListener(OnShowWayPoint);

        Control("ChangeV107").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("1.07"); });
        //Control("Ver108").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer(108); });
        Control("ChangeV907").GetComponent<Button>().onClick.AddListener(() => { OnChangeVer("9.07"); });
        Control("AppVerText").GetComponent<Text>().text = AppInfo.AppVersion();
        Control("MeteorVerText").GetComponent<Text>().text = AppInfo.MeteorVersion;
        Control("ChangeModel").GetComponent<Button>().onClick.AddListener(() => { ModelWnd.Instance.Open(); });
        mWindowStyle = WindowStyle.WS_Modal;
    }

    void OnChangeVer(string ver)
    {
        if (AppInfo.MeteorVersion == ver)
        {
            U3D.PopupTip(string.Format("当前流星版本已为{0}", ver));
            return;
        }
        AppInfo.MeteorVersion = ver;
        GameData.gameStatus.MeteorVersion = AppInfo.MeteorVersion;
        GameData.SaveState();
        //提示返回到主场景，然后重新加载数据
        GameBattleEx.Instance.Pause();
        GameBattleEx.Instance.StopAllCoroutines();
        SoundManager.Instance.StopAll();
        BuffMng.Instance.Clear();
        MeteorManager.Instance.Clear();
        Close();
        FightWnd.Instance.Close();
        if (GameOverlayWnd.Exist)
            GameOverlayWnd.Instance.ClearSystemMsg();
        U3D.ReStart();
    }

    void OnEnableFunc(bool on)
    {
        GameData.gameStatus.EnableFunc = on;
        if (FightWnd.Exist)
            FightWnd.Instance.UpdateUIButton();
    }

    void OnEnableGodMode(bool on)
    {
        GameData.gameStatus.EnableGodMode = on;
    }

    void OnEnableInfiniteAngry(bool on)
    {
        GameData.gameStatus.EnableInfiniteAngry = on;
    }

    void OnEnableItemName(bool on)
    {
        GameData.gameStatus.EnableItemName = on;
    }

    void OnEnableDebug(bool on)
    {
        GameData.gameStatus.EnableDebug = on;
        if (FightWnd.Exist)
            FightWnd.Instance.UpdateUIButton();
    }

    void OnLoadLevel()
    {
        WsGlobal.ShowLevelSelect();
        OnClickClose();
    }

    void OnShowWayPoint(bool on)
    {
        GameBattleEx.Instance.ShowWayPoint(on);
    }

    void OnDoScript()
    {
        ScriptInputWnd.Instance.Open();
    }

    void OnSetJoyPosition()
    {
        JoyAdjustWnd.Instance.Open();
    }

    void OnResetPosition()
    {
        //如果在PVP里，是不能这样的。PVP没有寻路，且使用的路点是场景des文件里的user01-user16等
        if (MeteorManager.Instance.LocalPlayer != null && !MeteorManager.Instance.LocalPlayer.Dead && Global.GLevelItem != null)
            MeteorManager.Instance.LocalPlayer.transform.position = Global.GLevelItem.wayPoint[0].pos;
    }

    void OnClickClose()
    {
        GameBattleEx.Instance.Resume();
        base.Close();
    }

    void OnMusicVolumeChange(float vo)
    {
        SoundManager.Instance.SetMusicVolume(vo);
        if (Startup.ins != null)
            GameData.gameStatus.MusicVolume = vo;
    }

    void OnXSensitivityChange(float v)
    {
        GameData.gameStatus.AxisSensitivity.x = v;
    }

    void OnYSensitivityChange(float v)
    {
        GameData.gameStatus.AxisSensitivity.y = v;
    }

    void OnEffectVolumeChange(float vo)
    {
        SoundManager.Instance.SetSoundVolume(vo);
        GameData.gameStatus.SoundVolume = vo;
    }

    void OnClickBack()
    {
        GameBattleEx.Instance.Pause();
        GameBattleEx.Instance.StopAllCoroutines();
        SoundManager.Instance.StopAll();
        BuffMng.Instance.Clear();
        MeteorManager.Instance.Clear();
        Close();
        FightWnd.Instance.Close();
        if (GameOverlayWnd.Exist)
            GameOverlayWnd.Instance.ClearSystemMsg();
        U3D.GoBack();
    }
}
