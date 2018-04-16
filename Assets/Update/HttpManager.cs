using System.Net.Sockets;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System;

public enum RequestStatus
{
	Normal,
	Pause,
}


public class HttpRequest
{
	public HttpRequest(string file, HttpClient parent, HttpClient.cb call, long start, string strLocal, UpdateFile src)
	{
		strfile = file;
		owner = parent;
		callback = call;
		cbstart = start;
		cboffset = 0;
		cbsize = 0;
		strLocalPath = strLocal;
        cell = src;
	}
    public UpdateFile cell;
	public HttpClient.cb callback;
	public string strfile;
	public long cbstart;
	public HttpClient owner;
	public string error;
	//seek point
	public long cboffset;
	public long cbsize;
	public string strLocalPath;
	public long loadBytes = 0;
	//download order
	public RequestStatus order = RequestStatus.Normal;
	public bool bDone = false;
}


public class HttpClient{

	class ThreadParam
	{
		public AutoResetEvent waitfor;
		public ManualResetEvent close;
	}

	public static string strHttpVer = 
			"GET {0} HTTP/1.1\r\n"
			+	"connection:close\r\n" 
			+   "Host:{1}\r\n" 
			+   "port:{2}\r\n"
			+   "Range: bytes={3}-\r\n"
			+	"Cache-Control:no-cache\r\n\r\n";

