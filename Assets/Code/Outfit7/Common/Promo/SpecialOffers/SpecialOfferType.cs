using SimpleJSON;

namespace Outfit7.Promo.SpecialOffers {

    public class SpecialOfferType {

        public static readonly SpecialOfferType AddOn = new SpecialOfferType("ADD_ON");
        public static readonly SpecialOfferType Iap = new SpecialOfferType("IN_APP_PURCHASE");
        public static readonly SpecialOfferType IapBonus = new SpecialOfferType("IN_APP_PURCHASE_BONUS");
        public static readonly SpecialOfferType CrossPromo = new SpecialOfferType("CROSS_PROMO");

        public string StringType;

        public SpecialOfferType(string type){
            StringType = type;
        }

        public static SpecialOfferType GetType(JSONNode rawData) {
            if (rawData == null) return null;

            string type = rawData[SpecialOffer.JsonType];
            if (AddOn.StringType == type) return AddOn;
            if (Iap.StringType == type) return Iap;
            if (CrossPromo.StringType == type) return CrossPromo;
            if (IapBonus.StringType == type) return IapBonus;

            return null;
        }
    }
}
