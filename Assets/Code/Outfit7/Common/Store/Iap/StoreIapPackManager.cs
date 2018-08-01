//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Event;
using Outfit7.Threading;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Store.Iap {

    /// <summary>
    /// Store in-app purchase pack manager.
    /// </summary>
    public class StoreIapPackManager {

        private const string Tag = "StoreIapPackManager";
        private CoroutineExecutor coroutineExecutor;
        private Dictionary<string, StoreIapPack> iapPackMap;

        public EventBus EventBus { get; set; }

        public MainExecutor MainExecutor { get; set; }

        public bool Ready {
            get {
                return iapPackMap != null;
            }
        }

        public void Setup() {
            O7Log.DebugT(Tag, "Setup");

            this.coroutineExecutor = new CoroutineExecutor(MainExecutor);
        }

        public void Load(string data) {
            O7Log.DebugT(Tag, "Loading IAP packs from store data: {0}...", data);

            Action<Dictionary<string, StoreIapPack>> callback = delegate(Dictionary<string, StoreIapPack> newIapPackMap) {
                Dictionary<string, StoreIapPack> oldIapPackMap = iapPackMap;
                this.iapPackMap = newIapPackMap;

                if (O7Log.DebugEnabled && iapPackMap != null) {
                    O7Log.DebugT(Tag, "Loaded {0} valid IAP packs from store data: {1}", iapPackMap.Count,
                        StringUtils.CollectionToCommaDelimitedString(iapPackMap));
                }

                if (oldIapPackMap != iapPackMap) { // Check if both are null
                    EventBus.FireEvent(CommonEvents.STORE_IAPS_CHANGE);
                }
            };
            coroutineExecutor.Post(Unmarshal(data, callback));
        }

        private IEnumerator<Null> Unmarshal(string data, Action<Dictionary<string, StoreIapPack>> callback) {
            JSONNode iapPacksJ;
            try {
                iapPacksJ = JSON.Parse(data);

            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "Cannot unmarshal IAP packs");
                callback(null);
                yield break;
            }

            if (iapPacksJ == null) {
                O7Log.WarnT(Tag, "No IAP packs found");
                callback(null);
                yield break;
            }

            Dictionary<string, StoreIapPack> iapPacks = new Dictionary<string, StoreIapPack>(iapPacksJ.Count);

            foreach (JSONNode packJ in iapPacksJ.Childs) {
                yield return null;

                try {
                    StoreIapPack pack = new StoreIapPack(packJ);
                    iapPacks.Add(pack.Id, pack);

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

        public StoreIapPack GetPack(string iapPackId) {
            Assert.State(Ready, "Not ready");
            if (!iapPackMap.ContainsKey(iapPackId))
                return null;
            return iapPackMap[iapPackId];
        }
    }
}
