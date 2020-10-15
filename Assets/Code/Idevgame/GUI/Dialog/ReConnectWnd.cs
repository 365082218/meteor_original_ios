using UnityEngine;
using System.Collections;
using Idevgame.GameState.DialogState;

public class ReconnectDialogState: PersistDialog<ReConnectWnd>
{
    public override string DialogName { get { return "ReConnectWnd"; } }
}

public class ReConnectWnd:Dialog
{

}