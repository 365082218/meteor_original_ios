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
    private void Awake()
    {
        if (Install != null)
            Install.onClick.AddListener(OnInstall); 
    }
    // Update is called once per frame
    void Update () {
		
	}

    //安装/取消
    void OnInstall()
    {
        if (download == null)
        {
            download = new WebClient();
            download.DownloadProgressChanged += this.DownLoadProgressChanged;
            download.DownloadFileCompleted += this.DownLoadFileCompleted;
            Install.GetComponentInChildren<Text>().text = "取消";
            string downloadUrl = string.Format("http://{0}/meteor/{1}", Main.strHost, Target.Path);
            download.DownloadFile(downloadUrl, Target.LocalPath);
        }
        else
        {
            Install.GetComponentInChildren<Text>().text = "下载";
        }
    }

    void DownLoadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        this.Progress.fillAmount = e.ProgressPercentage / 100.0f;
    }

    void DownLoadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        Install.GetComponentInChildren<Text>().text = "安装中";
        //解压zip然后把skc和贴图保存，然后保存插件的存档.下次进来
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
                Preview.material.mainTexture = tex;
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

    }
}
