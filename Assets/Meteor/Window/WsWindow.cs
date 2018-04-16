using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WsWindow
{
    public static string Battle = "Battle";//战斗界面.
    public static string Scene = "Scene";//地图界面.
    public static string Inventory = "Inventory";//物品界面.
    public static string StartGame = "GameStart";//第一次过场.
    public static string Dialogue = "Dialogue";//对话框界面
    public static string ViewItem = "ViewItem";//物品具体信息界面
    public static string UIPage = "UIPage";//选择门派界面
    //public static string RegAndLogin = "RegAndLogin";//登录混合注册界面
    public static string PopupTip = "PopupTip";//弹出提示.
    public static string RoleInfo = "RoleInfo";//登录线路时取对应线上的角色信息
    public static string ServerInfo = "ServerInfo";//游戏服线路选择
    public static string SelectTarget = "SelectTarget";
    public static string NpcTalkPanel = "NpcTalkPanel";//与NPC对话界面.
    public static string Jiguanyuan = "Jiguanyuan";
    public static string DebugPanel = "DebugPanel";
    public static string MapPanel = "Map";
    public static string Question = "Question";
    public static string ChangeItem = "ChangeItem";
    public static string BuildingCtrl = "BuildingCtrl";
    public static string RestCtrl = "Rest";
    public static string Shop = "Shop";
    public static string PlayerShop = "PlayerShop";
    public static string ChangeEquipment = "ChangeEquipment";
    public static string YesNo = "YesNo";
    public static string System = "NewSystemWnd";
    public static string ArmyShop = "ArmyShop";
    public static string LanguageSelect = "LanguageSelect";
    public static string SystemInFight = "NewSystemWnd";//战斗中的系统菜单
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

    public static T OpenNoParent<T>(string strPrefab)
    {
        if (PanelList.ContainsKey(strPrefab))
        {
            PanelList[strPrefab].transform.SetAsLastSibling();
            return PanelList[strPrefab].GetComponent<T>();
        }
        GameObject Panel = GameObject.Instantiate(Resources.Load(strPrefab, typeof(GameObject))) as GameObject;
        Panel.transform.localPosition = Vector3.zero;
        Panel.transform.localScale = Vector3.one;
        Panel.transform.localRotation = Quaternion.identity;
        PanelList.Add(strPrefab, Panel);
        return PanelList[strPrefab].GetComponent<T>();
    }

    public static GameObject Open(string strPrefab)
    {
        if (PanelList.ContainsKey(strPrefab))
        {
            PanelList[strPrefab].gameObject.SetActive(true);
            PanelList[strPrefab].transform.SetAsLastSibling();
            return PanelList[strPrefab].gameObject;
        }
        GameObject Panel = GameObject.Instantiate(Resources.Load(strPrefab, typeof(GameObject))) as GameObject;
        if (Canvas != null)
            Panel.transform.SetParent(Canvas);
        Panel.transform.localPosition = Vector3.zero;
        Panel.transform.localScale = Vector3.one;
        Panel.transform.localRotation = Quaternion.identity;
        PanelList.Add(strPrefab, Panel);
        return PanelList[strPrefab].gameObject;
    }

    public static void CloseAll()
    {
        Close(Battle);
        Close(Scene);
        Close(Inventory);
        Close(StartGame);
        Close(Dialogue);
        Close(ViewItem);
        Close(UIPage);
        Close(RoleInfo);
        Close(ServerInfo);
    }

    public static void Close(GameObject objPanel)
    {
        string val = "";
        foreach (var each in PanelList)
        {
            if (each.Value == objPanel)
            {
                GameObject willDelete = null;
                foreach (var eachRoot in MenuMng)
                {
                    if (eachRoot.Key.IsChildOf(objPanel.transform))
                    {
                        willDelete = eachRoot.Key.gameObject;
                        break;
                    }
                }
                if (willDelete != null)
                    MenuMng.Remove(willDelete.transform);
                GameObject.DestroyImmediate(each.Value);
                val = each.Key;
                break;
            }
        }
        if (PanelList.ContainsKey(val))
            PanelList.Remove(val);
    }

    public static void Close(string strPrefab)
    {
        if (PanelList.ContainsKey(strPrefab))
        {
            Close(PanelList[strPrefab]);
            PanelList.Remove(strPrefab);
        }
    }

    static Dictionary<Transform, List<GameObject>> MenuMng = new Dictionary<Transform, List<GameObject>>();
    //parent拥有布局控件，这里只需要生成预设，挂到这个节点就行.
    public static T AddMenu<T>(string strPrefab, Transform parent)
    {
        GameObject MenuItem = GameObject.Instantiate(Resources.Load(strPrefab, typeof(GameObject))) as GameObject;
        MenuItem.transform.SetParent(parent);
        MenuItem.transform.localPosition = Vector3.zero;
        MenuItem.transform.localScale = Vector3.one;
        MenuItem.transform.localRotation = Quaternion.identity;
        if (MenuMng.ContainsKey(parent))
            MenuMng[parent].Add(MenuItem);
        else
            MenuMng.Add(parent, new List<GameObject>() { MenuItem });
        return MenuItem.GetComponent<T>();
    }

    public static GameObject AddMenu(string strPrefab, Transform parent)
    {
        GameObject MenuItem = GameObject.Instantiate(Resources.Load(strPrefab, typeof(GameObject))) as GameObject;
        MenuItem.transform.SetParent(parent);
        MenuItem.transform.localPosition = Vector3.zero;
        MenuItem.transform.localScale = Vector3.one;
        MenuItem.transform.localRotation = Quaternion.identity;
        if (MenuMng.ContainsKey(parent))
            MenuMng[parent].Add(MenuItem);
        else
            MenuMng.Add(parent, new List<GameObject>() { MenuItem });
        return MenuItem;
    }

    //删除指定的按钮.
    public static void RemoveMenu(GameObject MenuItem, Transform parent)
    {
        if (MenuMng.ContainsKey(parent))
        {
            GameObject.DestroyImmediate(MenuItem);
            MenuMng[parent].Remove(MenuItem);
            if (MenuMng[parent].Count == 0)
                MenuMng.Remove(parent);
        }
    }

    public static void RemoveDiaMenu(Transform parent, string str)
    {
        if (MenuMng.ContainsKey(parent))
        {
            GameObject objRemove = null;
            for (int i = 0; i < MenuMng[parent].Count; i++)
            {
                Text txt = MenuMng[parent][i].GetComponentInChildren<Text>();
                if (txt != null && txt.text == str)
                {
                    objRemove = txt.transform.parent.gameObject;
                    break;
                }
            }

            if (objRemove != null)
            {
                MenuMng[parent].Remove(objRemove);
                GameObject.DestroyImmediate(objRemove);
            }

            if (MenuMng[parent].Count == 0)
                MenuMng.Remove(parent);
        }
    }

    public static void CleanMenu(Transform menuRoot)
    {
        if (MenuMng.ContainsKey(menuRoot))
        {
            for (int i = 0; i < MenuMng[menuRoot].Count; i++)
                GameObject.DestroyImmediate(MenuMng[menuRoot][i]);
            MenuMng.Remove(menuRoot);
        }
    }
}
