using UnityEngine;

namespace Outfit7.UI {
    public partial class ScrollViewController {
        private RectTransform CenterOnCellTransform = null;
        private RectTransform CenterOnNonCellTransform = null;
        private Vector2? CenterOnNormalizedPositionVector = null;
        private float CenteringNormalizedPosition = float.MinValue;

        public override void CenterOnCell(AbstractCellData data) {
            CenterOnCell(IndexOf(data));
        }

        public override void CenterOnCell(int index) {
            if (index > -1 && index < ContainerRectTransformList.Count) {
                CenterOnCellRectTransform(ContainerRectTransformList[index]);
            } else {
                CenterOnNormalizedPosition(new Vector2(0f, 1f));
            }
        }

        // Used for rect transforms that are in scrollview but are not cells, rectTransform should have a bottom middle anchor for now...
        public void CenterOnNonCellRectTransform(RectTransform target) {
            // Reset
            Canvas.willRenderCanvases -= OnWillRenderCanvasCenterOnNonCellTarget;

            Canvas.willRenderCanvases += OnWillRenderCanvasCenterOnNonCellTarget;

            CenterOnNonCellTransform = target;
        }

        protected virtual void OnWillRenderCanvasCenterOnNonCellTarget() {
            if (CenterOnNonCellTransform == null)
                return;

            float width = Content.rect.width;
            float height = Content.rect.height;

            float normalizedX = ScrollRect.horizontalNormalizedPosition;
            float normalizedY = ScrollRect.verticalNormalizedPosition;

            if (ScrollRect.horizontal) {
                float itemXPos = -Content.InverseTransformPoint(CenterOnNonCellTransform.position).x;
                float scrollViewWidth = (transform as RectTransform).rect.width;
                float val = 1f - ((itemXPos - scrollViewWidth * 0.5f) / (width - scrollViewWidth));
                normalizedX = Mathf.Clamp01(val);
            }
            if (ScrollRect.vertical) { 
                float itemYPos = -Content.InverseTransformPoint(CenterOnNonCellTransform.position).y;
                float scrollViewHeight = (transform as RectTransform).rect.height;
                float val = 1f - ((itemYPos - scrollViewHeight * 0.5f) / (height - scrollViewHeight));
                normalizedY = Mathf.Clamp01(val);
            }

            ScrollRect.normalizedPosition = new Vector2(normalizedX, normalizedY);

            Canvas.willRenderCanvases -= OnWillRenderCanvasCenterOnNonCellTarget;
            CenterOnNonCellTransform = null;

            OnScrollValueChanged(Vector2.zero);
        }

        public void CenterOnCellRectTransform(RectTransform target) {
            // Reset
            Canvas.willRenderCanvases -= OnWillRenderCanvasCenterOnCellTarget;

            Canvas.willRenderCanvases += OnWillRenderCanvasCenterOnCellTarget;

            CenterOnCellTransform = target;
        }

        protected virtual void OnWillRenderCanvasCenterOnCellTarget() {
            if (CenterOnCellTransform == null)
                return;

            Canvas.willRenderCanvases -= OnWillRenderCanvasCenterOnCellTarget;
            ScrollRect.normalizedPosition = GetCenterOnCellTargetPosition(CenterOnCellTransform);

            CenterOnCellTransform = null;

            OnScrollValueChanged(Vector2.zero);
        }

        public void CenterOnNormalizedPosition(Vector2 normalizedPosition) {
            // Reset
            Canvas.willRenderCanvases -= OnWillRenderCanvasCenterOnNormalizedPosition;

            Canvas.willRenderCanvases += OnWillRenderCanvasCenterOnNormalizedPosition;

            CenterOnNormalizedPositionVector = normalizedPosition;
        }

        protected virtual void OnWillRenderCanvasCenterOnNormalizedPosition() {
            if (CenterOnNormalizedPositionVector == null)
                return;

            float normalizedX = ScrollRect.horizontalNormalizedPosition;
            float normalizedY = ScrollRect.verticalNormalizedPosition;

            if (ScrollRect.horizontal) {
                normalizedX = Mathf.Clamp01(CenterOnNormalizedPositionVector.Value.x);
            }
            if (ScrollRect.vertical) {
                normalizedY = Mathf.Clamp01(CenterOnNormalizedPositionVector.Value.y);
            }

            ScrollRect.normalizedPosition = new Vector2(normalizedX, normalizedY);

            Canvas.willRenderCanvases -= OnWillRenderCanvasCenterOnNormalizedPosition;
            CenterOnNormalizedPositionVector = null;

            OnScrollValueChanged(Vector2.zero);
        }

        public override void CenterOnCellAnimated(int index, bool modal) {
            if (index < 0 || index >= ContainerRectTransformList.Count) {
                return;
            }
            EnableTouchRectTransform(!modal);
            ScrollRect.StopMovement();
            StopLerping();
            Canvas.willRenderCanvases -= CenterAnimated;
            Canvas.willRenderCanvases += CenterAnimated;
            Vector2 val = GetCenterOnCellTargetPosition(ContainerRectTransformList[index]);
            CenteringNormalizedPosition = GetScrollRect().horizontal ? val.x : val.y;
        }

        public override void CenterOnCellAnimated(AbstractCellData cellData, bool modal) {
            CenterOnCellAnimated(CellDataList.IndexOf(cellData), modal);
        }

        private void CenterAnimated() {
            if (IsDestroyed() || !IsActive() || !enabled) {
                if (!IsDestroyed() && TouchRectTransform != null && !TouchRectTransform.IsDestroyed()) {
                    EnableTouchRectTransform(true);
                }
                return;
            }
            if (CenteringNormalizedPosition == float.MinValue) {
                EnableTouchRectTransform(true);
                StopAnimating();
                return;
            }

            if (UpdateNormalizedPosition(CenteringNormalizedPosition)) {
                EnableTouchRectTransform(true);
                StopAnimating();
                if (OnCenteringEnded != null) {
                    OnCenteringEnded();
                }
            }
        }

        private void StopAnimating() {
            Canvas.willRenderCanvases -= CenterAnimated;
            CenteringNormalizedPosition = float.MinValue;
        }
    }
}
