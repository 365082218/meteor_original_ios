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
        Leave.onClick.AddListener(OnLeave);
        Continue.onClick.AddListener(OnBackPress);
    }

    void OnLeave()
    {
        GameData.Instance.SaveState();
        GameBattleEx.Instance.Pause();
        Main.Instance.StopAllCoroutines();
        SoundManager.Instance.StopAll();
        BuffMng.Instance.Clear();
        MeteorManager.Instance.Clear();
        OnBackPress();
        Main.Instance.PersistState.ExitState(Main.Instance.PersistState.FightDialog);
        if (GameOverlayDialogState.Exist())
            GameOverlayDialogState.Instance.ClearSystemMsg();
        //离开副本
        if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            UdpClientProxy.LeaveLevel();
        else
        {
            FrameReplay.Instance.OnDisconnected();
            U3D.GoBack();
        }
    }
}
