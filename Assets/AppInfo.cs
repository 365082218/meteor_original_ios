using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AppInfo:Singleton<AppInfo>
{
    public const string versionKey = "APPVERSION";
    public string localVer = "0.3.2.1";
    //返回本地版本号是否大于指定版本号，本地版本号即localVer;
    public bool VersionIsEqual(string s)
    {
        if (localVer == s)
            return true;
        return false;
    }
    private bool VersionIsGraterThan(string save)
    {
        string [] local = localVer.Split(new char[] { '.' });
        string[] vsave = save.Split(new char[] { '.' });
        for (int i = 0; i < local.Length; i++)
        {
            int nlocal = int.Parse(local[i]);
            int nsave = int.Parse(vsave[i]);
            if (nlocal != nsave)
                return nlocal > nsave;
        }
        return true;
    }

    public bool AppVersionIsSmallThan(string server)
    {
        if (string.IsNullOrEmpty(server))
            return false;
        string[] local = AppVersion().Split(new char[] { '.' });
        string[] vsave = server.Split(new char[] { '.' });
        for (int i = 0; i < local.Length; i++)
        {
            int nlocal = int.Parse(local[i]);
            int nsave = int.Parse(vsave[i]);
            if (nlocal != nsave)
                return nlocal < nsave;
        }
        return false;
    }

    public string AppVersion()
    {
        string sVersion = PlayerPrefs.GetString(versionKey);
        if (string.IsNullOrEmpty(sVersion))
            PlayerPrefs.SetString(versionKey, localVer);
        else
        {
            if (VersionIsGraterThan(sVersion))
                return localVer;
        }
        return PlayerPrefs.GetString(versionKey);
    }

    public void SetAppVersion(string v)
    {
        PlayerPrefs.SetString(versionKey, v);
    }

    public const int ProtocolVersion = 20201013;//对战协议版本，在网络初始化后，如果服务器协议版本与此数字相等，则可以进行联机对战，否则需要更新到最新版本
    public string MeteorVersion = "9.07";
    public int MeteorV1()
    {
        return (int)MeteorV2();
    }
    public protocol.RoomInfo.MeteorVersion MeteorV2()
    {
        return MeteorVersion.Equals("9.07") ? protocol.RoomInfo.MeteorVersion.V907 : protocol.RoomInfo.MeteorVersion.V107;
    }
    public int LinkDelay()
    {
        return Application.targetFrameRate * 12 / 30;
    }
}
