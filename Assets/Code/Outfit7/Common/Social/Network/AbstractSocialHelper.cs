//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Analytics.BigQuery;
using Outfit7.Analytics.Tracking;
using Outfit7.Common;
using Outfit7.Event;
using Outfit7.Json;
using Outfit7.Purchase;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Social.Network {

    /// <summary>
    /// Abstract social helper.
    /// </summary>
    public abstract class AbstractSocialHelper {

        protected int FriendsUsingAppListLoadingInProgressCount;
        protected float LastInitTime;

        protected abstract string Tag { get; }

        public abstract SocialNetworkType SocialType { get; }

        protected abstract IapPack Iap { get; }

        protected abstract string AutoLoginKey { get; }

        protected abstract string SelectedKey { get; }

        public abstract string LoginText { get; }

        public abstract bool WasRewarded { get; }

        public abstract string Id { get; }

        protected abstract string InviteFriendsMessage { get; }

        public AbstractPurchaseManager PurchaseManager { get; set; }

        public EventBus EventBus { get; set; }

        public AgeGateManager AgeGateManager { get; set; }

        public AbstractSocialPlugin SocialPlugin { get; set; }

        public BigQueryTracker BqTracker { get; set; }

        public delegate void FriendsWithInstalledAppDelegate(List<string> friends);

        public virtual bool FriendsUsingAppListLoadingInProgress {
            get {
                return FriendsUsingAppListLoadingInProgressCount > 0;
            }
        }

        public virtual Dictionary<string, SocialFriend> Friends { get; protected set; }

        public virtual SocialFriend User { get; protected set; }

        public virtual List<string> FriendsWithAppInstalledIdList { get; protected set; }

        public virtual List<SocialFriend> FriendsToInvite { get; protected set; }

        public virtual bool Initialized { get; protected set; }

        protected Action OnLoginAction;

        public virtual bool LoginInProgress { get { return SocialPlugin.LoginInProgress; } }

        public virtual bool SelectedNetwork {
            protected set {
                UserPrefs.SetBool(SelectedKey, value);
                UserPrefs.SaveDelayed();
            }
            get {
                return UserPrefs.GetBool(SelectedKey, false);
            }
        }

        public virtual bool NativeInviteSupport {
            get {
                return SocialPlugin.NativeInviteSupport;
            }
        }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");
            LastInitTime = Time.time;

            Dictionary<string, SocialFriend> friends = SocialFriendPersister.LoadFriendsData(SocialFriendDataFileName);
            FriendUsingAppListData friendsData = new FriendUsingAppListData(friends, FriendMustHaveAppInstalled);

            User = SocialFriendPersister.LoadUserData(SocialUserDataKey);

            UpdateFriendsData(friendsData);

            SocialPlugin.Init();

            OnGridChange(null);

            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, OnGridChange);
        }

        protected virtual void OnGridChange(object eventData) {
        }

        public virtual bool Available {
            get {
                return Initialized && (AgeGateManager == null || AgeGateManager.MustAskOrDidPass) && (IsEnabled || SelectedNetwork);
            }
        }

        public virtual bool IsEnabled {
            get {
                return PurchaseManager.FindGridIapPack(Iap) != null;
            }
        }

        public virtual void SyncReward() {
            O7Log.DebugT(Tag, "SyncReward");
            if (!IsEnabled) return;

            if (LoggedIn && !WasRewarded) {
                PurchaseManager.RewardPurchase(Iap, true);
            }
        }

        protected bool loggedIn;

        public virtual bool LoggedIn {
            get {
                return Initialized && loggedIn;
            }
        }

        public virtual void LogIn() {
            Assert.State(Available, "Login not available");
            if (AgeGateManager != null) {
                Assert.State(AgeGateManager.DidPass, "User did not pass age gate");
            }
            if (SocialPlugin.LogIn()) {
                EventBus.FireEvent(CommonEvents.SOCIAL_LOGIN_START, this);
            }
        }

        public virtual void InviteFriends(string inviteesCSV) { // in use with new invite dialog
            O7Log.DebugT(Tag, "Invite friends with include list {0}", inviteesCSV);
            SocialPlugin.InviteFriends(InviteFriendsMessage, inviteesCSV);
        }

        public virtual void InviteFriends() {
            string excludeList = null;
            JSONArray excludedFriendsJ = SimpleJsonUtils.CreateJsonArray(FriendsWithAppInstalledIdList);
            if (excludedFriendsJ != null && excludedFriendsJ.Count > 0) {
                excludeList = excludedFriendsJ.ToString();
            }

            O7Log.DebugT(Tag, "Invite friends with exclude list {0}", excludeList);
            SocialPlugin.InviteFriends(InviteFriendsMessage, excludeList);
        }

        public virtual void LogOut() {
            // Allow log out without GRID data
            O7Log.DebugT(Tag, "LogOut");
            if (loggedIn) {
                SocialPlugin.Logout();

                BqTracker.CreateBuilder(CommonTrackingEventParams.GroupId.Social,
                    CommonTrackingEventParams.EventId.LogOut, true)
                    .SetP1(Id)
                    .SetP2(User.Id)
                    .Add();
            }

            ClearSocialData();
        }

        protected virtual void ClearSocialData() {
            loggedIn = false;
            O7Log.DebugT(Tag, "ClearSocialData {0}", Id);

            // clear user data
            User = null;
            UserPrefs.Remove(SocialUserDataKey);
            loggedIn = false;

            // clear friends data
            Friends = null;
            FriendsToInvite = null;
            FriendsWithAppInstalledIdList = null;
            SocialFriendPersister.DeleteFile(SocialFriendDataFileName);

            // clear other flags
            UserPrefs.Remove(AutoLoginKey);
            UserPrefs.Remove(SelectedKey);
            UserPrefs.SaveDelayed();

            // notify others
            SyncReward();
            EventBus.FireEvent(CommonEvents.SOCIAL_LOGIN_CHANGE, this);
            EventBus.FireEvent(CommonEvents.SOCIAL_FRIENDS_UPDATE);
        }

        public virtual void FriendsUsingAppListFailed(string data) {
            O7Log.DebugT(Tag, "FriendsUsingAppListFailed {0} updates in progress {1}", data, FriendsUsingAppListLoadingInProgressCount);
            FriendsUsingAppListLoadingInProgressCount--;
        }

        protected virtual void UpdateFriendsData(FriendUsingAppListData data) {
            if (data == null) return;

            O7Log.DebugT(Tag, "UpdateFriendsData friends {0}", CollectionUtils.Count(data.Friends));

            List<string> oldFriendsUsingApp = FriendsWithAppInstalledIdList;

            Friends = data.Friends;
            FriendsWithAppInstalledIdList = data.FriendsThatUseThisApp;
            FriendsToInvite = data.FriendsToInvite;

            if (data.FriendsUpdated) { // Only on log-in or actual friends change
                TryCreateFriendsChangeEvent(oldFriendsUsingApp, FriendsWithAppInstalledIdList);
            }
        }

        protected virtual bool TryCreateFriendsChangeEvent(List<string> oldFriendsUsingApp, List<string> newFriendsUsingApp) {
            int oldFriendUsingAppCount = CollectionUtils.Count(oldFriendsUsingApp);
            int newFriendUsingAppCount = CollectionUtils.Count(newFriendsUsingApp);

            if (newFriendUsingAppCount == oldFriendUsingAppCount) return false;

            BqTracker.CreateBuilder(CommonTrackingEventParams.GroupId.Social,
                CommonTrackingEventParams.EventId.FriendList, true)
                .SetP1(Id)
                .SetP3(newFriendUsingAppCount)
                .Add();

            return true;
        }

        protected class FriendUsingAppListData {

            public List<string> FriendsThatUseThisApp { get; private set; }

            public List<SocialFriend> FriendsToInvite { get; private set; }

            public Dictionary<string, SocialFriend> Friends { get; private set; }

            public bool FriendsUpdated { get; internal set; }

            public FriendUsingAppListData(Dictionary<string, SocialFriend> friends, bool mustHaveInstalled) {
                Friends = friends;
                if (CollectionUtils.IsEmpty(friends)) return;

                FriendsThatUseThisApp = new List<string>(friends.Count);
                FriendsToInvite = new List<SocialFriend>(friends.Count);

                foreach (KeyValuePair<string, SocialFriend> friendPair in friends) {
                    if (friendPair.Value.Installed == mustHaveInstalled) {
                        FriendsThatUseThisApp.Add(friendPair.Value.Id);
                    }
                    if (!friendPair.Value.Installed) {
                        FriendsToInvite.Add(friendPair.Value);
                    }
                }
            }
        }

        protected virtual bool FriendMustHaveAppInstalled {
            get {
                return true;
            }
        }

        public virtual void FriendsThatUseThisApp() {
            O7Log.DebugT(Tag, "FriendsThatUseThisApp ... updates in progress {0}", FriendsUsingAppListLoadingInProgressCount);
            FriendsUsingAppListLoadingInProgressCount++;
            SocialPlugin.FriendsThatUseThisApp();
        }

        public virtual void OnAppStartOrResume() {
            if (Time.time - LastInitTime < 60) { // don't reinit to often ... wait 1 min in between
                O7Log.DebugT(Tag, "Skiping reinit");
                return;
            }
            LastInitTime = Time.time;
            SocialPlugin.ReInit();
        }

