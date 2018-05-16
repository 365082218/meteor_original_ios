using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;
using ProtoBuf;
using System.Xml;
using System.Net;
//此文件功能当前不可用

[ProtoBuf.ProtoContract]
public class UpdateItem
{
	[ProtoBuf.ProtoMember(1)]
	public string strLocalPath;
	[ProtoBuf.ProtoMember(2)]
	public string md5;
    [ProtoBuf.ProtoMember(3)]
    public long size;
}

[ProtoBuf.ProtoContract]
public class UpdateConfig
{
    [ProtoBuf.ProtoIgnore]
    public static Dictionary<string, UpdateFile> keySearch = new Dictionary<string,UpdateFile>();
    [ProtoBuf.ProtoIgnore]
    public static bool bChanged;
	[ProtoBuf.ProtoIgnore]
	public static FileStream fs;
	[ProtoBuf.ProtoMember(1)]
	public string strResourcePath;
	[ProtoBuf.ProtoMember(2)]
	public string strUpdatePath;
	[ProtoBuf.ProtoMember(3)]
	public string strCurrentVer;
	[ProtoBuf.ProtoMember(4)]
	public string strTargetVer;
	//table script bytes
	[ProtoBuf.ProtoMember(5)]
	public List<UpdateFile> mlst
	{
		get;
		set;
	}

	[ProtoBuf.ProtoMember(6)]
	public List<UpdateItem> necessaryData
	{
		get;
		set;
	}
	[ProtoBuf.ProtoMember(7)]
	public List<UpdateItem> unnecessaryData
	{
		get;
		set;
	}
	[ProtoBuf.ProtoMember(8)]
	public List<UpdateItem> res
	{
		get;
		set;
	}
	[ProtoBuf.ProtoMember(9)]
	public List<UpdateItem> reference
	{
		get;
		set;
	}
}

