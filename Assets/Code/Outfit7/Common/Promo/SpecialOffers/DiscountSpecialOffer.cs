//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using SimpleJSON;
using Outfit7.Util;

namespace Outfit7.Promo.SpecialOffers {

    /// <summary>
    /// Special offer pack.
    /// </summary>
    public abstract class DiscountSpecialOffer : SpecialOffer {

        private const string JsonDiscountPercentage = "dP";
        private const string JsonCurrencyOldPriceStrikeThroughColor = "oPSC";

        public int DiscountPercentage { get; private set; }

        public string OldPriceStrikeThroughColor { get; private set; }

        protected DiscountSpecialOffer(JSONNode rawData, string cachePath) : base(rawData, cachePath) {
            DiscountPercentage = rawData[JsonDiscountPercentage].AsInt;
            OldPriceStrikeThroughColor = rawData[JsonCurrencyOldPriceStrikeThroughColor];
        }

        public override JSONClass ToJson() {
            JSONClass j = base.ToJson();
            j[JsonDiscountPercentage].AsInt = DiscountPercentage;
            j[JsonCurrencyOldPriceStrikeThroughColor] = OldPriceStrikeThroughColor;
            return j;
        }

        public override bool IsValid {
            get {
                if (!base.IsValid) return false;
                if (DiscountPercentage <= 0) {
                    O7Log.DebugT(Tag, "DiscountPercentage <= 0");
                    return false;
                }
                if (DiscountPercentage > 100) { // Allow 100% discount
                    O7Log.DebugT(Tag, "DiscountPercentage > 100");
                    return false;
                }

                return true;
            }
        }

        public void UpdatePrices() {
            OldPriceLabel.UpdateText(OldPriceText);
            NewPriceLabel.UpdateText(NewPriceText);
        }

        protected abstract string NewPriceText { get; }

        protected abstract string OldPriceText { get; }

        public override string ToString() {
            return string.Format("[DiscountSpecialOffer: DiscountPercentage={0}, OldPriceStrikeThroughColor={1}, Offer={2}]", DiscountPercentage, OldPriceStrikeThroughColor, base.ToString());
        }
    }
}
