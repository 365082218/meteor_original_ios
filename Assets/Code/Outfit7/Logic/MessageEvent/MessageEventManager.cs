using System.Collections.Generic;

using System;

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
using System.Text;
#endif

#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
#endif
using UnityEngine;
using Outfit7.Util;
using Outfit7.Logic.MessageEventInternal;

namespace Outfit7.Logic {

    public class MessageEventManager : Manager<MessageEventManager> {

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
        public static Dictionary<int, string> MessageEventsInfo = new Dictionary<int, string>();
        StringBuilder MessageEventInfoStringBuilder = new StringBuilder(256);
        public Action<MessageEvent, StringBuilder> OnMessageEventInfo;
#endif

#if !STRIP_TEST
        public bool BlockPostEvents;
#endif

        public class MessageEventDelegateComparer : IComparer<IMessageEventHandler> {
            public int Compare(IMessageEventHandler a, IMessageEventHandler b) {
                int ahashCode = a.GetHashCode();
                int bhashCode = b.GetHashCode();
                if (ahashCode < bhashCode) {
                    return -1;
                }
                if (ahashCode > bhashCode) {
                    return 1;
                }
                return 0;
            }
        }

        private const int InitialEventCount = 128;
        private const int InitialPoolSize = 32;
        private const string Tag = "MessageEventManager";

        private Queue<MessageEvent> EventPoolQueue;
        private Queue<MessageEventGroup> GroupPoolQueue;

        private Queue<MessageEvent> EventQueue;
        private MessageEventDelegateComparer Comparer;
        private Dictionary<int, MessageEventGroup> Groups;

        private int[] GlobalLockRefCount = new int[32];
        private int LockMask = 0;

        public object LastEventSender { get; private set; }

        public string LastEventSenderName { get; private set; }

        private MessageEvent GetEvent() {
            // Get new event
            MessageEvent messageEvent;
            if (EventPoolQueue.Count > 0) {
                messageEvent = EventPoolQueue.Dequeue();
            } else {
                messageEvent = new MessageEvent();
            }
            return messageEvent;
        }

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
        public string GetMessageEventDesc(MessageEvent msgEvent) {
            string name = "unknown";
            MessageEventsInfo.TryGetValue(msgEvent.EventId, out name);

            MessageEventInfoStringBuilder.Length = 0;
            MessageEventInfoStringBuilder.AppendFormat("[{0}] - [{1}]", name, msgEvent.ToString());
            MessageEventInfoStringBuilder.AppendLine();
            if (OnMessageEventInfo != null) {
                OnMessageEventInfo(msgEvent, MessageEventInfoStringBuilder);
            }
            return MessageEventInfoStringBuilder.ToString();
        }
#endif

        private void ProcessEvent(MessageEvent messageEvent) {
            // Get listeners
            MessageEventGroup messageEventGroup;
            if (!Groups.TryGetValue(messageEvent.EventId, out messageEventGroup) || !messageEventGroup.OnMessageEvent(messageEvent)) {
#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
                string desc = MessageEventManager.Instance.GetMessageEventDesc(messageEvent);
                Assert.IsTrue(false, "Message Event not handled: '{0}'", desc);
#endif
            }
            // Remeber last sender
            if (messageEvent.Sender != null) {
                LastEventSender = messageEvent.Sender;
                if (messageEvent.Sender is UnityEngine.Object) {
                    LastEventSenderName = (messageEvent.Sender as UnityEngine.Object).name;
                }
            } else {
                LastEventSender = null;
                LastEventSenderName = null;
            }
            // Push event back to pool
            EventPoolQueue.Enqueue(messageEvent);
        }

        private void UpdateQueue() {
            if (EventQueue.Count == 0) {
                return;
            }
            // Get next event
            MessageEvent messageEvent = EventQueue.Dequeue();
            ProcessEvent(messageEvent);
        }

        private bool SetLockMask(int mask) {
            int index = 0;
            for (int globalMask = 1; globalMask <= mask; globalMask <<= 1, ++index) {
                if (GlobalLockRefCount[index] > 0 && (globalMask & mask) != 0) {
                    return false;
                }
            }
            if ((mask & LockMask) != 0) {
                return false;
            }
            LockMask |= mask;
            return true;
        }

        public void Post(int eventHash, int intData0 = 0, int intData1 = 0, float floatData0 = 0.0f, float floatData1 = 0.0f, UnityEngine.Object objectData0 = null, object objectData1 = null, object userData = null, int lockMask = 0, object sender = null) {
#if !STRIP_TEST
            if (BlockPostEvents) {
                O7Log.DebugT(Tag, "Post: {0} blocked by running test!", eventHash);
                return;
            }
#endif

            // Check if locked
            if (!SetLockMask(lockMask)) {
                O7Log.DebugT(Tag, "Post: {0} locked!", eventHash); 
                return;
            }
            // Get new event
            MessageEvent messageEvent = GetEvent();
            // Set data
            messageEvent.EventId = eventHash;
            messageEvent.IntData0 = intData0;
            messageEvent.IntData1 = intData1;
            messageEvent.FloatData0 = floatData0;
            messageEvent.FloatData1 = floatData1;
            messageEvent.ObjectData0 = objectData0;
            messageEvent.ObjectData1 = objectData1;
            messageEvent.UserData = userData;
            messageEvent.Sender = sender;
            // Queue it
            EventQueue.Enqueue(messageEvent);

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
            string desc = MessageEventManager.Instance.GetMessageEventDesc(messageEvent);
            O7Log.DebugT(Tag, "Post: {0}", desc); 
#endif
        }

