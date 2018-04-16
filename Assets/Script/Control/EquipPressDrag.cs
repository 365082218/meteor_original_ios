using UnityEngine;
using System.Collections;

public class EquipPressDrag : MonoBehaviour {

	public Transform target;
	public float speed = 1f;
	
	Transform mTrans;
	
	void Start ()
	{
		mTrans = transform;
	}
	
	void OnPress()
	{
		//AssetBundle.
		//AssetBundle.
	}
	
	void OnDrag (Vector2 delta)
	{
		UICamera.currentTouch.clickNotification = UICamera.ClickNotification.None;
		
		if (target != null)
		{
			//target.localRotation = Quaternion.Euler(0, -0.5f * delta.x * speed, 0f) * target.localRotation;
			target.Rotate (new Vector3 (0, -0.5f * delta.x * speed, 0.5f * delta.y * speed) , Space.Self);
		}
		else
		{
			//mTrans.localRotation = Quaternion.Euler(0, -0.5f * delta.x * speed, 0f) * mTrans.localRotation;
			mTrans.Rotate (new Vector3 ( 0,-0.5f * delta.x * speed,0.5f * delta.y * speed) , Space.Self);
		}
	}
}
