using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DlcLevelMng:TableManagerEx<Level, DlcLevelMng>
{
    string levText;
    public DlcLevelMng(string lev)
    {
        levText = lev;
        ReLoad();
    }
    public override string TableName() { return levText; }
}

//资料片管理器
public class DlcMng:Singleton<DlcMng> {
    Dictionary<int, Module> Plgins = new Dictionary<int, Module>();
    Module currentSelect = null;
    public void SelectDlc(int dlcIdx)
    {
        if (Plgins.ContainsKey(dlcIdx))
            currentSelect = Plgins[dlcIdx];
    }

    public LevelScriptBase GetLevelScript(int lv)
    {
        if (currentSelect != null)
        {
            Type type = currentSelect.GetType(string.Format("LevelScript_{0}", lv));
            if (type == null)
                return null;
            Global.Instance.GScriptType = type;
            return System.Activator.CreateInstance(type) as LevelScriptBase;
        }
        return null;
    }

    //得到资料片内指定关卡资料
    public Level GetLevelItem(int level)
    {
        return null;
    }

    //取得资料片内所有关卡资料.
    public Level[] GetDlcLevel(int idx)
    {
        Chapter cha = U3D.GetPluginChapter(idx);
        return cha.LoadAll();
    }

    //打开资料片中指定关卡
    public void PlayDlc(int dlcIdx, int levelIdx)
    {
        
        GameData.Instance.SaveState();
        if (FightWnd.Exist)
            FightWnd.Instance.Close();
        //if (StateWnd.Exist)
        //    StateWnd.Instance.Close();
        WindowMng.CloseAll();
        //暂时不允许使用声音管理器，在切换场景时不允许播放
        SoundManager.Instance.StopAll();
        SoundManager.Instance.Enable(false);
        U3D.SaveLastLevelData();
        U3D.ClearLevelData();
        Level lev = DlcMng.Instance.GetLevelItem(levelIdx);
        Global.Instance.GLevelItem = lev;
        Global.Instance.GLevelMode = LevelMode.SinglePlayerTask;
        Global.Instance.GGameMode = GameMode.Normal;
        LoadingWnd.Instance.Open();
        Resources.UnloadUnusedAssets();
        GC.Collect();
        LevelHelper helper = U3D.ins.gameObject.AddComponent<LevelHelper>();
        helper.Load(levelIdx);
        Log.Write("helper.load end");
    }
}
