using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingEx : Dialog {

}

public class LoadingEXDialogState : CommonDialogState<LoadingEx>
{
    public LoadingEXDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
    public override string DialogName { get { return "LoadingEX"; } }
}
