using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConfirmWnd<T> where T : class, new()  {
	public string PrefabName { get { return "ConfirmWnd"; } }
	
	UILabel mTitleText;
    UILabel mConfirmText;
    UILabel mConfirmBtnText;
    UILabel mCancleBtnText;
	
	public delegate void OnConFirm(bool confirm, int tag);
	Window<T> mParentWnd;
	GameObject mWndObject = null;	
	GameObject mRootUI = null;

	int mConfirmTag = 0;
	public bool OnOpen()
	{
		mWndObject = GameObject.Instantiate(Resources.Load(PrefabName)) as GameObject;
		ShaderUtil.FiexdActionAndGodDoubleSide(mWndObject);
		// attach to the root.//attention Order
		mRootUI = GameObject.Find("Anchor");
		mWndObject.transform.parent = mRootUI.transform;
		//        WndObject.transform.localPosition = Vector3.zero;
		mWndObject.transform.localScale = Vector3.one;
		mWndObject.transform.localPosition= new Vector3(0,0,-50);
		Init();
		return true;
	}

	//这个函数能够是拿到在编辑状态SetActive为false的控件的，并且递归下去拿 add by Lindean 20141018
	public GameObject ldaControl(string name)
	{		
		//return Control(name, mWndObject);
		return ldaControl(name, mWndObject);
	}
	
	public GameObject ldaControl (string name, GameObject parent) {
		
		for (int i=0; i < parent.transform.childCount; i++) {
			GameObject childObj = parent.transform.GetChild(i).gameObject;
			if(name == childObj.name){
				return childObj;
			}
			GameObject childchildObj = ldaControl (name, childObj);
			if(childchildObj != null)
				return childchildObj;
		}
		return null;
	}

	void Init()
	{
        mTitleText = ldaControl("TitleLabel").GetComponent<UILabel>() as UILabel;
		mConfirmText = ldaControl("ConfirmText").GetComponent<UILabel>() as UILabel;
       
		GameObject go = ldaControl("CancleBtn");
		UIEventListener.Get(go).onClick = CancleClick;
		mCancleBtnText = go.transform.Find("Label").GetComponent<UILabel>() as UILabel;

		go = ldaControl("ConfirmBtn");
		UIEventListener.Get(go).onClick = ConfirmClick;
        mConfirmBtnText = go.transform.Find("Label").GetComponent<UILabel>() as UILabel;

        UIEventListener.Get(ldaControl("BackButton1")).onClick = OnBackButton;
        UIEventListener.Get(ldaControl("BackButton2")).onClick = OnBackButton;

	}

	public void SetParentWnd(Window<T> pWnd, int confirmTag, string centerTips, string confirmbtn = "", string cancleBtn = "", string title = "")
	{
		mParentWnd = pWnd;
        mTitleText.text = title;
		mConfirmText.text = centerTips;
		mConfirmTag = confirmTag;
		if (confirmbtn != "")
			mConfirmBtnText.text = confirmbtn;
		if (cancleBtn != "")
			mCancleBtnText.text = cancleBtn;
	}

	void ConfirmClick(GameObject go)
	{
		if(mParentWnd != null)
			mParentWnd.Confirm(true, mConfirmTag);
		if (mConfirmTag == 103) {//弹出点金手.
			//GoldHandWnd.Instance.Open();
		}
		if (mConfirmTag == 108) {

				//RequestCmd request = new RequestCmd ();
				//request.cmd = CMD.BuyEnergyAction;
				//RParamVO rparam = new RParamVO ();
				//request.param = rparam;
				//MainScript.Instance.Request (request, delegate(string err, Response response) {
				//});
		}
		Close();
	}

	void CancleClick(GameObject go)
	{
		if(mParentWnd != null)
			mParentWnd.Confirm(false, mConfirmTag);
		Close();
	}

	void Close(){
		if(mWndObject != null)
			GameObject.Destroy(mWndObject);
	}

    public GameObject GetConfirmWndObj()
    {
        return mWndObject;
    }

    public GameObject GetConfirmTextContent()
    {
        return mConfirmText.gameObject;
    }

    public void StretchBackgroundSpriteToOverspreadTheWidthOfScreen()
    {
        GameObject bgsprite = ldaControl("confirmBG");
        bgsprite.transform.localScale = new UnityEngine.Vector3(WindowsRoot.ScaleWidth, bgsprite.transform.localScale.y, bgsprite.transform.localScale.z);
    }

    void OnBackButton(GameObject go)
    {
        Close();
    }

}
