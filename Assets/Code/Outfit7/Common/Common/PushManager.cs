//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Common;
using Outfit7.Event;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Common {

    /// <summary>
    /// Push subscription manager.
    /// </summary>
    public class PushManager {

        protected const string Tag = "PushManager";
        protected const string DidAutoSubscribeKey = "PushManager.DidAutoSubscribe";

        public static void ClearPrefs() {
            UserPrefs.Remove(DidAutoSubscribeKey);
        }

        protected bool DidAutoSubscribe;

        public bool ArePushNotificationsAvailable { get; private set; }

        public bool SubscribedToPushNotifications { get; private set; }

        public EventBus EventBus { get; set; }

        public PushPlugin PushPlugin { get; set; }

        public virtual void Init() {
            DidAutoSubscribe = UserPrefs.GetBool(DidAutoSubscribeKey, false);
            O7Log.DebugT(Tag, "DidAutoSubscribe {0}", DidAutoSubscribe);
            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, OnGridChange);
            EventBus.AddListener(CommonEvents.GRID_IAPS_CHANGE, OnGridChange);
        }

        protected virtual void OnGridChange(object eventData) {

            TryToAutoSuscribe();
        }

        private void TryToAutoSuscribe() {

            if (!PushPlugin.AutoSubscribeAvailable) {
                return;
            }

            if (DidAutoSubscribe) {
                return;
            }

            if (!ArePushNotificationsAvailable) {
                return;
            }

            if (SubscribedToPushNotifications) {
                MarkDidAutoSuscribe(); // user already subscribed - don't schedule auto subscription
                return;
            }

            if (AutoSubscribe()) {
                MarkDidAutoSuscribe();
            }
        }

        private void MarkDidAutoSuscribe() {

            if (DidAutoSubscribe) {
                return;
            }

            O7Log.DebugT(Tag, "MarkDidAutoSuscribe subscribed: {0}", SubscribedToPushNotifications);
            DidAutoSubscribe = true;
            UserPrefs.SetBool(DidAutoSubscribeKey, DidAutoSubscribe);
        }

        protected virtual bool AutoSubscribe() {
            // try to autosubscribe - check iap if subscribe available
            O7Log.DebugT(Tag, "Try to autosubscribe");
            return false;
        }

        internal virtual void SetPushNotificationStart(string payload) {
            JSONNode payloadJ;
            try {
                payloadJ = JSON.Parse(payload);

            } catch (Exception e) {
                O7Log.ErrorT(Tag, e, "Push notification start payload is not parsable");
                return;
            }

            if (payloadJ["id"] == null) {
                O7Log.ErrorT(Tag, "Push notification start payload is missing 'id' field");
                return;
            }

            EventBus.FireEvent(CommonEvents.PUSH_START, payloadJ);
        }

        internal virtual void OnPushNotification(string payload) {
            JSONNode payloadJ;
            try {
                payloadJ = JSON.Parse(payload);

            } catch (Exception e) {
                O7Log.ErrorT(Tag, e, "Push notification payload is not parsable");
                return;
            }

            if (payloadJ["id"] == null) {
                O7Log.ErrorT(Tag, "Push notification payload is missing 'id' field");
                return;
            }

            EventBus.FireEvent(CommonEvents.PUSH_RECEIVE, payloadJ);
        }

        internal virtual void SetSubscribedToPushNotifications(bool subscribed) {
            ArePushNotificationsAvailable = true;

            bool oldSubscribed = SubscribedToPushNotifications;
            SubscribedToPushNotifications = subscribed;

            OnSubscribeToPushNotifications(oldSubscribed);
        }

        protected virtual void OnSubscribeToPushNotifications(bool oldSubscribed) {
            if (oldSubscribed != SubscribedToPushNotifications) {
                EventBus.FireEvent(CommonEvents.PUSH_REGISTRATION, SubscribedToPushNotifications);
            }

            if (SubscribedToPushNotifications) {
                MarkDidAutoSuscribe();
            } else {
                TryToAutoSuscribe();
            }
        }

        public virtual void SubscribeToPushNotifications(bool showIntro) {
            SubscribeToPushNotifications(showIntro, true);
        }

        public virtual void SubscribeToPushNotifications(bool showIntro, bool showProgress) {
            if (!showIntro) {
                Assert.State(ArePushNotificationsAvailable, "Not available");
            }
            PushPlugin.StartSubscribingToPushNotifications(showIntro, showProgress);
        }

        public virtual void UnsubscribeFromPushNotifications() {
            Assert.State(ArePushNotificationsAvailable, "Not available");
            PushPlugin.StartUnsubscribingFromPushNotifications();
        }
    }
}
