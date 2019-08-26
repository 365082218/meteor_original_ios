using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingEx : Dialog {

}

public class LoadingEXDialogState : PersistDialog<LoadingEx>
{
    public LoadingEXDialogState()
    {

    }
    public override string DialogName { get { return "LoadingEX"; } }
}
