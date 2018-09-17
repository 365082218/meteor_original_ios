//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using UnityEngine;
using Outfit7.Event;
using Outfit7.Util;
using System;
using UnityEngine.Profiling;

namespace Outfit7.Common {

    /// <summary>
    /// Plugin for common application stuff.
    /// </summary>
    public class AppPlugin : MonoBehaviour {

        private const string Tag = "AppPlugin";

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetUid();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetPlatform();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _IsJailBroken();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetAppId();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetAppToken();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetAppVersion();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetLibraryVersion();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetPromoLibraryVersion();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetUserAgentName();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetLanguageCode();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetCountryCode();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _GetServerBaseUrl();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetDevelServerEnabled(bool enable);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _IsDevelServerEnabled();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _IsAppInstalled(string appId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _OpenApp(string appId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _AfterStartUpSceneLoad();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _AfterMainSceneLoad();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _AfterFirstRoomSceneLoad();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _DispatchException(string msg, string stackTrace);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ResetSleepIdleTimer();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetAgeGateStateWithBirthYear(bool passed, int birthYear);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetUserGender(int userGender);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ShowNativeHtml(string what);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _HideSplashScreen();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetSplashScreenProgress(float progress);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetSplashScreenProgressText(string text);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetSplashScreenProgressTextAndColor(string text, int hexColor);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _SetSplashScreenProgressTextColorAndOutlineColor(string text, int hexColor, int hexColorOutline);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern long _GetDiskFreeSpace();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern long _O7ToggleAdsDebug();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern long _StartDownloadingAssets();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern long _StartAssetExtracting();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern long _DoneAssetExtracting();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern long _AssetExtractingError(string error);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ResolveAndOpenUrl(string url);

#endif

        // UID must no be cached, because it can be changed after new GRID.
        // For example: on Android, it can be changed after the first GRID if BE finds current UID as duplicate.
        public static string Uid {
            get {
#if UNITY_EDITOR || NATIVE_SIM
                string uid = UniqueIdConverter.Udid2Uid(SystemInfo.deviceUniqueIdentifier);
#elif UNITY_IPHONE
                string uid = _GetUid();
#elif UNITY_ANDROID
                string uid = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getUid");
#elif UNITY_WP8
                string uid = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetUid();
#endif
                O7Log.VerboseT(Tag, "Uid: {0}", uid);
                return uid;
            }
        }

        private static string cachedPlatform;

        public static string Platform {
            get {
                if (!StringUtils.HasText(cachedPlatform)) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedPlatform = "Android-devel";
#elif UNITY_IPHONE
                    cachedPlatform = _GetPlatform();
#elif UNITY_ANDROID
                    cachedPlatform = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getPlatform");
#elif UNITY_WP8
                    cachedPlatform = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetPlatform();
#endif
                    O7Log.VerboseT(Tag, "Platform: {0}", cachedPlatform);
                }
                return cachedPlatform;
            }
        }

        private static bool? cachedRooted;

        public static bool Rooted {
            get {
                if (cachedRooted == null) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedRooted = false;
#elif UNITY_IPHONE
                    cachedRooted = _IsJailBroken();
#elif UNITY_ANDROID
                    cachedRooted = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("isRooted");
#elif UNITY_WP8
                    cachedRooted = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.IsRooted();
#endif
                    O7Log.VerboseT(Tag, "Rooted: {0}", cachedRooted);
                }
                return cachedRooted.Value;
            }
        }

        private static string cachedAppId;

        public static string AppId {
            get {
                if (!StringUtils.HasText(cachedAppId)) {
#if UNITY_EDITOR
                    cachedAppId = UnityEditor.PlayerSettings.bundleIdentifier;
#elif NATIVE_SIM
                    cachedAppId = "O7DummyAppId";
#elif UNITY_IPHONE
                    cachedAppId = _GetAppId();
#elif UNITY_ANDROID
                    cachedAppId = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getAppId");
#elif UNITY_WP8
                    cachedAppId = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetAppId();
#endif
                    O7Log.VerboseT(Tag, "AppId: {0}", cachedAppId);
                }
                return cachedAppId;
            }
        }

#if UNITY_EDITOR || NATIVE_SIM
        private const string AppTokenPref = "AppPlugin.AppToken";
#endif

        private static string cachedAppToken;

