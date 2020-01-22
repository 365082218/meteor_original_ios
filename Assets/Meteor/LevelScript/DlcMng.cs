using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using UnityEngine;

public class DlcLevelMng
{
    string levText;
    public DlcLevelMng()
    {

    }

    public DlcLevelMng(string lev)
    {
        levText = lev;
    }
    public string TableName() { return levText; }
    public List<LevelDatas.LevelDatas> GetAllLevel()
    {
        //加载外部的proto.bytes,获取里面的数据
        return null;
    }

    public LevelDatas.LevelDatas GetLevel(int level)
    {
        return null;
    }
}

//资料片管理器
public class DlcMng
{
    //取得资料片内所有关卡资料.
    public List<LevelDatas.LevelDatas> GetDlcLevel(int idx)
    {
        Chapter cha = GetPluginChapter(idx);
        return cha.LoadAll();
    }

    //打开资料片中指定关卡
    public void PlayDlc(Chapter chapter, int levelIdx)
    {
        Main.Instance.GameStateMgr.SaveState();
        LevelDatas.LevelDatas lev = chapter.GetItem(levelIdx);
        Main.Instance.CombatData.GLevelItem = lev;
        Main.Instance.CombatData.GLevelMode = LevelMode.SinglePlayerTask;
        Main.Instance.CombatData.GGameMode = GameMode.Normal;
        Main.Instance.CombatData.wayPoints = CombatData.GetWayPoint(lev);
        U3D.LoadLevelEx();
    }

    //初始化各个路径.
    public void Init()
    {
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Plugins/");

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/Model/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Plugins/Model/");

        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/Dlc/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Plugins/Dlc/");

        //下载NPC定义文件，用于DLC内得关卡脚本加载NPC时使用
        if (!System.IO.Directory.Exists(Application.persistentDataPath + "/Plugins/Def/"))
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Plugins/Def/");
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
        for (int i = 0; i < Main.Instance.GameStateMgr.gameStatus.pluginChapter.Count; i++)
        {
            if (Main.Instance.GameStateMgr.gameStatus.pluginChapter[i].ChapterId == dlc)
            {
                Target = Main.Instance.GameStateMgr.gameStatus.pluginChapter[i];
                break;
            }
        }
        return Target;
    }

    public static ModelItem GetPluginModel(int model)
    {
        ModelItem Target = null;
        for (int i = 0; i < Main.Instance.GameStateMgr.gameStatus.pluginModel.Count; i++)
        {
            if (Main.Instance.GameStateMgr.gameStatus.pluginModel[i].ModelId == model)
            {
                Target = Main.Instance.GameStateMgr.gameStatus.pluginModel[i];
                break;
            }
        }
        return Target;
    }

