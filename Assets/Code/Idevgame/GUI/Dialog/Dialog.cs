using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Idevgame.GameState.DialogState;
using DG.Tweening;
using Idevgame.StateManagement.DialogStateManagement;

//�˳��޶���.
public enum FadeInEffect
{
    None,
    Scale,//�볡���Ŷ���.��scale 0������1
}

public class Dialog:UIBehaviour
{
    protected FadeInEffect effectIn = FadeInEffect.None;
    protected float duration = 0.5f;//��������0.5s
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


    ///�־û���岿��
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