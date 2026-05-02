using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Reward
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimRewardPopup))]
    public sealed class RewardPopup : UIModule<RewardPopupData>
    {
        public const int ItemCurrencySentinel = -1;

        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Amount TMP label. Shown for Coins/Gems/Bundle (bundle uses joined lines). Hidden for Item.")]
            public TMP_Text AmountLabel;
            [Tooltip("Item id TMP label. Shown only for Item kind.")]
            public TMP_Text ItemLabel;
            [Tooltip("TMP label inside the claim button.")]
            public TMP_Text ClaimLabel;
            [Tooltip("Claim button. Triggers OnClaimed.")]
            public Button ClaimButton;
            [Tooltip("Full-screen backdrop button. Routes to claim only if RewardPopupData.CloseOnBackdrop = true.")]
            public Button BackdropButton;
            [Tooltip("Image showing reward icon. Filled from Theme.IconCoin/IconGem or RewardPopupData.IconOverride.")]
            public Image RewardIcon;
            [Tooltip("Image whose color is tinted with Theme.SuccessColor on the claim button.")]
            public Image ClaimTint;
        }

        [SerializeField] private Refs _refs;

        public event Action<CurrencyType, int> OnClaimed;
        public event Action OnDismissed;

        private RewardPopupData _data;
        private IUIAnimator _animator;
        private float _autoClaimRemaining;
        private bool _autoClaimActive;

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

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            if (_refs.ClaimButton != null) _refs.ClaimButton.onClick.AddListener(HandleClaim);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.AddListener(HandleBackdrop);
        }

        private void OnDestroy()
        {
            if (_refs.ClaimButton != null) _refs.ClaimButton.onClick.RemoveListener(HandleClaim);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.RemoveListener(HandleBackdrop);
            ClearAllEvents();
        }

        public override void Bind(RewardPopupData data)
        {
            ClearAllEvents();
            if (Animator != null && Animator.IsPlaying) Animator.Skip();
            _data = data ?? new RewardPopupData();
            CancelAutoClaim();
            ApplyTexts(_data);
            ApplyIcon(_data);
            ApplyTint();
            ApplyVisibility(_data);
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            StartAutoClaimIfRequested();
            if (Animator == null) return;
            Animator.ApplyPreset(ResolveAnimPreset());
            Animator.PlayShow();
        }

        public override void OnHide()
        {
            CancelAutoClaim();
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            HandleClaim();
        }

        public override void OnUpdate()
        {
            AdvanceAutoClaim(Time.unscaledDeltaTime);
        }

        internal void AdvanceAutoClaim(float deltaTime)
        {
            if (!_autoClaimActive || IsDismissing) return;
            _autoClaimRemaining -= deltaTime;
            if (_autoClaimRemaining > 0f) return;
            CancelAutoClaim();
            HandleClaim();
        }

        private void HandleClaim()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            CancelAutoClaim();
            Services?.Audio?.Play(UIAudioCue.Success);
            OnClaimed?.Invoke(ResolveCurrency(_data), ResolveAmount(_data));
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            HandleClaim();
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
            OnClaimed = null;
            OnDismissed = null;
        }

        private void StartAutoClaimIfRequested()
        {
            if (_data == null || _data.AutoClaimSeconds <= 0f) return;
            _autoClaimRemaining = _data.AutoClaimSeconds;
            _autoClaimActive = true;
        }

        private void CancelAutoClaim()
        {
            _autoClaimActive = false;
            _autoClaimRemaining = 0f;
        }

        private void ApplyTexts(RewardPopupData data)
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(data.Title);
            if (_refs.ClaimLabel != null) _refs.ClaimLabel.SetText(data.ClaimLabel);
            ApplyAmountAndItem(data);
        }

        private void ApplyAmountAndItem(RewardPopupData data)
        {
            switch (data.Kind)
            {
                case RewardKind.Coins:
                case RewardKind.Gems:
                    SetAmountText("+" + data.Amount);
                    SetItemText(string.Empty);
                    break;
                case RewardKind.Item:
                    SetAmountText(string.Empty);
                    SetItemText(string.IsNullOrEmpty(data.ItemId) ? "(item)" : data.ItemId);
                    break;
                case RewardKind.Bundle:
                    SetAmountText(BundleToText(data.BundleLines));
                    SetItemText(string.Empty);
                    break;
            }
        }

        private void SetAmountText(string text)
        {
            if (_refs.AmountLabel == null) return;
            _refs.AmountLabel.SetText(text);
            _refs.AmountLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        private void SetItemText(string text)
        {
            if (_refs.ItemLabel == null) return;
            _refs.ItemLabel.SetText(text);
            _refs.ItemLabel.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        private static string BundleToText(string[] lines)
        {
            if (lines == null || lines.Length == 0) return "(empty bundle)";
            return string.Join("\n", lines);
        }

        private void ApplyIcon(RewardPopupData data)
        {
            if (_refs.RewardIcon == null) return;
            var sprite = ResolveIconSprite(data);
            _refs.RewardIcon.sprite = sprite;
            _refs.RewardIcon.enabled = sprite != null;
        }

        private Sprite ResolveIconSprite(RewardPopupData data)
        {
            if (data.IconOverride != null) return data.IconOverride;
            if (Theme == null) return null;
            switch (data.Kind)
            {
                case RewardKind.Coins: return Theme.IconCoin;
                case RewardKind.Gems:  return Theme.IconGem;
                default:               return null;
            }
        }

        private void ApplyTint()
        {
            if (_refs.ClaimTint == null || Theme == null) return;
            _refs.ClaimTint.color = Theme.SuccessColor;
        }

        private void ApplyVisibility(RewardPopupData data)
        {
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = data.CloseOnBackdrop;
        }

        private static CurrencyType ResolveCurrency(RewardPopupData data)
        {
            if (data == null) return CurrencyType.Coins;
            switch (data.Kind)
            {
                case RewardKind.Coins: return CurrencyType.Coins;
                case RewardKind.Gems:  return CurrencyType.Gems;
                default:               return (CurrencyType)ItemCurrencySentinel;
            }
        }

        private static int ResolveAmount(RewardPopupData data)
        {
            if (data == null) return 0;
            return data.Kind == RewardKind.Coins || data.Kind == RewardKind.Gems ? data.Amount : 0;
        }
    }
}
