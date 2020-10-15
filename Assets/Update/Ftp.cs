using System; 
using System.Collections.Generic; 
using System.Text; 
using System.Net; 
using System.IO;
using UnityEngine; 

public static class FtpLog
{
    static List<string> logFile = new List<string>();
    static System.Threading.Thread uploadThread;
    public static void Uninit()
    {
        if (uploadThread != null)
            uploadThread.Abort();
        logFile.Clear();
    }
    public static string UUID;
    public static void UploadStart()
    {
        FileStream fs = null;
        UUID = SystemInfo.deviceUniqueIdentifier;
        string strfile = Application.persistentDataPath + "/" + Application.platform + "_debug.log";
        string strfile2 = Application.persistentDataPath + "/" + Application.platform + "_error.log";
        if (File.Exists(strfile))
        {
            fs = File.Open(strfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (fs == null)
                return;
            if (fs.Length == 0)
            {
                fs.Close();
                return;
            }
        }
        else
        {
            U3D.PopupTip("无日志文件发送");
            return;
        }
        if (fs != null)
            fs.Close();

        DateTime time = DateTime.UtcNow;
        logFile.Clear();
        logFile.Add(strfile);
        logFile.Add(strfile2);

        if (uploadThread == null)
        {
            uploadThread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(FtpLog.UploadLog));
            uploadThread.Start();
        }
    }

    static void UploadLog(object param)
    {
        int fileCount = 0;
        for (int i = 0; i < logFile.Count; i++)
        {
            if (!File.Exists(logFile[i]))
                continue;
            fileCount++;
            FileStream fs = File.Open(logFile[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            FileInfo finfo = new FileInfo(logFile[i]);
            FtpUpload(fs, finfo.Name);
            if (fs != null)
                fs.Close();
            //File.Delete(logFile[i]);
        }
        LocalMsg msg = new LocalMsg();
        msg.Message = (int)LocalMsgType.SendFTPLogComplete;
        msg.Result = 1;
        msg.Param = fileCount;
        TcpProtoHandler.Ins.PostMessage(msg);
        uploadThread = null;
    }

	static void FtpUpload(FileStream fs, string name)
	{
		FtpWebRequest ftp;
		ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://www.idevgame.com/" + UUID + "_" + name));
		ftp.Credentials = new NetworkCredential("winson", "xuwen1013");
		ftp.UsePassive = true;
		ftp.ContentLength = fs.Length;
		ftp.KeepAlive = true; 
		ftp.Method = WebRequestMethods.Ftp.UploadFile;
		ftp.UseBinary = true;
        ftp.Timeout = 10000;
		int buffLength = 2048; 
		byte[] buff = new byte[buffLength];
        System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
        int contentLen;
		Stream sw = null;
        try
        {
            sw = ftp.GetRequestStream();
            contentLen = fs.Read(buff, 0, buffLength);
            while (contentLen != 0)
            {
                sw.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }
            sw.Close();
        }
        catch (Exception e)
        {
            Log.WriteError(string.Format("upload log file exception:{0}", e.Message));
		}
		finally
		{
			if (sw != null)
				sw.Close();
        }
	}
} 