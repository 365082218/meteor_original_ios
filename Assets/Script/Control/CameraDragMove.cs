using UnityEngine;
using System.Collections;

public class CameraDragMove : MonoBehaviour
{

	// Use this for initialization
	private Bounds mDragBounds;
	Transform mTrans;
	
	void Start ()
	{
		mTrans = GameObject.Find("CameraPos0").transform;
		mDragBounds.center = new Vector3(-207.5f,mTrans.localPosition.y,65.0f);
		mDragBounds.extents = new Vector3(7.5f,0.0f,15.0f);
	}
	
	// Update is called once per frame
	void LateUpdate()
	{
		if(UICamera.hoveredObject == null)
			Drag();
	}
	
	
	void Drag()
	{
		if(Application.platform == RuntimePlatform.WindowsEditor)
		{
			//Right Mouse Click
			if(Input.GetMouseButton(1))
			{
				float x = Input.GetAxis("Mouse X")*Time.deltaTime*30.0f;
				float z = Input.GetAxis("Mouse Y")*Time.deltaTime*30.0f;
				
				GetDragRange(new Vector3(x,0.0f,z));
			}
		}
		else if(Application.platform == RuntimePlatform.IPhonePlayer || 
			    Application.platform == RuntimePlatform.Android )
		{
			if((Input.touchCount > 0) &&(Input.GetTouch(0).phase == TouchPhase.Moved))
			{
				float x = Input.GetAxis("Mouse X")*Time.deltaTime*30.0f;
				float z = Input.GetAxis("Mouse Y")*Time.deltaTime*30.0f;
				
				GetDragRange(new Vector3(x,0.0f,z));
			}
		}
	}
	
	void GetDragRange(Vector3 range)
	{
		Vector3 curPos = mTrans.localPosition;
		if(curPos.x + range.x < mDragBounds.min.x)
			range.x = mDragBounds.min.x - curPos.x;
		
		if(curPos.x + range.x > mDragBounds.max.x)
			range.x = mDragBounds.max.x - curPos.x;		
		
		if(curPos.z + range.z < mDragBounds.min.z)
			range.z = mDragBounds.min.z - curPos.z;
		
		if(curPos.z + range.z > mDragBounds.max.z)
			range.z = mDragBounds.max.z - curPos.z;
				
		if(range.magnitude > 0.001f)
			mTrans.localPosition += range;
	}
	
	public void WaitForTime(float time)
	{
		Invoke("ShowContent",time);
	}
	
	private void ShowContent()
	{
		//LevelSelectWnd.Instance.ShowBorderFrame();
	}	
}

