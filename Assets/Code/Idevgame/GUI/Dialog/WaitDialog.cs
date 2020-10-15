using Assets.Code.Idevgame.Common.Util;
using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitDialog : Dialog {
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data) {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init(data as string);
    }

    public override void OnClose()
    {

    }

    public void Update() {

    }

    void Init(string t)
    {
        Control("Title").GetComponent<Text>().text = t;
    }

    
}

public class WaitDialogState : PersistDialog<WaitDialog>
{
    public override string DialogName { get { return "WaitDialog"; } }
    public WaitDialogState()
    {

    }


}