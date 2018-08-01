//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using NUnit.Framework;
using Outfit7.Analytics.BigQuery;
using Outfit7.Analytics.Firebase;

namespace Outfit7.Currency {

    [TestFixture]
    public class CurrencyManagerTest {

        private class BigQueryTrackerMock : BigQueryTracker {

            public string EventId;
            public string ItemId;
            public string CurrencyId;
            public int Amount;
            public int Balance;

            public override IBigQueryEventBuilder CreateCurrencyBuilder(string eventId, string itemId,
                string currencyId, int amount, int balance) {
                Assert.AreEqual(EventId, eventId);
                Assert.AreEqual(ItemId, itemId);
                Assert.AreEqual(CurrencyId, currencyId);
                Assert.AreEqual(Amount, amount);
                Assert.AreEqual(Balance, balance);
                return new DummyBigQueryEventBuilder();
            }
        }

        private class FirebaseTrackerMock : FirebaseTracker {

            public string ItemName;
            public string CurrencyName;
            public int Amount;

            public override void LogEarnCurrencyEvent(string itemName, string currencyName, int amount) {
                Assert.AreEqual(ItemName, itemName);
                Assert.AreEqual(CurrencyName, currencyName);
                Assert.AreEqual(Amount, amount);
            }

            public override void LogSpendCurrencyEvent(string itemName, string currencyName, int amount) {
                Assert.AreEqual(ItemName, itemName);
                Assert.AreEqual(CurrencyName, currencyName);
                Assert.AreEqual(Amount, amount);
            }
        }

        private class CurrencyManagerMock : CurrencyManager {

            public bool ShouldCallWriteChanges;
            private bool WriteChangesCalled;

            public bool ShouldCallCheckAndFireBalanceChangeEvent;
            public CurrencyAccount CheckAndFireBalanceChangeEventAccount;
            public int CheckAndFireBalanceChangeEventOldBalance;
            private bool CheckAndFireBalanceChangeEventCalled;

            public bool ShouldCallAddReceiptEvent;
            public string AddReceiptEventReceiptData;
            public string AddReceiptEventEventItemId;
            private bool AddReceiptEventCalled;

            public override bool WriteChanges(bool force = false) {
                Assert.IsTrue(ShouldCallWriteChanges);
                Assert.IsFalse(WriteChangesCalled, "Already called");
                WriteChangesCalled = true;
                return true;
            }

            protected override void CheckAndFireBalanceChangeEvent(CurrencyAccount account, int oldBalance) {
                Assert.IsTrue(ShouldCallCheckAndFireBalanceChangeEvent);
                Assert.AreSame(CheckAndFireBalanceChangeEventAccount, account);
                Assert.AreEqual(CheckAndFireBalanceChangeEventOldBalance, oldBalance);
                Assert.IsFalse(CheckAndFireBalanceChangeEventCalled, "Already called");
                CheckAndFireBalanceChangeEventCalled = true;
            }

            protected override void AddReceiptEvent(string receiptData, string eventItemId) {
                Assert.IsTrue(ShouldCallAddReceiptEvent);
                Assert.AreEqual(AddReceiptEventReceiptData, receiptData);
                Assert.AreEqual(AddReceiptEventEventItemId, eventItemId);
                Assert.IsFalse(AddReceiptEventCalled, "Already called");
                AddReceiptEventCalled = true;
            }

            public void Validate() {
                Assert.AreEqual(ShouldCallWriteChanges, WriteChangesCalled);
                Assert.AreEqual(ShouldCallCheckAndFireBalanceChangeEvent, CheckAndFireBalanceChangeEventCalled);
                Assert.AreEqual(ShouldCallAddReceiptEvent, AddReceiptEventCalled);
            }
        }

        private CurrencyManagerMock Mng;
        private BigQueryTrackerMock BqTracker;
        private FirebaseTrackerMock FirebaseTracker;

        [SetUp]
        public void SetUp() {
            Mng = new CurrencyManagerMock();
            BqTracker = new BigQueryTrackerMock();
            FirebaseTracker = new FirebaseTrackerMock();
            Mng.BqTracker = BqTracker;
            Mng.FirebaseTracker = FirebaseTracker;
        }

        [TearDown]
        public void TearDown() {
            Mng.Validate();
        }

#region Free

