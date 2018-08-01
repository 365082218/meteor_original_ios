//
//   Copyright (c) 2016 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Util;

namespace Outfit7.Achievements {

    public class CollectionAchievement : Achievement<HashSet<string>> {

        public int MaxItems { get; set; }

        public int ItemsCount { get { return Data == null ? 0 : Data.Count; } }

        public float ProgressInPercent { get { return 100f * ItemsCount / MaxItems; } }

        public bool IsIncremental { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Outfit7.Achievements.CollectionAchievement"/> class.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="name">Name.</param>
        /// <param name="maxItems">Max number of items for this collection achievement.</param>
        /// <param name="isIncremental">If set to <c>true</c> this achievement acts as incremental, otherwise it acts as an ordinary achievement.</param>
        public CollectionAchievement(int id, string name, int maxItems, bool isIncremental = false) : base(id, name) {
            Assert.IsTrue(maxItems > 0, "maxItems must be greater than 0!");
            MaxItems = maxItems;
            IsIncremental = isIncremental;
        }

        /// <summary>
        /// Checks and adds an item only if not yet in hashset.
        /// </summary>
        /// <returns><c>true</c>, if item was added to hashset, <c>false</c> otherwise.</returns>
        /// <param name="item">Item.</param>
        public virtual bool CheckAndAddItem(string item) {
            if (IsUnlocked) return false;

            if (Data == null) {
                Data = new HashSet<string>();
            }

            bool added = Data.Add(item);
            Unlock(); // check and unlock achievement
            O7Log.VerboseT(Tag, "Adding item '{0}' for achievement '{1}': added = {2}, current items: {3}/{4} ({5}%)", item, Name, added, ItemsCount, MaxItems, ProgressInPercent);
            return added;
        }

        public override bool Unlock() {
            if (ItemsCount == MaxItems) {
                return base.Unlock();
            }
            return false;
        }

        public override string ToString() {
            return string.Format("{0}; MaxItems={1}, ItemsCount={2}, ProgressInPercent={3}", base.ToString(), MaxItems, ItemsCount, ProgressInPercent);
        }
    }

}

