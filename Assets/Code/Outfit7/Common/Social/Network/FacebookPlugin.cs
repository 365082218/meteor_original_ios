
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Social.Network {

    /// <summary>
    /// Facebook social plugin.
    /// </summary>
    public class FacebookPlugin : AbstractSocialPlugin {

        protected override string Tag {
            get {
                return "FacebookPlugin";
            }
        }

        protected override string AndroidGetter {
            get {
                return "getFacebook";
            }
        }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _FBInit();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _FBUser();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _FBLogin(string[] permission, int permissionCount);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _FBLogout();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _FBIsLoggedIn();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _FBFriendsThatUseThisApp();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _FBInviteFriends(string inviteMessage, string excludeList);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _ShowLikeDialog();
#endif

#if UNITY_WP8 && !(UNITY_EDITOR || NATIVE_SIM)
        protected override void RegisterCallbacks() {
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.OnFacebookInitCompleted += __OnFacebookInitCompleted;
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.OnFacebookLoginSuccess += __OnFacebookLoginSuccess;
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.OnFacebookLoginFailed += __OnFacebookLoginFailed;
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.OnFacebookFriendsUsingApp += __OnFacebookFriendsUsingApp;
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.OnFacebookFriendsUsingAppFailed += __OnFacebookFriendsUsingAppFailed;
        }

        private void __OnFacebookInitCompleted() {
            // NOT on main thread!
            MainExecutor.Post(OnInitCompleted);
        }

        private void __OnFacebookLoginSuccess() {
            // NOT on main thread!
            MainExecutor.Post(delegate{
                LoginCompleted(null);
            });
        }

        private void __OnFacebookLoginFailed(string errMsg) {
            // NOT on main thread!
            MainExecutor.Post(delegate{
                LoginFailed(errMsg);
            });
        }

        private void __OnFacebookFriendsUsingApp(string data) {
            // NOT on main thread!
            MainExecutor.Post(delegate{
                FriendsUsingAppListCompleted(data);
            });
        }

        private void __OnFacebookFriendsUsingAppFailed(string errMsg) {
            // NOT on main thread!
            MainExecutor.Post(delegate{
                FriendsUsingAppListFailed(errMsg);
            });
        }
#endif

        public override bool NativeInviteSupport {
            get {
                #if UNITY_EDITOR || NATIVE_SIM
                return true;
                #elif UNITY_IPHONE
                return true;
                #elif UNITY_WP8
                return false;
                #elif ANDROID_AMAZON
                return false;
                #elif UNITY_ANDROID
                return true;
                #endif
            }
        }

        public override void Init() {
            O7Log.DebugT(Tag, "Init");

            #if UNITY_EDITOR || NATIVE_SIM
            base.Init();
            #elif UNITY_IPHONE
             _FBInit();
            #elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.FBInit();
            #elif UNITY_ANDROID
            base.Init();
            #endif
        }

        public override void Logout() {
            O7Log.DebugT(Tag, "Logout");

            #if UNITY_EDITOR || NATIVE_SIM
            base.Logout();
            #elif UNITY_IPHONE
             _FBLogout();
            #elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.FBLogout();
            #elif UNITY_ANDROID
            base.Logout();
            #endif

        }

        public override void InviteFriends(string inviteMessage, string excludeList) {
            O7Log.DebugT(Tag, "InviteFriends");

            #if UNITY_EDITOR || NATIVE_SIM
            base.InviteFriends(inviteMessage, excludeList);
            #elif UNITY_IPHONE
             _FBInviteFriends(inviteMessage, excludeList);
            #elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.FBInviteFriends(inviteMessage, excludeList);
            #elif UNITY_ANDROID
            base.InviteFriends(inviteMessage, excludeList);
            #endif

        }

        public override void FriendsThatUseThisApp() {
            O7Log.DebugT(Tag, "FriendsThatUseThisApp");

            #if UNITY_EDITOR || NATIVE_SIM
            WWW www = new WWW("file:///" + Application.dataPath + "/EditorTestFiles/facebookFriends.json.txt");
            while (!www.isDone) {
            }
            MainExecutor.Post(delegate {
                FriendsUsingAppListCompleted(www.text);
            });
            #elif UNITY_IPHONE
            _FBFriendsThatUseThisApp();
            #elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.FBFriendsThatUseThisApp();
            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "getAppUsingFriends");
            #endif
        }

        public override string User {
            get {
                O7Log.DebugT(Tag, "User");

                #if UNITY_EDITOR || NATIVE_SIM
                return "{\"installed\" : true,\"id\" : \"100003691341479\",\"first_name\" : \"\u010ci\u0161\u017e\",\"last_name\" : \"Dva\",\"img_url\" : \"http:\\/\\/graph.facebook.com\\/100003691341479\\/picture?type=square&width=128&height=128\"}";
                #elif UNITY_IPHONE
                return _FBUser();
                #elif UNITY_WP8
                return O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.FBUserData();
                #elif UNITY_ANDROID
                return base.User;
                #endif
            }
        }

        public override bool LoggedIn {
            get {
                bool loggedIn;

                #if UNITY_EDITOR || NATIVE_SIM
                loggedIn = base.LoggedIn;
                #elif UNITY_IPHONE
                loggedIn = _FBIsLoggedIn();
                #elif UNITY_WP8
                loggedIn = O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.FBLoggedIn();
                #elif UNITY_ANDROID
                loggedIn = base.LoggedIn;
                #endif

                O7Log.DebugT(Tag, "LoggedIn {0}", loggedIn);
                return loggedIn;
            }
        }

        public override string[] Permissions {
            get {
                return new string[0];
            }
        }

        public override void LogInWithPermissions(string[] permissions) {
            O7Log.DebugT(Tag, "LogInWithPermissions");

            #if UNITY_EDITOR || NATIVE_SIM
            base.LogInWithPermissions(permissions);
            #elif UNITY_IPHONE
            _FBLogin(permissions, permissions == null ? 0 : permissions.Length);
            #elif UNITY_WP8
            O7.Plugins.Wp8.UnityCommon.SocialNativeProvider.FBLogin(permissions);
            #elif UNITY_ANDROID
            base.LogInWithPermissions(permissions);
            #endif
        }

        public virtual void ShowLikeDialog() {
            O7Log.DebugT(Tag, "ShowLikeDialog");

            #if UNITY_EDITOR || NATIVE_SIM
            MainExecutor.PostDelayed(() => LikeButtonPressed("true"), 1);
            Application.OpenURL("http://www.facebook.com");
            #elif UNITY_IPHONE
            _ShowLikeDialog();
            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "showLikeDialog");
            #endif
        }

        public virtual void LikeButtonPressed(string data) {
            (SocialHelper as FacebookHelper).Liked();
        }
    }
}
