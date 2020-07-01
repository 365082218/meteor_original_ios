 /// <summary>
 /// 此部分代码由工具生成 
 /// 自行修改的内容小心被重新生成的代码覆盖 
 /// </summary>

using System.Collections;
using System.Collections.Generic;

namespace Excel2Json{
public class InputData:ITableItem{
 public int Key { get { return Idx; } }
	public int Idx;
	public string Lines;
	public string LinesAir;
	public string InputString;
	public string DelayFrame;
	public string Desc;
	public string Detail;

}
public partial class InputDataMgr {
public Dictionary<int,InputData> InputDatas;
}
}
