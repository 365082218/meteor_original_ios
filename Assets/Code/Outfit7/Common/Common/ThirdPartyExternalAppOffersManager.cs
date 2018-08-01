//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Event;
using Outfit7.Purchase;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Common {

    /// <summary>
    /// 3rd-party external application offers manager.
    /// </summary>
    public class ThirdPartyExternalAppOffersManager {

        private bool thirdPartyExternalAppOffersAvailable;

        public EventBus EventBus { get; set; }

        public ThirdPartyExternalAppOffersPlugin ThirdPartyExternalAppOffersPlugin { get; set; }

        public AbstractPurchaseManager PurchaseManager { get; set; }

        public virtual bool ThirdPartyExternalAppOffersAvailable {
            get {
                return this.thirdPartyExternalAppOffersAvailable;
            }
            internal set {
                if (this.thirdPartyExternalAppOffersAvailable == value)
                    return;
                this.thirdPartyExternalAppOffersAvailable = value;

                EventBus.FireEvent(CommonEvents.EXTERNAL_OFFER_AVAIL_CHANGE, value);
            }
        }

        public virtual void ShowExternalAppOffers() {
            Assert.IsTrue(ThirdPartyExternalAppOffersAvailable, "3rd-party external app offers are not available");
            ThirdPartyExternalAppOffersPlugin.StartShowingExternalAppOffers();
        }

        internal virtual void OnExternalAppOfferCompletion(string data) {
            JSONNode dataJ = JSON.Parse(data);
            JSONNode rewardJ = dataJ[0]; // Only one node
            string id = rewardJ.Key;
            int amount = rewardJ.AsInt;

            PurchaseManager.Reward3rdPartyExternalAppOffer(id, amount);
        }
    }
}
