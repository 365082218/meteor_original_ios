using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
public class PluginCtrl : MonoBehaviour {

    public Image Progress;
    public Image Preview;
    public Text Desc;
    public Button Install;
    public Text Title;
    float wantPercent = 0;
    float currentPercent = 0;
    bool downLoadComplete = false;
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
            Install.GetComponentInChildren<Text>().text = "安装中";
        }
        else if (downLoadComplete && instanllComplete && !showTips)
        {
            Install.GetComponentInChildren<Text>().text = "安装完成";
            U3D.PopupTip("安装成功,角色面板新增角色:" + Target.Name);
            showTips = true;
        }
	}

    //安装/取消
    void OnInstall()
    {
        if (Target.Installed)
            return;

        if (download == null)
        {
            download = new WebClient();
            download.DownloadProgressChanged += this.DownLoadProgressChanged;
            download.DownloadFileCompleted += this.DownLoadFileCompleted;
            Install.GetComponentInChildren<Text>().text = "取消";
            string downloadUrl = string.Format("http://{0}/meteor/{1}", Main.strHost, Target.Path);
            download.DownloadFileAsync(new System.Uri(downloadUrl), Target.LocalPath);
        }
        else
        {
            Install.GetComponentInChildren<Text>().text = "下载";
        }
    }

    void DownLoadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        
        wantPercent = e.ProgressPercentage;
    }

    void DownLoadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Error != null)
        {
            Debug.LogError(e.Error);
            return;
        }

        downLoadComplete = true;
        if (OnUnZipFinish == null)
            OnUnZipFinish = new UnzipCallbackEx(this);
        ZipUtility.UnzipFile(Target.LocalPath, Target.LocalPath.Substring(0, Target.LocalPath.Length - 4), null, OnUnZipFinish);
        //解压zip然后把skc和贴图保存，然后保存插件的存档.下次进来
    }

    public void OnUnZipComplete()
    {
        //扫描该文件夹下的所有图片文件，作为查找skc贴图文件的素材库.
        string [] files = System.IO.Directory.GetFiles(Target.LocalPath.Substring(0, Target.LocalPath.Length - 4));
        Target.resPath = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            Debug.Log(files[i]);
            Target.resPath[i] = files[i].Replace("\\", "/");
        }
        Target.Installed = true;
        GameData.Instance.gameStatus.RegisterModel(Target);
        GameData.Instance.SaveState();
        instanllComplete = true;
    }

    private void OnDestroy()
    {
        if (preview != null)
        {
            preview.CancelAsync();
            preview.Dispose();
            preview = null;
        }

        if (download != null)
        {
            download.CancelAsync();
            download.Dispose();
            download = null;
        }
    }

    WebClient preview;
    WebClient download;
    ModelItem Target;
    public void AttachModel(ModelItem it)
    {
        Target = it;
        string urlIconPreview = string.Format("http://{0}/meteor/{1}", Main.strHost, it.IcoPath);
        try
        {
            preview = new WebClient();
            byte[] bitIcon = preview.DownloadData(urlIconPreview);
            if (bitIcon != null && bitIcon.Length != 0)
            {
                Texture2D tex = new Texture2D(200, 150);
                tex.LoadImage(bitIcon);
                Preview.overrideSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            }
            preview.Dispose();
            preview = null;
        }
        catch
        {
            
        }
        Title.text = it.Name;
        Desc.text = it.Desc ?? "";
        Progress.fillAmount = 0;

        //
        if (GameData.Instance.gameStatus.IsModelInstalled(Target))
        {
            Target.Installed = true;
            Install.GetComponentInChildren<Text>().text = "已安装";
            wantPercent = 100;
        }
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
            Target.OnUnZipComplete();//安装成功
        }
    }
}