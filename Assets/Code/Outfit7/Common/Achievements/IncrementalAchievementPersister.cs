//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using SimpleJSON;
using Outfit7.Util;

namespace Outfit7.Achievements {

    public class IncrementalAchievementPersister<T> : AchievementPersister<T> where T : IncrementalAchievement {

        private const string JsonCurrentSteps = "CurrentSteps";

        public IncrementalAchievementPersister(T achievement) : base(achievement) {
        }

        public override JSONClass SerializeAchievementData(JSONClass json) {
            json[JsonCurrentSteps].AsInt = Achievement.CurrentSteps;
            return base.SerializeAchievementData(json);
        }

        public override void DeserializeAchievementData(JSONNode json) {
            Achievement.IncCurrentSteps(json[JsonCurrentSteps].AsInt);
            base.DeserializeAchievementData(json);
        }

        /// <summary>
        /// Increments the current steps by 1 and saves.
        /// </summary>
        /// <returns><c>true</c>, if new progress was saved, <c>false</c> otherwise.</returns>
        /// <param name="bySteps">By steps.</param>
        public virtual bool IncCurrentStepsAndSave() {
            return IncCurrentStepsAndSave(1);
        }

        /// <summary>
        /// Increments the current steps and saves.
        /// </summary>
        /// <returns><c>true</c>, if new progress was saved, <c>false</c> otherwise.</returns>
        /// <param name="bySteps">By steps.</param>
        public virtual bool IncCurrentStepsAndSave(int bySteps) {
            int currentSteps = Achievement.CurrentSteps;
            int newSteps = Achievement.IncCurrentSteps(bySteps);

            if (currentSteps != newSteps) {
                SaveAchievementData();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the current steps if greater than current steps and saves.
        /// </summary>
        /// <returns><c>true</c>, if new progress was saved, <c>false</c> otherwise.</returns>
        /// <param name="steps">By steps.</param>
        public virtual bool SetCurrentStepsAndSave(int steps) {
            int currentSteps = Achievement.CurrentSteps;
            if (currentSteps >= steps) {
                O7Log.DebugT(Tag, "Not incrementing '{0}', steps = {1}, CurrentSteps = {2}", Achievement.Name, steps, currentSteps);
                return false;
            }

            return IncCurrentStepsAndSave(steps - currentSteps);
        }
    }

    public class IncrementalAchievementPersister : IncrementalAchievementPersister<IncrementalAchievement> {

        public IncrementalAchievementPersister(IncrementalAchievement achievement) : base(achievement) {
        }

    }

}

