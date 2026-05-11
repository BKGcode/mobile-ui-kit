using KitforgeLabs.UIKit.Services.Null;
using UnityEngine;

namespace KitforgeLabs.UIKit.Services
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-100)]
    public sealed class UIServices : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _economyServiceRef;
        [SerializeField] private MonoBehaviour _playerDataServiceRef;
        [SerializeField] private MonoBehaviour _progressionServiceRef;
        [SerializeField] private MonoBehaviour _shopDataProviderRef;
        [SerializeField] private MonoBehaviour _adsServiceRef;
        [SerializeField] private MonoBehaviour _timeServiceRef;
        [SerializeField] private MonoBehaviour _audioRouterRef;
        [SerializeField] private MonoBehaviour _localizationServiceRef;

        public IEconomyService Economy { get; private set; }
        public IPlayerDataService PlayerData { get; private set; }
        public IProgressionService Progression { get; private set; }
        public IShopDataProvider ShopData { get; private set; }
        public IAdsService Ads { get; private set; }
        public ITimeService Time { get; private set; }
        public IUIAudioRouter Audio { get; private set; }
        public IUILocalizationService Localization { get; private set; }

        public bool UsingNullDefaults { get; private set; }

        private void Awake()
        {
            Economy ??= Resolve<IEconomyService>(_economyServiceRef, nameof(Economy)) ?? FallbackNull<IEconomyService>(new NullEconomyService());
            PlayerData ??= Resolve<IPlayerDataService>(_playerDataServiceRef, nameof(PlayerData)) ?? FallbackNull<IPlayerDataService>(new NullPlayerDataService());
            Progression ??= Resolve<IProgressionService>(_progressionServiceRef, nameof(Progression)) ?? FallbackNull<IProgressionService>(new NullProgressionService());
            ShopData ??= Resolve<IShopDataProvider>(_shopDataProviderRef, nameof(ShopData)) ?? FallbackNull<IShopDataProvider>(new NullShopDataProvider());
            Ads ??= Resolve<IAdsService>(_adsServiceRef, nameof(Ads)) ?? FallbackNull<IAdsService>(new NullAdsService());
            Time ??= Resolve<ITimeService>(_timeServiceRef, nameof(Time)) ?? FallbackNull<ITimeService>(new NullTimeService());
            Audio ??= Resolve<IUIAudioRouter>(_audioRouterRef, nameof(Audio)) ?? FallbackNull<IUIAudioRouter>(new NullAudioRouter());
            Localization ??= Resolve<IUILocalizationService>(_localizationServiceRef, nameof(Localization)) ?? FallbackNull<IUILocalizationService>(new NullLocalizationService());
            if (UsingNullDefaults) Debug.LogWarning("[UIServices] Using Null Object defaults for unwired services. Wire your production implementations in the Inspector before shipping.", this);
        }

        public void SetEconomy(IEconomyService impl) => Economy = impl;
        public void SetPlayerData(IPlayerDataService impl) => PlayerData = impl;
        public void SetProgression(IProgressionService impl) => Progression = impl;
        public void SetShopData(IShopDataProvider impl) => ShopData = impl;
        public void SetAds(IAdsService impl) => Ads = impl;
        public void SetTime(ITimeService impl) => Time = impl;
        public void SetAudio(IUIAudioRouter impl) => Audio = impl;
        public void SetLocalization(IUILocalizationService impl) => Localization = impl;

        private T Resolve<T>(MonoBehaviour reference, string slot) where T : class
        {
            if (reference == null) return null;
            if (reference is T typed) return typed;
            Debug.LogError($"[UIServices] '{reference.GetType().Name}' assigned to {slot} does not implement {typeof(T).Name}.", this);
            return null;
        }

        private T FallbackNull<T>(T nullImpl) where T : class
        {
            UsingNullDefaults = true;
            return nullImpl;
        }
    }
}
