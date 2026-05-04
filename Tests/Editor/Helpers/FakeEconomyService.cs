using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers
{
    internal sealed class FakeEconomyService : IEconomyService
    {
        private readonly Dictionary<CurrencyType, int> _values = new Dictionary<CurrencyType, int>();

        public event Action<CurrencyType, int> OnChanged;

        public int Get(CurrencyType currency) => _values.TryGetValue(currency, out var v) ? v : 0;
        public bool CanAfford(CurrencyType currency, int amount) => Get(currency) >= amount;

        public bool Spend(CurrencyType currency, int amount)
        {
            var current = Get(currency);
            if (current < amount) return false;
            _values[currency] = current - amount;
            OnChanged?.Invoke(currency, _values[currency]);
            return true;
        }

        public void Add(CurrencyType currency, int amount)
        {
            _values[currency] = Get(currency) + amount;
            OnChanged?.Invoke(currency, _values[currency]);
        }

        public void SetCoins(int value) => SetCurrency(CurrencyType.Coins, value);
        public void SetGems(int value) => SetCurrency(CurrencyType.Gems, value);
        public void AddCoins(int amount) => Add(CurrencyType.Coins, amount);
        public void AddGems(int amount) => Add(CurrencyType.Gems, amount);

        public void SetCurrency(CurrencyType currency, int value)
        {
            _values[currency] = value;
            OnChanged?.Invoke(currency, value);
        }
    }
}
