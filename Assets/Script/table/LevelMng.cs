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

public enum LevelMode
{
    Normal,//单机关卡,以路点作为出生点,剧本关卡
    MENGZHU,//时间限制回合，不分阵营
    ANSHA,//分为蝴蝶和流星阵营`每一边人数一般都是8个才开始玩，暗杀有队长和队友，队长脚下有个圈圈，流星阵营是蓝的，蝴蝶阵营是红的，
    //杀死对方队长算胜利，队友死了队长可以复活队友，复活的对友血量只有一半，以地图上的流星蝴蝶阵营的位置为出生点
    SIDOU,//分为蝴蝶和流星阵营，不分队长和队友，死了不能复活。杀死对方全部敌人才算胜利
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
            string s = sceneItems;
            string des = Global.GScript.GetDesName();//用脚本设定的场景物品列表代替默认场景物品列表
            if (!string.IsNullOrEmpty(des))
                s = des;

            if (wayPoints == null && !string.IsNullOrEmpty(s))
                wayPoints = WayLoader.Instance.Load(s);
            else if (WayMng.Instance != null)
                wayPoints = WayMng.Instance.wayPoints;
            return wayPoints;
        }
    }
	
    public string sceneItems;
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
    public string Param;//通过参数
    public int Pass;//通关条件
};

public class LevelMng : TableManager<Level, LevelMng>
{
    public override string TableName() { return "Level"; }
}
