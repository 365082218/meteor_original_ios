//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Bee7;
using Outfit7.Event;
using Outfit7.Grid.Iap;
using Outfit7.Util;
using Outfit7.Video;
using Outfit7.Promo.SpecialOffers;

namespace Outfit7.Purchase {

    /// <summary>
    /// Purchase manager.
    /// </summary>
    public abstract class AbstractPurchaseManager {

        protected const string Tag = "PurchaseManager";

        protected class RewardingPurchase {

            public IapPack IapPack { get; private set; }

            public string TransactionId { get; private set; }

            public double Price { get; private set; }

            public string CurrencyId { get; private set; }

            public string ReceiptData { get; private set; }

            public string Payload { get; private set; }

            public bool ShowBubble { get; private set; }

            public RewardingPurchase(IapPack iap, string transactionId, double price, string currencyId, string receiptData, string payload, bool showBubble) {
                IapPack = iap;
                TransactionId = transactionId;
                Price = price;
                CurrencyId = currencyId;
                ReceiptData = receiptData;
                Payload = payload;
                ShowBubble = showBubble;
            }
        }

        private readonly HashSet<RewardingPurchase> RewardingPurchaseQueue = new HashSet<RewardingPurchase>();

        public EventBus EventBus { get; set; }

        public GridIapPackManager GridIapPackManager { get; set; }

        public VideoClipManager VideoClipManager { get; set; }

        public SpecialOfferManager SpecialOfferManager { get; set; }

        public SpecialOfferManager PurchaseScreenSpecialOfferManager { get; set; }

        public virtual void Init() {
            EventBus.AddListener(CommonEvents.GRID_IAPS_CHANGE, delegate {
                OnGridIapsChange();
            });

            VideoClipManager.VideoClipCompletionCallback = delegate(string id, int amount) {
                RewardVideoClip(id, amount);
            };
        }

        protected virtual void OnGridIapsChange() {
            if (RewardingPurchaseQueue.Count == 0)
                return;

            O7Log.DebugT(Tag, "Processing {0} queued purchases...", RewardingPurchaseQueue.Count);
            HashSet<RewardingPurchase> copy = new HashSet<RewardingPurchase>(RewardingPurchaseQueue); // Copy set
            foreach (RewardingPurchase ppp in copy) {
                RewardPurchase(ppp);
            }
        }

#region Reward

        public virtual bool IsIapSpecialOfferPackValid(SpecialOfferManager specialOfferManager, IapPack iap) {
            if (specialOfferManager == null) return false;
            if (specialOfferManager.ActivatedSpecialOffer == null) return false;
            IapSpecialOffer iapSpecialOffer = specialOfferManager.ActivatedSpecialOffer as IapSpecialOffer;
            if (iapSpecialOffer == null) return false;
            return (iap.Id == iapSpecialOffer.DiscountedIapPackId);
        }

        public virtual bool IsIapSpecialOfferPack(IapPack iap) {
            return IsIapSpecialOfferPackValid(SpecialOfferManager, iap);
        }

        public virtual bool IsPurchaseScreenIapSpecialOfferPack(IapPack iap) {
            return IsIapSpecialOfferPackValid(PurchaseScreenSpecialOfferManager, iap);
        }

        public abstract IapPack FindIapPack(string iapId);

        public virtual GridIapPack FindGridIapPack(IapPack iap) {
            if (!GridIapPackManager.Ready)
                return null;
            return GridIapPackManager.GetPack(iap.Id);
        }

        public virtual bool RewardPurchase(IapPack iap, bool showBubble) {
            return RewardPurchase(iap.Id, null, 0, null, null, null, showBubble);
        }

        public virtual bool RewardPurchase(string iapId, string transactionId, double price, string currencyId, string receiptData, string payload, bool showBubble) {
            IapPack iap = FindIapPack(iapId);
            bool rewarded;
            if (iap.IsForCurrency) {
                RewardingPurchase ppp = new RewardingPurchase(iap, transactionId, price, currencyId, receiptData, payload, showBubble);
                rewarded = RewardPurchase(ppp);
            } else {
                RewardNonCurrencyPurchase(iap, transactionId, price, currencyId, receiptData);
                rewarded = true;
            }

            if (rewarded && SpecialOfferManager != null && IsIapSpecialOfferPack(iap)) {
                SpecialOfferManager.OnActivatedSpecialOfferBought();
            }
            if (rewarded && PurchaseScreenSpecialOfferManager != null && IsPurchaseScreenIapSpecialOfferPack(iap)) {
                PurchaseScreenSpecialOfferManager.OnActivatedSpecialOfferBought();
            }
            return rewarded;
        }

