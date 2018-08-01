//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Util;
using UnityEngine;
using System;

namespace Outfit7.Bee7 {
    public class PublisherPlugin : MonoBehaviour {

        public PublisherHelper PublisherHelper { get; set; }

        private const string Tag = "PublisherPlugin";

        public Action OnHide;

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7PublisherStart(string jsonString);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7PublisherGetAllAppOffers();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7PublisherStartOffer(string offerId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7PublisherGetPendingRewards();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _OnGameWallButtonImpression();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _OnGameWallImpression();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _OnGameWallCloseImpression();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _OnAppOffersImpression(string offers);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetTestVariant(string variant);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7ExternalMiniGameWillOpen();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7UpdateMinigames(string jsonString);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7HideGameWall();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7ShowGameWall();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7ShowReward(string data);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ShowBee7Settings();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ForceGameWallFallback(bool force);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetVirtualCurrencyState(bool lowCurrency);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ToggleNotificationShowing(bool show);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ShowBannerNotification();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetOfferTypes(int offerType);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7SetRewardFactor(float factor);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _CloseBee7Notification();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7ScheduleReengagementNotification();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _Bee7ShowGameWallWithNotifications(string notificationsJson);

#endif

#if UNITY_EDITOR || NATIVE_SIM
        private const string TestReward = "{\"rewards\":[{\"thousandthUsdAmount\":4,\"sizeIconUrls\":{\"large\":\"http:\\/\\/cdn.outfit7.com\\/bee7\\/bee7tom2\\/icon240.png\",\"small\":\"http:\\/\\/cdn.outfit7.com\\/bee7\\/bee7tom2\\/icon120.png\"},\"virtualCurrencyAmount\":4,\"name\":\"Tom2\",\"appId\":\"bee7tom2\",\"l10nNames\":{\"en\":\"Tom 2\",\"it\":\"Tom 2\"}}]}";
        private Outfit7.Threading.Executor MainExecutor;

        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
        }
#endif

#if !(UNITY_EDITOR || NATIVE_SIM) && UNITY_ANDROID
        private readonly string AndroidGetter = "getBee7Publisher";
#endif

        public void StartPublisher(string miniGamesJson) {

            O7Log.DebugT(Tag, "Start {0}", miniGamesJson);

            #if UNITY_EDITOR || NATIVE_SIM
            OnEnableChange("true");
            #elif UNITY_IPHONE
            _Bee7PublisherStart(miniGamesJson);
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "StartPublisher",miniGamesJson);
            #elif UNITY_WP8

            #endif
        }

        public void StartOffer(string offer) {

            O7Log.DebugT(Tag, "StartOffer {0}", offer);

            #if UNITY_EDITOR || NATIVE_SIM
            MainExecutor.Post(delegate {
                OnRewardClaim(TestReward);
            });
            #elif UNITY_IPHONE
            _Bee7PublisherStartOffer(offer);
            #elif UNITY_ANDROID
//            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "StartOffer", offer);
            #elif UNITY_WP8

            #endif
        }

        public void GetPendingRewards() {

            O7Log.DebugT(Tag, "GetPendingRewards");

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _Bee7PublisherGetPendingRewards();
            #elif UNITY_ANDROID
//            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "GetPendingRewards");
            #elif UNITY_WP8

            #endif
        }

        public void GetAllAppOffers() {

            O7Log.DebugT(Tag, "GetAllAppOffers");

            #if UNITY_EDITOR || NATIVE_SIM
            //TODO matej

            WWW www = new WWW("file:///" + Application.dataPath + "/EditorTestFiles/appOffers.json.txt");
            while (!www.isDone) {
            }
            AllAppsOffersCompleted(www.text);

            #elif UNITY_IPHONE
            _Bee7PublisherGetAllAppOffers();
            #elif UNITY_ANDROID
//            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "GetAllAppOffers");
            #elif UNITY_WP8
            return;
            #endif
        }

