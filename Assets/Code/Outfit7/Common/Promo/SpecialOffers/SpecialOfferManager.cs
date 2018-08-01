//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using System;
using System.Collections;
using System.Collections.Generic;
using Outfit7.AddOn;
using Outfit7.Analytics.Tracking;
using Outfit7.Common;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Json;
using Outfit7.Threading;
using Outfit7.Util;
using Outfit7.Util.Io;
using Outfit7.Web;
using SimpleJSON;
using UnityEngine;
using Outfit7.Store.Iap;
using Outfit7.Purchase;
using Outfit7.Grid.Iap;

namespace Outfit7.Promo.SpecialOffers {

    /// <summary>
    /// Special offer manager.
    /// </summary>
    public abstract class SpecialOfferManager {

        public const string PrefTestGridData = "SpecialOfferManager.TestGridData";
        private const int UsedSpecialOfferHistoryLength = 25;
        private const string CacheName = "SpecialOfferBackgrounds";
        private const string JsonGridExt = "ext";

        protected const string SpecialOfferActionUrl = "o7so://";

        public virtual string Tag { get { return this.GetType().Name; } }

        protected abstract string AnalyticsName { get; }

        protected abstract string PrefActivatedSpecialOfferKey { get; }

        protected abstract string PrefActivatedSpecialOfferStartDateTimeKey { get; }

        protected abstract string PrefUsedSpecialOfferIdsKey { get; }

        private readonly Dictionary<string, string> RestHeaders = new Dictionary<string, string> {
            { RestCall.Header.UserAgent, AppPlugin.UserAgentName },
        };

        public static void ClearPrefs() {
            DeleteCache(CacheName);
        }

        protected static string CreateCachePath(string dirName) {
            return Application.persistentDataPath + "/" + dirName + "/";
        }

        protected static void DeleteCache(string dirName) {
            try {
                O7Directory.Delete(CreateCachePath(dirName), true);

            } catch (Exception e) {
                // TODO TineL: Use Tag once not static
                O7Log.WarnT("SpecialOfferManager", e, "Cannot delete cache: {0}", CreateCachePath(dirName));
            }
        }

        public IList<SpecialOffer> AvailableSpecialOffers { get; protected set; }

        public SpecialOffer ActivatedSpecialOffer { get; protected set; }

        public DateTime ActivatedSpecialOfferActivationTime { get; protected set; }

        public EventBus EventBus { get; set; }

        public GridManager GridManager { get; set; }

        public GridIapPackManager GridIapPackManager { get; set; }

        public AbstractPurchaseManager PurchaseManager { get; set; }

        public AddOnManager AddOnManager { get; set; }

        public AbstractPurchaseHelper PurchaseHelper { get; set; }

        public StoreIapPackManager StoreIapPackManager { get; set; }

        public TrackingManager TrackingManager { get; set; }

        public MainExecutor MainExecutor { get; set; }

        public event SpecialOfferBecomeReadyDelegate OnSpecialOfferBecomeReady;

        public delegate void SpecialOfferBecomeReadyDelegate();

        public event Action OnSpecialOfferActivate;

        public event Action<IList<SpecialOffer>> OnSpecialOffersAvailable;

        private bool IsRetrievingImage;
        private List<string> UsedSpecialOfferIds;

        protected abstract string GridChannelTagName { get; }

        protected string CachePath {
            get {
                return CreateCachePath(CacheName);
            }
        }

        public bool IsActivated {
            get {
                return ActivatedSpecialOffer != null;
            }
        }

        public bool IsActiveAndValid {
            get {
                if (!IsActivated) return false;
                return ActivatedSpecialOfferTimeLeft > TimeSpan.Zero;
            }
        }

        public TimeSpan ActivatedSpecialOfferTimeLeft {
            get {
                return ActivatedSpecialOfferActivationTime.ToUniversalTime() + ActivatedSpecialOffer.TimeLimit - DateTime.UtcNow;
            }
        }

