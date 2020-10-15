using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {
	
	float mFrequence = 0f;
	float mAmplitude = 0f;
	Vector3 mOrigWorldPos = Vector3.zero;
    Vector3 mOrigLocalPos = Vector3.zero;
	float mTotal = 0f;
	float mCount = 0f;
	private static bool isShaking=false;
    //float mShakeTime = -f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void Shake(float time, float frequence, float amplitude)
	{
        //时间，频率，幅度
        //CameraShake(0.5, 10, 20);
		if(isShaking)return;
		isShaking=true;
        mOrigWorldPos = transform.position;
        mOrigLocalPos = transform.localPosition;//Lindean
		mTotal = frequence;
		mFrequence = frequence / time;
		mAmplitude = amplitude/100f;
		mCount = 0f;
		Play();
	}
	
	void Play()
	{
        transform.position = mOrigWorldPos + new Vector3(Utility.Range(-1, 1) * mAmplitude, Utility.Range(-1, 1) * mAmplitude, 0f);
		mCount++;
        if (mCount < mTotal)
            Invoke("Play", mFrequence / 1000);
        else{
            //transform.position = mOrigWorldPos;
            //transform.localPosition = mOrigLocalPos;//Lindean
			transform.localPosition = Vector3.zero;//Lindean
			isShaking=false;
		}
			
	}

}
