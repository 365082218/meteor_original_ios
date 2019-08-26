using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStatusDialogState : Idevgame.GameState.DialogState.PersistDialog<BattleStatusDialog>
{
    public override string DialogName { get { return "BattleStatusDialog"; } }
    public BattleStatusDialogState()
    {

    }
}

public class BattleStatusDialog : Dialog
{
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    GameObject BattleResult;
    //GameObject BattleTitle;
    Transform MeteorResult;
    Transform ButterflyResult;
    Dictionary<int, BattleResultItem> battleResult = new Dictionary<int, BattleResultItem>();
    public void Init()
    {
        //拷贝一份对战数据
        battleResult.Clear();
        foreach (var each in GameBattleEx.Instance.BattleResult)
        {
            battleResult.Add(each.Key, each.Value);
        }
        MeteorResult = Control("MeteorResult").transform;
        ButterflyResult = Control("ButterflyResult").transform;
        BattleResult = Global.ldaControlX("AllResult", WndObject);
        Control("CampImage", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
        Control("Title", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
        Control("Result", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
        Control("CampImage1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
        Control("Title1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
        Control("Result1", WndObject).SetActive(Global.Instance.GGameMode != GameMode.MENGZHU);
        Control("CampImageAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
        Control("TitleAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
        Control("ResultAll", WndObject).SetActive(Global.Instance.GGameMode == GameMode.MENGZHU);
        //BattleTitle = Global.ldaControlX("BattleTitle", WndObject);
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (battleResult.ContainsKey(MeteorManager.Instance.UnitInfos[i].InstanceId))
            {
                InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, battleResult[MeteorManager.Instance.UnitInfos[i].InstanceId]);
                battleResult.Remove(MeteorManager.Instance.UnitInfos[i].InstanceId);
            }
            else
                InsertPlayerResult(MeteorManager.Instance.UnitInfos[i].InstanceId, MeteorManager.Instance.UnitInfos[i].InstanceId, 0, 0, MeteorManager.Instance.UnitInfos[i].Camp);
        }

        foreach (var each in battleResult)
            InsertPlayerResult(each.Key, each.Value);
    }

    void InsertPlayerResult(int instance, int id, int killed, int dead, EUnitCamp camp)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
        if (Global.Instance.GGameMode == GameMode.MENGZHU)
        {
            obj.transform.SetParent(BattleResult.transform);
        }
        else
            obj.transform.SetParent(camp == EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
        obj.layer = MeteorResult.gameObject.layer;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        Text Idx = Control("Idx", obj).GetComponent<Text>();
        Text Name = Control("Name", obj).GetComponent<Text>();
        if (Global.Instance.GGameMode == GameMode.MENGZHU)
        {

        }
        else
        {
            Text Camp = Control("Camp", obj).GetComponent<Text>();
            Camp.text = U3D.GetCampStr(camp);
        }

        Text Killed = Control("Killed", obj).GetComponent<Text>();
        Text Dead = Control("Dead", obj).GetComponent<Text>();
        Idx.text = (id + 1).ToString();
        Name.text = U3D.GetUnit(instance).Name;

        Killed.text = killed.ToString();
        Dead.text = dead.ToString();
        MeteorUnit u = U3D.GetUnit(id);
        if (u != null)
        {
            if (u.Dead)
            {
                Idx.color = Color.red;
                Name.color = Color.red;
                Killed.color = Color.red;
                Dead.color = Color.red;
            }
        }
        else
        {
            //得不到信息了。说明该NPC被移除掉了
            Idx.color = Color.red;
            Name.color = Color.red;
            Killed.color = Color.red;
            Dead.color = Color.red;
        }
    }

    void InsertPlayerResult(int instanceId, BattleResultItem result)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
        if (Global.Instance.GGameMode == GameMode.MENGZHU)
        {
            obj.transform.SetParent(BattleResult.transform);
        }
        else
            obj.transform.SetParent(result.camp == (int)EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
        obj.layer = MeteorResult.gameObject.layer;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        Text Idx = Control("Idx", obj).GetComponent<Text>();
        Text Name = Control("Name", obj).GetComponent<Text>();
        if (Global.Instance.GGameMode == GameMode.MENGZHU)
        {

        }
        else
        {
            Text Camp = Control("Camp", obj).GetComponent<Text>();
            Camp.text = U3D.GetCampStr((EUnitCamp)result.camp);
        }
        Text Killed = Control("Killed", obj).GetComponent<Text>();
        Text Dead = Control("Dead", obj).GetComponent<Text>();
        Idx.text = (result.id + 1).ToString();
        Name.text = U3D.GetUnit(instanceId).Name;
        //Camp.text = result.camp == 1 ""
        Killed.text = result.killCount.ToString();
        Dead.text = result.deadCount.ToString();
        MeteorUnit u = U3D.GetUnit(result.id);
        if (u != null)
        {
            if (u.Dead)
            {
                Idx.color = Color.red;
                Name.color = Color.red;
                Killed.color = Color.red;
                Dead.color = Color.red;
            }
        }
    }
}