        public void Setup() {
            O7Log.DebugT(Tag, "Setup");

            UsedSpecialOfferIds = SimpleJsonUtils.CreateList(UserPrefs.GetJson(PrefUsedSpecialOfferIdsKey, null));

            RestoreActivatedSpecialOffer();

            if (GridManager.Ready) {
                InitSpecialOffersFromGridData(GridManager.JsonData);
            }
            EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, delegate {
                InitSpecialOffersFromGridData(GridManager.JsonData);
            });

            O7Log.DebugT(Tag, "Done; UsedSpecialOfferIds={0}", StringUtils.CollectionToCommaDelimitedString(UsedSpecialOfferIds));
        }

        private void InitSpecialOffersFromGridData(JSONNode gridData) {
            O7Log.VerboseT(Tag, "Initing special offers from grid data...");

            if (BuildConfig.IsProdOrDevel) { // check if test mode
                string testGridData = UserPrefs.GetString(PrefTestGridData, string.Empty);
                if (!string.IsNullOrEmpty(testGridData)) {
                    O7Log.VerboseT(Tag, "Loading preloaded test data instead of grid");
                    gridData = JSONNode.Parse(testGridData);
                }
            }

            JSONArray soA = SimpleJsonUtils.EnsureJsonArray(gridData[JsonGridExt][GridChannelTagName]);
            if (soA == null || soA.Count == 0) {
                O7Log.DebugT(Tag, "No special offers in GRID data");
                return;
            }

            List<SpecialOffer> sops = new List<SpecialOffer>(soA.Count);
            foreach (JSONNode soJ in soA) {
                SpecialOffer sop = ParseSpecialOfferPack(soJ);

                if (sop == null) {
                    O7Log.WarnT(Tag, "Unparsable special offer data: {0}", soJ);
                    continue;
                }

                if (!sop.IsValid) {
                    O7Log.WarnT(Tag, "Invalid special offer: {0}", sop);
                    continue;
                }

                if (CollectionUtils.Contains(UsedSpecialOfferIds, sop.Id)) {
                    O7Log.DebugT(Tag, "{0} already used, skipping ...", sop.Id);
                    continue;
                }

                sops.Add(sop);
            }

            if (sops.Count == 0) {
                O7Log.DebugT(Tag, "No valid special offers in GRID data");
                return;
            }

            AvailableSpecialOffers = sops;

            O7Log.DebugT(Tag, "Got available special offers ({0}): {1}", AvailableSpecialOffers.Count,
                StringUtils.CollectionToCommaDelimitedString(AvailableSpecialOffers));

            if (OnSpecialOffersAvailable != null) {
                OnSpecialOffersAvailable(AvailableSpecialOffers);
            }
        }

        protected virtual SpecialOffer ParseSpecialOfferPack(JSONNode nodeJ) {

            SpecialOfferType type = SpecialOfferType.GetType(nodeJ);
            string cachePath = CachePath;

            SpecialOffer offer = null;
            try {
                if (SpecialOfferType.AddOn == type) {
                    AddOnSpecialOffer addOnOffer = new AddOnSpecialOffer(nodeJ, cachePath);
                    addOnOffer.AddOnManager = AddOnManager;
                    offer = addOnOffer;
                } else if (SpecialOfferType.CrossPromo == type) {
                    CrossPromoSpecialOffer crossPromoOffer = new CrossPromoSpecialOffer(nodeJ, cachePath);
                    crossPromoOffer.GiveReward = delegate {
                        GiveCrossPromoReward(crossPromoOffer);
                        if (ActivatedSpecialOffer == crossPromoOffer) {
                            UserPrefs.SetJson(PrefActivatedSpecialOfferKey, ActivatedSpecialOffer.ToJson());
                            UserPrefs.SaveDelayed();
                        }
                    };
                    offer = crossPromoOffer;
                } else if (SpecialOfferType.Iap == type) {
                    IapSpecialOffer iapSpecialOffer = new IapSpecialOffer(nodeJ, cachePath);
                    iapSpecialOffer.StoreIapPackManager = StoreIapPackManager;
                    offer = iapSpecialOffer;

                } else if (SpecialOfferType.IapBonus == type) {
                    IapBonusSpecialOffer iapSpecialOffer = new IapBonusSpecialOffer(nodeJ, cachePath);
                    iapSpecialOffer.StoreIapPackManager = StoreIapPackManager;
                    offer = iapSpecialOffer;
                }
            } catch (Exception ex) {
                O7Log.WarnT(Tag, ex, "Unparsable offer {0}", nodeJ);
            }

            return offer;
        }

