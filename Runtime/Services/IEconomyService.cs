using System;

namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IEconomyService
    {
        int Get(CurrencyType currency);
        bool Spend(CurrencyType currency, int amount);
        void Add(CurrencyType currency, int amount);
        bool CanAfford(CurrencyType currency, int amount);

        event Action<CurrencyType, int> OnChanged;
    }
}
