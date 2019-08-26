using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.GameState.DialogState;

//资料片详情页面.和资料片选择界面，点击预览图 展示资料片详情
public class DlcInfoDialogState:CommonDialogState<Dialog>
{
    public override string DialogName { get { return "DlcInfoDialog"; } }
    public DlcInfoDialogState(MainDialogStateManager stateMgr):base(stateMgr)
    {

    }
}

public class DlcInfoDialog : Dialog {
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {

    }
}
