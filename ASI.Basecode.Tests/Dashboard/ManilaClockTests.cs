using ASI.Basecode.Services.Utilities;
using System;
using Xunit;

namespace ASI.Basecode.Tests.Dashboard
{
    public sealed class ManilaClockTests
    {
        [Fact]
        public void TodayUtcRange_spans_one_day_from_Manila_midnight()
        {
            var nowUtc = new DateTime(2026, 9, 30, 16, 30, 0, DateTimeKind.Utc);

            var (startUtc, endUtc) = ManilaClock.TodayUtcRange(nowUtc);

            Assert.Equal(TimeSpan.FromDays(1), endUtc - startUtc);
            Assert.Equal(16, startUtc.Hour);
            Assert.Equal(new DateTime(2026, 9, 30, 16, 0, 0, DateTimeKind.Utc), startUtc);
        }

        [Fact]
        public void ToLocal_converts_utc_midnight_boundary_to_Manila_time()
        {
            var local = ManilaClock.ToLocal(
                new DateTime(2026, 9, 30, 16, 30, 0, DateTimeKind.Utc));

            Assert.Equal(new DateTime(2026, 10, 1, 0, 30, 0), local);
            Assert.Equal(DateTimeKind.Unspecified, local.Kind);
        }

        [Fact]
        public void NowLocal_has_unspecified_kind()
        {
            Assert.Equal(DateTimeKind.Unspecified, ManilaClock.NowLocal.Kind);
        }
    }
}
