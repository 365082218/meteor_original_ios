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
            Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("InstallIng");
        }
        else if (downLoadComplete && instanllComplete && !showTips)
        {
            Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("UnInstall");//安装完成后，该按钮变为卸载按钮.
            if (Target != null)
                U3D.PopupTip(LanguagesMgr.GetText("Install.Model.Complete", Target.Name));
            if (Chapter != null)
                U3D.PopupTip(LanguagesMgr.GetText("Install.Dlc.Complete", Chapter.Name));
            showTips = true;
            loading = false;
        }
        else if (downLoadError)
        {
            Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("Install.Failed");
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
                Target.CleanRes();
                GameData.Instance.gameStatus.UnRegisterModel(Target);
                Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("Install");
                loading = false;
                GameData.Instance.SaveState();
                return;
            }
            if (!loading)
            {
                //开始下载
                DlcMng.Instance.AddDownloadTask(this);
                Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("Cancel");
                loading = true;
                retryNum = 0;
            }
            else
            {
                //取消下载
                DlcMng.Instance.RemoveDownloadTask(this);
                loading = false;
                Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("Install");
            }
        }

        if (Chapter != null)
        {
            if (Chapter.Installed)
            {
                Chapter.CleanRes();
                GameData.Instance.gameStatus.UnRegisterDlc(Chapter);
                Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("Install");
                loading = false;
                GameData.Instance.SaveState();
                return;
            }
            if (!loading)
            {
                DlcMng.Instance.AddDownloadTask(this);
                Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("Cancel");
                loading = true;
                retryNum = 0;
            }
            else
            {
                DlcMng.Instance.RemoveDownloadTask(this);
                loading = false;
                Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("Install");
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
            GameData.Instance.gameStatus.RegisterModel(Target);
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
                        for (int j = 0; j < GameData.Instance.gameStatus.pluginNpc.Count; j++)
                        {
                            if (GameData.Instance.gameStatus.pluginNpc[j].npcTemplate == template.npcTemplate)
                            {
                                find = true;
                                break;
                            }
                        }
                        if (!find)
                            GameData.Instance.gameStatus.pluginNpc.Add(template);
                    }
                }
                Chapter.resPath[i] = files[i].Replace("\\", "/");
            }
            Chapter.Installed = true;
            GameData.Instance.gameStatus.RegisterDlc(Chapter);
        }
        GameData.Instance.SaveState();
        instanllComplete = true;
    }

    public void OnFailed()
    {
        U3D.PopupTip("安装失败");
        downLoadError = true;
    }

    public void OnUnZipFailed()
    {
        OnFailed();
    }

    private void OnDestroy()
    {
        DlcMng.Instance.RemovePreviewTask(this);
        DlcMng.Instance.RemoveDownloadTask(this);
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
        if (GameData.Instance.gameStatus.IsModelInstalled(Target))
        {
            Target.Installed = true;
            Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("UnInstall");
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
        Title.text = LanguagesMgr.GetText("Dlc.Model", it.Name);
        Desc.text = it.Desc ?? "";
        Progress.fillAmount = 0;
    }

    public void AttachDlc(Chapter chapter)
    {
        Chapter = chapter;
        Function.onClick.RemoveAllListeners();
        Function.onClick.AddListener(()=> {
            Main.Instance.DialogStateManager.ChangeState(Main.Instance.DialogStateManager.DlcInfoDialogState);
        });
        //已经安装
        if (GameData.Instance.gameStatus.IsDlcInstalled(Chapter))
        {
            Chapter.Installed = true;
            Install.GetComponentInChildren<Text>().text = LanguagesMgr.GetText("UnInstall");
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
        Title.text = LanguagesMgr.GetText("Dlc.Level", Chapter.Name);
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