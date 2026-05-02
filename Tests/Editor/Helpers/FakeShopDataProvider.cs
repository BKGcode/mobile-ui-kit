using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.Tests.Helpers
{
    internal sealed class FakeShopDataProvider : IShopDataProvider
    {
        private readonly List<ShopItemData> _items = new List<ShopItemData>();
        private readonly Dictionary<string, PurchaseResult> _purchaseResults = new Dictionary<string, PurchaseResult>();

        public void SetItems(IEnumerable<ShopItemData> items)
        {
            _items.Clear();
            if (items != null) _items.AddRange(items);
        }

        public void QueuePurchaseResult(string itemId, PurchaseResult result) => _purchaseResults[itemId] = result;

        public IReadOnlyList<ShopItemData> GetItems(ShopCategory category)
        {
            var filtered = new List<ShopItemData>();
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Category == category) filtered.Add(_items[i]);
            }
            return filtered;
        }

        public IReadOnlyList<ShopItemData> GetAllItems() => _items;

        public ShopItemData GetItem(string itemId)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Id == itemId) return _items[i];
            }
            return default;
        }

        public bool CanAfford(string itemId) => true;

        public PurchaseResult Purchase(string itemId)
        {
            return _purchaseResults.TryGetValue(itemId, out var result) ? result : PurchaseResult.Success;
        }
    }
}
