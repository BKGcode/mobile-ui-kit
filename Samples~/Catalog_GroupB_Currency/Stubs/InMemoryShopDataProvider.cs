using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupB
{
    [RequireComponent(typeof(InMemoryEconomyService))]
    public sealed class InMemoryShopDataProvider : MonoBehaviour, IShopDataProvider
    {
        [SerializeField] private List<ShopItemData> _items = new List<ShopItemData>();
        [SerializeField] private List<string> _ownedCosmetics = new List<string>();

        private InMemoryEconomyService _economy;

        private void Awake()
        {
            _economy = GetComponent<InMemoryEconomyService>();
            if (_items.Count == 0) SeedDefaults();
        }

        private void SeedDefaults()
        {
            _items.Add(new ShopItemData { Id = "coins_small", DisplayName = "100 Coins", Category = ShopCategory.Currency, PriceCurrency = CurrencyType.Gems, PriceAmount = 1, IconKey = "coin" });
            _items.Add(new ShopItemData { Id = "coins_big", DisplayName = "1000 Coins", Category = ShopCategory.Currency, PriceCurrency = CurrencyType.Gems, PriceAmount = 8, IconKey = "coin" });
            _items.Add(new ShopItemData { Id = "boost_xp", DisplayName = "XP Boost x2", Category = ShopCategory.Consumable, PriceCurrency = CurrencyType.Coins, PriceAmount = 200, IconKey = "boost" });
            _items.Add(new ShopItemData { Id = "skin_gold", DisplayName = "Golden Skin", Category = ShopCategory.Cosmetic, PriceCurrency = CurrencyType.Gems, PriceAmount = 20, IconKey = "skin" });
        }

        public IReadOnlyList<ShopItemData> GetAllItems() => _items;

        public IReadOnlyList<ShopItemData> GetItems(ShopCategory category)
        {
            var result = new List<ShopItemData>();
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Category == category) result.Add(_items[i]);
            }
            return result;
        }

        public ShopItemData GetItem(string itemId)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == itemId) return _items[i];
            }
            return default;
        }

        public bool CanAfford(string itemId)
        {
            var item = GetItem(itemId);
            if (string.IsNullOrEmpty(item.Id) || _economy == null) return false;
            return _economy.CanAfford(item.PriceCurrency, item.PriceAmount);
        }

        public PurchaseResult Purchase(string itemId)
        {
            var item = GetItem(itemId);
            if (string.IsNullOrEmpty(item.Id)) return PurchaseResult.Failed;
            if (item.Category == ShopCategory.Cosmetic && _ownedCosmetics.Contains(item.Id)) return PurchaseResult.AlreadyOwned;
            if (!_economy.CanAfford(item.PriceCurrency, item.PriceAmount)) return PurchaseResult.InsufficientFunds;

            var spent = _economy.Spend(item.PriceCurrency, item.PriceAmount);
            if (!spent) return PurchaseResult.Failed;

            GrantItem(item);
            return PurchaseResult.Success;
        }

        private void GrantItem(ShopItemData item)
        {
            switch (item.Category)
            {
                case ShopCategory.Currency:
                    if (item.Id == "coins_small") _economy.Add(CurrencyType.Coins, 100);
                    else if (item.Id == "coins_big") _economy.Add(CurrencyType.Coins, 1000);
                    break;
                case ShopCategory.Cosmetic:
                    _ownedCosmetics.Add(item.Id);
                    break;
            }
        }
    }
}
