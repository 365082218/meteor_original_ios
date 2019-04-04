using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class DlcLevelMng:TableManagerEx<Level, DlcLevelMng>
{
    string levText;
    public DlcLevelMng()
    {

    }

    public DlcLevelMng(string lev)
    {
        levText = lev;
        ReLoad();
    }
    public override string TableName() { return levText; }
}

//资料片管理器
public class DlcMng:Singleton<DlcMng> {
    //取得资料片内所有关卡资料.
    public Level[] GetDlcLevel(int idx)
    {
        Chapter cha = GetPluginChapter(idx);
        return cha.LoadAll();
    }

    //打开资料片中指定关卡
    public void PlayDlc(Chapter chapter, int levelIdx)
    {
        GameData.Instance.SaveState();
        if (FightWnd.Exist)
            FightWnd.Instance.Close();
        WindowMng.CloseAll();
        //暂时不允许使用声音管理器，在切换场景时不允许播放
        SoundManager.Instance.StopAll();
        SoundManager.Instance.Enable(false);
        U3D.SaveLastLevelData();
        U3D.ClearLevelData();
        Level lev = chapter.GetItem(levelIdx);
        Global.Instance.GLevelItem = lev;
        Global.Instance.GLevelMode = LevelMode.SinglePlayerTask;
        Global.Instance.GGameMode = GameMode.Normal;
        LoadingWnd.Instance.Open();
        Resources.UnloadUnusedAssets();
        GC.Collect();
        LevelHelper helper = U3D.ins.gameObject.AddComponent<LevelHelper>();
        helper.Load();
        Log.Write("helper.load end");
    }

    //初始化各个路径.
    public void Init()
    {
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/Model/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Plugins/Model/");

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/Dlc/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "Plugins/Dlc/");

        //下载NPC定义文件，用于DLC内得关卡脚本加载NPC时使用
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/Def/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "Plugins/Def/");
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

    public bool CheckDependence(Chapter chapter, out string tip)
    {
        tip = "";
        if (chapter.Res == null)
            return false;

        if (chapter.Res.model != null)
        {
            for (int i = 0; i < chapter.Res.model.Count; i++)
            {
                ModelItem m = GetPluginModel(chapter.Res.model[i]);
                if (m == null)
                {
                    m = FindModel(chapter.Res.model[i]);
                    tip = "需要先安装模型[" + m.Name + "-" + chapter.Res.model[i] + "]";
                    return true;
                }

                m.Check();
                if (!m.Installed)
                {
                    tip = "需要先安装模型[" + m.Name + "-" + chapter.Res.model[i] + "]";
                    return true;
                }
            }
        }

        if (chapter.Res.scene != null)
        {
            //地图暂时无效.
            for (int i = 0; i < chapter.Res.scene.Count; i++)
            {

            }
        }

        if (chapter.Res.weapon != null)
        {
            //武器暂时无效.
            for (int i = 0; i < chapter.Res.weapon.Count; i++)
            {

            }
        }

        return false;
    }

    public static Chapter GetPluginChapter(int dlc)
    {
        Chapter Target = null;
        for (int i = 0; i < GameData.Instance.gameStatus.pluginChapter.Count; i++)
        {
            if (GameData.Instance.gameStatus.pluginChapter[i].ChapterId == dlc)
            {
                Target = GameData.Instance.gameStatus.pluginChapter[i];
                break;
            }
        }
        return Target;
    }

    public static ModelItem GetPluginModel(int model)
    {
        ModelItem Target = null;
        for (int i = 0; i < GameData.Instance.gameStatus.pluginModel.Count; i++)
        {
            if (GameData.Instance.gameStatus.pluginModel[i].ModelId == model)
            {
                Target = GameData.Instance.gameStatus.pluginModel[i];
                break;
            }
        }
        return Target;
    }

    public Chapter FindChapter(int dlc)
    {
        Chapter Target = null;
        for (int i = 0; i < Dlcs.Count; i++)
        {
            if (Dlcs[i].ChapterId == dlc)
            {
                Target = Dlcs[i];
                break;
            }
        }
        return Target;
    }

    public ModelItem FindModel(int model)
    {
        ModelItem Target = null;
        for (int i = 0; i < Models.Count; i++)
        {
            if (Models[i].ModelId == model)
            {
                Target = Models[i];
                break;
            }
        }
        return Target;
    }

    //把插件放到一个集合里，便于翻页.
    public void CollectAll(bool showInstall = true)
    {
        allItem.Clear();
        for (int i = 0; i < Models.Count; i++)
        {
            if (Models[i].Installed && !showInstall)
                continue;
            allItem.Add(Models[i]);
        }
        for (int i = 0; i < Dlcs.Count; i++)
        {
            if (Dlcs[i].Installed && !showInstall)
                continue;
            allItem.Add(Dlcs[i]);
        }
    }

    public List<object> allItem = new List<object>();
}
