using System;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Pause
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimPausePopup))]
    public sealed class PausePopup : UIModule<PausePopupData>
    {
        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label (e.g. \"Paused\").")]
            public TMP_Text TitleLabel;
            [Tooltip("Optional subtitle TMP label (e.g. level name).")]
            public TMP_Text SubtitleLabel;
            [Tooltip("Full-screen backdrop button. Closes only if PausePopupData.CloseOnBackdrop = true.")]
            public Button BackdropButton;

            [Header("Action buttons (dismiss popup)")]
            [Tooltip("Resume. Restores Time.timeScale and dismisses.")]
            public Button ResumeButton;
            [Tooltip("Restart current level/run. Dismisses.")]
            public Button RestartButton;
            [Tooltip("Go to main menu / home. Dismisses.")]
            public Button HomeButton;
            [Tooltip("Quit application. Dismisses.")]
            public Button QuitButton;

            [Header("Shortcut buttons (do NOT dismiss)")]
            [Tooltip("Open settings popup. Pause stays open.")]
            public Button SettingsButton;
            [Tooltip("Open shop popup. Pause stays open.")]
            public Button ShopButton;
            [Tooltip("Open help/tutorial popup. Pause stays open.")]
            public Button HelpButton;

            [Header("Inline toggles (do NOT dismiss)")]
            public Toggle SoundToggle;
            public Toggle MusicToggle;
            public Toggle VibrationToggle;
        }

        [SerializeField] private Refs _refs;
        [Tooltip("Time.timeScale value applied while the popup is visible. 0 = full pause.")]
        [SerializeField, Range(0f, 1f)] private float _pauseTimeScale = 0f;

        public event Action OnResume;
        public event Action OnRestart;
        public event Action OnHome;
        public event Action OnQuit;
        public event Action OnSettings;
        public event Action OnShop;
        public event Action OnHelp;
        public event Action<bool> OnSoundChanged;
        public event Action<bool> OnMusicChanged;
        public event Action<bool> OnVibrationChanged;
        public event Action OnDismissed;
        public event Action OnPaused;
        public event Action OnResumed;

        public bool IsPaused { get; private set; }

        private PausePopupData _data;
        private IUIAnimator _animator;
        private float _restoreTimeScale = 1f;
        private bool _themeWarningLogged;

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
            SubscribeButtons();
            SubscribeToggles();
        }

        private void OnDestroy()
        {
            UnsubscribeButtons();
            UnsubscribeToggles();
            ClearAllEvents();
            if (IsPaused) RestoreTimeScale();
        }

        public override void Bind(PausePopupData data)
        {
            ClearAllEvents();
            _data = data ?? new PausePopupData();
            ApplyTexts(_data);
            ApplyVisibility(_data);
            ApplyToggleStates(_data);
            IsDismissing = false;
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            if (Theme == null && !_themeWarningLogged)
            {
                _themeWarningLogged = true;
                Debug.LogWarning("[PausePopup] Theme not initialized — animation/audio will not apply. Spawn via PopupManager.", this);
            }
            Services?.Audio?.Play(UIAudioCue.PopupOpen);
            if (Animator == null) { ApplyPauseTimeScale(); return; }
            Animator.ApplyPreset(ResolveAnimPreset());
            Animator.PlayShow(ApplyPauseTimeScale);
        }

        public override void OnHide()
        {
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            HandleResume();
        }

        public void Resume() => HandleResume();

        private void HandleResume() => DismissWith(OnResume);
        private void HandleRestart() => DismissWith(OnRestart);
        private void HandleHome() => DismissWith(OnHome);
        private void HandleQuit() => DismissWith(OnQuit);

        private void HandleSettings() => RaiseShortcut(OnSettings);
        private void HandleShop() => RaiseShortcut(OnShop);
        private void HandleHelp() => RaiseShortcut(OnHelp);

        private void HandleBackdrop()
        {
            if (IsDismissing || _data == null || !_data.CloseOnBackdrop) return;
            HandleResume();
        }

        private void DismissWith(Action reason)
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            reason?.Invoke();
            DismissWithAnimation();
        }

        private void RaiseShortcut(Action shortcut)
        {
            if (IsDismissing) return;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            shortcut?.Invoke();
        }

        private void DismissWithAnimation()
        {
            Services?.Audio?.Play(UIAudioCue.PopupClose);
            RestoreTimeScale();
            if (Animator == null) { FinalizeDismissal(); return; }
            Animator.PlayHide(FinalizeDismissal);
        }

        private void FinalizeDismissal()
        {
            RaiseDismissRequested();
            OnDismissed?.Invoke();
        }

        private void ApplyPauseTimeScale()
        {
            if (IsPaused) return;
            _restoreTimeScale = Time.timeScale;
            Time.timeScale = _pauseTimeScale;
            IsPaused = true;
            OnPaused?.Invoke();
        }

        private void RestoreTimeScale()
        {
            if (!IsPaused) return;
            Time.timeScale = _restoreTimeScale;
            IsPaused = false;
            OnResumed?.Invoke();
        }

        private void ClearAllEvents()
        {
            OnResume = null;
            OnRestart = null;
            OnHome = null;
            OnQuit = null;
            OnSettings = null;
            OnShop = null;
            OnHelp = null;
            OnSoundChanged = null;
            OnMusicChanged = null;
            OnVibrationChanged = null;
            OnDismissed = null;
            OnPaused = null;
            OnResumed = null;
        }

        private void ApplyTexts(PausePopupData data)
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(data.Title);
            if (_refs.SubtitleLabel != null) _refs.SubtitleLabel.SetText(data.Subtitle);
        }

        private void ApplyVisibility(PausePopupData data)
        {
            SetActive(_refs.ResumeButton, data.ShowResume);
            SetActive(_refs.RestartButton, data.ShowRestart);
            SetActive(_refs.SettingsButton, data.ShowSettings);
            SetActive(_refs.HomeButton, data.ShowHome);
            SetActive(_refs.ShopButton, data.ShowShop);
            SetActive(_refs.HelpButton, data.ShowHelp);
            SetActive(_refs.QuitButton, data.ShowQuit);
            SetActive(_refs.SoundToggle, data.ShowSoundToggle);
            SetActive(_refs.MusicToggle, data.ShowMusicToggle);
            SetActive(_refs.VibrationToggle, data.ShowVibrationToggle);
            if (_refs.BackdropButton != null) _refs.BackdropButton.interactable = data.CloseOnBackdrop;
        }

        private void ApplyToggleStates(PausePopupData data)
        {
            if (_refs.SoundToggle != null) _refs.SoundToggle.SetIsOnWithoutNotify(data.SoundOn);
            if (_refs.MusicToggle != null) _refs.MusicToggle.SetIsOnWithoutNotify(data.MusicOn);
            if (_refs.VibrationToggle != null) _refs.VibrationToggle.SetIsOnWithoutNotify(data.VibrationOn);
        }

        private static void SetActive(Component c, bool active)
        {
            if (c != null) c.gameObject.SetActive(active);
        }

        private void SubscribeButtons()
        {
            AddListener(_refs.ResumeButton, HandleResume);
            AddListener(_refs.RestartButton, HandleRestart);
            AddListener(_refs.HomeButton, HandleHome);
            AddListener(_refs.QuitButton, HandleQuit);
            AddListener(_refs.SettingsButton, HandleSettings);
            AddListener(_refs.ShopButton, HandleShop);
            AddListener(_refs.HelpButton, HandleHelp);
            AddListener(_refs.BackdropButton, HandleBackdrop);
        }

        private void UnsubscribeButtons()
        {
            RemoveListener(_refs.ResumeButton, HandleResume);
            RemoveListener(_refs.RestartButton, HandleRestart);
            RemoveListener(_refs.HomeButton, HandleHome);
            RemoveListener(_refs.QuitButton, HandleQuit);
            RemoveListener(_refs.SettingsButton, HandleSettings);
            RemoveListener(_refs.ShopButton, HandleShop);
            RemoveListener(_refs.HelpButton, HandleHelp);
            RemoveListener(_refs.BackdropButton, HandleBackdrop);
        }

        private void SubscribeToggles()
        {
            if (_refs.SoundToggle != null) _refs.SoundToggle.onValueChanged.AddListener(HandleSoundChanged);
            if (_refs.MusicToggle != null) _refs.MusicToggle.onValueChanged.AddListener(HandleMusicChanged);
            if (_refs.VibrationToggle != null) _refs.VibrationToggle.onValueChanged.AddListener(HandleVibrationChanged);
        }

        private void UnsubscribeToggles()
        {
            if (_refs.SoundToggle != null) _refs.SoundToggle.onValueChanged.RemoveListener(HandleSoundChanged);
            if (_refs.MusicToggle != null) _refs.MusicToggle.onValueChanged.RemoveListener(HandleMusicChanged);
            if (_refs.VibrationToggle != null) _refs.VibrationToggle.onValueChanged.RemoveListener(HandleVibrationChanged);
        }

        private void HandleSoundChanged(bool v)
        {
            if (_data != null) _data.SoundOn = v;
            OnSoundChanged?.Invoke(v);
        }

        private void HandleMusicChanged(bool v)
        {
            if (_data != null) _data.MusicOn = v;
            OnMusicChanged?.Invoke(v);
        }

        private void HandleVibrationChanged(bool v)
        {
            if (_data != null) _data.VibrationOn = v;
            OnVibrationChanged?.Invoke(v);
        }

        private static void AddListener(Button b, UnityEngine.Events.UnityAction action)
        {
            if (b != null) b.onClick.AddListener(action);
        }

        private static void RemoveListener(Button b, UnityEngine.Events.UnityAction action)
        {
            if (b != null) b.onClick.RemoveListener(action);
        }

    }
}
