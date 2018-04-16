using System;
using System.Collections.Generic;
using UnityEngine;

class CaculateAllElementWidthControl : MonoBehaviour
{
    public float ElementDistance = 0.0f;
    public float ItemDistanceEx = 0.0f;
    void Start()
    {
        ElementDistance = 5.0f;
    }

    public float GetBorderWidth()
    {
        float borderwidth = 0f;
        for (int i = 0; i < gameObject.transform.childCount; ++i )
        {
            borderwidth = borderwidth > 0 ? borderwidth + ElementDistance : borderwidth;
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            UISprite sprite = child.transform.GetComponent<UISprite>();
            if(null != sprite)
            {
                borderwidth += sprite.transform.localScale.x;
                continue;
            }
            UILabel label = child.transform.GetComponent<UILabel>();
            if(null != label)
            {
                borderwidth += label.relativeSize.x * label.transform.localScale.x;
                continue;
            }

            borderwidth = borderwidth > 0 ? borderwidth - ElementDistance : borderwidth;
        }

        return borderwidth;
    }

    public void AdjustElementPos()
    {
        float borderwidth = 0f;
        for (int i = 0; i < gameObject.transform.childCount; ++i)
        {
            borderwidth = borderwidth > 0 ? borderwidth + ElementDistance : borderwidth;
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            child.transform.localPosition = new Vector3(borderwidth, child.transform.localPosition.y, child.transform.localPosition.z);

            UISprite sprite = child.transform.GetComponent<UISprite>();
            if (null != sprite)
            {
                borderwidth += sprite.transform.localScale.x;
                continue;
            }
            UILabel label = child.transform.GetComponent<UILabel>();
            if (null != label)
            {
                borderwidth += label.relativeSize.x * label.transform.localScale.x;
                continue;
            }

            borderwidth = borderwidth > 0 ? borderwidth - ElementDistance : borderwidth;
        }

        gameObject.transform.localPosition = new Vector3(-(borderwidth + ItemDistanceEx), gameObject.transform.localPosition.y, gameObject.transform.localPosition.z);
    }
}
