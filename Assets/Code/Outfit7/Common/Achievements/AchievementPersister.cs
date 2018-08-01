//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Achievements {

    public abstract class AchievementPersister<T> where T : Achievement {

        private const string JsonUnlocked = "Unlocked";

        public T Achievement { get; protected set; }

        protected virtual string Tag { get { return GetType().Name; } }

        protected virtual string PrefKey { get { return "Achievements.Achievement." + Achievement.Id; } }

        public void SaveAchievementData() {
            JSONClass json = new JSONClass();
            SerializeAchievementData(json);
            UserPrefs.SetJson(PrefKey, json);
            UserPrefs.SaveDelayed();
        }

        /// <summary>
        /// Loads the achievement data.
        /// </summary>
        /// <returns><c>true</c>, if achievement data was loaded, <c>false</c> otherwise.</returns>
        public bool LoadAchievementData() {
            JSONNode json = UserPrefs.GetJson(PrefKey, null);
            if (json == null) {
                return false;
            }
            DeserializeAchievementData(json);
            return true;
        }

        /// <summary>
        /// Checks and unlocks the achievement. If unlocked, it saves the data as well.
        /// </summary>
        /// <returns><c>true</c>, if data was saved, <c>false</c> otherwise.</returns>
        public virtual bool CheckUnlockAndSave() {
            if (Achievement.Unlock()) {
                SaveAchievementData();
                return true;
            }

            return false;
        }

        public virtual JSONClass SerializeAchievementData(JSONClass json) {
            json[JsonUnlocked].AsBool = Achievement.IsUnlocked;
            return json;
        }

        public virtual void DeserializeAchievementData(JSONNode json) {
            Achievement.IsUnlocked = json[JsonUnlocked].AsBool;
        }

        protected AchievementPersister(T achievement) {
            Assert.NotNull(achievement, "Every AchievementPersister must have it's own achievement as well!");
            Achievement = achievement;
        }

        /// <summary>
        /// Clears the achievement data. Assumes achievment data was saved in UserPrefs with PrefKey by default!
        /// </summary>
        public virtual void ClearAchievementData() {
            UserPrefs.Remove(PrefKey);
        }

    }
}

