//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Json;
using SimpleJSON;

namespace Outfit7.Achievements {

    public class CollectionAchievementPersister : AchievementPersister<CollectionAchievement> {

        public const string JsonItems = "Items";

        public CollectionAchievementPersister(CollectionAchievement achievement) : base(achievement) {
        }

        public override JSONClass SerializeAchievementData(JSONClass json) {
            json[JsonItems] = SimpleJsonUtils.CreateJsonArray(Achievement.Data);
            return base.SerializeAchievementData(json);
        }

        public override void DeserializeAchievementData(JSONNode json) {
            base.DeserializeAchievementData(json);
            Achievement.Data = SimpleJsonUtils.CreateHashSet(json[JsonItems]);
        }

        /// <summary>
        /// Checks and adds an item only if not already preset (also saves data).
        /// </summary>
        /// <returns><c>true</c>, if item was added, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public virtual bool CheckAndAddItem(string item) {
            if (Achievement.CheckAndAddItem(item)) {
                SaveAchievementData();
                return true;
            }

            return false;
        }
    }
}
