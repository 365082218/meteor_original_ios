using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class ClientVersion
{
    public int Version;//存档版本
    public string strVersion;//软件版本
}

public class AppInfo
{
    public const string versionKey = "APPVERSION";
    public static string AppVersion()
    {
        string sVersion = PlayerPrefs.GetString(versionKey);
        if (string.IsNullOrEmpty(sVersion))
            PlayerPrefs.SetString(versionKey, "0.0.7.1");
        return PlayerPrefs.GetString(versionKey);
    }

    public static void SetAppVersion(string v)
    {
        PlayerPrefs.SetString(versionKey, v);
    }
    public const int Version = 20180710;
    public static string MeteorVersion = "9.07";
    //运行帧速率设置 60 = 12 30 = 6 120 = 24
    public static int GetTargetFrame()
    {
        return GameData.gameStatus.TargetFrame;
    }

    public static int GetWaitForNextInput()
    {
        if (GameData.gameStatus.TargetFrame == 30)
            return 12;
        return 24;//2个输入中间最大间隔24帧超过即断开.
    }

#if LOCALHOST
    public static string Domain = "127.0.0.1";
#else
    public static string Domain = "www.idevgame.com";
#endif
    public static ushort GatePort = 7200;
}
