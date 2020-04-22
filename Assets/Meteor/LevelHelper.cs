using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.UI;

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

    public void LoadScene(string scene, Action OnFinished)
    {
        StartCoroutine(LoadSceneAsync(scene, OnFinished));
    }

    IEnumerator LoadSceneAsync(string s, Action OnFinished)
    {
        mAsync = SceneManager.LoadSceneAsync(s);
        mAsync.allowSceneActivation = true;
        while (!mAsync.isDone)
            yield return 0;
        if (OnFinished != null)
            OnFinished();
        Destroy(this);
    }

    public void Load()
    {
        StartCoroutine(LoadLevelAsync());
    }

    IEnumerator LoadLevelAsync()
    {
        int displayProgress = 0;
        int toProgress = 0;
        yield return 0;
        LevelDatas.LevelDatas lev = Main.Ins.CombatData.GLevelItem;
        ResMng.LoadScene(lev.Scene);
        mAsync = SceneManager.LoadSceneAsync(lev.Scene);
        mAsync.allowSceneActivation = false;
        while (mAsync.progress < 0.9f)
        {
            toProgress = (int)mAsync.progress * 100;
            while (displayProgress < toProgress)
            {
                ++displayProgress;
                if (LoadingDialogState.Exist)
                    LoadingDialogState.Instance.UpdateProgress(displayProgress / 100.0f);
                yield return 0;
            }
            yield return 0;
        }
        toProgress = 100;
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            if (LoadingDialogState.Exist)
                LoadingDialogState.Instance.UpdateProgress(displayProgress / 100.0f);
            yield return 0;
        }
        mAsync.allowSceneActivation = true;
        while (!mAsync.isDone)
            yield return 0;
        OnLoadFinishedEx(lev);
        Destroy(this);
    }

    public static LevelScriptBase GetLevelScript(string sn)
    {
        string typeIden = string.Format("LevelScript_{0}", sn);
        Type type = Type.GetType(typeIden);
        if (type == null)
        {
            //尝试在chapter的dll里加载
            if (Main.Ins.CombatData.Chapter != null && System.IO.File.Exists(Main.Ins.CombatData.Chapter.Dll))
            {
                Assembly ass = Assembly.Load(System.IO.File.ReadAllBytes(Main.Ins.CombatData.Chapter.Dll));           
                Type[] t = ass.GetTypes();
                for (int i = 0; i < t.Length; i++)
                {
                    if (t[i].Name == typeIden)
                    {
                        //LevelScriptBase l = System.Activator.CreateInstance(t[i]) as LevelScriptBase;
                        type = t[i];
                        break;
                    }
                }
            }
            if (type == null)
            {
                Log.WriteError(string.Format("Load sn failed {0}, Meteor Version:{1}", sn, Main.Ins.AppInfo.AppVersion()));
                return null;
            }
        }
        Main.Ins.CombatData.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    //只加载地图/地图物件.要令所有客户端初始化完毕后状态一致，然后用指令播放器，播放帧指令.
    //如果是单机，就使用帧播放
    void OnLoadFinishedEx(LevelDatas.LevelDatas lev)
    {
        Main.Ins.SoundManager.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.ID, lev.LevelScript));
            return;
        }

        Main.Ins.CombatData.GScript = script;
        script.OnLoad();
        //加载场景配置数据
        Main.Ins.SceneMng.OnEnterLevel();
        GameObject battleRoot = new GameObject("GameBattle");
        Main.Ins.GameBattleEx = battleRoot.AddComponent<GameBattleEx>();
        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        Main.Ins.GameBattleEx.Init(script);
        FrameReplay.Instance.OnBattleStart();
        //寻路
        PathHelper.Ins.StartCalc();
        //如果是录制模式-完毕后不要打开战斗UI
        if (Main.Ins.CombatData.Replay)
            Main.Ins.EnterState(Main.Ins.ReplayState);
        else
            Main.Ins.EnterState(Main.Ins.FightState);
    }
}
