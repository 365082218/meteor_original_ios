using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.GameState.DialogState;
/// <summary>
/// 最开始进入游戏时，出现开屏图，以及加载进度条
/// </summary>
public class StartupDialogState:CommonDialogState<StartupUiController> {
    StartupUiController controller;
    public override string DialogName { get { return "StartupWnd"; }}
    public StartupDialogState(MainDialogMgr stateManager) : base(stateManager) {

    }

    public override void OnAction(DialogAction dialogAction, object data)
    {
        switch (dialogAction)
        {
            default:
                break;
        }
    }
}
