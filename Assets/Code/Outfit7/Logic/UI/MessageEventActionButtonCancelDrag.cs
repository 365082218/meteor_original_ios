using UnityEngine;
using UnityEngine.EventSystems;

namespace Outfit7.Logic {

    public class MessageEventActionButtonCancelDrag : MessageEventActionButton, IBeginDragHandler, IDragHandler, IEndDragHandler {

        protected float DragDelta;
        protected Vector2 LastDragPosition;

        [Tooltip("Smaller value cancels click sooner, if user is click-hold & dragging on a button."), Range(0.0f, 1.0f)]
        public float CancelClickWhenMovingFactor = 0.05f;

        protected override bool FireActionClick() {
            float maxDelta = Screen.width * CancelClickWhenMovingFactor;
            if (maxDelta > 0.0f) {
                if (DragDelta >= maxDelta) {
                    return false;
                }
            }

            DoPostWithData(ClickGameAction.GetHashCode(), MessageEventData, LastPointerEventData);
            return true;
        }

        public override void OnPointerClick(PointerEventData pointerEventData) {
            Vector2 delta = pointerEventData.position - LastDragPosition;
            DragDelta += delta.magnitude;
            LastDragPosition = pointerEventData.position;
            base.OnPointerClick(pointerEventData);
        }

        public override void OnPointerDown(PointerEventData pointerEventData) {
            DragDelta = 0.0f;
            LastDragPosition = pointerEventData.position;
            base.OnPointerDown(pointerEventData);
        }

        public virtual void OnBeginDrag(PointerEventData pointerEventData) {
            if (!interactable) {
                return;
            }

            DragDelta = 0.0f;
            LastDragPosition = pointerEventData.position;

            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.BeginDrag, pointerEventData);
            }
        }

        public virtual void OnDrag(PointerEventData pointerEventData) {
            if (!interactable) {
                return;
            }

            Vector2 delta = pointerEventData.position - LastDragPosition;
            DragDelta += delta.magnitude;
            LastDragPosition = pointerEventData.position;

            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.Drag, pointerEventData);
            }
        }

        public virtual void OnEndDrag(PointerEventData pointerEventData) {
            if (!interactable) {
                return;
            }

            Vector2 delta = pointerEventData.position - LastDragPosition;
            DragDelta += delta.magnitude;
            LastDragPosition = pointerEventData.position;

            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.EndDrag, pointerEventData);
            }
        }
    }
}