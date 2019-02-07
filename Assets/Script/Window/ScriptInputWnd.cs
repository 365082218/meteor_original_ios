using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ScriptInputWnd:Window<ScriptInputWnd>
{
    InputField scriptInput;

    Text result;
    protected override bool OnOpen()
    {
        scriptInput = Control("InputField").GetComponent<InputField>();
        Control("Close").GetComponent<Button>().onClick.AddListener(() => { Close(); });
        Control("DoScript").GetComponent<Button>().onClick.AddListener(() => {
            try
            {
                if (!UseCheatCode(scriptInput.text))
                    ScriptMng.ins.CallString(scriptInput.text);
                result.text = "秘籍成功执行";
            }
            catch (Exception exp)
            {
                result.text = "执行出错:" + exp.Message + "-" + exp.StackTrace;
            }
        });
        result = Control("Result").GetComponent<Text>();
        return true;
    }

    //使用作弊码
    bool UseCheatCode(string cheatcode)
    {
        bool ret = false;
        if (CheatOK(cheatcode, "god"))
        {
            U3D.GodLike();
            if (NewSystemWnd.Exist)
                NewSystemWnd.Instance.Close();
            NewSystemWnd.Instance.Open();
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
