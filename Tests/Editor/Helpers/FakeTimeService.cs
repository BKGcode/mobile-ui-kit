using System;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers
{
    internal sealed class FakeTimeService : ITimeService
    {
        private DateTime _nowUtc = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        public DateTime GetServerTimeUtc() => _nowUtc;
        public TimeSpan GetTimeUntil(DateTime utcTarget) => utcTarget - _nowUtc;
        public bool HasElapsed(DateTime utcSince, TimeSpan duration) => _nowUtc - utcSince >= duration;

        public void SetNow(DateTime utc) => _nowUtc = utc;
        public void Advance(TimeSpan delta) => _nowUtc += delta;
    }
}
