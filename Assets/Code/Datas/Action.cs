 /// <summary>
 /// 此部分代码由工具生成 
 /// 自行修改的内容小心被重新生成的代码覆盖 
 /// </summary>

using System.Collections;
using System.Collections.Generic;

namespace Excel2Json{
public class ActionData:ITableItem{
 public int Key { get { return Idx; } }
	public int Idx;
	public string Name;
	public int IgnoreMove;
	public int IgnoreGravity;
	public int IgnoreCollision;
	public int IgnoreXZVelocity;

}
public partial class ActionDataMgr {
public Dictionary<int,ActionData> ActionDatas;
}
}
