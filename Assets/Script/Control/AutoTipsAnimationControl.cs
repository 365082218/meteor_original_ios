using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AutoTipsAnimationControl : MonoBehaviour
{
    public float mOriginScale = 0.001f;
    public float mTargetScale = 0.0f;
    public float mOriginPos = 0.0f;
    public float mTargetPos = 0.0f;

    public bool DestroyWhenAnimationEnd = true;
    //0:not begin; 1:animation; 2:animation end.
    int mAnimationState = 0;
    public int AnimationState { get { return mAnimationState; } set { mAnimationState = value; } }

    void Start()
    {
        AnimationState = 0;
    }

    public void StartAction(bool destroywhenend = true, float originscale = 0.001f, float targetscale = 1.00f, float originpos = 0.00f, float targetpos = 100.00f,float moveTime=0.6f)
    {
        mOriginScale = originscale;
        mTargetScale = targetscale;
        mOriginPos = originpos;
        mTargetPos = targetpos;
        DestroyWhenAnimationEnd = destroywhenend;
        AnimationState = 1;

        transform.localScale = new Vector3(mTargetScale, mTargetScale, transform.localScale.z);
        Vector3 pos = new Vector3(transform.localPosition.x, mTargetPos, transform.localPosition.z);

        iTween.MoveTo(gameObject, iTween.Hash("position", pos, "time", moveTime, "delay", 0.0f, "easetype", iTween.EaseType.linear, "islocal", true, "ignoretimescale", true, "oncomplete", "OnMoveComplete"));
    }

    public void OnMoveComplete()
    {
        AnimationState = 2;
        if (true == DestroyWhenAnimationEnd) GameObject.Destroy(gameObject);
    }
}
