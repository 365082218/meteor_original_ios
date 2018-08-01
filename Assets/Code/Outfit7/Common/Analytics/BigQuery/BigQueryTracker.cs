//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Analytics.Tracking;
using Outfit7.Common;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Util;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Analytics.BigQuery {

    /// <summary>
    /// BigQuery event tracker.
    /// </summary>
    public class BigQueryTracker {

        protected const string Tag = "BigQueryTracker";
        protected bool IsDevel;
        protected bool IsRooted;
        protected bool IsPushedGrid;
        protected HashSet<string> ActiveGroupIds;

        public BigQueryPlugin BigQueryPlugin { get; set; }

        public EventBus EventBus { get; set; }

        public AppSession AppSession { get; set; }

        public GridManager GridManager { get; set; }

        public AnalyticsPlugin AnalyticsPlugin { get; set; }

        public bool IsDisabled { get; set; }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");

            IsDevel = BuildConfig.IsDevel;
            IsRooted = AppPlugin.Rooted;
            ActiveGroupIds = new HashSet<string>();

            if (GridManager.Ready) {
                OnGridLoad(GridManager.JsonData);
            }

            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, delegate(object eventData) {
                OnGridLoad((JSONNode) eventData);
            });
        }

        protected virtual void OnGridLoad(JSONNode gridData) {
            string reportingId = gridData["reportingId"];
            IsPushedGrid = gridData["rewardedPushRequest"].AsBool;

            ActiveGroupIds.Clear();
            JSONArray activeGroupIdsJ = SimpleJsonUtils.EnsureJsonArray(gridData["activeEventGroups"]);
            if (activeGroupIdsJ != null) {
                foreach (JSONNode agiJ in activeGroupIdsJ.Childs) {
                    string agi = agiJ.Value;
                    if (StringUtils.HasText(agi)) {
                        ActiveGroupIds.Add(agi);
                    }
                }
            }

            if (O7Log.DebugEnabled) {
                O7Log.DebugT(Tag, "Got reportingId={0}, pushedGrid={1}, activeGroupIds={2}",
                    reportingId, IsPushedGrid, StringUtils.CollectionToCommaDelimitedString(ActiveGroupIds));
            }
        }

#region Builder

        public virtual IBigQueryEventBuilder CreateBuilder(string groupId, string eventId, bool forceActive = false) {
            Assert.HasText(groupId, "groupId");
            Assert.HasText(eventId, "eventId");

            if (!forceActive && !IsGroupActive(groupId)) return DummyBigQueryEventBuilder.Instance;

            return new BigQueryEventBuilder(this)
                .SetGroupId(groupId)
                .SetEventId(eventId);
        }

        public virtual IBigQueryEventBuilder CreateCurrencyBuilder(string eventId, string itemId, string currencyId,
            int amount, int balance) {
            if (eventId == CommonTrackingEventParams.EventId.OfferInternal
                && itemId == CommonTrackingEventParams.InternalOfferItemId.Debug)
                return DummyBigQueryEventBuilder.Instance;

            return CreateBuilder(CommonTrackingEventParams.GroupId.Currency, eventId, true)
                .SetP1(currencyId)
                .SetP2(itemId)
                .SetP3(amount)
                .SetP4(balance);
        }

        // Must be run on Unity thread!
        public virtual void ConfigureBuilder(IBigQueryEventBuilder builder) {
            int network = 0;
            switch (Application.internetReachability) {
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    network = 1;
                    break;
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    network = 2;
                    break;
            }

            builder.SetNetwork(network);
            builder.SetRooted(IsRooted);
            builder.SetPushedGrid(IsPushedGrid);
            builder.SetTime(TimeUtils.CurrentTimeMillis);
            builder.SetTimeZoneOffset((int) TimeUtils.CurrentTimeZoneOffset.TotalMilliseconds);
            builder.SetSessionId(AppSession.SessionId);
        }

#endregion

        public virtual void AddEvent(string data) {
            if (IsDisabled) return;
            if (!AnalyticsPlugin.IsEnabled) return;
            Assert.HasText(data, "data");
            BigQueryPlugin.AddEvent(data);
        }

        public virtual bool IsGroupActive(string groupId) {
            if (IsDisabled) return false;
            if (!AnalyticsPlugin.IsEnabled) return false;
            if (IsDevel) return true;
            return ActiveGroupIds.Contains(groupId);
        }

        public virtual void SendEventsToBackend(bool forceNotEnough = false, bool forceNoGrid = true) {
            BigQueryPlugin.SendEventsToBackend(forceNotEnough, forceNoGrid);
        }
    }
}
