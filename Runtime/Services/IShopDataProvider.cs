using System.Collections.Generic;

namespace KitforgeLabs.UIKit.Services
{
    public interface IShopDataProvider
    {
        IReadOnlyList<ShopItemData> GetItems(ShopCategory category);
        IReadOnlyList<ShopItemData> GetAllItems();
        ShopItemData GetItem(string itemId);
        bool CanAfford(string itemId);
        PurchaseResult Purchase(string itemId);
    }
}
