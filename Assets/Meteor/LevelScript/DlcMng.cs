using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using UnityEngine;
using Excel2Json;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
//下载的资料类型
public enum TaskType {
    Chapter,//剧本
    Model,//模型
    ChapterPreview,//剧本预览图
    ModelPreview,//模型预览图
}
public enum TaskStatus {
    None = 0,//队列中等待
    Normal = 1,//下载中
    Loaded,//下载完成
    Installed,//解压完毕
    Error,//出错.
}
public class DownloadTask {
    public TaskType type;
    public int index;
    public int progress;
    public TaskStatus state;//0队列中，1正在处理，2已安装待删除--只针对ZIP格式的
    public Action<object, DownloadProgressChangedEventArgs> OnProgressChanged;
    public Action<object, AsyncCompletedEventArgs> OnComplete;//成功完成
    public Action OnError;//出问题
    public Action OnCancel;//取消
    public int errno;
    public string url;
    public DownloadTask(TaskType t, int k = 0, string path = "") {
        type = t;
        url = path;
        index = k;
        state = TaskStatus.None;
        if (string.IsNullOrEmpty(url)) {
            VaildUrl();
        }
    }

    void VaildUrl() {
        Chapter chapter = null;
        ModelItem model = null;
        switch (type) {
            case TaskType.Chapter:
                chapter = DlcMng.Ins.GetChapterMeta(index);
                url = chapter.Path;
                break;
            case TaskType.Model:
                model = DlcMng.Ins.GetModelMeta(index);
                url = model.Path;
                break;
            case TaskType.ChapterPreview:
                chapter = DlcMng.Ins.GetChapterMeta(index);
                url = chapter.webPreview;
                break;
            case TaskType.ModelPreview:
                model = DlcMng.Ins.GetModelMeta(index);
                url = model.webPreview;
                break;
        }
    }
}

public class UnzipCallbackExDefault : ZipUtility.UnzipCallback {
    DownloadManager Target;
    public UnzipCallbackExDefault(DownloadManager ctrl) {
        Target = ctrl;
    }
    /// <summary>
    /// 解压执行完毕后的回调
    /// </summary>
    /// <param name="_result">true表示解压成功，false表示解压失败</param>
    public override void OnFinished(bool _result) {
        if (_result) {
            if (Target != null)
                Target.OnUnZipComplete();//安装成功
        } else {
            if (Target != null)
                Target.OnUnZipFailed();//安装失败，可能是磁盘空间不足
        }
    }
}
//记录了所有的任务列表
public class DownloadManager:Singleton<DownloadManager> {
    public bool processing = false;
    private DownloadTask processingTask = null;
    UnzipCallbackExDefault OnUnZipFinishDefault;
    public DownloadManager() {
        OnUnZipFinishDefault = new UnzipCallbackExDefault(this);
    }

