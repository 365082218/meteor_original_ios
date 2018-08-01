//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Store.Iap {

    /// <summary>
    /// In-app purchase store pack.
    /// </summary>
    public class StoreIapPack {

        public JSONNode RawData { get; private set; }

        public string Id { get; private set; }

        public string FormattedPrice { get; private set; }

        public bool IsSubscribed { get; private set; }

        public StoreIapPack(JSONNode rawData)
            : this(rawData["id"], rawData["formattedPrice"], rawData["subscribed"].AsBool) {
            RawData = rawData;
        }

        public StoreIapPack(string id, string formattedPrice, bool subscribed) {
            Assert.HasText(id, "id");
            Assert.HasText(formattedPrice, "formattedPrice");
            Id = id;
            FormattedPrice = formattedPrice;
            IsSubscribed = subscribed;
        }

        public override string ToString() {
            return string.Format("[StoreIapPack: Id={0}, FormattedPrice={1}, IsSubscribed={2}, RawData={3}]",
                Id, FormattedPrice, IsSubscribed, RawData);
        }
    }
}
