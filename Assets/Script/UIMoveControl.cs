using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
//仅仅支持transform指定的坐标系统
//RectTramsform不支持
public class UIMoveControl : MonoBehaviour {
	public Vector3 mStartPos = Vector3.zero;
    public Vector3 mRectStartPos = Vector3.zero;//这个值，只有在面板 锚点 和 布局都居中时有效
	public float mDelateTime = 0.0f;
	public float mDurTime = 0.0f;
	public bool isOutElastic = false;
	public bool isShowWhenWake = true;
	public bool isShow = true;
    bool useRectTransform = false;
    public bool isShowMoveComplete = false;

	public static List<UIMoveControl> mUIMoveControlList = new List<UIMoveControl>();
	private Vector3 mEndPos = new Vector3();
    private Vector3 mEndRectPos = new Vector3();
    void Awake()
    {
        if (GetComponent<RectTransform>() != null)
            useRectTransform = true;
    }

	// Use this for initialization
	void Start () {
        mEndPos.x = transform.localPosition.x;
        mEndPos.y = transform.localPosition.y;
        mEndPos.z = transform.localPosition.z;
        if (useRectTransform)
            mEndRectPos = transform.GetComponent<RectTransform>().anchoredPosition3D;
        mUIMoveControlList.Add(this);
        if (isShowWhenWake)
            ShowAction();
    }
	
	// Update is called once per frame

	public void ShowAction()
	{
        if (useRectTransform)
        {
            transform.GetComponent<RectTransform>().anchoredPosition3D = mRectStartPos;
            transform.GetComponent<RectTransform>().DOAnchorPos3D(mEndRectPos, mDurTime);
        }
        else
        {
            transform.localPosition = mStartPos;
            if (isOutElastic)
            {
                iTween.MoveTo(gameObject, iTween.Hash("name", "action", "position", mEndPos, "time", mDurTime, "delay", mDelateTime, "easetype", iTween.EaseType.easeOutBounce, "islocal", true, "oncomplete", "MoveComplete", "ignoretimescale", true));
            }
            else
                iTween.MoveTo(gameObject, iTween.Hash("name", "action", "position", mEndPos, "islocal", true, "time", mDurTime, "delay", mDelateTime, "oncomplete", "MoveComplete", "ignoretimescale", true));
            isShowMoveComplete = false;
        }
	}

	public void HideAction()
	{
        if (useRectTransform)
        {
            transform.GetComponent<RectTransform>().anchoredPosition3D = mEndRectPos;
            transform.GetComponent<RectTransform>().DOAnchorPos3D(mRectStartPos, mDurTime);
        }
        else
        {
            transform.localPosition = mEndPos;
            if (isOutElastic)
            {
                iTween.MoveTo(gameObject, iTween.Hash("name", "action", "position", mStartPos, "time", mDurTime, "delay", mDelateTime, "easetype", iTween.EaseType.easeOutBounce, "islocal", true, "ignoretimescale", true));
            }
            else
                iTween.MoveTo(gameObject, iTween.Hash("name", "action", "position", mStartPos, "islocal", true, "time", mDurTime, "delay", mDelateTime, "ignoretimescale", true));
        }
	}

	public void StopAction()
	{
		iTween.StopByName(gameObject, "action");
	}

	public void SetEndPos()
	{
        if (useRectTransform)
            mEndRectPos = transform.GetComponent<RectTransform>().anchoredPosition3D;
        else
            mEndPos  = transform.localPosition;
	}

	public void Play()
	{
		if (isShow)
			ShowAction();
		else
			HideAction();
		isShow = !isShow;
	}

	void Update () {

	}

	void OnDestroy(){
		mUIMoveControlList.Remove(this);
	}

    void MoveComplete()
    {
        isShowMoveComplete = true;
    }
}
