//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using Outfit7.Currency;
using Outfit7.Grid.Iap;
using Outfit7.Purchase;
using Outfit7.Util;

namespace Outfit7.Common {

    /// <summary>
    /// Push subscription manager with reward.
    /// </summary>
    public class RewardingPushManager : PushManager {

        private bool ShowDialog;

        public CurrencyManager CurrencyManager { get; set; }

        public IapPack IapPack { get; set; }

        public AbstractPurchaseManager PurchaseManager { get; set; }

        protected virtual GridIapPack GridIapPack {
            get {
                if (!PurchaseManager.GridIapPackManager.Ready) return null;
                return PurchaseManager.GridIapPackManager.GetPack(IapPack.Id);
            }
        }

        protected override void OnSubscribeToPushNotifications(bool oldSubscribed) {
            SyncReward();
            base.OnSubscribeToPushNotifications(oldSubscribed);
        }

        protected virtual void SyncReward() {
            Assert.State(ArePushNotificationsAvailable, "Not available");
            GridIapPack gp = GridIapPack;
            if (gp == null) return;

            if (SubscribedToPushNotifications && !CurrencyManager.CurrencyState.PushSubscribeRewarded) {
                PurchaseManager.RewardPurchaseNow(IapPack, gp, ShowDialog);

            } else if (!SubscribedToPushNotifications && CurrencyManager.CurrencyState.PushSubscribeRewarded) {
                PurchaseManager.SeizePurchaseNow(IapPack, gp, true);
            }
        }

        public override void SubscribeToPushNotifications(bool showIntro) {
            SubscribeToPushNotifications(showIntro, true, true);
        }

        public virtual void SubscribeToPushNotifications(bool showIntro, bool showDialog, bool showProgress) {
            ShowDialog = showDialog;
            SubscribeToPushNotifications(showIntro, showProgress);
        }
    }
}
