using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.LevelComplete
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimLevelCompletePopup))]
    public sealed class LevelCompletePopup : UIModule<LevelCompletePopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Optional subtitle (e.g. 'Level 7'). Hidden when DTO LevelLabel is empty.")]
            public TMP_Text LevelLabel;
            [Tooltip("Star Image slots (typically 3). Slots 0..Stars-1 use Theme.StarFilledSprite; rest use Theme.StarEmptySprite.")]
            public Image[] StarImages;
            [Tooltip("Score TMP label. Driven by DTO Score (rollup tween deferred to _tween session).")]
            public TMP_Text ScoreLabel;
            [Tooltip("Best score TMP label. Hidden when BestScore <= 0.")]
            public TMP_Text BestScoreLabel;
            [Tooltip("New-best banner GameObject. Visible when DTO IsNewBest = true.")]
            public GameObject NewBestBanner;
            [Tooltip("Next CTA button. Hidden when DTO ShowNext = false (last level).")]
            public Button NextButton;
            [Tooltip("TMP label inside Next button.")]
            public TMP_Text NextLabelText;
            [Tooltip("Retry CTA button. Also fired by back press per L7.")]
            public Button RetryButton;
            [Tooltip("TMP label inside Retry button.")]
            public TMP_Text RetryLabelText;
            [Tooltip("Main Menu CTA button. Hidden by default (DTO ShowMainMenu = false).")]
            public Button MainMenuButton;
            [Tooltip("TMP label inside Main Menu button.")]
            public TMP_Text MainMenuLabelText;
            [Tooltip("Full-screen backdrop button. Routes to retry path when DTO CloseOnBackdrop = true.")]
            public Button BackdropButton;
        }

        [SerializeField] private Refs _refs;

        public event Action<LevelCompletePopupData> OnNextRequested;
        public event Action<LevelCompletePopupData> OnRetryRequested;
        public event Action<LevelCompletePopupData> OnMainMenuRequested;
        public event Action OnDismissed;

        private LevelCompletePopupData _data;
        private IUIAnimator _animator;

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        internal void SetAnimatorForTests(IUIAnimator animator) => _animator = animator;
        internal void InvokeNextForTests() => HandleNext();
        internal void InvokeRetryForTests() => HandleRetry();
        internal void InvokeMainMenuForTests() => HandleMainMenu();
        internal void InvokeBackdropForTests() => HandleBackdrop();
        internal LevelCompletePopupData DataForTests => _data;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            if (_refs.NextButton != null) _refs.NextButton.onClick.AddListener(HandleNext);
            if (_refs.RetryButton != null) _refs.RetryButton.onClick.AddListener(HandleRetry);
            if (_refs.MainMenuButton != null) _refs.MainMenuButton.onClick.AddListener(HandleMainMenu);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.AddListener(HandleBackdrop);
        }

        private void OnDestroy()
        {
            if (_refs.NextButton != null) _refs.NextButton.onClick.RemoveListener(HandleNext);
            if (_refs.RetryButton != null) _refs.RetryButton.onClick.RemoveListener(HandleRetry);
            if (_refs.MainMenuButton != null) _refs.MainMenuButton.onClick.RemoveListener(HandleMainMenu);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.RemoveListener(HandleBackdrop);
            ClearAllEvents();
        }

        public override void Bind(LevelCompletePopupData data)
        {
            ClearAllEvents();
            if (Animator != null && Animator.IsPlaying) Animator.Skip();
            _data = data ?? new LevelCompletePopupData();
            ClampStars();
            EnsureAtLeastOneCta();
            ApplyTexts();
            ApplyStars();
            ApplyVisibility();
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            if (_data.IsNewBest) Services?.Audio?.Play(UIAudioCue.Success);
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
            FireRetry();
        }

        private void HandleNext()
        {
            if (IsDismissing || _data == null || !_data.ShowNext) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnNextRequested?.Invoke(_data);
            DismissWithAnimation();
        }

        private void HandleRetry() => FireRetry();

        private void FireRetry()
        {
            if (IsDismissing || _data == null) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnRetryRequested?.Invoke(_data);
            DismissWithAnimation();
        }

        private void HandleMainMenu()
        {
            if (IsDismissing || _data == null || !_data.ShowMainMenu) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnMainMenuRequested?.Invoke(_data);
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            FireRetry();
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
            OnNextRequested = null;
            OnRetryRequested = null;
            OnMainMenuRequested = null;
            OnDismissed = null;
        }

        private void ClampStars()
        {
            if (_data.Stars >= 0 && _data.Stars <= 3) return;
            Debug.LogWarning($"LevelCompletePopup: Stars={_data.Stars} out of range [0,3]. Clamped.", this);
            _data.Stars = Mathf.Clamp(_data.Stars, 0, 3);
        }

        private void EnsureAtLeastOneCta()
        {
            if (_data.ShowNext || _data.ShowRetry || _data.ShowMainMenu) return;
            Debug.LogError("LevelCompletePopup: All CTAs hidden (ShowNext/ShowRetry/ShowMainMenu all false). Forcing ShowRetry=true so player has an exit.", this);
            _data.ShowRetry = true;
        }

        private void ApplyTexts()
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(_data.Title);
            if (_refs.LevelLabel != null)
            {
                _refs.LevelLabel.SetText(_data.LevelLabel);
                _refs.LevelLabel.gameObject.SetActive(!string.IsNullOrEmpty(_data.LevelLabel));
            }
            if (_refs.ScoreLabel != null) _refs.ScoreLabel.SetText(_data.Score.ToString("N0"));
            if (_refs.BestScoreLabel != null) _refs.BestScoreLabel.SetText(_data.BestScore.ToString("N0"));
            if (_refs.NextLabelText != null) _refs.NextLabelText.SetText(_data.NextLabel);
            if (_refs.RetryLabelText != null) _refs.RetryLabelText.SetText(_data.RetryLabel);
            if (_refs.MainMenuLabelText != null) _refs.MainMenuLabelText.SetText(_data.MainMenuLabel);
        }

        private void ApplyStars()
        {
            if (_refs.StarImages == null) return;
            var filled = Theme != null ? Theme.StarFilledSprite : null;
            var empty = Theme != null ? Theme.StarEmptySprite : null;
            for (var i = 0; i < _refs.StarImages.Length; i++)
            {
                var img = _refs.StarImages[i];
                if (img == null) continue;
                img.sprite = i < _data.Stars ? filled : empty;
            }
        }

        private void ApplyVisibility()
        {
            if (_refs.NextButton != null) _refs.NextButton.gameObject.SetActive(_data.ShowNext);
            if (_refs.RetryButton != null) _refs.RetryButton.gameObject.SetActive(_data.ShowRetry);
            if (_refs.MainMenuButton != null) _refs.MainMenuButton.gameObject.SetActive(_data.ShowMainMenu);
            if (_refs.NewBestBanner != null) _refs.NewBestBanner.SetActive(_data.IsNewBest);
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = _data.CloseOnBackdrop;
        }
    }
}
