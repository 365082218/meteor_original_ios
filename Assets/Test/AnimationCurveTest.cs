using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Vector3Keyframe {
    public Vector3 inTangent;
    public Vector3 outTangent;
    public float time;
    public Vector3 value;
}

public class QuaternionKeyframe {
    public Vector4 inTangent;
    public Vector4 outTangent;
    public float time;
    public Quaternion value;
}

public class AnimationCurveTest : MonoBehaviour {
    //public AnimationCurve curve;
    //public AnimationClip clip;
    public int index;
    private void Awake() {
        //AmbLoader.Ins.LoadCharacterAmb(0);
        //for (int i = 1; i <= 65; i++) {
        //    if (AmbLoader.PlayerAnimation[0].ContainsKey(i)) {

        //    }
        //}

        
        //curve = null;

        //if (curve == null) {
        //    Keyframe[] ks = new Keyframe[2];
        //    ks[0] = new Keyframe(0, -1);
        //    ks[1] = new Keyframe(1, 1);
        //    curve = new AnimationCurve(ks);
        //}
        //curve.postWrapMode = WrapMode.PingPong;
        //curve.preWrapMode = WrapMode.PingPong;
    }
    // Use this for initialization
    void Start () {
        //Anim = gameObject.GetComponent<Animation>();
        //Anim.wrapMode = WrapMode.Loop;
        //UnityEditor.EditorCurveBinding[] a = UnityEditor.AnimationUtility.GetCurveBindings(Anim.clip);
        //for (int i = 0; i < a.Length; i++) {
        //    curve = UnityEditor.AnimationUtility.GetEditorCurve(Anim.clip, a[i]);
        //    for (int j = 0; j < curve.keys.Length; j++) {
        //        Debug.Log("key:" + j + " ");
        //        Debug.Log("tangentMode" + curve.keys[j].tangentMode + " ");
        //        //UnityEditor.AnimationUtility.TangentMode.Linear
        //        Debug.Log("intangent" + curve.keys[j].inTangent + " ");
        //        Debug.Log("outtangent" + curve.keys[j].outTangent + " ");
        //        Debug.Log("time:" + curve.keys[j].time + " ");
        //        Debug.Log("value:" + curve.keys[j].value + "\n");
        //    }
        //}
    }
	
	// Update is called once per frame
	void Update () {
        //if (curve != null) {
        //    transform.position = new Vector3(0, curve.Evaluate(0.25f), 0);
        //}

        if (Input.GetKeyDown(KeyCode.E)) {
            //    for (int i = 0; i < curve.keys.Length; i++) {
            //        Debug.Log("key:" + i + " ");
            //        Debug.Log("tangentMode" + curve.keys[i].tangentMode + " ");
            //        //UnityEditor.AnimationUtility.TangentMode.Linear
            //        Debug.Log("intangent" + curve.keys[i].inTangent + " ");
            //        Debug.Log("outtangent" + curve.keys[i].outTangent + " ");
            //        Debug.Log("time:" + curve.keys[i].time + " ");
            //        Debug.Log("value:" + curve.keys[i].value + "\n");

            //    }
            //    float v = _hermiteInterpolate(curve.keys[0], curve.keys[1], 0.25f, 1.0f);
            //    Debug.Log("v:" + v);
            //if (AmbLoader.PlayerAnimation[0].ContainsKey(index)) {
            //    Debug.Log("boneStatus:" + AmbLoader.PlayerAnimation[0][index]);
            //}
        }
    }

    //计算普通插值
    float _hermiteInterpolate(Keyframe frame, Keyframe nextFrame, float t, float dur) {
        float t0 = frame.outTangent, t1 = nextFrame.inTangent;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1)) {
            float t2 = t * t;
            float t3 = t2 * t;
            float a = 2.0f * t3 - 3.0f * t2 + 1.0f;
            float b = t3 - 2.0f * t2 + t;
            float c = t3 - t2;
            float d = -2.0f * t3 + 3.0f * t2;
            return a * (float)frame.value + b * t0 * dur + c * t1 * dur + d * (float)nextFrame.value;
        } else
            return frame.value;
    }

    //计算平移的插值
    void _hermiteInterpolateVector3(Vector3Keyframe frame, Vector3Keyframe nextFrame, float t, float dur, Vector3 calcOut) {
        Vector3 p0 = frame.value;
        Vector3 tan0 = frame.outTangent;
        Vector3 p1 = nextFrame.value;
        Vector3 tan1 = nextFrame.inTangent;
        float t2 = t * t;
        float t3 = t2 * t;
        float a = 2.0f * t3 - 3.0f * t2 + 1.0f;
        float b = t3 - 2.0f * t2 + t;
        float c = t3 - t2;
        float d = -2.0f * t3 + 3.0f * t2;
        float t0 = tan0.x, t1 = tan1.x;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1))
			calcOut.x = a * p0.x + b * t0 * dur + c * t1 * dur + d * p1.x;
		else
		calcOut.x = p0.x;
        t0 = tan0.y; t1 = tan1.y;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1))
            calcOut.y = a * p0.y + b * t0 * dur + c * t1 * dur + d * p1.y;
		else
		calcOut.y = p0.y;
        t0 = tan0.z; t1 = tan1.z;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1))
			calcOut.z = a * p0.z + b * t0 * dur + c * t1 * dur + d * p1.z;
		else
		calcOut.z = p0.z;
    }

    //计算四元数的插值
    void _hermiteInterpolateQuaternion(QuaternionKeyframe frame, QuaternionKeyframe nextFrame, float t, float dur, Quaternion calcOut) {
        Quaternion p0 = frame.value;
        Vector4 tan0 = frame.outTangent;
        Quaternion p1 = nextFrame.value;
        Vector4 tan1 = nextFrame.inTangent;
        float t2 = t * t;
        float t3 = t2 * t;
        float a = 2.0f * t3 - 3.0f * t2 + 1.0f;
        float b = t3 - 2.0f * t2 + t;
        float c = t3 - t2;
        float d = -2.0f * t3 + 3.0f * t2;
        float t0 = tan0.x, t1 = tan1.x;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1))
			calcOut.x = a * p0.x + b * t0 * dur + c * t1 * dur + d * p1.x;
		else
		calcOut.x = p0.x;
        t0 = tan0.y; t1 = tan1.y;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1))
			calcOut.y = a * p0.y + b * t0 * dur + c * t1 * dur + d * p1.y;
		else
		calcOut.y = p0.y;
        t0 = tan0.z; t1 = tan1.z;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1))
			calcOut.z = a * p0.z + b * t0 * dur + c * t1 * dur + d * p1.z;
		else
		calcOut.z = p0.z;
        t0 = tan0.w; t1 = tan1.w;
        if (/*__JS__ */!float.IsInfinity(t0) && !float.IsInfinity(t1))
			calcOut.w = a * p0.w + b * t0 * dur + c * t1 * dur + d * p1.w;
		else
		calcOut.w = p0.w;
    }
}
