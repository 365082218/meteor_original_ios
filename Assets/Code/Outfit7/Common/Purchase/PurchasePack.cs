//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using Outfit7.Grid.Iap;
using Outfit7.Store.Iap;

namespace Outfit7.Purchase {

    /// <summary>
    /// The complete purchase pack.
    /// </summary>
    public class PurchasePack {

        public IapPack IapPack { get; private set; }

        public GridIapPack GridPack { get; private set; }

        public StoreIapPack StorePack { get; private set; }

        public StoreIapPack AdditionalStorePack { get; private set; }

        private bool isFirstDoubble = false;
        public bool FirstDoubble {
            get {
                return isFirstDoubble;
            }
            set {
                isFirstDoubble = value;
            }
        }
        public PurchasePack(IapPack iap, GridIapPack gridPack = null, StoreIapPack storePack = null,
            StoreIapPack additionalStorePack = null) {
            IapPack = iap;
            GridPack = gridPack;
            StorePack = storePack;
            AdditionalStorePack = additionalStorePack;
        }

        public override string ToString() {
            return string.Format("[PurchasePack: IapPack={0}, GridPack={1}, StorePack={2}, AdditionalStorePack={3}]",
                IapPack, GridPack, StorePack, AdditionalStorePack);
        }
    }
}
