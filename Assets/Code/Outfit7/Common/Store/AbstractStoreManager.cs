//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using Outfit7.Purchase;
using Outfit7.Store.Iap;
using Outfit7.Util;

namespace Outfit7.Store {

    /// <summary>
    /// Store manager.
    /// </summary>
    public abstract class AbstractStoreManager {

        protected const string Tag = "StoreManager";
        private const string BuyCanceledErrorId = "CANCELED";
        private const string BuyFailureErrorId = "FAILURE";

        protected abstract HashSet<string> StoreIapIds { get; }

        public StorePlugin StorePlugin { get; set; }

        public StoreIapPackManager StoreIapPackManager { get; set; }

        public AbstractPurchaseManager PurchaseManager { get; set; }

        public virtual void OnAppStartOrResume() {
            StartLoadingStoreData();
        }

        public virtual bool StartLoadingStoreData() {
            if (StoreIapPackManager.Ready)
                return false;
            if (!StorePlugin.IsStoreAvailable())
                return false;

            StorePlugin.StartLoadingStoreData(StoreIapIds);
            return true;
        }

        internal virtual void OnStoreDataLoad(string data) {
            StoreIapPackManager.Load(data);
        }

        public virtual void StartBuying(StoreIapPack pack) {
            StorePlugin.StartBuying(pack.Id);
        }

        internal virtual void OnBuyComplete(string purchaseId, string transactionId, double price, string currencyId, string receiptData, string payload) {
            PurchaseManager.RewardPurchase(purchaseId, transactionId, price, currencyId, receiptData, payload, true);
        }

        internal virtual void OnBuyFail(string purchaseId, string errorId) {
            if (errorId == BuyCanceledErrorId) {
                OnBuyCancel(purchaseId);

            } else if (errorId == BuyFailureErrorId) {
                OnBuyError(purchaseId);

            } else {
                throw new ArgumentException("Unknown errorId: " + errorId);
            }
        }

        protected virtual void OnBuyCancel(string purchaseId) {
            O7Log.InfoT(Tag, "User canceled buying of IAP {0}", purchaseId);
        }

        protected virtual void OnBuyError(string purchaseId) {
            O7Log.ErrorT(Tag, "Failed buying of IAP {0}", purchaseId);
        }

        public virtual void ConfirmPurchaseProcessed(string iapId, string receiptData) {
            StorePlugin.ConfirmPurchaseProcessed(iapId, receiptData);
        }

        public virtual void RestorePurchases() {
            StorePlugin.StartRestoring();
        }
    }
}
