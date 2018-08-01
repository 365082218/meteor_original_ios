//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Social.Network;
using Outfit7.User;
using Outfit7.Util;

namespace Outfit7.Social {

    /// <summary>
    /// Social user.
    /// </summary>
    public class BasicSocialUser {

        private const string VKontakteIdPrefix = "vk_";

        public static string WrapIdWithPrefix(string socialId, SocialNetworkType socialType) {
            if (socialId == null) return null;

            switch (socialType) {
                case SocialNetworkType.VKontakte:
                    return VKontakteIdPrefix + socialId;
                default:
                    return socialId;
            }
        }

        public static void UnwrapIdWithPrefix(string wrappedSocialId, out string socialId, out SocialNetworkType? socialType) {
            if (!StringUtils.HasText(wrappedSocialId)) {
                socialId = null;
                socialType = null;
                return;
            }

            if (wrappedSocialId.StartsWith(VKontakteIdPrefix, StringComparison.Ordinal)) {
                socialId = wrappedSocialId.Remove(0, VKontakteIdPrefix.Length);
                socialType = SocialNetworkType.VKontakte;
                return;
            }

            socialId = wrappedSocialId;
            socialType = SocialNetworkType.Facebook;
        }

        public string SocialId { get; private set; }

        public SocialNetworkType? SocialType { get; private set; }

        public string CountryCode { get; private set; }

        public IDictionary<string, BasicMiniGameData> IdMiniGames { get; private set; }

        public SocialFriend SocialFriend { get; private set; }

        public BasicSocialUser(string socialId, SocialNetworkType? socialType, string countryCode,
            IDictionary<string, BasicMiniGameData> idMiniGames, SocialFriend friend) {
            if (friend != null) {
                Assert.HasText(socialId, "socialId");
                Assert.NotNull(socialType, "socialType");
                Assert.State(socialId == friend.Id, "socialId must match friend's Id");
            }

            SocialId = socialId;
            SocialType = socialType;
            CountryCode = countryCode;
            IdMiniGames = idMiniGames;
            SocialFriend = friend;
        }

        protected string ExtractFirstName(string name) {
            if (name == null) return null;

            return name.Split(null, 2)[0]; // Splits by white-space
        }

        public int GetMiniGameHiScore(string miniGameId) {
            if (IdMiniGames == null) return 0;

            BasicMiniGameData mgd;
            IdMiniGames.TryGetValue(miniGameId, out mgd);
            return (mgd == null) ? 0 : mgd.HiScore;
        }
    }
}
