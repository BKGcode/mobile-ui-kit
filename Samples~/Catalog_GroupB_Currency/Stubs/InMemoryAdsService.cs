using System;
using System.Collections;
using KitforgeLabs.MobileUIKit.Services;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogGroupB
{
    public sealed class InMemoryAdsService : MonoBehaviour, IAdsService
    {
        [SerializeField] private float _adDurationSeconds = 1f;
        [SerializeField] private bool _alwaysReady = true;
        [SerializeField] private bool _completeWithReward = true;

        public bool IsRewardedAdReady() => _alwaysReady;
        public bool IsInterstitialReady() => _alwaysReady;

        public void ShowRewardedAd(Action<bool> onComplete)
        {
            StartCoroutine(SimulateAd(onComplete, _completeWithReward));
        }

        public void ShowInterstitial(Action onClosed)
        {
            StartCoroutine(SimulateAd(_ => onClosed?.Invoke(), false));
        }

        private IEnumerator SimulateAd(Action<bool> onComplete, bool reward)
        {
            yield return new WaitForSecondsRealtime(_adDurationSeconds);
            onComplete?.Invoke(reward);
        }
    }
}
