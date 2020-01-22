using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultDialogState : CommonDialogState<BattleResultDialog>
{
    public override string DialogName { get { return "BattleResultDialog"; } }
    public BattleResultDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

public class BattleResultDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    GameObject BattleResultAll;
    GameObject BattleResult;
    GameObject BattleTitle;
    Transform MeteorResult;
    Transform ButterflyResult;
    public void SetResult(int result)
    {
        if (result == 1)
        {
            for (int i = 0; i < Main.Instance.MeteorManager.UnitInfos.Count; i++)
            {
                if (Main.Instance.MeteorManager.UnitInfos[i].StateMachine != null)
                    Main.Instance.MeteorManager.UnitInfos[i].StateMachine.Stop();
                Main.Instance.MeteorManager.UnitInfos[i].controller.Input.ResetVector();
                Main.Instance.MeteorManager.UnitInfos[i].OnGameResult(result);
            }
        }

        if (Main.Instance.CombatData.GGameMode == GameMode.MENGZHU)
        {
            U3D.InsertSystemMsg("回合结束");
        }
        else
        {
            string mat = "";
            Text txt;
            switch (result)
            {
                case -1:
                case 0:
                    mat = "BattleLose";
                    txt = Control("ButterflyWin").GetComponent<Text>();
                    U3D.InsertSystemMsg("蝴蝶阵营 获胜");
                    txt.text = "1";
                    break;
                case 1:
                case 2:
                    mat = "BattleWin";
                    txt = Control("MeteorWin").GetComponent<Text>();
                    U3D.InsertSystemMsg("流星阵营 获胜");
                    txt.text = "1";
                    break;
                case 3:
                    mat = "BattleNone";
                    U3D.InsertSystemMsg("和局");
                    break;

            }
            BattleResult.GetComponent<Image>().material = Resources.Load<Material>(mat);
            BattleResult.SetActive(true);
            BattleTitle.SetActive(true);
        }
        Control("Close").SetActive(true);
        Control("Close").GetComponent<Button>().onClick.AddListener(() =>
        {
            Main.Instance.GameStateMgr.SaveState();
            Main.Instance.GameBattleEx.Pause();
            Main.Instance.StopAllCoroutines();
            Main.Instance.SoundManager.StopAll();
            Main.Instance.BuffMng.Clear();
            Main.Instance.MeteorManager.Clear();
            OnBackPress();
            Main.Instance.ExitState(Main.Instance.FightDialogState);
            if (GameOverlayDialogState.Exist())
                GameOverlayDialogState.Instance.ClearSystemMsg();
            //离开副本
            if (Main.Instance.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
                UdpClientProxy.LeaveLevel();
            else
            {
                FrameReplay.Instance.OnDisconnected();
                Main.Instance.PlayEndMovie(result == 1);
            }
        });
    }

    public void Init()
    {
#if !STRIP_DBG_SETTING
        U3D.Instance.CloseDbg();
#endif
        MeteorResult = Control("MeteorResult").transform;
        ButterflyResult = Control("ButterflyResult").transform;
        BattleResult = NodeHelper.Find("BattleResult", WndObject);
        BattleTitle = NodeHelper.Find("BattleTitle", WndObject);
        Control("Close").SetActive(false);
        BattleResultAll = NodeHelper.Find("AllResult", WndObject);
        bool active = Main.Instance.CombatData.GGameMode != GameMode.MENGZHU;
        bool active2 = Main.Instance.CombatData.GGameMode == GameMode.MENGZHU;
        Control("CampImage", WndObject).SetActive(active);
        Control("Title", WndObject).SetActive(active);
        Control("Result", WndObject).SetActive(active);
        Control("CampImage1", WndObject).SetActive(active);
        Control("Title1", WndObject).SetActive(active);
        Control("Result1", WndObject).SetActive(active);
        Control("CampImageAll", WndObject).SetActive(active2);
        Control("TitleAll", WndObject).SetActive(active2);
        Control("ResultAll", WndObject).SetActive(active2);

        for (int i = 0; i < Main.Instance.MeteorManager.UnitInfos.Count; i++)
        {
            if (Main.Instance.GameBattleEx.BattleResult.ContainsKey(Main.Instance.MeteorManager.UnitInfos[i].InstanceId))
            {
                InsertPlayerResult(Main.Instance.MeteorManager.UnitInfos[i].InstanceId, Main.Instance.GameBattleEx.BattleResult[Main.Instance.MeteorManager.UnitInfos[i].InstanceId]);
                Main.Instance.GameBattleEx.BattleResult.Remove(Main.Instance.MeteorManager.UnitInfos[i].InstanceId);
            }
            else
                InsertPlayerResult(Main.Instance.MeteorManager.UnitInfos[i].InstanceId, Main.Instance.MeteorManager.UnitInfos[i].InstanceId, 0, 0, Main.Instance.MeteorManager.UnitInfos[i].Camp);
        }

        foreach (var each in Main.Instance.GameBattleEx.BattleResult)
            InsertPlayerResult(each.Key, each.Value);
        Main.Instance.GameBattleEx.BattleResult.Clear();
    }

    void InsertPlayerResult(int instanceId, int id, int killed, int dead, EUnitCamp camp)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
        if (Main.Instance.CombatData.GGameMode == GameMode.MENGZHU)
        {
            obj.transform.SetParent(BattleResultAll.transform);
        }
        else
            obj.transform.SetParent(camp == EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
        //obj.transform.SetParent(camp ==  EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
        obj.layer = MeteorResult.gameObject.layer;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        Text Idx = Control("Idx", obj).GetComponent<Text>();
        Text Name = Control("Name", obj).GetComponent<Text>();
        if (Main.Instance.CombatData.GGameMode == GameMode.MENGZHU)
        {

        }
        else
        {
            Text Camp = Control("Camp", obj).GetComponent<Text>();
            Camp.text = U3D.GetCampStr(camp);
        }
        //Text Camp = ldaControl("Camp", obj).GetComponent<Text>();
        Text Killed = Control("Killed", obj).GetComponent<Text>();
        Text Dead = Control("Dead", obj).GetComponent<Text>();
        Idx.text = (id + 1).ToString();
        Name.text = U3D.GetUnit(instanceId).Name;
        //Camp.text = result.camp == 1 ""
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
        if (Main.Instance.CombatData.GGameMode == GameMode.MENGZHU)
        {
            obj.transform.SetParent(BattleResultAll.transform);
        }
        else
            obj.transform.SetParent(result.camp == (int)EUnitCamp.EUC_FRIEND ? MeteorResult : ButterflyResult);
        obj.layer = MeteorResult.gameObject.layer;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = Vector3.zero;

        Text Idx = Control("Idx", obj).GetComponent<Text>();
        Text Name = Control("Name", obj).GetComponent<Text>();
        Text Killed = Control("Killed", obj).GetComponent<Text>();
        Text Dead = Control("Dead", obj).GetComponent<Text>();
        Idx.text = (result.id + 1).ToString();
        Name.text = U3D.GetUnit(instanceId).Name;
        if (Main.Instance.CombatData.GGameMode == GameMode.MENGZHU)
        {

        }
        else
        {
            Text Camp = Control("Camp", obj).GetComponent<Text>();
            Camp.text = U3D.GetCampStr((EUnitCamp)result.camp);
        }
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