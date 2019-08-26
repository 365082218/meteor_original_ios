using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LoginDialogState : CommonDialogState<LoginDialog>
{
    public override string DialogName { get { return "LoginDialog"; } }
    public LoginDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}

public class LoginDialog : Dialog
{

}