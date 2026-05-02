using System;
using KitforgeLabs.MobileUIKit.Services;

namespace KitforgeLabs.MobileUIKit.Catalog.NotEnough
{
    [Serializable]
    public class NotEnoughCurrencyPopupData
    {
        public CurrencyType Currency = CurrencyType.Coins;
        public int Required;
        public int Missing;
        public string Title = "Not enough!";
        public string Message = string.Empty;
        public string BuyMoreLabel = "Buy more";
        public string WatchAdLabel = "Watch ad";
        public string DeclineLabel = "Cancel";
        public bool ShowBuyMore = true;
        public bool ShowWatchAd = true;
        public bool ShowDecline = false;
        public bool CloseOnBackdrop = true;
    }
}
