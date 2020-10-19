//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//public static class MicChat
//{
//    public static int maxRecordTime = 10;
//    public static AudioClip clip;
//    public static int samplingRate = 12000;
//    /// <summary>
//    /// 开始录音
//    /// </summary>
//    public static bool TryStartRecording()
//    {
//        try
//        {
//            Microphone.End(null);
//            clip = Microphone.Start(null, false, maxRecordTime, samplingRate);
//        }
//        catch
//        {
//            return false;
//        }

//        return true;
//    }
//    /// <summary>
//    /// 结束录音
//    /// </summary>
//    public static void EndRecording(out int length, out AudioClip outClip)
//    {
//        int lastPos = Microphone.GetPosition(null);

//        if (Microphone.IsRecording(null))
//        {
//            length = lastPos / samplingRate;
//        }
//        else
//        {
//            length = maxRecordTime;
//        }

//        Microphone.End(null);

//        if (length < 1.0f)
//        {
//            outClip = null;
//            return;
//        }

//        outClip = clip;
//    }

//    public static byte[] GetData(this AudioClip clip)
//    {
//        var data = new float[clip.samples * clip.channels];

//        clip.GetData(data, 0);

//        byte[] bytes = new byte[data.Length * 4];
//        System.Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
//        return Utility.ConvertBytesZlib(bytes, Ionic.Zlib.CompressionMode.Compress);
//    }

//    public static void SetData(this AudioClip clip, byte[] bytes)
//    {
//        bytes = Utility.ConvertBytesZlib(bytes, Ionic.Zlib.CompressionMode.Decompress);

//        float[] data = new float[bytes.Length / 4];
//        System.Buffer.BlockCopy(bytes, 0, data, 0, data.Length);

//        clip.SetData(data, 0);
//    }

//    public static int GetClipDataLength()
//    {
//        if (clip == null)
//            return 0;
//        int sample = Microphone.GetPosition(null);
//        return clip.samples * clip.channels;
//    }
//}