        private void RestoreActivatedSpecialOffer() {

            JSONNode soJ = UserPrefs.GetJson(PrefActivatedSpecialOfferKey, null);

            if (soJ == null) {
                O7Log.DebugT(Tag, "Activated special offer not found");
                return;
            }

            SpecialOffer specialOffer = ParseSpecialOfferPack(soJ);

            if (specialOffer == null) {
                O7Log.DebugT(Tag, "Activated special could not be restored");
                return;
            }

            O7Log.DebugT(Tag, "Restoring activated special offer...");

            if (!specialOffer.IsCreativesCached) {
                O7Log.WarnT(Tag, "Special offer cannot be activated, missing assets");
                return;
            }

            ActivatedSpecialOffer = specialOffer;
            ActivatedSpecialOfferActivationTime = UserPrefs.GetDateTime(PrefActivatedSpecialOfferStartDateTimeKey, DateTime.MinValue);

            O7Log.DebugT(Tag, "Restored activated special offer {0} started on {1}", specialOffer, ActivatedSpecialOfferActivationTime);


            if (!CreativeReady(specialOffer)) {
                RetrieveContent(specialOffer);
            }
        }

        protected void ActivateSpecialOffer(SpecialOffer pack) {

            O7Log.DebugT(Tag, "Activating special offer {0}...", pack);

            ActivatedSpecialOffer = pack;
            ActivatedSpecialOfferActivationTime = DateTime.Now;
            if (UsedSpecialOfferIds == null) {
                UsedSpecialOfferIds = new List<string>();
            }
            UsedSpecialOfferIds.Add(pack.Id);
            AvailableSpecialOffers.Remove(pack);

            while (UsedSpecialOfferIds.Count > UsedSpecialOfferHistoryLength) {
                UsedSpecialOfferIds.RemoveAt(0);
            }

            UserPrefs.SetJson(PrefActivatedSpecialOfferKey, ActivatedSpecialOffer.ToJson());
            UserPrefs.SetDateTime(PrefActivatedSpecialOfferStartDateTimeKey, ActivatedSpecialOfferActivationTime);
            UserPrefs.SetJson(PrefUsedSpecialOfferIdsKey, SimpleJsonUtils.CreateJsonArray(UsedSpecialOfferIds));
            UserPrefs.SaveDelayed();

            O7Log.DebugT(Tag, "Activated special offer {0} started on {1}", ActivatedSpecialOffer, ActivatedSpecialOfferActivationTime);

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.SpecialOffer,
                CommonTrackingEventParams.EventId.SpecialOfferStart, AnalyticsName, pack.Id, null, null, null, null);