    public void OnUnZipComplete() {
        if (processingTask != null) {
            if (processingTask.type == TaskType.Model) {
                ModelItem Model = DlcMng.Ins.GetModelMeta(processingTask.index);
                if (Model != null) {
                    string[] files = System.IO.Directory.GetFiles(Model.LocalPath.Substring(0, Model.LocalPath.Length - 4), "*.*", System.IO.SearchOption.AllDirectories);
                    if (Model.resPath == null)
                        Model.resPath = new List<string>();
                    if (Model.resCrc == null)
                        Model.resCrc = new List<string>();
                    Model.resPath.Clear();
                    Model.resCrc.Clear();
                    for (int i = 0; i < files.Length; i++) {
                        Model.resPath.Add(files[i].Replace("\\", "/"));
                        Model.resCrc.Add(Utility.getFileHash(files[i]));
                    }
                    Model.Installed = true;
                    GameStateMgr.Ins.gameStatus.RegisterModel(Model);
                }
            } else if (processingTask.type == TaskType.Chapter) {
                Chapter Chapter = DlcMng.Ins.GetChapterMeta(processingTask.index);
                if (Chapter != null) {
                    string[] files = System.IO.Directory.GetFiles(Chapter.LocalPath.Substring(0, Chapter.LocalPath.Length - 4), "*.*", System.IO.SearchOption.AllDirectories);
                    if (Chapter.resPath == null)
                        Chapter.resPath = new List<string>();
                    if (Chapter.resCrc == null)
                        Chapter.resCrc = new List<string>();
                    if (Chapter.Res == null)
                        Chapter.Res = new List<ReferenceItem>();
                    Chapter.resPath.Clear();
                    Chapter.resCrc.Clear();
                    Chapter.Res.Clear();
                    for (int i = 0; i < files.Length; i++) {
                        //Debug.Log(files[i]);
                        System.IO.FileInfo fi = new System.IO.FileInfo(files[i]);
                        ReferenceItem template = new ReferenceItem();
                        template.Name = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                        template.Path = files[i].Replace("\\", "/");

                        if (fi.Extension.ToLower() == ".txt") {
                            if (fi.FullName.Contains("npc/") || fi.FullName.Contains("npc\\")) {
                                template.Type = FileExt.Txt;
                            }
                        } else if (fi.Extension.ToLower() == ".pos") {//动画分段配置
                            template.Type = FileExt.Pos;
                        } else if (fi.Extension.ToLower() == ".des") {//关卡配置
                            template.Type = FileExt.Des;
                        } else if (fi.Extension.ToLower() == ".skc") {//角色皮肤
                            template.Type = FileExt.Skc;
                        } else if (fi.Extension.ToLower() == ".bnc") {//角色骨架
                            template.Type = FileExt.Bnc;
                        } else if (fi.Extension.ToLower() == ".png") {//贴图
                            template.Type = FileExt.Png;
                        } else if (fi.Extension.ToLower() == ".jpg") {//贴图
                            template.Type = FileExt.Jpeg;
                        } else if (fi.Extension.ToLower() == ".amb") {//动画
                            template.Type = FileExt.Amb;
                        } else if (fi.Extension.ToLower() == ".wp") {//路点
                            template.Type = FileExt.WayPoint;
                        } else if (fi.Extension.ToLower() == ".gmc") {//模型
                            template.Type = FileExt.Gmc;
                        } else if (fi.Extension.ToLower() == ".fmc") {//动画
                            template.Type = FileExt.Fmc;
                        } else if (fi.Extension.ToLower() == ".gmb") {//模型
                            template.Type = FileExt.Gmc;
                        } else if (fi.Extension.ToLower() == ".ef") {//特效
                            template.Type = FileExt.Sfx;
                        } else if (fi.Extension.ToLower() == ".dll") {//特效
                            template.Type = FileExt.Dll;
                        } else if (fi.Extension.ToLower() == ".json") {//特效
                            template.Type = FileExt.Json;
                        } else if (fi.Extension.ToLower() == ".mp3") {
                            template.Type = FileExt.MP3;
                        }
                        Chapter.Res.Add(template);
                        Chapter.resPath.Add(files[i].Replace("\\", "/"));
                        Chapter.resCrc.Add(Utility.getFileHash(files[i]));
                    }
                    Chapter.Installed = true;
                    GameStateMgr.Ins.gameStatus.RegisterDlc(Chapter);
                }
            }
            GameStateMgr.Ins.SaveState();
            processingTask.state = TaskStatus.Installed;
            //刷新一下各个任务的状态.
            if (DlcManagerDialogState.Exist) {
                DlcManagerDialogState.Instance.OnRefresh(processingTask.index, processingTask.type);
            }

            if (client != null) {
                client.Dispose();
                client = null;
            }
            if (processingTask != null)
                RemoveTask(processingTask.type, processingTask.index);
            processingTask = null;
            processing = false;
        }

    }
    public void OnUnZipFailed() {

    }
    //队列里的全部任务
    List<DownloadTask> Tasks = new List<DownloadTask>();
    public DownloadTask GetTask(TaskType type, int key) {
        for (int i = 0; i < Tasks.Count; i++) {
            if (Tasks[i].index == key && Tasks[i].type == type) {
                return Tasks[i];
            }
        }
        return null;
    }

