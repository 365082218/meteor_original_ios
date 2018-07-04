using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

public class Main : MonoBehaviour {
	public static Main Ins = null;
#if LOCALHOST
    private static string strHost = "127.0.0.1";
#else
    private static string strHost = "www.idevgame.com";
#endif
    private static string strPort = "80";
	private static string strProjectUrl = "meteor";
#if UNITY_ANDROID
    private static string strPlatform { get { return RuntimePlatform.Android.ToString(); } }
#elif UNITY_IOS
    private static string strPlatform { get { return RuntimePlatform.IPhonePlayer.ToString(); } }
#else
    private static string strPlatform { get { return RuntimePlatform.WindowsPlayer.ToString(); } }
#endif
    private static string strVFileName = "Version.json.zip";
    public static string strVerFile = "Version.json";
    //版本仓库地址
    //http://{192.168.14.163}:{80}/{meteor}/{iphone}/Version.json.zip
    private static string strVFile = "http://{0}:{1}/{2}/{3}/{4}";
	//更新仓库版本路径
	//http://{192.168.14.163}:{80}/{meteor}/{iphone}/{0.0.0.1_0.0.0.2.zip}
	private static string strDirectoryBase = "http://{0}:{1}/{2}/{3}/{4}/{5}";
	private static string strUpdateFile;
	private static string strClientVer = "0.0.0.0";
	private static string strServerVer;
	public static HttpClient UpdateClient = null;
    void OnApplicationQuit()
    {
        //主线程阻塞到下载线程结束.
        HttpManager.Instance.Quit();
        //释放日志占用
        WSLog.Uninit();
        //保存下载进度
        GlobalUpdate.SaveCache();
    }

    private void Awake()
    {
        Ins = this;
        WSLog.LogError("Awake");
        GlobalUpdate.LoadCache();
        GameData.LoadState();
        GameData.InitTable();
        ResMng.Reload();
    }

    void Start()
    {
        ConnectWnd.Instance.Open();
        StartCoroutine(CheckNeedUpdate());
    }

	public void GameStart()
	{
        GlobalUpdate.SaveCache();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Startup");
    }
	
	IEnumerator CheckNeedUpdate()
	{
        //仅在WIFI下可用
		if (Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork)
        {
            WSLog.LogError("download:" + string.Format(strVFile, strHost, strPort, strProjectUrl, strPlatform, strVFileName));
            UnityWebRequest vFile = new UnityWebRequest();
            vFile.url = string.Format(strVFile, strHost, strPort, strProjectUrl, strPlatform, strVFileName);
            vFile.timeout = 2;
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            vFile.downloadHandler = dH;
            yield return vFile.Send();
            if (vFile.isError || vFile.responseCode != 200)
            {
                WSLog.LogError(string.Format("update version file error:{0} or responseCode:{1}", vFile.error, vFile.responseCode));
                vFile.Dispose();
                GameStart();
                yield break;
            }

            if (vFile.downloadHandler.data != null && vFile.downloadedBytes != 0)
            {
                if (File.Exists(ResMng.GetResPath() + "/" + strVFileName))
                    File.Delete(ResMng.GetResPath() + "/" + strVFileName);
                if (File.Exists(ResMng.GetResPath() + "/" + strVerFile))
                    File.Delete(ResMng.GetResPath() + "/" + strVerFile);
                File.WriteAllBytes(ResMng.GetResPath() + "/" + strVFileName, vFile.downloadHandler.data);
                LZMAHelper.DeCompressFile(ResMng.GetResPath() + "/" + strVFileName, ResMng.GetResPath() + "/" + strVerFile);
                vFile.Dispose();
            }
            else
            {
                GameStart();
                yield break;
            }
            List<VersionItem> v = ReadVersionJson(File.ReadAllText(ResMng.GetResPath() + "/" + strVerFile));
            //从版本信息下载指定压缩包
            for (int i = 0; i < v.Count; i++)
            {
                if (v[i].strVersion == AppInfo.AppVersion() && v[i].strVersionMax != v[i].strVersion)
                {
                    GlobalUpdate.ApplyVersion(v[i], this);
                    yield break;
                }
            }

            GameStart();
        }
        else
		{
            WSLog.LogError("Application.internetReachability != NetworkReachability.ReachableViaLocalAreaNetwork");
            GameStart();
            yield break;
		}
	}

    public static List<VersionItem> ReadVersionJson(string json)
    {
        JsonData jd = LitJson.JsonMapper.ToObject(json);
        List<VersionItem> v = new List<VersionItem>();
        for (int i = 0; i < jd.Count; i++)
        {
            VersionItem it = new VersionItem();
            it.strFilelist = jd[i]["strFilelist"].ToString();
            it.strVersion = jd[i]["strVersion"].ToString();
            it.strVersionMax = jd[i]["strVersionMax"].ToString();
            if (jd[i]["zip"] != null)
            {
                it.zip = new UpdateZip();
                it.zip.fileName = jd[i]["zip"]["fileName"].ToString();
                it.zip.Md5 = jd[i]["zip"]["Md5"].ToString();
                it.zip.size = long.Parse(jd[i]["zip"]["size"].ToString());
            }
            v.Add(it);
        }
        return v;
    }

