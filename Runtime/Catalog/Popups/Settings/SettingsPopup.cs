using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Catalog.Settings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnimSettingsPopup))]
    public sealed class SettingsPopup : UIModule<SettingsPopupData>
    {
        public const string KeyMusicVolume = "kfmui.settings.musicVolume";
        public const string KeySfxVolume = "kfmui.settings.sfxVolume";
        public const string KeyLanguage = "kfmui.settings.language";
        public const string KeyNotifications = "kfmui.settings.notifications";
        public const string KeyHaptics = "kfmui.settings.haptics";

        private const float DefaultMusicVolume = 1f;
        private const float DefaultSfxVolume = 1f;
        private const string DefaultLanguage = "";
        private const bool DefaultNotifications = true;
        private const bool DefaultHaptics = true;

        [Serializable]
        private struct Refs
        {
            [Tooltip("Title TMP label.")]
            public TMP_Text TitleLabel;
            [Tooltip("Backdrop button. Tap dismisses (always — no DTO opt-out).")]
            public Button BackdropButton;
            [Tooltip("Close button (footer).")]
            public Button CloseButton;
            [Tooltip("Close button TMP label.")]
            public TMP_Text CloseLabelText;

            [Header("Rows (parent containers — toggled by Show<X> flags)")]
            public GameObject MusicRow;
            public GameObject SfxRow;
            public GameObject LanguageRow;
            public GameObject NotificationsRow;
            public GameObject HapticsRow;

            [Header("Controls")]
            public Slider MusicSlider;
            public Slider SfxSlider;
            public TMP_Dropdown LanguageDropdown;
            public Toggle NotificationsToggle;
            public Toggle HapticsToggle;
        }

        [SerializeField] private Refs _refs;

        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSfxVolumeChanged;
        public event Action<string> OnLanguageChanged;
        public event Action<bool> OnNotificationsChanged;
        public event Action<bool> OnHapticsChanged;
        public event Action OnDismissed;

        private SettingsPopupData _data;
        private IUIAnimator _animator;
        private LanguageOption[] _activeLanguageOptions;
        private bool _missingPlayerDataLogged;
        private bool _missingLocalizationLogged;

        internal SettingsPopupData DataForTests => _data;

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        internal void SetAnimatorForTests(IUIAnimator a) => _animator = a;

        internal void InvokeCloseForTests() => HandleClose();
        internal void InvokeBackdropForTests() => HandleBackdrop();
        internal void InvokeMusicChangedForTests(float v) => HandleMusicVolumeChanged(v);
        internal void InvokeSfxChangedForTests(float v) => HandleSfxVolumeChanged(v);
        internal void InvokeNotificationsChangedForTests(bool v) => HandleNotificationsChanged(v);
        internal void InvokeHapticsChangedForTests(bool v) => HandleHapticsChanged(v);
        internal void InvokeLanguageChangedForTests(int idx) => HandleLanguageChanged(idx);

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            SubscribeAll();
        }

        private void OnDestroy()
        {
            UnsubscribeAll();
            ClearAllEvents();
        }

        public override void Bind(SettingsPopupData data)
        {
            ClearAllEvents();
            _data = data ?? new SettingsPopupData();
            ApplyTexts(_data);
            ApplyVisibility(_data);
            BindControlValues(_data);
            IsDismissing = false;
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
            Services?.PlayerData?.Save();
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (IsDismissing) return;
            HandleClose();
        }

        private void HandleClose()
        {
            if (IsDismissing) return;
            IsDismissing = true;
            Services?.Audio?.Play(UIAudioCue.ButtonTap);
            DismissWithAnimation();
        }

        private void HandleBackdrop() => HandleClose();

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

        private void HandleMusicVolumeChanged(float v)
        {
            if (IsDismissing) return;
            Services?.PlayerData?.SetFloat(KeyMusicVolume, v);
            OnMusicVolumeChanged?.Invoke(v);
        }

        private void HandleSfxVolumeChanged(float v)
        {
            if (IsDismissing) return;
            Services?.PlayerData?.SetFloat(KeySfxVolume, v);
            OnSfxVolumeChanged?.Invoke(v);
        }

        private void HandleNotificationsChanged(bool v)
        {
            if (IsDismissing) return;
            Services?.PlayerData?.SetBool(KeyNotifications, v);
            OnNotificationsChanged?.Invoke(v);
        }

        private void HandleHapticsChanged(bool v)
        {
            if (IsDismissing) return;
            Services?.PlayerData?.SetBool(KeyHaptics, v);
            OnHapticsChanged?.Invoke(v);
        }

        private void HandleLanguageChanged(int idx)
        {
            if (IsDismissing) return;
            if (_activeLanguageOptions == null) return;
            if (idx < 0 || idx >= _activeLanguageOptions.Length) return;
            var code = _activeLanguageOptions[idx].Code;
            Services?.PlayerData?.SetString(KeyLanguage, code);
            Services?.Localization?.SetLanguage(code);
            OnLanguageChanged?.Invoke(code);
        }

        private void ApplyTexts(SettingsPopupData data)
        {
            if (_refs.TitleLabel != null) _refs.TitleLabel.SetText(data.Title);
            if (_refs.CloseLabelText != null) _refs.CloseLabelText.SetText(data.CloseLabel);
        }

        private void ApplyVisibility(SettingsPopupData data)
        {
            SetActive(_refs.MusicRow, data.ShowMusicSlider);
            SetActive(_refs.SfxRow, data.ShowSfxSlider);
            SetActive(_refs.LanguageRow, ResolveLanguagePickerVisible(data));
            SetActive(_refs.NotificationsRow, data.ShowNotificationsToggle);
            SetActive(_refs.HapticsRow, data.ShowHapticsToggle);
        }

        private bool ResolveLanguagePickerVisible(SettingsPopupData data)
        {
            if (!data.ShowLanguagePicker) return false;
            if (data.LanguageOptions == null || data.LanguageOptions.Length == 0) return false;
            if (Services?.Localization != null) return true;
            if (!_missingLocalizationLogged)
            {
                _missingLocalizationLogged = true;
                Debug.LogError("[SettingsPopup] IUILocalizationService not available — language picker hidden. Wire UIServices.Localization.", this);
            }
            return false;
        }

        private void BindControlValues(SettingsPopupData data)
        {
            var pd = Services?.PlayerData;
            if (pd == null && !_missingPlayerDataLogged)
            {
                _missingPlayerDataLogged = true;
                Debug.LogError("[SettingsPopup] IPlayerDataService not available — controls disabled. Wire UIServices.PlayerData.", this);
            }
            BindMusicSlider(pd);
            BindSfxSlider(pd);
            BindLanguagePicker(pd, data);
            BindNotificationsToggle(pd);
            BindHapticsToggle(pd);
        }

        private void BindMusicSlider(IPlayerDataService pd)
        {
            if (_refs.MusicSlider == null) return;
            var v = pd != null ? pd.GetFloat(KeyMusicVolume, DefaultMusicVolume) : DefaultMusicVolume;
            _refs.MusicSlider.SetValueWithoutNotify(v);
            _refs.MusicSlider.interactable = pd != null;
        }

        private void BindSfxSlider(IPlayerDataService pd)
        {
            if (_refs.SfxSlider == null) return;
            var v = pd != null ? pd.GetFloat(KeySfxVolume, DefaultSfxVolume) : DefaultSfxVolume;
            _refs.SfxSlider.SetValueWithoutNotify(v);
            _refs.SfxSlider.interactable = pd != null;
        }

        private void BindNotificationsToggle(IPlayerDataService pd)
        {
            if (_refs.NotificationsToggle == null) return;
            var v = pd != null ? pd.GetBool(KeyNotifications, DefaultNotifications) : DefaultNotifications;
            _refs.NotificationsToggle.SetIsOnWithoutNotify(v);
            _refs.NotificationsToggle.interactable = pd != null;
        }

        private void BindHapticsToggle(IPlayerDataService pd)
        {
            if (_refs.HapticsToggle == null) return;
            var v = pd != null ? pd.GetBool(KeyHaptics, DefaultHaptics) : DefaultHaptics;
            _refs.HapticsToggle.SetIsOnWithoutNotify(v);
            _refs.HapticsToggle.interactable = pd != null;
        }

        private void BindLanguagePicker(IPlayerDataService pd, SettingsPopupData data)
        {
            _activeLanguageOptions = data.LanguageOptions ?? new LanguageOption[0];
            var saved = pd != null ? pd.GetString(KeyLanguage, DefaultLanguage) : DefaultLanguage;
            var idx = ResolveLanguageIndex(saved);
            if (_refs.LanguageDropdown == null) return;
            PopulateLanguageOptions(_activeLanguageOptions);
            _refs.LanguageDropdown.SetValueWithoutNotify(idx);
            _refs.LanguageDropdown.interactable = pd != null && Services?.Localization != null;
        }

        private void PopulateLanguageOptions(LanguageOption[] options)
        {
            _refs.LanguageDropdown.ClearOptions();
            var list = new List<TMP_Dropdown.OptionData>(options.Length);
            for (var i = 0; i < options.Length; i++)
            {
                list.Add(new TMP_Dropdown.OptionData(options[i].DisplayName ?? options[i].Code ?? ""));
            }
            _refs.LanguageDropdown.AddOptions(list);
        }

        private int ResolveLanguageIndex(string savedCode)
        {
            for (var i = 0; i < _activeLanguageOptions.Length; i++)
            {
                if (_activeLanguageOptions[i].Code == savedCode) return i;
            }
            if (!string.IsNullOrEmpty(savedCode))
            {
                Debug.LogWarning($"[SettingsPopup] Saved language '{savedCode}' not in LanguageOptions. Falling back to first entry.", this);
            }
            return 0;
        }

        private void SubscribeAll()
        {
            AddButton(_refs.CloseButton, HandleClose);
            AddButton(_refs.BackdropButton, HandleBackdrop);
            AddSlider(_refs.MusicSlider, HandleMusicVolumeChanged);
            AddSlider(_refs.SfxSlider, HandleSfxVolumeChanged);
            AddToggle(_refs.NotificationsToggle, HandleNotificationsChanged);
            AddToggle(_refs.HapticsToggle, HandleHapticsChanged);
            AddDropdown(_refs.LanguageDropdown, HandleLanguageChanged);
        }

        private void UnsubscribeAll()
        {
            RemoveButton(_refs.CloseButton, HandleClose);
            RemoveButton(_refs.BackdropButton, HandleBackdrop);
            RemoveSlider(_refs.MusicSlider, HandleMusicVolumeChanged);
            RemoveSlider(_refs.SfxSlider, HandleSfxVolumeChanged);
            RemoveToggle(_refs.NotificationsToggle, HandleNotificationsChanged);
            RemoveToggle(_refs.HapticsToggle, HandleHapticsChanged);
            RemoveDropdown(_refs.LanguageDropdown, HandleLanguageChanged);
        }

        private void ClearAllEvents()
        {
            OnMusicVolumeChanged = null;
            OnSfxVolumeChanged = null;
            OnLanguageChanged = null;
            OnNotificationsChanged = null;
            OnHapticsChanged = null;
            OnDismissed = null;
        }

        private static void SetActive(GameObject g, bool active)
        {
            if (g != null) g.SetActive(active);
        }

        private static void AddButton(Button b, UnityEngine.Events.UnityAction a)
        {
            if (b != null) b.onClick.AddListener(a);
        }

        private static void RemoveButton(Button b, UnityEngine.Events.UnityAction a)
        {
            if (b != null) b.onClick.RemoveListener(a);
        }

        private static void AddSlider(Slider s, UnityEngine.Events.UnityAction<float> a)
        {
            if (s != null) s.onValueChanged.AddListener(a);
        }

        private static void RemoveSlider(Slider s, UnityEngine.Events.UnityAction<float> a)
        {
            if (s != null) s.onValueChanged.RemoveListener(a);
        }

        private static void AddToggle(Toggle t, UnityEngine.Events.UnityAction<bool> a)
        {
            if (t != null) t.onValueChanged.AddListener(a);
        }

        private static void RemoveToggle(Toggle t, UnityEngine.Events.UnityAction<bool> a)
        {
            if (t != null) t.onValueChanged.RemoveListener(a);
        }

        private static void AddDropdown(TMP_Dropdown d, UnityEngine.Events.UnityAction<int> a)
        {
            if (d != null) d.onValueChanged.AddListener(a);
        }

        private static void RemoveDropdown(TMP_Dropdown d, UnityEngine.Events.UnityAction<int> a)
        {
            if (d != null) d.onValueChanged.RemoveListener(a);
        }
    }
}
