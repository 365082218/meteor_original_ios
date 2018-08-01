//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Analytics.Tracking;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Purchase;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Bee7 {
    public class PublisherHelper {

        private readonly string Tag = "PublisherHelper";

        public bool Enabled { get; private set; }

        public bool IsGameWallAvailable { get; private set; }

        public List<AppOffer> AppOffers { get; set; }

        public EventBus EventBus { get; set; }

        public PublisherPlugin PublisherPlugin { get; set; }

        public GridManager GridManager { get; set; }

        public AbstractPurchaseManager PurchaseManager { get; set; }

        public TrackingManager TrackingManager { get; set; }

        public Action<string> OnGameSelectAction;

        public Action OnCloseGameWall;

        public bool NotificationReady { get; private set; }

        public delegate void GameWallAvailableDelegate(bool available);

        public static event GameWallAvailableDelegate GameWallAvailabilityChanged;

        protected virtual int CurrentBallance { get { return 0; } }

        protected virtual bool ShowReward { get { return true; } }

        protected int LowBalanceLimit { get; set; }

        protected List<string> PendingRewards = new List<string>();

        private int LastReportedBalance = int.MinValue;
        private int AppListLoadingInProgressCount;

        private class AppOffersData {

            public List<AppOffer> AppOffers { get; set; }

            public AppOffersData(List<AppOffer> appOffers) {
                AppOffers = appOffers;
            }
        }

        public virtual void Init() {

            O7Log.DebugT(Tag, "Init");
            Enabled = false;

            OnInit();

            if (GridManager.Ready) {
                OnGridLoad(GridManager.JsonData);
            }

            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, delegate(object eventData) {
                OnGridLoad((JSONNode) eventData);
            });

            PublisherPlugin.ForceGameWallFallback(false); //TODO: to be removed in near future
        }

        public virtual void OnInit() {

        }

        private void OnGridLoad(JSONNode gridData) {
            string reportingId = gridData["reportingId"];
            if (!StringUtils.IsNullOrEmpty(reportingId)) {
                PublisherPlugin.SetTestVariant(reportingId);
            }

            JSONNode bee7J = gridData["ext"]["bee7"];
            if (bee7J != null) {
                JSONNode lowBalanceJ = bee7J["lowBalance"];
                if (lowBalanceJ != null) {
                    LowBalanceLimit = lowBalanceJ.AsInt;
                }
            }
        }

        public void OnEnabledChanged(bool enabled) {

            O7Log.DebugT(Tag, "OnEnableChanged {0}", enabled);
            Enabled = enabled;

            GetAppList();
        }

        public void OnAppListChanged() {

            O7Log.DebugT(Tag, "UpdateAppList");
            GetAppList();
        }

        public bool IsRewardPending {
            get {
                return PendingRewards.Count > 0;
            }
        }

        internal void OnRewardClaim(string data, bool instantReward) {

            JSONNode dataJ = JSON.Parse(data);
            JSONArray rewardArr = SimpleJsonUtils.EnsureJsonArray(dataJ["rewards"]);
            if (rewardArr == null) {
                return;
            }
            foreach (JSONNode rewardJ in rewardArr) {

                if (!ShowReward && !instantReward) {
                    PendingRewards.Add(rewardJ.ToString());
                }

                Reward reward = new Reward(rewardJ);
                PurchaseManager.RewardBee7App(reward, ShowReward && !instantReward);
            }
        }

        public void ShowPendingReward() {
            string data = PendingRewards[0];
            PendingRewards.RemoveAt(0);
            PublisherPlugin.ShowReward(data);
        }

        private void UpdateAppList(AppOffersData data) {

            O7Log.DebugT(Tag, "UpdateAppList {0}", data);
            AppOffers = data.AppOffers;
            EventBus.FireEvent(CommonEvents.BEE7_PUBLISHER_APP_OFFERS_UPDATE);
        }

        public void StartOffer(string appId) {
            Assert.HasText(appId, "appId");
            O7Log.DebugT(Tag, "StartOffer {0}", appId);
            PublisherPlugin.StartOffer(appId);
        }

        public void GetAppList() {

            O7Log.DebugT(Tag, "GetAppList");
            AppListLoadingInProgressCount++;
            PublisherPlugin.GetAllAppOffers();
        }

        public void OnAppResume() {

            if (!Enabled) {
                return;
            }
            GetPendingRewards();
        }

        private void GetPendingRewards() {
            O7Log.DebugT(Tag, "GetPendingRewards");
            PublisherPlugin.GetPendingRewards();
        }

        internal void GetAppListCompleted(string data) {

            O7Log.DebugT(Tag, "GetAppListCompleted {0}", data);

            if (!Enabled) {
                O7Log.DebugT(Tag, "GetAppListCompleted not enabled in task count {0}", AppListLoadingInProgressCount);
                AppListLoadingInProgressCount--;
                return;
            }

            O7Log.DebugT(Tag, "GetAppListCompleted all app list '{0}'", data);

            if (data.Length == 0) { // empty nothing to do
                O7Log.DebugT(Tag, "GetAppListCompleted data len 0 task count {0}", AppListLoadingInProgressCount);
                AppListLoadingInProgressCount--;
                return;
            }

            TaskFeedback<AppOffersData> FriendsUsingAppListCompletedFeedback = new TaskFeedback<AppOffersData>(
                                                                                   delegate { // cancel

                    O7Log.DebugT(Tag, "GetAppListCompleted canceled task count {0}", AppListLoadingInProgressCount);
                    AppListLoadingInProgressCount--;

                }, delegate(AppOffersData friendsData) { // finish

                O7Log.DebugT(Tag, "GetAppListCompleted got publishers task count {0}", AppListLoadingInProgressCount);
                if (Enabled) {
                    UpdateAppList(friendsData);
                }
                AppListLoadingInProgressCount--;

            }, delegate(Exception e) { // error
                O7Log.WarnT(Tag, e, "FriendsUsingAppListCompletedFeedback exception {0}", AppListLoadingInProgressCount);
                AppListLoadingInProgressCount--;
            });

            TaskFeedback<AppOffersData> executorFeedback = new ExecutorTaskFeedbackWrapper<AppOffersData>(FriendsUsingAppListCompletedFeedback);

            Action runner = delegate() {
                try {
                    JSONNode json = JSON.Parse(data);
                    JSONArray appOffersJson = SimpleJsonUtils.EnsureJsonArray(json);

                    List<AppOffer> appOffers = new List<AppOffer>(appOffersJson.Count);
                    foreach (JSONNode child in appOffersJson) {
                        AppOffer appOffer = new AppOffer(child);
                        appOffers.Add(appOffer);
                    }

                    AppOffersData appOffersData = new AppOffersData(appOffers);
                    executorFeedback.OnFinish(appOffersData);

                } catch (Exception e) {
                    O7Log.ErrorT(Tag, "cannot parse appOffers json {0}", e);
                    executorFeedback.OnError(e);
                }
            };
            SimpleWorker.RunAsync(runner);
        }

        public void OnGameWallButtonImpression() {
            PublisherPlugin.OnGameWallButtonImpression();
        }

        public void OnGameWallImpression() {
            PublisherPlugin.OnGameWallImpression();
        }

        public void OnGameWallCloseImpression() {
            PublisherPlugin.OnGameWallCloseImpression();
        }

        /// <summary>
        /// OnAppOffersImpression
        /// </summary>
        /// <param name="pair">offer id, shown index</param>
        public void OnAppOffersImpression(Pair<string,int> pair) {

            if (pair == null) {
                return;
            }

            PublisherPlugin.OnAppOffersImpression(pair.First + "," + pair.Second);
        }

        public void OnTrackingInfoReceived(string jsonString) {
            string reportingId;
            long configTimestamp;
            try {
                JSONNode trackingJ = JSON.Parse(jsonString);

                reportingId = trackingJ["reportingId"];
                configTimestamp = trackingJ["configTimestamp"].AsLong;

            } catch {
                O7Log.WarnT(Tag, "Cannot parse tracking info: {0}", jsonString);
                return;
            }

            if (!StringUtils.HasText(reportingId)) {
                O7Log.WarnT(Tag, "Empty tracking info's reporting ID: {0}", jsonString);
                return;
            }

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.ExternalReporting,
                CommonTrackingEventParams.EventId.Bee7PublisherReporting, null, null, reportingId, configTimestamp,
                null, null, null, true);
        }

        public void UpdateMinigames(string jsonString) {
            PublisherPlugin.UpdateMinigames(jsonString);
        }

        public void OpenMinigame(string gameId) {

            if (OnGameSelectAction != null) {
                OnGameSelectAction(gameId);
            }
        }

        public void CloseGameWall() {
            if (OnCloseGameWall != null) {
                OnCloseGameWall();
            }
        }

        public virtual void OnGameWallShowRequest() {
            // user clicked on notification -> open gamewall
        }

        public void OnBannerNotificationShowRequest() {
            NotificationReady = true;
        }

        public virtual void OnBannerNotificationClick() {
            // user clicked on notification -> hide notification
        }

        public virtual void OnBannerNotificationVisibilityChanged(bool visible) {
            // notification did show/hide
        }

        public void GameWallAvailable(bool available) {
            IsGameWallAvailable = available;
            if (GameWallAvailabilityChanged != null) {
                GameWallAvailabilityChanged(IsGameWallAvailable);
            }
        }

        public void ToggleNotificationShowing(bool show) {
            if (show) {
                UpdateBallance();
            } else {
                NotificationReady = false;
            }
            PublisherPlugin.ToggleNotificationShowing(show);
        }

        public virtual void UpdateBallance() {

            if (LastReportedBalance == CurrentBallance) {
                return;
            }
            LastReportedBalance = CurrentBallance;

            PublisherPlugin.SetVirtualCurrencyState(CurrentBallance < LowBalanceLimit);
        }

        public void ShowBannerNotification() {
            NotificationReady = false;
            PublisherPlugin.ShowBannerNotification();
        }

        public virtual void CloseBannerNotification() {
            // only if you want to forcefull close notification
            NotificationReady = false;
            PublisherPlugin.CloseBannerNotification();
        }

        protected enum OfferType {
            ANY = 0,
            // - any type of offer
            XPROMO = 1,
            // - cross promo
            OTHER = 2,
            // - 3rd party offers
            NONE = 3
            // no offers, just mini games
        }

        protected virtual OfferType PaidUserOfferType {
            get {
                return OfferType.XPROMO;
            }
        }

        protected virtual OfferType FreeUserOfferType {
            get {
                return OfferType.ANY;
            }
        }

        protected void SetOfferTypes(bool isPaidUser) {

            int offerType = (int) (isPaidUser ? PaidUserOfferType : FreeUserOfferType);

            PublisherPlugin.SetOfferTypes(offerType);
        }

        protected void SetRewardFactor(float factor) {
            PublisherPlugin.SetRewardFactor(factor);
        }

        public void ScheduleReengagementNotifications() {
            PublisherPlugin.ScheduleReengagementNotifications();
        }

        public void ShowGameWallWithNotifications(string notificationsJson) {
            PublisherPlugin.ShowGameWallWithNotifications(notificationsJson);
        }
    }
}
