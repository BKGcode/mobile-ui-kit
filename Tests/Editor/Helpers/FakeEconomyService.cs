using System;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers
{
    internal sealed class FakeEconomyService : IEconomyService
    {
        private int _coins;
        private int _gems;

        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;

        public int GetCoins() => _coins;
        public int GetGems() => _gems;

        public bool CanAfford(CurrencyType currency, int amount) =>
            currency == CurrencyType.Coins ? _coins >= amount : _gems >= amount;

        public void SetCoins(int value) { _coins = value; OnCoinsChanged?.Invoke(_coins); }
        public void SetGems(int value) { _gems = value; OnGemsChanged?.Invoke(_gems); }

        public bool SpendCoins(int amount)
        {
            if (_coins < amount) return false;
            _coins -= amount;
            OnCoinsChanged?.Invoke(_coins);
            return true;
        }

        public bool SpendGems(int amount)
        {
            if (_gems < amount) return false;
            _gems -= amount;
            OnGemsChanged?.Invoke(_gems);
            return true;
        }

        public void AddCoins(int amount)
        {
            _coins += amount;
            OnCoinsChanged?.Invoke(_coins);
        }

        public void AddGems(int amount)
        {
            _gems += amount;
            OnGemsChanged?.Invoke(_gems);
        }
    }
}
