using System;

namespace KitforgeLabs.UIKit.Catalog.Shop
{
    [Serializable]
    public class ShopPopupData
    {
        public string Title = "Shop";
        public ShopCategoryFilter Category = ShopCategoryFilter.All;
        public bool CloseOnBackdrop = true;
    }
}
