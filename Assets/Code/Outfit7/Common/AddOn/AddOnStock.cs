//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.AddOn {

    /// <summary>
    /// Add-on stock.
    /// </summary>
    public class AddOnStock {

        public Dictionary<string, int> BoughtItems { get; private set; }

        public HashSet<string> EnabledItems { get; private set; }

        public HashSet<string> NewlyUnlockedItems { get; private set; }

        public AddOnStock() : this(null, null, null) {
        }

        public AddOnStock(AddOnStock stock) {
            BoughtItems = new Dictionary<string, int>(stock.BoughtItems);
            EnabledItems = new HashSet<string>(stock.EnabledItems);
            NewlyUnlockedItems = new HashSet<string>(stock.NewlyUnlockedItems);
        }

        public AddOnStock(ICollection<string> boughtItems, ICollection<string> enabledIds, ICollection<string> newlyUnlockedIds) {
            BoughtItems = ConvertCompactBoughtItems(boughtItems);
            EnabledItems = (enabledIds != null) ? new HashSet<string>(enabledIds) : new HashSet<string>();
            NewlyUnlockedItems = (newlyUnlockedIds != null) ? new HashSet<string>(newlyUnlockedIds) : new HashSet<string>();
        }

#region Bought

        internal void SetBoughtItem(AddOnItem item) {
            if (item.Stock <= 0) {
                BoughtItems.Remove(item.Id);
            } else {
                BoughtItems[item.Id] = item.Stock;
            }
        }

        public List<string> GetCompactBoughtItems() {
            List<string> boughtItemsS = new List<string>(BoughtItems.Count);
            foreach (KeyValuePair<string, int> entry in BoughtItems) {
                string entryS = StringUtils.CombineStringWithCount(entry.Key, entry.Value);
                boughtItemsS.Add(entryS);
            }
            return boughtItemsS;
        }

        private Dictionary<string, int> ConvertCompactBoughtItems(ICollection<string> boughtItemsS) {
            if (boughtItemsS == null) {
                return new Dictionary<string, int>();
            }

            Dictionary<string, int> boughtItems = new Dictionary<string, int>(boughtItemsS.Count);
            foreach (string entryS in boughtItemsS) {
                string id;
                int stock;
                StringUtils.TryParsingCombinedStringWithCount(entryS, out id, out stock);
                if (stock == 0) {
                    // To prevent useless stock from data
                    continue;
                }
                if (!StringUtils.HasText(id)) {
                    // To prevent empty add-on IDs from data
                    continue;
                }
                boughtItems[id] = stock;
            }
            return boughtItems;
        }

#endregion

#region Enabled

        internal bool AddEnabledItem(AddOnItem item) {
            return EnabledItems.Add(item.Id);
        }

        internal bool RemoveEnabledItem(AddOnItem item) {
            return EnabledItems.Remove(item.Id);
        }

#endregion

#region Newly Unlocked

        internal bool AddNewlyUnlockedItem(AddOnItem item) {
            return NewlyUnlockedItems.Add(item.Id);
        }

        internal bool RemoveNewlyUnlockedItem(AddOnItem item) {
            return NewlyUnlockedItems.Remove(item.Id);
        }

        public override string ToString() {
            return string.Format("[AddOnStock: BoughtItems=[{0}], EnabledItems=[{1}], NewlyUnlockedItems=[{2}]]",
                StringUtils.CollectionToCommaDelimitedString(BoughtItems),
                StringUtils.CollectionToCommaDelimitedString(EnabledItems),
                StringUtils.CollectionToCommaDelimitedString(NewlyUnlockedItems));
        }

#endregion
    }
}