        protected abstract void RewardNonCurrencyPurchase(IapPack iap, string transactionId, double price, string currencyId, string receiptData);

        protected virtual bool RewardPurchase(RewardingPurchase rp) {
            // If GRID IAP packs are not ready, postpone it.
            // This can happen on reinstall if some purchase was not processed. GRID data is not ready yet, but store has been inited
            if (!GridIapPackManager.Ready) {
                O7Log.WarnT(Tag, "No GRID IAPs: Postponing purchase {0}", rp.IapPack.Id);
                RewardingPurchaseQueue.Add(rp); // Won't add if already in, because it is hash set
                return false;
            }

            GridIapPack gp = FindGridIapPack(rp.IapPack);

            // If GRID IAP pack does not exist, postpone it.
            // This can happen if GRID data has become corrupted or somebody has removed this pack from it
            if (gp == null) {
                O7Log.WarnT(Tag, "Missing GRID IAP {0}: Postponing purchase", rp.IapPack.Id);
                RewardingPurchaseQueue.Add(rp); // Won't add if already in, because it is hash set
                return false;
            }

            RewardingPurchaseQueue.Remove(rp);

            return RewardPurchaseNow(rp.IapPack, gp, rp.TransactionId, rp.Price, rp.CurrencyId, rp.ReceiptData, rp.Payload, rp.ShowBubble);
        }

        public virtual bool RewardPurchaseNow(PurchasePack pack, bool showBubble) {
            return RewardPurchaseNow(pack.IapPack, pack.GridPack, showBubble);
        }

        public virtual bool RewardPurchaseNow(IapPack iap, GridIapPack gp, bool showBubble) {
            return RewardPurchaseNow(iap, gp, null, 0, null, null, null, showBubble);
        }

        public abstract bool RewardPurchaseNow(IapPack iap, GridIapPack gp, string transactionId, double price, string currencyId, string receiptData, string payload, bool showBubble);

        public abstract bool RewardBee7App(Reward reward, bool showBubble);

        public abstract bool RewardVideoClip(string providerId, int amount);

        public abstract bool Reward3rdPartyExternalAppOffer(string offerId, int amount);

#endregion

#region Seize

        public virtual bool SeizePurchase(IapPack iap, bool showBubble) {
            GridIapPack gp = GridIapPackManager.GetPack(iap.Id);
            return SeizePurchaseNow(iap, gp, showBubble);
        }

        public abstract bool SeizePurchaseNow(IapPack iap, GridIapPack gp, bool showBubble);

#endregion

#region Currency Amount

        public int GetCurrencyAmount(IapPack iap, int defaultAmount) {
            Assert.IsTrue(iap.IsForCurrency, "IAP is not for currency: {0}", iap);
            return GetCurrencyAmount(iap.Id, defaultAmount, iap.CurrencyId);
        }

        public int? GetCurrencyAmount(IapPack iap) {
            Assert.IsTrue(iap.IsForCurrency, "IAP is not for currency: {0}", iap);
            return GetCurrencyAmount(iap.Id, iap.CurrencyId);
        }

        public int GetCurrencyAmount(string iapId, int defaultAmount, string currencyId) {
            int? amount = GetCurrencyAmount(iapId, currencyId);
            return (amount != null) ? amount.Value : defaultAmount;
        }

        public int? GetCurrencyAmount(string iapId, string currencyId) {
            if (!GridIapPackManager.Ready)
                return null;
            GridIapPack gridPack = GridIapPackManager.GetPack(iapId);
            if (gridPack == null)
                return null;
            return GetCurrencyAmount(gridPack, currencyId);
        }

        public int GetCurrencyAmount(GridIapPack gridPack, int defaultAmount, string currencyId) {
            int? amount = GetCurrencyAmount(gridPack, currencyId);
            return (amount != null) ? amount.Value : defaultAmount;
        }

        public int? GetCurrencyAmount(GridIapPack gridPack, string currencyId) {
            string itemId = GetGridIapPackItemId(currencyId);
            return GridIapPackManager.GetAmount(gridPack, itemId);
        }

        protected abstract string GetGridIapPackItemId(string currencyId);

#endregion
    }
}
