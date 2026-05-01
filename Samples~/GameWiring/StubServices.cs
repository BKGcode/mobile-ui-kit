using System;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.GameWiring
{
    public sealed class StubEconomyService : IEconomyService
    {
    }

    public sealed class StubPlayerDataService : IPlayerDataService
    {
    }

    public sealed class StubProgressionService : IProgressionService
    {
    }

    public sealed class StubShopDataProvider : IShopDataProvider
    {
    }

    public sealed class StubAdsService : IAdsService
    {
    }

    public sealed class StubTimeService : ITimeService
    {
        public DateTime GetServerTimeUtc() => DateTime.UtcNow;
    }
}
