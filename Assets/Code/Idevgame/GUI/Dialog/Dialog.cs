using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Idevgame.GameState.DialogState;
using DG.Tweening;
using Idevgame.StateManagement.DialogStateManagement;

//�˳��޶���.
public enum FadeEffect
{
    None,
    Scale,//�볡���Ŷ���.��scale 0������1
}

public class Dialog:UIBehaviour
{
    protected FadeEffect FadeIn = FadeEffect.None;
    protected FadeEffect FadeOut = FadeEffect.None;//Ҫ�ȶ������������ж��
    protected float duration = 0.5f;//��������0.5s
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

    public virtual void OnRefresh(int message, object param)
    {

    }

    public virtual void OnClose()
    {

    }

    public virtual void OnBackPress()
    {
        if (Persist != null)
        {
            Main.Ins.ExitState(Persist);
            Persist = null;
        }
        if (OwnerState != null && OwnerState.DialogStateManager != null)
            OwnerState.DialogStateManager.FireAction(DialogAction.Close);
    }

    public virtual void OnPreviousPress()
    {
        if (Persist != null)
        {
            Main.Ins.ExitState(Persist);
            Persist = null;
        }
        if (OwnerState != null && OwnerState.DialogStateManager != null)
            OwnerState.DialogStateManager.FireAction(DialogAction.Previous);
    }


    ///�־û���岿��
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