    public DownloadTask AddTask(TaskType type, int key) {
        if (string.IsNullOrEmpty(Main.Ins.baseUrl))
            return null;
        DownloadTask t = GetTask(type, key);
        if (t == null) {
            //这是个新任务.
            t = new DownloadTask(type, key);
            //if (type == TaskType.Chapter)
            //    Debug.LogError("add task once");
            //如果UI没有了，不影响我后台安装.
            if (type == TaskType.Chapter || type == TaskType.Model) {
                t.OnComplete = OnFileDownload;
            }
            Tasks.Insert(0, t);
        }
        return t;
    }
    
    //默认处理各种资料片下载后的行为
    public void OnFileDownload(object sender, AsyncCompletedEventArgs e) {
        if (e.Error != null) {
            UnityEngine.Debug.LogError(e.Error);
            if (processingTask != null) {
                processingTask.state = TaskStatus.Error;
                //刷新一下各个任务的状态.
                if (DlcManagerDialogState.Exist) {
                    DlcManagerDialogState.Instance.OnRefresh(processingTask.index, processingTask.type);
                }
            }
            return;
        }

        if (e.Cancelled) {
            return;
        }

        if (processingTask != null) {
            processingTask.state = TaskStatus.Loaded;
            if (processingTask.type == TaskType.Model) {
                ModelItem Model = DlcMng.Ins.GetModelMeta(processingTask.index);
                if (Model != null) {
                    ZipUtility.UnzipFile(Model.LocalPath, Model.LocalPath.Substring(0, Model.LocalPath.Length - 4), null, OnUnZipFinishDefault);
                }
            }
            else if (processingTask.type == TaskType.Chapter) {
                Chapter Chapter = DlcMng.Ins.GetChapterMeta(processingTask.index);
                if (Chapter != null) {
                    ZipUtility.UnzipFile(Chapter.LocalPath, Chapter.LocalPath.Substring(0, Chapter.LocalPath.Length - 4), null, OnUnZipFinishDefault);
                }
            }

        }
    }

    //已经下载完毕正在安装的时候删除，返回失败
    public bool CancelTask(TaskType type, int key) {
        DownloadTask task = GetTask(type, key);
        if (task != null) {
            if (processingTask == task) {
                if (processingTask.state >= TaskStatus.Loaded)
                    return false;
                if (client != null) {
                    client.CancelAsync();
                    client.Dispose();
                    client = null;
                }
                processing = false;
            }
            if (task.OnCancel != null)
                task.OnCancel.Invoke();
            Tasks.Remove(task);
            return true;
        }
        return false;
    }

    public bool RemoveTask(TaskType type, int key) {
        DownloadTask task = GetTask(type, key);
        if (task != null) {
            if (processingTask == task) {
                if (client != null) {
                    client.CancelAsync();
                    client.Dispose();
                    client = null;
                }
            }
            if (task.OnCancel != null)
                task.OnCancel.Invoke();
            Tasks.Remove(task);
            return true;
        }
        return false;
    }

