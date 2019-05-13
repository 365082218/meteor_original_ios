using UnityEngine;
using System;

public abstract partial class AbstractScrollViewController {
    [Header("AbstractScrollViewSnap")]
    [SerializeField] protected bool Snap = false;
    [SerializeField] protected bool ScrollSpeedLinear = false;
    [SerializeField] protected float SnapSpeedLinear = 0.75f;
    [SerializeField] protected float ContinuousPressTimeTreshold = 0.3f;
    [SerializeField] protected float ContinuousPressSpeed = 2.5f;
    [SerializeField] protected float SnapSpeed = 4f;
    [SerializeField] protected float InertiaCutoffMagnitude = 800f;
    /// <summary>
    /// The pages in one swipe damping. More is less pages through.
    /// </summary>
    [SerializeField] protected int PagesInOneSwipeDamp = 4000;
    [SerializeField] protected int MaxSteps = 0;

    public Action OnSnappingEnded;

    protected bool Lerping;
    protected float TargetSnap;

    protected float PreviousPressedTime = 0f;
    protected float NextPressedTime = 0f;

    public bool IsLerping {
        get { 
            return Lerping;
        }
    }

    public abstract void PreviousContinuousEnd();

    public abstract void NextContinuousEnd();

    public virtual void PreviousContinuousStart() {
        if (!IsTouchRectTransformNullOrEnabled())
            return;

        PreviousPressedTime = Time.time;
        Canvas.willRenderCanvases -= ContinousPreviousPressUpdate;
        Canvas.willRenderCanvases += ContinousPreviousPressUpdate;
    }

    public virtual void NextContinuousStart() {
        if (!IsTouchRectTransformNullOrEnabled())
            return;

        NextPressedTime = Time.time;
        Canvas.willRenderCanvases -= ContinousNextPressUpdate;
        Canvas.willRenderCanvases += ContinousNextPressUpdate;
    }

    protected virtual void OnContiniousPreviousPressApplied() {
    }

    protected virtual void OnContiniousNextPressApplied() {
    }

    protected virtual void ContinuousPreviousChange() {
        if (ScrollRect.horizontal)
            GetScrollRect().horizontalNormalizedPosition = Mathf.Clamp01(GetScrollRect().horizontalNormalizedPosition - Time.smoothDeltaTime * ContinuousPressSpeed);
        else if (ScrollRect.vertical) {
            GetScrollRect().verticalNormalizedPosition = Mathf.Clamp01(GetScrollRect().verticalNormalizedPosition - Time.smoothDeltaTime * ContinuousPressSpeed);
        }
    }

    protected virtual void ContiniousNextChange() {
        if (ScrollRect.horizontal)
            GetScrollRect().horizontalNormalizedPosition = Mathf.Clamp01(GetScrollRect().horizontalNormalizedPosition + Time.smoothDeltaTime * ContinuousPressSpeed);
        else if (ScrollRect.vertical) {
            GetScrollRect().verticalNormalizedPosition = Mathf.Clamp01(GetScrollRect().verticalNormalizedPosition + Time.smoothDeltaTime * ContinuousPressSpeed);
        }
    }

    protected virtual void ContinousPreviousPressUpdate() {
        if (IsDestroyedOrDeactivated()) {
            Canvas.willRenderCanvases -= ContinousPreviousPressUpdate;
            return;
        }
        if (Snap) {
            if (Time.time - PreviousPressedTime > ContinuousPressTimeTreshold) {
                OnContiniousPreviousPressApplied();
                ContinuousPreviousChange();
            }
        } else {
            ContinuousPreviousChange();
        }
    }

    protected virtual void ContinousNextPressUpdate() {
        if (IsDestroyedOrDeactivated()) {
            Canvas.willRenderCanvases -= ContinousNextPressUpdate;
            return;
        }
        if (Snap) {
            if (Time.time - NextPressedTime > ContinuousPressTimeTreshold) {
                OnContiniousNextPressApplied();
                ContiniousNextChange();
            }
        } else {
            ContiniousNextChange();
        }
    }


    protected void OnWillRenderCanvasSnap() {
        if (Lerping) {
            if (UpdateNormalizedPosition(TargetSnap)) {
                Lerping = false;
                SnapEnded();
            }
        }
    }

    protected bool IsDestroyedOrDeactivated() {
        return GetScrollRect() == null || IsDestroyed() || !IsActive() || !enabled;
    }

    protected virtual void SnapEnded() {
        if (OnSnappingEnded != null) {
            OnSnappingEnded();
        }
    }

    public void ToggleSnap(bool snap) {
        Snap = snap;
    }
}