//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

namespace Outfit7.Analytics.Tracking {

    /// <summary>
    /// Event tracking event parameters.
    /// </summary>
    public static class CommonTrackingEventParams {

        public static class GroupId {
            public const string Session = "session";
            public const string UserInfo = "user-info";
            public const string AppOpen = "app-open";
            public const string Currency = "gc";
            public const string UserState = "user-state";
            public const string Purchase = "purchase";
            public const string SpecialOffer = "special-offer";
            public const string Tampering = "tampering";
            public const string MiniGames = "minigames";
            public const string Social = "social";
            public const string VideoRecording = "video-share";
            public const string VideoGallery = "video-gallery";
            public const string AppFeatures = "app-features";
            public const string PromoAd = "dream-ad";
            public const string VideoReward = "video-reward";
            public const string Bee7Reward = "bee7-reward";
            public const string ExternalReporting = "external-reporting";
            public const string Ads = "ads";
            public const string GameFeatures = "game-features";
            public const string Settings = "settings";
        }

        public static class EventId {
            public const string NewSession = "new-session";
            public const string FirstRun = "first-run";
            public const string CumulativeCounts = "cumulative-counts";
            public const string Open = "open";
            public const string Restore = "restore";
            public const string Iap = "iap";
            public const string AddOnPurchaseAtFullPrice = "addon-p";
            public const string AddOnRewarded = "addon-r";
            public const string AddOnPushNotification = "addon-pn";
            public const string AddOnPurchaseAtDiscountedPrice = "addon-d";
            public const string AddOnSellAtFullPrice = "addon-rf";
            public const string AddOnSellAtRecycledPrice = "addon-r";
            public const string AddOnLockAtFullPrice = "addon-lf";
            public const string AddOnLockAtRecycledPrice = "addon-l";
            public const string AddOnEarlyUnlockAtFullPrice = "addon-eu";
            public const string AddOnEarlyUnlockAtDiscountedPrice = "addon-eud";
            public const string Adjust = "adjust";
            public const string MiniGame = "minigame";
            public const string MiniGameContinue = "minigame-continue";
            public const string VideoClip = "clips";
            public const string SignIn = "sign-in";
            public const string OfferInternal = "offers-int";
            public const string OfferExternal = "offers-ext";
            public const string OfferCrossPromo = "offers-cp";
            public const string DailyReward = "daily-reward";
            public const string Receipt = "receipt";
            public const string Account = "account";
            public const string AddOns = "addons";
            public const string LogIn = "log-in";
            public const string LogOut = "log-out";
            public const string InviteFriend = "invite-friend";
            public const string FriendList = "friend-list";
            public const string Like = "like";
            public const string MiniGameStart = "game-start";
            public const string MiniGameForceCloseEnter = "force-close-enter";
            public const string MiniGameForceCloseExit = "force-close-exit";
            public const string MiniGameOpenSinglePlayer = "open-singleplayer";
            public const string MiniGameOpenMultiPlayer = "open-multiplayer";
            public const string MiniGameInstall = "install";
            public const string MiniGameClickLocked = "locked-click";
            public const string SpecialOfferStart = "offer-start";
            public const string SpecialOfferShow = "offer-show";
            public const string SpecialOfferClick = "offer-click";
            public const string SpecialOfferComplete = "offer-complete";
            public const string RecordingStart = "recording-start";
            public const string RecordingStop = "recording-stop";
            public const string Share = "share";
            public const string VideoGalleryEnter = "gallery-enter";
            public const string VideoGalleryExit = "gallery-exit";
            public const string AgeGateShown = "sharing-question";
            public const string AgeGatePassed = "sharing-enabled";
            public const string AgeGatePrivacyPolicyClick = "privacy-policy-click";
            public const string AdShow = "dream-show";
            public const string AdClick = "dream-click";
            public const string Reward = "reward";
            public const string Bee7PublisherReporting = "bee7-publisher-rpi";
            public const string InterstitialShowOpportunityFail = "interstitial-opportunity-fail";
            public const string VideoShowOpportunityFail = "video-opportunity-fail";
            public const string AchievementsWon = "achievements-won";
            public const string AchievementsView = "achievements-view";
            public const string Change = "change";
            public const string GenderQuestion = "gender-question";
            public const string GenderAnswer = "gender-answer";
        }

        public static class UserStateRestoreType {
            public const string New = "new";
            public const string NewSkippingRemoteDevice = "new-remote";
            public const string Restored = "restored";
            public const string RestoredFromRemoteDevice = "restored-remote";
        }

        public static class InternalOfferItemId {
            public const string Debug = "debug";
            public const string PushRegistration = "push-r";
            public const string YouTubeRegistration = "yt-s";
            public const string FacebookLogin = "fb-login";
            public const string VKontakteSubscribe = "vk-subscribe";
            public const string VKontakteLogin = "vk-login";
            public const string TwitterFollow = "tw-follow";
        }
    }
}
