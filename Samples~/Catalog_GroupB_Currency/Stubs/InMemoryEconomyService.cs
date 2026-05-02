using System;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupB
{
    public sealed class InMemoryEconomyService : MonoBehaviour, IEconomyService
    {
        [SerializeField] private int _coins = 250;
        [SerializeField] private int _gems = 5;

        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;

        public int GetCoins() => _coins;
        public int GetGems() => _gems;

        public bool CanAfford(CurrencyType currency, int amount) =>
            currency == CurrencyType.Coins ? _coins >= amount : _gems >= amount;

        public bool SpendCoins(int amount)
        {
            if (amount < 0 || _coins < amount) return false;
            _coins -= amount;
            OnCoinsChanged?.Invoke(_coins);
            return true;
        }

        public bool SpendGems(int amount)
        {
            if (amount < 0 || _gems < amount) return false;
            _gems -= amount;
            OnGemsChanged?.Invoke(_gems);
            return true;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            _coins += amount;
            OnCoinsChanged?.Invoke(_coins);
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            _gems += amount;
            OnGemsChanged?.Invoke(_gems);
        }

        [ContextMenu("Debug — Add 1000 coins")]
        private void DebugAddCoins() => AddCoins(1000);

        [ContextMenu("Debug — Add 50 gems")]
        private void DebugAddGems() => AddGems(50);

        [ContextMenu("Debug — Reset to defaults")]
        private void DebugReset()
        {
            _coins = 250;
            _gems = 5;
            OnCoinsChanged?.Invoke(_coins);
            OnGemsChanged?.Invoke(_gems);
        }
    }
}