        public static string AppToken {
            get {
                if (!StringUtils.HasText(cachedAppToken)) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedAppToken = UserPrefs.GetString(AppTokenPref, null);
                    if (cachedAppToken == null) {
                        cachedAppToken = Guid.NewGuid().ToString();
                        UserPrefs.SetString(AppTokenPref, cachedAppToken);
                    }
#elif UNITY_IPHONE
                    cachedAppToken = _GetAppToken();
#elif UNITY_ANDROID
                    cachedAppToken = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getAppToken");
#elif UNITY_WP8
                    cachedAppToken = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetAppToken();
#endif
                    O7Log.VerboseT(Tag, "AppToken: {0}", cachedAppToken);
                }
                return cachedAppToken;
            }
        }

        private static string cachedAppVersion;

        public static string AppVersion {
            get {
                if (!StringUtils.HasText(cachedAppVersion)) {
#if UNITY_EDITOR
                    cachedAppVersion = UnityEditor.PlayerSettings.bundleVersion;
#elif NATIVE_SIM
                    cachedAppVersion = "1.0.0";
#elif UNITY_IPHONE
                    cachedAppVersion = _GetAppVersion();
#elif UNITY_ANDROID
                    cachedAppVersion = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getAppVersion");
#elif UNITY_WP8
                    cachedAppVersion = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetAppVersion();
#endif
                    O7Log.VerboseT(Tag, "AppVersion: {0}", cachedAppVersion);
                }
                return cachedAppVersion;
            }
        }

        private static string cachedLibVersion;

        public static string LibraryVersion {
            get {
                if (!StringUtils.HasText(cachedLibVersion)) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedLibVersion = "2.9";
#elif UNITY_IPHONE
                    cachedLibVersion = _GetLibraryVersion();
#elif UNITY_ANDROID
                    cachedLibVersion = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getLibraryVersion");
#elif UNITY_WP8
                    cachedLibVersion = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetLibraryVersion();
#endif
                    O7Log.VerboseT(Tag, "LibraryVersion: {0}", cachedLibVersion);
                }
                return cachedLibVersion;
            }
        }

        private static string cachedPromoLibVersion;

        public static string PromoLibraryVersion {
            get {
                if (!StringUtils.HasText(cachedPromoLibVersion)) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedPromoLibVersion = "1.2";
#elif UNITY_IPHONE
                    cachedPromoLibVersion = _GetPromoLibraryVersion();
#elif UNITY_ANDROID
                    cachedPromoLibVersion = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getPromoLibraryVersion");
#elif UNITY_WP8
                    cachedPromoLibVersion = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetPromoLibraryVersion();
#endif
                    O7Log.VerboseT(Tag, "PromoLibraryVersion: {0}", cachedPromoLibVersion);
                }
                return cachedPromoLibVersion;
            }
        }

        private static string cachedUserAgentName;

        public static string UserAgentName {
            get {
                if (!StringUtils.HasText(cachedUserAgentName)) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedUserAgentName = AppId + "/" + AppVersion;
#elif UNITY_IPHONE
                    cachedUserAgentName = _GetUserAgentName();
#elif UNITY_ANDROID
                    cachedUserAgentName = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getUserAgentName");
#elif UNITY_WP8
                    cachedUserAgentName = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetUserAgentName();
#endif
                    O7Log.VerboseT(Tag, "UserAgentName: {0}", cachedUserAgentName);
                }
                return cachedUserAgentName;
            }
        }

        private static string cachedLanguageCode;

        public static string LanguageCode {
            get {
                if (cachedLanguageCode == null) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedLanguageCode = Text.Localization.LanguageCodes.Chinese;
#elif UNITY_IPHONE
                    cachedLanguageCode = _GetLanguageCode();
#elif UNITY_ANDROID
                    cachedLanguageCode = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getLanguageCode");
#elif UNITY_WP8
                    cachedLanguageCode = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetLanguageCode();
#endif
                    O7Log.VerboseT(Tag, "LanguageCode: {0}", cachedLanguageCode);
                }
                return cachedLanguageCode;
            }
        }

        private static string cachedCountryCode;

        public static string CountryCode {
            get {
                if (cachedCountryCode == null) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedCountryCode = "SI";
#elif UNITY_IPHONE
                    cachedCountryCode = _GetCountryCode();
#elif UNITY_ANDROID
                    cachedCountryCode = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getCountryCode");
#elif UNITY_WP8
                    cachedCountryCode = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetCountryCode();
#endif
                    O7Log.VerboseT(Tag, "CountryCode: {0}", cachedCountryCode);
                }
                return cachedCountryCode;
            }
        }

        private static string cachedAppStoreId;

        public static string AppStoreId {
            get {
                if (cachedAppStoreId == null) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedAppStoreId = "Unity Local";
#elif UNITY_IPHONE
                    cachedAppStoreId = "iOS";
#elif UNITY_ANDROID
                    cachedAppStoreId = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getAppStore");
#else
                    cachedAppStoreId = "Windows Store";
#endif
                    O7Log.VerboseT(Tag, "AppStoreId: {0}", cachedAppStoreId);
                }
                return cachedAppStoreId;
            }
        }

        private static string cachedInternalStoragePath;

        public static string InternalStoragePath {
            get {
                if (!StringUtils.HasText(cachedInternalStoragePath)) {
#if UNITY_EDITOR || NATIVE_SIM
                    cachedInternalStoragePath = Application.persistentDataPath;
#elif UNITY_IPHONE
                    cachedInternalStoragePath = Application.persistentDataPath;
#elif UNITY_ANDROID
                    // Application.persistentDataPath on Android sometimes does not point to internal storage but on vulnerable SD card
                    // http://answers.unity3d.com/questions/282411/acess-internal-storage.html?sort=oldest
                    cachedInternalStoragePath = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getInternalStoragePath");
#elif UNITY_WP8
                    // This is the same as ApplicationData.LocalFolder
                    cachedInternalStoragePath = Application.persistentDataPath;
#endif
                    O7Log.VerboseT(Tag, "InternalStoragePath: {0}", cachedInternalStoragePath);
                }
                return cachedInternalStoragePath;
            }
        }

