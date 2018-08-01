//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

using Outfit7.Util;

namespace Outfit7.Common.Analytics.AppsFlyer {

    /// <summary>
    /// AppsFlyer event Unity-native plugin.
    /// </summary>
    public class AppsFlyerPlugin {

        protected const string Tag = "AppsFlyerPlugin";

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _LogAppsFlyerEvent(string name, string paramz);
#endif

        public virtual void LogEvent(string name, string paramz) {
            O7Log.VerboseT(Tag, "LogEvent(name={0}, params={1})", name, paramz);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _LogAppsFlyerEvent(name, paramz);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("logAppsFlyerEvent", name, paramz);
#elif UNITY_WP8

#endif
        }
    }
}
