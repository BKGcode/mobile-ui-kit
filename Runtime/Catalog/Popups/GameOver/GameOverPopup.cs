using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.GameOver
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimGameOverPopup))]
    public sealed class GameOverPopup : UIModule<GameOverPopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Subtitle TMP label. Hidden when DTO Subtitle is empty.")]
            public TMP_Text SubtitleLabel;
            [Tooltip("Score block GameObject. Hidden when DTO Score < 0 (per GO8).")]
            public GameObject ScoreBlock;
            [Tooltip("Score TMP label inside the score block. Formatted with N0 thousand separators.")]
            public TMP_Text ScoreLabel;
            [Tooltip("Continue-with-Ad button. Hidden when ContinueMode excludes Ad or session limit reached. Disabled (interactable=false) when IAdsService is null or rewarded ad not ready.")]
            public Button ContinueAdButton;
            [Tooltip("TMP label inside the Continue-with-Ad button.")]
            public TMP_Text ContinueAdLabelText;
            [Tooltip("Continue-with-Currency button. Hidden when ContinueMode excludes Currency or session limit reached. Visually grayed (alpha 0.5) when player can't afford; click still emits OnContinueAffordCheckFailed (GO3). When IEconomyService is null, button is non-interactable and click is a no-op.")]
            public Button ContinueCurrencyButton;
            [Tooltip("TMP label inside the Continue-with-Currency button. Token {amount} is replaced with DTO ContinueAmount.")]
            public TMP_Text ContinueCurrencyLabelText;
            [Tooltip("Currency icon shown next to the Continue-with-Currency button. Sprite resolved from Theme by DTO ContinueCurrency (Coins/Gems/Energy).")]
            public Image ContinueCurrencyIcon;
            [Tooltip("Restart CTA button. Hidden when DTO ShowRestart = false. Also fired by back press when DTO BackPressBehavior = Restart.")]
            public Button RestartButton;
            [Tooltip("TMP label inside the Restart button.")]
            public TMP_Text RestartLabelText;
            [Tooltip("Main Menu CTA button. Hidden when DTO ShowMainMenu = false. Also fired by back press when DTO BackPressBehavior = MainMenu.")]
            public Button MainMenuButton;
            [Tooltip("TMP label inside the Main Menu button.")]
            public TMP_Text MainMenuLabelText;
            [Tooltip("Full-screen backdrop button. Routes to BackPressBehavior path when DTO CloseOnBackdrop = true.")]
            public Button BackdropButton;
            [Tooltip("Image whose color is tinted with Theme.FailureColor on the header band.")]
            public Image HeaderTint;
        }

        private static readonly Color GrayedColor = new Color(1f, 1f, 1f, 0.5f);
        private static readonly Color FullColor = Color.white;

        [SerializeField] private Refs _refs;

        public event Action OnContinueWithAdRequested;
        public event Action<CurrencyType, int> OnContinueWithCurrencyRequested;
        public event Action<CurrencyType, int> OnContinueAffordCheckFailed;
        public event Action OnRestartRequested;
        public event Action OnMainMenuRequested;
        public event Action OnDismissed;

        private GameOverPopupData _data;
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
        internal void InvokeContinueAdForTests() => HandleContinueAd();
        internal void InvokeContinueCurrencyForTests() => HandleContinueCurrency();
        internal void InvokeRestartForTests() => HandleRestart();
        internal void InvokeMainMenuForTests() => HandleMainMenu();
        internal void InvokeBackdropForTests() => HandleBackdrop();
        internal GameOverPopupData DataForTests => _data;

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            if (_refs.ContinueAdButton != null) _refs.ContinueAdButton.onClick.AddListener(HandleContinueAd);
            if (_refs.ContinueCurrencyButton != null) _refs.ContinueCurrencyButton.onClick.AddListener(HandleContinueCurrency);
            if (_refs.RestartButton != null) _refs.RestartButton.onClick.AddListener(HandleRestart);
            if (_refs.MainMenuButton != null) _refs.MainMenuButton.onClick.AddListener(HandleMainMenu);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.AddListener(HandleBackdrop);
        }

        private void OnDestroy()
        {
            if (_refs.ContinueAdButton != null) _refs.ContinueAdButton.onClick.RemoveListener(HandleContinueAd);
            if (_refs.ContinueCurrencyButton != null) _refs.ContinueCurrencyButton.onClick.RemoveListener(HandleContinueCurrency);
            if (_refs.RestartButton != null) _refs.RestartButton.onClick.RemoveListener(HandleRestart);
            if (_refs.MainMenuButton != null) _refs.MainMenuButton.onClick.RemoveListener(HandleMainMenu);
            if (_refs.BackdropButton != null) _refs.BackdropButton.onClick.RemoveListener(HandleBackdrop);
            ClearAllEvents();
        }

        public override void Bind(GameOverPopupData data)
        {
            ClearAllEvents();
            if (Animator != null && Animator.IsPlaying) Animator.Skip();
            _data = data ?? new GameOverPopupData();
            EnsureAtLeastOneCta();
            ApplyTexts();
            ApplyIcon();
            ApplyTint();
            ApplyVisibility();
            ApplyContinueGating();
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            Services?.Audio?.Play(UIAudioCue.Error);
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
            if (IsDismissing || _data == null) return;
            switch (_data.BackPressBehavior)
            {
                case BackPressBehavior.Restart:  FireRestart();  return;
                case BackPressBehavior.MainMenu: FireMainMenu(); return;
                default:                         return;
            }
        }

        private void HandleContinueAd()
        {
            if (IsDismissing || _data == null) return;
            if (!IsContinueAdShown()) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnContinueWithAdRequested?.Invoke();
            DismissWithAnimation();
        }

        private void HandleContinueCurrency()
        {
            if (IsDismissing || _data == null) return;
            if (!IsContinueCurrencyShown()) return;
            var economy = Services?.Economy;
            if (economy == null) return;
            if (!economy.CanAfford(_data.ContinueCurrency, _data.ContinueAmount))
            {
                OnContinueAffordCheckFailed?.Invoke(_data.ContinueCurrency, _data.ContinueAmount);
                return;
            }
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnContinueWithCurrencyRequested?.Invoke(_data.ContinueCurrency, _data.ContinueAmount);
            DismissWithAnimation();
        }

        private void HandleRestart() => FireRestart();
        private void HandleMainMenu() => FireMainMenu();

        private void FireRestart()
        {
            if (IsDismissing || _data == null || !_data.ShowRestart) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnRestartRequested?.Invoke();
            DismissWithAnimation();
        }

        private void FireMainMenu()
        {
            if (IsDismissing || _data == null || !_data.ShowMainMenu) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            OnMainMenuRequested?.Invoke();
            DismissWithAnimation();
        }

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            OnBackPressed();
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
            OnContinueWithAdRequested = null;
            OnContinueWithCurrencyRequested = null;
            OnContinueAffordCheckFailed = null;
            OnRestartRequested = null;
            OnMainMenuRequested = null;
            OnDismissed = null;
        }

        private void EnsureAtLeastOneCta()
        {
            if (IsContinueAdShown() || IsContinueCurrencyShown() || _data.ShowRestart || _data.ShowMainMenu) return;
            Debug.LogError("GameOverPopup: All CTAs hidden (Continue limit reached + ShowRestart=false + ShowMainMenu=false). Forcing ShowMainMenu=true so player has an exit.", this);
            _data.ShowMainMenu = true;
        }

        private bool ContinuesAvailable() => _data.ContinuesUsedThisSession < _data.MaxContinuesPerSession;
        private bool IsContinueAdShown() => ContinuesAvailable() && (_data.ContinueMode == ContinueMode.Ad || _data.ContinueMode == ContinueMode.AdOrCurrency);
        private bool IsContinueCurrencyShown() => ContinuesAvailable() && (_data.ContinueMode == ContinueMode.Currency || _data.ContinueMode == ContinueMode.AdOrCurrency);

        private void ApplyTexts()
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(_data.Title);
            ApplySubtitle();
            if (_refs.ScoreLabel != null) _refs.ScoreLabel.SetText(_data.Score.ToString("N0"));
            if (_refs.ContinueAdLabelText != null) _refs.ContinueAdLabelText.SetText(_data.ContinueAdLabel);
            if (_refs.ContinueCurrencyLabelText != null) _refs.ContinueCurrencyLabelText.SetText(ResolveCurrencyLabel());
            if (_refs.RestartLabelText != null) _refs.RestartLabelText.SetText(_data.RestartLabel);
            if (_refs.MainMenuLabelText != null) _refs.MainMenuLabelText.SetText(_data.MainMenuLabel);
        }

        private void ApplySubtitle()
        {
            if (_refs.SubtitleLabel == null) return;
            _refs.SubtitleLabel.SetText(_data.Subtitle);
            _refs.SubtitleLabel.gameObject.SetActive(!string.IsNullOrEmpty(_data.Subtitle));
        }

        private string ResolveCurrencyLabel()
        {
            return (_data.ContinueCurrencyLabel ?? string.Empty).Replace("{amount}", _data.ContinueAmount.ToString());
        }

        private void ApplyIcon()
        {
            if (_refs.ContinueCurrencyIcon == null) return;
            var sprite = ResolveCurrencyIconSprite(_data.ContinueCurrency);
            _refs.ContinueCurrencyIcon.sprite = sprite;
            _refs.ContinueCurrencyIcon.enabled = sprite != null;
        }

        private Sprite ResolveCurrencyIconSprite(CurrencyType currency)
        {
            if (Theme == null) return null;
            switch (currency)
            {
                case CurrencyType.Coins:  return Theme.IconCoin;
                case CurrencyType.Gems:   return Theme.IconGem;
                case CurrencyType.Energy: return Theme.IconEnergy;
                default:                  return null;
            }
        }

        private void ApplyTint()
        {
            if (_refs.HeaderTint == null || Theme == null) return;
            _refs.HeaderTint.color = Theme.FailureColor;
        }

        private void ApplyVisibility()
        {
            if (_refs.ContinueAdButton != null) _refs.ContinueAdButton.gameObject.SetActive(IsContinueAdShown());
            if (_refs.ContinueCurrencyButton != null) _refs.ContinueCurrencyButton.gameObject.SetActive(IsContinueCurrencyShown());
            if (_refs.RestartButton != null) _refs.RestartButton.gameObject.SetActive(_data.ShowRestart);
            if (_refs.MainMenuButton != null) _refs.MainMenuButton.gameObject.SetActive(_data.ShowMainMenu);
            if (_refs.ScoreBlock != null) _refs.ScoreBlock.SetActive(_data.Score >= 0);
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = _data.CloseOnBackdrop;
        }

        private void ApplyContinueGating()
        {
            ApplyContinueAdGating();
            ApplyContinueCurrencyGating();
        }

        private void ApplyContinueAdGating()
        {
            if (_refs.ContinueAdButton == null) return;
            var ads = Services?.Ads;
            var ready = ads != null && ads.IsRewardedAdReady();
            _refs.ContinueAdButton.interactable = ready;
            ApplyButtonAlpha(_refs.ContinueAdButton, ready);
        }

        private void ApplyContinueCurrencyGating()
        {
            if (_refs.ContinueCurrencyButton == null) return;
            var economy = Services?.Economy;
            if (economy == null)
            {
                _refs.ContinueCurrencyButton.interactable = false;
                ApplyButtonAlpha(_refs.ContinueCurrencyButton, false);
                return;
            }
            var canAfford = economy.CanAfford(_data.ContinueCurrency, _data.ContinueAmount);
            _refs.ContinueCurrencyButton.interactable = true;
            ApplyButtonAlpha(_refs.ContinueCurrencyButton, canAfford);
        }

        private static void ApplyButtonAlpha(Button button, bool full)
        {
            if (button == null || button.image == null) return;
            button.image.color = full ? FullColor : GrayedColor;
        }
    }
}
