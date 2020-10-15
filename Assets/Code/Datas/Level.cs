
using ProtoBuf;
/// <summary>
/// 此部分代码由工具生成 
/// 自行修改的内容小心被重新生成的代码覆盖 
/// </summary>
using System.Collections;
using System.Collections.Generic;

namespace Excel2Json{
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class LevelData:ITableItem{
    [ProtoIgnore]
    public int Key { get { return Id; } }
	public int Id;
	public string Scene;
	public string Name;
	public int LevelType;
	public string LevelScript;
	public string BgmName;
	public string StartScript;
	public string sceneItems;
	public string BgTexture;
    public string Desc;
    public string Mission;
}
public partial class LevelDataMgr {
public Dictionary<int,LevelData> LevelDatas;
}
}
