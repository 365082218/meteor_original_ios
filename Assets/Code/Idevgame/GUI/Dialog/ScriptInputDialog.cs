using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Idevgame.GameState.DialogState;

public class ScriptInputDialogState : CommonDialogState<ScriptInputDialog>
{
    public override string DialogName
    {
        get { return "ScriptInputWnd"; }
    }

    public ScriptInputDialogState(MainDialogStateManager stateMgr):base(stateMgr)
    {

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
        if (Main.Ins.LocalPlayer != null) {
            inputLocked = Main.Ins.LocalPlayer.meteorController.InputLocked;
            Main.Ins.LocalPlayer.meteorController.LockInput(inputLocked);
        }
    }

    bool inputLocked;
    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        if (Main.Ins.LocalPlayer != null) {
            inputLocked = Main.Ins.LocalPlayer.meteorController.InputLocked;
            Main.Ins.LocalPlayer.meteorController.LockInput(true);
        }
        Close.onClick.AddListener(() => { OnPreviousPress(); });
        DoScript.onClick.AddListener(() =>
        {
            if (Main.Ins.CombatData.GLevelMode == LevelMode.MultiplyPlayer)
            {
                result.text = "联机禁用此功能";
                return;
            }
            try
            {
                if (string.IsNullOrEmpty(scriptInput.text))
                {
                    result.text = "输入为空";
                    return;
                }

                if (!CheatCode.UseCheatCode(scriptInput.text))
                    Main.Ins.ScriptMng.CallString(scriptInput.text);
                result.text = "秘籍成功执行";
            }
            catch (Exception exp)
            {
                result.text = "执行出错:" + exp.Message + "-" + exp.StackTrace;
            }
        });
    }
}
