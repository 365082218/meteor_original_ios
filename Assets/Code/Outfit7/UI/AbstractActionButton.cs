using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Outfit7.UI {
    public abstract class AbstractActionButton : UnityEngine.UI.Button {

        protected const float DefaultDelayTouchTime = 0.3f;

        [SerializeField] protected bool DelayedTouch = false;
        public Action OnPointerDownSuccess;
        public Action OnPointerDownFailed;
        public Action OnPointerUpSuccess;
        public Action OnPointerUpFailed;
        protected float AwakeTime;

        public object UserActionData { get; set; }

        public bool Pressed {
            get { return IsPressed(); }
        }

        private bool HasExitedButtonWhileDown = false;

        protected abstract void OnStartErrorCheck();

        protected abstract bool IsDownActionSet();

        protected abstract bool IsClickActionSet();

        protected abstract bool IsCancelActionSet();

        protected abstract bool CanTriggerPointerDown();

        protected abstract bool CanTriggerPointerCancel();

        protected abstract bool CanTriggerPointerClick();

        protected abstract bool FireActionDown();

        protected abstract bool FireActionCancel();

        protected abstract bool FireActionClick();

        protected override void Awake() {
            base.Awake();

            AwakeTime = Time.unscaledTime;
        }

        protected override void Start() {
            base.Start();

            if (Application.isPlaying) {
                OnStartErrorCheck();
            }
        }

        protected virtual bool DelayTouch() {
            return DelayedTouch && Time.unscaledTime - AwakeTime < DefaultDelayTouchTime;
        }

        protected virtual bool CanTouch() {
            return IsInteractable() && !DelayTouch();
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            // empty because when the touch goes back into the object it shouldn't react
        }

        public override void OnPointerExit(PointerEventData eventData) {
            if (!IsPressed()) {
                // the following line fixes a problem where Cancel was never fired e.g. Age gate scrollview
                if (eventData.lastPress == null || currentSelectionState != SelectionState.Highlighted) {
                    base.OnPointerExit(eventData);
                    return;
                }
            }

            if (CanTriggerPointerCancel()) {
                FireActionCancel();
            }
            base.OnPointerExit(eventData);
            if (OnPointerUpFailed != null) {
                OnPointerUpFailed();
            }

            HasExitedButtonWhileDown = true;

            // clear everything - we don't want to do anything with the button anymore
            InstantClearState();
            eventData.selectedObject = null;
        }

        public override void OnPointerClick(PointerEventData eventData) {
            if (HasExitedButtonWhileDown || !CanTriggerPointerClick()) {
                return;
            }

            if (FireActionClick()) {
                base.OnPointerClick(eventData);
                if (OnPointerUpSuccess != null) {
                    OnPointerUpSuccess();
                }
            } else {
                if (OnPointerUpFailed != null) {
                    OnPointerUpFailed();
                }
            }

            // clear everything - we don't want to do anything with the button anymore
            InstantClearState();
            eventData.selectedObject = null;
        }

        public override void OnPointerDown(PointerEventData eventData) {
            // OnPointerEnter is here because it can't be called in its function (see reason above)
            base.OnPointerEnter(eventData);
            HasExitedButtonWhileDown = false;

            if (!CanTriggerPointerDown()) {
                return;
            }

            bool success = false;
            if (IsDownActionSet()) {
                if (FireActionDown()) {
                    base.OnPointerDown(eventData);
                    success = true;
                    if (OnPointerDownSuccess != null) {
                        OnPointerDownSuccess();
                    }
                }
            } else if (IsClickActionSet()) {
                success = true;
                base.OnPointerDown(eventData); // OnPointerClick doesn't work otherwise
                if (OnPointerDownSuccess != null) {
                    OnPointerDownSuccess();
                }
            }

            if (!success) {
                if (OnPointerDownFailed != null) {
                    OnPointerDownFailed();
                }
            }
        }
    }
}
