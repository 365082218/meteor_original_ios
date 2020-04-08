using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class SKeyFrame
{
    int keyIndex;//关键帧序号
    int frameIndex;//游戏帧序号
}

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class GameRecord
{
    public string RecordVersion = "1.0.0";
    public string Name;//录像名
    public string Mode;//录制时游戏模式
    public int Chapter;//剧本ID-单机为0
    public int Id;//关卡ID-单机有效
    public int RandomSeed;//随机种子
    public List<SKeyFrame> Keyframes;//关键状态同步.保存了全地图在某一帧，所有对象的所有属性.
}

public class FrameRecord : Singleton<FrameRecord> {
    
    public static void WriteFile()
    {

    }
}
