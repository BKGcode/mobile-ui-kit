using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Services
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

        public IEconomyService Economy { get; private set; }
        public IPlayerDataService PlayerData { get; private set; }
        public IProgressionService Progression { get; private set; }
        public IShopDataProvider ShopData { get; private set; }
        public IAdsService Ads { get; private set; }
        public ITimeService Time { get; private set; }
        public IUIAudioRouter Audio { get; private set; }

        private void Awake()
        {
            Economy ??= Resolve<IEconomyService>(_economyServiceRef, nameof(Economy));
            PlayerData ??= Resolve<IPlayerDataService>(_playerDataServiceRef, nameof(PlayerData));
            Progression ??= Resolve<IProgressionService>(_progressionServiceRef, nameof(Progression));
            ShopData ??= Resolve<IShopDataProvider>(_shopDataProviderRef, nameof(ShopData));
            Ads ??= Resolve<IAdsService>(_adsServiceRef, nameof(Ads));
            Time ??= Resolve<ITimeService>(_timeServiceRef, nameof(Time));
            Audio ??= Resolve<IUIAudioRouter>(_audioRouterRef, nameof(Audio));
        }

        public void SetEconomy(IEconomyService impl) => Economy = impl;
        public void SetPlayerData(IPlayerDataService impl) => PlayerData = impl;
        public void SetProgression(IProgressionService impl) => Progression = impl;
        public void SetShopData(IShopDataProvider impl) => ShopData = impl;
        public void SetAds(IAdsService impl) => Ads = impl;
        public void SetTime(ITimeService impl) => Time = impl;
        public void SetAudio(IUIAudioRouter impl) => Audio = impl;

        private T Resolve<T>(MonoBehaviour reference, string slot) where T : class
        {
            if (reference == null) return null;
            if (reference is T typed) return typed;
            Debug.LogError($"[UIServices] '{reference.GetType().Name}' assigned to {slot} does not implement {typeof(T).Name}.", this);
            return null;
        }

        [ContextMenu("Validate")]
        private void Validate()
        {
            var missing = 0;
            if (_economyServiceRef == null) { Debug.LogWarning("[UIServices] Economy service is null.", this); missing++; }
            if (_playerDataServiceRef == null) { Debug.LogWarning("[UIServices] PlayerData service is null.", this); missing++; }
            if (_progressionServiceRef == null) { Debug.LogWarning("[UIServices] Progression service is null.", this); missing++; }
            if (_shopDataProviderRef == null) { Debug.LogWarning("[UIServices] ShopData provider is null.", this); missing++; }
            if (_adsServiceRef == null) { Debug.LogWarning("[UIServices] Ads service is null.", this); missing++; }
            if (_timeServiceRef == null) { Debug.LogWarning("[UIServices] Time service is null.", this); missing++; }
            if (_audioRouterRef == null) { Debug.LogWarning("[UIServices] Audio router is null.", this); missing++; }
            Debug.Log($"[UIServices] Validation complete. Missing: {missing}/7.", this);
        }
    }
}
