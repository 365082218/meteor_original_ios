using Assets.Code.Idevgame.Common.Util;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BattleResultDialogState : CommonDialogState<BattleResultDialog>
{
    public override string DialogName { get { return "BattleResultDialog"; } }
    public BattleResultDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class BattleResultDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
        if (ScriptInputDialogState.Exist()) {
            ScriptInputDialogState.State.Close();
        }
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
            for (int i = 0; i < MeteorManager.Ins.UnitInfos.Count; i++)
            {
                if (MeteorManager.Ins.UnitInfos[i].StateMachine != null)
                    MeteorManager.Ins.UnitInfos[i].StateMachine.Stop();
                MeteorManager.Ins.UnitInfos[i].meteorController.Input.ResetVector();
                MeteorManager.Ins.UnitInfos[i].OnGameResult(result);
            }
        }

        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
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
        Control("Close").SetActive(CombatData.Ins.GLevelMode != LevelMode.MultiplyPlayer);
        if (CombatData.Ins.GLevelMode == LevelMode.MultiplyPlayer) {
            Timer t = Timer.once(3.0f, ReEnterLevel);
        }
        Control("Close").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameStateMgr.Ins.SaveState();
            Main.Ins.GameBattleEx.Pause();
            Main.Ins.StopAllCoroutines();
            SoundManager.Ins.StopAll();
            OnBackPress();
            if (FightState.Exist())
                FightState.State.Close();
            if (GameOverlayDialogState.Exist())
                GameOverlayDialogState.Instance.ClearSystemMsg();
            //离开副本
            if (U3D.IsMultiplyPlayer()) {

            } else {
                FrameReplay.Ins.OnDisconnected();
                Main.Ins.GotoMenu();
            }
        });
        //Control("SaveRecord").SetActive(false);
        //Control("SaveRecord").GetComponent<Button>().onClick.AddListener(() => {
        //    Main.Ins.EnterState(Main.Ins.WaitDialogState, "正在保存录像，请稍后");
        //    //单独开一个线程去保存录像信息.
        //    RecordMgr.Ins.WriteFile();
        //    Control("SaveRecord").SetActive(false);//隐藏掉该按钮
        //});
    }

    //联机出了结算之后，3S内展示 A=>阵营选择 B=》角色选择
    private void ReEnterLevel() {
        MeteorManager.Ins.Reset();
        SceneMng.Ins.Reset();
        FrameReplay.Ins.OnBattleFinished();
        //等一定时间，返回到角色选择/阵营选择界面，开始下一轮的游戏.
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoleSelectDialogState);
        else
            Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.CampSelectDialogState);
    }

    public void Init()
    {
        MeteorResult = Control("MeteorResult").transform;
        ButterflyResult = Control("ButterflyResult").transform;
        BattleResult = NodeHelper.Find("BattleResult", WndObject);
        BattleTitle = NodeHelper.Find("BattleTitle", WndObject);
        Control("Close").SetActive(false);
        Control("SaveRecord").SetActive(false);
        BattleResultAll = NodeHelper.Find("AllResult", WndObject);
        bool active = CombatData.Ins.GGameMode != GameMode.MENGZHU;
        bool active2 = CombatData.Ins.GGameMode == GameMode.MENGZHU;
        Control("CampImage", WndObject).SetActive(active);
        Control("Title", WndObject).SetActive(active);
        Control("Result", WndObject).SetActive(active);
        Control("CampImage1", WndObject).SetActive(active);
        Control("Title1", WndObject).SetActive(active);
        Control("Result1", WndObject).SetActive(active);
        Control("CampImageAll", WndObject).SetActive(active2);
        Control("TitleAll", WndObject).SetActive(active2);
        Control("ResultAll", WndObject).SetActive(active2);
        GameBattleEx battle = Main.Ins.GameBattleEx;
        List<MeteorUnit> Units = MeteorManager.Ins.UnitInfos;
        for (int i = 0; i < Units.Count; i++)
        {
            MeteorUnit unit = Units[i];
            if (unit == null)
                continue;
            if (battle.BattleResult.ContainsKey(unit.InstanceId))
            {
                InsertPlayerResult(unit.InstanceId, battle.BattleResult[unit.InstanceId]);
                battle.BattleResult.Remove(unit.InstanceId);
            }
            else
                InsertPlayerResult(unit.InstanceId, unit.InstanceId, 0, 0, unit.Camp);
        }

        foreach (var each in battle.BattleResult)
            InsertPlayerResult(each.Key, each.Value);
        battle.BattleResult.Clear();
    }

    void InsertPlayerResult(int instanceId, int id, int killed, int dead, EUnitCamp camp)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
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
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
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
        MeteorUnit unit = U3D.GetUnit(instanceId);
        Name.text = unit == null ? "[无名氏]" : unit.name;
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

    void InsertPlayerResult(int instanceId, ResultItem result)
    {
        GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>("ResultItem"));
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
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
        MeteorUnit unit = U3D.GetUnit(instanceId); 
        Name.text = unit == null ? "[无名氏]": unit.name;
        if (CombatData.Ins.GGameMode == GameMode.MENGZHU)
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
        } else {
            //得不到信息了。说明该NPC被移除掉了
            Idx.color = Color.red;
            Name.color = Color.red;
            Killed.color = Color.red;
            Dead.color = Color.red;
        }
    }
}