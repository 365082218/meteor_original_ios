
using System;

namespace Idevgame.Util {

    /// <summary>
    /// Time utilities.
    /// Partly copied from Spring.Core.Threading.Utils.
    /// </summary>
    public static class TimeUtils {

        /// <summary>
        /// Returns the current UTC time in milliseconds.
        /// </summary>
        /// <returns>
        /// The difference, measured in milliseconds, between the current UTC time and midnight, January 1, 1970 UTC.
        /// </returns>
        public static long CurrentTimeMillis {
            get {
                return ToTimeMillis(DateTime.UtcNow);
            }
        }

        /// <returns>
        /// The difference, measured in milliseconds, between the specified time and midnight, January 1, 1970 UTC.
        /// </returns>
        public static long ToTimeMillis(DateTime dateTime) {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Reverse of <see cref="ToTimeMillis"/>.
        /// </summary>
        /// <param name="timeMillis">the time in milliseconds since midnight, January 1, 1970 UTC</param>
        public static DateTime ToDateTime(long timeMillis) {
            // May also be new DateTime(1970, 1, 1, DateTimeKind.Utc).AddMilliseconds(timeMillis);
            return new DateTime(timeMillis * TimeSpan.TicksPerMillisecond + 621355968000000000, DateTimeKind.Utc);
        }

        /// <returns>
        /// The difference between milliseconds of the first and second date, regardless the time zone.
        /// </returns>
        public static long DeltaTimeMillis(DateTime one, DateTime another) {
            return (one.ToUniversalTime().Ticks - another.ToUniversalTime().Ticks) / TimeSpan.TicksPerMillisecond;
        }

        /// <summary>
        /// Returns the current time zone UTC offset in whole seconds adjusted for daylight-saving time for now.
        /// </summary>
        /// <value>The current time zone adjusted UTC offset in seconds.</value>
        public static int CurrentTimeZoneOffsetSeconds {
            get {
                return (int) CurrentTimeZoneOffset.TotalSeconds;
            }
        }

        /// <summary>
        /// Returns the current time zone UTC offset adjusted for daylight-saving time for now.
        /// </summary>
        /// <value>The current time zone adjusted UTC offset.</value>
        public static TimeSpan CurrentTimeZoneOffset {
            get {
                return GetTimeZoneOffset(DateTime.Now);
            }
        }

        /// <summary>
        /// Returns the current time zone UTC offset adjusted for daylight-saving time for the specified time.
        /// </summary>
        /// <returns>The current time zone adjusted UTC offset.</returns>
        /// <param name="time">the time to return time zone for</param>
        public static TimeSpan GetTimeZoneOffset(DateTime time) {
#if UNITY_WP8 && !UNITY_EDITOR
            return TimeZoneInfo.Local.GetUtcOffset(time);
#else
            return TimeZone.CurrentTimeZone.GetUtcOffset(time);
#endif
        }
    }
}
