using System.Collections;
using UnityEngine;
using Outfit7.Audio;
using Outfit7.Util;
using Outfit7.UI;
using UnityEngine.EventSystems;
using System;

namespace Outfit7.Logic {

    public class MessageEventActionButton : AbstractActionButton {

        private const float MinDialogActionTime = 0.1f;
        protected PointerEventData LastPointerEventData = null;

        [SerializeField] protected AudioEventObject AudioEventUp = new AudioEventObject();
        [SerializeField] protected AudioEventObject AudioEventDown = new AudioEventObject();

        [SerializeField] protected string DownGameAction;
        [SerializeField] protected string CancelGameAction;
        [SerializeField] protected string ClickGameAction;

        public enum ButtonAction {
            Enter,
            Exit,
            Down,
            Up,
            Click,
            BeginDrag,
            Drag,
            EndDrag,
        }

        public delegate void OnButtonAction(ButtonAction btnAction, PointerEventData pointerEventData);

        public OnButtonAction ButtonActionDelegate;

        public MessageEvent MessageEventData = new MessageEvent();

        public IEnumerator PlayAudioEventUpCoroutine() {            
            yield return null;
            AudioEventUp.Play();
        }

        private void PlayAudioEventUp() {
            if (!isActiveAndEnabled) {
                return;
            }
            StartCoroutine(PlayAudioEventUpCoroutine());
        }

        private void PlayAudioEventDown() {
            if (!isActiveAndEnabled) {
                return;
            }
            AudioEventDown.Play();
        }

        // ActionButton
        protected override bool DelayTouch() {
            return DelayedTouch && Time.unscaledTime - AwakeTime < MinDialogActionTime;
        }

        // UI.Button
        protected override void Awake() {
            base.Awake();

            OnPointerUpSuccess += PlayAudioEventUp;
            OnPointerUpFailed += PlayAudioEventUp;

            OnPointerDownSuccess += PlayAudioEventDown;
            OnPointerDownFailed += PlayAudioEventDown;
        }

        protected override void OnStartErrorCheck() {
            if (!IsClickActionSet() && !IsDownActionSet() && !IsCancelActionSet()) {
                throw new System.Exception(string.Format("No game action set on button '{0}' in scene state '{1}'!", name, SceneStateManager.Instance.GetActiveState()));
            }
        }

        protected override bool IsDownActionSet() {
            return !string.IsNullOrEmpty(DownGameAction);
        }

        protected override bool IsCancelActionSet() {
            return !string.IsNullOrEmpty(CancelGameAction);
        }

        protected override bool IsClickActionSet() {
            return !string.IsNullOrEmpty(ClickGameAction);
        }

        protected override bool CanTriggerPointerDown() {
            if (!IsDownActionSet() && !IsClickActionSet()) {
                return false;
            }

            if (!CanTouch()) {
                return false;
            }

            return true;
        }

        protected override bool CanTriggerPointerCancel() {
            if (!IsCancelActionSet()) {
                return false;
            }

            if (!CanTouch()) {
                return false;
            }

            return true;
        }

        protected override bool CanTriggerPointerClick() {
            if (!IsClickActionSet()) {
                return false;
            }

            if (!CanTouch()) {
                return false;
            }

            return true;
        }

        protected void DoPostWithData(int hashId, MessageEvent data, PointerEventData pointerEventData) {
            MessageEventManager.Instance.Post(hashId, data.IntData0, data.IntData1, data.FloatData0, data.FloatData1, data.ObjectData0, UserActionData, pointerEventData.button, 1, gameObject);
        }

        protected override bool FireActionDown() {
            DoPostWithData(DownGameAction.GetHashCode(), MessageEventData, LastPointerEventData);
            return true;
        }

        protected override bool FireActionCancel() {
            DoPostWithData(CancelGameAction.GetHashCode(), MessageEventData, LastPointerEventData);
            return true;
        }

        protected override bool FireActionClick() {
            DoPostWithData(ClickGameAction.GetHashCode(), MessageEventData, LastPointerEventData);
            return true;
        }

        private void SetEventPosition(PointerEventData pointerEventData) {
            MessageEventData.FloatData0 = pointerEventData.position.x;
            MessageEventData.FloatData1 = pointerEventData.position.y;
        }

        public override void OnPointerDown(PointerEventData pointerEventData) {
            SetEventPosition(pointerEventData);
            LastPointerEventData = pointerEventData;
            base.OnPointerDown(pointerEventData);
            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.Down, pointerEventData);
            }
        }

        public override void OnPointerUp(PointerEventData pointerEventData) {
            SetEventPosition(pointerEventData);
            LastPointerEventData = pointerEventData;
            base.OnPointerUp(pointerEventData);
            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.Up, pointerEventData);
            }
        }

        public override void OnPointerClick(PointerEventData pointerEventData) {
            SetEventPosition(pointerEventData);
            LastPointerEventData = pointerEventData;
            base.OnPointerClick(pointerEventData);
            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.Click, pointerEventData);
            }
        }

        public override void OnPointerEnter(PointerEventData pointerEventData) {
            LastPointerEventData = pointerEventData;
            base.OnPointerEnter(pointerEventData);
            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.Enter, pointerEventData);
            }
        }

        public override void OnPointerExit(PointerEventData pointerEventData) {
            LastPointerEventData = pointerEventData;
            base.OnPointerExit(pointerEventData);
            if (ButtonActionDelegate != null) {
                ButtonActionDelegate.Invoke(ButtonAction.Exit, pointerEventData);
            }
        }

    }
}
