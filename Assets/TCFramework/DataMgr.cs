using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using ProtoBuf;

public class DataMgr
{
    private Dictionary<Type, Dictionary<int, object>> m_datasList = new Dictionary<Type, Dictionary<int, object>>();
    private int m_maxNum = 23;
    public void Register()
    {

    }

    public int GetMaxNum()
    {
        return m_maxNum;
    }

    public void LoadAllData()
    {
        LoadData<LevelDatas.LevelDatas, LevelDatas.LevelDatastable>();
        LoadData<InputDatas.InputDatas, InputDatas.InputDatastable>();
        LoadData<ItemDatas.ItemDatas, ItemDatas.ItemDatastable>();
        LoadData<ActionDatas.ActionDatas, ActionDatas.ActionDatastable>();
        LoadData<ModelDatas.ModelDatas, ModelDatas.ModelDatastable>();
        LoadData<WeaponDatas.WeaponDatas, WeaponDatas.WeaponDatastable>();
    }

    public void CleanAllData()
    {
        m_datasList.Clear();
    }

    public void LoadData<TData, TTable>(/*TType _type, TData _data*/)
            where TTable : class, new()
            where TData : class, new()
    {
        string[] _types = typeof(TData).ToString().Split('.');
        string _typeString = _types.Length > 0 ? _types[_types.Length - 1] : _types[0];
        _typeString = string.Concat("Data", "/", _typeString);
        TextAsset _text = Resources.Load<TextAsset>(_typeString);
        OnLoadData<TData, TTable>(_typeString, _text);
    }

    public void OnLoadData<TData, TTable>(string _url, object _data,object param=null)
        where TTable : class, new()
        where TData : class, new()
    {
		if (m_datasList.ContainsKey (typeof(TData)))
		{
            Debuger.Log("加载了相同的数据表格：" + typeof(TData));
            return;
		}

        MemoryStream _stream = new MemoryStream((_data as TextAsset).bytes);
        TTable _tdata = Serializer.Deserialize<TTable>(_stream);
        PropertyInfo[] _infos = _tdata.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        PropertyInfo _info = FindProperyInfoFromName(_infos, "tlist");

        if (null == _info)
            return;

        object _ilist = _info.GetValue(_tdata, null);

        if (null == _ilist)
            return;

        IList value = _ilist as IList;

        if (null == value)
            return;

        foreach (object _obj in value)
        {
            PropertyInfo[] _objInfos = _obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            PropertyInfo _objinfo = FindProperyInfoFromName(_objInfos, "ID");
            if (null == _objinfo)
                return;
            
            int _id = (int)_objinfo.GetValue(_obj, null);
            if (m_datasList.ContainsKey(typeof(TData)))
            {
                m_datasList[typeof(TData)].Add(_id, _obj);
            }
            else
            {
                Dictionary<int, object> _list = new Dictionary<int, object>();
                _list.Add(_id, _obj);
                m_datasList.Add(typeof(TData), _list);
            }

        }
    }

    public Dictionary<int, Tdata> GetDatas<Tdata>()
        where Tdata : class ,new()
    {
        Dictionary<int, Tdata> _temp = new Dictionary<int, Tdata>();
        if (m_datasList.ContainsKey(typeof(Tdata)))
        {
            foreach (int _id in m_datasList[typeof(Tdata)].Keys)
            {
                _temp.Add(_id, m_datasList[typeof(Tdata)][_id] as Tdata);
            }
            return _temp;// m_datasList[typeof(Tdata)] as Dictionary<int, Tdata>;
        }
        return null;
    }

    public List<Tdata> GetDatasArray<Tdata>()
    where Tdata : class, new()
    {
        List<Tdata> ret = new List<Tdata>();
        if (m_datasList.ContainsKey(typeof(Tdata)))
        {
            foreach (int _id in m_datasList[typeof(Tdata)].Keys)
            {
                ret.Add(m_datasList[typeof(Tdata)][_id] as Tdata);
            }
            return ret;
        }
        return null;
    }

    public T GetData<T>(int _id)
        where T :class, new()
    {
        Dictionary<int, object> _data;
        m_datasList.TryGetValue(typeof(T), out _data);
        if (_data != null)
        {
            if (_data.ContainsKey(_id))
                return (T)_data[_id];
        }
        return null;
    }

    public PropertyInfo FindProperyInfoFromName(PropertyInfo[] _infos,string _name)
    {
        for (int i = 0; i < _infos.Length; i++)
        {
            PropertyInfo _info = _infos[i];
            if (_info.Name == _name)
            {
                return _info;
            }
        }
        return null;
    }
}
