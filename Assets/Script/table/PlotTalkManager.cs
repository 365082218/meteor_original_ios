using System;
using System.Collections.Generic;

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]


public class PlotTalkBase : TableBase, ITableItem
{
    public int fuBenId;                             //副本ID
    public int talkId;                              //对话ID
    public int modelId;                             //模型ID
    public int modelPos;                            //模型位置
    public string name;                             //名称
    public string content;                          //对话内容
    public int talkNode;                            //对话节点
    public int monsterId;                           //怪物id
    public int skip;                                //跳过剧情
    public int talkTime;                            //对话时长
    public string vocFile;                          //音频文件
    public string actionId;                            //动作ID
    public float zoomRatio;                      //模型缩放

    public static int MakeKey(int fubenid, int talknode, int talkid)
    {
        return (fubenid << 16) + (talknode << 8) + talkid;
    }
    public int Key() { return MakeKey(fuBenId, talkNode, talkId); }
}

public class PlotTalkManager : TableManager<PlotTalkBase, PlotTalkManager>
{
    public override string TableName() { return "PlotTalk"; }

    public PlotTalkBase GetPlotTalkBaseItem(int fubenid, int talknode, int talkid)
    {
        return GetItem(PlotTalkBase.MakeKey(fubenid, talknode, talkid));
    }

    public int GetPlotTalkMonsterId(int fubenid, int talknode, int talkid)
    {
        PlotTalkBase ptb = GetPlotTalkBaseItem(fubenid, talknode, talkid);
        if (null == ptb) return -1;
        if (null == ptb.monsterId) return -1;
        return ptb.monsterId;
    }
}
