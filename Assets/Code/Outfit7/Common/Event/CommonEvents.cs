//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

namespace Outfit7.Event {

    /// <summary>
    /// Common events.
    /// </summary>
    public static class CommonEvents {
        public const int APP_RESUME = -1;
        public const int APP_PAUSE = -2;
        public const int NEW_SESSION = -3;
        public const int APP_ACTIVATION = -10;
        public const int NATIVE_DIALOG_CANCEL = -11;
        public const int FRESH_GRID_DOWNLOAD = -20;
        public const int GRID_IAPS_CHANGE = -21;
        public const int STORE_IAPS_CHANGE = -22;
        public const int NEWS_READY = -23;
        public const int NEWS_PENDING = -24;
        public const int MANUAL_NEWS_BUTTON_READY = -25;
        public const int GAME_CENTER_SIGN_IN = -60;
        public const int REFRESH_STICKERS_ANIMATE = -61;
        public const int EMAIL_COMPLETION = -100;
        public const int EXTERNAL_OFFER_AVAIL_CHANGE = -101;
        public const int PUSH_START = -110;
        public const int PUSH_RECEIVE = -111;
        public const int APP_SHORTCUT = -120;
        public const int PUSH_REGISTRATION = -152;
        public const int VIDEO_CLIP_AVAILABILITY_CHANGE = -160;
        //        public static int ADDONS_ADD = -300;
        //        public static int ADDONS_REMOVE = -301;
        public const int ADDONS_CHANGE = -302;
        public const int CURRENCY_BALANCE_CHANGE = -350;
        //public const int APP_MUTE = -403;
        public const int BEE7_PUBLISHER_APP_OFFERS_UPDATE = -404;
        public const int BEE7_ADVERTISER_ENABLED = -406;
        public const int SOUND_MUSIC_MUTE = -407;
        public const int SOUND_SFX_MUTE = -408;
        public const int SOCIAL_LOGIN_CHANGE = -500;
        public const int SOCIAL_FRIENDS_UPDATE = -501;
        public const int SOCIAL_USERS_RELOADED = -502;
        public const int SOCIAL_SUBSCRIBE = -503;
        //public const int SOCIAL_NETWORK_SELECTED = -504;
        public const int SOCIAL_LOGIN_START = -505;
        public const int MenuDialogOpen = 9416;
        public const int MenuDialogClose = 9418;
        public const int DisableUICamera = 5916;
        public const int EnableUICamera = 5918;
        public const int ShowBanner = 6916;
        public const int HideBanner = 6918;
        public const int ShowFashionRedPoint = 6919;
        public const int CloseFashionRedPoint = 6920;
    }
}
