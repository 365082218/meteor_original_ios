//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Bee7 {

    /// <summary>
    /// Bee7 Advertiser plugin.
    /// </summary>
    public class AdvertiserPlugin : MonoBehaviour {

        private const string Tag = "AdvertiserPlugin";

#if UNITY_EDITOR || NATIVE_SIM
        private const string PredefinedReward = "{\"sizeVirtualCurrencyIconUrls\":{\"small\":\"http://cdn.outfit7.com/bee7/bee7mytalkingtom/vc56.png\",\"large\":\"http://cdn.outfit7.com/bee7/bee7mytalkingtom/vc112.png\"},\"l10nShortNames\":{\"de\":\"Demo Publisher DE\",\"zh\":\"Demo Publisher ZH\",\"it\":\"Demo Publisher IT\",\"tr\":\"Demo Publisher TR\",\"ko\":\"Demo Publisher KO\",\"pt\":\"Demo Publisher PT\",\"fr\":\"Demo Publisher FR\",\"en\":\"Demo Publisher\",\"ar\":\"Demo Publisher AR\",\"ru\":\"Demo Publisher RU\",\"es\":\"Demo Publisher ES\",\"ja\":\"Demo Publisher JA\"},\"virtualCurrencyAmount\":10,\"l10nNames\":{\"de\":\"Bee7 Demo Publisher DE\",\"zh\":\"Bee7 Demo Publisher ZH\",\"it\":\"Bee7 Demo Publisher IT\",\"tr\":\"Bee7 Demo Publisher TR\",\"ko\":\"Bee7 Demo Publisher KO\",\"pt\":\"Bee7 Demo Publisher PT\",\"fr\":\"Bee7 Demo Publisher FR\",\"en\":\"Bee7 Demo Publisher\",\"ar\":\"Bee7 Demo Publisher AR\",\"ru\":\"Bee7 Demo Publisher RU\",\"es\":\"Bee7 Demo Publisher ES\",\"ja\":\"Bee7 Demo Publisher JA\"},\"appId\":\"com.bee7.demo.publisher\",\"virtualCurrencyName\":\"GC\",\"l10nDescriptions\":{\"de\":\"description DE\",\"zh\":\"description ZH\",\"it\":\"description IT\",\"tr\":\"description TR\",\"ko\":\"description KO\",\"pt\":\"description PT\",\"fr\":\"description FR\",\"en\":\"description\",\"ar\":\"description AR\",\"ru\":\"description RU\",\"es\":\"description ES\",\"ja\":\"description JA\"},\"description\":\"This is Bee7 Publisher demo application\",\"name\":\"Bee7 Publisher demo\",\"thousandthUsdAmount\":10,\"l10nVirtualCurrencyNames\":{\"de\":\"virtual currency DE\",\"zh\":\"virtual currency ZH\",\"it\":\"virtual currency IT\",\"tr\":\"virtual currency TR\",\"ko\":\"virtual currency KO\",\"pt\":\"virtual currency PT\",\"fr\":\"virtual currency FR\",\"en\":\"virtual currency\",\"ar\":\"virtual currency AR\",\"ru\":\"virtual currency RU\",\"es\":\"virtual currency ES\",\"ja\":\"virtual currency JA\"},\"sizeIconUrls\":{\"small\":\"http://cdn.outfit7.com/button/misc/btn-boom-brw-120.png\",\"large\":\"http://cdn.outfit7.com/button/misc/btn-boom-brw-240.png\"},\"shortName\":\"Publisher demo\"}";
#endif

#if !(UNITY_EDITOR || NATIVE_SIM) && UNITY_ANDROID
        private const string AndroidGetter = "getBee7Advertiser";
#endif

        public Advertiser Advertiser { get; set; }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserStart();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserStartOrResumeRewardedSession();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserPauseRewardedSession();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserEndRewardedSession(int points);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserClearReward();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserStartGettingAccumulatedReward();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserStartGettingVirtualReward();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7AdvertiserClaimReward();
#endif

        public void StartAdvertiser() {
            O7Log.DebugT(Tag, "StartAdvertiser");

#if UNITY_EDITOR || NATIVE_SIM
            _OnEnableChange(PredefinedReward);
#elif UNITY_IPHONE
            _Bee7AdvertiserStart();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "start");
#elif UNITY_WP8

#endif
        }

        public void StartOrResumeRewardedSession() {
            O7Log.DebugT(Tag, "StartOrResumeRewardedSession");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _Bee7AdvertiserStartOrResumeRewardedSession();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "startOrResumeRewardedSession");
#elif UNITY_WP8

#endif
        }

        public void PauseRewardedSession() {
            O7Log.DebugT(Tag, "PauseRewardedSession");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _Bee7AdvertiserPauseRewardedSession();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "pauseRewardedSession");
#elif UNITY_WP8

#endif
        }

        public void EndRewardedSession(int points) {
            O7Log.DebugT(Tag, "EndRewardedSession({0})", points);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _Bee7AdvertiserEndRewardedSession(points);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "endRewardedSession", points);
#elif UNITY_WP8

#endif
        }

        public void ClearReward() {
            O7Log.DebugT(Tag, "ClearReward");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _Bee7AdvertiserClearReward();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "clearReward");
#elif UNITY_WP8

#endif
        }

        public void StartGettingAccumulatedReward() {
            O7Log.DebugT(Tag, "StartGettingAccumulatedReward");

#if UNITY_EDITOR || NATIVE_SIM
            _OnAccumulatedRewardGet(PredefinedReward);
#elif UNITY_IPHONE
            _Bee7AdvertiserStartGettingAccumulatedReward();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "startGettingAccumulatedReward");
#elif UNITY_WP8

#endif
        }

        public void _OnAccumulatedRewardGet(string reward) {
            O7Log.DebugT(Tag, "_OnAccumulatedRewardGet({0})", reward);
            Reward r = string.IsNullOrEmpty(reward) ? null : new Reward(JSON.Parse(reward));
            Advertiser.OnAccumulatedRewardGet(r);
        }

        public void StartGettingVirtualReward(int points) {
            O7Log.DebugT(Tag, "StartGettingVirtualReward({0})", points);

#if UNITY_EDITOR || NATIVE_SIM
            _OnVirtualRewardGet(PredefinedReward);
#elif UNITY_IPHONE
            _Bee7AdvertiserStartGettingVirtualReward();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "startGettingVirtualReward");
#elif UNITY_WP8

#endif
        }

        public void _OnVirtualRewardGet(string reward) {
            O7Log.DebugT(Tag, "_OnVirtualRewardGet({0})", reward);
            Reward r = string.IsNullOrEmpty(reward) ? null : new Reward(JSON.Parse(reward));
            Advertiser.OnVirtualRewardGet(r);
        }

        public void ClaimReward() {
            O7Log.DebugT(Tag, "ClaimReward");

#if UNITY_EDITOR || NATIVE_SIM
            Application.OpenURL("http://bee7.com");
#elif UNITY_IPHONE
            _Bee7AdvertiserClaimReward();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "claimReward");
#elif UNITY_WP8

#endif
        }

        public void _OnEnableChange(string virtualReward) {
            O7Log.DebugT(Tag, "_OnEnableChange({0})", virtualReward);
            Reward r = string.IsNullOrEmpty(virtualReward) ? null : new Reward(JSON.Parse(virtualReward));
            Advertiser.OnEnableChange(r);
        }
    }
}
