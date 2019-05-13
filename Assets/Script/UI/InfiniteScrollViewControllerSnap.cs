using UnityEngine;
using UnityEngine.EventSystems;

public partial class InfiniteScrollViewController : IBeginDragHandler, IEndDragHandler, IDragHandler {
    [Header("InfiniteScrollViewSnap")]
    [SerializeField] private bool OneCellPerSwipe = false;
    [SerializeField] private bool BlockTouchesAfterOnCellPerSwipe = true;
    [SerializeField] private float ClampVelocityValue = 0f;

    private bool DragInit = true;
    private int DragStartNearest;
    private bool SwipeNextCell = true;
    private bool OnCellDragged = false;

    // TODO: optimize with binary search
    private int FindNearest(float currentNormalizedPosition) {
        float distance = Mathf.Infinity;
        int output = 0;
        float velocity = 0f;

        if (GetScrollRect().horizontal) {
            velocity = GetScrollRect().velocity.x;
        } else if (GetScrollRect().vertical) {
            velocity = GetScrollRect().velocity.y;
        }
        int steps = (int) (Mathf.Abs(velocity) / PagesInOneSwipeDamp);
        if (MaxSteps > 0 && steps > MaxSteps) {
            steps = MaxSteps;
        }

        for (int index = 0; index < GetScrollRect().NormalizedPagePositions.Length; index++) {
            float npp = GetScrollRect().NormalizedPagePositions[index];
            float curdist = Mathf.Abs(npp - currentNormalizedPosition);
            if (curdist < distance) {
                distance = curdist;
                output = index;
            }
        }
        if (steps > 0) {
            if (velocity > 0) {
                output -= steps;
            } else if (velocity < 0) {
                output += steps;
            }
        }
        output = Mathf.Clamp(output, 0, GetScrollRect().NormalizedPagePositions.Length - 1);
        return output;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (!DragEnabled) {
            return;
        }

        if (OneCellPerSwipe && OnCellDragged && BlockTouchesAfterOnCellPerSwipe) {
            GetScrollRect().SetDragEnabled(false);
            return;
        }
        OnCellDragged = true;

        StopLerping();

        if (GetScrollRect().horizontal) {
            DragStartNearest = FindNearest(GetScrollRect().horizontalNormalizedPosition);
        } else {
            DragStartNearest = FindNearest(GetScrollRect().verticalNormalizedPosition);
        }

        //Debug.LogWarningFormat("OnBeginDrag: DragStartNearest: {0}", DragStartNearest);

        SwipeNextCell = true;

        if (!Snap)
            return;
    }