    WebClient client = null;
    public delegate void OnCrossThreadFunc();
    List<OnCrossThreadFunc> handler = new List<OnCrossThreadFunc>();
    public void Update() {
        if (!processing) {
            //下载预览图任务
            if (Tasks.Count != 0) {
                processingTask = Tasks[0];
                if (processingTask.type == TaskType.ModelPreview) {
                    ModelItem it = DlcMng.Ins.GetModelMeta(processingTask.index);
                    if (it != null) {
                        //如果该封面已经下载过，并且存储到本地了，那么从本地读取
                        if (File.Exists(it.Preview)) {
                            System.IO.FileStream fs = new System.IO.FileStream(it.Preview, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                            byte[] bitIcon = new byte[fs.Length];
                            fs.Read(bitIcon, 0, (int)fs.Length);
                            fs.Close();
                            if (processingTask != null && processingTask.OnComplete != null)
                                processingTask.OnComplete.Invoke(null, null);
                            processingTask = null;
                            Tasks.RemoveAt(0);
                            return;
                        }
                        //如果该封面没有下载过.开始下载
                        processing = true;
                        string urlIconPreview = Main.Ins.baseUrl + it.webPreview;
                        try {
                            if (client == null) {
                                client = new WebClient();
                                client.DownloadDataCompleted += OnDownloadPreviewComplete;
                            }
                            client.DownloadDataAsync(new Uri(urlIconPreview));
                        } catch {
                            processing = false;
                        }
                        return;
                    }
                } else if (processingTask.type == TaskType.ChapterPreview) {
                    //dlc的预览图.
                    Chapter cha = DlcMng.Ins.GetChapterMeta(processingTask.index);
                    //如果该封面已经下载过，并且存储到本地了，那么从本地读取
                    if (File.Exists(cha.Preview)) {
                        System.IO.FileStream fs = new System.IO.FileStream(cha.Preview, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                        byte[] bitIcon = new byte[fs.Length];
                        fs.Read(bitIcon, 0, (int)fs.Length);
                        fs.Close();
                        //Texture2D tex = new Texture2D(200, 150);
                        //tex.LoadImage(bitIcon);
                        //processingTask.Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                        if (processingTask != null && processingTask.OnComplete != null)
                            processingTask.OnComplete.Invoke(null, null);
                        processingTask = null;
                        Tasks.RemoveAt(0);
                        return;
                    }
                    processing = true;
                    string urlIconPreview = Main.Ins.baseUrl + cha.webPreview;
                    try {
                        if (client == null) {
                            client = new WebClient();
                            client.DownloadDataCompleted += OnDownloadPreviewComplete;
                        }
                        client.DownloadDataAsync(new Uri(urlIconPreview));
                    } catch {
                        processing = false;
                    }
                } else if (processingTask.type == TaskType.Model) {
                    processing = true;
                    processingTask.state = TaskStatus.Normal;
                    ModelItem it = DlcMng.Ins.GetModelMeta(processingTask.index);
                    if (client == null) {
                        client = new WebClient();
                        client.DownloadProgressChanged += this.DownLoadProgressChanged;
                        client.DownloadFileCompleted += this.DownLoadFileCompleted;
                        string downloadUrl = Main.Ins.baseUrl + it.Path;
                        client.DownloadFileAsync(new System.Uri(downloadUrl), it.LocalPath);
                    }
                } else if (processingTask.type == TaskType.Chapter) {
                    processing = true;
                    processingTask.state = TaskStatus.Normal;
                    Chapter cha = DlcMng.Ins.GetChapterMeta(processingTask.index);
                    //dlc的下载.
                    if (client == null) {
                        client = new WebClient();
                        client.DownloadProgressChanged += this.DownLoadProgressChanged;
                        client.DownloadFileCompleted += this.DownLoadFileCompleted;
                        string downloadUrl = Main.Ins.baseUrl + cha.Path;
                        client.DownloadFileAsync(new System.Uri(downloadUrl), cha.LocalPath);
                    }
                }
            }
        }

        lock (handler) {
            for (int i = 0; i < handler.Count; i++) {
                handler[i].Invoke();
            }
            handler.Clear();
        }
    }

    void DownLoadFileCompleted(object sender, AsyncCompletedEventArgs e) {
        lock (handler) {
            handler.Add(() => {
                if (processingTask != null && processingTask.OnComplete != null) {
                    processingTask.OnComplete(sender, e);
                }
            });
        }
    }

    void DownLoadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
        lock (handler) {
            handler.Add(() => {
                if (processingTask != null){
                    processingTask.progress = e.ProgressPercentage;
                    if (processingTask.OnProgressChanged != null)
                        processingTask.OnProgressChanged(sender, e);
                }
            });
        }
    }

    private void OnDownloadPreviewComplete(object sender, DownloadDataCompletedEventArgs e) {
        lock (handler) {
            handler.Add(() => {
                if (e.Error != null) {
                    if (client != null) {
                        client.Dispose();
                        client = null;
                    }
                    if (processingTask != null) {
                        RemoveTask(processingTask.type, processingTask.index);
                    }
                    processingTask = null;
                    processing = false;
                } else {
                    if (processingTask == null) {
                        processing = false;
                        return;
                    }
                    byte[] bitIcon = e.Result;
                    if (bitIcon != null && bitIcon.Length != 0) {
                        if (processingTask.type == TaskType.ModelPreview) {
                            ModelItem model = DlcMng.Ins.GetModelMeta(processingTask.index);
                            if (model != null) {
                                try {
                                    System.IO.File.WriteAllBytes(model.Preview, bitIcon);
                                } catch (Exception exp) {
                                    UnityEngine.Debug.Log(exp.Message);
                                }
                                //刷新UI
                                if (processingTask.OnComplete != null) {
                                    processingTask.OnComplete.Invoke(null, null);
                                }
                                client.Dispose();
                                client = null;
                                if (processingTask != null) {
                                    RemoveTask(processingTask.type, processingTask.index);
                                }
                                processingTask = null;
                                processing = false;
                            }
                        } else if (processingTask.type == TaskType.ChapterPreview) {
                            Chapter chapter = DlcMng.Ins.GetChapterMeta(processingTask.index);
                            if (chapter != null) {
                                if (bitIcon != null && bitIcon.Length != 0) {
                                    System.IO.File.WriteAllBytes(chapter.Preview, bitIcon);
                                }
                                if (processingTask.OnComplete != null) {
                                    processingTask.OnComplete.Invoke(null, null);
                                }
                                client.Dispose();
                                client = null;
                                if (processingTask != null) {
                                    RemoveTask(processingTask.type, processingTask.index);
                                }
                                processingTask = null;
                                processing = false;
                            }
                        }
                    }
                }
            });
        }
    }
}

public class DlcLevelMng {
    LevelDataMgr data;
    public DlcLevelMng(string lev) {
        data = new LevelDataMgr();
        //Stopwatch start = new Stopwatch();
        //start.Start();
        DataMgr.Ins.loadJson<LevelDataMgr>(data, lev);
        //start.Stop();
        //UnityEngine.Debug.LogError("load level json cost:" + start.ElapsedMilliseconds);
    }

