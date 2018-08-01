//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Text.Localization;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.Bee7 {

    /// <summary>
    /// Bee7 application offer.
    /// </summary>
    public class AppOffer {

        public enum IconUrlSize {
            Small,
            Large
        }

        public enum AppOfferState {
            NotConnected,
            NotConnectedPendingInstall,
            Connected
        }

        private string Name;
        private JSONNode L10nNames;
        private string ShortName;
        private JSONNode L10nShortNames;
        private string Description;
        private JSONNode L10nDescriptions;
        private JSONNode SizeIconUrls;

        public bool ShowGameWallTitle { get; private set; }

        public string Id { get; private set; }

        public int Priority { get; private set; }

        public AppOfferState State { get; private set; }

        public AppOffer(JSONNode appOfferJ) {
            Id = appOfferJ["id"];
            Assert.HasText(Id, "id");
            Name = appOfferJ["name"];
            L10nNames = appOfferJ["l10nNames"];
            ShortName = appOfferJ["shortName"];
            L10nShortNames = appOfferJ["l10nShortNames"];
            Description = appOfferJ["description"];
            L10nDescriptions = appOfferJ["l10nDescription"];
            SizeIconUrls = appOfferJ["sizeIconUrls"];
            Priority = appOfferJ["priority"].AsInt;

            JSONNode GameWallTitle = appOfferJ["showGameWallTitle"];

            if (StringUtils.IsNullOrEmpty(GameWallTitle)) {
                ShowGameWallTitle = true; // defaults to true if not defined
            } else {
                ShowGameWallTitle = GameWallTitle.AsBool;
            }

            string stateS = appOfferJ["state"];
            switch (stateS) {
                case "not_connected":
                    State = AppOfferState.NotConnected;
                    break;
                case "not_connected_pending_install":
                    State = AppOfferState.NotConnectedPendingInstall;
                    break;
                case "connected":
                    State = AppOfferState.Connected;
                    break;
            }
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

        public override string ToString() {
            return string.Format("[AppOffer: Id={0}, Priority={1}, State={2}, LocalizedName={3}, LocalizedShortName={4}, LocalizedDescription={5}]",
                                 Id, Priority, State, LocalizedName, LocalizedShortName, LocalizedDescription);
        }
    }
}
