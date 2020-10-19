using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;
using System;
using Idevgame.GameState;

public class EscConfirmDialogState:CommonDialogState<EscConfirmWnd>
{
    public override string DialogName{get{return "EscConfirmWnd";}}
    public EscConfirmDialogState(MainDialogMgr stateMgr):base(stateMgr)
    {

    }

    public override void OnAction(DialogAction dialogAction, object data)
    {
        base.OnAction(dialogAction, data);
        switch (dialogAction)
        {
            case DialogAction.Previous:
                ChangeState(previousS);
                break;
        }
        base.OnAction(dialogAction, data);
    }
}

//退出二次确认框
public class EscConfirmWnd : Dialog
{
    Button Leave;
    Button Continue;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    void Init()
    {
        Leave = Control("Leave").GetComponent<Button>();
        Leave.onClick.AddListener(OnLeave);
        Continue = Control("Continue").GetComponent<Button>();
        Continue.onClick.AddListener(OnContinue);
    }

    void OnLeave()
    {
        GameStateMgr.Ins.SaveState();
        Main.Ins.GameBattleEx.Pause();
        SoundManager.Ins.StopAll();
        Main.Ins.StopAllCoroutines();
        PathHelper.Ins.StopCalc();
        FightState.State.Close();
        MeteorManager.Ins.Reset();
        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        //离开副本
        if (U3D.IsMultiplyPlayer())
            TcpClientProxy.Ins.LeaveLevel();
        else {
            FrameReplay.Ins.OnDisconnected();
            U3D.GoBack();
        }
    }

    void OnContinue() {
        Main.Ins.GameBattleEx.Resume();
        OnBackPress();
    }
}
