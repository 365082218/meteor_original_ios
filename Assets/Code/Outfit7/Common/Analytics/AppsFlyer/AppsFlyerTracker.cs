//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

using SimpleJSON;

namespace Outfit7.Common.Analytics.AppsFlyer {

    /// <summary>
    /// AppsFlyer event tracker.
    /// </summary>
    public class AppsFlyerTracker {

        public AppsFlyerPlugin AppsFlyerPlugin { get; set; }

        public virtual void LogEvent(string name, JSONNode paramz = null) {
            string paramzS = (paramz == null) ? null : paramz.ToString();
            AppsFlyerPlugin.LogEvent(name, paramzS);
        }
    }
}
