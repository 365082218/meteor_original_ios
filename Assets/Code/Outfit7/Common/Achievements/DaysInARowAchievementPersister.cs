//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using SimpleJSON;

namespace Outfit7.Achievements {

    public class DaysInARowAchievementPersister : AchievementPersister<DaysInARowAchievement> {

        public const string JsonStartTime = "StartTime";
        public const string JsonLastUpdateTime = "LastUpdateTime";

        public DaysInARowAchievementPersister(DaysInARowAchievement achievement) : base(achievement) {
        }

        public override JSONClass SerializeAchievementData(JSONClass json) {
            json[JsonStartTime].AsDateTime = Achievement.Data.StartTime;
            json[JsonLastUpdateTime].AsDateTime = Achievement.Data.LastUpdateTime;
            return base.SerializeAchievementData(json);
        }

        public override void DeserializeAchievementData(JSONNode json) {
            Achievement.Data.StartTime = json[JsonStartTime].AsDateTime;
            Achievement.Data.LastUpdateTime = json[JsonLastUpdateTime].AsDateTime;
            base.DeserializeAchievementData(json);
        }

        /// <summary>
        /// Checks and updates achievement data. If data has changed it's saved as well.
        /// </summary>
        /// <returns><c>true</c>, if (updated) achievement data was saved, <c>false</c> otherwise.</returns>
        public virtual bool CheckUpdateAndSaveAchievementData() {
            if (Achievement.CheckAndUpdateAchievementData()) {
                SaveAchievementData();
                return true;
            }

            return false;
        }

    }
}

