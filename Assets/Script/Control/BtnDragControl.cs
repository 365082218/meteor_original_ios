using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BtnDragControl : MonoBehaviour {
	private List<Transform> mBtnTrans = new List<Transform>();
	public float mFriction = 10.0f;
	public float mLimDistance = 2.0f;
	public float mDeltaTimes = 20.0f;
    public float lastdelateTime = 0.0f;
	// Use this for initialization
	void Start () {
		mBtnTrans.Clear();
		Transform[] temp = GetComponentsInChildren<Transform>();
		int cnt = 0;
		foreach (Transform trans in temp)
		{
			if (trans.name.Contains("Pos"))
			{
				trans.localPosition = new Vector3(0, R - 132 * cnt, -3);
				mBtnTrans.Add(trans);
				cnt++;
			}
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (!BtnDrag.isPressed)
			CheckBtnSpring();

        if (BtnDrag.isPressed && mBtnTrans[0]!=null)
        {
            mInitByOnClickBool = false;
        }
	}

    float R = 579.0f;  // R^2 = (535 - x)^2 + y^2

	public void ChangePosByDelta(float disY)//
	{
		float tempPosY = mBtnTrans[0].localPosition.y - disY;
		if (tempPosY > R || tempPosY < -R)
			return;
		tempPosY = mBtnTrans[mBtnTrans.Count - 1].localPosition.y - disY;
		if (tempPosY > R || tempPosY < -R)
			return;

		foreach(Transform trans in mBtnTrans)
		{
			float posY =trans.localPosition.y - disY;
			float posX = 550- Mathf.Sqrt(R*R - posY*posY);
			trans.localPosition = new Vector3(posX, posY, -3);
		}
	}
    public void ChangePosByOnClick(float disY)//
    {
        mInitByOnClickBool = true;
        ChangePosByDelta(disY);
    }
    bool  mInitByOnClickBool =false;
    float disUp = 0.0f;
	void CheckBtnSpring()
	{
        if (mInitByOnClickBool == false)
        {
            disUp = R - mBtnTrans[0].localPosition.y;
        }
        else
        {
            disUp = 50;
        }
		float disDown = mBtnTrans[mBtnTrans.Count - 1].localPosition.y + R;
		float disForce = disUp - disDown;
		if (disForce < mLimDistance && disForce > -mLimDistance)
			return;

		if (disForce > 0)
			disForce -= mFriction;
		else
            disForce += mFriction;

		float disY = disForce / mDeltaTimes;
		ChangePosByDelta(-disY);
        lastdelateTime = Time.deltaTime;
		//ChangePosByDelta(100);
	}
}
