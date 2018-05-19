using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppInfo
{
    public static string AppVersion()
    {
        return (string)ScriptMng.ins.GetVariable("AppVersion");
    }
    public const int Version = 20180502;
    public static string MeteorVersion = "9.07";
    //运行帧速率设置 60 = 12 30 = 6 120 = 24
#if UNITY_IOS || UNITY_ANDROID
    public static int waitForNextInput = 10;//2个输入中间最大间隔6帧超过即断开.
    public static int targetFrame = 30;
#elif UNITY_EDITOR
    public static int waitForNextInput = 12;//2个输入中间最大间隔24帧超过即断开.
    public static int targetFrame = 120;
#endif
#if LOCALHOST
    public static string Domain = "127.0.0.1";
#else
    public static string Domain = "www.idevgame.com";
#endif
    public static ushort GatePort = 7200;
}
