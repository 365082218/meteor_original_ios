using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

public interface ITableItem
{
    int Key();
}

public abstract class TableBase
{
	public object GetValue(string name){
		Type type =this.GetType();
		FieldInfo fieldInfo  = type.GetField(name);
		object obj = fieldInfo.GetValue (this);
		return obj;
	}

	public string GetStringValue(string name)
	{
		return Convert.ToString(GetValue(name));
	}

	public int GetIntValue(string name)
	{
		return Convert.ToInt32(GetValue(name));
	}
}


public abstract class TableManager<T> where T : ITableItem
{
    public object TableData { get { return mItemArray; } }
    // the data arrays.
    List<T> mItemArray;
    Dictionary<int, int> mKeyItemMap = new Dictionary<int, int>();

	public virtual void ReLoad(string table, bool fileMode = false)
	{
		mItemArray = TableParser.Parse<T>(table, fileMode).ToList();
		
		// build the key-value map.
		for (int i = 0; i < mItemArray.Count; i++)
			mKeyItemMap[mItemArray[i].Key()] = i;
	}
    // get a item base the key.
    public T GetItem(int key)
    {
        int itemIndex;
        if (mKeyItemMap.TryGetValue(key, out itemIndex))
            return mItemArray[itemIndex];
        return default(T);
    }
	
    // get the item array.
	public T[] GetAllItem()
	{
		return mItemArray.ToArray();
	}

    //当外挂加载一项新配置时.
    public void InsertItem(T item)
    {
        mItemArray.Add(item);
        mKeyItemMap[item.Key()] = mItemArray.Count - 1;
    }
}

//Ex用于在构造函数外加载
public abstract class TableManagerEx<T> where T : ITableItem
{
    // abstract functions need tobe implements.
    public abstract string TableName();
    public object TableData { get { return mItemArray; } }

    // the data arrays.
    List<T> mItemArray;
    Dictionary<int, int> mKeyItemMap = new Dictionary<int, int>();

    // constructor.
    internal TableManagerEx()
    {
    }


    public virtual void ReLoad()
    {
        mItemArray = TableParser.Parse<T>(TableName(), true).ToList();

        // build the key-value map.
        for (int i = 0; i < mItemArray.Count; i++)
            mKeyItemMap[mItemArray[i].Key()] = i;
    }
    // get a item base the key.
    public T GetItem(int key)
    {
        int itemIndex;
        if (mKeyItemMap.TryGetValue(key, out itemIndex))
            return mItemArray[itemIndex];
        return default(T);
    }

    // get the item array.
    public T[] GetAllItem()
    {
        if (mItemArray == null)
            return new T[0];
        return mItemArray.ToArray();
    }

    //当外挂加载一项新配置时.
    public void InsertItem(T item)
    {
        mItemArray.Add(item);
        mKeyItemMap[item.Key()] = mItemArray.Count - 1;
    }
}