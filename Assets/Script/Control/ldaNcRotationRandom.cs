using UnityEngine;
using System.Collections;

public class ldaNcRotationRandom : MonoBehaviour {

	// Use this for initialization
    //void Start () {
	
    //}
	
    //// Update is called once per frame
    //void Update () {
	
    //}

    public Transform RandomZ;

    public float mEndTimeFactor = 0.1f;

    float mMovingTime = 0;

    float mEndingTime = 0;

    void OnEnable() {

        mMovingTime = GetComponentInParent<MoneyBall>().MovingTime;

        mEndingTime = mMovingTime * mEndTimeFactor;

        Vector3 RotationValue = Vector3.zero;

        if (Random.Range(0, 200) > 100)
            RotationValue = new Vector3(Random.Range(180, 540), 0, 0);
        else
            RotationValue = new Vector3(Random.Range(-540, -180), 0, 0);

        this.GetComponent<NcRotation>().m_vRotationValue = RotationValue;

        RandomZ.localPosition = new Vector3(Random.value, Random.value, Random.value);
    }

    void Update()
    {
        if (mMovingTime > mEndingTime)
        {
            mMovingTime -= Time.deltaTime;
            return;
        }

        //超过80%时间位置往里面缩
        if (mMovingTime > Time.deltaTime)
        {
            RandomZ.localPosition = Vector3.Lerp(RandomZ.localPosition, Vector3.zero, Time.deltaTime / mMovingTime);
            mMovingTime -= Time.deltaTime;
        }
    }
}
