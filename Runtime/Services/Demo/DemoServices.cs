using System;
using System.Collections.Generic;

namespace KitforgeLabs.UIKit.Services.Demo
{
    public sealed class DemoEconomyService : IEconomyService
    {
        private readonly Dictionary<CurrencyType, int> _wallet = new()
        {
            { CurrencyType.Coins, 1250 },
            { CurrencyType.Gems, 80 },
            { CurrencyType.Energy, 5 }
        };

        public event Action<CurrencyType, int> OnChanged;

        public int Get(CurrencyType currency) => _wallet.TryGetValue(currency, out var value) ? value : 0;

        public bool CanAfford(CurrencyType currency, int amount) => Get(currency) >= amount;

        public bool Spend(CurrencyType currency, int amount)
        {
            if (!CanAfford(currency, amount)) return false;
            _wallet[currency] = Get(currency) - amount;
            OnChanged?.Invoke(currency, _wallet[currency]);
            return true;
        }

        public void Add(CurrencyType currency, int amount)
        {
            _wallet[currency] = Get(currency) + amount;
            OnChanged?.Invoke(currency, _wallet[currency]);
        }
    }

    public sealed class DemoPlayerDataService : IPlayerDataService
    {
        private const string KeyPrefix = "kfdemo.";
        private readonly Dictionary<string, object> _store = new();

        public int GetInt(string key, int defaultValue = 0) => _store.TryGetValue(KeyPrefix + key, out var v) && v is int i ? i : defaultValue;
        public void SetInt(string key, int value) => _store[KeyPrefix + key] = value;
        public float GetFloat(string key, float defaultValue = 0f) => _store.TryGetValue(KeyPrefix + key, out var v) && v is float f ? f : defaultValue;
        public void SetFloat(string key, float value) => _store[KeyPrefix + key] = value;
        public string GetString(string key, string defaultValue = "") => _store.TryGetValue(KeyPrefix + key, out var v) && v is string s ? s : defaultValue;
        public void SetString(string key, string value) => _store[KeyPrefix + key] = value;
        public bool GetBool(string key, bool defaultValue = false) => _store.TryGetValue(KeyPrefix + key, out var v) && v is bool b ? b : defaultValue;
        public void SetBool(string key, bool value) => _store[KeyPrefix + key] = value;
        public bool Has(string key) => _store.ContainsKey(KeyPrefix + key);
        public void Delete(string key) => _store.Remove(KeyPrefix + key);
        public void Save() { }
        public void Reload() { }
    }

    public sealed class DemoProgressionService : IProgressionService
    {
        private const int MaxEnergy = 5;
        private const int LevelCount = 12;
        private readonly LevelData[] _levels;
        private DailyLoginState _dailyLogin;
        private EnergyRegenState _energy;

        public DemoProgressionService()
        {
            _levels = new LevelData[LevelCount];
            for (var i = 0; i < LevelCount; i++)
            {
                var state = i < 3 ? LevelState.Complete : i == 3 ? LevelState.Available : LevelState.Locked;
                _levels[i] = new LevelData { Id = i, State = state, Stars = i < 3 ? 3 : 0, UnlockCost = 0 };
            }
            _dailyLogin = new DailyLoginState
            {
                CurrentDay = 3,
                LastClaimUtc = DateTime.UtcNow.AddDays(-1),
                AlreadyClaimedToday = false,
                DoubledToday = false,
                MaxStreakGapDays = 1
            };
            _energy = new EnergyRegenState
            {
                Current = MaxEnergy,
                Max = MaxEnergy,
                NextRegenUtc = DateTime.UtcNow.AddMinutes(15),
                IsFull = true
            };
        }

        public event Action<LevelData> OnLevelCompleted;
        public event Action<int> OnLevelUnlocked;

        public int GetCurrentLevelIndex() => 3;
        public LevelData GetLevelData(int levelId) => levelId >= 0 && levelId < _levels.Length ? _levels[levelId] : default;
        public IReadOnlyList<LevelData> GetAllLevels() => _levels;

        public void CompleteLevel(int levelId, int starsEarned)
        {
            if (levelId < 0 || levelId >= _levels.Length) return;
            _levels[levelId].State = LevelState.Complete;
            _levels[levelId].Stars = Math.Max(_levels[levelId].Stars, starsEarned);
            OnLevelCompleted?.Invoke(_levels[levelId]);
            UnlockLevel(levelId + 1);
        }