        [Test]
        public void TestGotFreeCurrency1() {
            CurrencyAccount acc = new CurrencyAccount("test", 1);
            Mng.ShouldCallCheckAndFireBalanceChangeEvent = true;
            Mng.CheckAndFireBalanceChangeEventAccount = acc;
            Mng.CheckAndFireBalanceChangeEventOldBalance = 1;
            Mng.GotFreeCurrency(acc, 2, null, null, false, false);
            Assert.AreEqual(3, acc.Balance);
        }

        [Test]
        public void TestGotFreeCurrency2() {
            CurrencyAccount acc = new CurrencyAccount("test", 100);
            Mng.ShouldCallWriteChanges = true;
            Mng.ShouldCallCheckAndFireBalanceChangeEvent = true;
            Mng.CheckAndFireBalanceChangeEventAccount = acc;
            Mng.CheckAndFireBalanceChangeEventOldBalance = 100;
            Mng.GotFreeCurrency(acc, 23, null, null, true, false);
            Assert.AreEqual(123, acc.Balance);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestGotFreeCurrency3() {
            CurrencyAccount acc = new CurrencyAccount("test", 101);
            Mng.ShouldCallWriteChanges = true;
            Mng.GotFreeCurrency(acc, 25, null, null); // eventId is null -> must throw ArgumentNullException
            Assert.AreEqual(126, acc.Balance);
        }

        [Test]
        public void TestGotFreeCurrency4() {
            CurrencyAccount acc = new CurrencyAccount("test", 91);
            Mng.ShouldCallWriteChanges = true;
            Mng.ShouldCallCheckAndFireBalanceChangeEvent = true;
            Mng.CheckAndFireBalanceChangeEventAccount = acc;
            Mng.CheckAndFireBalanceChangeEventOldBalance = 91;
            BqTracker.EventId = "eid";
            BqTracker.ItemId = "iid";
            BqTracker.CurrencyId = "test";
            BqTracker.Amount = 5;
            BqTracker.Balance = 96;
            FirebaseTracker.ItemName = "iid";
            FirebaseTracker.CurrencyName = "test";
            FirebaseTracker.Amount = 5;
            Mng.GotFreeCurrency(acc, 5, "eid", "iid");
            Assert.AreEqual(96, acc.Balance);
        }

        [Test]
        public void TestTakeCurrency1() {
            CurrencyAccount acc = new CurrencyAccount("test", 1);
            Mng.ShouldCallCheckAndFireBalanceChangeEvent = true;
            Mng.CheckAndFireBalanceChangeEventAccount = acc;
            Mng.CheckAndFireBalanceChangeEventOldBalance = 1;
            Mng.TakeCurrency(acc, 2, null, null, null, false, false);
            Assert.AreEqual(-1, acc.Balance);
        }

        [Test]
        public void TestTakeCurrency2() {
            CurrencyAccount acc = new CurrencyAccount("test", 100);
            Mng.ShouldCallWriteChanges = true;
            Mng.ShouldCallCheckAndFireBalanceChangeEvent = true;
            Mng.CheckAndFireBalanceChangeEventAccount = acc;
            Mng.CheckAndFireBalanceChangeEventOldBalance = 100;
            Mng.TakeCurrency(acc, 23, null, null, null, true, false);
            Assert.AreEqual(77, acc.Balance);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestTakeCurrency3() {
            CurrencyAccount acc = new CurrencyAccount("test", 101);
            Mng.ShouldCallWriteChanges = true;
            Mng.TakeCurrency(acc, 25, null, null, null); // eventId is null -> must throw ArgumentNullException
            Assert.AreEqual(76, acc.Balance);
        }

        [Test]
        public void TestTakeCurrency4() {
            CurrencyAccount acc = new CurrencyAccount("test", 91);
            Mng.ShouldCallWriteChanges = true;
            Mng.ShouldCallCheckAndFireBalanceChangeEvent = true;
            Mng.CheckAndFireBalanceChangeEventAccount = acc;
            Mng.CheckAndFireBalanceChangeEventOldBalance = 91;
            Mng.ShouldCallAddReceiptEvent = true;
            Mng.AddReceiptEventEventItemId = "iid";
            Mng.AddReceiptEventReceiptData = "rd";
            BqTracker.EventId = "eid";
            BqTracker.ItemId = "iid";
            BqTracker.CurrencyId = "test";
            BqTracker.Amount = -5;
            BqTracker.Balance = 86;
            FirebaseTracker.ItemName = "iid";
            FirebaseTracker.CurrencyName = "test";
            FirebaseTracker.Amount = 5;
            Mng.TakeCurrency(acc, 5, "rd", "eid", "iid");
            Assert.AreEqual(86, acc.Balance);
        }

#endregion
    }
}
