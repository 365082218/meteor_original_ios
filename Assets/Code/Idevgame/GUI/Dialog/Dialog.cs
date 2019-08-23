using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Idevgame.GameState.DialogState;
using DG.Tweening;
using Idevgame.StateManagement.DialogStateManagement;

//退场无动画.
public enum FadeInEffect
{
    None,
    Scale,//入场缩放动画.从scale 0缩放至1
}

public class Dialog:UIBehaviour
{
    protected FadeInEffect effectIn = FadeInEffect.None;
    protected float duration = 0.5f;//过场动画0.5s
    private BaseDialogState OwnerState;

    public virtual void OnDialogStateEnter(BaseDialogState ownerState, BaseDialogState previousDialog, object data)
    {
        OwnerState = ownerState;
        if (effectIn == FadeInEffect.Scale)
        {
            transform.localScale = Vector3.zero;
            Tweener tweenScale = transform.DOScale(Vector3.one, duration);
            tweenScale.SetEase(Ease.InCubic);
        }
    }

    public virtual void OnRefresh(int message, object param)
    {

    }

    public virtual void OnBackPress()
    {
        OwnerState.DialogStateManager.FireAction(DialogAction.Close);
    }

    public virtual void OnPreviousPress()
    {
        OwnerState.DialogStateManager.FireAction(DialogAction.Previous);
    }


    ///持久化面板部分
    public PersistState Persist;
    public virtual void OnDialogStateEnter(PersistState ownerState, BaseDialogState previousDialog, object data)
    {
        Persist = ownerState;
        if (effectIn == FadeInEffect.Scale)
        {
            transform.localScale = Vector3.zero;
            Tweener tweenScale = transform.DOScale(Vector3.one, duration);
            tweenScale.SetEase(Ease.InCubic);
        }
    }
}