using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Outfit7.UI {
    [AddComponentMenu("Layout/Embedded Content Size Fitter", 201)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class EmbeddedContentSizeFitter : UIBehaviour, ILayoutSelfController {

        [SerializeField] protected bool m_HorizontalFit = false;

        public bool horizontalFit {
            get { return m_HorizontalFit; }
            set {
                if (m_HorizontalFit != value) {
                    m_HorizontalFit = value;
                    SetDirty();
                }
            }
        }

        [SerializeField] protected bool m_VerticalFit = false;

        public bool verticalFit {
            get { return m_VerticalFit; }
            set {
                if (m_VerticalFit != value) {
                    m_VerticalFit = value;
                    SetDirty();
                }
            }
        }

        [SerializeField] protected RectTransform SizeTemplateRectTransform = null;
        [SerializeField] private Vector2 m_MaximumSize = Vector2.zero;

        private Vector2 MaximumSize {
            get { 
                if (m_MaximumSize == Vector2.zero) {
                    m_MaximumSize = rectTransform.sizeDelta;
                }
                return m_MaximumSize;
            }
        }

        private Rect MaximumSizeRect {
            get { 
                return new Rect(new Vector2(transform.position.x - MaximumSize.x / 2f, transform.position.y - MaximumSize.y / 2f), MaximumSize);
            }
        }

        [System.NonSerialized] private RectTransform m_Rect;

        private RectTransform rectTransform {
            get {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private DrivenRectTransformTracker m_Tracker;

        protected override void OnRectTransformDimensionsChange() {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis) {
            bool fitting = (axis == 0 ? horizontalFit : verticalFit);
            if (!fitting)
                return;

            m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            float maximum = 0f;
            if (SizeTemplateRectTransform == null) {
                // use maximum rect instead of size template
                maximum = axis == 0 ? MaximumSizeRect.width : MaximumSizeRect.height;
            } else {
                maximum = axis == 0 ? SizeTemplateRectTransform.rect.width : SizeTemplateRectTransform.rect.height;
            }

            float preferredSize = LayoutUtility.GetPreferredSize(m_Rect, axis);

            rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis) axis, Mathf.Min(preferredSize, maximum));
        }

        public virtual void SetLayoutHorizontal() {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        public virtual void SetLayoutVertical() {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty() {
            if (!IsActive())
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            Rect maximumSizeRect = MaximumSizeRect;
            float horizontalOffset = rectTransform.sizeDelta.x / 2f - rectTransform.sizeDelta.x * rectTransform.pivot.x;
            float verticalOffset = rectTransform.sizeDelta.y / 2f - rectTransform.sizeDelta.y * rectTransform.pivot.y;
            Vector3 bottomLeft = transform.TransformPoint(new Vector3(maximumSizeRect.min.x + horizontalOffset, maximumSizeRect.min.y + verticalOffset, 0f));
            Vector3 bottomRight = transform.TransformPoint(new Vector3(maximumSizeRect.max.x + horizontalOffset, maximumSizeRect.min.y + verticalOffset, 0f));
            Vector3 topRight = transform.TransformPoint(new Vector3(maximumSizeRect.max.x + horizontalOffset, maximumSizeRect.max.y + verticalOffset, 0f));
            Vector3 topLeft = transform.TransformPoint(new Vector3(maximumSizeRect.min.x + horizontalOffset, maximumSizeRect.max.y + verticalOffset, 0f));
            Gizmos.DrawLine(bottomLeft, bottomRight);
            Gizmos.DrawLine(bottomRight, topRight);
            Gizmos.DrawLine(topRight, topLeft);
            Gizmos.DrawLine(topLeft, bottomLeft);
        }

        protected override void OnValidate() {
            SetDirty();
        }
        #endif
    }
}