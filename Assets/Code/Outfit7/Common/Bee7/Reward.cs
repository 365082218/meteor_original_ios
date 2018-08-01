//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Text.Localization;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Bee7 {

    /// <summary>
    /// Bee7 Reward.
    /// </summary>
    public class Reward {

        public enum IconUrlSize {
            Small,
            Large
        }

        private string Name;
        private JSONNode L10nNames;
        private string ShortName;
        private JSONNode L10nShortNames;
        private string Description;
        private JSONNode L10nDescriptions;
        private JSONNode SizeIconUrls;
        private string VcName;
        private JSONNode L10nVcNames;
        private JSONNode SizeVcIconUrls;

        public int ThousandthUsdAmount { get; private set; }

        public int VirtualCurrencyAmount { get; private set; }

        public bool IsVideoClip { get; private set; }

        public string AppId { get; private set; }

        public Reward(JSONNode rewardJ) {
            ThousandthUsdAmount = rewardJ["thousandthUsdAmount"].AsInt;
            VirtualCurrencyAmount = rewardJ["virtualCurrencyAmount"].AsInt;
            IsVideoClip = rewardJ["videoReward"].AsBool;
            AppId = rewardJ["appId"];
            Assert.HasText(AppId, "appId");
            Name = rewardJ["name"];
            L10nNames = rewardJ["l10nNames"];
            ShortName = rewardJ["shortName"];
            L10nShortNames = rewardJ["l10nShortNames"];
            Description = rewardJ["description"];
            L10nDescriptions = rewardJ["l10nDescription"];
            SizeIconUrls = rewardJ["sizeIconUrls"];
            VcName = rewardJ["virtualCurrencyName"];
            L10nVcNames = rewardJ["l10nVirtualCurrencyNames"];
            SizeVcIconUrls = rewardJ["sizeVirtualCurrencyIconUrls"];
        }

        public string LocalizedName {
            get {
                if (L10nNames == null)
                    return Name;
                string name = L10n.LocalizationManagerInstance.GetText(L10nNames);
                if (name == null)
                    return Name;
                return name;
            }
        }

        public string LocalizedShortName {
            get {
                if (L10nShortNames == null)
                    return ShortName;
                string name = L10n.LocalizationManagerInstance.GetText(L10nShortNames);
                if (name == null)
                    return ShortName;
                return name;
            }
        }

        public string LocalizedDescription {
            get {
                if (L10nDescriptions == null)
                    return Description;
                string name = L10n.LocalizationManagerInstance.GetText(L10nDescriptions);
                if (name == null)
                    return Description;
                return name;
            }
        }

        public string IconUrl(IconUrlSize size) {
            if (SizeIconUrls == null)
                return null;
            string sizeS;
            switch (size) {
                case IconUrlSize.Small:
                    sizeS = "small";
                    break;
                case IconUrlSize.Large:
                    sizeS = "large";
                    break;
                default:
                    return null;
            }
            return SizeIconUrls[sizeS];
        }

        public string LocalizedVirtualCurrencyName {
            get {
                if (L10nVcNames == null)
                    return VcName;
                string name = L10n.LocalizationManagerInstance.GetText(L10nVcNames);
                if (name == null)
                    return VcName;
                return name;
            }
        }

        public string VirtualCurrencyIconUrl(IconUrlSize size) {
            if (SizeVcIconUrls == null)
                return null;
            string sizeS;
            switch (size) {
                case IconUrlSize.Small:
                    sizeS = "small";
                    break;
                case IconUrlSize.Large:
                    sizeS = "large";
                    break;
                default:
                    return null;
            }
            return SizeVcIconUrls[sizeS];
        }

        public override string ToString() {
            return string.Format("[Reward: ThousandthUsdAmount={0}, VirtualCurrencyAmount={1}, IsVideoClip={2}, AppId={3}, LocalizedName={4}, LocalizedShortName={5}, LocalizedDescription={6}, LocalizedVirtualCurrencyName={7}]",
                ThousandthUsdAmount, VirtualCurrencyAmount, IsVideoClip, AppId, LocalizedName, LocalizedShortName, LocalizedDescription, LocalizedVirtualCurrencyName);
        }
    }
}
