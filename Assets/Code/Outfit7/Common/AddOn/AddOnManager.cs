//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Threading;
using Outfit7.Threading.Task;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.AddOn {

    /// <summary>
    /// Add-on stock store.
    /// </summary>
    public interface AddOnStockStore {

        AddOnStock LoadAddOnStock();

        void SaveAddOnStock(AddOnStock stock, bool boughtChanged, bool enabledChanged, bool newlyUnlockedChanged);
    }

    /// <summary>
    /// Add-on manager.
    /// </summary>
    public class AddOnManager {

        private const string Tag = "AddOnManager";
        private bool Inited;
        private IDictionary<string, AddOnItem> AllItems;
        private CoroutineExecutor CoroutineExecutor;
        private bool BoughtAddOnChanges;
        private bool EnabledAddOnChanges;
        private bool NewlyUnlockedAddOnChanges;
        private int BoughtAddOnsBoosterSumCache = -1;

        public AddOnStock Stock { get; private set; }

        public EventBus EventBus { get; set; }

        public MainExecutor MainExecutor { get; set; }

        public GridManager GridManager { get; set; }

        public AddOnCacheHandler AddOnCacheHandler { get; set; }

        public AddOnFactory AddOnFactory { get; set; }

        public AddOnStockStore AddOnStockStore { get; set; }

        public void Init(AddOnStock stock) {
            O7Log.DebugT(Tag, "Init(stock={0})", stock);

            ClearChanges();
            LoadAllItems();

            if (stock != null) {
                Stock = new AddOnStock(stock);
            } else {
                Stock = AddOnStockStore.LoadAddOnStock();
            }

            CheckAndFixItemsIntegrity();

            // Try to update anyway even if inited multiple times (on game restore),
            // because first download might not be finished yet
            if (GridManager.Ready) {
                OnGridLoad(GridManager.JsonData);
            }

            if (!Inited) { // Init can be called multiple times (on game restore)
                EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, delegate(object eventData) {
                    OnGridLoad((JSONNode) eventData);
                });

                Inited = true;
            }

            O7Log.DebugT(Tag, "Done");
        }

        private void CheckAndFixItemsIntegrity() {
            O7Log.DebugT(Tag, "Checking and fixing integrity...");

            Dictionary<string, int> boughtCopy = new Dictionary<string, int>(Stock.BoughtItems);
            foreach (KeyValuePair<string, int> entry in boughtCopy) {
                AddOnItem item = GetAddOn(entry.Key);
                if (item == null) {
                    // Not exists anymore
                    Stock.BoughtItems.Remove(entry.Key);
                    BoughtAddOnChanges = true;
                    O7Log.WarnT(Tag, "Found non-existent bought item {0}", entry.Key);
                    continue;
                }
                item.State = AddOnItemState.Ready;
                item.Stock = entry.Value;
            }
            BoughtAddOnsBoosterSumCache = -1;

            List<string> enabledCopy = new List<string>(Stock.EnabledItems);
            foreach (string itemId in enabledCopy) {
                AddOnItem item = GetAddOn(itemId);
                if (item == null) {
                    // Not exists anymore
                    Stock.EnabledItems.Remove(itemId);
                    EnabledAddOnChanges = true;
                    O7Log.WarnT(Tag, "Found non-existent enabled item {0}", itemId);
                    continue;
                }
                if (item.State != AddOnItemState.Ready) {
                    // Not bought at all
                    Stock.EnabledItems.Remove(itemId);
                    EnabledAddOnChanges = true;
                    O7Log.WarnT(Tag, "Found non-bought enabled item {0}", itemId);
                    continue;
                }
                item.State = AddOnItemState.Enabled;
            }

            List<string> newlyUnlockedCopy = new List<string>(Stock.NewlyUnlockedItems);
            foreach (string itemId in newlyUnlockedCopy) {
                AddOnItem item = GetAddOn(itemId);
                if (item == null) {
                    // Not exists anymore
                    Stock.NewlyUnlockedItems.Remove(itemId);
                    NewlyUnlockedAddOnChanges = true;
                    O7Log.WarnT(Tag, "Found non-existent newly unlocked item {0}", itemId);
                }
            }

            bool changed = WriteChanges();
            if (changed) {
                O7Log.WarnT(Tag, "Checking and fixing integrity finished. Fixed? true");
            } else {
                O7Log.DebugT(Tag, "Checking and fixing integrity finished. Fixed? false");
            }
        }

        private void OnGridLoad(JSONNode dataJ) {
            JSONNode configJ = FindGridConfig(dataJ);
            if (configJ == null) return;

            UpdateIgnoredItems(configJ);

            string url = configJ["u"].Value;
            if (!StringUtils.HasText(url)) {
                O7Log.DebugT(Tag, "Not updating add-ons, no URL in GRID");
                return;
            }
            if (url == AddOnCacheHandler.Url) {
                O7Log.DebugT(Tag, "Not updating add-ons, got same URL={0} in GRID", AddOnCacheHandler.Url);
                return;
            }
            UpdateItemsCache(url);
        }

        private JSONNode FindGridConfig(JSONNode dataJ) {
            JSONArray arrayJ = SimpleJsonUtils.EnsureJsonArray(dataJ["cIsCs"]);
            if (arrayJ == null) return null;
            for (int i = 0; i < arrayJ.Count; i++) {
                JSONNode cJ = arrayJ[i];
                if (cJ["t"].Value == "ADDONS") return cJ;
            }
            return null;
        }

        public int BoughtAddOnsBoosterSum {
            get {
                if (BoughtAddOnsBoosterSumCache == -1) {
                    int booster = 0;
                    foreach (AddOnItem item in AllItems.Values) {
                        if (item.IsBought && item.Booster > 0) {
                            booster += item.Booster;
                        }
                    }
                    BoughtAddOnsBoosterSumCache = booster;
                }
                return BoughtAddOnsBoosterSumCache;
            }
        }

#region Load

        private void LoadAllItems() {
            AllItems = AddOnFactory.UnmarshalBundledAddOns();

            // Update bundled add-ons with those from cached data immediately
            JSONNode cachedJ = AddOnCacheHandler.LoadAddOnsCache();
            if (cachedJ != null) {
                Action<IDictionary<string, AddOnItem>> callback = delegate(IDictionary<string, AddOnItem> cachedItems) {
                    UpdateItemsFromCache(AllItems, cachedItems);
                };
                IEnumerator<Null> unmarshaller = AddOnFactory.UnmarshalCachedAddOns(cachedJ, callback);
                while (unmarshaller.MoveNext()) {
                }
            }

            O7Log.DebugT(Tag, "Loaded {0} add-ons", AllItems.Count);
        }

        private void UpdateItemsCache(string url) {
            O7Log.VerboseT(Tag, "UpdateItemsCache({0})", url);

            // NOTE: Be sure to update right items in coroutines, when data is downloaded.
            // This method may be called more than once in the middle of updating cached items in coroutines.
            // So let coroutines update old items or none.

            IDictionary<string, AddOnItem> items = AllItems;

            // Download new add-ons
            TaskFeedback<JSONNode> feedbackWrapper = new TaskFeedback<JSONNode>(delegate {
            }, delegate(JSONNode responseJ) {
                if (items != AllItems) {
                    O7Log.DebugT(Tag, "Add-ons has already been downloaded before or re-inited again");
                    return;
                }

                if (CoroutineExecutor == null) {
                    CoroutineExecutor = new CoroutineExecutor(MainExecutor);
                }

                // Update current add-ons with those from cached data in coroutine
                Action<IDictionary<string, AddOnItem>> onUmarshalSuccess = delegate(IDictionary<string, AddOnItem> cachedItems) {
                    if (items != AllItems) {
                        O7Log.DebugT(Tag, "Add-ons has already been reloaded before or re-inited again");
                        return;
                    }
                    UpdateItemsFromCache(items, cachedItems);
                    UpdateIgnoredItems(null);
                    AddOnCacheHandler.SaveAddOnsCache(responseJ, url);
                    BoughtAddOnsBoosterSumCache = -1; // Booster may have been updated
                    EventBus.FireEvent(CommonEvents.ADDONS_CHANGE);
                };
                CoroutineExecutor.Post(AddOnFactory.UnmarshalCachedAddOns(responseJ, onUmarshalSuccess));

            }, delegate(Exception e) {
            });

            AddOnCacheHandler.DownloadAddOns(url, feedbackWrapper);
        }

        // This method is super fast so no need to be in coroutine (5ms for 500 items).
        // In fact, many problems arise if add-ons are not updated all at once, because some needed to be fixed after
        // they are updated and so there would be an event needed for each add-on update.
        private void UpdateItemsFromCache(IDictionary<string, AddOnItem> items, IDictionary<string, AddOnItem> cachedItems) {
            O7Log.DebugT(Tag, "Updating {0} add-ons from {1} cached data items...", items.Count, cachedItems.Count);

            int c = 0;
            foreach (AddOnItem cachedItem in cachedItems.Values) {
                if (!items.ContainsKey(cachedItem.Id)) {
                    O7Log.WarnT(Tag, "Cached add-on {0} not found between bundled items", cachedItem);
                    continue;
                }

                AddOnItem bundledItem = items[cachedItem.Id];
                AddOnFactory.UpdateAddOn(bundledItem, cachedItem);
                c++;
            }

            O7Log.DebugT(Tag, "Updated {0} add-ons from cached data", c);
        }

        private void UpdateIgnoredItems(JSONNode configJ) {
            if (configJ == null) {
                if (!GridManager.Ready) return;
                configJ = FindGridConfig(GridManager.JsonData);
                if (configJ == null) return;
            }

            O7Log.DebugT(Tag, "Updating ignored add-ons from grid data...");

            JSONArray ignoresJ = SimpleJsonUtils.EnsureJsonArray(configJ["iIs"]);
            int c = UpdateIgnoredItems(ignoresJ, true);
            JSONArray unignoresJ = SimpleJsonUtils.EnsureJsonArray(configJ["uIs"]);
            c += UpdateIgnoredItems(unignoresJ, false);

            O7Log.DebugT(Tag, "Updated {0} ignored add-ons from grid data", c);
        }

        private int UpdateIgnoredItems(JSONArray ignoresJ, bool ignore) {
            if (ignoresJ == null) return 0;

            int c = 0;
            for (int i = 0; i < ignoresJ.Count; i++) {
                string id = ignoresJ[i];
                if (!AllItems.ContainsKey(id)) continue;
                AddOnItem item = GetAddOn(id);
                if (item.IsIgnored == ignore) continue;
                item.IsIgnored = ignore;
                c++;
                O7Log.VerboseT(Tag, "Add-on set to ignored={0}: {1}", ignore, item);
            }
            return c;
        }

        public AddOnItem GetAddOn(string id) {
            if (!AllItems.ContainsKey(id)) {
                // For devel purposes of new add-ons: do not return null if add-on is not found in manager
                if (BuildConfig.IsDevel) {
                    O7Log.WarnT(Tag, "Add-on '{0}' does not exist. Creating one for debug purposes", id);
                    AddOnItem item = AddOnFactory.CreateDevelAddOn(id, AllItems.Count);
                    AllItems[id] = item;
                    return item;
                }
                return null;
            }

            return AllItems[id];
        }

#endregion

#region Save

        public bool AreChanges {
            get {
                return BoughtAddOnChanges || EnabledAddOnChanges || NewlyUnlockedAddOnChanges;
            }
        }

        public bool WriteChanges() {
            return WriteChanges(false);
        }

        public bool WriteChanges(bool force) {
            if (force) {
                AddOnStockStore.SaveAddOnStock(Stock, true, true, true);
                ClearChanges();
                return true;
            }

            if (!AreChanges)
                return false;

            AddOnStockStore.SaveAddOnStock(Stock, BoughtAddOnChanges, EnabledAddOnChanges, NewlyUnlockedAddOnChanges);
            ClearChanges();
            return true;
        }

        public void ClearChanges() {
            BoughtAddOnChanges = false;
            EnabledAddOnChanges = false;
            NewlyUnlockedAddOnChanges = false;
        }

#endregion

#region Debug

        internal void UnlockAndBuyAllAddOns(bool writeChanges) {
            foreach (AddOnItem item in AllItems.Values) {
                if (item.IsBought)
                    continue;

                item.State = AddOnItemState.Ready;
                item.Stock = 1;
                Stock.SetBoughtItem(item);
                BoughtAddOnChanges = true;
            }

            if (Stock.NewlyUnlockedItems.Count > 0) {
                NewlyUnlockedAddOnChanges = true;
            }
            BoughtAddOnsBoosterSumCache = -1;
            Stock.NewlyUnlockedItems.Clear();

            if (writeChanges) {
                WriteChanges();
            }
        }

#endregion

#region Lock

        public bool IsAddOnUnlockedInLevel(AddOnItem item, int level) {
            return item.IsBought || !item.IsLockedForLevel(level);
        }

        public int NewlyUnlockedAddOnsCount {
            get {
                return Stock.NewlyUnlockedItems.Count;
            }
        }

        public bool IsAddOnNewlyUnlocked(AddOnItem item) {
            return Stock.NewlyUnlockedItems.Contains(item.Id);
        }

        public void SetNewlyUnlockedAddOns<T>(ICollection<T> items, bool writeChanges) where T : AddOnItem {
            Assert.HasLength(items, "items");

            foreach (AddOnItem item in items) {
                bool added = Stock.AddNewlyUnlockedItem(item);
                Assert.State(added, "Add-on already newly unlocked: {0}", item);
                NewlyUnlockedAddOnChanges = true;
            }

            if (writeChanges) {
                WriteChanges();
            }

            LogItems("Set newly unlocked add-ons: {0}", items);
        }

        public void SetNewlyUnlockedAddOn(AddOnItem item, bool writeChange) {
            bool added = Stock.AddNewlyUnlockedItem(item);
            Assert.State(added, "Add-on already newly unlocked: {0}", item);
            NewlyUnlockedAddOnChanges = true;

            if (writeChange) {
                WriteChanges();
            }

            O7Log.InfoT(Tag, "Set newly unlocked add-on: {0}", item);
        }

        public void RemoveNewlyUnlockedAddOns<T>(ICollection<T> items, bool writeChanges) where T : AddOnItem {
            Assert.HasLength(items, "items");

            foreach (AddOnItem item in items) {
                bool removed = Stock.RemoveNewlyUnlockedItem(item);
                Assert.State(removed, "Add-on not newly unlocked: {0}", item);
                NewlyUnlockedAddOnChanges = true;
            }

            if (writeChanges) {
                WriteChanges();
            }

            LogItems("Removed newly unlocked add-ons: {0}", items);
        }

        public void RemoveNewlyUnlockedAddOn(AddOnItem item, bool writeChange) {
            bool removed = Stock.RemoveNewlyUnlockedItem(item);
            Assert.State(removed, "Add-on not newly unlocked: {0}", item);
            NewlyUnlockedAddOnChanges = true;

            if (writeChange) {
                WriteChanges();
            }

            O7Log.InfoT(Tag, "Removed newly unlocked add-on: {0}", item);
        }

        private void LogItems<T>(string text, ICollection<T> items) where T : AddOnItem {
            if (!O7Log.InfoEnabled)
                return;
            if (items.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            foreach (AddOnItem item in items) {
                sb.Append(item.Id);
                sb.Append(", ");
            }
            O7Log.InfoT(Tag, text, sb);
        }

#endregion

#region Ignore

        public bool IsAddOnIgnored(AddOnItem item) {
            if (BuildConfig.IsDevel) return false;
            if (item.IsBought) return false;
            return item.IsIgnored;
        }

#endregion

#region Buy

        internal bool SetAsPreBoughtAddOn(AddOnItem item, int stock, bool writeChange) {
            if (item.IsBought && item.Stock == stock)
                return false;

            if (!item.IsBought) {
                item.State = AddOnItemState.Ready;

                if (item.Booster > 0) {
                    BoughtAddOnsBoosterSumCache = -1;
                }
            }
            item.Stock = stock;
            Stock.SetBoughtItem(item);
            BoughtAddOnChanges = true;

            // Don't debit, even if price > 0

            // Can't be locked

            if (writeChange) {
                WriteChanges();
            }

            O7Log.DebugT(Tag, "Add-on {0} set as pre-bought with stock {1}", item.Id, item.Stock);

            return true;
        }

        public void BuyAddOn(AddOnItem item, bool writeChange) {
            Assert.State(!item.IsBought, "Add-on is already bought: {0}", item);

            item.State = AddOnItemState.Ready;
            item.Stock = 1;
            Stock.SetBoughtItem(item);
            BoughtAddOnChanges = true;

            if (item.Booster > 0) {
                BoughtAddOnsBoosterSumCache = -1;
            }

            if (writeChange) {
                WriteChanges();
            }

            O7Log.DebugT(Tag, "Add-on {0} bought", item.Id);
        }

        public void BuyOneConsumableAddOn(AddOnItem item, bool writeChange) {
            if (!item.IsBought) {
                item.State = AddOnItemState.Ready;

                if (item.Booster > 0) {
                    BoughtAddOnsBoosterSumCache = -1;
                }
            }
            item.Stock++;
            Stock.SetBoughtItem(item);
            BoughtAddOnChanges = true;

            if (writeChange) {
                WriteChanges();
            }

            O7Log.DebugT(Tag, "Add-on {0} bought (1x)", item.Id);
        }

        public void ConsumeOneAddOn(AddOnItem item, bool writeChange) {
            Assert.State(item.IsBought, "Add-on is not bought: {0}", item);

            item.Stock--;
            if (item.Stock == 0) {
                if (item.IsEnabled) {
                    DisableAddOn(item, false);
                }
                item.State = AddOnItemState.NotBought;

                if (item.Booster > 0) {
                    BoughtAddOnsBoosterSumCache = -1;
                }
            }
            Stock.SetBoughtItem(item);
            BoughtAddOnChanges = true;

            if (writeChange) {
                WriteChanges();
            }

            O7Log.DebugT(Tag, "Add-on {0} consumed (1x)", item.Id);
        }

#endregion

#region Enable/Equip/Wear

        internal bool SetAsPreEnabledAddOn(AddOnItem item, bool writeChange) {
            Assert.State(item.IsBought, "Add-on is not bought: {0}", item);

            if (item.IsEnabled)
                return false; // Already enabled

            item.State = AddOnItemState.Enabled;
            Stock.AddEnabledItem(item);
            EnabledAddOnChanges = true;

            if (writeChange) {
                WriteChanges();
            }

            O7Log.DebugT(Tag, "Add-on {0} set as pre-enabled", item.Id);

            return true;
        }

        public void EnableAddOn(AddOnItem item, bool writeChange) {
            Assert.State(item.State == AddOnItemState.Ready, "Add-on is NOT ready: {0}", item);

            item.State = AddOnItemState.Enabled;
            Stock.AddEnabledItem(item);
            EnabledAddOnChanges = true;

            if (writeChange) {
                WriteChanges();
            }

            O7Log.DebugT(Tag, "Add-on {0} enabled", item.Id);
        }

        public void DisableAddOn(AddOnItem item, bool writeChange) {
            Assert.State(item.IsEnabled, "Add-on is NOT enabled yet: {0}", item);

            item.State = AddOnItemState.Ready;
            Stock.RemoveEnabledItem(item);
            EnabledAddOnChanges = true;

            if (writeChange) {
                WriteChanges();
            }

            O7Log.DebugT(Tag, "Add-on {0} disabled", item.Id);
        }

#endregion
    }
}
