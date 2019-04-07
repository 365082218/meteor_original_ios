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
        List<int> allModel = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            allModel.Add(i);
        }

        if (GameData.Instance.gameStatus.pluginModel != null)
        {
            for (int i = 0; i < GameData.Instance.gameStatus.pluginModel.Count; i++)
            {
                GameData.Instance.gameStatus.pluginModel[i].Check();
                if (GameData.Instance.gameStatus.pluginModel[i].Installed)
                    allModel.Add(GameData.Instance.gameStatus.pluginModel[i].ModelId);
            }
        }

        for (int i = 0; i < allModel.Count; i++)
        {
            AddModel(allModel[i]);
            yield return 0;
        }
    }

    void AddModel(int Idx)
    {
        UIFunCtrl obj = (GameObject.Instantiate(Resources.Load("UIFuncItem")) as GameObject).GetComponent<UIFunCtrl>();
        obj.SetEvent(ChangeModel, Idx);
        obj.SetText(Global.Instance.GetCharacter(Idx));
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
}

public class EscWnd : Window<EscWnd>
{
    public override string PrefabName { get { return "EscWnd"; } }

    protected override bool OnOpen()
    {
        Init();
        GameBattleEx.Instance.Pause();
        return base.OnOpen();
    }

    public override void OnClick()
    {
        //if (mWindowStyle >= WindowStyle.WS_Ext)
        //    OnClickClose();
    }

    void Init()
    {
//#if UNITY_STANDALONE
//        Cursor.lockState = CursorLockMode.None;
//#endif
        Control("Continue").GetComponent<Button>().onClick.AddListener(OnClickClose);
        Control("QuitGame").GetComponent<Button>().onClick.AddListener(OnClickBack);
        Control("ResetPosition").GetComponent<Button>().onClick.AddListener(OnResetPosition);
        Control("DoScript").GetComponent<Button>().onClick.AddListener(OnDoScript);
        Control("Snow").GetComponent<Button>().onClick.AddListener(OnSnow);
        Control("SpeedFast").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(true); });
        Control("SpeedSlow").GetComponent<Button>().onClick.AddListener(() => { OnChangeSpeed(false); });
        Control("ChangeModel").GetComponent<Button>().onClick.AddListener(() => { ModelWnd.Instance.Open(); });
        Control("ChangeUIPosition").GetComponent<Button>().onClick.AddListener(() => { UIAdjustWnd.Instance.Open(); });
        //战斗内显示角色信息
        Toggle toggleDebugStatus = Control("EnableDebugStatus").GetComponent<Toggle>();
        toggleDebugStatus.isOn = GameData.Instance.gameStatus.EnableDebugStatus;
        toggleDebugStatus.onValueChanged.AddListener(OnEnableDebugStatus);
        mWindowStyle = WindowStyle.WS_Modal;
    }

    void OnEnableDebugStatus(bool on)
    {
        GameData.Instance.gameStatus.EnableDebugStatus = on;
        UnitTopUI[] unitsUI = GameObject.FindObjectsOfType(typeof(UnitTopUI)) as UnitTopUI[];
        for (int i = 0; i < unitsUI.Length; i++)
            unitsUI[i].EnableInfo(on);
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

    void OnSnow()
    {
        if (Global.Instance.GScript != null)
            Global.Instance.GScript.Snow();
    }

    void OnDoScript()
    {
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
        {
            U3D.PopupTip("联机模式不允许使用秘籍");
            return;
        }
        ScriptInputWnd.Instance.Open();
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
        base.Close();
    }

    void OnClickBack()
    {
        GameData.Instance.SaveState();
        GameBattleEx.Instance.Pause();
        GameBattleEx.Instance.StopAllCoroutines();
        SoundManager.Instance.StopAll();
        BuffMng.Instance.Clear();
        MeteorManager.Instance.Clear();
        Close();
        if (FightWnd.Exist)
            FightWnd.Instance.Close();
        if (GameOverlayWnd.Exist)
            GameOverlayWnd.Instance.ClearSystemMsg();
        //离开副本
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            ClientProxy.LeaveLevel();
        else
            U3D.GoBack();
    }
}
