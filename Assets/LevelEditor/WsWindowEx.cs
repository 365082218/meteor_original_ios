using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class WsWindowEx
{
    protected GameObject Panel;
    static Dictionary<string, WsWindowEx> PanelList = new Dictionary<string, WsWindowEx>();
    public virtual string strPrefab
    {
        get
        {
            return "";
        }
    }

    public virtual float GetZAxis()
    {
        return 0;
    }
    static GameObject _Canvas;
    public void Open()
    {
        if (Panel == null)
            Panel = GameObject.Instantiate(Resources.Load(strPrefab, typeof(GameObject))) as GameObject;
        if (_Canvas == null)
            _Canvas = GameObject.Find("Canvas");
        if (_Canvas == null)
            _Canvas = GameObject.Find("Anchor"); 
        Panel.transform.SetParent(_Canvas.transform);
        Panel.transform.localPosition = new Vector3(0, 0, GetZAxis());
        Panel.transform.localScale = new Vector3(1, 1, 1);
        Panel.transform.localRotation = Quaternion.identity;
        Panel.SetActive(true);
        UIInit();
    }


    public void Close()
    {
        if (Panel != null)
        {
            if (PanelList.ContainsKey(strPrefab))
                PanelList.Remove(strPrefab);
            GameObject.DestroyImmediate(Panel);
            Panel = null;
        }
    }


    public virtual void UIInit()
    {

    }


    public static T OpenSinglePanel<T>() where T : WsWindowEx
    {
        WsWindowEx obj = System.Activator.CreateInstance<T>();
        if (PanelList.ContainsKey(obj.strPrefab))
        {
            PanelList[obj.strPrefab].Close();
            PanelList.Remove(obj.strPrefab);
        }
        PanelList.Add(obj.strPrefab, obj);
        obj.Open();
        return (T)obj;
    }

    public static T OpenPanel<T>() where T : WsWindowEx
    {
        WsWindowEx obj = System.Activator.CreateInstance<T>();
        obj.Open();
        return (T)obj;
    }

    public static bool Exist<T>()
    {
        foreach (var each in PanelList)
        {
            if (each.Value.GetType().IsAssignableFrom(typeof(T)))
                return true;
        }
        return false;
    }
}