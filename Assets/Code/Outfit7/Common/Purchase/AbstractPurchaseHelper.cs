//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System.Collections.Generic;
using Outfit7.Bee7;
using Outfit7.Util;

namespace Outfit7.Purchase {

    /// <summary>
    /// Purchase helper.
    /// </summary>
    public abstract class AbstractPurchaseHelper {

        protected const string Tag = "PurchaseHelper";
        private readonly List<Pair<IapPack, int>> AfterAppResumeRewardBubbles = new List<Pair<IapPack, int>>();

        public PublisherHelper Publisher { get; set; }

        public virtual void OnAppResume() {
            foreach (Pair<IapPack, int> pair in AfterAppResumeRewardBubbles) {
                EnqueueRewardBubble(pair.First, pair.Second);
            }
            AfterAppResumeRewardBubbles.Clear();
        }

        protected virtual void SortPurchasePacks(List<PurchasePack> packs) {
            // Sort ascending by GRID IAP position
            packs.Sort(delegate(PurchasePack x, PurchasePack y) {
                return x.GridPack.Position - y.GridPack.Position;
            });
        }

        public virtual void ProcessBee7AppOffer(AppOffer offer) {
            Publisher.StartOffer(offer.Id);
        }

        protected void EnqueueAfterAppResumeRewardBubble(IapPack iap, int amount) {
            AfterAppResumeRewardBubbles.Add(new Pair<IapPack, int>(iap, amount));
        }

        protected abstract void EnqueueRewardBubble(IapPack iap, int amount);
    }
}
