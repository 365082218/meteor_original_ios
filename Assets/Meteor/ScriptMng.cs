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
        svr = new LuaSvr();
		svr.init (null, ()=>
        {
            CallScript("Main");
            save = LuaSvr.mainState.getFunction("save");
            //Startup.ins.GameStart();
        });
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
                if (File.Exists(Application.persistentDataPath + "/" + script))
                    svr.start(Application.persistentDataPath + "/" + script);
                else
                    svr.start(script);
            }
            else
                Debug.Log("script file is null or empty");
        }
        catch (Exception exp)
        {
            Debug.LogError(exp.Message + ":" + exp.StackTrace);
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
    //返回，指示场景物体不要在调用此函数，因为脚本里没有相应处理
    public bool OnAttack(SceneItemAgent sceneObj, int characterid, int damage)
    {
        if (sceneObj != null)
        {
            LuaFunction func = LuaSvr.mainState.getFunction(sceneObj.name + "_OnAttack");
            if (func != null)
            {
                func.call(sceneObj.InstanceId, characterid, damage);
                return true;
            }
            else
            {
                Debug.LogError("can not find function:" + sceneObj.name + "_OnIdle");
                return false;
            }
        }
        return true;
    }

    //同上
    public bool OnIdle(SceneItemAgent sceneObj)
    {
        if (sceneObj != null)
        {
            LuaFunction func = LuaSvr.mainState.getFunction(sceneObj.name + "_OnIdle");
            if (func != null)
            {
                func.call(sceneObj.InstanceId);
                return true;
            }
            else
            {
                Debug.LogError("can not find function:" + sceneObj.name + "_OnIdle");
                return false;
            }
        }
        return true;
    }

    public void Save()
    {
        if (save != null)
            save.call();
    }

    public void Load(int index)
    {
        //恢复脚本变量状态
        if (File.Exists(Application.persistentDataPath + "/" + index + "/script_status.txt"))
            CallScript(Application.persistentDataPath + "/" + index + "/script_status.txt");
    }
}

