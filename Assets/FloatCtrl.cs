using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatCtrl : MonoBehaviour {
    AnimationCurve curve;
    public void Start()
    {
        hScale = 10;
        Keyframe[] ks = new Keyframe[2];
        ks[0] = new Keyframe(0, 0);
        ks[1] = new Keyframe(4, 2);
        curve = new AnimationCurve(ks);
        curve.postWrapMode = WrapMode.PingPong;
        curve.preWrapMode = WrapMode.PingPong;
        initializeY = transform.position.y;
    }

    float initializeY;
    public float hScale;

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        float y = curve.Evaluate(Time.time);
        transform.position = new Vector3(transform.position.x, initializeY + hScale * y, transform.position.z);
    }
}
