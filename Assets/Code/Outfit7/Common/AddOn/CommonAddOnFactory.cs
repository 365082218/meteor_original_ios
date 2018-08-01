//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Outfit7.Common;
using Outfit7.Json;
using Outfit7.Util;
using SimpleJSON;
using UnityEngine;

namespace Outfit7.AddOn {

    /// <summary>
    /// Common add-on factory.
    /// </summary>
    public abstract class CommonAddOnFactory : AddOnFactory {

        protected const string Tag = "AddOnFactory";
        public const string BundledJsonFile = "addons.json";

        protected Version AppVersion { get; set; }

        public virtual void Init() {
            AppVersion = VersionUtils.ParseMissingToZero(AppPlugin.AppVersion);
        }

        public abstract AddOnItem CreateDevelAddOn(string id, int position);

        public virtual IDictionary<string, AddOnItem> UnmarshalBundledAddOns() {
            TextAsset jsonFile = Resources.Load(BundledJsonFile) as TextAsset;
            string json = jsonFile.text;

            JSONNode itemsJ = JSON.Parse(json);

            return UnmarshalAddOns(itemsJ);
        }

        protected IDictionary<string, AddOnItem> UnmarshalAddOns(JSONNode itemsJ) {
            Dictionary<string, AddOnItem> itemMap = new Dictionary<string, AddOnItem>(itemsJ.Count);

            for (int i = 0; i < itemsJ.Count; i++) {
                JSONNode itemJ = itemsJ[i];
                AddOnItem item = UnmarshalAddOn(itemJ, i);
                if (item == null) {
                    //O7Log.VerboseT(Tag, "Add-on filtered out: {0}", itemJ);
                    continue;
                }
                try {
                    itemMap.Add(item.Id, item);

                } catch (ArgumentException) {
                    throw new ArgumentException("Add-on '" + item.Id + "' already exists");
                }
                if (item.IsIgnored) {
                    O7Log.VerboseT(Tag, "Add-on recognized as ignored: {0}", item);
                }
            }

            return itemMap;
        }

        public virtual IEnumerator<Null> UnmarshalCachedAddOns(JSONNode cachedData, Action<IDictionary<string, AddOnItem>> callback) {
            O7Log.DebugT(Tag, "Unmarshalling add-ons from cached data...");

            JSONArray itemsJ = SimpleJsonUtils.EnsureJsonArray(cachedData);
            if (itemsJ == null || itemsJ.Count == 0) {
                O7Log.WarnT(Tag, "Corrupt or missing add-ons cached data");
                yield break;
            }

            IEnumerator<Null> umarshaller = UnmarshalAddOns(itemsJ, callback);
            while (umarshaller.MoveNext()) {
                yield return null;
            }

            O7Log.DebugT(Tag, "Unmarshalled add-ons from cached data");
        }

        protected IEnumerator<Null> UnmarshalAddOns(JSONArray itemsJ, Action<IDictionary<string, AddOnItem>> callback) {
            Dictionary<string, AddOnItem> itemMap = new Dictionary<string, AddOnItem>(itemsJ.Count);

            for (int i = 0; i < itemsJ.Count; i++) {
                JSONNode itemJ = itemsJ[i];
                yield return null;

                AddOnItem item;
                try {
                    item = UnmarshalAddOn(itemJ, i);

                } catch (Exception e) {
                    O7Log.WarnT(Tag, e, "Cannot unmarshal add-on: {0}", itemJ);
                    continue;
                }
                if (item == null) {
                    //O7Log.VerboseT(Tag, "Add-on filtered out: {0}", itemJ);
                    continue;
                }
                try {
                    itemMap.Add(item.Id, item);

                } catch (ArgumentException e) {
                    O7Log.WarnT(Tag, e, "Add-on {0} already exists", item.Id);
                }
                if (item.IsIgnored) {
                    O7Log.VerboseT(Tag, "Add-on recognized as ignored: {0}", item);
                }
            }

            callback(itemMap);
        }

        protected abstract AddOnItem UnmarshalAddOn(JSONNode itemJ, int position);

        protected virtual Version ParseVersion(string version) {
            if (!StringUtils.HasText(version)) return null;
            return VersionUtils.ParseMissingToZero(version);
        }

        protected virtual bool TryParseAndCheckMinVersion(string minVersionS, out Version minVersion) {
            minVersion = ParseVersion(minVersionS);
            if (minVersion == null) return true;
            return AppVersion >= minVersion;
        }

        protected virtual bool TryParseAndCheckMaxVersion(string maxVersionS, out Version maxVersion) {
            maxVersion = ParseVersion(maxVersionS);
            if (maxVersion == null) return true;
            return AppVersion < maxVersion;
        }

        protected virtual bool TryParseAndCheckBlackListedPlatformRegex(string pattern, out Regex regex) {
            if (!StringUtils.HasText(pattern)) {
                regex = null;
                return false;
            }
            Assert.IsTrue(pattern.Length <= 512, "Black-listed platform regex pattern length > 512: {0}", pattern.Length);
            regex = new Regex(pattern,
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return regex.IsMatch(AppPlugin.Platform);
        }

        public virtual void UpdateAddOn(AddOnItem bundledItem, AddOnItem gridItem) {
            bundledItem.DefaultName = gridItem.DefaultName;
            bundledItem.NameL10nMap = gridItem.NameL10nMap;
            bundledItem.DefaultDescription = gridItem.DefaultDescription;
            bundledItem.DescriptionL10nMap = gridItem.DescriptionL10nMap;
            bundledItem.CurrencyId = gridItem.CurrencyId;
            bundledItem.Price = gridItem.Price;
            bundledItem.DiscountPercentage = gridItem.DiscountPercentage;
            bundledItem.UnlockLevel = gridItem.UnlockLevel;
            bundledItem.Booster = gridItem.Booster;
            bundledItem.MeterGain = gridItem.MeterGain;
            bundledItem.MinAppVersion = gridItem.MinAppVersion;
            bundledItem.MaxAppVersion = gridItem.MaxAppVersion;
            bundledItem.BlackListedPlatformRegex = gridItem.BlackListedPlatformRegex;
            bundledItem.IsIgnored = gridItem.IsIgnored;
            bundledItem.Position = gridItem.Position;
        }
    }
}
