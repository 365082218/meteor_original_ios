//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Util;

namespace Outfit7.Common {

    /// <summary>
    /// Push notification plugin.
    /// </summary>
    public class PushPlugin : MonoBehaviour {

        private const string Tag = "PushPlugin";

        public PushManager PushManager { get; set; }

#region Push on Start

#if UNITY_EDITOR || NATIVE_SIM
        // Init push start & subscription in Unity editor only
        private void OnApplicationPause(bool paused) {
            if (paused) return;

//            string payload =
//@"
//{
//            ""id"": ""awake"",
//            ""social"": ""true"",
//}
//";
//            _SetPushNotificationStart(payload);

            PushManager.SetSubscribedToPushNotifications(false);
        }
#endif

#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        private Outfit7.Threading.Executor MainExecutor;

        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
            O7.Plugins.Wp8.UnityCommon.NotificationNativeProvider.OnPushNotification += __OnPushNotification;
            O7.Plugins.Wp8.UnityCommon.NotificationNativeProvider.OnPushNotificationStart += __SetPushNotificationStart;
            O7.Plugins.Wp8.UnityCommon.NotificationNativeProvider.OnSubscribedToPushNotifications += __SetSubscribedToPushNotifications;
        }

        private void __OnPushNotification(string data) {
            MainExecutor.Post(delegate {
                _OnPushNotification(data);
            });
        }

        private void __SetPushNotificationStart(string data) {
            MainExecutor.Post(delegate {
                _SetPushNotificationStart(data);
            });
        }

        private void __SetSubscribedToPushNotifications(bool subscribed) {
            MainExecutor.Post(delegate {
                SetSubscribedToPushNotifications(subscribed);
            });
        }
#endif

        public bool AutoSubscribeAvailable {
            get {
#if UNITY_EDITOR || NATIVE_SIM
                return true;
#elif UNITY_IPHONE
                return false; // ios requires user approval for push
#else
                return true; // android & wp does not require user approval to subscribe
#endif
            }
        }

        public void _SetPushNotificationStart(string payload) {
            O7Log.VerboseT(Tag, "_SetPushNotificationStart({0})", payload);
            PushManager.SetPushNotificationStart(payload);
        }

        public void _OnPushNotification(string payload) {
            O7Log.VerboseT(Tag, "_OnPushNotification({0})", payload);
            PushManager.OnPushNotification(payload);
        }

#endregion

#region Push Subscription

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartSubscribingToPushNotifications(bool showIntro);
#endif

        private void SetSubscribedToPushNotifications(bool subscribed) {
            O7Log.VerboseT(Tag, "SetSubscribedToPushNotifications({0})", subscribed);
            PushManager.SetSubscribedToPushNotifications(subscribed);
        }

        public void _SetSubscribedToPushNotifications(string subscribed) {
            SetSubscribedToPushNotifications(subscribed == "true");
        }

        public void StartSubscribingToPushNotifications(bool showIntro) {
            StartSubscribingToPushNotifications(showIntro, true);
        }

        public void StartSubscribingToPushNotifications(bool showIntro, bool showProgress) {
            O7Log.VerboseT(Tag, "StartSubscribingToPushNotifications({0},{1})", showIntro, showProgress);

#if UNITY_EDITOR || NATIVE_SIM
            _SetSubscribedToPushNotifications("true");
#elif UNITY_IPHONE
            _StartSubscribingToPushNotifications(showIntro);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("startSubscribingToPushNotifications", showIntro, showProgress);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.NotificationNativeProvider.StartSubscribingToPushNotifications(showIntro);
#endif
        }

        public void StartUnsubscribingFromPushNotifications() {
            O7Log.VerboseT(Tag, "StartUnsubscribingFromPushNotifications()");

#if UNITY_EDITOR || NATIVE_SIM
            _SetSubscribedToPushNotifications("false");
#elif UNITY_IPHONE

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("startUnsubscribingFromPushNotifications");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.NotificationNativeProvider.StartUnsubscribingFromPushNotifications();
#endif
        }

#endregion
    }
}
