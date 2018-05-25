using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum WindowStyle{
	WS_Normal,
	WS_Ext,
    WS_Modal,//模态
	WS_CullingMask,
};

public class WindowMng
{
    static Dictionary<GameObject, Windows> WndList = new Dictionary<GameObject, Windows>();
    public static void AddWindow(GameObject obj, Windows win)
    {
        if (!WndList.ContainsKey(obj))
            WndList.Add(obj, win);
    }
    public static void RemoveWindow(GameObject obj, Windows win)
    {
        if (WndList.ContainsKey(obj))
            WndList.Remove(obj);
    }
    public static void OnModalClick(GameObject obj)
    {
        if (WndList.ContainsKey(obj))
            WndList[obj].OnClick();
    }
    public static void CloseAll()
    {
        List<Windows> all = new List<Windows>();
        foreach (var each in WndList)
            all.Add(each.Value);
        for (int i = 0; i < all.Count; i++)
        {
            all[i].Close();
        }
    }
}

public abstract class  Windows
{
    public virtual void Close() { }
    protected virtual bool OnClose() { return true; }
    protected virtual bool OnShow() { return true; }
    protected virtual bool OnHide() { return true; }
    protected virtual bool OnConfirm(bool confirm, int tag) { return true; }
    protected virtual int GetZ() { return 0; }
    protected virtual int GetY() { return 0; }
    protected virtual int GetX() { return 0; }
    protected virtual bool OnOpen(){return true;}
    public virtual void Show(){}
    public virtual void Hide(){}
    public virtual void OnClick(){}
}

public abstract class Window<T> :Windows where T : class, new() 
{
    
   	static T msInstance = null;
	static int mCullingMask = 0;
    public static T Instance
    {
        get
        {
            if (msInstance == null)
                msInstance = new T();
            return msInstance;
        }
    }

    public static bool Exist { get { return msInstance != null; } }
	GameObject mRootUI = null;
    GameObject mWndObject = null;
	GameObject mExtBackground = null;
    public string preWnd = "";
	
    public virtual string PrefabName { get { return typeof(T).Name; } }
    public GameObject WndObject { get { return mWndObject; } }
    protected override bool OnOpen() 
    {
        return true; 
    }
    protected virtual bool Use3DCanvas()
    {
        return false;
    }
    protected virtual bool CanvasMode()
    {
        return false;
    }
    protected virtual bool FullStretch()
    {
        return true;
    }
	public WindowStyle mWindowStyle = WindowStyle.WS_Normal;
	public WindowStyle WinStyle { get { return mWindowStyle; } set { mWindowStyle = value; }}


	//这个函数能够是拿到在编辑状态SetActive为false的控件的，并且递归下去拿 add by Lindean 20141018
	public GameObject ldaControl(string name)
	{
		if (msInstance == null)
			return null;

        //return Control(name, mWndObject);
		return ldaControl(name, mWndObject);
	}

	public GameObject ldaControl (string name, GameObject parent) {

		if (msInstance == null)
			return null;

        if (parent == null)
            return null;
		try
		{
			for (int i=0; i < parent.transform.childCount; i++) {
				GameObject childObj = parent.transform.GetChild(i).gameObject;
				if(name == childObj.name){
					return childObj;
				}
				GameObject childchildObj = ldaControl (name, childObj);
				if(childchildObj != null)
					return childchildObj;
			}
		}
		catch
		{
			Debug.LogError("err:  "+name+" "+parent);
		}
		return null;
	}

	public GameObject Control(string name)
    {
        if (msInstance == null)
            return null;

        return Control(name, mWndObject);
    }

