//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;

namespace Outfit7.Common {

    /// <summary>
    /// Local reminder plugin.
    /// </summary>
    public static class ReminderPlugin {

        private const string Tag = "ReminderPlugin";
        public const string SoundEnabledPref = "ReminderPlugin.SoundEnabled";

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ClearAllReminders();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetReminder(string id, string text, double fireTimeSecondsSince1970, bool playSound);
#endif

        public static void ClearAllReminders() {
            O7Log.VerboseT(Tag, "ClearAllReminders()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _ClearAllReminders();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("clearAllReminders");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.NotificationNativeProvider.ClearAllReminders();
#endif
        }

        public static void SetReminder(string id, string text, DateTime fireTime) {
            SetReminder(id, text, fireTime, null, null);
        }

        public static void SetReminder(string id, string text, DateTime fireTime, string button1Text, string button1Action) {
            SetReminder(id, text, fireTime, button1Text, button1Action, null, null);
        }

        public static void SetReminder(string id, string text, DateTime fireTime, string button1Text, string button1Action, string button2Text, string button2Action) {
            // Limit fireTime to fire after 1 minute soonest
            DateTime nowPlus1Min = DateTime.UtcNow + TimeSpan.FromMinutes(1);
            if (nowPlus1Min > fireTime.ToUniversalTime()) {
                fireTime = nowPlus1Min;
            }
            bool playSound = UserPrefs.GetBool(SoundEnabledPref, true);
            O7Log.VerboseT(Tag, "SetReminder({0}, '{1}', {2}, {3}, button1Text {4}, button1Action {5}, button1Text {6}, button1Action {7})",
                id, text, fireTime.ToLocalTime(), playSound, button1Text, button1Action, button2Text, button2Action);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            double fireTimeSecondsSince1970 = Outfit7.Util.TimeUtils.ToTimeMillis(fireTime) / 1000L;
            _SetReminder(id, text, fireTimeSecondsSince1970, playSound);
#elif UNITY_ANDROID
            long fireTimeSince1970 = Outfit7.Util.TimeUtils.ToTimeMillis(fireTime);
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setReminder", id, text, fireTimeSince1970, playSound, button1Text, button1Action, button2Text, button2Action);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.NotificationNativeProvider.SetReminder(id, text, fireTime, playSound);
#endif
        }
    }
}
