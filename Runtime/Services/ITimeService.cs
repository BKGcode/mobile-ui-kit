using System;

namespace KitforgeLabs.MobileUIKit.Services
{
    public interface ITimeService
    {
        DateTime GetServerTimeUtc();
        TimeSpan GetTimeUntil(DateTime utcTarget);
        bool HasElapsed(DateTime utcSince, TimeSpan duration);
    }
}
