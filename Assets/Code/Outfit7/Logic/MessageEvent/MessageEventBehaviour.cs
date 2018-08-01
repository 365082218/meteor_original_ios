using System.Collections.Generic;
using UnityEngine;
using System;

namespace Outfit7.Logic {
    public class MessageEventBehaviour : BucketUpdateBehaviour, IMessageEventHandler {

        [HideInInspector]
        public bool AutoRegister;
        private bool IsRegistered;
        [SerializeField, HideInInspector] private List<int> MessageEvents = new List<int>();

#if UNITY_EDITOR
        [HideInInspector]
        public bool EditorShowMessageEventsGroup = false;
#endif

        public virtual bool OnMessageEvent(MessageEvent msgEvent) {
            return false;
        }

        protected override void OnEnable() {
            base.OnEnable();

            if (AutoRegister) {
                RegisterMessageEvents();
            }
        }

        protected override void OnDisable() {
            base.OnDisable();

            if (AutoRegister && IsRegistered) {
                UnregisterMessageEvents();
            }
        }

        public void RegisterMessageEvents() {
            if (IsRegistered) throw new Exception(string.Format("Message events already registered: {0}", this.name));

            IsRegistered = true;
            for (int a = 0; a < MessageEvents.Count; a++) {
                MessageEventManager.Instance.RegisterEvent(MessageEvents[a], this);
            }
        }

        public void UnregisterMessageEvents() {
            if (!IsRegistered) throw new Exception(string.Format("Message events not registered: {0}", this.name));

            IsRegistered = false;
            for (int a = 0; a < MessageEvents.Count; a++) {
                MessageEventManager.Instance.UnregisterEvent(MessageEvents[a], this);
            }
        }

#if UNITY_EDITOR
        public List<int> EditorGetMessageEvents() {
            return MessageEvents;
        }
#endif
    }

    // add custom editor, fill list from checkbox of MessageEvents
}