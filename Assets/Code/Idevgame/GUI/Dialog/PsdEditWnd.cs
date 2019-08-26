using Idevgame.GameState.DialogState;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PsdEditDialogState : PersistDialog<PsdEditWnd>
{
    public override string DialogName { get { return "PsdEditDialog"; } }
    public PsdEditDialogState()
    {

    }
}

public class PsdEditWnd : Dialog
{
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    public Action OnConfirm;
    void Init()
    {
        Control("Confirm").GetComponent<Button>().onClick.AddListener(() => { if (OnConfirm != null) OnConfirm(); });
        Control("Cancel").GetComponent<Button>().onClick.AddListener(() => { OnBackPress(); });
    }
}