#region Invite friends

        public virtual void InviteFriendsCompleted(string data) {
            O7Log.DebugT(Tag, "InviteFriendsCompleted {0}", data);

            List<string> invitedFriendIds = StringUtils.CommaDelimitedListToStringList(data);

            foreach (string friendId in invitedFriendIds) {
                BqTracker.CreateBuilder(CommonTrackingEventParams.GroupId.Social,
                    CommonTrackingEventParams.EventId.InviteFriend, true)
                    .SetP2(friendId)
                    .Add();
            }
        }

#endregion

#region Login

        public virtual void LoginCompleted() {
            if (loggedIn) {
                O7Log.WarnT(Tag, "Login already completed");
                return;
            }

            O7Log.DebugT(Tag, "LoginCompleted");

            UpdateUserData();

            SelectedNetwork = true;
            Initialized = true;

            loggedIn = SocialPlugin.LoggedIn;

            SyncReward();

            // search for friends using this apps
            FriendsThatUseThisApp();
            EventBus.FireEvent(CommonEvents.SOCIAL_LOGIN_CHANGE, this);

            BqTracker.CreateBuilder(CommonTrackingEventParams.GroupId.Social, CommonTrackingEventParams.EventId.LogIn, true)
                .SetP1(Id)
                .SetP2(User.Id)
                .Add();

            if (OnLoginAction != null) {
                OnLoginAction();
            }
        }

        public virtual void LoginFailed(string error) {
            Initialized = true;

            O7Log.DebugT(Tag, "LoginFailed with error: {0}", error);
            ClearSocialData();

            if (OnLoginAction != null) {
                OnLoginAction();
            }
        }