[ProtoBuf.ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class UpdateFile
{
	[ProtoBuf.ProtoMember(1)]
	public string strFile;
	[ProtoBuf.ProtoMember(2)]
	public string strMd5;
	[ProtoBuf.ProtoMember(3)]
	public string strLoadbytes;
	[ProtoBuf.ProtoMember(4)]
	public string strTotalbytes;
	[ProtoBuf.ProtoMember(5)]
	public bool bHashChecked;
	//csl bytes txt
	[ProtoBuf.ProtoMember(6)]
	public bool bForceUpdate;
	public UpdateFile()
	{
		strFile = "";
		strMd5 = "";
		strLoadbytes = "0";
		strTotalbytes = "0";
		bHashChecked = false;
		bForceUpdate = false;
	}
	public UpdateFile(string file, string md5, string loadbytes, string totalbytes, bool bForce)
	{
		strFile = file;
		strMd5 = md5;
		strLoadbytes = loadbytes;
        strTotalbytes = totalbytes;
		bHashChecked = false;
		bForceUpdate = bForce;
        if (!UpdateConfig.keySearch.ContainsKey(file))
            UpdateConfig.keySearch.Add(file, this);
	}
}

public enum LoadStep
{
	Wait = -1,//等待
	ChcekNeedUpdate,//检查是否需要更新
    CannotConnect,//无法链接到服务器
	NotNeedUpdate,//不需要更新
	NeedUpdate,//需要更新
	DownAllDone,//下载完毕
	CanStart,//可以开始
}

public class MainLoader : MonoBehaviour {
	static MainLoader Ins = null;
	static LoadStep state = LoadStep.Wait;
	private static string strHost = "www.idevgame.com";
	private static string strPort = "80";
	private static string strProjectUrl = "meteor";
    private static string strPlatform { get { return Application.platform.ToString(); } }
    private static string strVFileName = "v.zip";

    //vfile
    //http://{192.168.14.163}:{80}/{meteor}/{iphone}/v.zip
    private static string strVFile = "http://{0}:{1}/{2}/{3}/{4}";
	//download base directory
	//http://{192.168.14.163}:{80}/{meteor}/{iphone}/{0.0.0.1}/XXDirectory/XXFile
	private static string strDirectoryBase = "http://{0}:{1}/{2}/{3}/{4}/{5}";
	private static string strUpdateFile;
	private static string strClientVer = "0.0.0.0";
	private static string strServerVer;

	public static string strResource;
	public static string strUpdatePath;
	public static string strUpdateConfig;
	public static HttpClient UpdateClient = null;

	public static UpdateConfig config = null;
	//void OnApplicationQuit() 
	//{
        //主线程阻塞到下载线程结束.
		//HttpManager.Quit();
        //释放日志占用
        //WSLog.Uninit();
        //保存下载进度
		//SaveConfig(true);
	//}

    private void Awake()
    {
        Ins = this;
    }

    void Start()
    {
        Ins = this;
        GameData.LoadState();
        strResource = Application.persistentDataPath + "/Resource";
        strUpdatePath = Application.persistentDataPath + "/Update";
        strUpdateConfig = Application.persistentDataPath + "/" + "config.xml";
        //can not use Application.streamingAssetsPath in other thread so save it
        GameObject.DontDestroyOnLoad(this);
        CreateConfig();
        //检查表是否需要全部资源替换.
        //if (CheckTableCompleted())
        //    TableUpdate();

        Global.ShowLoadingStart();
        state = LoadStep.ChcekNeedUpdate;
        StartCoroutine(RestoreUpdate());
    }

    static void SaveConfig(bool bQuit = false)
	{
		if (UpdateConfig.fs != null && MainLoader.config != null)
		{
            if (!UpdateConfig.bChanged)
            {
                if (bQuit)
                {
                    UpdateConfig.fs.Close();
                    UpdateConfig.fs = null;
                }
                return;
            }
            if (MainLoader.config.mlst.Count != 0)
            {
                UpdateConfig.fs.Seek(0, SeekOrigin.Begin);
                ProtoBuf.Serializer.Serialize<UpdateConfig>(UpdateConfig.fs, MainLoader.config);
                UpdateConfig.fs.Flush();
                UpdateConfig.bChanged = false;
            }
            if (bQuit)
            {
                UpdateConfig.fs.Close();
                UpdateConfig.fs = null;
            }
		}
	}

	static int DownloadCount = 0;
	static int AllCount = 0;
	static int errorCount = 0;

	public static void UpdateTableProgress(ref HttpRequest req)
	{
		if (req.error != null)
		{
			WSLog.LogInfo(req.error);
			errorCount++;
			return;
		}

		//xml sync
		
		if (req.cbsize != 0)
		{
            req.cell.strTotalbytes = req.cbsize.ToString();
			req.cbsize = 0;
            UpdateConfig.bChanged = true;
		}

		if (req.cboffset != 0)
		{
			req.cell.strLoadbytes = req.cboffset.ToString();
            UpdateConfig.bChanged = true;
			if (req.cell.strTotalbytes == req.cboffset.ToString() && req.cell.strTotalbytes != "0")
				DownloadCount++;
		}

        lock (MainLoader.config.mlst)
        {
            if (CheckTableCompleted())
            {
                TableUpdate();
                ChangeStep(LoadStep.DownAllDone);
            }
            SaveConfig();
		}
	}

	static bool DeCompressFile(string inFile, string outFile)
	{
		SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
		FileStream input = new FileStream(inFile, FileMode.Open);
		FileStream output = new FileStream(outFile, FileMode.Create);
		
		// Read the decoder properties
		byte[] properties = new byte[5];
		input.Read(properties, 0, 5);
		
		// Read in the decompress file size.
		byte [] fileLengthBytes = new byte[8];
		input.Read(fileLengthBytes, 0, 8);
		long fileLength = System.BitConverter.ToInt64(fileLengthBytes, 0);
		
		// Decompress the file.
		coder.SetDecoderProperties(properties);
		coder.Code(input, output, input.Length, fileLength, null);
		output.Flush();
		output.Close();
		input.Close();
		return true;
	}

	static bool TableUpdate()
	{
		//move file to resourcefile
		List<string> filelist = new List<string>();
		foreach (var file in MainLoader.config.mlst)
		{
			if (file.bForceUpdate)
				filelist.Add(file.strFile);
		}

		//delete old resource
		foreach (string str in filelist)
		{
			string strReplace = str;
			if (str.EndsWith(".zip"))
				strReplace = str.Substring(0, str.Length - 4);

            //存在旧的也有新的.删除旧的.
			if (File.Exists(strResource + "/" + strReplace) && File.Exists(strUpdatePath + "/" + str))
			{
				try
				{
					File.Delete(strResource + "/" + strReplace);
				}
				catch
				{
					return false;
				}
			}
		}

		//move new resource
		foreach (string str in filelist)
		{
			if (File.Exists(strUpdatePath + "/" + str))
			{
				string strReplace = str.Replace("\\", "/");
				int nDirectoryIndex = strReplace.LastIndexOf("/");

				if (str.EndsWith(".zip"))
				{
					strReplace = str.Substring(0, str.Length - 4);
					//createdirectory
					if (nDirectoryIndex != -1)
						Directory.CreateDirectory(strResource + "/" + strReplace.Substring(0, nDirectoryIndex));
					if (DeCompressFile(strUpdatePath + "/" + str, strResource + "/" + strReplace))
						File.Delete(strUpdatePath + "/" + str);
				}
				else
				{
					if (nDirectoryIndex != -1)
						Directory.CreateDirectory(strResource + "/" + strReplace.Substring(0, nDirectoryIndex));
					File.Move(strUpdatePath + "/" + str, strResource + "/" + strReplace);
				}
			}
		}
		return true;
	}

	static List<HttpClient.Condition> checkList = new List<HttpClient.Condition>();
	static List<string> needDownload = new List<string>();
	static List<string> downloadDone = new List<string>();
	public static void AddDownloadDone(string str)
	{
		lock (downloadDone)
		{
			if (!downloadDone.Contains(str))
				downloadDone.Add(str);
		}

		lock (needDownload)
		{
			if (needDownload.Contains(str))
			{
				needDownload.Remove(str);
			}
		}
	}
	public static void AddCheck(List<HttpClient.Condition> call)
	{
		lock (checkList)
		{
			foreach (var each in call)
			{
				bool bExist = false;
				HttpClient.Condition del = null;
				foreach (var exist in checkList)
				{
					if (exist.strResource == each.strResource)
					{
						bExist = true;
						del = exist;
						break;
					}
				}
				if (!bExist)
					checkList.Add(each);
				else
				{
					try
					{
						if (del != null)
							checkList.Remove(del);
						checkList.Add(each);
					}
					catch
					{
						WSLog.LogInfo("error");
					}
				}
			}
		}
	}

	void CheckCondition()
	{
		lock (downloadDone)
		{
			foreach (var str in downloadDone)
			{
                ResMng.AddDownloadDoneRes(str);
			}
			downloadDone.Clear();
		}

		if (checkList.Count != 0)
			Debug.Log("add");
		if (checkList.Count != 0)
		{
			HttpClient.Condition del = null;
			List<ReferenceNode> download = null;
			lock(checkList)
			{
				foreach (var check in checkList)
				{
					ReferenceNode root = ReferenceNode.GetExistNode(check.root.strResources);
					if (root != null)
					{
                        ResMng.CollectDownloadRes(check.root.strResources, ref download, ref root);
						if (download.Count == 0)
						{
							WSLog.LogInfo("download.count == 0 can load res");
							del = check;
						}
						else
						{
							WSLog.LogInfo("load " + check.strResource + "need extra res" + download.Count);
						}
					}
					else
					{
						del = check;
						break;
					}
				}

				if (del != null)
				{
					WSLog.LogInfo("remove condition:" + del + "resource" + del.strResource);
					checkList.Remove(del);
					if (download != null && download.Count == 0)
					{
						WSLog.LogInfo("call callback to load resources" + del.strResource);
						del.cb(del.param);
					}
				}
			}
			
		}
	}
	
	void CleanRes()
	{
        ResMng.Clean();
	}

	void GameStart()
	{
        ResMng.InitResPath(strResource);
		InvokeRepeating("CheckCondition", 5.0f, 5.0f);
		InvokeRepeating("CleanRes", 60.0f, 60.0f);
		bShowPercent = false;
		Global.ShowLoadingEnd();
	}

	public static void ChangeStep(LoadStep step)
	{
		state = step;
        switch (state)
        {
            case LoadStep.Wait:
                break;
            case LoadStep.ChcekNeedUpdate:
                Ins.StartCoroutine(Ins.RestoreUpdate());
                break;
            case LoadStep.NotNeedUpdate:
                state = LoadStep.Wait;
                Ins.GameStart();
                break;
            case LoadStep.DownAllDone:
                state = LoadStep.CanStart;
                Ins.GameStart();
                break;
            case LoadStep.CannotConnect:
                state = LoadStep.ChcekNeedUpdate;
                
                break;
            default: break;
        }
    }
	
	IEnumerator RestoreUpdate()
	{
		if (Application.internetReachability == NetworkReachability.NotReachable)
		{
			WSLog.LogInfo("connect time out");
            MainLoader.ChangeStep(LoadStep.NotNeedUpdate);
			yield break;
		}
		else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
		{
			//3G Tip
		}
		//Restore DownLoad From Progress.xml
		System.Net.Sockets.Socket sClient = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.IP);
        System.Net.EndPoint server = new System.Net.IPEndPoint(Dns.GetHostAddresses(GameData.Domain)[0], System.Convert.ToInt32(strPort));
		System.Threading.ManualResetEvent connectDone = new System.Threading.ManualResetEvent(false);
		try
		{
			connectDone.Reset();
			sClient.BeginConnect(server, delegate(IAsyncResult ar) {
				try 
				{
					System.Net.Sockets.Socket client = (System.Net.Sockets.Socket) ar.AsyncState;
					client.EndConnect(ar);
				} 
				catch (System.Exception e) 
				{
					WSLog.LogInfo(e.Message + "|" + e.StackTrace);
				}
				finally
				{
					connectDone.Set();
				}
			}
			, sClient);
			//timeout
			if (!connectDone.WaitOne(2000))
			{
				WSLog.LogInfo("connect time out");
                MainLoader.ChangeStep(LoadStep.CannotConnect);
				yield break;
			}
			else
			{
				if (!sClient.Connected)
				{
					WSLog.LogInfo("connect disabled by server");
                    MainLoader.ChangeStep(LoadStep.CannotConnect);
					yield break;
				}
			}
		}
		catch
		{
			sClient.Close();
            MainLoader.ChangeStep(LoadStep.CannotConnect);
			yield break;
		}
		finally
		{
			connectDone.Close();
		}
		//WSLog.LogInfo("download:" + string.Format(strVFile, strHost, strPort, strProjectUrl, strPlatform, strVFileName));
		using (WWW vFile = new WWW (string.Format(strVFile, strHost, strPort, strProjectUrl, strPlatform, strVFileName)))
		{
			//WSLog.LogInfo("error:" + vFile.error);
			yield return vFile;
			if (vFile.error != null && vFile.error.Length != 0)
			{
				//WSLog.LogInfo("error " + vFile.error);
				vFile.Dispose();
                //not have new version file
                //can continue game
                MainLoader.ChangeStep(LoadStep.NotNeedUpdate);
				yield break;
			}
			if (vFile.bytes != null && vFile.bytes.Length != 0)
			{
				File.WriteAllBytes(strUpdatePath + "/" + "v.zip", vFile.bytes);
				DeCompressFile(strUpdatePath + "/" + "v.zip", strUpdatePath + "/" + "v.xml");
				vFile.Dispose();
			}
			else
			{
                MainLoader.ChangeStep(LoadStep.NotNeedUpdate);
				yield break;
			}
		}

		XmlDocument xmlVer = new XmlDocument();
		xmlVer.Load(strUpdatePath + "/" + "v.xml");
		XmlElement ServerVer = xmlVer.DocumentElement;
		if (ServerVer != null)
		{
			string strServer = ServerVer.GetAttribute("ServerV");
			UpdateClient = HttpManager.AllocClient(string.Format(strDirectoryBase, strHost, strPort, strProjectUrl, strPlatform, strServer, ""));
			//if (strServer != null && GameData.Version().CompareTo(strServer) == -1)
			//{
			//	strServerVer = strServer;
			//	foreach (XmlElement item in ServerVer)
			//	{
			//		string strClientV = item.GetAttribute("ClientV");
			//		if (strClientV == GameData.Version())
			//		{
			//			strUpdateFile = item.GetAttribute("File");
			//			break;
			//		}
			//	}

			//	if (strUpdateFile != null && strUpdateFile.Length != 0)
			//		StartCoroutine("DownloadNecessaryData");
			//}
			//else
			//{
			//	WSLog.LogInfo("not need update");
   //             MainLoader.ChangeStep(LoadStep.NotNeedUpdate);
			//}
		}
	}

	static bool CheckTableCompleted()
	{
		if (MainLoader.config.mlst == null || MainLoader.config.mlst.Count == 0)
			return false;

		int nTotalFile = 0;
		MD5CryptoServiceProvider md5Generator = new MD5CryptoServiceProvider();
		//check md5 and file total
		foreach (var file in MainLoader.config.mlst)
		{
			if (!file.bForceUpdate)
				continue;
			++nTotalFile;
            if (file.bHashChecked)
                continue;
			string strFileName = file.strFile;
			string strMD5 = file.strMd5;
			string strLoadByte = file.strLoadbytes;
			string strTotalByte = file.strTotalbytes;

			if (strLoadByte != strTotalByte || strLoadByte == "0")
				return false;

			if (!File.Exists(strUpdatePath + "/" + strFileName))
				return false;

			FileStream filestream = new FileStream(strUpdatePath + "/" + strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			if (filestream == null)
				return false;

			byte[] hash = md5Generator.ComputeHash(filestream);
			string strFileMD5 = System.BitConverter.ToString(hash);
			filestream.Close();

			if (strMD5 != strFileMD5)
			{
				File.Delete(strUpdatePath + "/" + strFileName);
				file.strLoadbytes = "0";
				return false;
			}
			file.bHashChecked = true;
            UpdateConfig.bChanged = true;
		}
		if (nTotalFile == 0)
			return false;
		return true;
	}

//	static void ResetProgressXml()
//	{
//		XmlDocument xmlProgress = new XmlDocument();
//		XmlElement xmlProgressRoot = xmlProgress.CreateElement("ProgressInfo");
//		xmlProgress.AppendChild(xmlProgressRoot);
//		xmlProgressRoot.SetAttribute("TargetVer", "UnKnown");
//		xmlProgressRoot.SetAttribute("TotalFileNum", "0");
//		xmlProgress.Save(strUpdateProgress);
//	}

	void CreateConfig()
	{
		if (!File.Exists(strUpdateConfig))
		{
   //         MainLoader.config = new UpdateConfig();
			//UpdateConfig.fs = File.Open(strUpdateConfig, FileMode.Create, FileAccess.ReadWrite);
   //         MainLoader.config.strResourcePath = strResource;
   //         MainLoader.config.strUpdatePath = strUpdatePath;
   //         MainLoader.config.strCurrentVer = GameData.Version();
   //         MainLoader.config.strTargetVer = "";
			//System.IO.Directory.CreateDirectory(strResource);
			//System.IO.Directory.CreateDirectory(strUpdatePath);
   //         MainLoader.config.mlst = new List<UpdateFile>();
   //         UpdateConfig.keySearch.Clear();
		}
		else
		{
			//check config is right
            UpdateConfig.fs = null;
			try
			{
				UpdateConfig.fs = File.Open(strUpdateConfig, FileMode.Open, FileAccess.ReadWrite);
                MainLoader.config = ProtoBuf.Serializer.Deserialize<UpdateConfig>(UpdateConfig.fs);
				if (MainLoader.config.mlst == null)
				{
					UpdateConfig.fs.Close();
                    UpdateConfig.fs = null;
					throw new Exception("need rebuild");
				}
                else
                {
                    foreach (var item in MainLoader.config.mlst)
                    {
                        if (!UpdateConfig.keySearch.ContainsKey(item.strFile))
                        {
                            UpdateConfig.keySearch.Add(item.strFile, item);
                        }
                    }
                }
			}
			catch (System.Exception exp)
			{
    //            if (UpdateConfig.fs != null)
    //            {
    //                UpdateConfig.fs.Close();
    //                UpdateConfig.fs = null;
    //            }

				//WSLog.LogInfo(exp.Message + "" + exp.StackTrace);
				//File.Delete(strUpdateConfig);
    //            MainLoader.config = new UpdateConfig();
				//UpdateConfig.fs = File.Open(strUpdateConfig, FileMode.Create, FileAccess.ReadWrite);
    //            MainLoader.config.strResourcePath = strResource;
    //            MainLoader.config.strUpdatePath = strUpdatePath;
    //            MainLoader.config.strCurrentVer = GameData.Version();
    //            MainLoader.config.strTargetVer = "";
				//System.IO.Directory.CreateDirectory(strResource);
				//System.IO.Directory.CreateDirectory(strUpdatePath);
    //            MainLoader.config.mlst = new List<UpdateFile>();
    //            UpdateConfig.keySearch.Clear();
			}
		}
	}




	bool bShowPercent = true;
	static bool bInLoadLevel = false;
	static int nNeedDownload = 0;
	public static void StartLoadLevel(List<ReferenceNode> need)
	{
		lock (needDownload)
		{
			foreach (var node in need)
			{
				if (!needDownload.Contains(node.strResources))
					needDownload.Add(node.strResources);
			}
			nNeedDownload = needDownload.Count;
		}

		bInLoadLevel = true;
		if (LoadingWnd.Exist)
			LoadingWnd.Instance.Show();
	}

	public static void OnLoadLevelEnd()
	{
		nNeedDownload = 0;
		bInLoadLevel = false;
		if (LoadingWnd.Exist)
			LoadingWnd.Instance.Close();
	}

	void Update()
	{
		if (EmptyForLoadingWnd.Exist && bShowPercent)
        {
			string str = "Checking Update..." + string.Format("{0:d}KB/s    {1:d}/{2:d}", HttpClient.Kbps, DownloadCount, AllCount);
			EmptyForLoadingWnd.Instance.Progress(str);
        }
		if (bInLoadLevel && LoadingWnd.Exist && nNeedDownload != 0)
		{
			LoadingWnd.Instance.UpdateProgress(nNeedDownload - needDownload.Count, nNeedDownload, HttpClient.Kbps);
		}
	}
	
	IEnumerator DownloadNecessaryData()  
	{
		//check data is ok
		//if (strServerVer == null || GameData.Version().CompareTo(strServerVer) != -1)
		//	yield break;

		//check last download is interrupt
		if (MainLoader.config != null && MainLoader.config.mlst.Count != 0)
		{
			string strTargetV = MainLoader.config.strTargetVer;
			//target ver == server ver restore from last state
			if (strTargetV == strServerVer)
			{
				foreach (var item in MainLoader.config.mlst)
				{
					if (!string.IsNullOrEmpty(item.strLoadbytes) && item.strTotalbytes == item.strLoadbytes && item.strTotalbytes != "0")
					{
						if (item.bHashChecked)
                            continue;
					}
					if (!string.IsNullOrEmpty(item.strFile) && item.bForceUpdate)
						UpdateClient.AddRequest(item.strFile, strUpdatePath + "/" + item.strFile, new HttpClient.cb(UpdateTableProgress), System.Convert.ToInt64(item.strLoadbytes), item);
				}

				AllCount = HttpClient.RequestMap.Count;
				if (AllCount != 0)
				{
                    MainLoader.ChangeStep(LoadStep.NeedUpdate);
					errorCount = 0;
					DownloadCount = 0;
					HttpClient.StartDownload();
				}
				else
                    MainLoader.ChangeStep(LoadStep.NotNeedUpdate);
				yield break;
			}
		}
		//download 0.0.0.0-0.0.0.1.xml
		string strUrl = string.Format(strVFile, strHost, strPort, strProjectUrl, strPlatform, strUpdateFile);
		string strLocalFile = strUpdatePath + "/" + strUpdateFile;
		using (WWW updateXml = new WWW(strUrl))
		{
			yield return updateXml;
			File.WriteAllBytes(strLocalFile, updateXml.bytes);
			DeCompressFile(strLocalFile, strUpdatePath + "/" + "update.xml");
			XmlDocument xmlVer = new XmlDocument();
			xmlVer.Load(strUpdatePath + "/" + "update.xml");
			XmlElement updateFilelst = xmlVer.DocumentElement;
			if (updateFilelst != null)
			{
				string strFileNum = updateFilelst.GetAttribute("Filenum");
				List<UpdateFile> data = new List<UpdateFile>();
				foreach (XmlElement each in updateFilelst)
				{
					string strFileName = each.GetAttribute("name");
					string strMD5 = each.GetAttribute("MD5");

					bool bFind = false;
					UpdateFile res = null;
					foreach (var item in MainLoader.config.mlst)
					{
						if (item.strFile == strFileName)
						{
							bFind = true;
							break;
						}
					}

					if (bFind && res != null)
					{
						res.strMd5 = strMD5;
						res.bHashChecked = false;
						res.strLoadbytes = "0";
						res.strTotalbytes = "0";

                        //下载的必要数据包括在Resources目录内的文件，以及以.zip结束的文件（表格或者数据或脚本）
                        string[] strResourceDirectory = strFileName.Split('/');
                        bool bForce = false;
                        foreach (var eachDir in strResourceDirectory)
                        {
                            if (eachDir == "Resources")
                            {
                                bForce = true;
                                break;
                            }
                        }
                        if (strFileName.EndsWith(".zip"))
                            bForce = true;
                        res.bForceUpdate = bForce;
						data.Add(res);
					}
					else
					{
                        string[] strResourceDirectory = strFileName.Split('/');
                        bool bForce = false;
                        foreach (var eachDir in strResourceDirectory)
                        {
                            if (eachDir == "Resources")
                            {
                                bForce = true;
                                break;
                            }
                        }
                        if (strFileName.EndsWith(".zip"))
                            bForce = true;

                        UpdateFile file = new UpdateFile(strFileName, strMD5, "0", "0", bForce);
						data.Add(file);
					}
				}

                MainLoader.config.mlst = data;
                MainLoader.config.strTargetVer = strServerVer;
				foreach (var item in MainLoader.config.mlst)
				{
                    if (!UpdateConfig.keySearch.ContainsKey(item.strFile))
                        UpdateConfig.keySearch.Add(item.strFile, item);
					if (item.strLoadbytes != "0" && item.strLoadbytes == item.strTotalbytes)
						continue;
					if (item.bForceUpdate)
						UpdateClient.AddRequest(item.strFile, strUpdatePath + "/" + item.strFile, new HttpClient.cb(UpdateTableProgress), Convert.ToInt64(item.strLoadbytes), item);
				}

				AllCount = HttpClient.RequestMap.Count;
				if (AllCount != 0)
				{
					int nUnNecessCount = Convert.ToInt32(strFileNum) - AllCount;
                    MainLoader.ChangeStep(LoadStep.NeedUpdate);
					errorCount = 0;
					DownloadCount = 0;
					HttpClient.StartDownload();
				}
				else
                    MainLoader.ChangeStep(LoadStep.NotNeedUpdate);
			}
		}
	}  
}
