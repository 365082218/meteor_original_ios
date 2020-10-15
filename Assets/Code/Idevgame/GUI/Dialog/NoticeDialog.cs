using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//公告面板.
public class NoticeDialogState : CommonDialogState<NoticeDialog>
{
    public override string DialogName { get { return "NoticeDialog"; } }
    public NoticeDialogState(BaseDialogStateManager stateMgr) : base(stateMgr)
    {
        read = UserPref.Ins.GetInt("AsRead", 0);
    }

    public override void OnEnter(BaseDialogState previousState, object data) {
        base.OnEnter(previousState, data);
        UserPref.Ins.SetInt("AsRead", 1);
        read = 1;
    }

    //如果拉取到游戏公告了，且未显示，则显示
    int read = 0;
    public override bool CanOpen()
    {
        if (read == 1)
            return false;
        if (Main.Ins.GameNotice.HaveNotice())
            return true;
        return false;
    }

    public override bool AutoClear()
    {
        return true;
    }
}

public class NoticeDialog : Dialog
{
    [SerializeField]
    Text Notice;
    [SerializeField]
    Button Close;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Notice.text = Main.Ins.GameNotice.notice;
        Close.onClick.AddListener(() => { OnBackPress(); });
    }
}