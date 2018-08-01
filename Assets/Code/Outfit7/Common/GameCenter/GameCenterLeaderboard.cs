//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

namespace Outfit7.GameCenter {

    /// <summary>
    /// Game center leaderboard.
    /// </summary>
    public class GameCenterLeaderboard {

        public int Number { get; private set; }

        public string Id { get; private set; }

        public string Title { get; private set; }

        public GameCenterLeaderboard(int number, string id, string title) {
            Number = number;
            Id = id;
            Title = title;
        }

        public override string ToString() {
            return string.Format("[GameCenterLeaderboard: Number={0}, Id={1}, Title={2}]", Number, Id, Title);
        }
    }
}
