using System.Collections.Generic;

namespace KitforgeLabs.MobileUIKit.Services
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
