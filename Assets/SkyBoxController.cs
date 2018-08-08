using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxController : MonoBehaviour {

    // Use this for initialization
    AnimationCurve curve;
    private void Awake()
    {
        Keyframe[] ks = new Keyframe[2];
        ks[0] = new Keyframe(0, 0);
        ks[1] = new Keyframe(1, 4);
        curve = new AnimationCurve(ks);
        curve.postWrapMode = WrapMode.PingPong;
        curve.preWrapMode = WrapMode.PingPong;
    }

    void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        float v = curve.Evaluate(Time.time);
        //Debug.Log("v:" + v);
        RenderSettings.skybox.SetFloat("_Exposure", v);
	}
}
