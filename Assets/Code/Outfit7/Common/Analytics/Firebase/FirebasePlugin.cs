//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Analytics.Firebase {

    /// <summary>
    /// Firebase plugin for native calls.
    /// </summary>
    public class FirebasePlugin {

        protected const string Tag = "FirebasePlugin";

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        protected static extern void _LogFirebaseEvent(string eventName, string jsonMap);
#endif

        public virtual void LogEvent(string eventName, IDictionary<string, string> parameters) {

            JSONClass j = new JSONClass();
            foreach (KeyValuePair<string, string> param in parameters) {
                j[param.Key] = param.Value;
            }
            string jsonMap = j.ToString();

            O7Log.VerboseT(Tag, "LogEvent({0} {1})", eventName, jsonMap);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _LogFirebaseEvent(eventName, jsonMap);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("logFirebaseEvent", eventName, jsonMap);
#endif
        }
    }
}
