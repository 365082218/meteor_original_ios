using UnityEngine;
using System.Collections;

/*
 * Lindean 控制开门窗体打开和关闭
 * 
 * 窗体空Entity，加入TweenTransform
 * 设置好两个空Entity地点 From to
 * 挂这个脚本上去
 * 
 * 
 * 
 */

public class ldaWinShowHideCtrl : MonoBehaviour {

	public GameObject ShowHideBtn;//控制用的UIButton
	bool IsShowOrHide;

	// Use this for initialization
	void Start () {
		IsShowOrHide = false;
		UIEventListener.Get (ShowHideBtn).onClick = ShowHideBtnOnClick;
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void ShowHideBtnOnClick (GameObject go) {
		IsShowOrHide = !IsShowOrHide;

		this.GetComponent<TweenTransform> ().Play (IsShowOrHide);
	}
}
