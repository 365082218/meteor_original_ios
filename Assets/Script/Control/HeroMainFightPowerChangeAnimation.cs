using System;
using System.Collections.Generic;
using UnityEngine;

class HeroMainFightPowerChangeAnimation : MonoBehaviour
{
    int mOriginData = 0;
    public int OriginData { get { return mOriginData; } set { mOriginData = value; } }
    int mTargetData = 0;
    public int TargetData { get { return mTargetData; } set { mTargetData = value; } }
    float mStartSpeed = 0.0f;
    public float StartSpeed { get { return mStartSpeed; } set { mStartSpeed = value; } }
    float mEndSpeed = 0.0f;
    public float EndSpeed { get { return mEndSpeed; } set { mEndSpeed = value; } }
    bool mShowSignal = false;
    public bool ShowSignal { get { return mShowSignal; } set { mShowSignal = value; } }

    public float SpriteGap = 0.00f;
    float mTempData = 0;
    bool mIsAnimation = false;

    public int mFirstSpriteDepth = 0;
    public int NumberResType = 1;
    public float OriginPositionX = 0.0f;

    List<string> mStrNumberArray = new List<string>();
    Vector3 mSingleSpriteSize = Vector3.one;

    UIAtlas mSpriteAtlas = null;

    void Awake()
    {
        mIsAnimation = false;
        if (1 == NumberResType)
        {
            mSpriteAtlas = Resources.Load("UI/GameCommon/Atlas/GameCommon", typeof(UIAtlas)) as UIAtlas;
            mStrNumberArray.Add("0");
            mStrNumberArray.Add("1");
            mStrNumberArray.Add("2");
            mStrNumberArray.Add("3");
            mStrNumberArray.Add("4");
            mStrNumberArray.Add("5");
            mStrNumberArray.Add("6");
            mStrNumberArray.Add("7");
            mStrNumberArray.Add("8");
            mStrNumberArray.Add("9");
            mSingleSpriteSize = new Vector3(67, 59, 1);
        }
        else if (2 == NumberResType)
        {
            mSpriteAtlas = Resources.Load("UI/NumberRes/Number2/Atlas/Number2Atlas", typeof(UIAtlas)) as UIAtlas;
            mStrNumberArray.Add("p_0");
            mStrNumberArray.Add("p_1");
            mStrNumberArray.Add("p_2");
            mStrNumberArray.Add("p_3");
            mStrNumberArray.Add("p_4");
            mStrNumberArray.Add("p_5");
            mStrNumberArray.Add("p_6");
            mStrNumberArray.Add("p_7");
            mStrNumberArray.Add("p_8");
            mStrNumberArray.Add("p_9");
            mSingleSpriteSize = new Vector3(23, 27, 1);
        }
    }

    public void StartUpdateNumberSprite(int origin, int target, bool showanimation = true, bool showsignal = false, float startspeed = 10.00f, float endspeed = 1.00f)
    {
        TargetData = target;
        ShowSignal = showsignal;
        if(true == showanimation)
        {
            OriginData = origin;
            StartSpeed = startspeed;
            EndSpeed = endspeed;
            mTempData = OriginData;
            mIsAnimation = true;
        }
        else
        {
            mTempData = target;
            UpdateNumberSprite();
        }
    }

    void Update()
    {
        if(false == mIsAnimation) return;

        if(OriginData < TargetData)
        {
            float deltasec = StartSpeed - ((mTempData - OriginData) / (TargetData - OriginData)) * (StartSpeed - EndSpeed);
            mTempData += deltasec;
            if (mTempData >= TargetData)
            {
                mIsAnimation = false;
                mTempData = TargetData;
                UpdateNumberSprite();
                return;
            }
        }
        else if(OriginData == TargetData)
        {
            mIsAnimation = false;
            mTempData = TargetData;
            UpdateNumberSprite();
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
                UpdateNumberSprite();
                return;
            }
        }
        
        UpdateNumberSprite();
    }

    void UpdateNumberSprite()
    {
        int total = 0;
        if(true == ShowSignal)
        {
            string signal = "";
            if (mTempData > 0) signal = "z_jia";
            else signal = "z_jian";
            UISprite sprite = null;
            if(gameObject.transform.childCount > 0)
            {
                sprite = gameObject.transform.GetChild(0).gameObject.GetComponent<UISprite>();
                sprite.spriteName = signal;
            }
            else
            {
                sprite = NGUITools.AddSprite(gameObject, mSpriteAtlas, signal);
                sprite.pivot = UIWidget.Pivot.Left;
                sprite.transform.localScale = mSingleSpriteSize;
            }
            sprite.transform.localPosition = new UnityEngine.Vector3(0, 0, 0);
            sprite.depth = mFirstSpriteDepth;

            ++total;
        }

        string str = ((int)(UnityEngine.Mathf.Abs(mTempData))).ToString();
        int originlength = str.Length;
        for (int i = 0; i < originlength; ++i)
        {
            if (str.Length < 1) continue;
            string substr = str.Substring(0, 1);
            int number = int.Parse(substr);
            if (number >= 10 || number < 0) continue;
            UISprite sprite = null;
            if ((i+total) < gameObject.transform.childCount)
            {
                sprite = gameObject.transform.GetChild(i + total).gameObject.GetComponent<UISprite>();
                sprite.spriteName = mStrNumberArray[number];
            }
            else
            {
                sprite = NGUITools.AddSprite(gameObject, mSpriteAtlas, mStrNumberArray[number]);
                sprite.pivot = UIWidget.Pivot.Left;
                sprite.transform.localScale = mSingleSpriteSize;
            }
            sprite.transform.localPosition = new UnityEngine.Vector3(SpriteGap * (i + total), 0, 0);
            sprite.depth = mFirstSpriteDepth + i + total;
            str = str.Substring(1, str.Length - 1);
        }
        //destroy addition number sprite.
        while(gameObject.transform.childCount > originlength+total)
        {
            GameObject.DestroyImmediate(gameObject.transform.GetChild(gameObject.transform.childCount - 1).gameObject);
        }
    }

    public float GetNumberSpriteWidth()
    {
        float totalwidth = 0f;
        if (0 == gameObject.transform.childCount) return totalwidth;
        totalwidth = SpriteGap * gameObject.transform.childCount;
        return totalwidth;
    }

    public enum NumberAlignType
    {
        NumberAlignType_None,
        NumberAlignType_Left,
        NumberAlignType_Middle,
        NumberAlignType_Right
    }

    public void SetNumberSpriteAlign(NumberAlignType type = NumberAlignType.NumberAlignType_None)
    {
        float width = GetNumberSpriteWidth() * gameObject.transform.localScale.x;
        Vector3 pos = Vector3.zero;
        if (NumberAlignType.NumberAlignType_Left == type) pos = gameObject.transform.localPosition;
        else if (NumberAlignType.NumberAlignType_Middle == type) pos = new Vector3(OriginPositionX - width / 2, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
        else if (NumberAlignType.NumberAlignType_Right == type) pos = new Vector3(OriginPositionX - width, gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
        gameObject.transform.localPosition = pos;
    }
}