#endregion

#region Init

        public virtual void UpdateUserData() {
            string userData = SocialPlugin.User;

            SocialFriendPersister.SaveUserData(userData, SocialUserDataKey);

            O7Log.DebugT(Tag, "UpdateUserData {0}", userData);

            User = SocialFriendPersister.ParseUserData(userData);
        }

        public virtual void OnInitCompleted() {
            O7Log.DebugT(Tag, "OnInitCompleted");

            loggedIn = SocialPlugin.LoggedIn;

            if (loggedIn) {
                Initialized = true;
                // load user data
                UpdateUserData();
                // search for friends using this apps
                FriendsThatUseThisApp();
                SyncReward();
                return;
            }

            bool triedAutoLogin = UserPrefs.GetBool(AutoLoginKey, false);
            bool shouldTryAutoLogin = !triedAutoLogin && Available && SelectedNetwork;

            O7Log.DebugT(Tag, "shouldTryAutoLogin {0} Logged in {1} selected {2} autologin {3}", shouldTryAutoLogin, loggedIn, SelectedNetwork, triedAutoLogin);

            if (triedAutoLogin) { // reset autologin - ony one try
                UserPrefs.SetBool(AutoLoginKey, false);
                UserPrefs.SaveDelayed();
            }

            if (shouldTryAutoLogin) { // try to autologin
                UserPrefs.SetBool(AutoLoginKey, true);
                UserPrefs.SaveDelayed();
                LogIn();
                return;
            }

            Initialized = true;
            if (SelectedNetwork || !CollectionUtils.IsEmpty(Friends)) {
                ClearSocialData();
            }
        }

        public virtual void FriendsUsingAppListCompleted(string data) {
            if (!LoggedIn) {
                O7Log.DebugT(Tag, "FriendsUsingAppListCompleted not logged in ... updates in progress {0}", FriendsUsingAppListLoadingInProgressCount);
                FriendsUsingAppListLoadingInProgressCount--;
                return;
            }

            O7Log.DebugT(Tag, "FriendsUsingAppListCompleted friend list '{0}'", data);

            if (data.Length == 0 && Friends == null) { // empty nothing to do
                O7Log.DebugT(Tag, "FriendsUsingAppListCompleted data len 0 & friends null ... updates in progress {0}", FriendsUsingAppListLoadingInProgressCount);
                FriendsUsingAppListLoadingInProgressCount--;
                return;
            }

            TaskFeedback<FriendUsingAppListData> FriendsUsingAppListCompletedFeedback = new TaskFeedback<FriendUsingAppListData>(delegate {
                O7Log.DebugT(Tag, "FriendsUsingAppListCompletedFeedback No friends ... updates in progress {0}", FriendsUsingAppListLoadingInProgressCount);
                FriendsUsingAppListLoadingInProgressCount--;

            }, delegate(FriendUsingAppListData friendsData) {
                if (LoggedIn) {
                    UpdateFriendsData(friendsData);
                    if (friendsData.FriendsUpdated) {
                        O7Log.DebugT(Tag, "Found friends that are using app list mismatch {0}", CollectionUtils.Count(friendsData.FriendsThatUseThisApp));
                        EventBus.FireEvent(CommonEvents.SOCIAL_FRIENDS_UPDATE, friendsData.FriendsThatUseThisApp);
                    }
                }
                O7Log.DebugT(Tag, "FriendsUsingAppListCompletedFeedback ... updates in progress {0}", FriendsUsingAppListLoadingInProgressCount);
                FriendsUsingAppListLoadingInProgressCount--;

            }, delegate(Exception e) {
                O7Log.WarnT(Tag, e, "FriendsUsingAppListCompletedFeedback exception ... updates in progress  {0}", FriendsUsingAppListLoadingInProgressCount);
                FriendsUsingAppListLoadingInProgressCount--;
            });

            TaskFeedback<FriendUsingAppListData> executorFeedback = new ExecutorTaskFeedbackWrapper<FriendUsingAppListData>(FriendsUsingAppListCompletedFeedback);

            bool currentFriendMustHaveAppInstalled = FriendMustHaveAppInstalled;
            List<string> currentFriendsWithAppInstalledIdList = FriendsWithAppInstalledIdList;
            string currentDataFileName = SocialFriendDataFileName;

            Action runner = delegate {
                try {
                    Dictionary<string, SocialFriend> updatedFriends = SocialFriendPersister.ParseSocialFriends(data);
                    if (updatedFriends == null) {
                        executorFeedback.OnCancel();
                        return;
                    }

                    FriendUsingAppListData friendsData = new FriendUsingAppListData(updatedFriends, currentFriendMustHaveAppInstalled);
                    if (!CollectionUtils.EqualsAll(friendsData.FriendsThatUseThisApp, currentFriendsWithAppInstalledIdList)) { // save only if collections differ
                        SocialFriendPersister.WriteFriendsData(data, currentDataFileName);
                        friendsData.FriendsUpdated = true;
                    }
                    executorFeedback.OnFinish(friendsData);

                } catch (Exception e) {
                    executorFeedback.OnError(e);
                }
            };
            SimpleWorker.RunAsync(runner);
        }

        protected abstract string SocialFriendDataFileName{ get; }

        protected abstract string SocialUserDataKey{ get; }

#endregion
    }
}