    public List<LevelData> GetAllLevel() {
        return data.LevelDatas.Values.ToList();
    }

    public LevelData GetLevel(int level) {
        if (data.LevelDatas.ContainsKey(level))
            return data.LevelDatas[level];
        return null;
    }
}

//资料片管理器
public class DlcMng:Singleton<DlcMng> {
    //取得资料片内所有关卡资料.
    public List<LevelData> GetDlcLevel(int idx) {
        Chapter cha = GetPluginChapter(idx);
        return cha.LoadAll();
    }

    //打开资料片中指定关卡
    public void PlayDlc(Chapter chapter, int levelIdx) {
        LevelData lev = chapter.GetLevel(levelIdx);
        CombatData.Ins.GLevelItem = lev;
        CombatData.Ins.GRecord = null;
        CombatData.Ins.GLevelMode = LevelMode.SinglePlayerTask;
        CombatData.Ins.GGameMode = (GameMode)lev.LevelType;
        CombatData.Ins.wayPoints = CombatData.GetWayPoint(lev);
        U3D.LoadLevelEx();
    }

    //初始化各个路径.
    public void Init() {
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

    //全部模型
    public List<ModelItem> Models = new List<ModelItem>();
    public void ClearModel() {
        Models.Clear();
    }

    //加入从json里获取到得一项资源
    public void AddModel(ModelItem Info) {
        //Debug.Log("增加外部角色:" + Info.Name);
        Models.Add(Info);
    }

    //资料片,已知的全部剧本
    public List<Chapter> Dlcs = new List<Chapter>();
    public void ClearDlc() {
        Dlcs.Clear();
    }

    public void AddDlc(Chapter cha) {
        //插件版本缺失，不再显示在面板里.
        if (!string.IsNullOrEmpty(cha.version)) {
            Dlcs.Add(cha);
        }
    }

    //判断插件版本和客户端版本的兼容性
    public bool CompatibleChapter(Chapter chapter) {
        //3.0后不支持旧版本的表了
        if (string.IsNullOrEmpty(chapter.version)) //新版本不支持旧版本格式
            return false;
        //如果客户端版本，比插件版本小，说明插件格式有更改，应该使用新的客户端读取.
        if (Main.Ins.AppInfo.AppVersionIsSmallThan(chapter.version))
            return false;
        return true;
    }

    public Chapter GetChapterMeta(int dlc) {
        for (int i = 0; i < Dlcs.Count; i++) {
            if (Dlcs[i].ChapterId == dlc)
                return Dlcs[i];
        }
        return null;
    }

    public ModelItem GetModelMeta(int model) {
        for (int i = 0; i < Models.Count; i++) {
            if (Models[i].ModelId == model)
                return Models[i];
        }
        return null;
    }

    //找到已经安装成功的章节信息
    public static Chapter GetPluginChapter(int dlc) {
        Chapter Target = null;
        for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginChapter.Count; i++) {
            if (GameStateMgr.Ins.gameStatus.pluginChapter[i].ChapterId == dlc) {
                Target = GameStateMgr.Ins.gameStatus.pluginChapter[i];
                break;
            }
        }
        return Target;
    }

