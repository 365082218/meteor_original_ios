using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.Util;
public class WayLength
{
    public int mode;//0 run 1 jump
    public float length;
}

[Serializable]
public class WayPoint
{
    public int index;
    public Vector3 pos;
    public int size;
    public Dictionary<int, WayLength> link;
}

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class Level : ITableItem
{
	public SmartInt ID
	{
		get
		{return FuBenID;}
	}
	public SmartInt FuBenID;
	public string Scene;
    public string SceneDebug;
    public string Name;
    List<WayPoint> wayPoints;
    //23关卡前使用旧版本的WP文件，23关卡后使用场景里设置的出生点.
    public List<WayPoint> wayPoint
    {
        get
        {
            if (wayPoints == null && !string.IsNullOrEmpty(goodList))
                wayPoints = WayLoader.Instance.Load(goodList);
            else if (WayMng.Instance != null)
                wayPoints = WayMng.Instance.wayPoints;
            return wayPoints;
        }
    }
	
    public string goodList;
    //关卡类型. 
    //1:主线剧情-场景物品会序列化并存储状态 
    //2:副本-反复刷-场景物品每次重新刷 
    //3:挑战-奖励（资源物品）
    //4:死斗-胜利获得 失败失去 大量金钱 装备另外算
    //5:限制时长，游戏关卡
    //6:无尽
    public int Key() { return ID; }
    public string BgmName;
    public string BgTexture;
    public string Param;
    public int Pass;
};

public class LevelMng : TableManager<Level, LevelMng>
{
    public override string TableName() { return "Level"; }
}
