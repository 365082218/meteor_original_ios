//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using Outfit7.Util;

namespace Outfit7.Currency {

    /// <summary>
    /// Currency account.
    /// </summary>
    public class CurrencyAccount {

        public string Id { get; private set; }

        public int Balance { get; private set; }

        public CurrencyAccount(string id, int balance) {
            Id = id;
            Balance = balance;
        }

        public CurrencyAccount(CurrencyAccount account) : this(account.Id, account.Balance) {
        }

        public void Credit(int amount) {
            Balance += amount;
        }

        public void Debit(int amount) {
            Balance -= amount;
        }

        public bool CanDebit(int amount) {
            Assert.IsTrue(amount >= 0, "amount must be >= 0");
            if (amount == 0) {
                // Can always debit zero even if balance is < 0
                return true;
            }
            return amount <= Balance;
        }

        public override string ToString() {
            return string.Format("[CurrencyAccount: Id={0}, Balance={1}]", Id, Balance);
        }
    }
}
