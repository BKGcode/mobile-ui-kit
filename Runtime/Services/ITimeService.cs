using System;

namespace KitforgeLabs.UIKit.Services
{
    public interface ITimeService
    {
        DateTime GetServerTimeUtc();
        TimeSpan GetTimeUntil(DateTime utcTarget);
        bool HasElapsed(DateTime utcSince, TimeSpan duration);
    }
}
