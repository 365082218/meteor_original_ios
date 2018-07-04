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
    public Image Background;
    //单机版不更新.
	void Start () {
#if UNITY_ANDROID
        //AndroidWrapper.Init();
#elif UNITY_IOS
        //IosWrapper.Init();
#endif
        GameData.LoadState();
        GameData.InitTable();
        //清理数据
        ResMng.Reload();
        StartCoroutine(LoadData());
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    //如果还没有进入Menu就退出，则还是保存信息.
    public void OnApplicationQuit()
    {
        ClientProxy.Exit();
        WSLog.Uninit();
        FtpLog.Uninit();
        GameData.SaveState();
        //保存升级的临时数据.
        GlobalUpdate.SaveCache();
    }

    //IEnumerator DownLoad()
    //{
    //    GameData.clientVersion.Version = GameData.updateVersion.TargetVersion;
    //    GameData.updateVersion = null;
    //    GameData.SaveState();
    //    GameData.SaveCache();
    //    LoadingProgress.Instance.Close();
    //    ConnectWnd.Instance.Open();
    //    StartCoroutine("UpdateXml");
    //    yield return null;
    //}

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
        //yield return new WaitForEndOfFrame();
        //toProgress = 30;
        //while (displayProgress < toProgress)
        //{
        //    displayProgress++;
        //    progress.fillAmount = (float)displayProgress / 100.0f;
        //    percent.text = displayProgress + "%";
        //    yield return new WaitForEndOfFrame();
        //}
        //在读取character.act后再初始化输入模块。
        ActionInterrupt.Instance.Lines.Clear();
        ActionInterrupt.Instance.Whole.Clear();
        ActionInterrupt.Instance.Root = null;
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
        PoseStatus.Clear();
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
        Application.targetFrameRate = AppInfo.GetTargetFrame();
        WSLog.LogError(string.Format("fps:{0}", Application.targetFrameRate));
        U3D.PlayMovie("start.mv");
        AppDomain.CurrentDomain.UnhandledException += UncaughtException;
        ResMng.LoadScene("Menu");
        SceneManager.LoadScene("Menu");
    }

    void UncaughtException(object sender, UnhandledExceptionEventArgs e)
    { 
        WSLog.LogInfo("UnCaughtException:" + e.ExceptionObject.ToString());
    }
}
