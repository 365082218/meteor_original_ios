//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Util;

namespace Outfit7.Analytics.BigQuery {

    /// <summary>
    /// BigQuery event Unity-native plugin.
    /// </summary>
    public class BigQueryPlugin {

        protected const string Tag = "BigQueryPlugin";

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _AddBqEvent(string data);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _SendBqEventsToBackend(bool forceNotEnough, bool forceNoGrid);
#endif

        public virtual void AddEvent(string data) {
            O7Log.VerboseT(Tag, "AddEvent({0})", data);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _AddBqEvent(data);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("addBqEvent", data);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.BigQueryNativeProvider.AddEvent(data);
#endif
        }

        public virtual void SendEventsToBackend(bool forceNotEnough, bool forceNoGrid) {
            O7Log.VerboseT(Tag, "SendEventsToBackend(forceNotEnough={0}, forceNoGrid={1})", forceNotEnough, forceNoGrid);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SendBqEventsToBackend(forceNotEnough, forceNoGrid);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("sendBqEventsToBackend", forceNotEnough, forceNoGrid);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.BigQueryNativeProvider.SendEventsToBackend(forceNotEnough, forceNoGrid);
#endif
        }
    }
}
