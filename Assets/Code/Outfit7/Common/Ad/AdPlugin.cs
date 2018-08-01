//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Ad {

    /// <summary>
    /// Ad plugin.
    /// </summary>
    public class AdPlugin : MonoBehaviour {

        private const string Tag = "AdPlugin";
#if UNITY_ANDROID
        private const string ActivityGetter = "getAdManager";
#elif UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        private Outfit7.Threading.Executor MainExecutor;
#endif

        public AdManager AdManager { get; set; }

        public InterstitialAdManager InterstitialAdManager { get; set; }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetAdsEnabled(bool enable);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetBannerAdVisible(bool visible);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _StartShowingInterstitialAd();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetReadyToShowFloaterAd(bool ready);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _OpenO7InterstitialAd(string channelId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _PrepareO7InterstitialAd(string url, string channelId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _PrepareInterstitialAd();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetAdTrackingDisabled(bool disabled);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _LastO7InterstitialReadyAppId(string channelId);
#endif

#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
            O7.Plugins.Wp8.UnityCommon.AdNativeProvider.OnBannerAdHeight += __OnSetBannerAdHeight;
            O7.Plugins.Wp8.UnityCommon.AdNativeProvider.OnOfflineBannerAdPressed += __OnOfflineBannerAdPress;
            O7.Plugins.Wp8.UnityCommon.AdNativeProvider.OnInterstitialAdReady += __OnSetInterstitialAdReady;
        }

        private void __OnSetBannerAdHeight(int height) {
            // NOT on main thread!
            MainExecutor.Post(delegate{
                SetBannerAdHeight(height);
            });
        }

        private void __OnOfflineBannerAdPress(string adId) {
            // NOT on main thread!
            MainExecutor.Post(delegate{
                _OnOfflineBannerAdPress(adId);
            });
        }

        private void __OnSetInterstitialAdReady(bool ready) {
            // NOT on main thread!
            MainExecutor.Post(delegate{
                SetInterstitialAdReady(ready);
            });
        }
#endif
        public void _ShowBanner(bool show)
        {

#if UNITY_EDITOR

#elif UNITY_ANDROID
#if !STRIP_LOGS
            UnityEngine.Debug.Log(string.Format("_ShowBanner:{0}", show));
#endif
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("ShowBanner", show);
            //Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "ShowBanner", show);
#endif
        }

        public void SetAdsEnabled(bool enable) {
            O7Log.VerboseT(Tag, "SetAdsEnabled({0})", enable);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetAdsEnabled(enable);
#elif UNITY_ANDROID
#if !STRIP_LOGS
            UnityEngine.Debug.Log(string.Format("setAdsEnabled:{0}", enable));
#endif
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "setAdsEnabled", enable);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.AdNativeProvider.SetAdsEnabled(enable);
#endif
        }

        public void SetBannerAdVisible(bool visible) {
            O7Log.VerboseT(Tag, "SetBannerAdVisible({0})", visible);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetBannerAdVisible(visible);
#elif UNITY_ANDROID
#if !STRIP_LOGS
            UnityEngine.Debug.Log(string.Format("setBannerAdVisible {0}", visible));
#endif
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "setBannerAdVisible", visible);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.AdNativeProvider.SetBannerAdVisible(visible);
#endif
        }

        public void SetBannerAdHeight(int height) {
            O7Log.VerboseT(Tag, "SetBannerAdHeight({0})", height);
#if !STRIP_LOGS
            UnityEngine.Debug.Log(string.Format("SetBannerAdHeight {0} ", height));
#endif
            AdManager.BannerHeightInPx = height;
        }

        public void _SetBannerAdHeight(string height) {
            SetBannerAdHeight(int.Parse(height));
        }

        public void _OnOfflineBannerAdPress(string adId) {
            O7Log.VerboseT(Tag, "_OnOfflineBannerAdPress({0})", adId);
            AdManager.OnOfflineBannerAdPress(adId);
        }

        public void SetInterstitialAdReady(bool ready) {
            O7Log.VerboseT(Tag, "SetInterstitialAdReady({0})", ready);

            InterstitialAdManager.InterstitialReady = ready;
        }

        public void _SetInterstitialAdReady(string ready) {
#if !STRIP_LOGS
            UnityEngine.Debug.LogError(string.Format("插屏广告=> android层返回到unity层，当前插屏广告状态:{0}", ready));
#endif
            SetInterstitialAdReady(ready == "true");
        }

        public void PrepareInterstitialAd() {
            O7Log.VerboseT(Tag, "PrepareInterstitialAd");

#if UNITY_EDITOR || NATIVE_SIM
            SetInterstitialAdReady(true);
#elif UNITY_IPHONE
            _PrepareInterstitialAd();
#elif UNITY_ANDROID
#if !STRIP_LOGS
            UnityEngine.Debug.Log("Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, fetchInterstitial)");
#endif
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "fetchInterstitial");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.AdNativeProvider.StartLoadingInterstitialAd();
#endif
        }

        public void StartShowingInterstitialAd() {
            O7Log.VerboseT(Tag, "StartShowingInterstitialAd()");
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _StartShowingInterstitialAd();
#elif UNITY_ANDROID
#if !STRIP_LOGS
            UnityEngine.Debug.Log("Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, startShowingInterstitialAd)");
#endif
            
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "startShowingInterstitialAd");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.AdNativeProvider.StartShowingInterstitialAd();
#endif
        }

        public void SetReadyToShowFloaterAd(bool ready) {
            O7Log.VerboseT(Tag, "SetReadyToShowFloaterAd({0})", ready);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetReadyToShowFloaterAd(ready);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "setReadyToShowFloaterAd", ready);
