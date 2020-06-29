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

    }

    //如果拉取到游戏公告了，且未显示，则显示
    bool read = false;
    public override bool CanOpen()
    {
        if (read)
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