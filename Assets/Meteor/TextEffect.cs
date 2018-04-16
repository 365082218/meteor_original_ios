using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public static class TextEffect {

    public static void FlyTo(this Graphic graphic)
    {
        RectTransform rt = graphic.rectTransform;
        Color c = graphic.color;
        Sequence mySequence = DOTween.Sequence();
        Tweener move1 = rt.DOLocalMoveY(rt.localPosition.y + 5.0f, 0.1f);
        Tweener alpha1 = graphic.DOColor(new Color(c.r, c.g, c.b, 0), 0.1f);
        Tweener scale = rt.DOScale(new Vector3(1.5f, 1.5f, 1.0f), 0.1f);
        mySequence.Append(move1);
        mySequence.Append(scale);
        mySequence.SetEase(Ease.OutElastic);
        mySequence.AppendInterval(0.1f);
        scale = rt.DOScale(new Vector3(1.2f, 1.2f, 1.0f), 0.1f);
        mySequence.Append(scale);
        mySequence.Join(alpha1);
    }

    public static void FlyTo(this GameObject graphic, float duration, float yHeight)
    {
        RectTransform rt = graphic.GetComponent<RectTransform>();
        Graphic[] graph = graphic.GetComponentsInChildren<Graphic>();
        
        Sequence mySequence = DOTween.Sequence();
        Tweener move1 = rt.DOLocalMoveY(rt.localPosition.y + yHeight, 0.5f);
        Tweener scale = rt.DOScale(new Vector3(1.5f, 1.5f, 1.0f), 0.5f);
        mySequence.SetEase(Ease.OutElastic);
        mySequence.Append(move1);
        mySequence.Join(scale);
        mySequence.AppendInterval(duration);
        mySequence.SetEase(Ease.Unset);
        move1 = rt.DOLocalMoveY(rt.localPosition.y + yHeight - 30.0f, 0.5f);
        mySequence.Append(move1);

        for (int i = 0; i < graph.Length; i++)
        {
            Color c = graph[i].color;
            Tweener alpha1 = graph[i].DOColor(new Color(c.r, c.g, c.b, 0), 0.5f);
            mySequence.Join(alpha1);
        }
    }

    public static void FlyTo(this Graphic graphic, float duration, float yHeight)
    {
        RectTransform rt = graphic.rectTransform;
        Color c = graphic.color;
        Sequence mySequence = DOTween.Sequence();
        Tweener move1 = rt.DOLocalMoveY(rt.localPosition.y + yHeight, 0.5f);
        Tweener scale = rt.DOScale(new Vector3(1.5f, 1.5f, 1.0f), 0.5f);
        Tweener alpha1 = graphic.DOColor(new Color(c.r, c.g, c.b, 0), 0.5f);  
        mySequence.SetEase(Ease.OutElastic);
        mySequence.Append(move1);
        mySequence.Join(scale);
        mySequence.AppendInterval(duration);
        mySequence.SetEase(Ease.Unset);
        move1 = rt.DOLocalMoveY(rt.localPosition.y + yHeight - 30.0f, 0.5f);
        mySequence.Append(move1);
        mySequence.Join(alpha1);
    }
}


