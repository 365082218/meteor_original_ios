//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using Outfit7.Event;

namespace Outfit7.Util {

    /// <summary>
    /// Application or game session. It overrides classic app pause/resume and cold start behavior,
    /// such that every pause that is longer than 3 minutes is considered like cold start
    /// and every cold start shorter than 3 minutes after exit is considered like resume.
    /// </summary>
    public class AppSession {

        protected const string Tag = "AppSession";
        private const string FirstStartTimePref = "GameState.FirstStartDateTime";
        private const string StartTimePref = "AppSession.StartTime";
        private const string PauseTimePref = "AppSession.PauseTime";
        private const string SessionIdPref = "AppSession.SessionId";
        protected static readonly TimeSpan SessionResetPauseDuration = TimeSpan.FromMinutes(3);

        public static void ClearPrefs() {
            UserPrefs.Remove(FirstStartTimePref);
            UserPrefs.Remove(StartTimePref);
            UserPrefs.Remove(PauseTimePref);
            UserPrefs.Remove(SessionIdPref);
        }

        protected DateTime StartTime;
        protected DateTime PauseTime;

        public virtual DateTime FirstStartTime { get; private set; }

        public virtual bool FirstStart { get; private set; }

        public virtual int SessionId { get; private set; }

        public virtual TimeSpan PreviousSessionDuration { get; private set; }

        public virtual TimeSpan PauseDuration { get; private set; }

        public EventBus EventBus { get; set; }

        public virtual void Init() {
            DateTime now = DateTime.UtcNow;
            FirstStartTime = UserPrefs.GetDateTime(FirstStartTimePref, DateTime.MinValue);
            if (FirstStartTime == DateTime.MinValue) {
                FirstStartTime = now;
                UserPrefs.SetDateTime(FirstStartTimePref, FirstStartTime);
                UserPrefs.Save();
                FirstStart = true;
            }
            StartTime = UserPrefs.GetDateTime(StartTimePref, now);
            PauseTime = UserPrefs.GetDateTime(PauseTimePref, now);
            SessionId = UserPrefs.GetInt(SessionIdPref, 0);

            AfterInit();
        }

        protected virtual void AfterInit() {
            // Check for new session now, before other classes init and use old session
            OnAppResume();

            O7Log.DebugT(Tag, "Init StartTime={0}, PauseTime={1}, SessionId={2}, PreviousSessionDuration={3}, FirstStartTime={4}, FirstStart={5}",
                StartTime.ToLocalTime(), PauseTime.ToLocalTime(), SessionId, PreviousSessionDuration, FirstStartTime.ToLocalTime(), FirstStart);
        }

        public virtual void OnAppResume() {
            // Reset as new session after some time in pause
            DateTime now = DateTime.UtcNow;
            if (PauseTime > now) {
                O7Log.WarnT(Tag, "Last pause time is in future! PauseTime={0}, now={1}", PauseTime, now);
                return;
            }
            if (now > PauseTime + SessionResetPauseDuration) {
                CreateNewSession(PauseTime);
                FireNewSessionEvent();
            }
        }

        public virtual void OnAppPause() {
            // Be sure to save StartTime if app is exited while in pause, because StartTime may not be persisted yet
            // If you do this in Init and app crashes before pause, than PauseTime == now and StartTime == FirstStartTime,
            // the difference could be enormous and would infact consider dead time as legit session time.
            UserPrefs.SetDateTime(StartTimePref, StartTime);
            PauseTime = DateTime.UtcNow;
            UserPrefs.SetDateTime(PauseTimePref, PauseTime);
        }

        public virtual void ForceNewSession(bool fireEvent) {
            CreateNewSession(DateTime.UtcNow);
            if (fireEvent) {
                FireNewSessionEvent();
            }
        }

        protected virtual void CreateNewSession(DateTime toTime) {
            DateTime now = DateTime.UtcNow;
            PreviousSessionDuration = toTime - StartTime;
            PauseDuration = now - PauseTime;
            if (PreviousSessionDuration.Milliseconds < 0) {
                O7Log.WarnT(Tag, "PreviousSessionDuration is negative ({0}), user manually changed time back", PreviousSessionDuration);
            }

            O7Log.DebugT(Tag, "New session #{0}, StartTime={1}, PauseTime={2}, PreviousSessionDuration={3}, PauseDuration {4}",
                SessionId + 1, StartTime.ToLocalTime(), PauseTime.ToLocalTime(), PreviousSessionDuration, PauseDuration);

            StartTime = now;
            PauseTime = StartTime;
            SessionId++;
            FirstStart = false;

            UserPrefs.SetDateTime(StartTimePref, StartTime);
            UserPrefs.SetDateTime(PauseTimePref, PauseTime);
            UserPrefs.SetInt(SessionIdPref, SessionId);
            UserPrefs.SaveDelayed();
        }

        protected virtual void FireNewSessionEvent() {
            EventBus.FireEvent(CommonEvents.NEW_SESSION, PreviousSessionDuration);
        }
    }
}