    public Chapter FindChapterByLevel(LevelDatas.LevelDatas lev)
    {
        if (Dlcs.Count == 0 && Main.Instance.GameStateMgr.gameStatus.pluginChapter != null)
        {
            for (int i = 0; i < Main.Instance.GameStateMgr.gameStatus.pluginChapter.Count; i++)
            {
                if (Main.Instance.GameStateMgr.gameStatus.pluginChapter[i].Installed)
                    Dlcs.Add(Main.Instance.GameStateMgr.gameStatus.pluginChapter[i]);
            }
        }

        for (int i = 0; i < Dlcs.Count; i++)
        {
            List<LevelDatas.LevelDatas> all = Dlcs[i].LoadAll();
            for (int j = 0; j < all.Count; j++)
            {
                if (all[j] == lev)
                    return Dlcs[i];
            }
        }
        return null;
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
    public bool processing = false;
    private PluginCtrl processingCtrl = null;
    //存储得取消掉得预览图
    private string cancelPreviewTask = "";
    //存储得取消掉得下载任务
    private string cancelDownloadTask = "";
    //放到预览图下载队列里，绑定到一个UI,做任务控制，避免多个UI同时更新自己的预览图.
    List<PluginCtrl> PreviewTask = new List<PluginCtrl>();
    List<string> PreviewTaskCache = new List<string>();//已经下载过预览图的，下次翻页不要再下载.单次运行APP下载一次
    public void AddPreviewTask(PluginCtrl uiCtrl)
    {
        lock (PreviewTask)
        {
            if (!PreviewTask.Contains(uiCtrl))
                PreviewTask.Add(uiCtrl);
        }
    }

    public void RemovePreviewTask(PluginCtrl uiCtrl)
    {
        lock (PreviewTask)
        {
            if (processingCtrl == uiCtrl)
            {
                if (uiCtrl.Target != null)
                    cancelPreviewTask = uiCtrl.Target.Preview;
                else if (uiCtrl.Chapter != null)
                    cancelPreviewTask = uiCtrl.Chapter.Preview;
                if (client != null)
                {
                    client.CancelAsync();
                    client.Dispose();
                    client = null;
                }
            }
            if (PreviewTask.Contains(uiCtrl))
                PreviewTask.Remove(uiCtrl);
        }
    }

    public void RemoveDownloadTask(PluginCtrl uiCtrl)
    {
        lock (DownloadTask)
        {
            if (processingCtrl == uiCtrl)
            {
                if (client != null)
                {
                    client.CancelAsync();
                    client.Dispose();
                    client = null;
                }
            }
            if (DownloadTask.Contains(uiCtrl))
                DownloadTask.Remove(uiCtrl);
        }
    }
    
    //下载队列，避免多个控制器一起下载.
    List<PluginCtrl> DownloadTask = new List<PluginCtrl>();
    public void AddDownloadTask(PluginCtrl uiCtrl)
    {
        lock (DownloadTask)
        {
            if (!DownloadTask.Contains(uiCtrl))
                DownloadTask.Insert(0, uiCtrl);
        }
    }

    WebClient client = null;
    public delegate void OnCrossThreadFunc();
    List<OnCrossThreadFunc> handler = new List<OnCrossThreadFunc>();
    public void Update()
    {
        if (!processing)
        {
            //下载预览图任务
            if (PreviewTask.Count != 0)
            {
                processingCtrl = PreviewTask[0];
                ModelItem it = processingCtrl.Target;
                if (it != null)
                {
                    //如果该封面已经下载过，并且存储到本地了，那么从本地读取
                    if (PreviewTaskCache.Contains(processingCtrl.Target.Preview))
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(processingCtrl.Target.Preview, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                        byte[] bitIcon = new byte[fs.Length];
                        fs.Read(bitIcon, 0, (int)fs.Length);
                        fs.Close();
                        Texture2D tex = new Texture2D(200, 150);
                        tex.LoadImage(bitIcon);
                        processingCtrl.Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                        processingCtrl = null;
                        PreviewTask.RemoveAt(0);
                        return;
                    }
                    //如果该封面没有下载过.开始下载
                    processing = true;
                    string urlIconPreview = string.Format("http://{0}/meteor/{1}", Main.strHost, it.webPreview);
                    try
                    {
                        if (client == null)
                        {
                            client = new WebClient();
                            client.DownloadDataCompleted += OnDownloadPreviewComplete;
                        }
                        client.DownloadDataAsync(new Uri(urlIconPreview));
                    }
                    catch
                    {
                        processing = false;
                    }
                    return;
                }
                else
                {
                    //dlc的预览图.
                    Chapter cha = processingCtrl.Chapter;
                    //如果该封面已经下载过，并且存储到本地了，那么从本地读取
                    if (PreviewTaskCache.Contains(processingCtrl.Chapter.Preview))
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(processingCtrl.Chapter.Preview, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                        byte[] bitIcon = new byte[fs.Length];
                        fs.Read(bitIcon, 0, (int)fs.Length);
                        fs.Close();
                        Texture2D tex = new Texture2D(200, 150);
                        tex.LoadImage(bitIcon);
                        processingCtrl.Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                        processingCtrl = null;
                        PreviewTask.RemoveAt(0);
                        return;
                    }
                    processing = true;
                    string urlIconPreview = string.Format("http://{0}/meteor/{1}", Main.strHost, cha.webPreview);
                    try
                    {
                        if (client == null)
                        {
                            client = new WebClient();
                            client.DownloadDataCompleted += OnDownloadPreviewComplete;
                        }
                        client.DownloadDataAsync(new Uri(urlIconPreview));
                       
                    }
                    catch
                    {

                    }
                }
            }

            //下载模组zip任务.
            if (DownloadTask.Count != 0)
            {
                processingCtrl = DownloadTask[0];
                ModelItem it = processingCtrl.Target;
                if (it != null)
                {
                    //如果该模组没有
                    processing = true;
                    if (client == null)
                    {
                        client = new WebClient();
                        client.DownloadProgressChanged += this.DownLoadProgressChanged;
                        client.DownloadFileCompleted += this.DownLoadFileCompleted;
                        
                        string downloadUrl = string.Format("http://{0}/meteor/{1}", Main.strHost, processingCtrl.Target.Path);
                        client.DownloadFileAsync(new System.Uri(downloadUrl), processingCtrl.Target.LocalPath);
                    }
                    return;
                }
                else
                {
                    processing = true;
                    //dlc的下载.
                    if (client == null)
                    {
                        client = new WebClient();
                        client.DownloadProgressChanged += this.DownLoadProgressChanged;
                        client.DownloadFileCompleted += this.DownLoadFileCompleted;

                        string downloadUrl = string.Format("http://{0}/meteor/{1}", Main.strHost, processingCtrl.Chapter.Path);
                        client.DownloadFileAsync(new System.Uri(downloadUrl), processingCtrl.Chapter.LocalPath);
                    }
                    return;
                }
            }
        }

        lock (handler)
        {
            for (int i = 0; i < handler.Count; i++)
            {
                handler[i].Invoke();
            }
            handler.Clear();
        }
    }

    void DownLoadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        lock (handler)
        {
            handler.Add(() =>
            {
                if (processingCtrl != null)
                    processingCtrl.DownLoadFileCompleted(sender, e);
                if (client != null)
                {
                    client.Dispose();
                    client = null;
                }
                lock (DownloadTask)
                {
                    if (DownloadTask.Contains(processingCtrl))
                        DownloadTask.Remove(processingCtrl);
                }
                processingCtrl = null;
                processing = false;
            });
        }
    }

