//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Json;
using Outfit7.Social.Network;
using Outfit7.User;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Social {

    /// <summary>
    /// Social users unmarshaller.
    /// </summary>
    public abstract class AbstractSocialUserUnmarshaller<T> where T : BasicSocialUser {

        protected const string Tag = "SocialUserUnmarshaller";

        protected const string JsonFriends = "fs";
        protected const string JsonStrangers = "ss";

        public static IDictionary<string, BasicMiniGameData> UnmarshalMiniGames(JSONNode miniGames) {
            JSONArray miniGamesA = SimpleJsonUtils.EnsureJsonArray(miniGames);
            if (miniGamesA == null) return null;
            if (miniGamesA.Count == 0) return null;

            IDictionary<string, BasicMiniGameData> idMiniGames = new Dictionary<string, BasicMiniGameData>(miniGamesA.Count);
            foreach (JSONNode mg in miniGamesA) {
                var mgd = new BasicMiniGameData(mg);
                if (!StringUtils.HasText(mgd.Id)) continue;
                if (mgd.HiScore <= 0) continue;
                idMiniGames[mgd.Id] = mgd;
            }
            return idMiniGames;
        }

        public virtual Pair<List<T>, List<T>> Unmarshal(JSONNode data, IDictionary<string, SocialFriend> socialIdFriends) {
            List<T> friends = UnmarshalFriends(data[JsonFriends], socialIdFriends);
            List<T> strangers = UnmarshalStrangers(data[JsonStrangers]);
            return new Pair<List<T>, List<T>>(friends, strangers);
        }

        public virtual List<T> UnmarshalFriends(JSONNode friendsJ, IDictionary<string, SocialFriend> socialIdFriends) {
            List<T> friends = new List<T>(CollectionUtils.Count(socialIdFriends));
            if (CollectionUtils.IsEmpty(socialIdFriends))
                return friends;

            O7Log.DebugT(Tag, "Parsing social friends...");

            JSONArray friendsA = SimpleJsonUtils.EnsureJsonArray(friendsJ);
            if (friendsA == null || friendsA.Count == 0) {
                O7Log.DebugT(Tag, "No social friends");
            } else {
                UnmarshalUsers(friendsA, friends, true, socialIdFriends);
            }

            O7Log.DebugT(Tag, "Got {0} social friends", friends.Count);

            return friends;
        }

        public virtual List<T> UnmarshalStrangers(JSONNode strangersJ) {
            O7Log.DebugT(Tag, "Parsing social strangers...");

            List<T> strangers;
            JSONArray strangersA = SimpleJsonUtils.EnsureJsonArray(strangersJ);
            if (strangersA == null || strangersA.Count == 0) {
                strangers = new List<T>();
                O7Log.DebugT(Tag, "No social strangers");
            } else {
                strangers = new List<T>(strangersA.Count);
                UnmarshalUsers(strangersA, strangers, false, null);
            }

            O7Log.DebugT(Tag, "Got {0} social strangers", strangers.Count);

            return strangers;
        }

        public virtual JSONArray MarshalUsers(ICollection<T> users) {
            if (users == null)
                return null;

            JSONArray usersJ = new JSONArray();
            foreach (T user in users) {
                usersJ.Add(MarshalUser(user));
            }
            return usersJ;
        }

        protected virtual void UnmarshalUsers(JSONArray usersJ, ICollection<T> users, bool mustBeFriend, IDictionary<string, SocialFriend> socialIdFriends) {
            foreach (JSONNode userJ in usersJ) {
                try {
                    T user = UnmarshalUser(userJ, mustBeFriend, socialIdFriends);
                    users.Add(user);

                } catch (Exception e) {
                    O7Log.WarnT(Tag, e, "Cannot unmarshal social user: {0}", userJ);
                }
            }
        }

        protected abstract T UnmarshalUser(JSONNode userJ, bool mustBeFriend, IDictionary<string, SocialFriend> socialIdFriends);

        protected abstract JSONClass MarshalUser(T user);
    }
}
