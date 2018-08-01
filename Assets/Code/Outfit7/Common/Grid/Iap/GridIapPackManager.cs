//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Threading;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Grid.Iap {

    /// <summary>
    /// Grid in-app purchase pack manager.
    /// </summary>
    public class GridIapPackManager {

        private const string Tag = "GridIapPackManager";

        public static int? GetAmount(GridIapPack pack, string itemId) {
            if (pack.ItemMap == null)
                return null;
            if (!pack.ItemMap.ContainsKey(itemId))
                return null;
            GridIapPackItem item = pack.ItemMap[itemId];
            return item.Amount;
        }

        private CoroutineExecutor coroutineExecutor;
        private Dictionary<string, GridIapPack> iapPackMap;

        public EventBus EventBus { get; set; }

        public MainExecutor MainExecutor { get; set; }

        public GridManager GridManager { get; set; }

        public bool Ready {
            get {
                return iapPackMap != null;
            }
        }

        public void Setup() {
            O7Log.DebugT(Tag, "Setup");

            this.coroutineExecutor = new CoroutineExecutor(MainExecutor);

            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, delegate(object eventData) {
                Load((JSONNode) eventData, false);
            });

            if (GridManager.Ready) {
                // Load immediatelly because other data can be dependant on it at app start
                Load(GridManager.JsonData, true);
            }
        }

        private void Load(JSONNode jsonData, bool immediatelly) {
            O7Log.DebugT(Tag, "Loading IAP packs from GRID data...");

            Action<Dictionary<string, GridIapPack>> callback = delegate(Dictionary<string, GridIapPack> newIapPackMap) {
                Dictionary<string, GridIapPack> oldIapPackMap = iapPackMap;
                this.iapPackMap = newIapPackMap;

                if (O7Log.DebugEnabled && iapPackMap != null) {
                    O7Log.DebugT(Tag, "Loaded {0} valid IAP packs from GRID data: {1}", iapPackMap.Count,
                        StringUtils.CollectionToCommaDelimitedString(iapPackMap));
                }

                if (oldIapPackMap != iapPackMap) { // Check if both are null
                    EventBus.FireEvent(CommonEvents.GRID_IAPS_CHANGE);
                }
            };

            if (immediatelly) {
                // Load now
                IEnumerator<Null> enumerator = Unmarshal(jsonData, callback);
                while (enumerator.MoveNext()) {
                }

            } else {
                // Load by coroutine
                coroutineExecutor.Post(Unmarshal(jsonData, callback));
            }
        }

        private IEnumerator<Null> Unmarshal(JSONNode gridJ, Action<Dictionary<string, GridIapPack>> callback) {
            JSONArray iapPacksJ = SimpleJsonUtils.EnsureJsonArray(gridJ["iapuPacks"]);
            if (iapPacksJ == null) {
                O7Log.WarnT(Tag, "No IAP packs found");
                callback(null);
                yield break;
            }

            Dictionary<string, GridIapPack> iapPacks = new Dictionary<string, GridIapPack>();

            int position = 0;
            foreach (JSONNode packJ in iapPacksJ) {
                yield return null;

                try {
                    GridIapPack iapPack = new GridIapPack(packJ, position);
                    iapPacks.Add(iapPack.Id, iapPack);
                    position++;

                } catch (Exception e) {
                    O7Log.WarnT(Tag, e, "Cannot unmarshal IAP pack: {0}", packJ);
                }
            }

            if (iapPacks.Count == 0) {
                O7Log.WarnT(Tag, "No valid IAP packs found");
                callback(null);
                yield break;
            }

            callback(iapPacks);
        }

        public GridIapPack GetPack(string iapPackId) {
            Assert.State(Ready, "Not ready");
            if (!iapPackMap.ContainsKey(iapPackId))
                return null;
            return iapPackMap[iapPackId];
        }

        public int? GetAmount(string iapPackId, string itemId) {
            Assert.State(Ready, "Not ready");
            if (!iapPackMap.ContainsKey(iapPackId))
                return null;
            GridIapPack pack = iapPackMap[iapPackId];
            return GetAmount(pack, itemId);
        }
    }
}