    void DownLoadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        lock (handler)
        {
            handler.Add(() =>
            {
                if (processingCtrl != null)
                    processingCtrl.DownLoadProgressChanged(sender, e);
            });
        }
    }

    private void OnDownloadPreviewComplete(object sender, DownloadDataCompletedEventArgs e)
    {
        lock (handler)
        {
            handler.Add(() => {
                if (e.Error != null)
                {
                    if (processingCtrl != null)
                    {
                        if (processingCtrl.retryNum >= 1)
                        {
                            //跳过这个任务，开始下载下一个的预览图
                            lock (PreviewTask)
                            {
                                if (PreviewTask.Contains(processingCtrl))
                                    PreviewTask.Remove(processingCtrl);
                            }
                        }
                        else
                            processingCtrl.retryNum++;
                    }
                    
                    if (client != null)
                    {
                        client.Dispose();
                        client = null;
                    }
                    processingCtrl = null;
                    processing = false;
                }
                else
                {
                    if (processingCtrl == null)
                    {
                        //保存到本地，下次直接从本地读取.
                        //中断得下载预览图任务存储路径
                        if (!string.IsNullOrEmpty(cancelPreviewTask))
                        {
                            byte[] bitPrev = e.Result;
                            if (bitPrev != null && bitPrev.Length != 0)
                            {
                                System.IO.FileStream fs = new System.IO.FileStream(cancelPreviewTask, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                                fs.Write(bitPrev, 0, bitPrev.Length);
                                fs.SetLength(bitPrev.Length);
                                fs.Flush();
                                fs.Close();
                                lock (PreviewTaskCache)
                                {
                                    PreviewTaskCache.Add(cancelPreviewTask);
                                }
                            }
                        }
                        processing = false;
                        return;
                    }
                    byte[] bitIcon = e.Result;
                    if (bitIcon != null && bitIcon.Length != 0)
                    {
                        if (processingCtrl.Target != null)
                        {
                            try
                            {
                                lock (PreviewTask)
                                {
                                    if (PreviewTask.Contains(processingCtrl))
                                    {
                                        System.IO.FileStream fs = new System.IO.FileStream(processingCtrl.Target.Preview, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                                        fs.Write(bitIcon, 0, bitIcon.Length);
                                        fs.SetLength(bitIcon.Length);
                                        fs.Flush();
                                        fs.Close();
                                        PreviewTask.Remove(processingCtrl);
                                    }
                                }
                            }
                            catch (System.Exception exp)
                            {
                                Debug.Log(exp.Message);
                            }
                            Texture2D tex = new Texture2D(200, 150);
                            tex.LoadImage(bitIcon);
                            processingCtrl.Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                            lock (PreviewTaskCache)
                            {
                                PreviewTaskCache.Add(processingCtrl.Target.Preview);
                            }
                            client.Dispose();
                            client = null;
                            processingCtrl = null;
                            processing = false;
                        }
                        else if (processingCtrl.Chapter != null)
                        {
                            lock (PreviewTask)
                            {
                                if (PreviewTask.Contains(processingCtrl))
                                {
                                    if (bitIcon != null && bitIcon.Length != 0)
                                    {
                                        System.IO.File.WriteAllBytes(processingCtrl.Chapter.Preview, bitIcon);
                                        Texture2D tex = new Texture2D(200, 150);
                                        tex.LoadImage(bitIcon);
                                        processingCtrl.Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                                    }
                                    PreviewTask.Remove(processingCtrl);
                                }
                            }

                            lock (PreviewTaskCache)
                            {
                                PreviewTaskCache.Add(processingCtrl.Chapter.Preview);
                            }
                            client.Dispose();
                            client = null;
                            processingCtrl = null;
                            processing = false;
                        }
                    }
                }
            });
        }
    }
}
