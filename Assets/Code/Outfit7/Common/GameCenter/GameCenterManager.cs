//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Analytics.Tracking;
using Outfit7.Common;
using Outfit7.Event;
using Outfit7.Util;

namespace Outfit7.GameCenter {

    /// <summary>
    /// Game center manager.
    /// </summary>
    public class GameCenterManager {

        protected const string Tag = "GameCenterManager";

        private const string UserTrackingDidLogInPref = "GameCenterManager.Tracking.DidLogIn";
        private const string UserTrackingDidLogOutPref = "GameCenterManager.Tracking.DidLogOut";
        private const string SignInsCountPref = "GameCenterManager.SignInsCount";

        private bool? available;
        private bool? signedIn;
        private int signInsCount = -1;
        private bool? isGameCenterAppInstalled;

        public EventBus EventBus { get; set; }

        public GameCenterPlugin GameCenterPlugin { get; set; }

        public AgeGateManager AgeGateManager  { get; set; }

        public TrackingManager TrackingManager { get; set; }

        public bool Available {
            get {
                if (this.available == null) {
                    this.available = GameCenterPlugin.Available;
                }
                return this.available.Value;
            }
        }

        public bool IsSignedIn {
            get {
                if (!Available)
                    return false;

                if (this.signedIn == null) {
                    this.signedIn = GameCenterPlugin.IsSignedIn;
                }
                return this.signedIn.Value;
            }
            internal set {
                if (this.signedIn == value)
                    return;

                this.signedIn = value;
                string eventPrefKey = value ? UserTrackingDidLogInPref : UserTrackingDidLogOutPref;

                if (!UserPrefs.HasKey(eventPrefKey)) {
                    UserPrefs.SetBool(eventPrefKey, true);
                    string eventId = value ? CommonTrackingEventParams.EventId.LogIn : CommonTrackingEventParams.EventId.LogOut;
                    TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.GameFeatures,
                        eventId, GameCenterPlugin.GamingPlatformName, null, null, null, null, null);

                    string eventPrefRemoveKey = !value ? UserTrackingDidLogInPref : UserTrackingDidLogOutPref;
                    UserPrefs.Remove(eventPrefRemoveKey);
                    UserPrefs.SaveDelayed();
                }
                EventBus.FireEvent(CommonEvents.GAME_CENTER_SIGN_IN, value);
            }
        }

        public virtual int SignInsCount {
            get {
                if (signInsCount == -1) {
                    signInsCount = UserPrefs.GetInt(SignInsCountPref, 0);
                }
                return signInsCount;
            }
            protected set {
                signInsCount = value;
                O7Log.VerboseT(Tag, "SignInsCount = {0}", signInsCount);
                UserPrefs.SetInt(SignInsCountPref, signInsCount);
                UserPrefs.SaveDelayed();
            }
        }

        public virtual bool IsGameCenterAppInstalled {
            get {
                if (isGameCenterAppInstalled == null) {
                    isGameCenterAppInstalled = GameCenterPlugin.IsGameCenterAppInstalled();
                }
                return isGameCenterAppInstalled.Value;
            }
        }

        public void SignIn() {
            if (!AgeGateManager.DidPass)
                return;

            if (IsSignedIn)
                return;

            GameCenterPlugin.SignIn();
            SignInsCount++;
        }

        public void SignOut() {
            if (!IsSignedIn)
                return;

            GameCenterPlugin.SignOut();
        }

        public string PlayerId {
            get {
                return GameCenterPlugin.PlayerId;
            }
        }

        public bool OpenApp() {
            return GameCenterPlugin.OpenApp();
        }

        public void OpenLeaderboard(GameCenterLeaderboard leaderboard) {
            GameCenterPlugin.StartOpeningLeaderboard(leaderboard.Number, leaderboard.Id);
        }

        public virtual void OnHiScoreUpdate(string jsonString) {
            // do nothing should be overriden
        }

        public void SubmitGameScore(GameCenterLeaderboard leaderboard, long score) {
            GameCenterPlugin.SubmitGameScore(leaderboard.Number, leaderboard.Id, score);
        }

        public virtual void OpenAchievements() {
            GameCenterPlugin.StartOpeningAchievements();
        }

        public virtual void UnlockAchievement(int id) {
            GameCenterPlugin.UnlockAchievement(id);
        }

        /// <summary>
        /// Increments the achievement to the set amount of steps or percent (percent is defined in [0.0, 100.0] range!).
        /// </summary>
        public virtual void IncrementAchievement(int id, int steps, float percent) {
            GameCenterPlugin.IncrementAchievement(id, steps, percent);
        }

        public virtual void ClearPrefs() {
            UserPrefs.Remove(UserTrackingDidLogInPref);
            UserPrefs.Remove(UserTrackingDidLogOutPref);
            UserPrefs.Remove(SignInsCountPref);
        }

    }
}