        public void AllAppsOffersCompleted(string data) {
            PublisherHelper.GetAppListCompleted(data);
        }

        public void OnInstantRewardClaim(string data) {
            O7Log.DebugT(Tag, "OnInstantRewardClaim {0}", data);
            PublisherHelper.OnRewardClaim(data, true);
        }

        public void OnRewardClaim(string data) {
            O7Log.DebugT(Tag, "OnRewardClaim {0}", data);
            PublisherHelper.OnRewardClaim(data, false);
        }

        public void ShowReward(string data) {
            O7Log.DebugT(Tag, "ShowReward {0}", data);

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _Bee7ShowReward(data);
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "ShowReward", data);
            #elif UNITY_WP8

            #endif
        }

        public void OnEnableChange(string data) {
            O7Log.DebugT(Tag, "OnEnableChange {0}", data);
            PublisherHelper.OnEnabledChanged("true".Equals(data));
        }

        public void AppListChanged(string data) {
            O7Log.DebugT(Tag, "AppListChanged {0}", data);
            PublisherHelper.OnAppListChanged();
        }

        public void OnGameWallButtonImpression() {
            O7Log.DebugT(Tag, "OnGameWallButtonImpression");

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _OnGameWallButtonImpression();
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "OnGameWallButtonImpression");
            #elif UNITY_WP8

            #endif
        }

        public void OnGameWallImpression() {
            O7Log.DebugT(Tag, "OnGameWallImpression");

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _OnGameWallImpression();
            #elif UNITY_ANDROID
//            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "OnGameWallImpression");
            #elif UNITY_WP8

            #endif
        }

        public void SetTestVariant(string variant) {
            O7Log.DebugT(Tag, "SetTestVariant {0}", variant);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetTestVariant(variant);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "SetTestVariant", variant);
#elif UNITY_WP8

