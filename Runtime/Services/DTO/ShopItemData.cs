namespace KitforgeLabs.MobileUIKit.Services
{
    public enum ShopCategory
    {
        Currency,
        Consumable,
        Cosmetic,
        Bundle
    }

    public enum CurrencyType
    {
        Coins,
        Gems
    }

    [System.Serializable]
    public struct ShopItemData
    {
        public string Id;
        public string DisplayName;
        public ShopCategory Category;
        public CurrencyType PriceCurrency;
        public int PriceAmount;
        public string IconKey;
    }
}
