//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Social.Network;
using Outfit7.Threading.Task;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Social {

    /// <summary>
    /// Social users manager.
    /// </summary>
    public abstract class SocialUserManager<U> where U : BasicSocialUser {

        protected const String Tag = "SocialUserManager";

        protected TimeSpan ReloadUsersIdleTime = BuildConfig.IsDevel ? TimeSpan.FromMinutes(1) : TimeSpan.FromDays(1);
        protected bool IsParsingUsers;

        public EventBus EventBus { get; set; }

        public GridManager GridManager { get; set; }

        public SocialNetwork SocialNetwork { get; set; }

        public SocialUserWorker<U> SocialUserWorker { get; set; }

        public ISocialUserDownloader SocialUserDownloader { get; set; }

        protected virtual bool HasInvitees {
            get {
                return SocialNetwork.SocialHelper.Friends != null;
            }
        }

        public virtual bool ShouldDownloadStrangers
        {
            get {
                // Refresh users from backend if "reload time"
                TimeSpan timeSinceLastDownload = DateTime.UtcNow - SocialUserWorker.SocialUserPersister.SaveTime.ToUniversalTime();
                O7Log.DebugT(Tag, "Time since last download of social users: {0}", timeSinceLastDownload);

                if (timeSinceLastDownload < TimeSpan.Zero) {
                    // Force reloading users if in future MTA-7064
                    O7Log.WarnT(Tag, "Time since last download of social users is in future!");
                    return true;
                }

                return timeSinceLastDownload >= ReloadUsersIdleTime;
            }
        }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");

            LoadUsers();

            EventBus.AddListener(CommonEvents.SOCIAL_FRIENDS_UPDATE, OnFriendsUpdate);
            EventBus.AddListener(CommonEvents.PUSH_START, OnPushReceive);
            EventBus.AddListener(CommonEvents.PUSH_RECEIVE, OnPushReceive);

            O7Log.DebugT(Tag, "Done");
        }

        protected virtual void LoadUsers() {
            var invitees = SocialNetwork.SocialHelper.Friends;
            Pair<List<U>, List<U>> pair = SocialUserWorker.LoadUsers(invitees);

            if (pair != null) {
                OnUsersLoad(pair);
            }
        }

        protected virtual void OnUsersLoad(Pair<List<U>, List<U>> users) {
            Friends = users.First;
            Strangers = users.Second;
        }

        public virtual void PostUpdateUsers(JSONNode usersJ) {
            if (SocialUserDownloader.IsDownloadInProgress) {
                // Another downloading is in progress that will call this method again
                return;
            }

            var invitees = SocialNetwork.SocialHelper.Friends;

            TaskFeedback<Pair<List<U>, List<U>>> task = new TaskFeedback<Pair<List<U>, List<U>>>(delegate {
                O7Log.VerboseT(Tag, "Canceled updating social users");
                IsParsingUsers = false;

            }, delegate(Pair<List<U>, List<U>> users) {
                O7Log.VerboseT(Tag, "Finished updating social users");
                OnUsersUpdate(users);
                IsParsingUsers = false;

                EventBus.FireEvent(CommonEvents.SOCIAL_USERS_RELOADED);

            }, delegate(Exception e) {
                O7Log.VerboseT(Tag, "Error updating social users");
                IsParsingUsers = false;
            });

            IsParsingUsers = true;
            SocialUserWorker.PostUpdateUsers(usersJ, invitees, task);
        }

        protected virtual void OnUsersUpdate(Pair<List<U>, List<U>> users) {
            if (HasInvitees) {
                Friends = users.First;
            }
            Strangers = users.Second;
        }

        protected virtual void OnFriendsUpdate(object eventData) {
            if (!HasInvitees) {
                // No more friends, probably logged out. May be without network, so don't rely on cached users.
                // WARNING: Just a temporary solution. If app is restarted, friends will be loaded again from cache.
                Friends = null;
                EventBus.FireEvent(CommonEvents.SOCIAL_USERS_RELOADED);

            } else {
                // Download social users, because user may have logged-in or friends has changed
                SocialUserDownloader.StartDownload();
            }
        }

        protected virtual void OnPushReceive(object eventData) {
            JSONNode payloadJ = (JSONNode) eventData;

            // Refresh users from backend if "social=true"
            if (payloadJ["social"].AsBool) {
                O7Log.DebugT(Tag, "Downloading social users from backend due to 'social' request in push notification payload");
                SocialUserDownloader.StartDownload();
            }
        }

#region Users for Public

        public virtual List<U> Friends { get; protected set; }

        public virtual List<U> Strangers { get; protected set; }

        public virtual bool HasAnyUsers {
            get {
                return HasAnyFriends || HasAnyStrangers;
            }
        }

        public virtual bool HasAnyFriends {
            get {
                // Friends are cached - but prevent friends if not logged in
                return !CollectionUtils.IsEmpty(Friends) && HasInvitees;
            }
        }

        public virtual bool HasAnyStrangers {
            get {
                return !CollectionUtils.IsEmpty(Strangers);
            }
        }

        public virtual bool IsLoadingUsers {
            get {
                return SocialUserDownloader.IsDownloadInProgress || IsParsingUsers;
            }
        }

#endregion

#region Social Data

        public virtual BasicSocialData GatherBasicSocialData() {
            SocialFriend me = SocialNetwork.SocialHelper.User;
            if (me == null) return null;

            var friendIdsWithApp = SocialNetwork.SocialHelper.FriendsWithAppInstalledIdList;
            BasicSocialData data = new BasicSocialData(me.Id, me.FirstName, me.MiddleName, me.LastName,
                                       SocialNetwork.SocialHelper.SocialType, friendIdsWithApp);
            return data;
        }

#endregion
    }
}
