using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class FixedPlatformCtrl : MonoBehaviour {
	public int PlatformIdx = 0;
	private AnimationCurve curve;
    List<MeteorUnit> unit = new List<MeteorUnit>();
    Vector3 lastpos;
    private void Awake()
    {
        
    }

    private void Start()
    {
        hScale = 50;
        if (curve == null)
        {
            Keyframe[] ks = new Keyframe[2];
            ks[0] = new Keyframe(0, 0);
            ks[1] = new Keyframe(4, 2);
            curve = new AnimationCurve(ks);
        }
        curve.postWrapMode = WrapMode.PingPong;
        curve.preWrapMode = WrapMode.PingPong;
        initializeY = transform.position.y;
    }

    float initializeY;
    public float hScale;

    // Update is called once per frame
    void Update () {
	
	}

    private void LateUpdate()
    {
        float y = curve.Evaluate(Time.time);
        lastpos = transform.position;
        transform.position = new Vector3(transform.position.x, initializeY + hScale * y, transform.position.z);
        Vector3 vdiff = transform.position - lastpos;
        for (int i = 0; i < unit.Count; i++)
            unit[i].charController.Move(vdiff);
        lastpos = transform.position;
    }

    public void OnTriggerEnter(Collider other)
    {
        MeteorUnit u = other.gameObject.GetComponent<MeteorUnit>();
        if (u != null)
        {
            if (!unit.Contains(u))
            {
                if (u.Attr.IsPlayer)
                    CameraFollow.Ins.Smooth = false;
                unit.Add(u);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        MeteorUnit target = other.gameObject.GetComponent<MeteorUnit>();
        if (target != null && unit.Contains(target))
        {
            if (target.Attr.IsPlayer)
                CameraFollow.Ins.Smooth = true;
            unit.Remove(target);
        }
    }

}
