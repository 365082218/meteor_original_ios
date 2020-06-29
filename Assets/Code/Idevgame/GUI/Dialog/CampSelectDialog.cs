using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CampSelectDialogState : CommonDialogState<CampSelectDialog>
{
    public override string DialogName { get { return "CampSelectDialog"; } }
    public CampSelectDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
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
        Control("Meteor").GetComponent<Button>().onClick.AddListener(() => { Main.Ins.NetWorkBattle.camp = (int)EUnitCamp.EUC_Meteor; Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoleSelectDialogState); });
        Control("Butterfly").GetComponent<Button>().onClick.AddListener(() => { Main.Ins.NetWorkBattle.camp = (int)EUnitCamp.EUC_Butterfly; Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.RoleSelectDialogState); });
    }
}