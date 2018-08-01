//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Json;
using Outfit7.Util;
using SimpleJSON;
using UnityEngine;
using Outfit7.Devel.O7Debug;

namespace Outfit7.Ad {

    /// <summary>
    /// Ad manager paid user provider.
    /// </summary>
    public interface AdManagerPaidUserProvider {

        bool IsPaidUser { get; }
    }

    /// <summary>
    /// Ad manager.
    /// </summary>
    public class AdManager {

        /*
         * These enum values should be in sync with the native ones!
         */
        // DON'T REMOVE/CHANGE VALUES FROM HERE, ONLY ADD NEW ONES FOR NEW AD TYPES
        public enum AdType {
            Banner = 0,
            Interstitial = 1,
            Video = 2,
        }

        protected const string Tag = "AdManager";

        private const string AdInterestBasedTrackingDisabledKey = "AdManager.AdInterestBasedTrackingDisabledKey";

        public event Action AdHeightChanged;
        public event Action<AdType, string> OnAdShown;
        public event Action OnAdHide;

        // Default in native must also be false
        protected bool adsEnabled;
        // Default in native must also be false
        protected bool bannerEnabled;
        protected int bannerHeightInPx;
        protected float bannerHeight;
        protected bool readyToShowFloaterAd;
        protected bool adInterestBasedTrackingDisabled;
        protected bool DebugMode;

        public virtual AdPlugin AdPlugin { get; set; }

        public virtual AdManagerPaidUserProvider AdManagerPaidUserProvider { get; set; }

        public virtual bool AdInterestBasedTrackingDisabled {
            get {
                return adInterestBasedTrackingDisabled;
            }
            set {
                AdPlugin.SetAdInterestBasedTrackingDisabled(value);
                if (value == adInterestBasedTrackingDisabled) return;
                adInterestBasedTrackingDisabled = value;
                UserPrefs.SetBool(AdInterestBasedTrackingDisabledKey, adInterestBasedTrackingDisabled);
                UserPrefs.SaveDelayed();
            }
        }

        public virtual bool IsDebugMode {
            set {
                if (BuildConfig.IsProdOrDevel) {
                    DebugMode = value;
                }
            }
            get {
                return DebugMode;
            }
        }

        public virtual bool AdsEnabled {
            get {
                return adsEnabled;
            }
            set {
#if UNITY_BEMOBI
                AdPlugin.SetAdsEnabled(false);
                return;
#else
                if (adsEnabled == value) return;
//                if (value && AdManagerPaidUserProvider.IsPaidUser) {
//                    //TODO Mihecp fix on native side. Waiting to separate video clips & offers from ads.
//#if UNITY_ANDROID
//                    AdPlugin.SetAdsEnabled(false);
//#endif
//                    return;
//                }

                adsEnabled = value;
                if (!adsEnabled) {
                    BannerEnabled = false;
                }
                AdPlugin.SetAdsEnabled(adsEnabled);
#endif
            }
        }

        public virtual void _ShowBanner(bool show)
        {
            AdPlugin._ShowBanner(show);
        }

        public virtual bool BannerEnabled {
            get {
                return bannerEnabled;
            }
            set {
                if (bannerEnabled == value) return;
                if (value && !AdsEnabled) return;
#if !STRIP_LOGS
                UnityEngine.Debug.Log(string.Format("AdPlugin.SetBannerAdVisible {0}", value));
#endif
                bannerEnabled = value;
                AdPlugin.SetBannerAdVisible(value);
                if (!bannerEnabled && OnAdHide != null)
                    OnAdHide();
                TriggerAdHeightChanged();
            }
        }

        public virtual int BannerHeightInPx {
            get {
                if (!BannerEnabled) return 0;
                return bannerHeightInPx;
            }
            internal set {
                if (value == bannerHeightInPx) return;
                bannerHeightInPx = value;
                BannerHeight = (1f / Screen.height) * value;
                TriggerAdHeightChanged();
            }
        }

        public virtual float BannerHeight {
            get {
                if (!BannerEnabled) return 0;
                return bannerHeight;
            }
            protected set {
                bannerHeight = value;
            }
        }

        public virtual bool ReadyToShowFloaterAd {
            get {
                return readyToShowFloaterAd;
            }
            internal set {
                if (readyToShowFloaterAd == value) return;
                if (value && !AdsEnabled) return;
                readyToShowFloaterAd = value;
                AdPlugin.SetReadyToShowFloaterAd(value);
            }
        }

        public virtual void Init() {
            AdsEnabled = true;
#if !STRIP_LOGS
            UnityEngine.Debug.Log("AdsEnabled = true In AdManager.Init()");
#endif
            AdInterestBasedTrackingDisabled = UserPrefs.GetBool(AdInterestBasedTrackingDisabledKey, false);
        }

        public virtual void OnOfflineBannerAdPress(string adId) {
        }

        public virtual float BannerHeightInPxNormalized(float containerHeight) {
            return BannerHeightInPx * (containerHeight / Screen.height);
        }

        protected virtual void TriggerAdHeightChanged() {
            if (AdHeightChanged != null) {
                AdHeightChanged();
            }
        }

        public List<string> GetAvailableAdProviders(AdType adType) {
            string json = AdPlugin.GetAvailableAdProviders((int) adType);
            if (StringUtils.IsNullOrEmpty(json)) {
                O7Log.WarnT(Tag, "No ad providers for selected ad type: {0}", adType);
                return null;
            }

            JSONNode j = JSONNode.Parse(json);
            return SimpleJsonUtils.CreateList(j["adProviders"]);
        }

        public void SetAdProvider(AdType adType, string provider) {
            AdPlugin.SetAdProvider((int) adType, provider);
        }

        public void AdShown(string json) {
            JSONNode j = JSONNode.Parse(json);
            // These keys should be in sync with the native ones!
            AdType adType = (AdType) j["adType"].AsInt;
            string provider = j["provider"].Value;

            O7Log.VerboseT(Tag, "AdShown: adType = {0}, provider = {1}", adType, provider);
            if (OnAdShown != null) {
                OnAdShown(adType, provider);
            }
        }
    }
}
