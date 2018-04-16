using UnityEngine;
using System.Collections;

public class UIScaleControl : MonoBehaviour {
	public Vector3 mStartScale = Vector3.zero;
	public float mDelayTime = 0.0f;
	public float mDurTime = 0.0f;

	private Vector3 mEndScale = Vector3.zero;
	private bool isShow = true;
	// Use this for initialization
	void Start () {
		mEndScale.x = transform.localScale.x;
		mEndScale.y = transform.localScale.y;
		mEndScale.z = 1;// transform.localScale.z;

		transform.localScale = mStartScale;
		ShowAction();	
	}

	public void ShowAction()
	{
		enabled =true;
		isShow = !isShow;
		iTween.ScaleTo(gameObject, iTween.Hash(
			"scale", isShow ? mEndScale : mStartScale,
			"delay", mDelayTime,
			"easetype", iTween.EaseType.linear,
			"time", mDurTime,
			"oncomplete","OnComplete"));
	}

	void OnComplete()
	{
		enabled =false;
		UILabel[] labs = GetComponents<UILabel>();
		foreach(UILabel lab in labs)
		{
			lab.transform.position = new Vector3(lab.transform.position.x,lab.transform.position.y,-5);
		}

	}

	// Update is called once per frame
	void Update () {
	
	}
}
