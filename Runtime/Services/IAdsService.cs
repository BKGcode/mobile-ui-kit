using System;

namespace KitforgeLabs.MobileUIKit.Services
{
    public interface IAdsService
    {
        bool IsRewardedAdReady();
        bool IsInterstitialReady();
        void ShowRewardedAd(Action<bool> onComplete);
        void ShowInterstitial(Action onClosed);
    }
}