    bool CheckUpdateCompleted(UpdateFile file)
	{
		MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();
        //check md5 and file total
        if (file.bHashChecked)
            return true;
		string strFileName = file.strFile;
		string strMD5 = file.strMd5;
		long LoadByte = file.Loadbytes;
		long TotalByte = file.Totalbytes;

		if (LoadByte != TotalByte || LoadByte == 0)
			return false;

		if (!File.Exists(file.strLocalPath))
			return false;

		FileStream filestream = new FileStream(file.strLocalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		if (filestream == null)
			return false;

		byte[] hash = md5Generator.ComputeHash(filestream);
		string strFileMD5 = System.BitConverter.ToString(hash);
		filestream.Close();

		if (strMD5 != strFileMD5)
		{
			File.Delete(ResMng.GetResPath() + "/" + strFileName);
			return false;
		}
		file.bHashChecked = true;
		return true;
	}


	void Update()
	{
        if (Complete != null)
        {
            ExtractUPK(Complete);
            Complete = null;
        }
    }

    UpdateVersion Complete = null;
    public void StartDownLoad(UpdateVersion zipInfo)
    {
        //先检查一次该下载是否完成
        if (CheckUpdateCompleted(zipInfo.File))
        {
            ExtractUPK(zipInfo);
        }
        else
        {
            StartDownload(zipInfo);
        }
    }

    IEnumerator UpdateProgress()
    {
        long target = 0;
        long source = 0;
        while (true)
        {
            HttpManager.Instance.UpdateInfo();
            if (HttpManager.Instance.TotalBytes != 0)
            {
                target = HttpManager.Instance.LoadBytes;
                string str = "Checking Update..." + string.Format("{0:d}KB/s    {1:d}/{2:d}", HttpManager.Instance.Kbps, 0, 1);
                while (source < target)
                {
                    LoadingNotice.Instance.UpdateProgress((float)source / (float)HttpManager.Instance.TotalBytes, str);
                    source += (target / 10);
                    yield return 0;
                }
                yield return 0;
                if (HttpManager.Instance.LoadBytes == HttpManager.Instance.TotalBytes)
                {
                    LoadingNotice.Instance.UpdateProgress(1.0f, str);
                    yield break;
                }
            }
            yield return 0;
        }
    }

    void StartDownload(UpdateVersion zipInfo)  
	{
        UpdateClient = HttpManager.Instance.Alloc();
        StartCoroutine(UpdateProgress());
        UpdateClient.AddRequest(string.Format(strVFile, strHost, strPort, strProjectUrl, strPlatform, zipInfo.File.strFile), zipInfo.File.strLocalPath, 
            (ref HttpRequest req)=> 
            {
                zipInfo.File.Loadbytes = req.loadBytes;
                zipInfo.File.Totalbytes = req.totalBytes;
                if (req.loadBytes == req.totalBytes && CheckUpdateCompleted(zipInfo.File))
                {
                    if (req.fs != null)
                    {
                        req.fs.Close();
                        req.fs = null;
                    }
                    req.bDone = true;
                    req.Status = TaskStatus.Done;
                    Complete = zipInfo;
                    HttpManager.Instance.Quit();
                }
                else if (req.error != null)
                {
                    //中断连接、断网，服务器关闭，或者http 404 /或者 http1.1发 thunk 或者其他，直接进入游戏。
                    HttpManager.Instance.Quit();
                    LocalMsg msg = new LocalMsg();
                    msg.Message = (int)LocalMsgType.GameStart;
                    ProtoHandler.PostMessage(msg);
                    //GameStart();
                }
            }
            , zipInfo.File.Loadbytes, zipInfo.File);
        UpdateClient.StartDownload();
	}

    void ExtractUPK(UpdateVersion zipInfo)
    {
        WSLog.LogError("ExtractUPK Start");
        try
        {
            string localPak = ResMng.GetUpdateTmpPath() + "/" + Guid.NewGuid().ToString() + ".pak";
            if (File.Exists(localPak))
                File.Delete(localPak);
            LZMAHelper.DeCompressFile(zipInfo.File.strLocalPath, localPak);
            UPKExtra.ExtraUPK(localPak, ResMng.GetResPath());
            AppInfo.SetAppVersion(zipInfo.VersionMax);
            GlobalUpdate.CleanVersion();
            GameStart();
        }
        catch (Exception exp)
        {
            WSLog.LogError(exp.Message + "|" + exp.StackTrace);
        }
        WSLog.LogError("ExtractUPK End");
    }
}
