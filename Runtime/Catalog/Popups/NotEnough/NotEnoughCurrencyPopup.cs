using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.NotEnough
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimNotEnoughCurrencyPopup))]
    public sealed class NotEnoughCurrencyPopup : UIModule<NotEnoughCurrencyPopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Message TMP label. Falls back to a generated string when DTO Message is empty.")]
            public TMP_Text MessageLabel;
            [Tooltip("Image showing the missing currency (Theme.IconCoin / Theme.IconGem).")]
            public Image CurrencyIcon;
            [Tooltip("TMP label inside Buy More button.")]
            public TMP_Text BuyMoreLabel;
            [Tooltip("TMP label inside Watch Ad button.")]
            public TMP_Text WatchAdLabel;
            [Tooltip("TMP label inside Decline button.")]
            public TMP_Text DeclineLabel;
            [Tooltip("Buy More button. Hidden when DTO ShowBuyMore = false.")]
            public Button BuyMoreButton;
            [Tooltip("Watch Ad button. Hidden when DTO ShowWatchAd = false. Disabled when IAdsService.IsRewardedAdReady() = false.")]
            public Button WatchAdButton;
            [Tooltip("Decline button. Hidden when DTO ShowDecline = false (default).")]
            public Button DeclineButton;
            [Tooltip("Full-screen backdrop button. Routes to OnDeclined when DTO CloseOnBackdrop = true (default).")]
            public Button BackdropButton;
            [Tooltip("Image whose color is tinted with Theme.WarningColor on the header band.")]
            public Image HeaderTint;
        }

        [SerializeField] private Refs _refs;

        public event Action<CurrencyType, int> OnBuyMoreRequested;
        public event Action<CurrencyType, int> OnWatchAdRequested;
        public event Action OnDeclined;
        public event Action OnDismissed;

        private NotEnoughCurrencyPopupData _data;
        private IUIAnimator _animator;

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        internal void SetAnimatorForTests(IUIAnimator animator)
        {
            _animator = animator;
        }

        internal void InvokeBuyMoreForTests() => HandleBuyMore();
        internal void InvokeWatchAdForTests() => HandleWatchAd();
        internal void InvokeDeclineForTests() => HandleDecline();

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            if (_refs.BuyMoreButton != null) _refs.BuyMoreButton.onClick.AddListener(HandleBuyMore);
            if (_refs.WatchAdButton != null) _refs.WatchAdButton.onClick.AddListener(HandleWatchAd);
            if (_refs.DeclineButton != null) _refs.DeclineButton.onClick.AddListener(HandleDecline);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.AddListener(HandleBackdrop);
        }

        private void OnDestroy()
        {
            if (_refs.BuyMoreButton != null) _refs.BuyMoreButton.onClick.RemoveListener(HandleBuyMore);
            if (_refs.WatchAdButton != null) _refs.WatchAdButton.onClick.RemoveListener(HandleWatchAd);
            if (_refs.DeclineButton != null) _refs.DeclineButton.onClick.RemoveListener(HandleDecline);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.RemoveListener(HandleBackdrop);
            ClearAllEvents();
        }

        public override void Bind(NotEnoughCurrencyPopupData data)
        {
            ClearAllEvents();
            if (Animator != null && Animator.IsPlaying) Animator.Skip();
            _data = data ?? new NotEnoughCurrencyPopupData();
            WarnIfTrappedConfiguration(_data);
            ApplyTexts(_data);
            ApplyIcon(_data);
            ApplyTint();
            ApplyVisibility(_data);
            RefreshAdAvailability();
            IsDismissing = false;
        }

        private void WarnIfTrappedConfiguration(NotEnoughCurrencyPopupData data)
        {
            if (data.ShowBuyMore || data.ShowWatchAd || data.ShowDecline || data.CloseOnBackdrop) return;
            Debug.LogWarning("[NotEnoughCurrencyPopup] All CTAs hidden and CloseOnBackdrop=false. Only hardware back-press can dismiss this popup — devices without a back button (iOS) will be stuck.", this);
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            if (Animator == null) return;
            Animator.ApplyPreset(ResolveAnimPreset());
            Animator.PlayShow();
        }

        public override void OnHide()
        {
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            HandleDecline();
        }

        private void HandleBuyMore()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnBuyMoreRequested?.Invoke(_data.Currency, _data.Missing);
            DismissWithAnimation();
        }

        private void HandleWatchAd()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnWatchAdRequested?.Invoke(_data.Currency, _data.Missing);
            DismissWithAnimation();
        }

        private void HandleDecline()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnDeclined?.Invoke();
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            HandleDecline();
        }

        private void DismissWithAnimation()
        {
            Services?.Audio?.Play(UIAudioCue.PopupClose);
            if (Animator == null) { FinalizeDismissal(); return; }
            Animator.PlayHide(FinalizeDismissal);
        }

        private void FinalizeDismissal()
        {
            RaiseDismissRequested();
            OnDismissed?.Invoke();
        }

        private void ClearAllEvents()
        {
            OnBuyMoreRequested = null;
            OnWatchAdRequested = null;
            OnDeclined = null;
            OnDismissed = null;
        }

        private void ApplyTexts(NotEnoughCurrencyPopupData data)
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(data.Title);
            if (_refs.MessageLabel != null) _refs.MessageLabel.SetText(ResolveMessage(data));
            if (_refs.BuyMoreLabel != null) _refs.BuyMoreLabel.SetText(data.BuyMoreLabel);
            if (_refs.WatchAdLabel != null) _refs.WatchAdLabel.SetText(data.WatchAdLabel);
            if (_refs.DeclineLabel != null) _refs.DeclineLabel.SetText(data.DeclineLabel);
        }

        private static string ResolveMessage(NotEnoughCurrencyPopupData data)
        {
            if (!string.IsNullOrEmpty(data.Message)) return data.Message;
            return $"You need {data.Missing} more {data.Currency}.";
        }

        private void ApplyIcon(NotEnoughCurrencyPopupData data)
        {
            if (_refs.CurrencyIcon == null) return;
            var sprite = ResolveIconSprite(data.Currency);
            _refs.CurrencyIcon.sprite = sprite;
            _refs.CurrencyIcon.enabled = sprite != null;
        }

        private Sprite ResolveIconSprite(CurrencyType currency)
        {
            if (Theme == null) return null;
            switch (currency)
            {
                case CurrencyType.Coins: return Theme.IconCoin;
                case CurrencyType.Gems:  return Theme.IconGem;
                default:                 return null;
            }
        }

        private void ApplyTint()
        {
            if (_refs.HeaderTint == null || Theme == null) return;
            _refs.HeaderTint.color = Theme.WarningColor;
        }

        private void ApplyVisibility(NotEnoughCurrencyPopupData data)
        {
            if (_refs.BuyMoreButton != null) _refs.BuyMoreButton.gameObject.SetActive(data.ShowBuyMore);
            if (_refs.WatchAdButton != null) _refs.WatchAdButton.gameObject.SetActive(data.ShowWatchAd);
            if (_refs.DeclineButton != null) _refs.DeclineButton.gameObject.SetActive(data.ShowDecline);
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = data.CloseOnBackdrop;
        }

        private void RefreshAdAvailability()
        {
            if (_refs.WatchAdButton == null) return;
            var ads = Services?.Ads;
            var ready = ads == null || ads.IsRewardedAdReady();
            _refs.WatchAdButton.interactable = ready;
        }
    }
}
