using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
public class PluginCtrl : MonoBehaviour {
    [SerializeField]
    Image Progress;
    public Image Preview;
    [SerializeField]
    Text Desc;
    [SerializeField]
    Button Install;
    [SerializeField]
    Image InstallProgress;
    [SerializeField]
    Text Title;
    [SerializeField]
    Button Function;
    float wantPercent = 0;
    float currentPercent = 0;
    bool downLoadComplete = false;
    public int retryNum = 0;//重试次数.手动点下载时，重置为0
    public bool downLoadError = false;
    public bool instanllComplete = false;
    bool showTips = false;
    UnzipCallbackEx OnUnZipFinish;
    private void Awake()
    {
        if (Install != null)
            Install.onClick.AddListener(OnInstall);
    }
    // Update is called once per frame
    void Update () {
		if (currentPercent != wantPercent)
        {
            currentPercent = Mathf.MoveTowards(currentPercent, wantPercent, 5.0f);
            this.Progress.fillAmount = currentPercent / 100.0f;
        }

        if (downLoadComplete && !instanllComplete)
        {
            InstallProgress.GetComponentInChildren<Text>().text = string.Format("安装中{0}%", currentPercent);
        }
        else if (downLoadComplete && instanllComplete && !showTips)
        {
            //InstallProgress.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("UnInstall");//安装完成后，该按钮变为卸载按钮.
            if (Target != null)
                U3D.PopupTip(string.Format("安装成功,角色面板新增角色:{0}", Target.Name));
            if (Chapter != null)
                U3D.PopupTip(string.Format("安装成功,新增资料片:{0}", Chapter.Name));
            showTips = true;
            loading = false;
        }
        else if (downLoadError)
        {
            InstallProgress.GetComponentInChildren<Text>().text = StringUtils.InstallFailed;
            downLoadError = false;
        }
    }

    //安装中
    bool loading = false;
    void OnInstall()
    {
        //处理外挂模型得安装和下载.
        if (Target != null)
        {
            if (Target.Installed)
            {
                //已安装-卸载
                Target.CleanRes();
                Main.Ins.GameStateMgr.gameStatus.UnRegisterModel(Target);
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                loading = false;
                Main.Ins.GameStateMgr.SaveState();
                return;
            }
            if (!loading)
            {
                //未安装-开始下载
                Main.Ins.DlcMng.AddDownloadTask(this);
                Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
                loading = true;
                retryNum = 0;
            }
            else
            {
                //安装中-取消下载
                Main.Ins.DlcMng.RemoveDownloadTask(this);
                loading = false;
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
            }
        }

        if (Chapter != null)
        {
            if (Chapter.Installed)
            {
                Chapter.CleanRes();
                Main.Ins.GameStateMgr.gameStatus.UnRegisterDlc(Chapter);
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
                loading = false;
                Main.Ins.GameStateMgr.SaveState();
                return;
            }
            if (!loading)
            {
                Main.Ins.DlcMng.AddDownloadTask(this);
                Install.GetComponentInChildren<Text>().text = StringUtils.Cancel;
                loading = true;
                retryNum = 0;
            }
            else
            {
                Main.Ins.DlcMng.RemoveDownloadTask(this);
                loading = false;
                Install.GetComponentInChildren<Text>().text = StringUtils.Install;
            }
        }
    }