	public static int nThreadSlotCount = 4;
	public static AutoResetEvent[] autoEvent = new AutoResetEvent[nThreadSlotCount];
	public static Thread []ThreadSlot = new Thread[nThreadSlotCount];
	public static ManualResetEvent[] Closed = new ManualResetEvent[nThreadSlotCount];
	public string strURIBase;
	public static bool bContinue = true;
	private static ManualResetEvent connectDone = new ManualResetEvent(false);
	static long nLastTick = 0;
	static long nLastTotalByte = 0;
	static long nLastKpbs = 0;
	static long nUpdateRate = 2;
	public static long Kbps
	{
		get
		{
			if (nLastTick == 0)
				nLastTick = DateTime.Now.Ticks;
			long nValue = 0;
			TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - nLastTick);
			if (elapsedSpan.TotalSeconds >= nUpdateRate)
			{
				nLastTick = DateTime.Now.Ticks;
				int nTimeCount = (int)elapsedSpan.TotalMilliseconds;
				lock(DownloadMap)
				{
					//downloading now or downloading end
					foreach (KeyValuePair<string, HttpRequest> pair in DownloadMap)
					{
						nValue += pair.Value.loadBytes;
					}
				}
				long nTmp = nLastTotalByte;
				nLastTotalByte = nValue;
				if (nTimeCount < 1)
				{
					Debug.Log("error");
				}
				nLastKpbs = ((nValue - nTmp) * 1000) / (1024 * nTimeCount);
				return nLastKpbs;
			}
			else
				return nLastKpbs;
		}
	}

	public class Condition
	{
		public ResMng.LoadCallback cb;
		public object param;
		public string strResource;
		public ReferenceNode root;
	}

	public static List<Condition> RegisterCondition = new List<Condition>();
	//check callback handler
	public static void RegisterCallback(ResMng.LoadCallback cb, object param, string strMain, ReferenceNode root)
	{
		lock(RegisterCondition)
		{
			Condition check = null;
			foreach (var each in RegisterCondition)
			{
				if (each.strResource == strMain)
				{
					check = each;
					break;
				}
			}

			if (check != null)
			{
				check.cb = cb;
				check.root = new ReferenceNode(root.strResources);
				check.param = param;
			}
			else
			{
				check = new Condition();
				check.cb = cb;
				check.root = new ReferenceNode(root.strResources);
				check.param = param;
				check.strResource = strMain;
				RegisterCondition.Add(check);
			}
		}
	}

	public static void CheckCondition(string strRes)
	{
        if (string.IsNullOrEmpty(strRes))
        {
            Debug.LogError("error:resource is empty");
            return;
        }
		MainLoader.AddDownloadDone(strRes);
		List<Condition> check = null;
		lock (RegisterCondition)
		{
			check = RegisterCondition;
			RegisterCondition = new List<Condition>();
		}

		//another thread call may be invalid so must push it into loader.list to check in update
		if (check != null)
		{
			MainLoader.AddCheck(check);
		}
	}

	private static void ConnectCallback(IAsyncResult ar) 
	{
		try 
		{
			Socket client = (Socket) ar.AsyncState;
			client.EndConnect(ar);
			
		} 
		catch (Exception e) 
		{
			//UnityEngine.Debug.LogError(e.Message);
		}
		finally
		{
			connectDone.Set();
		}
	}

	public static bool ParseURI(string strURI, ref string strAddress, ref string strPort, ref string strRelativePath)
	{
		string strAddressRet;
		string strPortRet;
		string strRelativePathRet;
		string strIPRet;

		string strProtocol = strURI.Substring(0, 7);
		if (strProtocol != "http://")
			return false;

		string strLeft = strURI.Substring(7, strURI.Length - 7);
		int nIndexPort = strLeft.IndexOf(':');
		if (nIndexPort == -1)
		{
			strPortRet = "80";
			int nIndexRelative = strLeft.IndexOf('/');
			if (nIndexRelative != -1)
			{
				strAddressRet = strLeft.Substring(0, nIndexRelative);
				strRelativePathRet = strLeft.Substring(nIndexRelative, strLeft.Length - nIndexRelative);
			}
			else
				return false;
		}
		else
		{
			strAddressRet = strLeft.Substring(0, nIndexPort);
			int nIndexRelative = strLeft.IndexOf('/');
			if (nIndexRelative != -1)
			{
				strPortRet = strLeft.Substring(nIndexPort + 1, nIndexRelative - (nIndexPort + 1));
				strRelativePathRet = strLeft.Substring(nIndexRelative, strLeft.Length - nIndexRelative);
			}
			else
				return false;
		}

		try
		{
			IPHostEntry hostinfo = Dns.GetHostEntry(strAddressRet);
			IPAddress[] aryIP = hostinfo.AddressList;
			strIPRet = aryIP[0].ToString();
		}
		catch
		{
			return false;
		}

		strAddress = strIPRet;
		strPort = strPortRet;
		strRelativePath = UrlEncode(strRelativePathRet);
		return true;
	}

	//http request uri contains invalid value
	public static string UrlEncode(string str)
	{
		string sb = "";
		List<char> filter = new List<char>(){'!','#','$','&','\'','(',')','*','+',',','-','.','/',':',';','=','?','@','_','~'};
		byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //System.Text.Encoding.Default.GetBytes(str)
		for (int i = 0; i < byStr.Length; i++)
		{
			if (filter.Contains((char)byStr[i]))
				sb += (char)byStr[i];
			else if ((char)byStr[i] >= 'a' && (char)byStr[i] <= 'z')
				sb += (char)byStr[i];
			else if ((char)byStr[i] >= 'A' && (char)byStr[i] <= 'Z')
				sb += (char)byStr[i];
			else if ((char)byStr[i] >= '0' && (char)byStr[i] <= '9')
				sb += (char)byStr[i];
			else
				sb += (@"%" + Convert.ToString(byStr[i], 16));
		}
		return sb;
	}

	public static void DownLoad(object param)
	{
		ThreadParam tParam = param as ThreadParam;
		ManualResetEvent close = tParam.close;
		AutoResetEvent Wait = tParam.waitfor;
        try
        {
            while (bContinue)
            {
                Wait.WaitOne();
                HttpRequest req = null;
                string strURI = null;
                while (bContinue)
                {
                    req = null;
                    strURI = null;
                    lock (RequestMap)
                    {
                        if (RequestMap.Count == 0)
                            break;

                        bool bFindOrder = false;
                        foreach (KeyValuePair<string, HttpRequest> each in RequestMap)
                        {
                            if (req == null)
                                req = each.Value;
                            if (strURI == null)
                                strURI = each.Key;

                            if (each.Value != null)
                            {
                                if (each.Value.order != RequestStatus.Pause)
                                {
                                    req = each.Value;
                                    strURI = each.Key;
                                    bFindOrder = true;
                                    break;
                                }
                                else
                                    continue;
                            }
                        }

                        if (strURI != null)
                        {
                            //may be is retry 
							lock (DownloadMap)
							{
	                            if (!DownloadMap.ContainsKey(strURI))
	                                DownloadMap.Add(strURI, req);
							}
                            RequestMap.Remove(strURI);
                        }
                    }

                    Socket sClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    //http1.1 download start
                    string strHost = "";
                    string strPort = "";
                    string strRelativePath = "";

                    if (!ParseURI(strURI, ref strHost, ref strPort, ref strRelativePath))
                    {
                        req.error = "404";
                        req.callback(ref req);
                        break;
                    }
                    else
                    {
                        EndPoint server = new IPEndPoint(IPAddress.Parse(strHost), System.Convert.ToInt32(strPort));
                        try
                        {
                            connectDone.Reset();
                            sClient.BeginConnect(server, new AsyncCallback(ConnectCallback), sClient);
                            connectDone.WaitOne(2000, false);
                            if (!sClient.Connected)
                            {
                                Log.LogInfo("connect time out");
                                //repush to download again
                                lock (RequestMap)
                                {
                                    if (!RequestMap.ContainsKey(strURI))
                                    {
                                        req.error = null;
                                        req.order = 0;
                                        RequestMap.Add(strURI, req);
                                    }
                                }
                                continue;
                            }
                        }
                        catch
                        {
                            req.error = "connect failed";
                            req.callback(ref req);

                            //repush to download again
                            lock (RequestMap)
                            {
                                if (!RequestMap.ContainsKey(strURI))
                                {
                                    req.error = null;
                                    req.order = 0;
                                    RequestMap.Add(strURI, req);
                                }
                            }
                            continue;
                        }

                        if (!sClient.Connected)
                        {
                            req.error = "connect failed";
                            req.callback(ref req);

                            //repush to download again
                            lock (RequestMap)
                            {
                                if (!RequestMap.ContainsKey(strURI))
                                {
                                    req.error = null;
                                    req.order = 0;
                                    RequestMap.Add(strURI, req);
                                }
                            }
                            continue;
                        }
                        string strSend = string.Format(strHttpVer, strRelativePath, strHost, strPort, req.cbstart);
                        Log.LogInfo("send packet:" + strSend);
                        byte[] bySend = System.Text.Encoding.UTF8.GetBytes(string.Format(strHttpVer, strRelativePath, strHost, strPort, req.cbstart));
                        sClient.Send(bySend);
                        int nByteRecved = 0;
                        int nNewLine = 0;
                        //recv http head
                        MemoryStream ms = new MemoryStream();
                        byte[] byteRecved = new byte[1];
                        while (true)
                        {
                            if (!bContinue)
                                break;
							try
							{
                            	nByteRecved = sClient.Receive(byteRecved, 1, 0);
							}
							catch (Exception exp)
							{
								break;
							}
                            if (nByteRecved <= 0)
                                break;
                            ms.Write(byteRecved, 0, 1);
                            if (byteRecved[0] == '\n')
                            {
                                nNewLine++;
                                if (System.Text.Encoding.UTF8.GetString(ms.GetBuffer()).Contains("\r\n\r\n"))
                                    break;
                            }
                        }
                        if (!sClient.Connected || !bContinue)
                        {
                            req.error = "recv interrupt";
                            req.callback(ref req);
                            lock (RequestMap)
                            {
                                if (!RequestMap.ContainsKey(strURI))
                                {
                                    req.error = null;
                                    req.order = 0;
                                    RequestMap.Add(strURI, req);
                                }
                            }
                            continue;
                        }
                        nByteRecved = 0;
                        string strHead = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
                        Log.LogInfo("http recv:" + strHead);
                        string strHeadLower = strHead.ToLower();
                        //check http1.1 return code
                        int nReturnCode = 0;
                        long nContentLength = 0;
                        int nRangeStart = 0;
                        int nRangeStop = 0;
                        string[] strResponse = new string[nNewLine];
                        char[] split = { '\n' };
                        strResponse = strHeadLower.Split(split);
                        for (int i = 0; i < strResponse.Length; ++i)
                        {
                            if (strResponse[i].Contains("http/"))
                            {
                                string strStatus = strResponse[i];
                                nReturnCode = System.Convert.ToInt32(strStatus.Substring(9, 3));
                                Log.LogInfo("http result:" + nReturnCode.ToString());
                            }
                            else if (strResponse[i].Contains("content-length:"))
                            {
                                string strLength = strResponse[i];
                                string[] strSplit = strLength.Split(new char[] { ' ' }, 2);
                                nContentLength = System.Convert.ToInt64(strSplit[1]);
                                if (req.cbstart == 0)
                                {
                                    req.cbsize = nContentLength;
                                    req.callback(ref req);
                                }
                            }
							else if (strResponse[i].Contains("tranfer-encoding:chunked"))
							{
								Log.LogInfo("error !!! can not read chunked data");
							}

                            if (nReturnCode != 0 && nContentLength != 0)
                                break;
                        }

                        ms.Close();
                        ms = null;

                        if (nReturnCode != 206 && nReturnCode != 200)
                        {
                            req.error = nReturnCode.ToString();
                            req.callback(ref req);
                            lock (RequestMap)
                            {
                                if (!RequestMap.ContainsKey(strURI))
                                {
                                    req.error = null;
                                    req.order = 0;
                                    RequestMap.Add(strURI, req);
                                }
                            }
                
                            sClient.Close();
                            continue;
                        }

                        FileStream fs = File.Open(req.strLocalPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                        if (req.cbstart != 0)
                            fs.Seek(req.cbstart, SeekOrigin.Begin);

                        //calc kbps

                        //total recved / total time => kbps
                        long nTotalRecved = req.cbstart;
                        int nLoop = 0;
                        byte[] memory = new byte[10 * 1024];
                        while (true)
                        {
                            long nTickBegin = DateTime.Now.Ticks;
                            if (!bContinue)
                                break;
							try
							{
                            	nByteRecved = sClient.Receive(memory, 10 * 1024, SocketFlags.None);
							}
							catch (Exception exp)
							{
								break;
							}
                            if (nByteRecved <= 0)
                                break;
                            nLoop++;

                            //Loader.LogErr("recv bytes:" + nByteRecved.ToString());
                            fs.Write(memory, 0, nByteRecved);
                            fs.Flush();
                            nTotalRecved += nByteRecved;
                            req.cboffset = nTotalRecved;
                            req.loadBytes += nByteRecved;
                            if (nTotalRecved == nContentLength)
                                break;
                            if (nLoop >= 10)
                            {
                                req.callback(ref req);
                                nLoop = 0;
                            }
                        }
                        
                        sClient.Close();
                        //Loader.LogErr("file transfer result:" + nByteRecved.ToString());
                        req.cboffset = fs.Seek(0, SeekOrigin.Current);
                        fs.Flush();
                        fs.Close();
                        req.callback(ref req);
                        req.bDone = true;
                    }
                }
            }
            if (close != null)
            {
                Log.LogInfo("thread quit signal open");
                close.Set();
                close = null;
            }
        }
        catch (Exception exp)
        {
            Debug.LogError(exp.Message + "|" + exp.StackTrace + " download thread crashed");
        }
        finally
        {
            if (close != null)
            {
                close.Set();
                close = null;
            }
        }
	}

	public static void InitThread()
	{
		for (int i = 0; i < ThreadSlot.Length; ++i)
		{
			autoEvent[i] = new AutoResetEvent(false);
			Closed[i] = new ManualResetEvent(true);
			ThreadSlot[i] = new Thread(new ParameterizedThreadStart(DownLoad));
			ThreadParam param = new ThreadParam();
			param.close = Closed[i];
			param.waitfor = autoEvent[i];
			ThreadSlot[i].Start(param);
		}
	}
	
	public delegate void cb(ref HttpRequest req);
	public HttpClient(string str)
	{
		strURIBase = str;
	}

	public static Dictionary<string, HttpRequest> DownloadMap = new Dictionary<string, HttpRequest>();
	public static Dictionary<string, HttpRequest> RequestMap = new Dictionary<string, HttpRequest>();

	public void ResetOrder()
	{
		lock (RequestMap)
		{
			foreach (var req in RequestMap)
			{
				req.Value.order = RequestStatus.Pause;
			}
		}
	}

	public void AddRequest(string file, string localpath, cb callback, long offset, UpdateFile item)
	{
		if (file == null || file.Length == 0 || localpath == null || localpath.Length == 0)
		{
			//Debug.LogError("AddRequest Error Invalid Param");
			return;
		}
		localpath = localpath.Replace("\\", "/");
		int nDirectoryIndex = localpath.LastIndexOf('/');
		Directory.CreateDirectory(localpath.Substring(0, nDirectoryIndex));
		HttpManager.InitHttpModule();
		HttpRequest req = null;
		//dont allow request same file again
		lock (RequestMap)
		{
			if (DownloadMap.TryGetValue(strURIBase + file, out req))
			{
				if (req.bDone)
					return;
				else
					req.order = RequestStatus.Normal;
				return;
			}

			if (!RequestMap.TryGetValue(strURIBase + file, out req))
			{
                req = new HttpRequest(file, this, callback, offset, localpath, item);
				RequestMap.Add(strURIBase + file, req);
			}
			else
			{
				req.order = RequestStatus.Normal;
			}
		}
	}

	public static bool StartDownload()
	{
		lock (RequestMap)
		{
			if (RequestMap.Count == 0)
				return false;
		}
		for (int i = 0; i < autoEvent.Length; i++)
		{
			if (autoEvent[i] == null)
				continue;
			autoEvent[i].Set();
		}
		return true;
	}
}


