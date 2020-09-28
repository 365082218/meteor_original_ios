using System.Net.Sockets;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System;

public enum TaskStatus
{
    None = 0,//队列中等待
	Normal = 1,//下载中
    Loaded,//下载完成
    Installed,//解压完毕
    Error,//出错.
    Pause,//暂停中
    Wait,//等待下载中
}

//unity下载器
public class HttpRequest
{
	public HttpRequest(string file, HttpClient parent, HttpClient.OnReceivedDataDelegate call, long seek, string strLocal, UpdateFile src)
	{
		strfile = file;
		owner = parent;
        OnReceivedData = call;
		cbstart = seek;
		cboffset = 0;
        loadBytes = 0;
        totalBytes = src.Totalbytes;
		strLocalPath = strLocal;
        //Source = src;
        fs = null;
	}
    public FileStream fs;
    //public UpdateFile Source;
	public HttpClient.OnReceivedDataDelegate OnReceivedData;
	public string strfile;
	public long cbstart;
	public HttpClient owner;
	public string error;
	//seek point
	public long cboffset;
	public long totalBytes;
	public string strLocalPath;
	public long loadBytes;
	//download order
	public TaskStatus Status = TaskStatus.Normal;
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

	public const int nThreadSlotCount = 1;
	public AutoResetEvent[] autoEvent = new AutoResetEvent[nThreadSlotCount];
	public Thread []ThreadSlot = new Thread[nThreadSlotCount];
	public ManualResetEvent[] Closed = new ManualResetEvent[nThreadSlotCount];
	public bool bContinue = true;
	private ManualResetEvent connectDone = new ManualResetEvent(false);
	long nLastTick = 0;
	public long nLastTotalByte = 0;
	long nLastKpbs = 0;
	static long nUpdateRate = 1;

    public void Update()
    {
        if (nLastTick == 0)
            nLastTick = DateTime.Now.Ticks;
        long nValue = 0;
        TimeSpan elapsedSpan = new TimeSpan(DateTime.Now.Ticks - nLastTick);
        if (elapsedSpan.TotalSeconds >= nUpdateRate)
        {
            nLastTick = DateTime.Now.Ticks;
            int nTimeCount = (int)elapsedSpan.TotalMilliseconds;
            //downloading now or downloading end
            foreach (KeyValuePair<string, HttpRequest> pair in TaskMap)
            {
                nValue += pair.Value.loadBytes;
            }
            long nTmp = nLastTotalByte;
            nLastTotalByte = nValue;
            if (nTimeCount < 1)
            {
                Debug.Log("error");
            }
            nLastKpbs = ((nValue - nTmp) * 1000) / (1024 * nTimeCount);
        }
    }

    public long Kbps
	{
        get { return nLastKpbs; }
	}

    long totalBytes;
    public long TotalBytes()
    {
        if (totalBytes != 0)
            return totalBytes;
        foreach (var each in TaskMap)
            totalBytes += each.Value.totalBytes;
        return totalBytes;
    }

    private void ConnectCallback(IAsyncResult ar) 
	{
		try 
		{
			Socket client = (Socket) ar.AsyncState;
			client.EndConnect(ar);
			
		} 
		catch (Exception e) 
		{
			//WSLog.LogError(e.Message);
		}
		finally
		{
			connectDone.Set();
		}
	}