    public void OnDrag(PointerEventData eventData) {
        if (!GetScrollRect().IsDragEnabled()) {
            return;
        }

        StopLerping(false);
        Dragged = true;
        if (!Snap) {
            return;
        }
            
        if (DragInit) {
            DragInit = false;
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (!Snap || !GetScrollRect().IsDragEnabled()) {
            return;
        }

        Dragged = false;
        OnTouchEnd();
    }

    private void OnTouchEnd(int direction = 0) {
        if (IsDestroyedOrDeactivated()) {
            return;
        }

        float velocity = 0f;
        float normalizedPos = 0.5f;
        if (GetScrollRect().horizontal) {
            normalizedPos = GetScrollRect().horizontalNormalizedPosition;
            velocity = GetScrollRect().velocity.x;
            if (ClampVelocityValue > 0f) {
                float currentVel = Mathf.Clamp(velocity, -ClampVelocityValue, ClampVelocityValue);
                GetScrollRect().velocity = new Vector2(currentVel, GetScrollRect().velocity.y);
            }
        } else if (GetScrollRect().vertical) {
            normalizedPos = GetScrollRect().verticalNormalizedPosition;
            velocity = GetScrollRect().velocity.y;
            if (ClampVelocityValue > 0f) {
                float currentVel = Mathf.Clamp(velocity, -ClampVelocityValue, ClampVelocityValue);
                GetScrollRect().velocity = new Vector2(GetScrollRect().velocity.x, currentVel);
            }
        }

        // TODO: Snapping with a finite infinite scrollview doesn't work well at the beginning and at the end

        int targetIndex;

        if (direction == 0) {
            targetIndex = DragStartNearest;

            int newtargetIndex = FindNearest(normalizedPos);

            bool useinertia = true;
            if (OneCellPerSwipe && newtargetIndex != DragStartNearest) {
                useinertia = false;
            }

            if (useinertia && (Mathf.Abs(velocity) > InertiaCutoffMagnitude || FiniteLogic)) {
                if (velocity < 0) {
                    if (!OneCellPerSwipe || OneCellPerSwipe && normalizedPos > 0.5f) {
                        targetIndex = newtargetIndex + Mathf.RoundToInt(Mathf.Abs(velocity) / InertiaCutoffMagnitude);
                    }
                    if (FiniteLogic && ContainerCount > CellDataList.Count && targetIndex == 1 && CellDataList.Count % 2 == 0) {
                        targetIndex++;
                    }
                } else if (velocity >= 0) {
                    if (!OneCellPerSwipe || OneCellPerSwipe && normalizedPos < 0.5f) {
                        targetIndex = newtargetIndex - Mathf.RoundToInt(Mathf.Abs(velocity) / InertiaCutoffMagnitude);
                    }
                    if (FiniteLogic && ContainerCount > CellDataList.Count && targetIndex == 1 && CellDataList.Count % 2 == 0) {
                        targetIndex--;
                    }
                }
                targetIndex = Mathf.Clamp(targetIndex, 0, GetScrollRect().NormalizedPagePositions.Length - 1);
            }
//                if (FiniteLogic) {
//                    if (targetIndex == DragStartNearest) {
//                        targetIndex = newtargetIndex;
//                    }
//                    targetIndex = Mathf.Clamp(targetIndex, (ContainerCount / 2 - 1), ContainerCount - (ContainerCount / 2 - 1) - 1);
//                }
        } else {
            int dir = direction;
            targetIndex = FindNearest(normalizedPos);

            float targetPosition = GetScrollRect().NormalizedPagePositions[targetIndex];
            if ((dir > 0 && targetPosition - normalizedPos >= 0) ||
                (dir < 0 && normalizedPos - targetPosition >= 0)) {
                dir = 0;
            }

            targetIndex = Mathf.Clamp(targetIndex + dir, 0, GetScrollRect().NormalizedPagePositions.Length - 1);
        }

        if (normalizedPos > 0f && normalizedPos < 1f || FiniteLogic) {

            if (OneCellPerSwipe && !SwipeNextCell) {
                TargetSnap = 0.5f;
            } else {
                TargetSnap = Mathf.Clamp01(GetScrollRect().NormalizedPagePositions[targetIndex]);
            }
            CenterOnLerp = false;
            Lerping = true;
        }

        //Debug.LogWarningFormat("OnEndDragSnapBehaviour: DragStartNearest: {0}; targetIndex: {1}; velocity: {2}; TargetSnap: {3}", DragStartNearest, targetIndex, velocity, TargetSnap);

        DragInit = true;
    }

    protected override void SnapEnded() {
        base.SnapEnded();

        GetScrollRect().SetDragEnabled(DragEnabled);
        OnCellDragged = false;
        SwipeNextCell = true;
    }

    public void ToggleLerp(bool lerp) {
        Lerping = lerp;
    }

    public override void PreviousContinuousEnd() {
        Canvas.willRenderCanvases -= ContinousPreviousPressUpdate;
        if (Snap) {
            if (Time.time - PreviousPressedTime <= ContinuousPressTimeTreshold) {
                Previous();
                return;
            }
            OnTouchEnd(-1);
        }
    }

    public override void NextContinuousEnd() {
        Canvas.willRenderCanvases -= ContinousNextPressUpdate;
        if (Snap) {
            if (Time.time - NextPressedTime <= ContinuousPressTimeTreshold) {
                Next();
                return;
            }
            OnTouchEnd(1);
        }
    }

    protected override void OnContiniousNextPressApplied() {
        base.OnContiniousNextPressApplied();

        if (Snap) {
            StopLerping();
        }
    }

    protected override void OnContiniousPreviousPressApplied() {
        base.OnContiniousPreviousPressApplied();


        if (Snap) {
            StopLerping();
        }
    }
}