        public bool UnlockLevel(int levelId)
        {
            if (levelId < 0 || levelId >= _levels.Length) return false;
            if (_levels[levelId].State != LevelState.Locked) return false;
            _levels[levelId].State = LevelState.Available;
            OnLevelUnlocked?.Invoke(levelId);
            return true;
        }

        public DailyLoginState GetDailyLoginState() => _dailyLogin;
        public EnergyRegenState GetEnergyRegenState() => _energy;
    }

    public sealed class DemoShopDataProvider : IShopDataProvider
    {
        private readonly ShopItemData[] _items;

        public DemoShopDataProvider()
        {
            _items = new[]
            {
                new ShopItemData { Id = "coins_small", DisplayName = "Coin Pouch", Category = ShopCategory.Currency, PriceCurrency = CurrencyType.Gems, PriceAmount = 10, IconKey = "icon_coins_small" },
                new ShopItemData { Id = "coins_medium", DisplayName = "Coin Bag", Category = ShopCategory.Currency, PriceCurrency = CurrencyType.Gems, PriceAmount = 25, IconKey = "icon_coins_medium" },
                new ShopItemData { Id = "coins_large", DisplayName = "Coin Chest", Category = ShopCategory.Currency, PriceCurrency = CurrencyType.Gems, PriceAmount = 80, IconKey = "icon_coins_large" },
                new ShopItemData { Id = "boost_2x", DisplayName = "2x Coins (1h)", Category = ShopCategory.Consumable, PriceCurrency = CurrencyType.Gems, PriceAmount = 15, IconKey = "icon_boost" },
                new ShopItemData { Id = "skin_neon", DisplayName = "Neon Skin", Category = ShopCategory.Cosmetic, PriceCurrency = CurrencyType.Gems, PriceAmount = 50, IconKey = "icon_skin" },
                new ShopItemData { Id = "starter_pack", DisplayName = "Starter Pack", Category = ShopCategory.Bundle, PriceCurrency = CurrencyType.Coins, PriceAmount = 500, IconKey = "icon_bundle" }
            };
        }

        public IReadOnlyList<ShopItemData> GetItems(ShopCategory category)
        {
            var list = new List<ShopItemData>(_items.Length);
            foreach (var item in _items) if (item.Category == category) list.Add(item);
            return list;
        }

        public IReadOnlyList<ShopItemData> GetAllItems() => _items;

        public ShopItemData GetItem(string itemId)
        {
            foreach (var item in _items) if (item.Id == itemId) return item;
            return default;
        }

        public bool CanAfford(string itemId)
        {
            var item = GetItem(itemId);
            return !string.IsNullOrEmpty(item.Id);
        }

        public PurchaseResult Purchase(string itemId)
        {
            var item = GetItem(itemId);
            return string.IsNullOrEmpty(item.Id) ? PurchaseResult.Failed : PurchaseResult.Success;
        }
    }

    public sealed class DemoAdsService : IAdsService
    {
        public bool IsRewardedAdReady() => true;
        public bool IsInterstitialReady() => true;
        public void ShowRewardedAd(Action<bool> onComplete) => onComplete?.Invoke(true);
        public void ShowInterstitial(Action onClosed) => onClosed?.Invoke();
    }

    public sealed class DemoTimeService : ITimeService
    {
        public DateTime GetServerTimeUtc() => DateTime.UtcNow;
        public TimeSpan GetTimeUntil(DateTime utcTarget) => utcTarget - DateTime.UtcNow;
        public bool HasElapsed(DateTime utcSince, TimeSpan duration) => DateTime.UtcNow - utcSince >= duration;
    }

    public sealed class DemoAudioRouter : IUIAudioRouter
    {
        public void Play(UIAudioCue cue) { }
    }

    public sealed class DemoLocalizationService : IUILocalizationService
    {
        private static readonly IReadOnlyList<string> Languages = new[] { "en", "es" };
        private string _current = "en";

        public event Action<string> OnLanguageChanged;
        public string CurrentLanguage => _current;
        public IReadOnlyList<string> AvailableLanguages => Languages;

        public void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code) || _current == code) return;
            _current = code;
            OnLanguageChanged?.Invoke(_current);
        }
    }
}
