//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Analytics.Tracking;
using Outfit7.GameCenter;
using Outfit7.Util;

namespace Outfit7.Achievements {

    public abstract class AchievementManager {

        public const string Tag = "AchievementManager";

        public GameCenterManager GameCenterManager { get ; set; }

        public TrackingManager TrackingManager { get; set; }

        public virtual bool AreAchievementsSupported {
            get {
#if UNITY_EDITOR || UNITY_IPHONE || (UNITY_ANDROID && !ANDROID_360 && !UNITY_BEMOBI)
                return GameCenterManager.Available;
#else
                return false;
#endif
            }
        }

        public abstract void Init();

        public abstract void ClearPrefs();

        public virtual void OpenAchievements() {
            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.GameFeatures, CommonTrackingEventParams.EventId.AchievementsView,
                null, null, null, null, null, null, true);
            GameCenterManager.OpenAchievements();
        }

        public virtual bool UnlockAchievement<T>(AchievementPersister<T> persister) where T : Achievement {
            if (!CheckSupport(persister.Achievement)) {
                return false;
            }

            bool updated = persister.CheckUnlockAndSave();
            if (updated) {
                CheckAndUnlockAchievment(persister.Achievement);
            }
            return updated;
        }

        public virtual bool IncrementStepsAndSend(IncrementalAchievementPersister persister) {
            return IncrementStepsAndSend(persister, 1);
        }

        /// <summary>
        /// Increments achievement steps and then saves and sends data if CurrentSteps increased.
        /// </summary>
        /// <returns><c>true</c>, if steps updated and sent, <c>false</c> otherwise.</returns>
        /// <param name="persister">Persister.</param>
        /// <param name="incSteps">Increment by this many steps.</param>
        public virtual bool IncrementStepsAndSend(IncrementalAchievementPersister persister, int incSteps) {
            if (!CheckSupport(persister.Achievement)) {
                return false;
            }

            bool updated = persister.IncCurrentStepsAndSave(incSteps);
            if (updated) {
                CheckAndIncrementAchievement(persister);
            }
            return updated;
        }

        /// <summary>
        /// Sets increments achievement steps and then saves and sends data if CurrentSteps increased.
        /// </summary>
        /// <returns><c>true</c>, if steps updated and sent, <c>false</c> otherwise.</returns>
        /// <param name="persister">Persister.</param>
        /// <param name="steps">Increment to this many steps.</param>
        public virtual bool SetIncrementStepsAndSend(IncrementalAchievementPersister persister, int steps) {
            if (!CheckSupport(persister.Achievement)) {
                return false;
            }

            bool updated = persister.SetCurrentStepsAndSave(steps);
            if (updated) {
                CheckAndIncrementAchievement(persister);
            }
            return updated;
        }

        /// <summary>
        /// Stops incrementing and sends timed achievement if number of steps actually increased (in given time).
        /// </summary>
        /// <returns><c>true</c>, if steps updated and sent, <c>false</c> otherwise.</returns>
        /// <param name="persister">Persister.</param>
        public virtual bool StopIncrementingAndSendTimedAchievement(TimedIncrementalAchievementPersister persister) {
            if (!CheckSupport(persister.Achievement)) {
                persister.Achievement.StopIncrementing();
                return false;
            }

            bool updated = persister.StopIncrementingAndSave();
            if (updated) {
                CheckAndIncrementAchievement(persister);
            }
            return updated;
        }

        /// <summary>
        /// Checks, updates, saves data and unlocks (if conditions met) "days in a row" achievement if data has changed.
        /// </summary>
        /// <returns><c>true</c>, if data was updated and saved, <c>false</c> otherwise.</returns>
        /// <param name="persister">Persister.</param>
        public virtual bool CheckAndUpdateDaysInARowAchievement(DaysInARowAchievementPersister persister) {
            if (!CheckSupport(persister.Achievement)) {
                return false;
            }

            bool updated = persister.CheckUpdateAndSaveAchievementData();
            if (updated) {
                CheckAndUnlockAchievment(persister.Achievement);
            }
            return updated;
        }

