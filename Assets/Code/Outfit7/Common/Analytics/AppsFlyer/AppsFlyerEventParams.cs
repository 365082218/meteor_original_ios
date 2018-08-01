//
//   Copyright (c) 2017 Outfit7. All rights reserved.
//

using SimpleJSON;

namespace Outfit7.Common.Analytics.AppsFlyer {

    /// <summary>
    /// Common AppsFlyer event parameters.
    /// </summary>
    public static class AppsFlyerEventParams {

        public static class Name {

            private const string LevelAchieved = "achieved_level_{0}";
            public const string TutorialCompletion = "af_tutorial_completion";
            public const string RewardedVideoView = "rewarded_video_view";

            public static string CreateLevelAchieved(int level) {
                return string.Format(LevelAchieved, level);
            }
        }

        public static class Param {

            public const string Quantity = "af_quantity";

            public static JSONClass CreateQuantity(int quantity = 1) {
                JSONClass j = new JSONClass();
                j[Quantity].AsInt = quantity;
                return j;
            }
        }
    }
}