    public void DownLoadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        wantPercent = e.ProgressPercentage;
    }

    public void DownLoadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            OnFailed();
            Debug.LogError(e.Error);
            return;
        }

        if (e.Cancelled)
        {
            return;
        }

        downLoadComplete = true;
        if (Target != null)
        {
            //解压zip然后把skc和贴图保存，然后保存插件的存档.下次进来
            if (OnUnZipFinish == null)
                OnUnZipFinish = new UnzipCallbackEx(this);
            ZipUtility.UnzipFile(Target.LocalPath, Target.LocalPath.Substring(0, Target.LocalPath.Length - 4), null, OnUnZipFinish);
        }
        
        if (Chapter != null)
        {
            if (OnUnZipFinish == null)
                OnUnZipFinish = new UnzipCallbackEx(this);
            ZipUtility.UnzipFile(Chapter.LocalPath, Chapter.LocalPath.Substring(0, Chapter.LocalPath.Length - 4), null, OnUnZipFinish);
        }
    }

    public void OnUnZipComplete()
    {
        //扫描该文件夹下的所有图片文件，作为查找skc贴图文件的素材库.
        if (Target != null)
        {
            string[] files = System.IO.Directory.GetFiles(Target.LocalPath.Substring(0, Target.LocalPath.Length - 4), "*.*", System.IO.SearchOption.AllDirectories);
            Target.resPath = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                
                Target.resPath[i] = files[i].Replace("\\", "/");
            }
            Target.Installed = true;
            Main.Ins.GameStateMgr.gameStatus.RegisterModel(Target);
        }
        
        if (Chapter != null)
        {
            string[] files = System.IO.Directory.GetFiles(Chapter.LocalPath.Substring(0, Chapter.LocalPath.Length - 4), "*.*", System.IO.SearchOption.AllDirectories);
            Chapter.resPath = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log(files[i]);
                System.IO.FileInfo fi = new System.IO.FileInfo(files[i]);
                if (fi.Extension == ".txt")
                {
                    if (fi.FullName.Contains("npc/") || fi.FullName.Contains("npc\\"))
                    {
                        NpcTemplate template = new NpcTemplate();
                        template.npcTemplate = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
                        template.filePath = files[i].Replace("\\", "/");
                        //如果该文件的父目录是npc,且
                        bool find = false;
                        for (int j = 0; j < Main.Ins.GameStateMgr.gameStatus.pluginNpc.Count; j++)
                        {
                            if (Main.Ins.GameStateMgr.gameStatus.pluginNpc[j].npcTemplate == template.npcTemplate)
                            {
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                        {
                            Main.Ins.GameStateMgr.gameStatus.pluginNpc.Add(template);
                        }
                    }
                }
                Chapter.resPath[i] = files[i].Replace("\\", "/");
            }
            Chapter.Installed = true;
            Main.Ins.GameStateMgr.gameStatus.RegisterDlc(Chapter);
        }
        Main.Ins.GameStateMgr.SaveState();
        instanllComplete = true;
    }

    public void OnFailed()
    {
        U3D.PopupTip(StringUtils.InstallFailed);
        downLoadError = true;
    }

    public void OnUnZipFailed()
    {
        OnFailed();
    }

    private void OnDestroy()
    {
        Main.Ins.DlcMng.RemovePreviewTask(this);
        Main.Ins.DlcMng.RemoveDownloadTask(this);
    }

    WebClient download;
    public ModelItem Target;
    public Chapter Chapter;
    
    public void AttachModel(ModelItem it)
    {
        Target = it;
        Function.gameObject.SetActive(false);
        Install.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        //已经安装
        if (Main.Ins.GameStateMgr.gameStatus.IsModelInstalled(Target))
        {
            Target.Installed = true;
            Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
            wantPercent = 100;
            //从本地读取一张图片，作为预览图
            if (System.IO.File.Exists(it.Preview))
            {
                byte[] array = System.IO.File.ReadAllBytes(it.Preview);
                Texture2D tex = new Texture2D(0, 0);
                tex.LoadImage(array);
                Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }
        }
        else
        {
            
        }
        Title.text = string.Format(StringUtils.ModelName, it.Name);
        Desc.text = it.Desc ?? "";
        Progress.fillAmount = 0;
    }

    public void AttachDlc(Chapter chapter)
    {
        Chapter = chapter;
        Function.onClick.RemoveAllListeners();
        Function.onClick.AddListener(()=> {
            if (Chapter.Installed)
            {
                Main.Ins.CombatData.Chapter = Chapter;
                string tip = "";
                if (!Main.Ins.DlcMng.CheckDependence(Main.Ins.CombatData.Chapter, out tip))
                {
                    Main.Ins.DialogStateManager.ChangeState(Main.Ins.DialogStateManager.LevelDialogState, false);
                }
                else
                {
                    U3D.PopupTip("Dlc依赖\n" + tip);
                }
            }
            else
            {
                U3D.PopupTip("安装后可查看");
            }
        });
        //已经安装
        if (Main.Ins.GameStateMgr.gameStatus.IsDlcInstalled(Chapter))
        {
            Chapter.Installed = true;
            Install.GetComponentInChildren<Text>().text = StringUtils.Uninstall;
            wantPercent = 100;
            //从本地读取一张图片，作为预览图
            if (System.IO.File.Exists(Chapter.Preview))
            {
                byte[] array = System.IO.File.ReadAllBytes(Chapter.Preview);
                Texture2D tex = new Texture2D(0, 0);
                tex.LoadImage(array);
                Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }
        }
        else
        {
            
        }
        Title.text = string.Format(StringUtils.DlcName, Chapter.Name);
        Desc.text = Chapter.Desc ?? "";
        Progress.fillAmount = 0;
    }
}

public class UnzipCallbackEx: ZipUtility.UnzipCallback
{
    PluginCtrl Target;
    public UnzipCallbackEx(PluginCtrl ctrl)
    {
        Target = ctrl;
    }
    /// <summary>
    /// 解压执行完毕后的回调
    /// </summary>
    /// <param name="_result">true表示解压成功，false表示解压失败</param>
    public override void OnFinished(bool _result) {
        if (_result)
        {
            if (Target != null)
                Target.OnUnZipComplete();//安装成功
        }
        else
        {
            if (Target != null)
                Target.OnUnZipFailed();//安装失败，可能是磁盘空间不足

        }
    }
}