    public GameObject Control(string name, GameObject parent)
    {
        if (msInstance == null)
            return null;

        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == name)
                return child.gameObject;
        }
        return null;
    }

	public U GetCom<U>(string name,GameObject parent) where U : MonoBehaviour
	{
		if(parent==null)return null;

        U[] children = parent.GetComponentsInChildren<U>();
		foreach (U child in children)
		{
			if (child.name == name)
				return child;
		}
		return null;
	}

	public void Confirm(bool confirm, int tag)
	{
		OnConfirm(confirm, tag);
	}

    public void DoModal()
    {
        mWindowStyle = WindowStyle.WS_Modal;
        Open();
    }

    public void Open()
    {
		//Debug.Log("Open Window:"+PrefabName);
        if (mWndObject)
        {
            Debug.LogError("Window:" + PrefabName + "The window already opened!!!!");
            WindowMng.RemoveWindow(mWndObject, this);
            GameObject.Destroy(mWndObject);//这句话会导致一个界面上的成员变量没清理
            if (WindowStyle.WS_Ext <= mWindowStyle)
                GameObject.Destroy(mExtBackground);
            //return;
            
        }
#if UNITY_2017 || UNITY_5_5
        if (Use3DCanvas())
        {
            mRootUI = GameObject.Find("3dCanvas");
            if (mRootUI == null)
            {
                mRootUI = GameObject.Instantiate(Resources.Load<GameObject>("3dCanvas"), Vector3.zero, Quaternion.identity);
                mRootUI.name = "3dCanvas";
            }
        }
        if (mRootUI == null)
            mRootUI = GameObject.Find("Canvas");
        if (mRootUI != null)
        {
            if (!CanvasMode())
                mWndObject = GameObject.Instantiate(Resources.Load<GameObject>(PrefabName), Vector3.zero, Quaternion.identity, mRootUI.transform) as GameObject;
            else
                mWndObject = GameObject.Instantiate(Resources.Load<GameObject>(PrefabName));
        }
        else
            mWndObject = GameObject.Instantiate(Resources.Load<GameObject>(PrefabName));
        if (mRootUI != null)
        {
            if (!CanvasMode())
            {
                WndObject.transform.SetParent(mRootUI.transform);
                WndObject.transform.localScale = Vector3.one;
                WndObject.transform.localRotation = Quaternion.identity;
                WndObject.layer = mRootUI.transform.gameObject.layer;
            }
            RectTransform rectTran = WndObject.GetComponent<RectTransform>();
            if (rectTran != null && rectTran.anchorMin == Vector2.zero && rectTran.anchorMax == Vector2.one)
            {
                if (rectTran.rect.width == 0 && rectTran.rect.height == 0)
                    rectTran.sizeDelta = new Vector2(0, 0);

            }
            if (rectTran != null)
                rectTran.anchoredPosition3D = new Vector3(0, 0, GetZ());
        }
        else
        {
            //mRootUI = GameObject.Find("Anchor");
            //if (mRootUI != null)
            //    WndObject.transform.SetParent(mRootUI.transform);
        }
#else
        mRootUI = GameObject.Find("Anchor");
        WndObject.transform.parent = mRootUI.transform;
#endif
        WndObject.transform.localScale = Vector3.one;
		//WndObject.transform.localPosition = new Vector3(GetX(), GetY(), GetZ());
        WindowMng.AddWindow(mWndObject, this);

        OnOpen();

        //attention Order
        //mCamera = GameObject.Find("Camera-CloseUp");
        //if (mCamera != null)
        //    mCullingMask = mCamera.GetComponent<Camera>().cullingMask;

        //阻挡UI之后其他的UI响应
        if (mWindowStyle >= WindowStyle.WS_Ext)
        {
			mExtBackground = GameObject.Instantiate(Resources.Load("BackgroundExtWnd")) as GameObject;
            mExtBackground.transform.SetParent(WndObject.transform);
            mExtBackground.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            mExtBackground.transform.localScale = Vector3.one;
            mExtBackground.transform.localRotation = Quaternion.identity;
            mExtBackground.transform.SetAsFirstSibling();
        }

        //if (WindowStyle.WS_CullingMask <= mWindowStyle)
        //{
        //    Debug.Log("LayerMask NGUI : = " + LayerMask.NameToLayer("NGUI"));
        //    //attention Order
        //    if (mCamera != null)
        //        mCamera.GetComponent<Camera>().cullingMask = LayerMask.NameToLayer("NGUI");
        //}
        //AutoAdaptGUI();
    }

    //当前窗口是模态窗口，且相机上挂了UICamera时，设置了消息响应层，模态背景被触摸时，自动关闭此窗口
    public override void OnClick()
    {
        if (mWindowStyle >= WindowStyle.WS_Ext)
            Close();
    }

    public override void Close()
    {
        WindowMng.RemoveWindow(mWndObject, this);
        OnClose();
        if (mWndObject != null)
        {
            GameObject.Destroy(mWndObject);
            mWndObject = null;
        }
        if (WindowStyle.WS_Ext <= mWindowStyle
            && mExtBackground != null)
        {
            GameObject.Destroy(mExtBackground);
            mExtBackground = null;
        }
        msInstance = null;
    }

    public override void Show()
    {
        if (Exist)
        {
            WndObject.SetActive(true);
            if (WindowStyle.WS_Ext <= mWindowStyle)
                mExtBackground.SetActive(true);
        }

        OnShow();
    }

    public override void Hide()
    {
        if (Exist)
        {
            WndObject.SetActive(false);
            if (WindowStyle.WS_Ext <= mWindowStyle)
                mExtBackground.SetActive(false);
        }

        OnHide();
	}

	
	void OnClickedTimeOutQuit( GameObject go )
	{
		Application.Quit();
	}

    public bool WndShown { get { return WndObject.activeSelf; } }
    public void BtnSetAble(GameObject go, bool bStatus)
    {
        GameObject goDis = go.transform.Find("Disable").gameObject;
        if (goDis == null)
            return;

        go.GetComponent<BoxCollider>().enabled = bStatus;
        goDis.SetActive(!bStatus);
        if (bStatus)//NGUI Bug
        {
            UIImageButton uiib = go.GetComponent<UIImageButton>();
            go.transform.Find("Background").GetComponent<UISprite>().spriteName = uiib.normalSprite;
        }
    }

    //public void UpdateItemQualityFrameIcon(GameObject go,ItemBase itemBase)
    //{
    //	//Debug.Log(quality);
    //	QualitySprite QSp = QualitySprite.Button10_BaseItem_Quality_00;

    //	switch((int)itemBase.Quality)
    //	{
    //		case (int) QualitySprite.Button10_BaseItem_Quality_00:
    //			QSp = QualitySprite.Button10_BaseItem_Quality_00;
    //			go.SetActive(false);
    //			break;
    //		case (int) QualitySprite.Button10_BaseItem_Quality_01:
    //			QSp = QualitySprite.Button10_BaseItem_Quality_01;
    //			go.SetActive(true);
    //			go.GetComponent<UISprite>().spriteName= QSp.ToString();
    //			break;
    //		case (int) QualitySprite.Button10_BaseItem_Quality_02:
    //			QSp = QualitySprite.Button10_BaseItem_Quality_02;
    //			go.SetActive(true);
    //			go.GetComponent<UISprite>().spriteName= QSp.ToString();
    //			break;
    //		case (int) QualitySprite.Button10_BaseItem_Quality_03:
    //			QSp = QualitySprite.Button10_BaseItem_Quality_03;
    //			go.SetActive(true);
    //			go.GetComponent<UISprite>().spriteName= QSp.ToString();
    //			break;
    //		case (int) QualitySprite.Button10_BaseItem_Quality_04:
    //			QSp = QualitySprite.Button10_BaseItem_Quality_04;
    //			go.SetActive(true);
    //			go.GetComponent<UISprite>().spriteName= QSp.ToString();
    //			break;
    //		default:
    //			go.SetActive(false);
    //			break;
    //	}
    //}
}
