//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using System.Collections.Generic;
using NUnit.Framework;
using A = NUnit.Framework.Assert;

namespace Outfit7.Util {
    public class UserPrefsTest {

        private const string PREF = "fgnkdsjgndjksfgjknj3k5";

        [Test]
        public void TestCollection() {
            ClearKey();
            {
                List<string> got = UserPrefs.GetCollectionAsList(PREF, null);
                Assert.State(got == null, "Not null");
            }
            {
                List<string> col = null;
                UserPrefs.SetCollection(PREF, col);
                UserPrefs.Save();
                List<string> got = UserPrefs.GetCollectionAsList(PREF, null);
                Assert.State(got.Count == 0, "Not empty");
            }
            ClearKey();
            {
                List<string> col = new List<string>{ };
                UserPrefs.SetCollection(PREF, col);
                UserPrefs.Save();
                List<string> got = UserPrefs.GetCollectionAsList(PREF, null);
                Assert.State(got.Count == 0, "Not empty");
            }
            ClearKey();
            {
                List<string> col = new List<string>{ "" };
                UserPrefs.SetCollection(PREF, col);
                UserPrefs.Save();
                List<string> got = UserPrefs.GetCollectionAsList(PREF, null);
                Assert.State(got.Count == 1 && got.Contains(""), "No single empty element");
            }
            ClearKey();
            {
                List<string> col = new List<string>{ " " };
                UserPrefs.SetCollection(PREF, col);
                UserPrefs.Save();
                List<string> got = UserPrefs.GetCollectionAsList(PREF, null);
                Assert.State(got.Count == 1 && got.Contains(" "), "No single element");
            }
//        ClearKey();
//        {
//            string value = "MarkoStamcar,kupuje,nov,avto!";
//            List<string> col = new List<string>{value};
//            UserPrefs.SetCollection(PREF, col);
//            UserPrefs.Save();
//            List<string> got = UserPrefs.GetCollectionAsList(PREF, null);
//            O7Log.Error(""+got.Count);
//            A.State(got.Count == 1 && got.Contains(value), "No single element match");
//        }
            ClearKey();
            {
                List<string> col = new List<string>{ "id1", "id2", "id3" };
                UserPrefs.SetCollection(PREF, col);
                UserPrefs.Save();
                List<string> got = UserPrefs.GetCollectionAsList(PREF, null);
                Assert.State(CollectionUtils.EqualsAll(col, got), "No match");
            }
        }

        private void ClearKey() {
            UserPrefs.Remove(PREF);
            UserPrefs.Save();
            Assert.State(!UserPrefs.HasKey(PREF), "Not removed");
        }

        [Test]
        public void TestDateTime() {
            {
                DateTime local = DateTime.Now;
                UserPrefs.SetDateTime("TestDateTime", local);
                DateTime p = UserPrefs.GetDateTime("TestDateTime", DateTime.MinValue);
                A.AreEqual(local.ToUniversalTime(), p);
                A.AreEqual(DateTimeKind.Utc, p.Kind);
            }

            {
                DateTime unexistent = UserPrefs.GetDateTime("TestDateTime2", DateTime.MinValue);
                A.AreEqual(DateTime.MinValue, unexistent);
                A.AreEqual(DateTime.MinValue.Kind, unexistent.Kind);
            }

            {
                DateTime min = DateTime.MinValue;
                UserPrefs.SetDateTime("MinDateTime", min);
                DateTime p = UserPrefs.GetDateTime("MinDateTime", DateTime.MaxValue);
                A.AreEqual(min, p);
                A.AreEqual(min.Kind, p.Kind);
            }

            {
                DateTime max = DateTime.MaxValue;
                UserPrefs.SetDateTime("MaxDateTime", max);
                DateTime p = UserPrefs.GetDateTime("MaxDateTime", DateTime.MinValue);
                A.AreEqual(max, p);
                A.AreEqual(max.Kind, p.Kind);
            }
        }
    }
}
