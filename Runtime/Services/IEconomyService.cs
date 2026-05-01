using System;

namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IEconomyService
    {
        int GetCoins();
        int GetGems();
        bool CanAfford(CurrencyType currency, int amount);
        bool SpendCoins(int amount);
        bool SpendGems(int amount);
        void AddCoins(int amount);
        void AddGems(int amount);

        event Action<int> OnCoinsChanged;
        event Action<int> OnGemsChanged;
    }
}
