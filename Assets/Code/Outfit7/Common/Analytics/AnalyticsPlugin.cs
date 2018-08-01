//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Analytics.BigQuery;
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Analytics {
    public class AnalyticsPlugin : MonoBehaviour {

        protected const string Tag = "AnalyticsPlugin";

#if UNITY_IPHONE
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _GetAnalyticsCollectionEnabled();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetAnalyticsCollectionEnabled(bool enabled);
#endif

        public BigQueryTracker BqTracker { get; set; }

        protected bool? cachedIsAnalyticsEnabled;

#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        private Outfit7.Threading.Executor MainExecutor;

        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
            O7.Plugins.Wp8.UnityCommon.AnalyticsNativeProvider.OnAnalyticsCollectionEnabledChanged += __SetAnalyticsCollectionEnabledChange;
        }

        private void __SetAnalyticsCollectionEnabledChange(string data) {
            MainExecutor.Post(delegate{
                _SetAnalyticsCollectionEnabledChange(data);
            });
        }
#endif

        public virtual bool IsEnabled {
            get {
                if (!cachedIsAnalyticsEnabled.HasValue) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedIsAnalyticsEnabled = true;
#elif UNITY_IPHONE
                    cachedIsAnalyticsEnabled = _GetAnalyticsCollectionEnabled();
#elif UNITY_ANDROID
                    cachedIsAnalyticsEnabled = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("isAnalyticsCollectionEnabled");
#elif UNITY_WP8
                    cachedIsAnalyticsEnabled = O7.Plugins.Wp8.UnityCommon.AnalyticsNativeProvider.IsAnalyticsCollectionEnabled();
#endif
                    O7Log.DebugT(Tag, "IsEnabled: {0}", cachedIsAnalyticsEnabled);
                }
                return cachedIsAnalyticsEnabled.Value;
            }
        }

        public virtual void SetEnabled(bool enabled) {
            O7Log.VerboseT(Tag, "SetEnabled({0})", enabled);

#if UNITY_EDITOR || NATIVE_SIM
            SetAnalyticsCollectionEnabledChange(enabled);
#elif UNITY_IPHONE
            _SetAnalyticsCollectionEnabled(enabled);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setAnalyticsCollectionEnabled", enabled);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.AnalyticsNativeProvider.SetAnalyticsCollectionEnabled(enabled);
#endif
        }

        protected virtual void SetAnalyticsCollectionEnabledChange(bool enabled) {
            O7Log.VerboseT(Tag, "SetAnalyticsCollectionEnabledChange({0})", enabled);
            cachedIsAnalyticsEnabled = enabled;
        }

        public virtual void _SetAnalyticsCollectionEnabledChange(string enabled) {
            SetAnalyticsCollectionEnabledChange(enabled == "true");
        }
    }
}
