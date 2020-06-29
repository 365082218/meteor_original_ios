using Idevgame.GameState.DialogState;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogUtils:Singleton<DialogUtils> {
    public void Init()
    {
        WaitDialogState = new WaitDialogState(null);
    }

    WaitDialogState WaitDialogState;
    public void OpenWait(string title)
    {
        WaitDialogState.OnEnter(null, title);
    }

    public void CloseWait()
    {
        WaitDialogState.OnExit();
    }

    public static CommonDialogState<T> OpenDialog<T>(string prefab, T dialog) where T:Dialog
    {
        CommonDialogState<T> dialogState = Activator.CreateInstance <CommonDialogState<T>> ();
        dialogState.OnEnter(null, null);
        return dialogState;
    }

    public static void OpenDialogState()
    {

    }
}
