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
            Install.GetComponentInChildren<Text>().text = "安装中";
        }
        else if (downLoadComplete && instanllComplete && !showTips)
        {
            Install.GetComponentInChildren<Text>().text = "安装完成";
            if (Target != null)
                U3D.PopupTip("安装成功,角色面板新增角色:" + Target.Name);
            if (Chapter != null)
                U3D.PopupTip("安装成功,新增资料片:" + Chapter.Name);
            showTips = true;
        }
        else if (downLoadError)
        {
            Install.GetComponentInChildren<Text>().text = "安装失败";
            downLoadError = false;
        }
    }

    //安装/取消
    void OnInstall()
    {
        //处理外挂模型得安装和下载.
        if (Target != null)
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

        if (Chapter != null)
        {
            if (Chapter.Installed)
                return;

            if (download == null)
            {
                download = new WebClient();
                download.DownloadProgressChanged += this.DownLoadProgressChanged;
                download.DownloadFileCompleted += this.DownLoadFileCompleted;
                Install.GetComponentInChildren<Text>().text = "取消";
                string downloadUrl = string.Format("http://{0}/meteor/{1}", Main.strHost, Chapter.Path);
                download.DownloadFileAsync(new System.Uri(downloadUrl), Chapter.LocalPath);
            }
            else
            {
                Install.GetComponentInChildren<Text>().text = "下载";
            }
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
            string[] files = System.IO.Directory.GetFiles(Target.LocalPath.Substring(0, Target.LocalPath.Length - 4));
            Target.resPath = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log(files[i]);
                Target.resPath[i] = files[i].Replace("\\", "/");
            }
            Target.Installed = true;
            GameData.Instance.gameStatus.RegisterModel(Target);
        }
        
        if (Chapter != null)
        {
            string[] files = System.IO.Directory.GetFiles(Chapter.LocalPath.Substring(0, Chapter.LocalPath.Length - 4));
            Chapter.resPath = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log(files[i]);
                Chapter.resPath[i] = files[i];
            }
            Chapter.Installed = true;
            GameData.Instance.gameStatus.RegisterDlc(Chapter);
        }
        GameData.Instance.SaveState();
        instanllComplete = true;
    }

    public void OnFailed()
    {
        ReleaseClient();
        U3D.PopupTip("安装失败");
        downLoadError = true;
    }

    public void OnUnZipFailed()
    {
        OnFailed();
    }

    void ReleaseClient()
    {
        if (download != null)
        {
            download.CancelAsync();
            download.Dispose();
            download = null;
        }
    }

    private void OnDestroy()
    {
        if (preview != null)
        {
            preview.CancelAsync();
            preview.Dispose();
            preview = null;
        }

        ReleaseClient();
    }

    WebClient preview;
    WebClient download;
    ModelItem Target;
    Chapter Chapter;
    public void AttachModel(ModelItem it)
    {
        Target = it;
        //已经安装
        if (GameData.Instance.gameStatus.IsModelInstalled(Target))
        {
            Target.Installed = true;
            Install.GetComponentInChildren<Text>().text = "已安装";
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
            string urlIconPreview = string.Format("http://{0}/meteor/{1}", Main.strHost, it.webPreview);
            try
            {
                preview = new WebClient();
                byte[] bitIcon = preview.DownloadData(urlIconPreview);
                if (bitIcon != null && bitIcon.Length != 0)
                {
                    try
                    {
                        System.IO.FileStream fs = new System.IO.FileStream(it.Preview, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                        fs.Write(bitIcon, 0, bitIcon.Length);
                        fs.SetLength(bitIcon.Length);
                        fs.Flush();
                        fs.Close();
                    }
                    catch (System.Exception exp)
                    {
                        Debug.Log(exp.Message);
                    }
                    Texture2D tex = new Texture2D(200, 150);
                    tex.LoadImage(bitIcon);
                    Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                }
                preview.Dispose();
                preview = null;
            }
            catch
            {

            }
        }
        Title.text = it.Name;
        Desc.text = it.Desc ?? "";
        Progress.fillAmount = 0;
    }

    public void AttachDlc(Chapter chapter)
    {
        Chapter = chapter;
        //已经安装
        if (GameData.Instance.gameStatus.IsDlcInstalled(Chapter))
        {
            Chapter.Installed = true;
            Install.GetComponentInChildren<Text>().text = "已安装";
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
            string urlIconPreview = string.Format("http://{0}/meteor/{1}", Main.strHost, Chapter.webPreview);
            try
            {
                preview = new WebClient();
                byte[] bitIcon = preview.DownloadData(urlIconPreview);
                if (bitIcon != null && bitIcon.Length != 0)
                {
                    System.IO.File.WriteAllBytes(Chapter.Preview, bitIcon);
                    Texture2D tex = new Texture2D(200, 150);
                    tex.LoadImage(bitIcon);
                    Preview.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                }
                preview.Dispose();
                preview = null;
            }
            catch
            {

            }
        }
        Title.text = Chapter.Name;
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

    int OwnerType = -1;
    public UnzipCallbackEx(int messageType)
    {
        OwnerType = messageType;
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

        //NPC定义解压完毕.
        if (OwnerType == 0)
            OnScanNpcFiles();
    }

    void OnScanNpcFiles()
    {
        GameData.Instance.gameStatus.pluginNpc.Clear();
        string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath + @"\Plugins\Npc\");
        for (int i = 0; i < files.Length; i++)
        {
            System.IO.FileInfo fi = new System.IO.FileInfo(files[i]);
            if (fi.Extension == ".zip")
                continue;
            NpcTemplate template = new NpcTemplate();
            template.npcTemplate = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
            template.filePath = fi.FullName;
            GameData.Instance.gameStatus.pluginNpc.Add(template);
        }
        GameData.Instance.SaveState();
    }
}