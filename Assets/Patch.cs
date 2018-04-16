using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class Patch : MonoBehaviour {

    public Text percent;
    public Image progress;
    public Text tip;
    public Image border;
    //单机版不更新.
	void Start () {
        GameData.LoadVersion();
        GameData.LoadState();
        StartCoroutine(LoadData());
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void UpdateFile()
    {
        if (GameData.updateVersion == null)
            StartCoroutine("UpdateXml");
        else
            ShowNotice();
    }

    void ShowNotice()
    {
        ConnectWnd.Instance.Close();
        LoadingNotice.Instance.Open();
        string notice = "";
        for (int i = 0; i < GameData.updateVersion.Notices.Count; i++)
            notice += (i + 1) + ":" + GameData.updateVersion.Notices[i] + Environment.NewLine;
        LoadingNotice.Instance.SetNotice(notice, () => {
            LoadingProgress.Instance.Open();
            LoadingProgress.Instance.InitProgress();
            StartCoroutine("DownLoad");
            LoadingNotice.Instance.Close();
        });
    }

    //如果还没有进入Menu就退出，则还是保存信息.
    public void OnApplicationQuit()
    {
        ClientProxy.Exit();
        Log.Uninit();
        FtpLog.Uninit();
        GameData.SaveState();
        GameData.SaveCache();
    }

    IEnumerator UpdateXml()
    {
        WWW xml = new WWW("http://www.idevgame.com/WindowsPlayer/v.zip");
        yield return xml;
        if (xml.error != null)
            StartCoroutine("LoadData");
        else
        {
            //得到当前版本号前往的目的版本号.和所有文件列表
            GameData.updateVersion = ProtoBuf.Serializer.Deserialize<UpdateVersion>(new System.IO.MemoryStream(xml.bytes));
            GameData.SaveCache();
            ShowNotice();
        }
    }

    IEnumerator DownLoad()
    {
        int i = 0;
        while (i < GameData.updateVersion.Total)
        {
            //if (GameData.gameVersion.FileList[i].done)
            {
                i++;
                continue;
            }
            //WWW f = new WWW(GameData.gameVersion.FileList[i].path);
            //if (f.isDone)
            {
                //GameData.gameVersion.FileList[i].done = true;
                i++;
                continue;
            }
            LoadingProgress.Instance.SetProgress(i);
            yield return 0;
        }
        GameData.clientVersion.Version = GameData.updateVersion.TargetVersion;
        GameData.updateVersion = null;
        GameData.SaveState();
        GameData.SaveCache();
        LoadingProgress.Instance.Close();
        ConnectWnd.Instance.Open();
        StartCoroutine("UpdateXml");
        yield return null;
    }

    IEnumerator LoadData()
    {
        percent.gameObject.SetActive(true);
        tip.gameObject.SetActive(true);
        progress.gameObject.SetActive(true);
        border.gameObject.SetActive(true);
        int toProgress = 0;
        int displayProgress = 0;
        if (ConnectWnd.Exist)
            ConnectWnd.Instance.Close();
        SFXLoader.Instance.Init();
        GameData.InitTable();
        yield return new WaitForEndOfFrame();
        toProgress = 30;
        while (displayProgress < toProgress)
        {
            displayProgress++;
            progress.fillAmount = (float)displayProgress / 100.0f;
            percent.text = displayProgress + "%";
            yield return new WaitForEndOfFrame();
        }
        //在读取character.act后再初始化输入模块。
        ActionInterrupt.Instance.Init();
        yield return new WaitForEndOfFrame();
        toProgress = 40;
        while (displayProgress < toProgress)
        {
            displayProgress++;
            progress.fillAmount = (float)displayProgress / 100.0f;
            percent.text = displayProgress + "%";
            yield return new WaitForEndOfFrame();
        }
        MenuResLoader.Instance.Init();
        yield return new WaitForEndOfFrame();
        toProgress = 50;
        while (displayProgress < toProgress)
        {
            displayProgress++;
            progress.fillAmount = (float)displayProgress / 100.0f;
            percent.text = displayProgress + "%";
            yield return new WaitForEndOfFrame();
        }
        //for (int i = 0; i < 20; i++)
        //{
        //    AmbLoader.Ins.LoadCharacterAmb(i);
        //    toProgress++;
        //    while (displayProgress < toProgress)
        //    {
        //        displayProgress++;
        //        progress.fillAmount = (float)displayProgress / 100.0f;
        //        percent.text = displayProgress + "%";
        //        yield return new WaitForEndOfFrame();
        //    }
        //    yield return new WaitForEndOfFrame();
        //}
        AmbLoader.Ins.LoadCharacterAmb();
        toProgress = 100;
        while (displayProgress < toProgress)
        {
            displayProgress++;
            progress.fillAmount = (float)displayProgress / 100.0f;
            percent.text = displayProgress + "%";
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForEndOfFrame();
        Application.targetFrameRate = Global.targetFrame;
        U3D.PlayMovie("start.mv");
        AppDomain.CurrentDomain.UnhandledException += UncaughtException;
        SceneManager.LoadScene("Menu");
    }

    void UncaughtException(object sender, UnhandledExceptionEventArgs e)
    { 
        Log.LogInfo("UnCaughtException:" + e.ExceptionObject.ToString());
    }
}
