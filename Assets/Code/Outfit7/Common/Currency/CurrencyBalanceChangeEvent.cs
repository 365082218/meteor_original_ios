//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

namespace Outfit7.Currency {

    /// <summary>
    /// Currency balance change event.
    /// </summary>
    public struct CurrencyBalanceChangeEvent {

        public CurrencyAccount Account;
        public int OldBalance;

        public CurrencyBalanceChangeEvent(CurrencyAccount account, int oldBalance) {
            Account = account;
            OldBalance = oldBalance;
        }

        public override string ToString() {
            return string.Format("[CurrencyBalanceChangeEvent: Account={0}, OldBalance={1}]", Account, OldBalance);
        }
    }
}
