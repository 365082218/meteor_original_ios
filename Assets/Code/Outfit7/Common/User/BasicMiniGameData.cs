//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using SimpleJSON;

namespace Outfit7.User {

    /// <summary>
    /// Basic data for one mini-game.
    /// </summary>
    public class BasicMiniGameData {

        protected const string JsonId = "id";
        protected const string JsonHiScore = "hS";

        public string Id { get; private set; }

        public int HiScore { get; protected set; }

        public BasicMiniGameData(BasicMiniGameData data) {
            Id = data.Id;
            HiScore = data.HiScore;
        }

        public BasicMiniGameData(string id, int hiScore) {
            Id = id;
            HiScore = hiScore;
        }

        public BasicMiniGameData(JSONNode rawData) {
            Id = rawData[JsonId];
            HiScore = rawData[JsonHiScore].AsInt;
        }

        public virtual JSONClass ToJson() {
            JSONClass j = new JSONClass();
            j[JsonId] = Id;
            j[JsonHiScore].AsInt = HiScore;
            return j;
        }

        public override string ToString() {
            return string.Format("[BasicMiniGameData: Id={0}, HiScore={1}]", Id, HiScore);
        }
    }
}
