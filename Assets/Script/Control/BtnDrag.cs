using UnityEngine;
using System.Collections;

public class BtnDrag : MonoBehaviour {
	public static bool isPressed = false;
	public static bool isDrag = false;
	Vector2 mClickPos;
	BtnDragControl mBtnDragControl;
	public Transform btnbg;
	// Use this for initialization
	void Start () {
		mBtnDragControl = transform.parent.parent.gameObject.GetComponent<BtnDragControl>() as BtnDragControl;
		UIEventListener.Get(gameObject).onPress = BtnPressFun;
		UIEventListener.Get(gameObject).onDrag = BtnDragFun;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void BtnPressFun(GameObject go, bool isPress)
	{
		isPressed = isPress;
		btnbg.gameObject.SetActive(isPress);
		if (isPress)
		{
			mClickPos = UIHelper.ScreenPointToUIPoint(UICamera.currentTouch.pos);
			btnbg.parent = transform.parent;
			btnbg.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -2);
			isDrag = false;
		}
	}

	public void BtnDragFun(GameObject go, Vector2 delta)
	{
		isDrag = true;
		Vector2 tempPos = UIHelper.ScreenPointToUIPoint(UICamera.currentTouch.pos);
		float distance  = mClickPos.y - tempPos.y;
		mClickPos = tempPos;
		mBtnDragControl.ChangePosByDelta(distance * 1.2f);
	}
}
