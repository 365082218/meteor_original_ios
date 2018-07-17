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

    public void Load(int id, LoadingUI loading)
    {
        StartCoroutine(LoadLevelAsync(id, loading));
    }

    IEnumerator LoadLevelAsync(int levelId, LoadingUI loading)
    {
        int displayProgress = 0;
        int toProgress = 0;
        WSLog.LogInfo("LoadLevelAsync");
        yield return 0;
        Level lev = LevelMng.Instance.GetItem(levelId);
        WSLog.LogInfo("Load Scene Async:" + lev.Scene);
        ResMng.LoadScene(lev.Scene);
        mAsync = SceneManager.LoadSceneAsync(lev.Scene);
        mAsync.allowSceneActivation = false;
        while (mAsync.progress < 0.9f)
        {
            toProgress = (int)mAsync.progress * 100;
            //WSLog.LogInfo("while (mAsync.progress < 0.9f)");
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                if (loading != null)
                    loading.UpdateProgress(displayProgress / 100.0f);
                yield return 0;
            }
            //WSLog.LogInfo("while (displayProgress < toProgress) end");
            yield return 0;
        }
        toProgress = 100;
        WSLog.LogInfo("displayProgress < toProgress");
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            if (loading != null)
                loading.UpdateProgress(displayProgress / 100.0f);
            yield return 0;
        }
        WSLog.LogInfo("displayProgress < toProgress");
        mAsync.allowSceneActivation = true;
        yield return 0;
        WSLog.LogInfo("OnLoadFinishedEx");
        OnLoadFinishedEx(lev);
        for (int i = 0; i < 5; i++)
            yield return 0;
        LoadingWnd.Instance.Close();
        Destroy(this);
    }

    LevelScriptBase GetLevelScript(string sn)
    {
        Type type = Type.GetType("LevelScript_" + sn);
        if (type == null)
            return null;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    void OnLoadFinishedEx(Level lev)
    {
        
        SoundManager.Instance.Enable(true);

        LevelScriptBase script = GetLevelScript(lev.goodList);

        if (script == null)
        {
            //一些场景是测试场景，不能战斗，不创建角色等.
            
            return;
        }

        SceneMng.OnLoad();//
        //加载场景配置数据
        SceneMng.OnEnterLevel(lev.ID);//原版功能不加载其他存档数据.

        //设置主角属性
        U3D.InitPlayer(script); ;

        //把音频侦听移到角色
        AudioListener listen = Startup.ins.gameObject.GetComponent<AudioListener>();
        if (listen != null)
            DestroyImmediate(listen);
        MeteorManager.Instance.LocalPlayer.gameObject.AddComponent<AudioListener>();
        
        //这个脚本暂时未调通，先放着
        //ScriptMng.ins.CallScript(lev.goodList);//负责关卡内物件的属性，及攻击收击逻辑等

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
            return unit.name + " 选择蝴蝶,进入战场";
        if (unit.Camp == EUnitCamp.EUC_FRIEND)
            return unit.name + " 选择流星,进入战场";
        return unit.name + " 进入战场";
    }
}
