using UnityEngine;
using System.Collections;
using Idevgame.GameState.DialogState;
using DG.Tweening;
public class ConnectDialogState : CommonDialogState<ConnectWnd>
{
    public ConnectDialogState(MainDialogMgr stateMgr) :base(stateMgr)
    {

    }
    public override string DialogName { get { return "ConnectWnd"; } }
}

public class ConnectWnd:Dialog
{
    //希望有一个入场动画，缩放0-1
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
    }
}

public class ConnectServerDialogState :PersistDialog<ConnectServerWnd> {
    public override string DialogName { get { return "ConnectServer"; } }
}

public class ConnectServerWnd : Dialog {
    //希望有一个入场动画，缩放0-1
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
    }
}