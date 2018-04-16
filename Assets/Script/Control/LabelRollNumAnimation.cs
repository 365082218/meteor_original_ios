using UnityEngine;
using System.Collections;

public class LabelRollNumAnimation : MonoBehaviour
{
    int mOriginData = 0;
    public int OriginData { get { return mOriginData; } set { mOriginData = value; } }
    int mTargetData = 0;
    public int TargetData { get { return mTargetData; } set { mTargetData = value; } }
    public float mStartSpeed = 0.0f;
    public float StartSpeed { get { return mStartSpeed; } set { mStartSpeed = value; } }
    public  float mEndSpeed = 0.0f;
    public float EndSpeed { get { return mEndSpeed; } set { mEndSpeed = value; } }
    GameObject mExpLabelObject = null;
    public GameObject ExpLabelObject { get { return mExpLabelObject; } set { mExpLabelObject = value; } }
    int mOldLevel = 0;
    public int OldLevel { get { return mOldLevel; } set { mOldLevel = value; } }


    public float SpriteGap = 0.00f;
    float mTempData = 0;
    bool mIsAnimation = false;

    public void StartUpdateNumber(int origin, int target, float startspeed = 10.00f, float endspeed = 1.00f)
    {
        TargetData = target;
        OriginData = origin;
        StartSpeed = startspeed;
        EndSpeed = endspeed;
        mTempData = OriginData;
        mIsAnimation = true;


    }

    public void StartUpdateExp(GameObject ExpLabelObjectS = null, int OldLevelVal = 0)
    {
        ExpLabelObject = ExpLabelObjectS;
        OldLevel = OldLevelVal;

    }

    void Update()
    {
        if (false == mIsAnimation) return;

        if (OriginData < TargetData)
        {
            float deltasec = StartSpeed - ((mTempData - OriginData) / (TargetData - OriginData)) * (StartSpeed - EndSpeed);
            mTempData += deltasec;
            if (mTempData >= TargetData)
            {
                mIsAnimation = false;
                mTempData = TargetData;
                UpdateLabelNum();
                return;
            }
        }
        else if (OriginData == TargetData)
        {
            mIsAnimation = false;
            mTempData = TargetData;
            UpdateLabelNum();
            return;
        }
        else
        {
            float deltasec = StartSpeed - ((OriginData - mTempData) / (OriginData - TargetData)) * (StartSpeed - EndSpeed);
            mTempData -= deltasec;
            if (mTempData <= TargetData)
            {
                mIsAnimation = false;
                mTempData = TargetData;
                UpdateLabelNum();
                return;
            }
        }

        UpdateLabelNum();
        if (ExpLabelObject != null)
        {
            UpdateGameObject();
        }

    }

    private void UpdateLabelNum()
    {
        gameObject.GetComponent<UILabel>().text = ((int)mTempData).ToString();
    }

    private void UpdateGameObject()
    {
        if (ExpLabelObject.name == "JingyanScroll")
        {
            //float UpgradeExp = HeroUpExpManager.Instance.GetItem(OldLevel).exp;
            //float barSzieVal = 0.1f;
            //if (mTempData <= UpgradeExp)
            //    barSzieVal = mTempData / UpgradeExp;
            //else
            //    barSzieVal = 1;
            //ExpLabelObject.GetComponent<UIScrollBar>().barSize = barSzieVal;

        }

    }

}
