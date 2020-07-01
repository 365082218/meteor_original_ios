 /// <summary>
 /// 此部分代码由工具生成 
 /// 自行修改的内容小心被重新生成的代码覆盖 
 /// </summary>

using System.Collections;
using System.Collections.Generic;

namespace Excel2Json{
public class WeaponData:ITableItem{
 public int Key { get { return ID; } }
	public int ID;
	public string Name;
	public string WeaponL;
	public string TextureL;
	public string WeaponR;
	public string TextureR;
	public string PosAWL;
	public string PosAWR;
	public string PosBWL;
	public string PosBWR;

}
public partial class WeaponDataMgr {
public Dictionary<int,WeaponData> WeaponDatas;
}
}
