using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Confirm
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimConfirmPopup))]
    public sealed class ConfirmPopup : UIModule<ConfirmPopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Message body TMP label.")]
            public TMP_Text MessageLabel;
            [Tooltip("TMP label inside the confirm button.")]
            public TMP_Text ConfirmLabel;
            [Tooltip("TMP label inside the cancel button.")]
            public TMP_Text CancelLabel;
            [Tooltip("Confirm button. Triggers OnConfirmed.")]
            public Button ConfirmButton;
            [Tooltip("Cancel button. Hidden when ConfirmPopupData.ShowCancel = false.")]
            public Button CancelButton;
            [Tooltip("Full-screen backdrop button. Closes only if ConfirmPopupData.CloseOnBackdrop = true.")]
            public Button BackdropButton;
            [Tooltip("Image whose color changes by Tone (Neutral=Primary, Destructive=Danger, Positive=Success).")]
            public Image ConfirmTint;
        }

        [SerializeField] private Refs _refs;

        public event Action OnConfirmed;
        public event Action OnCancelled;
        public event Action OnDismissed;

        private ConfirmPopupData _data;
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

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            if (_refs.ConfirmButton != null) _refs.ConfirmButton.onClick.AddListener(HandleConfirm);
            if (_refs.CancelButton != null) _refs.CancelButton.onClick.AddListener(HandleCancel);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.AddListener(HandleBackdrop);
        }

        private void OnDestroy()
        {
            if (_refs.ConfirmButton != null) _refs.ConfirmButton.onClick.RemoveListener(HandleConfirm);
            if (_refs.CancelButton != null) _refs.CancelButton.onClick.RemoveListener(HandleCancel);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.RemoveListener(HandleBackdrop);
            ClearAllEvents();
        }

        public override void Bind(ConfirmPopupData data)
        {
            ClearAllEvents();
            _data = data ?? new ConfirmPopupData();
            ApplyTexts(_data);
            ApplyTone(_data.Tone);
            ApplyVisibility(_data);
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            if (Animator == null) return;
            Animator.ApplyPreset(ResolvePreset());
            Animator.PlayShow();
        }

        public override void OnHide()
        {
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            if (_data != null && _data.ShowCancel) HandleCancel();
            else HandleConfirm();
        }

        private void HandleConfirm()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnConfirmed?.Invoke();
            DismissWithAnimation();
        }

        private void HandleCancel()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnCancelled?.Invoke();
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            if (_data.ShowCancel) HandleCancel();
            else HandleConfirm();
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
            OnConfirmed = null;
            OnCancelled = null;
            OnDismissed = null;
        }

        private void ApplyTexts(ConfirmPopupData data)
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(data.Title);
            if (_refs.MessageLabel != null) _refs.MessageLabel.SetText(data.Message);
            if (_refs.ConfirmLabel != null) _refs.ConfirmLabel.SetText(data.ConfirmLabel);
            if (_refs.CancelLabel != null) _refs.CancelLabel.SetText(data.CancelLabel);
        }

        private void ApplyVisibility(ConfirmPopupData data)
        {
            if (_refs.CancelButton != null) _refs.CancelButton.gameObject.SetActive(data.ShowCancel);
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = data.CloseOnBackdrop;
        }

        private void ApplyTone(ConfirmTone tone)
        {
            if (_refs.ConfirmTint == null || Theme == null) return;
            _refs.ConfirmTint.color = ToneToColor(tone, Theme);
        }

        private static Color ToneToColor(ConfirmTone tone, UIThemeConfig theme)
        {
            switch (tone)
            {
                case ConfirmTone.Destructive: return theme.DangerColor;
                case ConfirmTone.Positive:    return theme.SuccessColor;
                default:                      return theme.PrimaryColor;
            }
        }

        private UIAnimPreset ResolvePreset()
        {
            if (Theme == null || Theme.AnimPresetLibrary == null) return null;
            var style = AnimStyleOverride ?? Theme.DefaultAnimStyle;
            return Theme.AnimPresetLibrary.Resolve(style);
        }
    }
}
