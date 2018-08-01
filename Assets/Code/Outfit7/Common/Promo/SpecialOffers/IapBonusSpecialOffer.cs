using Outfit7.Grid.Iap;
using Outfit7.Purchase;
using Outfit7.Store.Iap;
using SimpleJSON;
using Outfit7.Util;

namespace Outfit7.Promo.SpecialOffers {
    public class IapBonusSpecialOffer : SpecialOffer {

        private const string JsonOriginalPackId = "oPId";
        private const string JsonIapAmountOverride = "iAPAO";

        public StoreIapPackManager StoreIapPackManager { get; set; }

        public PurchasePack PurchasePack { get; set; }

        public string OriginalIapPackId { get; private set; }

        public int BonusAmount { get; private set; }

        private StoreIapPack originalStoreIapPack;

        public IapBonusSpecialOffer(JSONNode rawData, string cachePath) : base(rawData, cachePath) {
            OriginalIapPackId = rawData[JsonOriginalPackId];
            BonusAmount = rawData[JsonIapAmountOverride].AsInt;
        }

        public StoreIapPack OriginalStoreIapPack {
            get {
                if (originalStoreIapPack != null) return originalStoreIapPack;

                if (!StoreIapPackManager.Ready) return null;
                originalStoreIapPack = StoreIapPackManager.GetPack(OriginalIapPackId);
                return originalStoreIapPack;
            }
        }

        public override JSONClass ToJson() {
            JSONClass j = base.ToJson();
            j[JsonOriginalPackId] = OriginalIapPackId;
            j[JsonIapAmountOverride].AsInt = BonusAmount;
            return j;
        }

        public override bool IsValid {
            get {
                if (!base.IsValid) return false;
                if (string.IsNullOrEmpty(OriginalIapPackId)) {
                    O7Log.DebugT(Tag, "OriginalIapPackId is not defined");
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
                if (BonusAmount <= 0) return false;
                return true;
            }
        }

        public override void InvalidateStateData() {
            base.InvalidateStateData();
            originalStoreIapPack = null;
        }

        public override string ToString() {
            return string.Format("[IapBonusSpecialOffer: StoreIapPackManager={0}, PurchasePack={1}, OriginalIapPackId={2}, OriginalStoreIapPack={3}, IsValid={4}, CanActivate={5}, base={6}]", StoreIapPackManager, PurchasePack, OriginalIapPackId, OriginalStoreIapPack, IsValid, CanActivate, base.ToString());
        }
    }
}
