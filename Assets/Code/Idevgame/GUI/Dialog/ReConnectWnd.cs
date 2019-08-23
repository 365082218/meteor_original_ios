using UnityEngine;
using System.Collections;
using Idevgame.GameState.DialogState;

public class ReconnectDialogState: CommonDialogState<ReConnectWnd>
{
    public ReconnectDialogState(MainDialogStateManager stateMgr) :base(stateMgr)
    {

    }
    public override string DialogName { get { return "ReConnectWnd"; } }
}

public class ReConnectWnd:Dialog
{

}