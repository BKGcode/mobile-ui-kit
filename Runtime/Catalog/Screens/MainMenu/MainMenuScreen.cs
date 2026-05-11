using System;
using KitforgeLabs.UIKit.Animation;
using KitforgeLabs.UIKit.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KitforgeLabs.UIKit.Catalog.Screens
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UIAnim_MainMenuScreen))]
    public sealed class MainMenuScreen : UIModule<MainMenuScreenData>
    {
        [Serializable]
        private struct MainMenuRefs
        {
            [Tooltip("Game title TMP label. Hidden when MainMenuScreenData.Title is empty.")]
            public TMP_Text TitleLabel;
            [Tooltip("Optional game logo Image. Receives Theme.LogoMainMenu sprite at prefab time.")]
            public Image LogoImage;
            [Tooltip("Play button. Hidden when ShowPlayButton = false.")]
            public Button PlayButton;
            [Tooltip("Settings button. Hidden when ShowSettingsButton = false.")]
            public Button SettingsButton;
            [Tooltip("Shop button. Hidden when ShowShopButton = false.")]
            public Button ShopButton;
            [Tooltip("Daily Login button. Hidden when ShowDailyButton = false.")]
            public Button DailyButton;
            [Tooltip("Red dot badge on Daily button. Shown when unclaimed reward is available.")]
            public GameObject DailyIndicatorDot;
        }

        [Serializable]
        private struct HudSlots
        {
            [Tooltip("Optional Coins HUD child GameObject. Activated with the screen, deactivated on hide.")]
            public GameObject Coins;
            [Tooltip("Optional Gems HUD child GameObject.")]
            public GameObject Gems;
            [Tooltip("Optional Energy HUD child GameObject.")]
            public GameObject Energy;
        }

        [SerializeField] private MainMenuRefs _refs;
        [SerializeField] private HudSlots _hudSlots;

        private IUIAnimator _animator;
        private MainMenuScreenData _data;
        private float _lastDotPollTime;
        private bool _isShowing;

        public event Action OnPlayRequested;
        public event Action OnSettingsRequested;
        public event Action OnShopRequested;
        public event Action OnDailyRequested;
        public event Action OnBackRequested;
        public event Action OnShown;

        internal void SetAnimatorForTests(IUIAnimator a) => _animator = a;
        internal bool DotVisibleForTests => _refs.DailyIndicatorDot != null && _refs.DailyIndicatorDot.activeSelf;
        internal void ForceDotPollForTests() => RefreshDailyDot();
        internal void InvokePlayForTests() => HandlePlay();
        internal void InvokeSettingsForTests() => HandleSettings();
        internal void InvokeShopForTests() => HandleShop();
        internal void InvokeDailyForTests() => HandleDaily();
        internal void SetRefsForTests(TMP_Text title, Image logo, Button play, Button settings, Button shop, Button daily, GameObject dot)
        {
            _refs.TitleLabel = title;
            _refs.LogoImage = logo;
            _refs.PlayButton = play;
            _refs.SettingsButton = settings;
            _refs.ShopButton = shop;
            _refs.DailyButton = daily;
            _refs.DailyIndicatorDot = dot;
        }

        private IUIAnimator Animator
        {
            get
            {
                if (_animator == null) _animator = GetComponent<IUIAnimator>();
                return _animator;
            }
        }

        private void Awake()
        {
            if (_animator == null) _animator = GetComponent<IUIAnimator>();
            WireButtons(add: true);
        }

        private void Update()
        {
            if (!_isShowing) return;
            if (Time.unscaledTime - _lastDotPollTime < 30f) return;
            _lastDotPollTime = Time.unscaledTime;
            RefreshDailyDot();
        }

        private void OnDestroy()
        {
            WireButtons(add: false);
            ClearAllEvents();
        }

        public override void Bind(MainMenuScreenData data)
        {
            _data = data ?? new MainMenuScreenData();
            ClearAllEvents();
            ApplyData(_data);
        }

        public override void OnShow()
        {
            if (_data == null) Bind(null);
            _isShowing = true;
            _lastDotPollTime = float.NegativeInfinity;
            RefreshDailyDot();
            SetHudSlotsActive(true);
            if (Animator != null) Animator.PlayShow(() => OnShown?.Invoke());
            else OnShown?.Invoke();
        }

        public override void OnHide()
        {
            _isShowing = false;
            SetHudSlotsActive(false);
            Animator?.Skip();
        }

        public override void OnBackPressed()
        {
            if (!_isShowing) return;
            OnBackRequested?.Invoke();
        }

        private void ApplyData(MainMenuScreenData data)
        {
            SetLabel(_refs.TitleLabel, data.Title);
            SetButtonVisible(_refs.PlayButton, data.ShowPlayButton);
            SetButtonVisible(_refs.SettingsButton, data.ShowSettingsButton);
            SetButtonVisible(_refs.ShopButton, data.ShowShopButton);
            SetButtonVisible(_refs.DailyButton, data.ShowDailyButton);
        }

        private static void SetLabel(TMP_Text label, string text)
        {
            if (label == null) return;
            var show = !string.IsNullOrEmpty(text);
            label.gameObject.SetActive(show);
            if (show) label.SetText(text);
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null) button.gameObject.SetActive(visible);
        }

        private void RefreshDailyDot()
        {
            var dot = _refs.DailyIndicatorDot;
            if (dot == null) return;
            if (!(_data?.ShowDailyButton ?? false)) { dot.SetActive(false); return; }
            var prog = Services?.Progression;
            if (prog == null) { dot.SetActive(false); return; }
            try { dot.SetActive(!prog.GetDailyLoginState().AlreadyClaimedToday); }
            catch (Exception e)
            {
                Debug.LogError($"[MainMenuScreen] IProgressionService.GetDailyLoginState threw: {e.Message}", this);
                dot.SetActive(false);
            }
        }

        private void SetHudSlotsActive(bool active)
        {
            if (_hudSlots.Coins != null) _hudSlots.Coins.SetActive(active);
            if (_hudSlots.Gems != null) _hudSlots.Gems.SetActive(active);
            if (_hudSlots.Energy != null) _hudSlots.Energy.SetActive(active);
        }

        private void WireButtons(bool add)
        {
            WireButton(_refs.PlayButton, HandlePlay, add);
            WireButton(_refs.SettingsButton, HandleSettings, add);
            WireButton(_refs.ShopButton, HandleShop, add);
            WireButton(_refs.DailyButton, HandleDaily, add);
        }

        private static void WireButton(Button btn, UnityAction action, bool add)
        {
            if (btn == null) return;
            if (add) btn.onClick.AddListener(action);
            else btn.onClick.RemoveListener(action);
        }

        private void HandlePlay() => OnPlayRequested?.Invoke();
        private void HandleSettings() => OnSettingsRequested?.Invoke();
        private void HandleShop() => OnShopRequested?.Invoke();
        private void HandleDaily() => OnDailyRequested?.Invoke();

        private void ClearAllEvents()
        {
            OnPlayRequested = null;
            OnSettingsRequested = null;
            OnShopRequested = null;
            OnDailyRequested = null;
            OnBackRequested = null;
        }
    }
}
