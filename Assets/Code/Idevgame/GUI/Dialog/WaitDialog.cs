using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitDialog : Dialog {
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init(data as string);
    }

    public override void OnClose()
    {

    }

    void Init(string t)
    {
        Control("Title").GetComponent<Text>().text = t;
    }
}

public class WaitDialogState : CommonDialogState<WaitDialog>
{
    public override string DialogName { get { return "WaitDialog"; } }
    public WaitDialogState(MainDialogStateManager stateMgr) : base(stateMgr)
    {

    }
}