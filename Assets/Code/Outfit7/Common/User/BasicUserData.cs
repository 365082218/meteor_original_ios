//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.AddOn;
using Outfit7.Currency;
using Outfit7.Util;

namespace Outfit7.User {

    /// <summary>
    /// Basic user data.
    /// </summary>
    public class BasicUserData {

        public static string EncodePlayerId(string playerId) {
#if UNITY_IOS
            return "GC|" + playerId;
#elif UNITY_ANDROID
            return "GP|" + playerId;
#elif UNITY_WP8
            return "XL|" + playerId;
#else
            return "UE|" + playerId;
#endif
        }

        public static string DecodePlayerId(string playerId) {
            if (string.IsNullOrEmpty(playerId)) return null;
            if (playerId.Length < 3) return null;

            return playerId.Substring(3);
        }

        public string PlayerId { get; private set; }

        public string Name { get; private set; }

        public CurrencyState CurrencyState { get; private set; }

        public AddOnStock AddOnStock { get; private set; }

        public ICollection<BasicMiniGameData> MiniGames { get; private set; }

        protected BasicUserData(string playerId, string name, CurrencyState currencyState, AddOnStock stock,
            ICollection<BasicMiniGameData> miniGames) {
            PlayerId = playerId;
            Name = name;
            if (currencyState != null) {
                CurrencyState = new CurrencyState(currencyState); // Make safe copy
            }
            if (stock != null) {
                AddOnStock = new AddOnStock(stock); // Make safe copy
            }
            MiniGames = miniGames;
        }

        public override string ToString() {
            return string.Format("[BasicUserData: PlayerId={0}, Name={1}, CurrencyState={2}, AddOnStock={3}, MiniGames={4}]",
                PlayerId, Name, CurrencyState, AddOnStock, StringUtils.CollectionToCommaDelimitedString(MiniGames));
        }
    }
}
