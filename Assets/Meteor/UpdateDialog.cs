using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using Idevgame.GameState.DialogState;
//更新提示
public class UpdateDialogState:CommonDialogState<UpdateDialog>
{
    public override string DialogName { get { return "UpdateNotice"; } }
    public UpdateDialogState(MainDialogMgr stateMgr):base(stateMgr)
    {

    }
}

public class UpdateDialog: Dialog
{
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);

    }

    [SerializeField]
    Text Notice;
    [SerializeField]
    Button Accept;
    [SerializeField]
    Button Cancel;
    [SerializeField]
    UILoadingBar LoadingBar;
    [SerializeField]
    Text SpeedText;

    public void SetNotice(string text, UnityAction onaccept, UnityAction oncancel)
    {
        Notice.text = text;
        Accept.onClick.AddListener(onaccept);
        Cancel.onClick.AddListener(oncancel);
    }

    public void UpdateProgress(float percent, string speedstr)
    {
        if (LoadingBar != null)
            LoadingBar.SetProgress(percent);
        if (SpeedText != null)
            SpeedText.text = speedstr;
    }

    public void DisableAcceptBtn()
    {
        Accept.interactable = false;
    }
}