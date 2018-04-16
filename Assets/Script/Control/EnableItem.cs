using UnityEngine;
using System.Collections;

public class EnableItem : MonoBehaviour {

    Vector3 BeginScale = new Vector3(1, 1, 1);
    Vector3 EndScale = new Vector3(1.1f, 1.1f, 1);
    private Transform[] itemtrans;
    void OnEnable()
    {
        num = 0;
        itemtrans = new Transform[gameObject.transform.childCount];
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            itemtrans[i] = gameObject.transform.GetChild(i);
        }
        showItem();
    }
    void showItem()
    {
      
        foreach (Transform tran in itemtrans)
        {
            if (tran.name != gameObject.name)
            {
                tran.gameObject.SetActive(false);
            }
        }
        setItem();
     
    }

    int num = 0;
    void setItem( )
    {
        if (itemtrans.Length > 0)
        {
            itemtrans[num].gameObject.SetActive(true);
            if (itemtrans[num].gameObject.GetComponent<TweenScale>() == null)
            {
                itemtrans[num].gameObject.AddComponent<TweenScale>();
            }

            TweenScale.Begin(itemtrans[num].gameObject, 0.1f, EndScale);
            TweenScale tween = itemtrans[num].gameObject.GetComponent<TweenScale>();
            tween.enabled = true;
            tween.onFinished = setOtherItem;
        }
      
    }

    void setOtherItem(UITweener tween0)
    {
        TweenScale.Begin(itemtrans[num].gameObject, 0.1f, BeginScale);
        TweenScale tween = itemtrans[num].gameObject.GetComponent<TweenScale>();
        tween.enabled = true;
        if (num < itemtrans.Length-1)
        {
            num++;
            setItem();
        }
        gameObject.GetComponent<UITable>().repositionNow = true;
    }
}
