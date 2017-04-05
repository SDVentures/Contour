namespace Contour.Helpers
{
    using System;

    /// <summary>
    /// The date time ex.
    /// </summary>
    internal static class DateTimeEx
    {
        /// <summary>
        /// The from unix timestamp.
        /// </summary>
        /// <param name="unixTime">
        /// The unix time.
        /// </param>
        /// <returns>
        /// The <see cref="DateTime"/>.
        /// </returns>
        public static DateTime FromUnixTimestamp(long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        /// <summary>
        /// The to unix timestamp.
        /// </summary>
        /// <param name="dateTime">
        /// The date time.
        /// </param>
        /// <returns>
        /// The <see cref="long"/>.
        /// </returns>
        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(dateTime.ToUniversalTime() - epoch).TotalSeconds;
        }
    }
}
