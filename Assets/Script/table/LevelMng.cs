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

//决定了入口是从单机任务来，还是开房间，进房间
public enum LevelMode
{
    Teach,//教学
    SinglePlayerTask,//剧情任务
    CreateWorld,//单机-创建世界
    MultiplyPlayer,//联机-看GameMode
}

public enum GameMode
{
    None,//还未进入关卡.
    MENGZHU = 1,//时间限制回合，不分阵营
    Rob = 2,//劫镖
    Defence = 3,//护城
    ANSHA = 4,//分为蝴蝶和流星阵营`每一边人数一般都是8个才开始玩，暗杀有队长和队友，队长脚下有个圈圈，流星阵营是蓝的，蝴蝶阵营是红的，
    //杀死对方队长算胜利，队友死了队长可以复活队友，复活的对友血量只有一半，以地图上的流星蝴蝶阵营的位置为出生点
    SIDOU = 5,//分为蝴蝶和流星阵营，不分队长和队友，死了不能复活。杀死对方全部敌人才算胜利
    Normal = 6,//单机关卡,以路点作为出生点,剧本关卡
}

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class ModelInfo:ITableItem
{
    public int Key() { return ID; }
    public int ID
    {
        get { return ModelId; }
    }
    public string Name;
    public int ModelId;
    //public float Height;
    //public float Pivot;
    public string Path;//资源路径,安装
}

public class ModelMng : TableManager<ModelInfo, ModelMng>
{
    public override string TableName() { return "ModelInfo"; }
}

[ProtoBuf.ProtoContract(ImplicitFields = ProtoBuf.ImplicitFields.AllFields)]
public class Level : ITableItem
{
	public int ID
	{
		get
		{return Id;}
	}
	public int Id;
	public string Scene;
    public string Name;
    List<WayPoint> wayPoints;
    //23关卡前使用旧版本的WP文件，23关卡后使用场景里设置的出生点.
    public List<WayPoint> wayPoint
    {
        get
        {
            string items = sceneItems;
            if (WayMng.Instance != null)
                wayPoints = WayMng.Instance.wayPoints;
            else if (wayPoints == null && !string.IsNullOrEmpty(items))
                wayPoints = WayLoader.Instance.Load(items);
            return wayPoints;
        }
    }
	
    public string sceneItems;//场景脚本&场景物品列表，当场景脚本想使用新物品列表调试时，重载GetDesName();
    //关卡类型. 
    //1:主线剧情-场景物品会序列化并存储状态 
    //2:副本-反复刷-场景物品每次重新刷 
    //3:挑战-奖励（资源物品）
    //4:死斗-胜利获得 失败失去 大量金钱 装备另外算
    //5:限制时长，游戏关卡
    //6:无尽
    public string LevelScript;//对应的关卡副本
    public string StartScript;//对应的开始脚本，用来加载额外的怪物，一般新场景模式才使用这种.旧场景模式会叠加怪物，但是不会处理这些怪物的剧情
    public int LevelType;
    public int Key() { return ID; }
    public string BgmName;
    public string BgTexture;
    public string Param;//通过参数
    public int Pass;//通关条件
    public int Template;//能否单机创建-作为场景模板 1能-即可在创建房间里创建
    //public string Des;
    //public string Comment;
    public int DesType;//读取*.des的种类 0-原作 1-新des 2-不读取des
    public int DisableFindWay;//禁止寻路
};

public class LevelMng : TableManager<Level, LevelMng>
{
    public override string TableName() { return "Level"; }
}
