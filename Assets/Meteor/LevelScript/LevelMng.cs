using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Idevgame.Util;

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
    public string Desc;//关卡任务描述
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
};

public class LevelMng : TableManager<Level>
{
    LevelMng()
    {
        ReLoad(TableName());
    }
    public string TableName() { return "Level"; }

}
