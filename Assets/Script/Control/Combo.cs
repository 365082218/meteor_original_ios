using UnityEngine;
using System.Collections;

public class Combo {
	
	int mComboCount = 0;
	GameObject mCombo;
	UILabel mCountLabel;
	TweenScale mTween;
	TweenAlpha mTweenAlpha;
	public Combo(Transform parent)
	{
		mCombo = GameObject.Instantiate(Resources.Load("Combo")) as GameObject;
		mCombo.transform.parent = parent;
		mCombo.transform.localScale = Vector3.one;
		mCombo.transform.localPosition = Vector3.zero+new Vector3(240,200,0);
	
		mTween = mCombo.transform.Find("Scale").GetComponent<TweenScale>();
		mCountLabel = mTween.transform.Find("Count").GetComponent<UILabel>();
		mTweenAlpha = mCombo.GetComponent<TweenAlpha>();
		mTweenAlpha.onFinished = OnFinish;
		mTweenAlpha.enabled = false;
		mCombo.SetActive(false);
	}
	
	public void OnComboHit(int comboHit)
	{
		if(!mCombo.activeSelf)
			mCombo.SetActive(true);
		mTweenAlpha.enabled = true;
		mTweenAlpha.Reset();
		mTween.Reset();
		mTween.enabled = true;
        if (mComboCount<999)//Lindean
		    mComboCount += comboHit;
		string conboString = mComboCount.ToString();
		mCountLabel.text = conboString;
	}

    public void ClearComboCount()
    {
        mComboCount = 0;
    }
	
	void OnFinish(UITweener tween)
	{
		mComboCount = 0;
	}
}
