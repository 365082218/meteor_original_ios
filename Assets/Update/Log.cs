using UnityEngine;
using System.Collections;
using System.IO;

public static class Log
{
    static Log()
    {
        Init();
    }

    public static void LogFrame(string message)
    {
        Debug.LogError("f:" + Time.frameCount + ":" + message);
    }

	private static FileStream fs_thread;
	static void LogFile(string strWarning, string strStackTrace, LogType type)
	{
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
	}

	public static void Init()
	{
		fs = File.Open(Application.persistentDataPath + "/" + Application.platform + "_debug.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        fs_thread = File.Open(Application.persistentDataPath + "/" + Application.platform + "_error.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
        fs_ref = File.Open(Application.persistentDataPath + "/" + Application.platform + "_ref.log", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
		Application.logMessageReceivedThreaded += LogFile;
	}

	private static FileStream fs;
	public static void LogInfo(string str)
	{
        if (fs == null)
            return;
		lock(fs)
		{
			byte [] line = new byte[2] {(byte)'\r', (byte)'\n'};
			if (fs != null)
			{
				byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
				fs.Write(buffer, 0, buffer.Length);
				fs.Write(line, 0, 2);
				fs.Flush();
			}
		}
	}

	private static FileStream fs_ref;
	public static void LogRefer(ReferenceNode root, System.Collections.Generic.List<ReferenceNode> refer)
	{
        if (fs_ref == null)
            return;
		lock(fs_ref)
		{
			if (fs_ref != null)
			{
				byte [] head = System.Text.Encoding.UTF8.GetBytes(root.strResources + ": contains begin\r\n");
				fs_ref.Write(head, 0, head.Length);
				byte [] line = new byte[2] {(byte)'\r', (byte)'\n'};
				foreach (var node in refer)
				{
					byte[] buffer = System.Text.Encoding.UTF8.GetBytes(node.strResources);
					fs_ref.Write(buffer, 0, buffer.Length);
					fs_ref.Write(line, 0, 2);
					fs_ref.Flush();
				}

				head = System.Text.Encoding.UTF8.GetBytes(root.strResources + ": contains end\r\n");
				fs_ref.Write(head, 0, head.Length);
                fs_ref.Flush();
			}
		}
	}
	
    public static void Uninit()
    {
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

        if (fs_ref != null)
        {
            fs_ref.Close();
            fs_ref = null;
        }
    }      
}