#elif UNITY_WP8

#endif
        }

        public void _SetPostitialAdReady(string ready) {
            O7Log.VerboseT(Tag, "_SetPostitialAdReady({0})", ready);
            InterstitialAdManager.PostitialReady = (ready == "true");
        }

        public bool QuitWithPostitial() {
            bool shouldQuitWithPostitial = false;

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("onUnityClosed");
            shouldQuitWithPostitial = Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>(ActivityGetter, "quitWithPostitial");
#elif UNITY_WP8

#endif

            O7Log.VerboseT(Tag, "QuitWithPostitial(): {0}", shouldQuitWithPostitial);
            return shouldQuitWithPostitial;
        }

        // TODO deprecated... remove from native and unity
        public void _AdsDebugMode(string debugMode) {
            O7Log.VerboseT(Tag, "_AdsDebugMode({0})", debugMode);
            AdManager.IsDebugMode = (debugMode == "true");
        }

        public void _IgnoreAdsTimeouts(string ignoreAdsTimeouts) {
            O7Log.VerboseT(Tag, "_IgnoreAdsTimeouts({0})", ignoreAdsTimeouts);
            InterstitialAdManager.IgnoreAdsTimeouts = (ignoreAdsTimeouts == "true");
        }

#region o7interstitial

        public void PrepareO7InterstitialAd(string url, string channelId) {
            O7Log.VerboseT(Tag, "PrepareO7InterstitialAd()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _PrepareO7InterstitialAd(url, channelId);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "PrepareO7InterstitialAd",url,channelId);
#elif UNITY_WP8

#endif
        }

        public void OpenO7InterstitialAd(string channelId) {
            O7Log.VerboseT(Tag, "OpenO7InterstitialAd({0})", channelId);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _OpenO7InterstitialAd(channelId);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>(ActivityGetter, "OpenO7InterstitialAd",channelId);
#elif UNITY_WP8

#endif
        }

        public void _SetO7InterstitialAdReady(string json) {
            O7Log.VerboseT(Tag, "SetO7InterstitialAdReady({0})", json);
            InterstitialAdManager.SetO7InterstitialAdReady(json);
        }

        public string LastO7InterstitialReadyAppId(string channelId) {
            string lastO7InterstitialAppId;

#if UNITY_EDITOR || NATIVE_SIM
            lastO7InterstitialAppId = "com.outfit7.mytalkingtom";
#elif UNITY_IPHONE
            lastO7InterstitialAppId = _LastO7InterstitialReadyAppId(channelId);
#elif UNITY_ANDROID
            lastO7InterstitialAppId = Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<string>(ActivityGetter, "LastO7InterstitialReadyAppId", channelId);
#elif UNITY_WP8
            lastO7InterstitialAppId = null;
#endif

            O7Log.VerboseT(Tag, "LastO7InterstitialReadyAppId({0}): {1}", channelId, lastO7InterstitialAppId);
            return lastO7InterstitialAppId;
        }

#endregion

        public void SetAdInterestBasedTrackingDisabled(bool disabled) {
            O7Log.VerboseT(Tag, "SetAdInterestBasedTrackingDisabled({0})", disabled);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetAdTrackingDisabled(disabled);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "setAdTrackingDisabled", disabled);
#elif UNITY_WP8

#endif
        }

        public string GetAvailableAdProviders(int adType) {
            string adProvidersJson = null;

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE

#elif UNITY_ANDROID
            adProvidersJson = Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<string>(ActivityGetter, "getAdProviders", (int) adType);
#elif UNITY_WP8

#endif

            O7Log.VerboseT(Tag, "GetAvailableAdProviders: adType = {0}, adProvidersJson = {1}", adType, adProvidersJson);
            return adProvidersJson;
        }

        public void SetAdProvider(int adType, string provider) {
            O7Log.VerboseT(Tag, "SetAdProvider: adType = {0}, provider = {1}", adType, provider);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(ActivityGetter, "setAdProvider", (int) adType, provider);
#elif UNITY_WP8

#endif
        }

        public void _AdShown(string json) {
            O7Log.VerboseT(Tag, "_AdShown: json = {0}", json);
#if !STRIP_LOGS

#endif
            AdManager.AdShown(json);
        }

    }
}
