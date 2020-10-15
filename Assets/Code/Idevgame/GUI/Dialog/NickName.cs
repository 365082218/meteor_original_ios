using Idevgame.GameState.DialogState;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NickNameDialogState : PersistDialog<NickName>
{
    public override string DialogName { get { return "NickName"; } }
}

public class NickName : Dialog
{
    public InputField Nick;
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Init();
    }

    void Init()
    {
        Control("Yes").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnApply();
        });
        Control("Cancel").GetComponent<Button>().onClick.AddListener(() =>
        {
            OnBackPress();
        });
        Nick = Control("Nick").GetComponent<InputField>();
        if (string.IsNullOrEmpty(GameStateMgr.Ins.gameStatus.NickName))
            Nick.text = "昱泉杀手";
        else
            Nick.text = GameStateMgr.Ins.gameStatus.NickName;
    }

    void OnApply()
    {
        if (!string.IsNullOrEmpty(Nick.text))
        {
            GameStateMgr.Ins.gameStatus.NickName = Nick.text;
            if (SettingDialogState.Exist)
                SettingDialogState.Instance.OnRefresh(0, null);
            OnBackPress();
        }
        else
            U3D.PopupTip("昵称不能为空");

    }
}
