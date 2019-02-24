using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MicChat
{
    public static float MaxLength = 10.0f;
    public static AudioClip clip;
    /// <summary>
    /// 开始录音
    /// </summary>
    public static bool StartRecord()
    {
        if (Microphone.devices.Length <= 0)
        {
            return false;
        }
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        //最长20秒录音.
        clip = Microphone.Start(null, false, (int)MaxLength, 8000);
        return true;
    }
    /// <summary>
    /// 结束录音
    /// </summary>
    public static void StopRecord(float [] content)
    {
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
            clip.GetData(content, 0);
            clip = null;
        }
    }

    public static int GetClipDataLength()
    {
        if (clip == null)
            return 0;
        int sample = Microphone.GetPosition(null);
        return clip.samples * clip.channels;
    }
}

