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

    //初始化各个路径.
    public void Init()
    {
        if (!System.IO.Directory.Exists(Application.persistentDataPath + @"\Plugins\Model\"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + @"\Plugins\Model\");

        if (!System.IO.Directory.Exists(Application.persistentDataPath + @"\Plugins\Dlc\"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + @"\Plugins\Dlc\");

        //下载NPC定义文件，用于DLC内得关卡脚本加载NPC时使用
        if (!System.IO.Directory.Exists(Application.persistentDataPath + @"\Plugins\Npc\"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + @"\Plugins\Npc\");
    }

    public List<ModelItem> Models = new List<ModelItem>();
    public void ClearModel()
    {
        Models.Clear();
    }

    //加入从json里获取到得一项资源
    public void AddModel(ModelItem Info)
    {
        Debug.Log("增加外部角色:" + Info.Name);
        Models.Add(Info);
    }


    //资料片
    public List<Chapter> Dlcs = new List<Chapter>();
    public void ClearDlc()
    {
        Dlcs.Clear();
    }

    public void AddDlc(Chapter cha)
    {
        Dlcs.Add(cha);
    }
}
