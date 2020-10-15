using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

public class ScriptInputDialogState : PersistDialog<ScriptInputDialog>
{
    public override string DialogName
    {
        get { return "ScriptInputWnd"; }
    }
}

public class ScriptInputDialog:Dialog
{
    [SerializeField]
    InputField scriptInput;
    [SerializeField]
    Text result;
    [SerializeField]
    Button Close;
    [SerializeField]
    Button DoScript;
    public override void OnDialogStateExit() {
        base.OnDialogStateExit();
        if (Main.Ins.GameBattleEx == null) {
            if (SettingDialogState.Exist) {
                Main.Ins.DialogStateManager.ChangeState(null);
                Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.SettingDialogState);
            }
        }
    }

    bool inputLocked;
    public override void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Close.onClick.AddListener(() => { OnPreviousPress(); });
        DoScript.onClick.AddListener(() =>
        {
            try
            {
                if (string.IsNullOrEmpty(scriptInput.text))
                {
                    result.text = "输入为空";
                    return;
                }
                result.text = "秘籍可查阅指令表";
                if (!CheatCode.UseCheatCode(scriptInput.text)) {
                    if (U3D.IsMultiplyPlayer())
                        result.text = "联机时只支持部分指令";
                    else
                        ScriptMng.Ins.CallString(scriptInput.text);
                } else {
                    result.text = "秘籍成功执行";
                }
                
            }
            catch (Exception exp)
            {
                result.text = "执行出错:" + exp.Message + "-" + exp.StackTrace;
            }
        });
    }
}
