//
//   Copyright (c) 2013 Outfit7. All rights reserved.
//

using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace SimpleJSON {
    public class SimpleJsonTest {

        [Test]
        public void TestNullValues() {
            JSONClass c = new JSONClass();
            c["empty"] = "";
            c["space"] = " ";
            c["(string)null"] = (string) null; // Skip
            c["JSONData(null)"] = new JSONData(null); // Skip
            c["JSONData()"] = new JSONData("");
            c["JSONData( )"] = new JSONData(" ");
            c[".Value=empty"].Value = ""; // NOP
            c[".Value=null"].Value = null; // Skip
            c["null"] = null; // Skip
            c["emptyArray"] = new JSONArray();
            c[".Value=emptyArray"].Value = new JSONArray();
            JSONArray array = new JSONArray();
            array.Add(new JSONData("empty"));
            array.Add(new JSONData(null)); // Skip
            array.Add(null); // Skip
            c["array"] = array;
            Outfit7.Util.O7Log.Debug(c.ToString());
            Assert.AreEqual("{\"empty\":\"\", \"space\":\" \", \"JSONData()\":\"\", \"JSONData( )\":\" \", \"emptyArray\":[  ], \"array\":[ \"empty\" ]}", c.ToString());
        }

        [Test]
        public void CheckLocalization() {
            CultureInfo ci = new CultureInfo("fr-FR"); // Uses , for decimal separator
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            int i = 34345635;
            long l = 5684745202233;
            float f = 12435.4556f;
            float f2 = 12435f;
            double d = 344654645.344576567;
            double d2 = 344654645;

            JSONClass j = new JSONClass();
            j["i"].AsInt = i;
            j["L"].AsLong = l;
            j["f"].AsFloat = f;
            j["f2"].AsFloat = f2;
            j["d"].AsDouble = d;
            j["d2"].AsDouble = d2;

            Outfit7.Util.O7Log.Debug("JSON: {0}", j.ToString());
            Outfit7.Util.O7Log.Debug("ToString: {0}, {1}, {2}, {3}, {4}, {5}", i, l, f, f2, d, d2);

            JSONClass _j = new JSONClass();
            _j["i"] = "6565565";
            _j["L"] = "653238332323";
            _j["f"] = "1235535,545";
            _j["f2"] = "1235535";
            _j["d"] = "565645545,454565456";
            _j["d2"] = "565645545";

            int _i = _j["i"].AsInt;
            long _l = _j["L"].AsLong;
            float _f = _j["f"].AsFloat;
            float _f2 = _j["f2"].AsFloat;
            double _d = _j["d"].AsDouble;
            double _d2 = _j["d2"].AsDouble;

            Outfit7.Util.O7Log.Debug("ToString: {0}, {1}, {2}, {3}, {4}, {5}", _i, _l, _f, _f2, _d, _d2);

            JSONClass t = new JSONClass();
            t["null"] = null;
            t["empty"] = "";
            Outfit7.Util.O7Log.Debug("null='{0}', empty='{1}', empty='{0}'", t["null"], t["empty"], t["empty"].Value);
        }

        [Test]
        public void TestDateTime() {
            JSONClass j = new JSONClass();

            {
                DateTime local = DateTime.Now;
                j["dt1"].AsDateTime = local;
                DateTime p = j["dt1"].AsDateTime;
                Assert.AreEqual(local.ToUniversalTime(), p);
                Assert.AreEqual(DateTimeKind.Utc, p.Kind);
            }

            TestInvalidDateTime(j["dt2"]); // Not set

            j["dt3"].AsLong = long.MaxValue; // Out of range
            TestInvalidDateTime(j["dt3"]);

            j["dt4"] = ""; // Empty
            TestInvalidDateTime(j["dt4"]);

            j["dt5"] = "string"; // Invalid
            TestInvalidDateTime(j["dt5"]);

            {
                DateTime min = DateTime.MinValue;
                j["dt6"].AsDateTime = min;
                DateTime p = j["dt6"].AsDateTime;
                Assert.AreEqual(min, p);
                Assert.AreEqual(min.Kind, p.Kind);
            }

            {
                DateTime max = DateTime.MaxValue;
                j["dt7"].AsDateTime = max;
                DateTime p = j["dt7"].AsDateTime;
                Assert.AreEqual(max, p);
                Assert.AreEqual(max.Kind, p.Kind);
            }
        }

        private void TestInvalidDateTime(JSONNode node) {
            DateTime invalid = node.AsDateTime;
            Assert.AreEqual(DateTime.MinValue, invalid);
            Assert.AreEqual(DateTime.MinValue.Kind, invalid.Kind);
        }
    }
}