    //找到已经安装成功的模型信息
    public static ModelItem GetPluginModel(int model) {
        ModelItem Target = null;
        for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginModel.Count; i++) {
            if (GameStateMgr.Ins.gameStatus.pluginModel[i].ModelId == model) {
                Target = GameStateMgr.Ins.gameStatus.pluginModel[i];
                break;
            }
        }
        return Target;
    }

    public Chapter FindChapterByLevel(LevelData lev) {
        if (Dlcs.Count == 0 && GameStateMgr.Ins.gameStatus.pluginChapter != null) {
            for (int i = 0; i < GameStateMgr.Ins.gameStatus.pluginChapter.Count; i++) {
                if (GameStateMgr.Ins.gameStatus.pluginChapter[i].Installed)
                    Dlcs.Add(GameStateMgr.Ins.gameStatus.pluginChapter[i]);
            }
        }

        for (int i = 0; i < Dlcs.Count; i++) {
            List<LevelData> all = Dlcs[i].LoadAll();
            for (int j = 0; j < all.Count; j++) {
                if (all[j] == lev)
                    return Dlcs[i];
            }
        }
        return null;
    }

    public Chapter FindChapter(int dlc) {
        Chapter Target = null;
        for (int i = 0; i < Dlcs.Count; i++) {
            if (Dlcs[i].ChapterId == dlc) {
                Target = Dlcs[i];
                break;
            }
        }
        return Target;
    }

    public ModelItem FindModel(int model) {
        ModelItem Target = null;
        for (int i = 0; i < Models.Count; i++) {
            if (Models[i].ModelId == model) {
                Target = Models[i];
                break;
            }
        }
        return Target;
    }

    //把插件放到一个集合里，便于翻页.
    public void CollectAll(bool showInstall = true, int filter = 0) {
        allItem.Clear();
        if (filter == 0) {
            for (int i = 0; i < Dlcs.Count; i++) {
                if (Dlcs[i].Installed && !showInstall)
                    continue;
                allItem.Add(Dlcs[i]);
            }
        } else {
            for (int i = 0; i < Models.Count; i++) {
                if (Models[i].Installed && !showInstall)
                    continue;
                allItem.Add(Models[i]);
            }
        }
    }

    public List<object> allItem = new List<object>();
}
