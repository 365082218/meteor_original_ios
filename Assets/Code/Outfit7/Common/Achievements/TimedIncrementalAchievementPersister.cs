//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

namespace Outfit7.Achievements {

    /// <summary>
    /// Timed incremental achievement persister - wrapper class.
    /// </summary>
    public class TimedIncrementalAchievementPersister : IncrementalAchievementPersister<TimedIncrementalAchievement> {

        private int startSteps = -1;

        public TimedIncrementalAchievementPersister(TimedIncrementalAchievement achievement) : base(achievement) {
        }

        public virtual bool CheckAndUpdateIncrement() {
            if (startSteps == -1) {
                startSteps = Achievement.CurrentSteps;
            }

            return Achievement.CheckAndUpdateIncrement();
        }

        public virtual bool StopIncrementingAndSave() {
            Achievement.StopIncrementing();

            bool updated = Achievement.CurrentSteps > startSteps;
            if (updated) {
                SaveAchievementData();
            }
            startSteps = -1;

            return updated;
        }
    }
}

