using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//读csv框架，把csv的数据，导入到类的每个成员变量.
public interface TblRoot
{
    void Init();
    void Reload();
}

public class TblCore:Singleton<TblCore>
{
    Dictionary<string, TblRoot> allTable = new Dictionary<string, TblRoot>();
    public void AddTable(string str, TblRoot root)
    {
        if (!allTable.ContainsKey(str))
            allTable.Add(str, root);
    }
    public void Reload()
    {
        string t = "";
        try
        {
            foreach (var each in allTable)
            {
                t = each.Key;
                each.Value.Reload();
            }
        }
        catch
        {
            Debug.LogError(string.Format("table Init Failed:{0}", t));
        }
        Init();
    }
    public void Init()
    {
        string t = "";
        try
        {
            foreach (var each in allTable)
            {
                t = each.Key;
                each.Value.Init();
            }
        }
        catch
        {
            Debug.LogError(string.Format("table Init Failed:{0}", t));
        }
    }
}

public class TblMng<T> : Singleton<TblMng<T>>, TblRoot where T: TblBase
{
    //调试信息
    public int row;
    public int col;
    public Dictionary<int, T> tblMap = new Dictionary<int, T>();
    
    public string Table;
    public TblMng<T> GetTable()
    {
        T obj = System.Activator.CreateInstance<T>();
        TblBase objRet = obj as TblBase;
        if (objRet == null)
            return null;
        if (tblMap.Count != 0)
            return this;
        TblCore.Ins.AddTable(objRet.TableName, this);
        try
        {
            LoadData(objRet.TableName);
        }
        catch
        {
            Debug.LogError(string.Format("{0} parse error line: {1} col :{2}", objRet.TableName, row, col));
            throw new System.Exception("error");
        }
        return this;
    }

    public void Reload()
    {
        tblMap.Clear();
        LoadData(Table);
    }

    public void Init()
    {
        foreach (var each in tblMap)
        {
            each.Value.Init();
        }
    }

    void LoadData(string TableName)
    {
        Table = TableName;
        TextAsset asset = Resources.Load<TextAsset>(TableName);
        if (asset == null)
            return;
        string[] lines = asset.text.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 2)
            return;
        bool findName = false;
        List<string> fieldIdenity = new List<string>();
        string[] eachline = lines[1].Split(new char[] { '\t' });
        for (int i = 0; i < eachline.Length; i++)
        {
            fieldIdenity.Add(eachline[i]);
            //if (eachline[i] == "Name")
            //    findName = true;
        }
        int idx = -1;
        string Name = "";
        for (int i = 2; i < lines.Length; i++)
        {
            row = i + 1;
            eachline = lines[i].Split(new char[] { '\t' });
            T obj = System.Activator.CreateInstance<T>();
            bool emptyRow = false;
            for (int j = 0; j < eachline.Length; j++)
            {
                col = j + 1;
                System.Reflection.FieldInfo field = typeof(T).GetField(fieldIdenity[j]);
                if (j == 0 && fieldIdenity[0].Equals("Idx"))
                {
                    if (eachline[j] == "")
                    {
                        emptyRow = true;
                        break;
                    }
                    idx = int.Parse(eachline[j]);
                }
                if (field != null)
                {
                    if (field.FieldType == typeof(int))
                        field.SetValue(obj, eachline[j] == "" ? 0 : int.Parse(eachline[j]));
                    else if (field.FieldType == typeof(uint))
                        field.SetValue(obj, eachline[j] == "" ? 0 : uint.Parse(eachline[j]));
                    else if (field.FieldType == typeof(float))
                        field.SetValue(obj, eachline[j] == "" ? 0 : float.Parse(eachline[j]));
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValue(obj, eachline[j]);
                        if (findName && field.Name.Equals("Name"))
                            Name = eachline[j];
                    }
                    else if (field.FieldType == typeof(Vector3))
                    {
                        string strv = eachline[j].Trim(new char[] { '\"' });
                        string[] pos = strv.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                        if (pos.Length == 3)
                        {
                            Vector3 vec = new Vector3(float.Parse(pos[0]), float.Parse(pos[1]), float.Parse(pos[2]));
                            field.SetValue(obj, vec);
                        }
                    }
                    else
                        Debug.LogWarning("错误的成员类型,需要手动在此增加其处理.");
                }
            }
            if (emptyRow)
                continue;
            //if (findName && !string.IsNullOrEmpty(Name))
            //{
            //    if (StrValues.ContainsKey(Name))
            //        Debug.LogWarning("tbl: " + TableName + " 包含不唯一的 Name行:" + idx);
            //    StrValues[Name] = obj;
            //}
            tblMap.Add(idx, obj);
        }
    }

    public object GetRowByIdx(int idx)
    {
        if (tblMap.ContainsKey(idx))
            return tblMap[idx];
        return null;
    }

    public List<T> GetFullRow()
    {
        List<T> ret = new List<T>();
        foreach (var each in tblMap)
            ret.Add(each.Value);
        return ret;
    }

    public int Count()
    {
        return tblMap.Count;
    }
}

public class TblBase
{
    public virtual string TableName { get { return ""; }  }
    public virtual void Init() { }
    public virtual TblBase Clone() { return this.MemberwiseClone() as TblBase; }
}


public class ActionBase:TblBase
{
    public override string TableName { get { return "Action"; } }
    public int Idx;
    public int IgnoreMove;//忽略动作位移.
    public int IgnoreGravity;//忽略重力.
    public int IgnoreCollision;//忽略与其他角色的碰撞.
    public int IgnoreXZVelocity;//忽略角色在世界XZ轴速度.
    public int IgnoreXZMove;//忽略动作在XZ轴上得位移.
}