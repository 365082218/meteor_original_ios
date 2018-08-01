//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Analytics.BigQuery;
using Outfit7.Analytics.Firebase;
using Outfit7.Analytics.Tracking;
using Outfit7.Event;
using Outfit7.Grid;
using Outfit7.Util;

namespace Outfit7.Currency {

    /// <summary>
    /// Currency state store.
    /// </summary>
    public interface CurrencyStateStore {

        CurrencyState LoadCurrencyState();

        void SaveCurrencyState(CurrencyState currencyState);
    }

    /// <summary>
    /// Currency manager.
    /// </summary>
    public class CurrencyManager {

        protected const string Tag = "CurrencyManager";

        public CurrencyState CurrencyState { get; protected set; }

        public EventBus EventBus { get; set; }

        public GridManager GridManager { get; set; }

        public CurrencyStateStore CurrencyStateStore { get; set; }

        public BigQueryTracker BqTracker { get; set; }

        public FirebaseTracker FirebaseTracker { get; set; }

        public virtual void Init() {
            O7Log.DebugT(Tag, "Init");

            if (CurrencyState == null) {
                CurrencyState = CurrencyStateStore.LoadCurrencyState();
                Assert.NotNull(CurrencyState, "CurrencyState");

                if (GridManager.Ready) {
                    OnGridLoad(null);
                }
                EventBus.AddListener(CommonEvents.FRESH_GRID_DOWNLOAD, OnGridLoad);
            }

            O7Log.DebugT(Tag, "Done: {0}", CurrencyState);
        }

        public virtual void RestoreState(CurrencyState currencyState, bool writeChanges = false) {
            Assert.NotNull(currencyState, "currencyState");

            bool paidUser = CurrencyState.PaidUser;

            CurrencyState = currencyState;
            AreChanges = true;

            // Fix paid user - purchases can be made before restore (MTA-5960)
            // Re-check paid user - can be hacked now
            if (paidUser || currencyState.PaidUser) {
                TrySetPaidUser(true);
            }

            if (writeChanges) {
                WriteChanges();
            }
        }

        public virtual bool TrySetPaidUser(bool paid, bool writeChanges = false) {
            if (paid && IsUserHacker) {
                paid = false;
                O7Log.WarnT(Tag, "User is hacker, paid flag is not allowed");
            }
            if (paid == CurrencyState.PaidUser) return false;

            CurrencyState.PaidUser = paid;
            AreChanges = true;

            if (writeChanges) {
                WriteChanges();
            }

            return true;
        }

        protected virtual bool IsUserHacker {
            get {
                if (!GridManager.Ready) return false;
                return GridManager.JsonData["h"].AsBool;
            }
        }

        protected virtual void OnGridLoad(object eventData) {
            if (IsUserHacker) {
                O7Log.WarnT(Tag, "User is hacker, paid flag is not allowed");
                TrySetPaidUser(false, true);
            }
        }

#region Save

        public virtual bool AreChanges { get; protected set; }

        public virtual bool WriteChanges(bool force = false) {
            if (!force && !AreChanges) return false;
            CurrencyStateStore.SaveCurrencyState(CurrencyState);
            ClearChanges();
            return true;
        }

        public virtual void ClearChanges() {
            AreChanges = false;
        }

#endregion

#region Purchases

        // Non virtual currency related
        public virtual void PurchasedNonCurrency(string id, bool paid, string receiptData = null,
            bool writeChanges = true, bool createEvent = true) {
            if (paid) {
                TrySetPaidUser(true, writeChanges);
            }

            if (createEvent) {
                Assert.HasText(id, "id");
                BqTracker.CreateCurrencyBuilder(CommonTrackingEventParams.EventId.Iap, id, null, 0, -1).Add();
                AddReceiptEvent(receiptData, id);
            }
        }

        public virtual int PurchasedCurrency(string accountId, int amount, string id, bool paid,
            string receiptData = null, bool writeChanges = true, bool createEvent = true) {
            CurrencyAccount account = CurrencyState.GetAccount(accountId);
            return PurchasedCurrency(account, amount, id, paid, receiptData, writeChanges, createEvent);
        }

