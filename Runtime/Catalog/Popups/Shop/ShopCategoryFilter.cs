using System;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.Shop
{
    [Serializable]
    public struct ShopCategoryFilter
    {
        public bool UseFilter;
        public ShopCategory Category;

        public static ShopCategoryFilter All => new ShopCategoryFilter { UseFilter = false };

        public static ShopCategoryFilter Of(ShopCategory category) =>
            new ShopCategoryFilter { UseFilter = true, Category = category };
    }
}
