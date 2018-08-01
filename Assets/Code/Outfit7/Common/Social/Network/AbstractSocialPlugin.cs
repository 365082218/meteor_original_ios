//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using Outfit7.Threading;
using Outfit7.Util;
using UnityEngine;

namespace Outfit7.Social.Network {

    /// <summary>
    /// Abstract social plugin.
    /// </summary>
    public abstract class AbstractSocialPlugin : MonoBehaviour {

        protected abstract string Tag { get; }

#if UNITY_EDITOR || NATIVE_SIM
        protected bool IsLoggedIn { get; set; }
#endif

#if UNITY_EDITOR || NATIVE_SIM || UNITY_WP8
        protected Executor MainExecutor { get; private set; }
#endif

        public AbstractSocialHelper SocialHelper { get; set; }

        public abstract bool NativeInviteSupport { get; }

        protected abstract string AndroidGetter { get; }

        public abstract string[] Permissions { get; }

        public bool LoginInProgress { get; private set; }

        private bool InitializationInProgress;
        private bool reInit;
        private float LogInTimeout;

#if UNITY_EDITOR || NATIVE_SIM || UNITY_WP8
        protected virtual void RegisterCallbacks() {
        }

        protected virtual void Awake() {
            MainExecutor = new Executor();
#if !(UNITY_EDITOR || NATIVE_SIM)
            RegisterCallbacks();
#endif
        }
#endif

        protected virtual void Start() {
            O7Log.DebugT(Tag, "Start");
            reInit = false;
        }

        protected virtual void OnApplicationPause(bool gamePaused) {
            if (!gamePaused && LoginInProgress && LogInTimeout < Time.time) {
                O7Log.DebugT(tag, "Game resumed after SSO");
                LogInTimeout = Time.time + 10; // timeout login if no activity after few seconds (should be almost instant)
            }
        }

        protected virtual void Update() {
            if (reInit) {
                O7Log.DebugT(Tag, "Update reinit");
                reInit = false;
                Init();
            }

            if (LoginInProgress && LogInTimeout > 0 && Time.time > LogInTimeout) {
                LoginFailed("Timeouted");
            }
        }

        public virtual bool LogIn() {
            if (LoggedIn) return false;
            if (LoginInProgress) return false;

            O7Log.DebugT(Tag, "Login");
            LoginInProgress = true;
            LogInWithPermissions(Permissions);
            return true;
        }

        public virtual void ReInit() {
            if (LoginInProgress) return; // in use when SSO
            if (InitializationInProgress) return;

            O7Log.DebugT(Tag, "Schedule reinit");
            reInit = true;
        }

        public virtual void Init() {
            Assert.IsTrue(!InitializationInProgress, "Initialization already in progress");

            InitializationInProgress = true;

            O7Log.DebugT(Tag, "Init");

            #if UNITY_EDITOR || NATIVE_SIM
            MainExecutor.Post(OnInitCompleted);
            #elif UNITY_IPHONE

            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "init");
            #endif
        }

        public virtual void Logout() {
            O7Log.DebugT(Tag, "Logout");

            #if UNITY_EDITOR || NATIVE_SIM
            IsLoggedIn = false;
            #elif UNITY_IPHONE

            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "logOut");
            #endif
        }

        public virtual void InviteFriends(string inviteMessage, string excludeList) {
            O7Log.DebugT(Tag, "InviteFriends list {0}", excludeList);

            #if UNITY_EDITOR || NATIVE_SIM
            MainExecutor.Post(delegate {
                InviteFriendsCompleted("unity_test_01,unity_test_02");
            });
            #elif UNITY_IPHONE

            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "inviteFriends", inviteMessage, excludeList);
            #endif
        }

        public virtual void FriendsThatUseThisApp() {
            O7Log.DebugT(Tag, "FriendsThatUseThisApp");

            #if UNITY_EDITOR || NATIVE_SIM

            #elif UNITY_IPHONE

            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "getAppUsingFriends");
            #endif
        }

        public virtual string User {
            get {
                #if UNITY_EDITOR || NATIVE_SIM
                return null;
                #elif UNITY_IPHONE
                return null;
                #elif UNITY_WP8
                return null;
                #elif UNITY_ANDROID
                return Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<string>(AndroidGetter, "getUser");
                #endif
            }
        }

        public virtual bool LoggedIn {
            get {
                #if UNITY_EDITOR || NATIVE_SIM
                return IsLoggedIn;
                #elif UNITY_IPHONE
                return false;
                #elif UNITY_WP8
                return false;
                #elif UNITY_ANDROID
                return Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef<bool>(AndroidGetter, "isLoggedIn");
                #endif
            }
        }

        public virtual void LogInWithPermissions(string[] permissions) {
            O7Log.DebugT(Tag, "LogInWithPermissions");

            #if UNITY_EDITOR || NATIVE_SIM
            IsLoggedIn = true;
            MainExecutor.Post(delegate {
                LoginCompleted(null);
            });
            #elif UNITY_IPHONE

            #elif UNITY_WP8

            #elif UNITY_ANDROID
            Outfit7.Util.AndroidPluginManager.Instance.CallAnActivityRef(AndroidGetter, "logIn");
            #endif
        }

#region Invite friends

        public void InviteFriendsCompleted(string data) {
            SocialHelper.InviteFriendsCompleted(data);
        }

#endregion

#region Login

        public void LoginCompleted(string data) {
            LoginInProgress = false;
            LogInTimeout = 0;
            SocialHelper.LoginCompleted();
        }

        public void LoginFailed(string error) {
            LoginInProgress = false;
            LogInTimeout = 0;
            SocialHelper.LoginFailed(error);
        }

#endregion

#region Init

        public void FriendsUsingAppListCompleted(string data) {
            // data is never null (comes from native)
            SocialHelper.FriendsUsingAppListCompleted(data);
        }

        public void FriendsUsingAppListFailed(string data) {
            // data is never null (comes from native)
            SocialHelper.FriendsUsingAppListFailed(data);
        }

        public void OnInitCompleted() {
            SocialHelper.OnInitCompleted();
            InitializationInProgress = false;
        }

#endregion
    }
}
