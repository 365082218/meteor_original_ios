using UnityEngine;
using System;
using SLua;
using System.Collections.Generic;
using System.IO;


//解决slua脚本如何与内部通信的问题,每个脚本都是一个txt文件.
public class ScriptMng:MonoBehaviour
{
	LuaSvr svr;
    public static ScriptMng ins = null;
    LuaFunction save;
    void Awake()
    {
        ins = this;
    }

	public void Start()
	{
        ResetStatus();
	}

	public void Update()
	{
		
	}
    
    public object GetVariable(string iden)
    {
        return LuaSvr.mainState[iden];
    }

    public LuaFunction GetFunc(string iden)
    {
        return LuaSvr.mainState.getFunction(iden);
    }

    //重加载一个脚本文件，变量的值会全部恢复.
	public void CallScript(string script)
	{
        try
        {
            if (!string.IsNullOrEmpty(script))
            {
                string p = string.Format("{0}/{1}", Application.persistentDataPath, script);
                if (File.Exists(p))
                    svr.start(p);
                else
                    svr.start(script);
            }
            else
                Debug.Log("script file is null or empty");
        }
        catch (Exception exp)
        {
            Debug.LogError(string.Format("{0}:{1}", exp.Message, exp.StackTrace));
        }
	}

	public void CallString(string scriptText)
	{
        if (!string.IsNullOrEmpty(scriptText))
        {
            object ret = LuaSvr.mainState.doString(scriptText);
            if (ret == null)
                throw new Exception("DoScript Failed");
        }
        else
            throw new Exception("script content is null or empty");
	}

    //只是设置变量，不调用函数
    public void DoScript(string script)
    {
        LuaSvr.mainState.doFile(script);
    }

    public void Save()
    {
        if (save != null)
            save.call();
    }

    public void Load(int index)
    {
        //恢复脚本变量状态
        string p = string.Format("{0}/{1}/script_status.txt", Application.persistentDataPath, index);
        if (File.Exists(p))
            CallScript(p);
    }

    public void ResetStatus()
    {
        svr = new LuaSvr();
        svr.init(null, () =>
        {
            CallScript("Main");
            save = LuaSvr.mainState.getFunction("save");
        });
    }
}

