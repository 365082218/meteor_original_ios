//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Text.RegularExpressions;
using Outfit7.Text.Localization;
using Outfit7.Util;
using SimpleJSON;

namespace Outfit7.AddOn {

    /// <summary>
    /// Add-on item state.
    /// </summary>
    public enum AddOnItemState {
        // Not bought

        // Add-on is not bought.
        NotBought,

        // Ready

        // Add-on is bought. It is ready to be used (to be enabled).
        Ready,
        // Add-on is being in use right now (is enabled).
        Enabled,
    }

    /// <summary>
    /// Add-on item.
    /// </summary>
    public class AddOnItem {

        private int stock;

        public string Id { get; private set; }

        public string DefaultName { get; internal set; }

        public JSONNode NameL10nMap { get; internal set; }

        public string DefaultDescription { get; internal set; }

        public JSONNode DescriptionL10nMap { get; internal set; }

        public string CurrencyId { get; internal set; }

        public int Price { get; internal set; }

        public int DiscountPercentage { get; internal set; }

        public int UnlockLevel { get; internal set; }

        public int Booster { get; internal set; }

        public int MeterGain { get; internal set; }

        public Version MinAppVersion { get; internal set; }

        public Version MaxAppVersion { get; internal set; }

        public Regex BlackListedPlatformRegex { get; internal set; }

        public bool IsIgnored { get; internal set; }

        public int Position { get; internal set; }

        public AddOnItemState State { get; internal set; }

        public int Stock {
            get {
                return this.stock;
            }
            internal set {
                Assert.IsTrue(value >= 0, "stock must be >= 0");
                this.stock = value;
            }
        }

        public bool IsBought {
            get {
                return State == AddOnItemState.Ready || State == AddOnItemState.Enabled;
            }
        }

        public bool IsEnabled {
            get {
                return State == AddOnItemState.Enabled;
            }
        }

        public AddOnItem(string id, string defaultName, JSONNode nameL10nMap, string defaultDescription,
            JSONNode descriptionL10nMap, string currencyId, int price, int discountPercentage, int unlockLevel,
            int booster, int meterGain, Version minAppVersion, Version maxAppVersion, Regex blackListedPlatformRegex,
            bool ignored, int position) {
            Assert.HasText(id, "id");
            Assert.HasText(defaultName, "defaultName");
            Assert.HasText(currencyId, "currencyId");
            Assert.IsTrue(price >= 0, "price must be >= 0");
            Assert.IsTrue(discountPercentage >= 0 && discountPercentage <= 100, "discountPercentage must be in [0, 100]");

            Id = id;
            DefaultName = defaultName;
            NameL10nMap = nameL10nMap;
            DefaultDescription = defaultDescription;
            DescriptionL10nMap = descriptionL10nMap;
            CurrencyId = currencyId;
            Price = price;
            DiscountPercentage = discountPercentage;
            UnlockLevel = unlockLevel;
            Booster = booster;
            MeterGain = meterGain;
            MinAppVersion = minAppVersion;
            MaxAppVersion = maxAppVersion;
            BlackListedPlatformRegex = blackListedPlatformRegex;
            IsIgnored = ignored;
            Position = position;
            State = AddOnItemState.NotBought;
        }

        public bool IsLockedForLevel(int level) {
            return level < UnlockLevel;
        }

        public virtual string GetLocalizedName(params object[] args) {
            if (NameL10nMap == null) return DefaultName;
            string name = L10n.LocalizationManagerInstance.GetText(NameL10nMap, true, args);
            return name ?? DefaultName;
        }

        public virtual string GetLocalizedDescription(params object[] args) {
            if (DescriptionL10nMap == null) return DefaultDescription;
            string desc = L10n.LocalizationManagerInstance.GetText(DescriptionL10nMap, true, args);
            return desc ?? DefaultDescription;
        }

        public override string ToString() {
            return string.Format("[AddOnItem: Id={0}, DefaultName={1}, NameL10nMap={2}, DefaultDescription={3}, DescriptionL10nMap={4}, CurrencyId={5}, Price={6}, DiscountPercentage={7}, UnlockLevel={8}, Booster={9}, MeterGain={10}, MinAppVersion={11}, MaxAppVersion={12}, BlackListedPlatformRegex={13}, IsIgnored={14}, Position={15}, State={16}, Stock={17}]",
                Id, DefaultName, NameL10nMap, DefaultDescription, DescriptionL10nMap, CurrencyId, Price, DiscountPercentage, UnlockLevel, Booster, MeterGain, MinAppVersion, MaxAppVersion, BlackListedPlatformRegex, IsIgnored, Position, State, Stock);
        }
    }
}
