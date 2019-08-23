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

    public override void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        base.OnDialogStateEnter(ownerState, previousDialog, data);
        Close.onClick.AddListener(() => { OnBackPress(); });
        DoScript.onClick.AddListener(() =>
        {
            if (Global.Instance.GLevelMode == LevelMode.MultiplyPlayer)
            {
                result.text = "联机禁用此功能";
                return;
            }
            try
            {
                if (!UseCheatCode(scriptInput.text))
                    ScriptMng.Instance.CallString(scriptInput.text);
                else
                {
                    OnBackPress();
                    return;
                }
                result.text = "秘籍成功执行";
            }
            catch (Exception exp)
            {
                result.text = "执行出错:" + exp.Message + "-" + exp.StackTrace;
            }
        });
    }

    //使用作弊码
    bool UseCheatCode(string cheatcode)
    {
        bool ret = false;
        if (CheatOK(cheatcode, "god"))
        {
            U3D.GodLike();
            ret = true;
        }
        return ret;
    }

    bool CheatOK(string cheat, string code)
    {
        if (cheat.ToUpper() == code || cheat.ToLower() == code)
            return true;
        return false;
    }
}
