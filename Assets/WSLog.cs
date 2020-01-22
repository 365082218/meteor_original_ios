using UnityEngine;
using System.Collections;
using System.IO;
public class Log
{
    public Log()
    {
        Init();
    }

    public static void Print(string message)
    {
        UnityEngine.Debug.Log(string.Format("message:{0}", message));
    }

    private static FileStream fs;
    public static void Write(string message)
    {
#if !NOLOG
        byte[] line = new byte[2] { (byte)'\r', (byte)'\n' };
        if (fs != null)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(string.Format("debug:{0}", message));
            fs.Write(buffer, 0, buffer.Length);
            fs.Write(line, 0, 2);
            fs.Flush();
        }
#endif
    }

    public static void WriteError(string message)
    {
#if !NOLOG
        byte[] line = new byte[2] { (byte)'\r', (byte)'\n' };
        if (fs != null)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(string.Format("error:{0}", message));
            fs.Write(buffer, 0, buffer.Length);
            fs.Write(line, 0, 2);
            fs.Flush();
        }
#endif
    }

    //static string GetStackTraceModelName()
    //{
    //    //当前堆栈信息
    //    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
    //    System.Diagnostics.StackFrame[] sfs = st.GetFrames();
    //    string _fullName = string.Empty, _methodName = string.Empty;
    //    for (int i = 1; i < sfs.Length - 1; ++i)
    //    {
    //        //非用户代码,系统方法及后面的都是系统调用，不获取用户代码调用结束
    //        if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == sfs[i].GetILOffset()) break;
    //        _methodName = sfs[i].GetMethod().Name;//方法名称
    //        _fullName = _methodName + "()->" + _fullName;
    //    }
    //    return _fullName.TrimEnd('-', '>');
    //}

    //public static void Print(string message)
    //{
    //    switch (level)
    //    {
    //        case LogLevel.Verbose:
    //        case LogLevel.Error:
    //            UnityEngine.Debug.Log(string.Format("{0}_Stack:{1}", message, GetStackTraceModelName()));
    //            break;
    //        case LogLevel.Debug: UnityEngine.Debug.Log(message); break;
    //        case LogLevel.Warn: UnityEngine.Debug.LogWarning(message); break;
    //    }
    //}

    //public static void Print(string message, params object[] param)
    //{
    //    string msg = string.Format(message, param);
    //    Print(msg);
    //}

    private static FileStream fs_thread;
	static void LogFile(string strWarning, string strStackTrace, LogType type)
	{
#if !NOLOG
        if (type == LogType.Error || type == LogType.Exception)
		{
			byte [] line = new byte[2] {(byte)'\r', (byte)'\n'};
			if (fs_thread != null)
			{
				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strWarning);
				fs_thread.Write(buffer, 0, buffer.Length);
				fs_thread.Write(line, 0, 2);
				buffer = System.Text.Encoding.UTF8.GetBytes(strStackTrace);
				fs_thread.Write(buffer, 0, buffer.Length);
				fs_thread.Write(line, 0, 2);
				fs_thread.Flush();
			}
		}
#endif
	}

	public void Init()
	{
#if !NOLOG
        fs = File.Open(string.Format("{0}/{1}_debug.log", Application.persistentDataPath, Application.platform), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        fs_thread = File.Open(string.Format("{0}/{1}_error.log", Application.persistentDataPath, Application.platform), FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
		Application.logMessageReceivedThreaded += LogFile;
#endif
	}

	
	
    public void Uninit()
    {
#if !NOLOG
        if (fs_thread != null)
        {
            lock (fs_thread)
            {
                if (fs_thread != null)
                {
                    fs_thread.Close();
                    fs_thread = null;
                }
            }
        }

        if (fs != null)
        {
            fs.Close();
            fs = null;
        }
#endif
    }      
}
