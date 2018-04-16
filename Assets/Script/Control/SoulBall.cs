using UnityEngine;
using System.Collections;

public class SoulBall : MonoBehaviour
{
    enum EMoveState
    {
        None,
        Rise,
        Moving,
    };

    public float RiseTime = 0.5f;
    public float MovingTime = 0.3f;
    public float RiseHeight = 1.8f;
    public float TargetHeight = 1.0f;

    public delegate void OnFinished(SoulBall soulBall);

    MeteorUnit mTarget;
    EMoveState mMoveState = EMoveState.None;
    OnFinished mOnFinished = null;
    float mMovingTime = 0;
    int mSoulValue = 0;

    // Update is called once per frame
    void Update()
    {
        if (mMoveState == EMoveState.Moving)
        {
            if (mMovingTime > Time.deltaTime)
            {
                Vector3 targetPos = new Vector3(mTarget.transform.position.x, mTarget.transform.position.y + TargetHeight, mTarget.transform.position.z);
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime / mMovingTime);
                mMovingTime -= Time.deltaTime;
            }
            else
            {
                // finished.
                mMoveState = EMoveState.None;
                //mTarget.UUnitInfo.UnitTopUI.OnInfoPopup("Soul+" + mSoulValue, UnitTopUI.SoulColor);
				//mTarget.AddSoul(mSoulValue);//cancel by Lindean 20141031
				//mTarget.AddExp(mSoulValue);//mTarget.AddSoul(mSoulValue);//怪物被杀出现的能量小球，增加魂力的,新战斗变成增加能量

                if (mOnFinished != null)
                    mOnFinished(this);
            }
        }
    }

    public void Begin(Vector3 pos, int soul, OnFinished onFinished)
    {
        transform.position = pos;
        mSoulValue = soul;
        mTarget = MeteorManager.Instance.LocalPlayer;
        mMoveState = EMoveState.Rise;
        mOnFinished = onFinished;

        iTween.MoveTo(gameObject, iTween.Hash(
            "y", transform.position.y + RiseHeight,
            "time", RiseTime,
            "easetype", iTween.EaseType.easeOutCubic,
            "oncomplete", "RiseComplete"));
    }

	public void ldaBegin(MeteorUnit attacker, Vector3 pos, int soul, OnFinished onFinished)
	{
		transform.position = pos;
		mSoulValue = soul;
		mTarget = attacker;
		mMoveState = EMoveState.Rise;
		mOnFinished = onFinished;
		
		iTween.MoveTo(gameObject, iTween.Hash(
			"y", transform.position.y + RiseHeight,
			"time", RiseTime,
			"easetype", iTween.EaseType.easeOutCubic,
			"oncomplete", "RiseComplete"));
	}

    void RiseComplete()
    {
        mMovingTime = MovingTime;
        mMoveState = EMoveState.Moving;
    }
}
