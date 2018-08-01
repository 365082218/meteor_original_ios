//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Json;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Grid.Iap {

    /// <summary>
    /// Grid in-app purchase pack.
    /// </summary>
    public class GridIapPack {

        private const string Tag = "GridIapPack";

        public JSONNode RawData { get; private set; }

        public string Id { get; private set; }

        public int Position { get; private set; }

        public string ClickUrl { get; private set; }

        public Dictionary<string, GridIapPackItem> ItemMap { get; private set; }

        public GridIapPack(JSONNode rawData, int position) {
            RawData = rawData;
            Position = position;

            Id = rawData["id"];
            Assert.HasText(Id, "id");

            ClickUrl = rawData["clickUrl"];

            JSONArray itemArr = SimpleJsonUtils.EnsureJsonArray(rawData["items"]);
            if (itemArr != null) {
                ItemMap = new Dictionary<string, GridIapPackItem>(itemArr.Count);
                foreach (JSONNode itemObj in itemArr) {
                    try {
                        GridIapPackItem item = new GridIapPackItem(itemObj);
                        ItemMap.Add(item.Name, item);

                    } catch (Exception e) {
                        O7Log.WarnT(Tag, e, "Cannot unmarshal IAP pack {0} item: {1}", Id, itemObj);
                    }
                }
            }
        }

        public override string ToString() {
            return string.Format("[GridIapPack: Id={0}, Position={1}, ClickUrl={2}, ItemMap={3}, RawData={4}]",
                Id, Position, ClickUrl, StringUtils.CollectionToCommaDelimitedString(ItemMap), RawData);
        }
    }
}
