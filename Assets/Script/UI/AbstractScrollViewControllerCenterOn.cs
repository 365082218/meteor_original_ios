using System;
using UnityEngine;
public abstract partial class AbstractScrollViewController {
    public Action OnCenteringEnded;

    public abstract void CenterOnCell(AbstractCellData data);

    public abstract void CenterOnCell(int index);

    public abstract void CenterOnCellAnimated(AbstractCellData data, bool modal);

    public abstract void CenterOnCellAnimated(int index, bool modal);

    protected bool Moving = false;

    public bool IsMoving {
        get {
            return Moving;
        }
    }

    protected virtual Vector2 GetCenterOnCellTargetPosition(RectTransform rectTransform) {
        float width = Content.rect.width;
        float height = Content.rect.height;

        float normalizedX = ScrollRect.horizontalNormalizedPosition;
        float normalizedY = ScrollRect.verticalNormalizedPosition;

        if (ScrollRect.horizontal) {
            float itemXPos = rectTransform.anchoredPosition.x;
            float scrollViewWidth = ScrollRect.viewport.rect.width;
            float val = (itemXPos - scrollViewWidth * 0.5f) / (width - scrollViewWidth);
            normalizedX = Mathf.Clamp01(val);
        }
        if (ScrollRect.vertical) {
            float itemYPos = -rectTransform.anchoredPosition.y;
            float scrollViewHeight = ScrollRect.viewport.rect.height;
            float val = 1f - ((itemYPos - scrollViewHeight * 0.5f) / (height - scrollViewHeight));
            normalizedY = Mathf.Clamp01(val);
        }

        return new Vector2(normalizedX, normalizedY);
    }

    protected bool UpdateNormalizedPosition(float target) {
        Moving = true;
        if (!ScrollSpeedLinear) {
            float factor = SnapSpeed * Time.smoothDeltaTime;
            if (ScrollRect.horizontal) {
                ScrollRect.horizontalNormalizedPosition = Mathf.Lerp(ScrollRect.horizontalNormalizedPosition, target, factor);
                //
                if (Mathf.Abs(ScrollRect.horizontalNormalizedPosition - target) < 0.00001f) {
                    ScrollRect.horizontalNormalizedPosition = target;
                    Moving = false;
                    return true;
                }
            } else {
                ScrollRect.verticalNormalizedPosition = Mathf.Lerp(ScrollRect.verticalNormalizedPosition, target, factor);
                if (Mathf.Abs(ScrollRect.verticalNormalizedPosition - target) < 0.00001f) {
                    ScrollRect.verticalNormalizedPosition = target;
                    Moving = false;
                    return true;
                }
            }

        } else {
            float delta = SnapSpeedLinear * Time.deltaTime;
            if (ScrollRect.horizontal) {
                if (ScrollRect.horizontalNormalizedPosition < target) {
                    ScrollRect.horizontalNormalizedPosition += delta;
                    if (ScrollRect.horizontalNormalizedPosition >= target) {
                        ScrollRect.horizontalNormalizedPosition = target;
                    }
                } else {
                    ScrollRect.horizontalNormalizedPosition -= delta;
                    if (ScrollRect.horizontalNormalizedPosition <= target) {
                        ScrollRect.horizontalNormalizedPosition = target;
                    }
                }
                //
                if (Mathf.Abs(ScrollRect.horizontalNormalizedPosition - target) < 0.00001f) {
                    Moving = false;
                    return true;
                }
            } else {
                if (ScrollRect.verticalNormalizedPosition < target) {
                    ScrollRect.verticalNormalizedPosition += delta;
                    if (ScrollRect.verticalNormalizedPosition >= target) {
                        ScrollRect.verticalNormalizedPosition = target;
                    }
                } else {
                    ScrollRect.verticalNormalizedPosition -= delta;
                    if (ScrollRect.verticalNormalizedPosition <= target) {
                        ScrollRect.verticalNormalizedPosition = target;
                    }
                }
                //
                if (Mathf.Abs(ScrollRect.verticalNormalizedPosition - target) < 0.001f) {
                    Moving = false;
                    return true;
                }
            }
        }
        //
        return false;
    }
}
