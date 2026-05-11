using System;
using System.Collections.Generic;

namespace KitforgeLabs.UIKit.Services.Null
{
    public sealed class NullEconomyService : IEconomyService
    {
        public event Action<CurrencyType, int> OnChanged;
        public int Get(CurrencyType currency) => 0;
        public bool Spend(CurrencyType currency, int amount) => false;
        public void Add(CurrencyType currency, int amount) { }
        public bool CanAfford(CurrencyType currency, int amount) => false;
    }

    public sealed class NullPlayerDataService : IPlayerDataService
    {
        public int GetInt(string key, int defaultValue = 0) => defaultValue;
        public void SetInt(string key, int value) { }
        public float GetFloat(string key, float defaultValue = 0f) => defaultValue;
        public void SetFloat(string key, float value) { }
        public string GetString(string key, string defaultValue = "") => defaultValue;
        public void SetString(string key, string value) { }
        public bool GetBool(string key, bool defaultValue = false) => defaultValue;
        public void SetBool(string key, bool value) { }
        public bool Has(string key) => false;
        public void Delete(string key) { }
        public void Save() { }
        public void Reload() { }
    }

    public sealed class NullProgressionService : IProgressionService
    {
        private static readonly IReadOnlyList<LevelData> EmptyLevels = Array.Empty<LevelData>();

        public event Action<LevelData> OnLevelCompleted;
        public event Action<int> OnLevelUnlocked;

        public int GetCurrentLevelIndex() => 0;
        public LevelData GetLevelData(int levelId) => default;
        public IReadOnlyList<LevelData> GetAllLevels() => EmptyLevels;
        public void CompleteLevel(int levelId, int starsEarned) { }
        public bool UnlockLevel(int levelId) => false;
        public DailyLoginState GetDailyLoginState() => default;
        public EnergyRegenState GetEnergyRegenState() => default;
    }

    public sealed class NullShopDataProvider : IShopDataProvider
    {
        private static readonly IReadOnlyList<ShopItemData> EmptyItems = Array.Empty<ShopItemData>();

        public IReadOnlyList<ShopItemData> GetItems(ShopCategory category) => EmptyItems;
        public IReadOnlyList<ShopItemData> GetAllItems() => EmptyItems;
        public ShopItemData GetItem(string itemId) => default;
        public bool CanAfford(string itemId) => false;
        public PurchaseResult Purchase(string itemId) => PurchaseResult.Failed;
    }

    public sealed class NullAdsService : IAdsService
    {
        public bool IsRewardedAdReady() => false;
        public bool IsInterstitialReady() => false;
        public void ShowRewardedAd(Action<bool> onComplete) => onComplete?.Invoke(false);
        public void ShowInterstitial(Action onClosed) => onClosed?.Invoke();
    }

    public sealed class NullTimeService : ITimeService
    {
        public DateTime GetServerTimeUtc() => DateTime.UtcNow;
        public TimeSpan GetTimeUntil(DateTime utcTarget) => utcTarget - DateTime.UtcNow;
        public bool HasElapsed(DateTime utcSince, TimeSpan duration) => DateTime.UtcNow - utcSince >= duration;
    }

    public sealed class NullAudioRouter : IUIAudioRouter
    {
        public void Play(UIAudioCue cue) { }
    }

    public sealed class NullLocalizationService : IUILocalizationService
    {
        private static readonly IReadOnlyList<string> DefaultLanguages = new[] { "en" };

        public event Action<string> OnLanguageChanged;
        public string CurrentLanguage => "en";
        public IReadOnlyList<string> AvailableLanguages => DefaultLanguages;
        public void SetLanguage(string code) { }
    }
}
