//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

namespace Outfit7.Purchase {

    /// <summary>
    /// The in-app product pack.
    /// </summary>
    public class IapPack {

        public string Id { get; private set ; }

        public bool IsForMoney { get; private set; }

        public bool IsConsumable { get; private set; }

        public string CurrencyId { get; private set; }

        public int DiscountPercentage { get; private set; }

        public bool IsForCurrency {
            get {
                return CurrencyId != null;
            }
        }

        public IapPack(string id, bool forMoney, bool consumable, string currencyId) : this(id, forMoney, consumable, currencyId, 0) {
        }

        public IapPack(string id, bool forMoney, bool consumable, string currencyId, int discountPercentage) {
            Id = id;
            IsForMoney = forMoney;
            IsConsumable = consumable;
            CurrencyId = currencyId;
            DiscountPercentage = discountPercentage;
        }

        public override string ToString() {
            return string.Format("[IapPack: Id={0}, IsForMoney={1}, IsConsumable={2}, CurrencyId={3}, IsForCurrency={4}, DiscountPercentage={5}]",
                Id, IsForMoney, IsConsumable, CurrencyId, IsForCurrency, DiscountPercentage);
        }
    }
}
