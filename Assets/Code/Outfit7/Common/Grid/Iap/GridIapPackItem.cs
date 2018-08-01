//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Grid.Iap {

    /// <summary>
    /// Grid in-app purchase pack item.
    /// </summary>
    public class GridIapPackItem {

        public JSONNode RawData { get; private set; }

        public string Name { get; private set; }

        public int Amount { get; private set; }

        public GridIapPackItem(JSONNode rawData) {
            RawData = rawData;
            Name = rawData["name"];
            Assert.HasText(Name, "name");
            Amount = rawData["amount"].AsInt;
        }

        public override string ToString() {
            return string.Format("[GridIapPackItem: Name={0}, Amount={1}, RawData={2}]", Name, Amount, RawData);
        }
    }
}
