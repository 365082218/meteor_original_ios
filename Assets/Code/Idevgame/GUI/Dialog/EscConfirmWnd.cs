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
    public EscConfirmDialogState(MainDialogStateManager stateMgr):base(stateMgr)
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
    [SerializeField]
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
        Continue.onClick.AddListener(OnBackPress);
    }

    void OnLeave()
    {
        Main.Instance.GameStateMgr.SaveState();
        Main.Instance.GameBattleEx.Pause();
        Main.Instance.StopAllCoroutines();
        Main.Instance.SoundManager.StopAll();
        Main.Instance.BuffMng.Clear();
        Main.Instance.MeteorManager.Clear();
        OnBackPress();
        Main.Instance.ExitState(Main.Instance.FightDialogState);
        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        //离开副本
        if (Main.Instance.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            UdpClientProxy.LeaveLevel();
        else
        {
            FrameReplay.Instance.OnDisconnected();
            U3D.GoBack();
        }
    }
}
