using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;
using UnityEngine.UI;
using Excel2Json;

//for loadingui updateui
public interface LoadingUI
{
    void UpdateProgress(float percent);
}

public class LevelHelper : MonoBehaviour
{
    public static LevelHelper Ins;
    public Coroutine loadLevel;
    public void Stop() {
        if (loadLevel != null) {
            StopCoroutine(loadLevel);
            loadLevel = null;
            Destroy(this);
        }
    }

    private void Awake() {
        Ins = this;
    }

    private void OnDestroy() {
        Ins = null;
    }

    AsyncOperation mAsync;
    public void LoadScene(string scene, Action OnFinished)
    {
        loadLevel = StartCoroutine(LoadSceneAsync(scene, OnFinished));
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
        loadLevel = StartCoroutine(LoadLevelAsync());
    }

    IEnumerator LoadLevelAsync()
    {
        //if (CombatData.Ins.Replay)
        //    CombatData.Ins.RandSeed = CombatData.Ins.GRecord.RandomSeed;
        //else
        //    CombatData.Ins.RandSeed = DateTime.Now.ToFileTime();
        //CombatData.Ins.Random = new System.Random((int)CombatData.Ins.RandSeed);
        int displayProgress = 0;
        int toProgress = 0;
        yield return 0;
        LevelData lev = CombatData.Ins.GLevelItem;
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
        loadLevel = null;
        Destroy(this);
    }

    public static LevelScriptBase GetLevelScript(string sn)
    {
        string typeIden = string.Format("LevelScript_{0}", sn);
        Type type = Type.GetType(typeIden);
        if (type == null)
        {
            //尝试在chapter的dll里加载
            if (CombatData.Ins.Chapter != null && System.IO.File.Exists(CombatData.Ins.Chapter.Dll))
            {
                Assembly ass = Assembly.Load(System.IO.File.ReadAllBytes(CombatData.Ins.Chapter.Dll));           
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
        CombatData.Ins.GScriptType = type;
        return System.Activator.CreateInstance(type) as LevelScriptBase;
    }

    //只加载地图/地图物件.要令所有客户端初始化完毕后状态一致，然后用指令播放器，播放帧指令.
    //如果是单机，就使用帧播放
    void OnLoadFinishedEx(LevelData lev)
    {
        SoundManager.Ins.Enable(true);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null)
        {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.Id, lev.LevelScript));
            return;
        }

        CombatData.Ins.GScript = script;
        //加载场景配置数据
        SceneMng.Ins.OnEnterLevel();
        GameObject battleRoot = new GameObject("GameBattle");
        Main.Ins.GameBattleEx = battleRoot.AddComponent<GameBattleEx>();
        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        Main.Ins.GameBattleEx.Init(script);
        FrameReplay.Ins.OnBattleStart();
        //寻路-单机时有效
        if (CombatData.Ins.GLevelMode <= LevelMode.CreateWorld)
            PathHelper.Ins.StartCalc();
        //如果是录制模式-完毕后不要打开战斗UI
        //if (CombatData.Ins.Replay)
        //    Main.Ins.EnterState(Main.Ins.ReplayState);
        //else
        FightState.State.Open();
    }

    public static void OnLoadFinishedSingle(int level) {
        Application.targetFrameRate = GameStateMgr.Ins.gameStatus.TargetFrame;
        DlcMng.Ins.Init();
        LevelData lev = DataMgr.Ins.GetLevelData(level);
        LevelScriptBase script = GetLevelScript(lev.LevelScript);
        if (script == null) {
            UnityEngine.Debug.LogError(string.Format("level script is null levId:{0}, levScript:{1}", lev.Id, lev.LevelScript));
            return;
        }

        CombatData.Ins.GLevelItem = lev;
        CombatData.Ins.GLevelMode = LevelMode.SinglePlayerTask;
        CombatData.Ins.GGameMode = GameMode.Normal;
        CombatData.Ins.GScript = script;
        CombatData.Ins.wayPoints = CombatData.GetWayPoint(CombatData.Ins.GLevelItem);
        //加载场景配置数据
        SceneMng.Ins.OnEnterLevel();

        GameObject battleRoot = new GameObject("GameBattle");
        Main.Ins.GameBattleEx = battleRoot.AddComponent<GameBattleEx>();
        //等脚本设置好物件的状态后，根据状态决定是否生成受击盒，攻击盒等.
        Main.Ins.GameBattleEx.Init(script);
        FrameReplay.Ins.OnBattleStart();
        PathHelper.Ins.StartCalc();
        FightState.State.Open();
    }
}
