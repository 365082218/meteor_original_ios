//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Analytics.BigQuery;
using Outfit7.Grid;
using Outfit7.Util;

namespace Outfit7.Analytics.Tracking {

    /// <summary>
    /// Event tracking manager.
    /// </summary>
    public class TrackingManager {

        private const string Tag = "TrackingManager";
        private const int MinPendingEventsToSend = 10;

        public GridManager GridManager { get; set; }

        public TrackingWorker TrackingWorker { get; set; }

        public BigQueryTracker BigQueryTracker { get; set; }

        public void AddCurrencyEvent(string eventId, string itemId, string currencyId, int amount, int balance) {
            BigQueryTracker.CreateCurrencyBuilder(eventId, itemId, currencyId, amount, balance).Add();
        }

        public void AddEvent(string groupId, string eventId, string param1, string param2, long? param3, long? param4,
            string param5, string customData) {
            AddEvent(groupId, eventId, null, param1, param2, param3, param4, param5, customData);
        }

        public void AddEvent(string groupId, string eventId, string param1, string param2, long? param3, long? param4,
            string param5, string customData, bool forceActive) {
            AddEvent(groupId, eventId, null, param1, param2, param3, param4, param5, customData, forceActive);
        }

        public void AddEvent(string groupId, string eventId, int? elapsedTime, string param1, string param2,
            long? param3, long? param4, string param5, string customData) {
            AddEvent(groupId, eventId, elapsedTime, param1, param2, param3, param4, param5, customData, false);
        }

        public void AddEvent(string groupId, string eventId, int? elapsedTime, string param1, string param2,
            long? param3, long? param4, string param5, string customData, bool forceActive) {
            var b = BigQueryTracker.CreateBuilder(groupId, eventId, forceActive);
            if (elapsedTime != null) {
                b.SetElapsedTime(elapsedTime.Value);
            }
            b.SetP1(param1);
            b.SetP2(param2);
            if (param3 != null) {
                b.SetP3(param3.Value);
            }
            if (param4 != null) {
                b.SetP4(param4.Value);
            }
            b.SetP5(param5);
            b.SetCustomData(customData);
            b.Add();
        }

        public bool IsGroupActive(string groupId) {
            return BigQueryTracker.IsGroupActive(groupId);
        }

        public bool SendEventsToBackend() {
            return SendEventsToBackend(false);
        }

        public bool SendEventsToBackend(bool force) {
            BigQueryTracker.SendEventsToBackend(force);

            // To allow new apps without TrackingWorker
            if (TrackingWorker == null) return true;

            if (!GridManager.Ready) {
                O7Log.WarnT(Tag, "Won't send events to backend. No GRID data");
                return false;
            }
            if (!force) {
                int count = TrackingWorker.PendingEventCount;
                if (count < MinPendingEventsToSend) {
                    O7Log.DebugT(Tag, "Won't send events to backend. Not enough events: {0}/{1}", count,
                        MinPendingEventsToSend);
                    return false;
                }
            }

            string reportingId = GridManager.JsonData["reportingId"];
            TrackingWorker.PostSendEvents(reportingId);

            return true;
        }
    }
}