#endif
        }

        public void OnGameWallCloseImpression() {
            O7Log.DebugT(Tag, "OnGameWallCloseImpression");

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _OnGameWallCloseImpression();
            #elif UNITY_ANDROID
//            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "OnGameWallCloseImpression");
            #elif UNITY_WP8

            #endif
        }

        public void OnAppOffersImpression(string offers) {
            O7Log.DebugT(Tag, "OnAppOffersImpression {0}", offers);

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _OnAppOffersImpression(offers);
            #elif UNITY_ANDROID
//            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "OnAppOffersImpression", offers);
            #elif UNITY_WP8

            #endif
        }

        public void OnTrackingInfoReceived(string json) {
            O7Log.DebugT(Tag, "OnTrackingInfoReceived {0}", json);
            PublisherHelper.OnTrackingInfoReceived(json);
        }

        public void ShowGameWall() {
            O7Log.DebugT(Tag, "ShowGameWall");
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _Bee7ShowGameWall();
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "ShowGameWall");
            #elif UNITY_WP8

            #endif
        }

        public void OnGameWallClose() { // iOS will notify when gamewall is really hidden
            O7Log.DebugT(Tag, "OnGameWallClose");
            if (OnHide != null) {
                OnHide();
            }
        }

        public void HideGameWall(Action onHide) {
            O7Log.DebugT(Tag, "HideGameWall");
            OnHide = onHide;
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _Bee7HideGameWall();
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "HideGameWall");
            OnGameWallClose();
            #elif UNITY_WP8

            #endif
        }

        public void OpenMinigame(string gameId) {
            O7Log.DebugT(Tag, "OpenMinigame {0}", gameId);
            PublisherHelper.OpenMinigame(gameId);
        }

        public void CloseGameWall() {
            O7Log.DebugT(Tag, "CloseGameWall");
            PublisherHelper.CloseGameWall();
        }

        public void UpdateMinigames(string jsonString) {
            O7Log.DebugT(Tag, "UpdateMinigames {0}", jsonString);
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _Bee7UpdateMinigames(jsonString);
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "UpdateMinigames",jsonString);
            #elif UNITY_WP8

            #endif
        }

        public void ExternalMiniGameWillOpen() {
            O7Log.DebugT(Tag, "ExternalMiniGameWillOpen");
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _Bee7ExternalMiniGameWillOpen();
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "ExternalMiniGameWillOpen");
            #elif UNITY_WP8

            #endif
        }

        public void ForceGameWallFallback(bool useOldGameWall) {

            O7Log.DebugT(Tag, "ForceGameWallFallback {0}", useOldGameWall);
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _ForceGameWallFallback(useOldGameWall);
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        public void ShowBee7Settings() {

            O7Log.DebugT(Tag, "ShowBee7Settings");
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _ShowBee7Settings();
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "ShowBee7Settings");
            #elif UNITY_WP8

            #endif
        }

        public void OnGameWallShowRequest() {
            O7Log.DebugT(Tag, "OnGameWallShowRequest");
            PublisherHelper.OnGameWallShowRequest();
        }

        public void OnBannerNotificationShowRequest() {
            O7Log.DebugT(Tag, "OnBannerNotificationShowRequest");
            PublisherHelper.OnBannerNotificationShowRequest();
        }

        public void OnBannerNotificationClick() {
            O7Log.DebugT(Tag, "OnBannerNotificationClick");
            PublisherHelper.OnBannerNotificationClick();
        }

        public void OnBannerNotificationVisibilityChanged(string visible) {
            O7Log.DebugT(Tag, "OnBannerNotificationVisibilityChanged {0}", visible);
            PublisherHelper.OnBannerNotificationVisibilityChanged(visible == "true");
        }

        public void GameWallAvailable(string available) {
            O7Log.DebugT(Tag, "GameWallAvailable {0}", available);
            PublisherHelper.GameWallAvailable(available == "true");
        }

        public void ToggleNotificationShowing(bool show) {
            O7Log.DebugT(Tag, "ToggleNotificationShowing {0}", show);
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _ToggleNotificationShowing(show);
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "ToggleNotificationShowing", show);
            #elif UNITY_WP8

            #endif
        }

        public void SetVirtualCurrencyState(bool lowCurrency) {
            O7Log.DebugT(Tag, "SetVirtualCurrencyState {0}", lowCurrency);
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _SetVirtualCurrencyState(lowCurrency);
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "SetVirtualCurrencyState", lowCurrency);
            #elif UNITY_WP8

            #endif
        }

        public void ShowBannerNotification() {
            O7Log.DebugT(Tag, "ShowBannerNotification");
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _ShowBannerNotification();
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "ShowBannerNotification");
            #elif UNITY_WP8

            #endif
        }

        public void CloseBannerNotification() {
            O7Log.DebugT(Tag, "CloseBannerNotification");
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _CloseBee7Notification();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "CloseBannerNotification");
#elif UNITY_WP8

#endif
        }

        public void SetOfferTypes(int offerType) {
            O7Log.DebugT(Tag, "SetOfferTypes {0}", offerType);
            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE
            _SetOfferTypes(offerType);
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "SetOfferTypes", offerType);
            #elif UNITY_WP8

            #endif
        }

        public void SetRewardFactor(float factor) {
            O7Log.DebugT(Tag, "_Bee7SetRewardFactor {0}", factor);
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _Bee7SetRewardFactor(factor);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "SetRewardFactor", factor);
#elif UNITY_WP8

#endif
        }

        public void ScheduleReengagementNotifications() {
            O7Log.DebugT(Tag, "ScheduleReengagementNotifications");
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _Bee7ScheduleReengagementNotification();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        public void ShowGameWallWithNotifications(string notificationsJson) {
            O7Log.DebugT(Tag, "ShowGameWallWithNotifications");
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _Bee7ShowGameWallWithNotifications(notificationsJson);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "Bee7ShowGameWallWithNotifications", notificationsJson);
#elif UNITY_WP8

#endif
        }
    }
}
