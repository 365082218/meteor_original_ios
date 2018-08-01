//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System;
using Outfit7.Util;

namespace Outfit7.Achievements {

    // TODO: should be moved to own class/file?
    public class TimedAchievementData {

        public DateTime StartTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public TimeSpan Duration { get; set; }

        public TimedAchievementData(TimeSpan duration) {
            Duration = duration;
            StartTime = DateTime.MinValue;
            LastUpdateTime = DateTime.MinValue;
        }

    }

    public class DaysInARowAchievement : Achievement<TimedAchievementData> {

        public DaysInARowAchievement(int id, string name, TimeSpan duration) : base(id, name) {
            Data = new TimedAchievementData(duration);
        }

        /// <summary>
        /// Checks and updates the achievement data. Also unlocks the achievement if condition met.
        /// </summary>
        /// <returns><c>true</c>, if achievement data was updated, <c>false</c> otherwise.</returns>
        public virtual bool CheckAndUpdateAchievementData() {
            /*
             * Local time has to be used here because this kind of achievements have to be updated every day and a day
             * is defined from 0:00 to 23:59 in LOCAL time!
             */
            DateTime now = DateTime.Now; // this has to be local time

            int daysSinceLastUpdate = (now - Data.LastUpdateTime.ToLocalTime()).Days;
            if (daysSinceLastUpdate > 1) {
                // if more than 1 day since achievement last checked/updated, achievement failed and you have to start from the beginning
                O7Log.VerboseT(Tag, "'{0}' last updated more than a day ago --> resetting achievement progress: daysSinceLastUpdate = {1}", Name, daysSinceLastUpdate);
                RestartAchievement();
                return true;
            }
            if (daysSinceLastUpdate < 0) {
                O7Log.WarnT(Tag, "'{0}' LastUpdateTime in the future --> resetting achievement progress: daysSinceLastUpdate = {1}", Name, daysSinceLastUpdate);
                Data.LastUpdateTime = DateTime.UtcNow; // this has to be UTC time
                return true;
            }

            int daysSinceAchievementStart = (now - Data.StartTime.ToLocalTime()).Days;
            if (daysSinceAchievementStart < 0) {
                O7Log.WarnT(Tag, "'{0}' StartTime in the future --> resetting achievement progress: daysSinceLastUpdate = {1}", Name, daysSinceAchievementStart);
                RestartAchievement();
                return true;
            }

            bool updated = false;
            if (daysSinceLastUpdate == 1) {
                Data.LastUpdateTime = DateTime.UtcNow; // this has to be UTC time
                updated = true;
            }
            O7Log.VerboseT(Tag, "'{0}': updated = {1}, daysSinceLastUpdate = {2}, daysSinceAchievementStart = {3}", Name, updated, daysSinceLastUpdate, daysSinceAchievementStart);

            // unlock if achievement completion condition met
            if (daysSinceAchievementStart >= Data.Duration.Days) {
                updated = Unlock();
            }

            return updated;
        }

        /// <summary>
        /// Restarts the achievement progress (like it has just started) and sets the StartTime to <c>UtcNow</c>.
        /// </summary>
        public virtual void RestartAchievement() {
            O7Log.VerboseT(Tag, "Restarting achievement: {0}", this);
            Data.StartTime = DateTime.UtcNow;
            Data.LastUpdateTime = Data.StartTime;
        }

        /// <summary>
        /// Resets the achievement progress (like it was never started) and sets the StartTime to <c>DateTime.MinValue</c>.
        /// </summary>
        public virtual void ResetAchievement() {
            O7Log.VerboseT(Tag, "Resetting achievement: {0}", this);
            Data.StartTime = DateTime.MinValue;
            Data.LastUpdateTime = Data.StartTime;
        }

        public override string ToString() {
            return string.Format("{0}; StartTime = {1}, LastUpdateTime = {2}", base.ToString(), Data.StartTime, Data.LastUpdateTime);
        }
    }
}