#if UNITY_EDITOR || NATIVE_SIM
        private const string DevelServerEnabledPref = "AppPlugin.DevelServerEnabled";
#endif

        public static string ServerBaseUrl {
            get {
#if UNITY_EDITOR || NATIVE_SIM
                string url = IsDevelServerEnabled ? "https://be.outfit7.net/" : "https://apps.outfit7.com/";
#elif UNITY_IPHONE
                string url = _GetServerBaseUrl();
#elif UNITY_ANDROID
                string url = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<string>("getServerBaseUrl");
#elif UNITY_WP8
                string url = O7.Plugins.Wp8.UnityCommon.GridNativeProvider.GetServerBaseUrl();
#endif
                O7Log.VerboseT(Tag, "ServerBaseUrl: {0}", url);
                return url;
            }
        }

        public static void SetDevelServerEnabled(bool enable) {
            O7Log.VerboseT(Tag, "SetDevelServerEnabled({0})", enable);

#if UNITY_EDITOR || NATIVE_SIM
            UserPrefs.SetBool(DevelServerEnabledPref, enable);
#elif UNITY_IPHONE
            _SetDevelServerEnabled(enable);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setDevelServerEnabled", enable);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.GridNativeProvider.SetDevelServerEnabled(enable);
#endif
        }

        public static bool IsDevelServerEnabled {
            get {
#if UNITY_EDITOR || NATIVE_SIM
                bool enabled = UserPrefs.GetBool(DevelServerEnabledPref, false);
#elif UNITY_IPHONE
                bool enabled = _IsDevelServerEnabled();
#elif UNITY_ANDROID
                bool enabled = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("isDevelServerEnabled");
#elif UNITY_WP8
                bool enabled = O7.Plugins.Wp8.UnityCommon.GridNativeProvider.IsDevelServerEnabled();
#endif
                O7Log.VerboseT(Tag, "IsDevelServerEnabled: {0}", enabled);
                return enabled;
            }
        }

        public static bool IsAppInstalled(string appId) {
#if UNITY_EDITOR || NATIVE_SIM
            const bool installed = false;
#elif UNITY_IPHONE
            bool installed = _IsAppInstalled(appId);
#elif UNITY_ANDROID
            bool installed = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("isAppInstalled", appId);
#elif UNITY_WP8
            bool installed = O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.IsAppInstalled(appId);
#endif

            O7Log.VerboseT(Tag, "IsAppInstalled({0}): {1}", appId, installed);
            return installed;
        }

        public static bool OpenApp(string appId) {
            O7Log.VerboseT(Tag, "OpenApp({0})", appId);

#if UNITY_EDITOR || NATIVE_SIM
            return true;
#elif UNITY_IPHONE
            return _OpenApp(appId);
#elif UNITY_ANDROID
            return Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("openApp", appId);
#elif UNITY_WP8
            return O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.OpenApp(appId);
#endif
        }

        public static void ResolveAndOpenUrl(string url) {
            O7Log.VerboseT(Tag, "ResolveAndOpenUrl({0})", url);

#if UNITY_EDITOR || NATIVE_SIM
            Application.OpenURL(url);
#elif UNITY_IPHONE
                _ResolveAndOpenUrl(url);
#elif UNITY_ANDROID
                Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("openUrl", url);
#elif UNITY_WP8
                Application.OpenURL(url);
#endif
        }

        /// <summary>
        /// Gets a value indicating that LAN/WiFi network connectivity is available.
        /// </summary>
        /// <value><c>true</c> if LAN/WiFi network is available; otherwise, <c>false</c>.</value>
        public static bool IsLanOrWiFiNetworkAvailable {
            get {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }

        /// <summary>
        /// Gets a value indicating network connectivity is available.
        /// </summary>
        /// <value><c>true</c> if network is available; otherwise, <c>false</c>.</value>
        public static bool IsNetworkAvailable {
            get {
#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
                return O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.GetNetworkReachability();
#else
                return Application.internetReachability != NetworkReachability.NotReachable;
#endif
            }
        }

        private static bool? cachedCameraPresent;
        public static bool IsDeviceCameraPresent
        {
            get
            {
                if (cachedCameraPresent == null)
                {
#if UNITY_EDITOR || NATIVE_SIM || UNITY_IPHONE || UNITY_WP8
                    cachedCameraPresent = true;
#elif UNITY_ANDROID
                    cachedCameraPresent = Outfit7.Util.AndroidPluginManager.Instance.ActivityCall<bool>("checkCameraHardware");
#endif
                    O7Log.VerboseT(Tag, "IsDeviceCameraPresent: {0}", cachedCameraPresent.Value);
                }
                return cachedCameraPresent.Value;
            }
        }
        public static void GoToBackground() {
            O7Log.VerboseT(Tag, "GoToBackground()");
#if NATIVE_SIM
#elif UNITY_EDITOR
#elif UNITY_IPHONE
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("pauseToHome");
#elif UNITY_WP8
#endif
        }

        public static void QuitEx()
        {
#if UNITY_EDITOR
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;
#elif NATIVE_SIM
            Application.Quit();
#elif UNITY_IPHONE
            Application.Quit();
#elif UNITY_ANDROID
            //Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("onUnityClosed");
            Outfit7.Util.AndroidPluginManager.Instance.O7CallStaticMethod("onExit");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.QuitApplication();
            Application.Quit();
#endif
        }

        public static void Quit() {
            O7Log.VerboseT(Tag, "Quit()");
#if UNITY_EDITOR
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;
#elif NATIVE_SIM
            Application.Quit();
#elif UNITY_IPHONE
            Application.Quit();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("onUnityClosed");
            Application.Quit();
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.QuitApplication();
            Application.Quit();
#endif
        }

        public static void AfterStartUpSceneLoad() {
            O7Log.VerboseT(Tag, "AfterStartUpSceneLoad()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _AfterStartUpSceneLoad();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("afterStartUpSceneLoad");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.AfterStartUpSceneLoad();
#endif
        }

        public static void AfterMainSceneLoad() {
            O7Log.VerboseT(Tag, "AfterMainSceneLoad()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _AfterMainSceneLoad();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("afterMainSceneLoad");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.AfterMainSceneLoad();
#endif
        }

        public static void AfterResume() {
            O7Log.VerboseT(Tag, "AfterResume()");

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("AfterUnityResume");
            #endif
        }

        public static void AfterFirstRoomSceneLoad() {
            O7Log.VerboseT(Tag, "AfterFirstRoomSceneLoad()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _AfterFirstRoomSceneLoad();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("afterFirstRoomSceneLoad");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.AfterFirstRoomSceneLoad();
#endif
        }

        public static void DispatchException(string msg, string stackTrace) {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying) {
                // Pause player
                UnityEditor.EditorApplication.isPaused = true;
            }
#elif NATIVE_SIM
            Quit();
#elif UNITY_IPHONE
            _DispatchException(msg, stackTrace);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("dispatchException", msg, stackTrace);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.DispatchException(msg, stackTrace);
#endif
        }

        public static void ResetSleepIdleTimer() {
            O7Log.VerboseT(Tag, "ResetSleepIdleTimer");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _ResetSleepIdleTimer();
#elif UNITY_ANDROID
            // Always on
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.ResetSleepIdleTimer();
#endif
        }

        public static void SetUserPassedAgeGate(bool passed, int birthYear) {
            O7Log.DebugT(Tag, "SetUserPassedAgeGate(passed={0}, birthYear={1})", passed, birthYear);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetAgeGateStateWithBirthYear(passed, birthYear);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setAgeGateState", passed, birthYear);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.SetUserPassedAgeGate(passed);
#endif
        }

        public static void SetUserGender(UserGender userGender) {
            O7Log.DebugT(Tag, "SetUserGender(userGender={0})", userGender);
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetUserGender((int) userGender);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setUserGender", (int) userGender);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.SetUserGender((int) userGender);
#endif
        }

        public static void ToggleNativeLogging(bool enable) {
            O7Log.VerboseT(Tag, "ToggleNativeLogging {0}", enable);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE

#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("nativeLogging", enable);
#elif UNITY_WP8

#endif
        }

        public static void ShowNativeSettings() {
            O7Log.VerboseT(Tag, "ShowNativeSettings");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _O7ToggleAdsDebug();
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("showSettingsActivity");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.ShowSettingsActivity();
#endif
        }

        public static void ShowNativeHtml(string what) {
            O7Log.VerboseT(Tag, "ShowNativeHtml {0}", what);
#if UNITY_EDITOR || NATIVE_SIM
            Application.OpenURL("http://outfit7.com/our-work/");
#elif UNITY_IPHONE
                    _ShowNativeHtml(what);
#elif UNITY_ANDROID
                    Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("showNativeHtml", what);
#elif UNITY_WP8
                    O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.ShowNativeHtml(what);
#endif
        }

        private static bool? CachedSystemRateThisAppDialogRequired;

        public static bool IsSystemRateThisAppDialogRequired
        {
            get
            {
                if (CachedSystemRateThisAppDialogRequired == null)
                {
#if UNITY_EDITOR || NATIVE_SIM
                    CachedSystemRateThisAppDialogRequired = false;
#elif UNITY_IPHONE
                    //CachedSystemRateThisAppDialogRequired = _IsSystemRateThisAppDialogRequired();
#elif UNITY_ANDROID
                    CachedSystemRateThisAppDialogRequired = false;
#elif UNITY_WP8
                    CachedSystemRateThisAppDialogRequired = false;
#endif
                    O7Log.VerboseT(Tag, "IsSystemRateThisAppDialogRequired={0}", CachedSystemRateThisAppDialogRequired);
                }
                return CachedSystemRateThisAppDialogRequired.Value;
            }
        }

        public static void OpenSystemRateThisAppDialog()
        {
            O7Log.VerboseT(Tag, "OpenSystemRateThisAppDialog()");

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            //_OpenSystemRateThisAppDialog();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        public EventBus EventBus { get; set; }

#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        private Outfit7.Threading.Executor MainExecutor;

        private void Awake() {
            MainExecutor = new Outfit7.Threading.Executor();
            O7.Plugins.Wp8.UnityCommon.AppNativeProvider.OnAppActivation += __OnAppActivation;
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.OnNativeDialogCanceled += __OnNativeDialogCanceled;
        }

        private void __OnAppActivation(string payload) {
            MainExecutor.Post(delegate {
                _OnAppActivation(payload);
            });
        }

        private void __OnNativeDialogCanceled() {
            MainExecutor.Post(delegate {
                _NativeDialogCancelled();
            });
        }
#endif

        public void _OnAppActivation(string payload) {
            O7Log.VerboseT(Tag, "_OnAppActivation({0})", payload);
            EventBus.FireEvent(CommonEvents.APP_ACTIVATION, payload);
        }

        public void _NativeDialogCancelled() {
            O7Log.VerboseT(Tag, "_NativeDialogCancelled");
            EventBus.FireEvent(CommonEvents.NATIVE_DIALOG_CANCEL);
        }

        public static void HideSplashScreen() {
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _HideSplashScreen();
#elif UNITY_ANDROID
			Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("hideSplash");
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.HideSplash();
#endif
        }

        public static void SetSplashScreenProgress(float progress) {
            O7Log.VerboseT(Tag, "SetSplashScreenProgress({0})", progress);
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetSplashScreenProgress(progress);
#elif UNITY_ANDROID
			Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setSplashProgress", progress);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.SetSplashScreenProgress(progress);
#endif
        }

        public static long GetDiskFreeSpace() {
#if UNITY_EDITOR || NATIVE_SIM
            throw new System.NotImplementedException();
#elif UNITY_IPHONE
            return _GetDiskFreeSpace();
#elif UNITY_ANDROID
            throw new System.NotImplementedException();
#elif UNITY_WP8
            throw new System.NotImplementedException();
#endif
        }

        [Obsolete("Use SetSplashScreenProgressTextWithColor instead.")]
        public static void SetSplashScreenProgressText(string text) {
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetSplashScreenProgressText(text);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setSplashTip", text);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.SetSplashScreenProgressText(text, 000000);
#endif
        }

        public static void SetSplashScreenProgressTextWithColor(string text, int hexColor) {
            O7Log.VerboseT(Tag, "SetSplashScreenProgressTextWithColor: text = {0}, hexColor = {1}", text, hexColor);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetSplashScreenProgressTextAndColor(text, hexColor);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setSplashTipAndColor", text, hexColor);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.SetSplashScreenProgressText(text, hexColor);
#endif
        }

        public static void SetSplashScreenProgressTextWithColor(string text, int hexColor, int hexColorOutline) {
            O7Log.VerboseT(Tag, "SetSplashScreenProgressTextWithColor: text = {0}, hexColor = {1}, hexColorOutline = {2}", text, hexColor, hexColorOutline);

#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
            _SetSplashScreenProgressTextColorAndOutlineColor(text, hexColor, hexColorOutline);
#elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.ActivityCall("setSplashTipAndColor", text, hexColor, hexColorOutline);
#elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.CommonNativeProvider.SetSplashScreenProgressTextWithOutline(text, hexColor, hexColorOutline);
#endif
        }

        public static void StartDownloadingAssets() {
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
           _StartDownloadingAssets();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        public static void StartAssetExtractingEvent() {
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
                    _StartAssetExtracting();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        public static void DoneAssetExtractingEvent() {
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
                    _DoneAssetExtracting();
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        public static void AssetExtractingErrorEvent(string error) {
#if UNITY_EDITOR || NATIVE_SIM

#elif UNITY_IPHONE
                    _AssetExtractingError(error);
#elif UNITY_ANDROID

#elif UNITY_WP8

#endif
        }

        public static long NativeHeapSize {
            get {
                long heapSize = -1;
#if UNITY_EDITOR || NATIVE_SIM
                heapSize = Profiler.usedHeapSize;
#elif UNITY_IPHONE

#elif UNITY_ANDROID
                // https://developer.android.com/reference/android/os/Debug.html#getNativeHeapSize()
                heapSize = AndroidPluginManager.Instance.ActivityCall<long>("getNativeHeapSize");
#elif UNITY_WP8

#endif
                O7Log.VerboseT(Tag, "NativeHeapSize = {0}", heapSize);
                return heapSize;
            }
        }

        public static long NativeHeapAllocatedSize {
            get {
                long heapSize = -1;
#if UNITY_EDITOR || NATIVE_SIM
                heapSize = Profiler.GetTotalAllocatedMemory();
#elif UNITY_IPHONE

#elif UNITY_ANDROID
                // https://developer.android.com/reference/android/os/Debug.html#getNativeHeapAllocatedSize()
                heapSize = AndroidPluginManager.Instance.ActivityCall<long>("getNativeHeapAllocatedSize");
#elif UNITY_WP8

#endif
                O7Log.VerboseT(Tag, "NativeHeapAllocatedSize = {0}", heapSize);
                return heapSize;
            }
        }

        public static long NativeHeapFreeSize {
            get {
                long heapSize = -1;
#if UNITY_EDITOR || NATIVE_SIM
                heapSize = Profiler.GetTotalUnusedReservedMemory();
#elif UNITY_IPHONE

#elif UNITY_ANDROID
                // https://developer.android.com/reference/android/os/Debug.html#getNativeHeapFreeSize()
                heapSize = AndroidPluginManager.Instance.ActivityCall<long>("getNativeHeapFreeSize");
#elif UNITY_WP8

#endif
                O7Log.VerboseT(Tag, "NativeHeapFreeSize = {0}", heapSize);
                return heapSize;
            }
        }
    }
}
