using SimpleJSON;
using Outfit7.Store.Iap;
using Outfit7.Purchase;
using Outfit7.Util;

namespace Outfit7.Promo.SpecialOffers {
    public class IapSpecialOffer : DiscountSpecialOffer {

        private const string JsonOriginalPackId = "oPId";
        private const string JsonDiscountedPackId = "dPId";

        public StoreIapPackManager StoreIapPackManager { get; set; }

        public PurchasePack PurchasePack { get; set; }

        public string OriginalIapPackId { get; private set; }

        private StoreIapPack originalStoreIapPack;

        public StoreIapPack OriginalStoreIapPack {
            get {
                if (originalStoreIapPack != null) return originalStoreIapPack;

                if (!StoreIapPackManager.Ready) return null;
                StoreIapPack pack = StoreIapPackManager.GetPack(OriginalIapPackId);
                if (pack == null) return null;

                originalStoreIapPack = pack;
                return pack;
            }
        }

        public string DiscountedIapPackId { get; private set; }

        private StoreIapPack discountedStoreIapPack;

        public StoreIapPack DiscountedStoreIapPack {
            get {
                if (discountedStoreIapPack != null) return discountedStoreIapPack;

                if (!StoreIapPackManager.Ready) return null;
                StoreIapPack pack = StoreIapPackManager.GetPack(DiscountedIapPackId);
                if (pack == null) return null;

                discountedStoreIapPack = pack;
                return pack;
            }
        }

        public IapSpecialOffer(JSONNode rawData, string cachePath) : base(rawData, cachePath) {
            OriginalIapPackId = rawData[JsonOriginalPackId];
            DiscountedIapPackId = rawData[JsonDiscountedPackId];
        }

        public override JSONClass ToJson() {
            JSONClass j = base.ToJson();
            j[JsonOriginalPackId] = OriginalIapPackId;
            j[JsonDiscountedPackId] = DiscountedIapPackId;
            return j;
        }

        public override bool IsValid {
            get {
                if (!base.IsValid) return false;
                if (string.IsNullOrEmpty(OriginalIapPackId)) {
                    O7Log.DebugT(Tag, "OriginalIapPackId not defined");
                    return false;
                }
                if (string.IsNullOrEmpty(DiscountedIapPackId)) {
                    O7Log.DebugT(Tag, "DiscountedIapPackId not defined");
                    return false;
                }
                return true;
            }
        }

        public override bool CanActivate {
            get {
                if (!base.CanActivate) return false;
                if (!StoreIapPackManager.Ready) return false;
                if (OriginalStoreIapPack == null) return false;
                if (DiscountedStoreIapPack == null) return false;
                return true;
            }
        }

        public override void InvalidateStateData() {
            base.InvalidateStateData();
            originalStoreIapPack = null;
            discountedStoreIapPack = null;
        }

        protected override string NewPriceText {
            get {
                if (DiscountedStoreIapPack == null) return null;
                return DiscountedStoreIapPack.FormattedPrice;
            }
        }

        protected override string OldPriceText {
            get {
                if (OriginalStoreIapPack == null) return null;
                return OriginalStoreIapPack.FormattedPrice;
            }
        }

        public override string ToString() {
            return string.Format("[IapSpecialOffer: OriginalIapPackId={0}, DiscountedIapPackId={1}, {2}]", OriginalIapPackId, DiscountedIapPackId, base.ToString());
        }
    }
}
