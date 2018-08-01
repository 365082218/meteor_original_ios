//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Util;

namespace Outfit7.Common {
    public static class FloatingNotificationPlugin {

        private const string Tag = "FloatingNotificationPlugin";

        private static bool? cachedIsSupported;

        public static bool IsSupported {
            get {

                if (cachedIsSupported.HasValue) {
                    return cachedIsSupported.Value;
                }

#if UNITY_EDITOR || NATIVE_SIM
                cachedIsSupported = true;
#elif UNITY_IPHONE
                cachedIsSupported = false;
#elif UNITY_ANDROID
                cachedIsSupported = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("isPopupSupported");
#elif UNITY_WP8
                cachedIsSupported = false;
#endif

                O7Log.VerboseT(Tag, "IsSupported: {0}", cachedIsSupported.Value);
                return cachedIsSupported.Value;
            }
        }

#if UNITY_EDITOR || NATIVE_SIM
        private static bool notificationEnabled = true;
#endif

        public static bool IsNotificationEnabled {
            get {
#if UNITY_EDITOR || NATIVE_SIM
                bool enabled = notificationEnabled;
#elif UNITY_IPHONE
                bool enabled = false;
#elif UNITY_ANDROID
                bool enabled = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("isPopupChecked");
#elif UNITY_WP8
                bool enabled = false;
#endif
                O7Log.VerboseT(Tag, "NotificationEnabled: {0}", enabled);
                return enabled;
            }
            set {
#if UNITY_EDITOR || NATIVE_SIM
                notificationEnabled = value;
#elif UNITY_IPHONE
                // do nothing
#elif UNITY_ANDROID
                Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("togglePopup", value);
#elif UNITY_WP8
                // do nothing
#endif
                O7Log.VerboseT(Tag, "NotificationEnabled: {0}", value);
            }
        }
    }
}
