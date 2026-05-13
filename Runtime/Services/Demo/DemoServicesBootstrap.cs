using UnityEngine;

namespace KitforgeLabs.UIKit.Services.Demo
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-200)]
    public sealed class DemoServicesBootstrap : MonoBehaviour
    {
        [SerializeField] private UIServices _uiServices;

        private void Reset()
        {
            _uiServices = GetComponentInChildren<UIServices>(true);
        }

        private void Awake()
        {
            if (_uiServices == null)
            {
                Debug.LogError("[DemoServicesBootstrap] UIServices reference missing. Drag the KitforgeRoot's UIServices into the Inspector slot.", this);
                return;
            }
            _uiServices.SetEconomy(new DemoEconomyService());
            _uiServices.SetPlayerData(new DemoPlayerDataService());
            _uiServices.SetProgression(new DemoProgressionService());
            _uiServices.SetShopData(new DemoShopDataProvider());
            _uiServices.SetAds(new DemoAdsService());
            _uiServices.SetTime(new DemoTimeService());
            _uiServices.SetAudio(new DemoAudioRouter());
            _uiServices.SetLocalization(new DemoLocalizationService());
        }
    }
}
