using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Idevgame.GameState.DialogState;
using DG.Tweening;
using Idevgame.StateManagement.DialogStateManagement;

//退场无动画.
public enum FadeEffect
{
    None,
    Scale,//入场缩放动画.从scale 0缩放至1
}

public class Dialog:UIBehaviour
{
    protected FadeEffect FadeIn = FadeEffect.None;
    protected FadeEffect FadeOut = FadeEffect.None;//要等动画播放完毕再卸载
    protected float duration = 0.5f;//过场动画0.5s
    public BaseDialogState State { get { return OwnerState; } }
    private BaseDialogState OwnerState;
    protected GameObject WndObject { get { return gameObject; } }

    public GameObject Control(string child)
    {
        return Control(child, gameObject);
    }

    protected GameObject Control(string child, GameObject root)
    {
        return NodeHelper.Find(child, root);
    }

    public virtual void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        OwnerState = ownerState;
        if (FadeIn == FadeEffect.Scale)
        {
            transform.localScale = Vector3.zero;
            Tweener tweenScale = transform.DOScale(Vector3.one, duration);
            tweenScale.SetEase(Ease.InCubic);
        }
    }

    public virtual void OnDialogStateExit()
    {
        if (FadeOut == FadeEffect.Scale)
        {
            transform.localScale = Vector3.one;
            Tweener tweenScale = transform.DOScale(Vector3.zero, duration);
            tweenScale.SetEase(Ease.OutCubic);
        }
        OnClose();
    }

    public virtual void OnRefresh(int message, object param = null)
    {

    }

    public virtual void OnClose()
    {

    }

    public virtual void OnBackPress()
    {
        if (Persist != null)
        {
            Persist.Close();
            Persist = null;
        }
        if (OwnerState != null && OwnerState.DialogStateManager != null)
            OwnerState.DialogStateManager.FireAction(DialogAction.Close);
    }

    public virtual void OnPreviousPress()
    {
        if (Persist != null)
        {
            Persist.Close();
            Persist = null;
        }
        if (OwnerState != null && OwnerState.DialogStateManager != null)
            OwnerState.DialogStateManager.FireAction(DialogAction.Previous);
    }


    ///持久化面板部分
    public PersistState Persist;
    public virtual void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        Persist = ownerState;
        if (FadeIn == FadeEffect.Scale)
        {
            transform.localScale = Vector3.zero;
            Tweener tweenScale = transform.DOScale(Vector3.one, duration);
            tweenScale.SetEase(Ease.InCubic);
        }
    }
}