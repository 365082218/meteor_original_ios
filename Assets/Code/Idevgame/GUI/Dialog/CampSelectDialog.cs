using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampSelectDialogState : CommonDialogState<CampSelectDialog>
{
    public override string DialogName { get { return "CampSelectDialog"; } }
    public CampSelectDialogState(MainDialogMgr stateMgr) : base(stateMgr)
    {

    }
}

public class CampSelectDialog : Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    void Init()
    {
        Image img = Control("Board").GetComponent<Image>();
        Utility.Expand(img, 256, 256);
        Control("MeteorBtn").GetComponent<Button>().onClick.AddListener(() => { SelectMeteor(); });
        Control("ButterflyBtn").GetComponent<Button>().onClick.AddListener(() => { SelectButterfly(); });
        Control("Meteor").GetComponent<Button>().onClick.AddListener(() => { SelectMeteor(); });
        Control("Butterfly").GetComponent<Button>().onClick.AddListener(() => { SelectButterfly(); });
    }

    void SelectMeteor() {
        NetWorkBattle.Ins.camp = (int)EUnitCamp.EUC_Meteor; Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoleSelectDialogState);
    }

    void SelectButterfly() {
        NetWorkBattle.Ins.camp = (int)EUnitCamp.EUC_Butterfly; Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoleSelectDialogState);
    }
}