    public void Quit()
    {
        bContinue = false;
        for (int i = 0; i < ThreadSlot.Length; ++i)
        {
            if (autoEvent[i] == null)
                continue;
            autoEvent[i].Set();
        }
        List<WaitHandle> handle = new List<WaitHandle>();
        for (int i = 0; i < Closed.Length; i++)
        {
            if (Closed[i] == null)
                break;
            handle.Add(Closed[i]);
        }
        //may be in writing file
        if (handle.Count != 0)
        {
            while (true)
            {
                if (!WaitHandle.WaitAll(handle.ToArray(), 500))
                {
                    for (int i = 0; i < ThreadSlot.Length; ++i)
                    {
                        if (autoEvent[i] == null)
                            continue;
                        autoEvent[i].Set();
                    }
                }
                else
                {
                    Log.WriteError("application quit normal return");
                    return;
                }
            }
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

	public void DownLoad(object param)
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
                    foreach (KeyValuePair<string, HttpRequest> each in TaskMap)
                    {
                        if (each.Value.Status != TaskStatus.Normal)
                            continue;
                        if (req == null)
                            req = each.Value;
                        if (strURI == null)
                            strURI = each.Key;
                    }

                    if (req == null)
                        continue;
                    Socket sClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    //http1.1 download start
                    string strHost = "";
                    string strPort = "";
                    string strRelativePath = "";

                    if (!ParseURI(strURI, ref strHost, ref strPort, ref strRelativePath))
                    {
                        req.error = "404";
                        if (req.OnReceivedData != null && !req.bDone)
                            req.OnReceivedData(ref req);
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
                                Log.WriteError("connect time out");
                                req.Status = TaskStatus.Error;
                                continue;
                            }
                        }
                        catch
                        {
                            req.error = "connect failed";
                            if (req.OnReceivedData != null && !req.bDone)
                                req.OnReceivedData(ref req);
                            req.Status = TaskStatus.Error;
                            continue;
                        }

                        if (!sClient.Connected)
                        {
                            req.error = "connect failed";
                            req.OnReceivedData(ref req);
                            req.Status = TaskStatus.Error;
                            continue;
                        }

                        string strSend = string.Format(strHttpVer, strRelativePath, strHost, strPort, req.cbstart);
                        Log.WriteError("send packet:" + strSend);
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
                            if (req.OnReceivedData != null && !req.bDone)
                                req.OnReceivedData(ref req);
                            req.Status = TaskStatus.Error;
                            continue;
                        }
                        nByteRecved = 0;
                        string strHead = System.Text.Encoding.UTF8.GetString(ms.GetBuffer());
                        Log.WriteError("http recv:" + strHead);
                        string strHeadLower = strHead.ToLower();
                        //check http1.1 return code
                        int nReturnCode = 0;
                        long nContentLength = 0;
                        string[] strResponse = new string[nNewLine];
                        char[] split = { '\n' };
                        strResponse = strHeadLower.Split(split);
                        for (int i = 0; i < strResponse.Length; ++i)
                        {
                            if (strResponse[i].Contains("http/"))
                            {
                                string strStatus = strResponse[i];
                                nReturnCode = System.Convert.ToInt32(strStatus.Substring(9, 3));
                                Log.WriteError("http result:" + nReturnCode.ToString());
                            }
                            else if (strResponse[i].Contains("content-length:"))
                            {
                                string strLength = strResponse[i];
                                string[] strSplit = strLength.Split(new char[] { ' ' }, 2);
                                nContentLength = System.Convert.ToInt64(strSplit[1]);
                                if (req.cbstart == 0)
                                {
                                    req.totalBytes = nContentLength;
                                    if (req.OnReceivedData != null && !req.bDone)
                                        req.OnReceivedData(ref req);
                                }
                            }
							else if (strResponse[i].Contains("tranfer-encoding:chunked"))
							{
                                Log.WriteError("error !!! can not read chunked data");
                                req.error = "can not read chunked data";
                                req.Status = TaskStatus.Error;
							}

                            if (nReturnCode != 0 && nContentLength != 0)
                                break;
                        }

                        ms.Close();
                        ms = null;

                        if (!string.IsNullOrEmpty(req.error) || req.Status != TaskStatus.Normal)
                        {
                            if (req.OnReceivedData != null)
                                req.OnReceivedData(ref req);
                            continue;
                        }

                        if (nReturnCode != 206 && nReturnCode != 200)
                        {
                            req.error = nReturnCode.ToString();
                            if (req.OnReceivedData != null && !req.bDone)
                                req.OnReceivedData(ref req);
                            req.Status = TaskStatus.Error;
                            sClient.Close();
                            continue;
                        }

