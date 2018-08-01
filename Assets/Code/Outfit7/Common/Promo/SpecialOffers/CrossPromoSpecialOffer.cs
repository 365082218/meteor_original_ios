//
//   Copyright (c) 2015 Outfit7. All rights reserved.
//

using SimpleJSON;
using Outfit7.Util;
using Outfit7.Common;

namespace Outfit7.Promo.SpecialOffers {

    /// <summary>
    /// Cross promo special offer pack.
    /// </summary>
    public class CrossPromoSpecialOffer : SpecialOffer {

        private const string JsonAppId = "iId";
        private const string JsonCurrencyType = "cT";
        private const string JsonCurrencyAmount = "cA";
        private const string JsonCurrencyAmountColor = "aC";
        private const string JsonRewardGiven = "_gC"; // local parameter

        public string AppId { get; private set; }

        public delegate void GiveRewardDelegate();

        public GiveRewardDelegate GiveReward { get; set; }

        private bool RewardGiven { get; set; }

        public string CurrencyType { get; private set; }

        public int CurrencyAmount { get; private set; }

        public string CurrencyAmountColor { get; private set; }

        private bool IsAppInstalled { get; set; }

        public CrossPromoSpecialOffer(JSONNode rawData, string cachePath) : base(rawData, cachePath) {
            AppId = rawData[JsonAppId];
            CurrencyType = rawData[JsonCurrencyType];
            CurrencyAmount = rawData[JsonCurrencyAmount].AsInt;
            CurrencyAmountColor = rawData[JsonCurrencyAmountColor];
            RewardGiven = rawData[JsonRewardGiven].AsBool;
        }

        public override JSONClass ToJson() {
            JSONClass j = base.ToJson();
            j[JsonAppId] = AppId;
            j[JsonCurrencyType] = CurrencyType;
            j[JsonCurrencyAmount].AsInt = CurrencyAmount;
            j[JsonCurrencyAmountColor] = CurrencyAmountColor;
            j[JsonRewardGiven].AsBool = RewardGiven;
            return j;
        }

        public override bool CanActivate {
            get {
                if (!base.CanActivate) return false;
                if (RewardGiven) return false;
                if (IsAppInstalled) return false;

                return true;
            }
        }

        public override bool IsValid {
            get {
                if (!base.IsValid) return false;
                if (!StringUtils.HasText(AppId)) {
                    O7Log.DebugT(Tag, "AppId not defined");
                    return false;
                }

                // check if currency defined
                if (CurrencyAmount < 0) {
                    O7Log.DebugT(Tag, "CurrencyAmount < 0");
                    return false;
                }
                if (!StringUtils.HasText(CurrencyType)) {
                    O7Log.DebugT(Tag, "CurrencyType not defined");
                    return false;
                }

                return true;
            }
        }

        public override void OnAppStartOrResume(bool activated) {
            base.OnAppStartOrResume(activated);

            IsAppInstalled = AppPlugin.IsAppInstalled(AppId);

            if (!activated) return;
            if (RewardGiven) return;
            if (!IsAppInstalled) return;

            RewardGiven = true;

            if (GiveReward != null) {
                GiveReward();
            }
        }

        public override string ToString() {
            return string.Format("[CrossPromoSpecialOffer: AppId={0}, GiveReward={1}, CurrencyType={2}, CurrencyAmount={3}, CurrencyAmountColor={4}, Offer={5}]", AppId, GiveReward, CurrencyType, CurrencyAmount, CurrencyAmountColor, base.ToString());
        }
    }
}
