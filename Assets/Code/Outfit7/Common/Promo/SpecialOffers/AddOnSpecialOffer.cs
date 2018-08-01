//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using SimpleJSON;
using Outfit7.AddOn;
using Outfit7.Util;
using System;

namespace Outfit7.Promo.SpecialOffers {

    /// <summary>
    /// Add-on special offer pack.
    /// </summary>
    public class AddOnSpecialOffer : DiscountSpecialOffer {

        private const string JsonAddOnId = "iId";

        public string AddOnId { get; private set; }

        public AddOnManager AddOnManager { get; set; }

        private AddOnItem addOnItem;

        public AddOnItem AddOnItem {
            get {
                if (AddOnId == null) return null;
                if (addOnItem != null) return addOnItem;

                addOnItem = AddOnManager.GetAddOn(AddOnId);
                return addOnItem;
            }
        }

        public AddOnSpecialOffer(JSONNode rawData, string cachePath) : base(rawData, cachePath) {
            AddOnId = rawData[JsonAddOnId];
        }

        public override JSONClass ToJson() {
            JSONClass j = base.ToJson();
            j[JsonAddOnId] = AddOnId;
            return j;
        }

        public override bool CanActivate {
            get {
                if (!base.CanActivate) return false;

                // check the status of the addon
                if (AddOnItem == null) return false;
                if (AddOnItem.IsBought) return false; // Already bought
                if (AddOnItem.Price <= 0) return false; // Free
                return true;
            }
        }

        public override bool IsValid {
            get {
                if (!base.IsValid) return false;

                // check the status of the addon
                if (!StringUtils.HasText(AddOnId)) {
                    O7Log.DebugT(Tag, "AddOnId not defined");
                    return false;
                }

                if (!StringUtils.HasText(ActionUrl)) {
                    O7Log.DebugT(Tag, "ActionUrl not defined");
                    return false;
                }

                return true;
            }
        }

        public override void InvalidateStateData() {
            base.InvalidateStateData();
            addOnItem = null;
        }

        protected override string NewPriceText {
            get {
                return NewPrice.ToString();
            }
        }

        public int NewPrice {
            get {
                int discountP = DiscountPercentage;
                if (discountP == 100) return 0; // To avoid rounding issues
                int price = AddOnItem.Price;
                return price - (int) (0.01 * price * discountP);
            }
        }

        protected override string OldPriceText {
            get {
                return OldPrice.ToString();
            }
        }

        public int OldPrice {
            get {
                return AddOnItem.Price;
            }
        }

        public override string ToString() {
            return string.Format("[AddOnSpecialOffer: AddOnId={0}, SpecialOffer={1}]", AddOnId, base.ToString());
        }
    }
}
