//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using Outfit7.Analytics.Tracking;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Common {

    /// <summary>
    /// Advanced <see cref="AppSession"/> that is triggering session related BQ events &amp; GA hits.
    /// </summary>
    public class EventingAppSession : AppSession {

        private const string FirstInstallReportedPref = "AppSession.FirstInstall.Reported";

        public static new void ClearPrefs() {
            AppSession.ClearPrefs();
            UserPrefs.Remove(FirstInstallReportedPref);
        }

        public virtual bool? IsFirstInstall { get; private set; }

        public GridManager GridManager { get; set; }

        public TrackingManager TrackingManager { get; set; }

        protected override void AfterInit() {
            if (FirstStart) {
                AddNewSessionEvent(TimeSpan.Zero);

            } else {
                // Check for new session now, before other classes init and use old session
                OnAppResume();
            }

            if (GridManager.Ready) {
                OnGridLoad(GridManager.JsonData);
            }
            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, delegate(object data) {
                OnGridLoad((JSONNode) data);
            });

            O7Log.DebugT(Tag, "Init StartTime={0}, PauseTime={1}, SessionId={2}, PreviousSessionDuration={3}, FirstStartTime={4}, FirstStart={5}, IsFirstInstall={6}",
                StartTime.ToLocalTime(), PauseTime.ToLocalTime(), SessionId, PreviousSessionDuration, FirstStartTime.ToLocalTime(), FirstStart, IsFirstInstall);
        }

        protected override void CreateNewSession(DateTime toTime) {
            base.CreateNewSession(toTime);

            AddNewSessionEvent(PreviousSessionDuration);
        }

        protected virtual void AddNewSessionEvent(TimeSpan prevSessionDuration) {
            int ms = (int) prevSessionDuration.TotalMilliseconds;
            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.Session,
                CommonTrackingEventParams.EventId.NewSession, ms, null, null, null, null, null, null, true);
            TrackingManager.SendEventsToBackend(true);
        }

        protected virtual void OnGridLoad(JSONNode dataJ) {
            IsFirstInstall = dataJ["firstInstall"].AsBool;

            if (IsFirstInstall.Value) {
                TryAddFirstInstallEvent();
            }
        }

        protected virtual bool TryAddFirstInstallEvent() {
            if (UserPrefs.GetBool(FirstInstallReportedPref, false)) return false;

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.Session,
                CommonTrackingEventParams.EventId.FirstRun, null, null, null, null, null, null, true);
            TrackingManager.SendEventsToBackend(true);

            UserPrefs.SetBool(FirstInstallReportedPref, true);
            UserPrefs.SaveDelayed();

            return true;
        }
    }
}
