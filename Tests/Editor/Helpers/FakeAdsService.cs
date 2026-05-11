using System;
using KitforgeLabs.UIKit.Services;

namespace KitforgeLabs.UIKit.Catalog.Tests.Helpers
{
    internal sealed class FakeAdsService : IAdsService
    {
        private bool _rewardedReady;
        private bool _interstitialReady;

        public bool IsRewardedAdReady() => _rewardedReady;
        public bool IsInterstitialReady() => _interstitialReady;
        public void ShowRewardedAd(Action<bool> onComplete) => onComplete?.Invoke(_rewardedReady);
        public void ShowInterstitial(Action onClosed) => onClosed?.Invoke();

        public void SetRewardedReady(bool ready) => _rewardedReady = ready;
        public void SetInterstitialReady(bool ready) => _interstitialReady = ready;
    }
}
