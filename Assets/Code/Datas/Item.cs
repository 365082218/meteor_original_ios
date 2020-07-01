 /// <summary>
 /// 此部分代码由工具生成 
 /// 自行修改的内容小心被重新生成的代码覆盖 
 /// </summary>

using System.Collections;
using System.Collections.Generic;

namespace Excel2Json{
public class ItemData:ITableItem{
 public int Key { get { return Idx; } }
	public int Idx;
	public string Name;
	public int MainType;
	public int SubType;
	public int EquipIdx;
	public int UnitId;
	public int Icon;
	public string Desc;
	public int Quality;
	public int HP;
	public int MP;
	public int Damage;
	public int Def;
	public int Speed;
	public int CritDamage;
	public int Crit;
	public int Coin;
	public int Stack;
	public string CNY;
	public int ArmyHP;
	public int ArmyDamage;
	public int ArmyDef;
	public string Script;
	public int Skill;
	public int LevReq;

}
public partial class ItemDataMgr {
public Dictionary<int,ItemData> ItemDatas;
}
}