        /// <summary>
        /// Tries to add an item to collection and increment or unlock the achievement.
        /// </summary>
        /// <returns><c>true</c>, if item added to collection, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        /// <param name="persister">Persister.</param>
        public virtual bool AddToCollectionAndSend(string item, CollectionAchievementPersister persister) {
            if (!CheckSupport(persister.Achievement)) {
                return false;
            }

            bool updated = persister.CheckAndAddItem(item);
            if (updated) {
                bool unlocked = CheckAndPostUnlockEvent(persister.Achievement);

                if (CheckSignIn(persister.Achievement)) {
                    if (!persister.Achievement.IsIncremental && unlocked) {
                        O7Log.DebugT(Tag, "Unlocked collection achievement: {0}", persister.Achievement);
                        GameCenterManager.UnlockAchievement(persister.Achievement.Id);
                    } else {
                        O7Log.DebugT(Tag, "Increment collection achievement: {0}", persister.Achievement);
                        GameCenterManager.IncrementAchievement(persister.Achievement.Id, persister.Achievement.ItemsCount, persister.Achievement.ProgressInPercent);
                    }
                }
            }

            return updated;
        }

        private bool CheckAndPostUnlockEvent(Achievement achievement) {
            if (achievement.IsUnlocked) {
                TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.GameFeatures, CommonTrackingEventParams.EventId.AchievementsWon,
                    null, achievement.Name, null, null, null, null, true);
                return true;
            }

            O7Log.DebugT(Tag, "Achievement not (yet?) unlocked: {0}", achievement);
            return false;
        }

        private bool CheckSupport(Achievement achievement) {
            if (!AreAchievementsSupported) {
                O7Log.DebugT(Tag, "Achievements not supported! {0}", achievement);
                return false;
            }

            return true;
        }

        private bool CheckSignIn(Achievement achievement) {
            if (!GameCenterManager.IsSignedIn) {
                O7Log.WarnT(Tag, "Can't unlock/increment achievement - not signed-in: {0}", achievement);
                return false;
            }

            return true;
        }

        private bool CheckAndUnlockAchievment(Achievement achievement) {
            bool unlocked = CheckAndPostUnlockEvent(achievement);
            if (!unlocked) {
                return false;
            }

            if (CheckSignIn(achievement)) {
                GameCenterManager.UnlockAchievement(achievement.Id);
                return true;
            }

            return false;
        }

        private bool CheckAndIncrementAchievement<T>(AchievementPersister<T> persister) where T : IncrementalAchievement {
            CheckAndPostUnlockEvent(persister.Achievement);
            // no need to check if achievement was actually unlocked since we want to report achievment progress regardles of unlock state

            if (CheckSignIn(persister.Achievement)) {
                O7Log.DebugT(Tag, "Increment achievement: {0}", persister.Achievement);
                GameCenterManager.IncrementAchievement(persister.Achievement.Id, persister.Achievement.CurrentSteps, persister.Achievement.CurrentProgressInPercent);
                return true;
            }

            return false;
        }

        public virtual void CheckAchievementProgress<T>(AchievementPersister<T> persister) where T : Achievement {
            // if achievement is an incremental one, just report its current steps/progress
            IncrementalAchievement incrementalAchievement = persister.Achievement as IncrementalAchievement;
            if (incrementalAchievement != null && incrementalAchievement.CurrentSteps > 0) {
                GameCenterManager.IncrementAchievement(incrementalAchievement.Id, incrementalAchievement.CurrentSteps, incrementalAchievement.CurrentProgressInPercent);
                return;
            }

            // if achievement is an incremental collection one, just report its current steps/progress
            CollectionAchievement collectionAchievement = persister.Achievement as CollectionAchievement;
            // non-incremental collection achievemnts should be treated as simple achievements and should be checked later down in this method
            if (collectionAchievement != null && collectionAchievement.IsIncremental && collectionAchievement.ItemsCount > 0) {
                GameCenterManager.IncrementAchievement(collectionAchievement.Id, collectionAchievement.ItemsCount, collectionAchievement.ProgressInPercent);
                return;
            }

            // if non-incremental achievement and unlocked, try to unlock achievement on GC/GPG
            if (persister.Achievement.IsUnlocked) {
                GameCenterManager.UnlockAchievement(persister.Achievement.Id);
                return;
            }

            O7Log.DebugT(Tag, "CheckAchievementProgress: Nothing to do with achievement = {0}", persister.Achievement);
        }

    }
}