        public virtual int PurchasedCurrency(CurrencyAccount account, int amount, string id, bool paid,
            string receiptData = null, bool writeChanges = true, bool createEvent = true) {
            int oldBalance = account.Balance;

            if (amount != 0) {
                account.Credit(amount);
                AreChanges = true;
            }
            if (paid) {
                TrySetPaidUser(true);
            }

            if (writeChanges) {
                WriteChanges();
            }

            if (createEvent) {
                Assert.HasText(id, "id");
                BqTracker.CreateCurrencyBuilder(CommonTrackingEventParams.EventId.Iap, id, account.Id, amount,
                    account.Balance).Add();
                FirebaseTracker.LogEarnCurrencyEvent(id, account.Id, amount);
                AddReceiptEvent(receiptData, id);
            }

            CheckAndFireBalanceChangeEvent(account, oldBalance);

            return account.Balance;
        }

#endregion

#region Free

        public virtual int GotFreeCurrency(string accountId, int amount, string eventId, string eventItemId,
            bool writeChanges = true, bool createEvent = true) {
            CurrencyAccount account = CurrencyState.GetAccount(accountId);
            return GotFreeCurrency(account, amount, eventId, eventItemId, writeChanges, createEvent);
        }

        public virtual int GotFreeCurrency(CurrencyAccount account, int amount, string eventId, string eventItemId,
            bool writeChanges = true, bool createEvent = true) {
            int oldBalance = account.Balance;

            if (amount != 0) {
                account.Credit(amount);
                AreChanges = true;
            }

            if (writeChanges) {
                WriteChanges();
            }

            if (createEvent) {
                Assert.HasText(eventId, "eventId");
                BqTracker.CreateCurrencyBuilder(eventId, eventItemId, account.Id, amount, account.Balance).Add();
                FirebaseTracker.LogEarnCurrencyEvent(eventItemId, account.Id, amount);
            }

            CheckAndFireBalanceChangeEvent(account, oldBalance);

            return account.Balance;
        }

        public virtual int TakeCurrency(string accountId, int amount, string receiptData, string eventId,
            string eventItemId, bool writeChanges = true, bool createEvent = true) {
            CurrencyAccount account = CurrencyState.GetAccount(accountId);
            return TakeCurrency(account, amount, receiptData, eventId, eventItemId, writeChanges, createEvent);
        }

        public virtual int TakeCurrency(CurrencyAccount account, int amount, string receiptData, string eventId,
            string eventItemId, bool writeChanges = true, bool createEvent = true) {
            int oldBalance = account.Balance;

            if (amount != 0) {
                // Negative balance is allowed
                account.Debit(amount);
                AreChanges = true;
            }

            if (writeChanges) {
                WriteChanges();
            }

            if (createEvent) {
                Assert.HasText(eventId, "eventId");
                BqTracker.CreateCurrencyBuilder(eventId, eventItemId, account.Id, -amount, account.Balance).Add();
                FirebaseTracker.LogSpendCurrencyEvent(eventItemId, account.Id, amount);
                AddReceiptEvent(receiptData, eventItemId);
            }

            CheckAndFireBalanceChangeEvent(account, oldBalance);

            return account.Balance;
        }

        public virtual int AdjustCurrency(string accountId, int amount, bool writeChanges = true,
            bool createEvent = true) {
            CurrencyAccount account = CurrencyState.GetAccount(accountId);
            return AdjustCurrency(account, amount, writeChanges, createEvent);
        }

        public virtual int AdjustCurrency(CurrencyAccount account, int amount, bool writeChanges = true,
            bool createEvent = true) {
            int oldBalance = account.Balance;

            if (amount != 0) {
                account.Credit(amount);
                AreChanges = true;
            }

            if (writeChanges) {
                WriteChanges();
            }

            if (createEvent) {
                BqTracker.CreateCurrencyBuilder(CommonTrackingEventParams.EventId.Adjust, null, account.Id, amount,
                    account.Balance).Add();
            }

            CheckAndFireBalanceChangeEvent(account, oldBalance);

            return account.Balance;
        }

#endregion

        protected virtual void AddReceiptEvent(string receiptData, string eventItemId) {
            if (!StringUtils.HasText(receiptData)) return;
            BqTracker.CreateBuilder(CommonTrackingEventParams.GroupId.Purchase,
                CommonTrackingEventParams.EventId.Receipt, true)
                .SetP2(eventItemId)
                .SetCustomData(receiptData)
                .AddAndSend();
        }

        protected virtual void CheckAndFireBalanceChangeEvent(CurrencyAccount account, int oldBalance) {
            if (oldBalance == account.Balance) return; // No change
            CurrencyBalanceChangeEvent e = new CurrencyBalanceChangeEvent(account, oldBalance);
            EventBus.FireEvent(CommonEvents.CURRENCY_BALANCE_CHANGE, e);
        }
    }
}