        public void PostNow(int eventHash, int intData0 = 0, int intData1 = 0, float floatData0 = 0.0f, float floatData1 = 0.0f, UnityEngine.Object objectData0 = null, object objectData1 = null, object userData = null, int lockMask = 0, object sender = null) {
            // Check if locked
            if (!SetLockMask(lockMask)) {
                O7Log.DebugT(Tag, "PostNow: {0} locked!", eventHash); 
                return;
            }
            // Get new event
            MessageEvent messageEvent = GetEvent();
            // Set data
            messageEvent.EventId = eventHash;
            messageEvent.IntData0 = intData0;
            messageEvent.IntData1 = intData1;
            messageEvent.FloatData0 = floatData0;
            messageEvent.FloatData1 = floatData1;
            messageEvent.ObjectData0 = objectData0;
            messageEvent.ObjectData1 = objectData1;
            messageEvent.UserData = userData;
            messageEvent.Sender = sender;

#if DEVEL_BUILD || PROD_BUILD || UNITY_EDITOR
            string desc = MessageEventManager.Instance.GetMessageEventDesc(messageEvent);
            O7Log.DebugT(Tag, "PostNow: {0}", desc);
#endif

            // Update it now
            ProcessEvent(messageEvent);
        }

        public void RegisterEvent(int eventHash, IMessageEventHandler messageEventDelegate) {
            // Get listeners
            MessageEventGroup messageEventGroup;
            if (!Groups.TryGetValue(eventHash, out messageEventGroup)) {
                // Get one from pool or create if empty
                if (GroupPoolQueue.Count > 0) {
                    messageEventGroup = GroupPoolQueue.Dequeue();
                    messageEventGroup.MessageEventHandlers.Clear();
                } else {
                    messageEventGroup = new MessageEventGroup();
                }
                Groups.Add(eventHash, messageEventGroup);
            }
            messageEventGroup.MessageEventHandlers.AddSorted(messageEventDelegate, Comparer);
        }

        public void UnregisterEvent(int eventHash, IMessageEventHandler messageEventDelegate) {
            // Get listeners
            MessageEventGroup messageEventGroup;
            if (!Groups.TryGetValue(eventHash, out messageEventGroup)) {
                return;
            }
            messageEventGroup.MessageEventHandlers.RemoveSorted(messageEventDelegate, Comparer);
        }

        public void LockGlobal(int bitIndex) {
            if (GlobalLockRefCount[bitIndex] < 0) throw new Exception(string.Format("Already locked bitIndex {0}", bitIndex));
            ++GlobalLockRefCount[bitIndex];
            if (GlobalLockRefCount[bitIndex] == 1) {
                O7Log.DebugT(Tag, "LockGlobal bitIndex {0}, refCount {1}", bitIndex, GlobalLockRefCount[bitIndex]);
            }
        }

        public void UnlockGlobal(int bitIndex) {
            --GlobalLockRefCount[bitIndex];
            if (GlobalLockRefCount[bitIndex] == 0) {
                O7Log.DebugT(Tag, "UnlockGlobal bitIndex {0}, refCount {1}", bitIndex, GlobalLockRefCount[bitIndex]);
            }
            if (GlobalLockRefCount[bitIndex] < 0) throw new Exception(string.Format("Still locked bitIndex {0}", bitIndex));
        }

        // Manager
        public override bool OnInitialize() {
            // Pre-fill pool queue
            EventPoolQueue = new Queue<MessageEvent>(InitialPoolSize);
            for (int i = 0; i < InitialPoolSize; ++i) {
                EventPoolQueue.Enqueue(new MessageEvent());
            }
            GroupPoolQueue = new Queue<MessageEventGroup>(InitialEventCount);
            for (int i = 0; i < InitialEventCount; ++i) {
                GroupPoolQueue.Enqueue(new MessageEventGroup());
            }
            // Initialize
            EventQueue = new Queue<MessageEvent>(InitialPoolSize);
            Comparer = new MessageEventDelegateComparer();
            Groups = new Dictionary<int, MessageEventGroup>(InitialEventCount);
            return base.OnInitialize();
        }

        public override void OnPreUpdate(float deltaTime) {
            base.OnPreUpdate(deltaTime);
            LockMask = 0;
            UpdateQueue();
        }

        public override void OnBucketUpdate(int index, float deltaTime) {
            base.OnBucketUpdate(index, deltaTime);
            UpdateQueue();
        }

    }

}