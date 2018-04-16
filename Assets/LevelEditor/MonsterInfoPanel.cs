using UnityEngine;
using System.Collections;

public class MonsterInfoPanel : WsWindowEx
{
	UILabel MonsterInfo;
	public override string strPrefab
	{
		get
		{
			return "MonsterInfoPanel";
		}
	}
	public override void UIInit()
	{
        MonsterInfo = Global.ldaControlX("MonsterInfo", Panel).GetComponent<UILabel>();
		Global.ldaControlX("OK", Panel).GetComponent<UIEventListener>().onClick = delegate(GameObject go) {
			Close();
		};
	}

    public void AttachLevel(int id)
    {
        //string strInfo = "";
        //Level info = LevelMng.Instance.GetItem(id);
        
        //string[] monsterGroup = info.MonsterPools.Split('|');
        //int[,] monster = new int[monsterGroup.Length, 2];
        //for (int i = 0; i < monsterGroup.Length; i++)
        //{
        //    string[] strMonster = monsterGroup[i].Split('#');
        //    monster[i, 0] = System.Convert.ToInt32(strMonster[0]);
        //    monster[i, 1] = System.Convert.ToInt32(strMonster[1]);
        //}

        //for (int i = 0; i < monster.Length / 2; i++)
        //{
        //    MonsterAttrib monInfo = MonsterAttribManager.Instance.GetItem(monster[i, 0]);
        //    if (monInfo != null)
        //    {
        //        strInfo += "怪物id:" + monster[i, 0] + ":名称" + monInfo.name + ":" + monster[i, 1] + "只\r\n";
        //    }
        //}
        //MonsterInfo.text = strInfo;
    }
}
