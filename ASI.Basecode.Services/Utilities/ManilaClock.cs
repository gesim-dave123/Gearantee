using System;

namespace ASI.Basecode.Services.Utilities
{
    public static class ManilaClock
    {
        private static readonly TimeZoneInfo ManilaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        public static DateTime NowLocal => TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            ManilaTimeZone);

        public static DateTime ToLocal(DateTime utc)
        {
            var utcValue = utc.Kind == DateTimeKind.Utc
                ? utc
                : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeFromUtc(utcValue, ManilaTimeZone);
        }

        public static (DateTime StartUtc, DateTime EndUtc) TodayUtcRange()
        {
            var todayLocal = NowLocal.Date;
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                todayLocal,
                ManilaTimeZone);
            return (startUtc, startUtc.AddDays(1));
        }

        public static (DateTime StartUtc, DateTime EndUtc) CurrentMonthUtcRange()
        {
            var nowLocal = NowLocal;
            var startLocal = new DateTime(
                nowLocal.Year,
                nowLocal.Month,
                1,
                0,
                0,
                0,
                DateTimeKind.Unspecified);
            var nextLocal = startLocal.AddMonths(1);
            return (
                TimeZoneInfo.ConvertTimeToUtc(startLocal, ManilaTimeZone),
                TimeZoneInfo.ConvertTimeToUtc(nextLocal, ManilaTimeZone));
        }
    }
}
