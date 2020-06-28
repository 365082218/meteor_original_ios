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
        Level lev = Global.Instance.GLevelItem;
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
            if (Global.Instance.Chapter != null && System.IO.File.Exists(Global.Instance.Chapter.Dll))
            {
                Assembly ass = Assembly.Load(System.IO.File.ReadAllBytes(Global.Instance.Chapter.Dll));           
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
                Log.WriteError(string.Format("Load sn failed {0}, Meteor Version:{1}", sn, AppInfo.Instance.AppVersion()));
                return null;
            }
        }
        Global.Instance.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    //只加载地图/地图物件.要令所有客户端初始化完毕后状态一致，然后用指令播放器，播放帧指令.
    //如果是单机，就使用帧播放
    void OnLoadFinishedEx(Level lev)
    {
        SoundManager.Instance.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.ID, lev.LevelScript));
            return;
        }

        Global.Instance.GScript = script;
        script.OnLoad();
        //加载场景配置数据
        SceneMng.Instance.OnEnterLevel();

        GameObject battleRoot = new GameObject("GameBattle");
        battleRoot.AddComponent<GameBattleEx>();
        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        GameBattleEx.Instance.Init(script);

        FrameReplay.Instance.OnBattleStart();

        Utility.EnterState(Main.Instance.FightDialogState);
    }

    public static void OnLoadFinishedSingle(int level) {
        GameData.Instance.LoadState();
        GameData.Instance.InitTable();

        SFXLoader.Instance.InitSync();
        //在读取character.act后再初始化输入模块。
        ActionInterrupt.Instance.Lines.Clear();
        ActionInterrupt.Instance.Whole.Clear();
        ActionInterrupt.Instance.Root = null;
        ActionInterrupt.Instance.Init();
        MenuResLoader.Instance.Init();

        for (int i = 0; i < 20; i++) {
            AmbLoader.Ins.LoadCharacterAmb(i);
        }

        AmbLoader.Ins.LoadCharacterAmb();
        AmbLoader.Ins.LoadCharacterAmbEx();

        PoseStatus.Clear();
        Application.targetFrameRate = GameData.Instance.gameStatus.TargetFrame;

        DlcMng.Instance.Init();

        Level lev = LevelMng.Instance.GetItem(level);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null) {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.ID, lev.LevelScript));
            return;
        }

        Global.Instance.GLevelItem = lev;
        Global.Instance.GLevelMode = LevelMode.SinglePlayerTask;
        Global.Instance.GGameMode = GameMode.Normal;
        Global.Instance.GScript = script;
        script.OnLoad();
        //加载场景配置数据
        SceneMng.Instance.OnEnterLevel();

        GameObject battleRoot = new GameObject("GameBattle");
        battleRoot.AddComponent<GameBattleEx>();
        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        GameBattleEx.Instance.Init(script);

        FrameReplay.Instance.OnBattleStart();

        if (Main.Instance != null) {
            Utility.EnterState(Main.Instance.FightDialogState);
        }
    }
}
