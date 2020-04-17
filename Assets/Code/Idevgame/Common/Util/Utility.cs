using Ionic.Zlib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static byte[] ConvertBytesZlib(byte[] data, CompressionMode compressionMode)
    {
        CompressionMode mode = compressionMode;
        if (mode != CompressionMode.Compress)
        {
            if (mode != CompressionMode.Decompress)
            {
                throw new NotImplementedException();
            }
            return ZlibStream.UncompressBuffer(data);
        }
        return ZlibStream.CompressBuffer(data);
    }

    public static void Zero(GameObject target)
    {
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;
        target.transform.localScale = Vector3.one;
    }
}

public class StringUtils
{
    //读取表就OK了，后续改.
    public static Dictionary<string, string> language = new Dictionary<string, string>();
    public const string Install = "安裝";
    public const string Cancel = "取消";
    public const string InstallFailed = "安裝失敗";
    public const string Uninstall = "卸载";
    public const string ModelName = "「模型-{0}」";
    public const string DlcName = "「剧本-{0}」";
    public const string Startup = "流星正在启动{0}%";
}