                        if (req.fs == null)
                        {
                            req.fs = File.Open(req.strLocalPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                            if (req.cbstart != 0)
                                req.fs.Seek(req.cbstart, SeekOrigin.Begin);
                        }

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
                            req.fs.Write(memory, 0, nByteRecved);
                            req.fs.Flush();
                            nTotalRecved += nByteRecved;
                            req.cboffset = nTotalRecved;
                            req.loadBytes = nTotalRecved;
                            if (nTotalRecved == nContentLength)
                                break;
                            if (nLoop >= 10)
                            {
                                if (req.OnReceivedData != null && !req.bDone)
                                    req.OnReceivedData(ref req);
                                nLoop = 0;
                            }
                        }
                        
                        sClient.Close();
                        req.cboffset = req.fs.Seek(0, SeekOrigin.Current);
                        req.fs.Flush();
                        req.fs.Close();
                        req.fs = null;
                        if (req.OnReceivedData != null && !req.bDone)
                            req.OnReceivedData(ref req);
                        req.bDone = true;
                    }
                }
            }
            if (close != null)
            {
                Log.WriteError("thread quit signal open");
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

	public void InitThread()
	{
		for (int i = 0; i < ThreadSlot.Length; ++i)
		{
            if (ThreadSlot[i] == null)
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
	}
	
	public delegate void OnReceivedDataDelegate(ref HttpRequest req);
    public Dictionary<string, HttpRequest> TaskMap = new Dictionary<string, HttpRequest>();

	public void Pause()
	{
		foreach (var req in TaskMap)
		{
			req.Value.Status = TaskStatus.Pause;
		}
	}

	public void AddRequest(string file, string localpath, OnReceivedDataDelegate callback, long offset, UpdateFile item)
	{
		if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(localpath))
		{
            Log.WriteError("AddRequest Error Invalid Param");
			return;
		}
		localpath = localpath.Replace("\\", "/");
		int nDirectoryIndex = localpath.LastIndexOf('/');
		Directory.CreateDirectory(localpath.Substring(0, nDirectoryIndex));
		HttpRequest req = null;
		//dont allow request same file again
		if (TaskMap.TryGetValue(file, out req))
		{
            if (req.bDone)
                return;
            else
                req.Status = TaskStatus.Normal;
			return;
		}

		if (!TaskMap.TryGetValue(file, out req))
		{
            req = new HttpRequest(file, this, callback, offset, localpath, item);
			TaskMap.Add(file, req);
		}
		else
		{
			req.Status = TaskStatus.Normal;
		}
        InitThread();
	}

	public bool StartDownload()
	{
		if (TaskMap.Count == 0)
			return false;
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
    static HttpManager _Ins;
    public static HttpManager Instance
    {
        get
        {
            if (_Ins == null)
                _Ins = new HttpManager();
            return _Ins;
        }
    }

    List<HttpClient> clients = new List<HttpClient>();
	public void Quit()
	{
        for (int i = 0; i < clients.Count; i++)
        {
            clients[i].Quit();
        }
        clients.Clear();
	}

    public HttpClient Alloc()
    {
        HttpClient client = new HttpClient();
        clients.Add(client);
        return client;
    }

    public void UpdateInfo()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            clients[i].Update();
        }
    }

    public long Kbps
    {
        get
        {
            long k = 0;
            for (int i = 0; i < clients.Count; i++)
                k += clients[i].Kbps;
            return k;
        }
    }

    public long LoadBytes
    {
        get
        {
            long k = 0;
            for (int i = 0; i < clients.Count; i++)
                k += clients[i].nLastTotalByte;
            return k;
        }
    }

    public long TotalBytes
    {
        get
        {
            long k = 0;
            for (int i = 0; i < clients.Count; i++)
                k += clients[i].TotalBytes();
            return k;
        }
    }
}

