
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Social.Network {

    /// <summary>
    /// VKontakte social plugin.
    /// </summary>
    public class VKontaktePlugin : AbstractSocialPlugin {

        protected override string Tag {
            get {
                return "VKontaktePlugin";
            }
        }

        protected override string AndroidGetter {
            get {
                return "getVkontakte";
            }
        }

#if UNITY_IPHONE && !NATIVE_SIM
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _VKInit();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string _VKUser();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _VKSubscribe();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _VKLogin(string[] permission, int permissionCount);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _VKLogout();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern bool _VKIsLoggedIn();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _VKFriendsThatUseThisApp();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void _VKInviteFriends(string inviteMessage, string excludeList);
#endif

        public override bool NativeInviteSupport {
            get {
                return false;
            }
        }

        public override void Init() {
            O7Log.DebugT(Tag, "Init");

            #if UNITY_EDITOR || NATIVE_SIM
            base.Init();
            #elif UNITY_IPHONE
             _VKInit();
            #elif UNITY_WP8

            #elif UNITY_ANDROID
            base.Init();
            #endif
        }

        public override void Logout() {
            O7Log.DebugT(Tag, "Logout");

            #if UNITY_EDITOR || NATIVE_SIM
            base.Logout();
            #elif UNITY_IPHONE
             _VKLogout();
            #elif UNITY_WP8

            #elif UNITY_ANDROID
            base.Logout();
            #endif
        }

        public virtual void Subscribe() {
            O7Log.DebugT(Tag, "Subscribe");

            #if UNITY_EDITOR || NATIVE_SIM
            MainExecutor.Post(SubscribeCompleted);
            #elif UNITY_IPHONE
            _VKSubscribe();
            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "subscribe");
            #endif
        }

        public virtual void SubscribeCompleted() {
            (SocialHelper as VKontakteHelper).SubscribeCompleted();
        }

        public virtual void SubscribeFailed() {
            (SocialHelper as VKontakteHelper).SubscribeFailed();
        }

        public override void InviteFriends(string inviteMessage, string excludeList) {
            O7Log.DebugT(Tag, "InviteFriends");

            #if UNITY_EDITOR || NATIVE_SIM
            base.InviteFriends(inviteMessage, excludeList);
            #elif UNITY_IPHONE
             _VKInviteFriends(inviteMessage, excludeList);
            #elif UNITY_WP8

            #elif UNITY_ANDROID

            #endif
        }

        public override void FriendsThatUseThisApp() {
            O7Log.DebugT(Tag, "FriendsThatUseThisApp");

            #if UNITY_EDITOR || NATIVE_SIM
            WWW www = new WWW("file:///" + Application.dataPath + "/EditorTestFiles/VKontakteFriends.json.txt");
            while (!www.isDone) {
            }
            MainExecutor.Post(delegate {
                FriendsUsingAppListCompleted(www.text);
            });
            #elif UNITY_IPHONE
            _VKFriendsThatUseThisApp();
            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "getAppUsingFriends");
            #endif
        }

        public override string User {
            get {
                O7Log.DebugT(Tag, "User");

                #if UNITY_EDITOR || NATIVE_SIM
                return "{\"installed\" : true,\"id\" : \"289758048\",\"first_name\" : \"\u010ci\u0161\u017e\",\"last_name\" : \"Dva\",\"img_url\" : \"https:\\/\\/pp.vk.me\\/c621531\\/v621531048\\/e416\\/BSa0bL470JU.jpg\"}";
                #elif UNITY_IPHONE
                return _VKUser();
                #elif UNITY_WP8
                return string.Empty;
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
                loggedIn = _VKIsLoggedIn();
                #elif UNITY_WP8
                loggedIn = false;
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
            _VKLogin(permissions, permissions == null ? 0 : permissions.Length);
            #elif UNITY_WP8

            #elif UNITY_ANDROID
            base.LogInWithPermissions(permissions);
            #endif
        }
    }
}
