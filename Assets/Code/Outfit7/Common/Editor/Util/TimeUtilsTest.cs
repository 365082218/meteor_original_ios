//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;
using NUnit.Framework;
using UnityEngine;
using A = NUnit.Framework.Assert;

namespace Outfit7.Util {

    /// <summary>
    /// TimeUtils unit tests.
    /// </summary>
    public class TimeUtilsTest {

        // 20000101T010101.001Z
        private const long Ms = 946688461001L;
        private readonly DateTime Utc = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

        [Test]
        public void TestBasic() {
            // Facts:
            // * DateTime arithmetics & comparisons don't know anything about the time zone.
            // * DateTimeOffset arithmetics & comparisons consider time zone differences, but lose knowledge of the time zones,
            //   so that arithmetics around time zone adjustments (daylight saving time on/off) are wrong.
            // * Persist only UTC DateTimes or DateTimeOffsets, convert DateTimes or DateTimeOffsets given from outside
            //   to UTC before doing arithmetics or comparisons.
            // http://msdn.microsoft.com/en-us/library/bb384267%28v=vs.90%29.aspx
            // http://msdn.microsoft.com/en-us/library/bb546099%28v=vs.90%29.aspx
            if (TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes > 1) { // Only if local time is not UTC
                DateTime local = new DateTime(2000, 1, 1, 1, 1, 1, 1);
                A.AreNotEqual(local, local.ToUniversalTime());
                A.AreNotEqual(local.Ticks, local.ToUniversalTime().Ticks);
            }
            A.AreEqual((DateTimeOffset.UtcNow - DateTimeOffset.Now).TotalMilliseconds, 0, 10);
            A.AreEqual(DateTimeOffset.UtcNow.UtcTicks, DateTimeOffset.Now.UtcTicks, 10 * TimeSpan.TicksPerMillisecond);

            // Just check if crashes
            Debug.Log("Max time:" + DateTime.MaxValue);
            Debug.Log("Max time to local:" + DateTime.MaxValue.ToLocalTime());
            Debug.Log("Max time to UTC:" + DateTime.MaxValue.ToUniversalTime());
            Debug.Log("Min time:" + DateTime.MinValue);
            Debug.Log("Min time to local:" + DateTime.MinValue.ToLocalTime());
            Debug.Log("Min time to UTC:" + DateTime.MinValue.ToUniversalTime());

            A.AreEqual((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds, TimeUtils.CurrentTimeMillis, 10);
        }

        [Test]
        public void TestToTimeMillis() {
            long utcMs = TimeUtils.ToTimeMillis(Utc);
            A.AreEqual(Ms, utcMs);
        }

        [Test]
        public void TestToDateTime() {
            DateTime utc = TimeUtils.ToDateTime(Ms);
            A.AreEqual(Utc, utc);
        }

        [Test]
        public void TestDeltaTimeMillis() {
            {
                DateTime dt1 = new DateTime(2000, 1, 1, 1, 1, 1, 1);
                DateTime dt2 = new DateTime(2000, 1, 1, 1, 1, 1, 0);

                long deltaMs = TimeUtils.DeltaTimeMillis(dt1, dt2);
                Debug.Log("deltaMs: " + deltaMs);
                A.AreEqual(1, deltaMs);
            }

            {
                DateTime dt1 = new DateTime(2000, 1, 1, 1, 1, 1, 0, DateTimeKind.Utc);
                DateTime dt2 = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

                long deltaMs = TimeUtils.DeltaTimeMillis(dt1, dt2);
                Debug.Log("deltaMs: " + deltaMs);
                A.AreEqual(-1, deltaMs);
            }

            {
                DateTime dt1 = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);
                DateTime dt2 = new DateTime(2000, 1, 1, 1, 1, 1, 0, DateTimeKind.Local);
                double localUtcDelta = TimeZone.CurrentTimeZone.GetUtcOffset(dt1).TotalMilliseconds;

                long deltaMs = TimeUtils.DeltaTimeMillis(dt1, dt2);
                Debug.Log("deltaMs: " + deltaMs);
                A.AreEqual(1 + localUtcDelta, deltaMs, 0.01);
            }
        }

        [Test]
        public void TestCurrentTimeZoneOffsetSeconds() {
            int offset1 = (int) TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalSeconds;
            int offset2 = TimeUtils.CurrentTimeZoneOffsetSeconds;
            A.AreEqual(offset1, offset2);
        }

        [Test]
        public void TestCurrentTimeZoneOffset() {
            TimeSpan offset1 = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            TimeSpan offset2 = TimeUtils.CurrentTimeZoneOffset;
            A.AreEqual(offset1, offset2);
        }

        [Test]
        public void TestGetTimeZoneOffset() {
            DateTime standard = new DateTime(2000, 1, 1, 1, 1, 1, 1, DateTimeKind.Local);
            DateTime daylight = new DateTime(2000, 7, 1, 1, 1, 1, 1, DateTimeKind.Local);

            TimeSpan standardOffset = TimeUtils.GetTimeZoneOffset(standard);
            TimeSpan daylightOffset = TimeUtils.GetTimeZoneOffset(daylight);
            Debug.Log("standardOffset: " + standardOffset);
            Debug.Log("daylightOffset: " + daylightOffset);
            A.AreEqual(standardOffset + TimeSpan.FromHours(1), daylightOffset);
        }
    }
}
