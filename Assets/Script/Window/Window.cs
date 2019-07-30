using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class  Windows
{
    public virtual void Close() { }
    protected virtual bool OnClose() { return true; }
    protected virtual bool OnShow() { return true; }
    protected virtual bool OnHide() { return true; }
    protected virtual int GetZ() { return 0; }
    protected virtual int GetY() { return 0; }
    protected virtual int GetX() { return 0; }
    protected virtual bool OnOpen(){return true;}
    public virtual void Show(){}
    public virtual void Hide(){}
    public virtual void OnClick(){}
    public virtual void OnRefresh(int message, object param) { }//当其他窗口刷新了本窗口用到的数据后，刷新本窗口的UI
}

public abstract class Window<T> :Windows where T : class, new() 
{
    
   	static T msInstance = null;
	//static int mCullingMask = 0;
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
	//GameObject mExtBackground = null;
    public string preWnd = "";
	
    public virtual string PrefabName { get { return typeof(T).Name; } }
    public GameObject WndObject { get { return mWndObject; } }
    protected override bool OnOpen() 
    {
        return true; 
    }

    public override void OnRefresh(int message, object param)
    {

    }

    protected virtual bool Use3DCanvas()
    {
        return false;
    }

    protected virtual bool CanvasMode()
    {
        return false;
    }

	public GameObject ldaControl(string name)
	{
		if (msInstance == null)
			return null;
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

    public void Open()
    {
        if (mWndObject)
            return;
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
#else
        mRootUI = GameObject.Find("Anchor");
        WndObject.transform.parent = mRootUI.transform;
#endif
        WndObject.transform.localScale = Vector3.one;
        OnOpen();
    }

    public override void Close()
    {
        OnClose();
        if (mWndObject != null)
        {
            GameObject.Destroy(mWndObject);
            mWndObject = null;
        }
        msInstance = null;
    }
}