            if (OnSpecialOfferActivate != null) {
                OnSpecialOfferActivate();
            }
        }

        public void DeactivateActivatedSpecialOffer() {

            O7Log.DebugT(Tag, "Deactivating activated special offer {0} started on {1}",
                ActivatedSpecialOffer, ActivatedSpecialOfferActivationTime);

            ActivatedSpecialOffer = null;
            ActivatedSpecialOfferActivationTime = DateTime.MinValue;

            UserPrefs.Remove(PrefActivatedSpecialOfferKey);
            UserPrefs.Remove(PrefActivatedSpecialOfferStartDateTimeKey);
            UserPrefs.SaveDelayed();
        }

        public bool CheckAndDeactivateInvalidSpecialOffer() {

            if (!IsActivated) return false;

            if (DateTime.UtcNow < ActivatedSpecialOfferActivationTime.ToUniversalTime() + ActivatedSpecialOffer.TimeLimit) return false;
            O7Log.InfoT(Tag, "Activated special offer {0} started on {1} has expired", ActivatedSpecialOffer, ActivatedSpecialOfferActivationTime);

            bool valid = CanShowActivatedSpecialOffer();
            if (valid) return false;

            O7Log.InfoT(Tag, "Activated special offer {0} started on {1} is not valid anymore",
                ActivatedSpecialOffer, PrefActivatedSpecialOfferStartDateTimeKey);

            DeactivateActivatedSpecialOffer();
            return true;
        }

        public virtual bool IsNextSpecialOfferReady {
            get {
                SpecialOffer specialOffer = GetNextAvailableSpecialOffer();
                return specialOffer != null;
            }
        }

        public virtual bool CanShowActivatedSpecialOffer() {

            if (!IsActiveAndValid) return false;
            if (!ActivatedSpecialOffer.CanActivate) return false;

            if (ActivatedSpecialOffer.Type == SpecialOfferType.Iap) {
                if (!IsIapPackDefined(ActivatedSpecialOffer as IapSpecialOffer)) return false;
            }

            return true;
        }

        protected virtual bool IsSpecialOfferTypeSupported(SpecialOffer specialOffer) {
            return true;
        }

        private SpecialOffer GetNextAvailableSpecialOffer() {

            if (IsRetrievingImage) return null;
            if (IsActivated) return null;
            if (AvailableSpecialOffers == null) return null;
            if (AvailableSpecialOffers.Count == 0) return null;

            for (int i = 0; i < AvailableSpecialOffers.Count; i++) {
                SpecialOffer pack = AvailableSpecialOffers[i];

                if (!IsSpecialOfferTypeSupported(pack)) continue;

                if (!CreativeReady(pack)) {
                    if (pack.DidTryDownloadBackgroundImage) continue;
                    RetrieveContent(pack);
                    return null;
                }

                if (pack.Icon.Required && !IconReady(pack)) {
                    if (pack.Icon.DidTryDownloadIcon) continue;
                    RetrieveIcon(pack);
                    return null;
                }

                if (!pack.CanActivate) continue;

                if (pack.Type == SpecialOfferType.Iap) {
                    if (!IsIapPackDefined(pack as IapSpecialOffer)) continue;
                }

                return pack;
            }

            return null;
        }

        protected virtual bool IsIapPackDefined(IapSpecialOffer offer) {
            if (offer == null) return false;
            if (!CheckIaps(offer)) return false;

            return true;
        }

        protected virtual bool CheckIaps(IapSpecialOffer sop) {
            IapPack iap = GetValidIap(sop.OriginalIapPackId);
            if (iap == null) {
                O7Log.WarnT(Tag, "Invalid IAP pack for special offer {0}", sop);
                return false;
            }
            IapPack discountedIap = GetValidDiscountedIap(sop.DiscountedIapPackId);
            if (discountedIap == null) {
                O7Log.WarnT(Tag, "Invalid discounted IAP pack for special offer {0}", sop);
                return false;
            }
            if (iap.IsForCurrency != discountedIap.IsForCurrency) {
                O7Log.WarnT(Tag, "Currency mismatch for special offer {0}", sop);
                return false;
            }
            return true;
        }

        protected virtual IapPack GetValidIap(string id) {
            if (!GridIapPackManager.Ready) {
                return null;
            }
            GridIapPack gp = GridIapPackManager.GetPack(id);
            if (gp == null) {
                O7Log.WarnT(Tag, "Unknown GRID pack ID {0}", id);
                return null;
            }
            IapPack iap = PurchaseManager.FindIapPack(id);
            if (iap == null) {
                O7Log.WarnT(Tag, "Unknown IAP pack ID {0}", id);
                return null;
            }
            if (iap.IsForCurrency) {
                int? amount = PurchaseManager.GetCurrencyAmount(iap);
                if (amount == null || amount.Value <= 0) {
                    O7Log.WarnT(Tag, "No IAP currency amount {0}", iap);
                    return null;
                }
            }
            return iap;
        }

        protected virtual IapPack GetValidDiscountedIap(string id) {
            return GetValidIap(id);
        }

        public bool IsSpecialOfferItem(AddOnItem addOnItem) {
            if (addOnItem == null) return false;

            AddOnSpecialOffer offer = ActivatedSpecialOffer as AddOnSpecialOffer;
            if (offer == null) return false;
            if (!IsActiveAndValid) return false;
            return addOnItem == offer.AddOnItem;
        }

        public virtual bool TryActivateSpecialOffer() {

            SpecialOffer so = GetNextAvailableSpecialOffer();
            if (so == null) return false;
            if (!so.CanActivate) return false;

            ActivateSpecialOffer(so);

            return true;
        }

        public virtual void OnAppStartOrResume() {

            if (AvailableSpecialOffers != null) {
                for (int i = 0; i < AvailableSpecialOffers.Count; i++) {
                    AvailableSpecialOffers[i].OnAppStartOrResume(false);
                }
            }

            if (!IsActivated) return;

            if (!CreativeReady(ActivatedSpecialOffer)) {
                RetrieveContent(ActivatedSpecialOffer);
            }

            ActivatedSpecialOffer.OnAppStartOrResume(true);
        }

        protected virtual bool CreativeReady(SpecialOffer specialOffer) {
            return specialOffer.IsBackgroundImageCached;
        }

        private bool IconReady(SpecialOffer specialOffer) {
            return specialOffer.Icon.IsIconCached;
        }

        public virtual void WasPresented(SpecialOffer offer) {

            O7Log.DebugT(Tag, "WasPresented {0}", offer.Id);

            if (!string.IsNullOrEmpty(offer.ImpressionUrl)) {
                RestCall call = new RestCall(offer.ImpressionUrl, RestHeaders);
                MainExecutor.StartCoroutine(call.Execute());
            }

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.SpecialOffer,
                CommonTrackingEventParams.EventId.SpecialOfferShow, AnalyticsName, offer.Id, null, null, null, null);
        }

        public virtual void HandleActionClick(SpecialOffer offer) {

            O7Log.DebugT(Tag, "HandleActionClick {0}", offer.Id);

            if (!string.IsNullOrEmpty(offer.ClickUrl)) {
                RestCall call = new RestCall(offer.ClickUrl, RestHeaders);
                MainExecutor.StartCoroutine(call.Execute());
            }

            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.SpecialOffer,
                CommonTrackingEventParams.EventId.SpecialOfferClick, AnalyticsName, offer.Id, null, null, null, null);
        }

        private void HandleActionComplete(SpecialOffer offer) {
            TrackingManager.AddEvent(CommonTrackingEventParams.GroupId.SpecialOffer,
                CommonTrackingEventParams.EventId.SpecialOfferComplete, AnalyticsName, offer.Id, null, null, null, null, true);
        }

        public Texture2D Texture(SpecialOffer specialOffer) {
            Assert.IsTrue(CreativeReady(specialOffer), "creative not yet ready");
            return ReadImageFromPath(specialOffer.BackImagePath);
        }

        public Texture2D IconTexture(SpecialOffer specialOffer) {
            Assert.IsTrue(IconReady(specialOffer), "icon not yet ready");
            return ReadImageFromPath(specialOffer.Icon.IconPath);
        }

        private Texture2D ReadImageFromPath(string path) {
            Texture2D texture = null;
            try {
                texture = new Texture2D(4, 4, TextureFormat.RGB24, false);
                byte[] bytes = O7File.ReadAllBytes(path);
                texture.LoadImage(bytes);
            } catch (Exception e) {
                O7Log.WarnT(Tag, e, "cannot read preloaded image");
            }
            return texture;
        }

        private void RetrieveContent(SpecialOffer offer) {
            if (IsRetrievingImage) return;
            if (CreativeReady(offer)) return;
            if (offer.DidTryDownloadBackgroundImage) return;

            MainExecutor.StartCoroutine(RetrieveImage(offer.BackImageUrl, offer.BackImagePath, delegate(bool sucess) {
                offer.OnBackgroundImageDownload(sucess);
                if (sucess) {
                    OnContentRetrieve();
                }
            }));
        }

        private void OnContentRetrieve() {
            if (OnSpecialOfferBecomeReady != null) {
                SpecialOffer specialOffer = GetNextAvailableSpecialOffer();
                if (specialOffer != null) {
                    OnSpecialOfferBecomeReady();
                }
            }
        }

        private void RetrieveIcon(SpecialOffer offer) {
            if (IsRetrievingImage) return;
            if (IconReady(offer)) return;
            if (offer.Icon.DidTryDownloadIcon) return;

            MainExecutor.StartCoroutine(RetrieveImage(offer.Icon.Url, offer.Icon.IconPath, delegate(bool sucess) {
                offer.Icon.OnIconDownload(sucess);
                if (sucess) {
                    OnContentRetrieve();
                }
            }));
        }

        private IEnumerator RetrieveImage(string url, string saveToPath, Action<bool> retrieveSucess) {
            IsRetrievingImage = true;

            O7Log.DebugT(Tag, "Image loading from {0}", url);

            url = RestCall.FixUrl(url);
            WWW www = new WWW(url);
            yield return www;

            IsRetrievingImage = false;
            retrieveSucess(IsValidImageAndSaved(www, saveToPath));
        }

        private bool IsValidImageAndSaved(WWW www, string fullPath) {

            if (!string.IsNullOrEmpty(www.error)) {
                O7Log.DebugT(Tag, "Image loading failed {0} download failed with error  {1}", www.url, www.error);
                return false;
            }

            Texture texture = www.texture;

            if (texture.height == 8 && texture.width == 8) {
                O7Log.DebugT(Tag, "Image loading failed...got dummy image {0}", www.url);
                return false;
            }

            try {
                O7Directory.CreateDirectory(CachePath);
                O7File.WriteAllBytes(fullPath, www.bytes);
                O7Log.DebugT(Tag, "Image saved to: {0}", fullPath);
            } catch (Exception e) {
                O7Log.WarnT(Tag, "Cannot save preloaded image, exception: {0}", e);
                return false;
            }

            if (texture != null) {
                texture.filterMode = FilterMode.Bilinear;
                O7Log.DebugT(Tag, "Image loading succeeded {0}", www.url);
                return true;
            } else {
                O7Log.DebugT(Tag, "Image loading failed {0}", www.url);
                return false;
            }
        }

        public void InvalidateStateData() {
            if (AvailableSpecialOffers == null) return;

            for (int i = 0; i < AvailableSpecialOffers.Count; i++) {
                AvailableSpecialOffers[i].InvalidateStateData();
            }

            DeactivateActivatedSpecialOffer();
            AvailableSpecialOffers = null;
        }

        public virtual void OnActivatedSpecialOfferBought() {
            HandleActionComplete(ActivatedSpecialOffer);
            ActivatedSpecialOfferActivationTime = DateTime.MinValue;
            UserPrefs.SetDateTime(PrefActivatedSpecialOfferStartDateTimeKey, ActivatedSpecialOfferActivationTime); // invalidate offer
            UserPrefs.Save();
        }

        protected virtual void GiveCrossPromoReward(CrossPromoSpecialOffer offer) {
            HandleActionComplete(offer);
        }

        public virtual void OnAddOnBought(SpecialOffer offer) {
            HandleActionComplete(offer);
        }
    }
}