public class HttpManager{

	public static bool Inited()
	{
		return bInit;
	}
	static bool bInit = false;
	public static Dictionary<string, HttpClient> ClientMap = new Dictionary<string, HttpClient>();

	public static void InitHttpModule()
	{
		if (bInit)
			return;
		bInit = true;
		HttpClient.InitThread();
	}

	public static void Quit()
	{
		HttpClient.bContinue = false;
		if (!HttpManager.Inited())
			return;
		for (int i = 0; i < HttpClient.ThreadSlot.Length; ++i)
		{
			if (HttpClient.autoEvent[i] == null)
				continue;
			HttpClient.autoEvent[i].Set();
		}
		List<WaitHandle> handle = new List<WaitHandle>();
		for (int i = 0; i < HttpClient.Closed.Length; i++)
		{
			if (HttpClient.Closed[i] == null)
				break;
			handle.Add(HttpClient.Closed[i]);
		}
		//may be in writing file
		if (handle.Count != 0)
		{
			while (true)
			{
				if (!WaitHandle.WaitAll(handle.ToArray(), 500))
				{
					for (int i = 0; i < HttpClient.ThreadSlot.Length; ++i)
					{
						if (HttpClient.autoEvent[i] == null)
							continue;
						HttpClient.autoEvent[i].Set();
					}
				}
				else
				{
					Log.LogInfo("application quit normal return");
					return;
				}
			}
		}
	}

	public static HttpClient AllocClient(string strURI)
	{
		HttpClient Client = null;
		if (ClientMap.TryGetValue(strURI, out Client))
			return Client;

		Client = new HttpClient(strURI);
		ClientMap.Add(strURI, Client);
		return Client;
	}
}

