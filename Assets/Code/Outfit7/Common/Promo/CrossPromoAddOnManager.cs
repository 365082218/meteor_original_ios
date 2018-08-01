using System.Collections.Generic;
using Outfit7.AddOn;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.Common.Promo {
    public abstract class CrossPromoAddOnManager {

        protected virtual string Tag { get { return GetType().Name; } }

        public EventBus EventBus { get; set; }

        public GridManager GridManager { get; set; }

        protected Dictionary<string, CrossPromoAddOnAppData> AllAppData;

        protected abstract Dictionary<string, CrossPromoAddOnAppData> GetAllAppData();

        protected abstract void CreateCrossPromoAddOnAppData(string addOnId, string appId, string actionUrl, string iconUrl, string promoText, bool isUnlocked);

        protected abstract void UpdateCrossPromoAddOnAppData(string addOnId, string appId, string actionUrl, string iconUrl, string promoText, bool isUnlocked);

        protected abstract void RemoveCrossPromoAddOnAppData(string addOnId);

        protected virtual void EnsureAllAppData() {
            if (AllAppData == null) {
                AllAppData = GetAllAppData();
            }
        }

        public virtual void Setup() {
            EnsureAllAppData();
            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, OnFreshGridDownload);
            OnFreshGridDownload(null);
        }

        protected virtual void OnFreshGridDownload(object eventData) {
            if (!GridManager.Ready) {
                return;
            }
            JSONArray advertisedAppA = SimpleJsonUtils.EnsureJsonArray(GridManager.JsonData["ext"]["advertisedAppUrls"]);
            if (advertisedAppA == null) {
                return;
            }

            int count = advertisedAppA.Count;
            for (int i = 0; i < count; i++) {
                JSONNode advertisedAppJ = advertisedAppA[i];
                string addOnId = advertisedAppJ["id"];
                string actionUrl = advertisedAppJ["url"];
                string iconUrl = advertisedAppJ["iU"];
                string promoText = advertisedAppJ["pT"];
                string appId = advertisedAppJ["aId"];
                bool isUnlocked = advertisedAppJ["u"].AsBool;
                bool isRemoved = advertisedAppJ["d"].AsBool;
                if (AllAppData.ContainsKey(addOnId)) {
                    if (isRemoved) {
                        RemoveCrossPromoAddOnAppData(addOnId);
                    } else {
                        UpdateCrossPromoAddOnAppData(addOnId, appId, actionUrl, iconUrl, promoText, isUnlocked);
                    }
                } else {
                    CreateCrossPromoAddOnAppData(addOnId, appId, actionUrl, iconUrl, promoText, isUnlocked);
                }
            }
        }

        public virtual CrossPromoAddOnAppData GetLockedItemAppData(AddOnItem item) {
            if (AllAppData.ContainsKey(item.Id)) {
                return AllAppData[item.Id];
            }

            return null;
        }

        protected virtual bool IsAppInstalled(AddOnItem item) {
            CrossPromoAddOnAppData appData = GetLockedItemAppData(item);
            if (appData == null) {
                return false;
            }

#if UNITY_EDITOR || UNITY_ANDROID
            return AppPlugin.IsAppInstalled(appData.AppId);
#else
            return true;
#endif
        }

        public virtual bool IsItemLockableByApp(AddOnItem item) {
            EnsureAllAppData();
            if (AllAppData == null) {
                return false;
            }

            CrossPromoAddOnAppData appData = GetLockedItemAppData(item);
            return appData != null && !appData.IsUnlocked;
        }

        public virtual bool IsItemLockedByApp(AddOnItem item) {
            if (item.IsBought) {
                return false;
            }

            return !IsAppInstalled(item);
        }

        public virtual Texture2D GetAppTexture(AddOnItem item) {
            CrossPromoAddOnAppData appData = GetLockedItemAppData(item);
            if (appData == null) {
                return null;
            }
            return appData.IconTexture;
        }
    }
}
