using UnityEngine;
using System.Collections;
using Idevgame.GameState.DialogState;
public class ConnectDialogState : CommonDialogState<Dialog>{
    public ConnectDialogState(MainDialogStateManager stateMgr) :base(stateMgr)
    {

    }
    public override string DialogName { get { return "ConnectWnd"; } }
}

public class ReconnectDialogState: CommonDialogState<Dialog>
{
    public ReconnectDialogState(MainDialogStateManager stateMgr) :base(stateMgr)
    {

    }
    public override string DialogName { get { return "ReConnectWnd"; } }
}

public class LoadingEXDialogState : CommonDialogState<Dialog>
{
    public LoadingEXDialogState(MainDialogStateManager stateMgr) :base(stateMgr)
    {

    }
    public override string DialogName { get { return "LoadingEX"; } }
}
