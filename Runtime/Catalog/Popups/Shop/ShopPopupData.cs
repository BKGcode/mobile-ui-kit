using System;

namespace KitforgeLabs.MobileUIKit.Catalog.Shop
{
    [Serializable]
    public class ShopPopupData
    {
        public string Title = "Shop";
        public ShopCategoryFilter Category = ShopCategoryFilter.All;
        public bool CloseOnBackdrop = true;
    }
}
