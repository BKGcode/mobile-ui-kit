using System;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupB
{
    public sealed class InMemoryEconomyService : MonoBehaviour, IEconomyService
    {
        [SerializeField] private int _coins = 250;
        [SerializeField] private int _gems = 5;
        [SerializeField] private int _energy = 30;

        public event Action<CurrencyType, int> OnChanged;

        public int Get(CurrencyType currency) => currency switch
        {
            CurrencyType.Coins  => _coins,
            CurrencyType.Gems   => _gems,
            CurrencyType.Energy => _energy,
            _ => 0
        };

        public bool CanAfford(CurrencyType currency, int amount) => Get(currency) >= amount;

        public bool Spend(CurrencyType currency, int amount)
        {
            if (amount < 0 || Get(currency) < amount) return false;
            ApplyDelta(currency, -amount);
            return true;
        }

        public void Add(CurrencyType currency, int amount)
        {
            if (amount <= 0) return;
            ApplyDelta(currency, amount);
        }

        private void ApplyDelta(CurrencyType currency, int delta)
        {
            switch (currency)
            {
                case CurrencyType.Coins:  _coins  += delta; OnChanged?.Invoke(currency, _coins);  break;
                case CurrencyType.Gems:   _gems   += delta; OnChanged?.Invoke(currency, _gems);   break;
                case CurrencyType.Energy: _energy += delta; OnChanged?.Invoke(currency, _energy); break;
            }
        }

        [ContextMenu("Debug — Add 1000 coins")]
        private void DebugAddCoins() => Add(CurrencyType.Coins, 1000);

        [ContextMenu("Debug — Add 50 gems")]
        private void DebugAddGems() => Add(CurrencyType.Gems, 50);

        [ContextMenu("Debug — Add 50 energy")]
        private void DebugAddEnergy() => Add(CurrencyType.Energy, 50);

        [ContextMenu("Debug — Reset to defaults")]
        private void DebugReset()
        {
            _coins = 250;
            _gems = 5;
            _energy = 30;
            OnChanged?.Invoke(CurrencyType.Coins, _coins);
            OnChanged?.Invoke(CurrencyType.Gems, _gems);
            OnChanged?.Invoke(CurrencyType.Energy, _energy);
        }
    }
}
