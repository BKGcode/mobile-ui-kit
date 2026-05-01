using KitforgeLabs.MobileUIKit.Routing;
using KitforgeLabs.MobileUIKit.Services;
using VContainer;
using VContainer.Unity;

namespace KitforgeLabs.MobileUIKit.GameWiring
{
    public class GameWiringLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IEconomyService, StubEconomyService>(Lifetime.Singleton);
            builder.Register<IPlayerDataService, StubPlayerDataService>(Lifetime.Singleton);
            builder.Register<IProgressionService, StubProgressionService>(Lifetime.Singleton);
            builder.Register<IShopDataProvider, StubShopDataProvider>(Lifetime.Singleton);
            builder.Register<IAdsService, StubAdsService>(Lifetime.Singleton);
            builder.Register<ITimeService, StubTimeService>(Lifetime.Singleton);

            builder.Register<UIRouterStub>(Lifetime.Singleton);
            builder.RegisterBuildCallback(container => container.Resolve<UIRouterStub>().TransitionTo(AppState.Loading));
        }
    }
}
