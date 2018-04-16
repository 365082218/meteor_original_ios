using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class HeroHeadItemControl : MonoBehaviour
{
    public int HeroModelId = 0;
    public int HeroStarLevel = 0;
    public int HeroLevel = 0;
    public int HeroQuality = 0;
    public string HeroIconRes = "";

    UISprite mHeroHeroIconSprite = null;
    GameObject mHeroGrayStarLevelPanel = null;
    GameObject mHeroLightStarLevelPanel = null;
    UISprite mHeroQualitySprite = null;
    UILabel mHeroLevelLabel = null;

    UISprite mHeroRedPointSprite = null;
    UISprite mHightLightSprite = null;

    void Awake()
    {
        mHeroHeroIconSprite = gameObject.transform.Find("HeadIconPanel").Find("HeroHeadIconSprite").gameObject.GetComponent<UISprite>();
        mHeroGrayStarLevelPanel = gameObject.transform.Find("SecondPanel").Find("GrayStarPanel").gameObject;
        mHeroLightStarLevelPanel = gameObject.transform.Find("SecondPanel").Find("LightStarPanel").gameObject;
        mHeroQualitySprite = gameObject.transform.Find("QualitySprite").gameObject.GetComponent<UISprite>();
        mHeroLevelLabel = gameObject.transform.Find("LevelLabelPanel").Find("LevelLabel").gameObject.GetComponent<UILabel>();

        mHeroRedPointSprite = gameObject.transform.Find("RedTipsSprite").gameObject.GetComponent<UISprite>();
        mHightLightSprite = gameObject.transform.Find("HightLightSprite").gameObject.GetComponent<UISprite>();
    }

    public void UpdateHeroHeadItemInfo(int level, int modelid, int starlevel, int quality, string iconres = "")
    {
        HeroModelId = modelid;
        HeroStarLevel = starlevel;
        HeroLevel = level;
        HeroQuality = quality;
        HeroIconRes = iconres;
        ShowHeroInfo();
    }

    void ShowHeroInfo()
    {
        if (HeroIconRes.Equals("") && 0 != HeroModelId)
        {
            //HeroBase hb = HeroBaseManager.Instance.GetItem(HeroModelId);
            //if (null == hb) return;
            //mHeroHeroIconSprite.spriteName = hb.icon;

            //mHeroQualitySprite.spriteName = GameUtil.GetHeroheadQualityBgSpriteEx(HeroQuality);

            //mHeroLevelLabel.gameObject.transform.localScale = new Vector3(26, 26, 1);

            //for (int i = 1; i < 6; ++i)
            //{
            //    string gaystr = "GrayStarSprite" + i.ToString();
            //    GameObject graysprite = mHeroGrayStarLevelPanel.transform.FindChild(gaystr).gameObject;
            //    UIAtlas atlas = Resources.Load("UI/JingJiChang/JingJiChangEnterPort/JingJiChangEnterPort", typeof(UIAtlas)) as UIAtlas;
            //    UISprite sprite = NGUITools.AddSprite(mHeroLightStarLevelPanel, atlas, "xin_17_17");
            //    sprite.gameObject.transform.localScale = new Vector3(17, 17, 1);
            //    sprite.gameObject.transform.localPosition = graysprite.transform.localPosition;
            //    sprite.depth = 7;
            //}
        }
        else
        {
            mHeroHeroIconSprite.spriteName = HeroIconRes;
            mHeroGrayStarLevelPanel.SetActive(false);
            mHeroLightStarLevelPanel.SetActive(false);
            mHeroQualitySprite.gameObject.SetActive(false);

            mHeroLevelLabel.gameObject.transform.localScale = new Vector3(24, 24, 1);
        }

        mHeroLevelLabel.text = HeroLevel.ToString();
    }

    public void ShowRedPoint(bool bshow = false)
    {
        mHeroRedPointSprite.gameObject.SetActive(bshow);
    }

    public void ShowHightLightSprite(bool bshow = false)
    {
        mHightLightSprite.gameObject.SetActive(bshow);
    }
}
