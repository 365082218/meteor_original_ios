 /// <summary>
 /// 此部分代码由工具生成 
 /// 自行修改的内容小心被重新生成的代码覆盖 
 /// </summary>

using System.Collections;
using System.Collections.Generic;

namespace Excel2Json{
public class ModelData:ITableItem{
 public int Key { get { return Id; } }
	public int Id;
	public string Name;
	public float Pivot;
	public float Height;

}
public partial class ModelDataMgr {
public Dictionary<int,ModelData> ModelDatas;
}
}
