using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.GameWiring
{
    public sealed class StubEconomyService : IEconomyService
    {
        private readonly Dictionary<CurrencyType, int> _values = new Dictionary<CurrencyType, int>();

        public event Action<CurrencyType, int> OnChanged;

        public int Get(CurrencyType currency) => _values.TryGetValue(currency, out var v) ? v : 0;
        public bool CanAfford(CurrencyType currency, int amount) => Get(currency) >= amount;

        public bool Spend(CurrencyType currency, int amount)
        {
            if (Get(currency) < amount) return false;
            _values[currency] = Get(currency) - amount;
            OnChanged?.Invoke(currency, _values[currency]);
            return true;
        }

        public void Add(CurrencyType currency, int amount)
        {
            _values[currency] = Get(currency) + amount;
            OnChanged?.Invoke(currency, _values[currency]);
        }
    }

    public sealed class StubPlayerDataService : IPlayerDataService
    {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public int GetInt(string key, int defaultValue = 0) => Read(key, defaultValue);
        public void SetInt(string key, int value) => _values[key] = value;
        public float GetFloat(string key, float defaultValue = 0f) => Read(key, defaultValue);
        public void SetFloat(string key, float value) => _values[key] = value;
        public string GetString(string key, string defaultValue = "") => Read(key, defaultValue);
        public void SetString(string key, string value) => _values[key] = value;
        public bool GetBool(string key, bool defaultValue = false) => Read(key, defaultValue);
        public void SetBool(string key, bool value) => _values[key] = value;
        public bool Has(string key) => _values.ContainsKey(key);
        public void Delete(string key) => _values.Remove(key);
        public void Save() { }
        public void Reload() => _values.Clear();

        private T Read<T>(string key, T defaultValue)
        {
            if (!_values.TryGetValue(key, out var raw)) return defaultValue;
            return raw is T typed ? typed : defaultValue;
        }
    }

    public sealed class StubProgressionService : IProgressionService
    {
        public event Action<LevelData> OnLevelCompleted;
        public event Action<int> OnLevelUnlocked;

        public int GetCurrentLevelIndex() => 0;
        public LevelData GetLevelData(int levelId) => default;
        public IReadOnlyList<LevelData> GetAllLevels() => Array.Empty<LevelData>();
        public DailyLoginState GetDailyLoginState() => default;
        public EnergyRegenState GetEnergyRegenState() => default;
        public void CompleteLevel(int levelId, int starsEarned) => OnLevelCompleted?.Invoke(default);
        public bool UnlockLevel(int levelId) { OnLevelUnlocked?.Invoke(levelId); return true; }
    }

    public sealed class StubShopDataProvider : IShopDataProvider
    {
        public IReadOnlyList<ShopItemData> GetItems(ShopCategory category) => Array.Empty<ShopItemData>();
        public IReadOnlyList<ShopItemData> GetAllItems() => Array.Empty<ShopItemData>();
        public ShopItemData GetItem(string itemId) => default;
        public bool CanAfford(string itemId) => false;
        public PurchaseResult Purchase(string itemId) => default;
    }

    public sealed class StubAdsService : IAdsService
    {
        public bool IsRewardedAdReady() => true;
        public bool IsInterstitialReady() => true;
        public void ShowRewardedAd(Action<bool> onComplete) => onComplete?.Invoke(true);
        public void ShowInterstitial(Action onClosed) => onClosed?.Invoke();
    }

    public sealed class StubTimeService : ITimeService
    {
        public DateTime GetServerTimeUtc() => DateTime.UtcNow;
        public TimeSpan GetTimeUntil(DateTime utcTarget) => utcTarget - DateTime.UtcNow;
        public bool HasElapsed(DateTime utcSince, TimeSpan duration) => DateTime.UtcNow - utcSince >= duration;
    }

    public sealed class StubAudioRouter : IUIAudioRouter
    {
        public void Play(UIAudioCue cue) { }
    }

    public sealed class StubLocalizationService : IUILocalizationService
    {
        private static readonly string[] DefaultLanguages = { "en" };

        public string CurrentLanguage => "en";
        public IReadOnlyList<string> AvailableLanguages => DefaultLanguages;

        public event Action<string> OnLanguageChanged;

        public void SetLanguage(string code)
        {
            if (code == CurrentLanguage) return;
            OnLanguageChanged?.Invoke(code);
        }
    }
}
