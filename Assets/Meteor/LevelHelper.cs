using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

//for loadingui updateui
public interface LoadingUI
{
    void UpdateProgress(float percent);
}

public class LevelHelper : MonoBehaviour
{
	AsyncOperation mAsync;
    public struct LevelParam
    {
        public int id;
        public int gate;
    }

    public void Load(int id)
    {
        StartCoroutine(LoadLevelAsync(id));
    }

    IEnumerator LoadLevelAsync(int levelId)
    {
        int displayProgress = 0;
        int toProgress = 0;
        //WSLog.LogInfo("LoadLevelAsync");
        yield return 0;
        Level lev = LevelMng.Instance.GetItem(levelId);
        //WSLog.LogInfo("Load Scene Async:" + lev.Scene);
        ResMng.LoadScene(lev.Scene);
        mAsync = SceneManager.LoadSceneAsync(lev.Scene);
        mAsync.allowSceneActivation = false;
        while (mAsync.progress < 0.9f)
        {
            toProgress = (int)mAsync.progress * 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                if (LoadingWnd.Exist)
                    LoadingWnd.Instance.UpdateProgress(displayProgress / 100.0f);
                yield return 0;
            }
            yield return 0;
        }
        toProgress = 100;
        //WSLog.LogInfo("displayProgress < toProgress");
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            if (LoadingWnd.Exist)
                LoadingWnd.Instance.UpdateProgress(displayProgress / 100.0f);
            yield return 0;
        }
        //WSLog.LogInfo("displayProgress < toProgress");
        mAsync.allowSceneActivation = true;
        yield return 0;
        //WSLog.LogInfo("OnLoadFinishedEx");
        OnLoadFinishedEx(lev);
        for (int i = 0; i < 5; i++)
            yield return 0;
        if (LoadingWnd.Exist)
            LoadingWnd.Instance.Close();
        Destroy(this);
    }

    LevelScriptBase GetLevelScript(string sn)
    {
        Type type = Type.GetType(string.Format("LevelScript_{0}", sn));
        if (type == null)
            return null;
        Global.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    void OnLoadFinishedEx(Level lev)
    {
        SoundManager.Instance.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.FuBenID, lev.LevelScript));
            return;
        }

        Global.GScript = script;
        SceneMng.OnLoad();//
        //加载场景配置数据
        SceneMng.OnEnterLevel(script, lev.ID);//原版功能不加载其他存档数据.

        //设置主角属性
        U3D.InitPlayer(script);

        //把音频侦听移到角色
        Startup.ins.listener.enabled = false;
        Startup.ins.playerListener = MeteorManager.Instance.LocalPlayer.gameObject.AddComponent<AudioListener>();
       
        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        GameBattleEx.Instance.Init(lev, script);

        //先创建一个相机
        GameObject camera = GameObject.Instantiate(Resources.Load("CameraEx")) as GameObject;
        camera.name = "CameraEx";

        //角色摄像机跟随者着角色.
        CameraFollow followCamera = GameObject.Find("CameraEx").GetComponent<CameraFollow>();
        followCamera.Init();
        GameBattleEx.Instance.m_CameraControl = followCamera;
        //摄像机完毕后
        FightWnd.Instance.Open();
        if (!string.IsNullOrEmpty(lev.BgmName))
            SoundManager.Instance.PlayMusic(lev.BgmName);

        //除了主角的所有角色,开始输出,选择阵营, 进入战场
        for (int i = 0; i < MeteorManager.Instance.UnitInfos.Count; i++)
        {
            if (MeteorManager.Instance.UnitInfos[i] == MeteorManager.Instance.LocalPlayer)
                continue;
            MeteorUnit unitLog = MeteorManager.Instance.UnitInfos[i];
            U3D.InsertSystemMsg(GetCampStr(unitLog));
        }
    }

    public static string GetCampStr(MeteorUnit unit)
    {
        if (unit.Camp == EUnitCamp.EUC_ENEMY)
            return string.Format("{0} 选择蝴蝶, 进入战场", unit.name);
        if (unit.Camp == EUnitCamp.EUC_FRIEND)
            return string.Format("{0} 选择流星,进入战场", unit.name);
        return string.Format("{0} 进入战场", unit.name);
    }
}
