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

public class AppInfo:Singleton<AppInfo>
{
    public const string versionKey = "APPVERSION";
    public string AppVersion()
    {
        string sVersion = PlayerPrefs.GetString(versionKey);
        if (string.IsNullOrEmpty(sVersion))
            PlayerPrefs.SetString(versionKey, "0.2.2.1");
        return PlayerPrefs.GetString(versionKey);
    }

    public void SetAppVersion(string v)
    {
        PlayerPrefs.SetString(versionKey, v);
    }
    public const int Version = 20190204;
    public const int ProtocolVersion = 1;//对战协议版本，在网络初始化后，如果服务器协议版本与此数字相等，则可以进行联机对战，否则需要更新到最新版本
    public string MeteorVersion = "9.07";

    public int GetWaitForNextInput()
    {
        return Application.targetFrameRate * 12 / 30;
    }
}
