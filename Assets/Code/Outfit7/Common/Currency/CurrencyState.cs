//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Currency {

    /// <summary>
    /// Currency state.
    /// </summary>
    public class CurrencyState {

        private readonly IDictionary<string, CurrencyAccount> IdAccounts;

        public bool PaidUser { get; set; }

        public bool FacebookLoginRewarded { get; set; }

        public bool VKontakteSubscribeRewarded { get; set; }

        public bool VKontakteLoginRewarded { get; set; }

        public bool TwitterFollowRewarded { get; set; }

        public bool PushSubscribeRewarded { get; set; }

        public bool YouTubeSubscribeRewarded { get; set; }

        public CurrencyAccount GetAccount(string id) {
            return IdAccounts[id];
        }

        public int GetBalance(string accountId) {
            return GetAccount(accountId).Balance;
        }

        public CurrencyState(ICollection<CurrencyAccount> accounts)
            : this(accounts, false, false, false, false, false, false, false) {
        }

        public CurrencyState(CurrencyState state)
            : this(state.IdAccounts.Values, state.PaidUser, state.TwitterFollowRewarded,
                state.PushSubscribeRewarded, state.YouTubeSubscribeRewarded, state.FacebookLoginRewarded,
                state.VKontakteSubscribeRewarded, state.VKontakteLoginRewarded) {
        }

        public CurrencyState(ICollection<CurrencyAccount> accounts, bool paidUser, bool twitterFollowRewarded,
            bool pushRewarded, bool youTubeRewarded, bool fbLoginRewarded, bool vkSubscribeRewarded,
            bool vkLoginRewarded) {
            IdAccounts = new Dictionary<string, CurrencyAccount>(accounts.Count);
            foreach (CurrencyAccount a in accounts) {
                IdAccounts[a.Id] = a;
            }
            PaidUser = paidUser;
            FacebookLoginRewarded = fbLoginRewarded;
            VKontakteSubscribeRewarded = vkSubscribeRewarded;
            VKontakteLoginRewarded = vkLoginRewarded;
            TwitterFollowRewarded = twitterFollowRewarded;
            PushSubscribeRewarded = pushRewarded;
            YouTubeSubscribeRewarded = youTubeRewarded;
        }

        public CurrencyState(JSONNode stateJ) {
            JSONArray accountsJ = stateJ["accounts"].AsArray;
            IdAccounts = new Dictionary<string, CurrencyAccount>(accountsJ.Count);
            foreach (JSONNode j in accountsJ) {
                string id = j["id"];
                int balance = j["balance"].AsInt;
                CurrencyAccount account = new CurrencyAccount(id, balance);
                IdAccounts[id] = account;
            }
            PaidUser = stateJ["paidUser"].AsBool;
            FacebookLoginRewarded = stateJ["facebookLoginRewarded"].AsBool;
            VKontakteSubscribeRewarded = stateJ["vKontakteSubscribeRewarded"].AsBool;
            VKontakteLoginRewarded = stateJ["vKontakteLoginRewarded"].AsBool;
            TwitterFollowRewarded = stateJ["twitterFollowRewarded"].AsBool;
            PushSubscribeRewarded = stateJ["pushRewarded"].AsBool;
            YouTubeSubscribeRewarded = stateJ["youtubeRewarded"].AsBool;
        }

        public JSONClass ToJson() {
            JSONArray accountsJ = new JSONArray();
            foreach (CurrencyAccount account in IdAccounts.Values) {
                JSONClass accountJ = new JSONClass();
                accountJ["id"] = account.Id;
                accountJ["balance"].AsInt = account.Balance;
                accountsJ.Add(accountJ);
            }
            JSONClass stateJ = new JSONClass();
            stateJ["accounts"] = accountsJ;
            stateJ["paidUser"].AsBool = PaidUser;
            stateJ["facebookLoginRewarded"].AsBool = FacebookLoginRewarded;
            stateJ["vKontakteSubscribeRewarded"].AsBool = VKontakteSubscribeRewarded;
            stateJ["vKontakteLoginRewarded"].AsBool = VKontakteLoginRewarded;
            stateJ["twitterFollowRewarded"].AsBool = TwitterFollowRewarded;
            stateJ["pushRewarded"].AsBool = PushSubscribeRewarded;
            stateJ["youtubeRewarded"].AsBool = YouTubeSubscribeRewarded;
            return stateJ;
        }

        public override string ToString() {
            return string.Format("[CurrencyState: Accounts={0}, PaidUser={1}, FacebookLoginRewarded={2}, VKontakteSubscribeRewarded={3}, VKontakteLoginRewarded={4}, TwitterFollowRewarded={5}, PushSubscribeRewarded={6}, YouTubeSubscribeRewarded={7}]",
                StringUtils.CollectionToCommaDelimitedString(IdAccounts.Values), PaidUser, FacebookLoginRewarded, VKontakteSubscribeRewarded, VKontakteLoginRewarded, TwitterFollowRewarded, PushSubscribeRewarded, YouTubeSubscribeRewarded);
        }
    }
}
