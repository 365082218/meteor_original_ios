using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//一些自己会关闭的弹窗提示等.
public class WsWindow
{
    public static string PopupTip = "PopupTip";//弹出提示.
    static Transform _Canvas;
    public static Transform Canvas
    {
        get
        {
            GameObject obj = null;
            if (_Canvas == null)
                obj = GameObject.Find("Canvas");

            if (obj != null)
                _Canvas = obj.transform;
            return _Canvas;
        }
    }
    public static T OpenMul<T>(string strPrefab)
    {
        GameObject Panel = GameObject.Instantiate(Resources.Load(strPrefab, typeof(GameObject))) as GameObject;
        if (Canvas != null)
            Panel.transform.SetParent(Canvas);
        Panel.transform.localPosition = Vector3.zero;
        Panel.transform.localScale = Vector3.one;
        Panel.transform.localRotation = Quaternion.identity;
        return Panel.GetComponent<T>();
    }

    static Dictionary<string, GameObject> PanelList = new Dictionary<string, GameObject>();
    public static T Open<T>(string strPrefab)
    {
        if (PanelList.ContainsKey(strPrefab))
        {
            PanelList[strPrefab].transform.SetAsLastSibling();
            return PanelList[strPrefab].GetComponent<T>();
        }
        GameObject Panel = GameObject.Instantiate(Resources.Load(strPrefab, typeof(GameObject))) as GameObject;
        if (Canvas != null)
            Panel.transform.SetParent(Canvas);
        Panel.transform.localPosition = Vector3.zero;
        Panel.transform.localScale = Vector3.one;
        Panel.transform.localRotation = Quaternion.identity;
        PanelList.Add(strPrefab, Panel);
        RectTransform rc = Panel.GetComponent<RectTransform>();
        if (rc != null)
            rc.sizeDelta = new Vector2(0, 0);
        return PanelList[strPrefab].GetComponent<T>();
    }
}
