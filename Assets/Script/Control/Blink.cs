using UnityEngine;
using System.Collections;

public class Blink : MonoBehaviour
{
	public float mDistance = 1.0f;
	private float mStartTime = 0.0f;
	private float mDeltaTime;
	private Transform[] children;
	// Use this for initialization
	void Start ()
	{
		mStartTime = 0.0f;
		children = gameObject.GetComponentsInChildren<Transform>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		mDeltaTime	+= Time.deltaTime;
		if( mDeltaTime >= mStartTime && mDeltaTime < (mStartTime + mDistance) ){
			foreach(Transform goTran in children){
				if(goTran.gameObject.name != gameObject.name)
					goTran.gameObject.SetActive(false);
			}
		}else if( mDeltaTime >= (mStartTime + mDistance)){
			foreach(Transform goTran in children){
				if(goTran.gameObject.name != gameObject.name)
					goTran.gameObject.SetActive(true);
			}
			mDeltaTime = mStartTime;
		}
	}
}

