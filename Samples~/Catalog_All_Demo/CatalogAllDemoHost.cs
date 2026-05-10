using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.Confirm;
using KitforgeLabs.MobileUIKit.Catalog.DailyLogin;
using KitforgeLabs.MobileUIKit.Catalog.GameOver;
using KitforgeLabs.MobileUIKit.Catalog.LevelComplete;
using KitforgeLabs.MobileUIKit.Catalog.NotEnough;
using KitforgeLabs.MobileUIKit.Catalog.Pause;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Catalog.Screens;
using KitforgeLabs.MobileUIKit.Catalog.Settings;
using KitforgeLabs.MobileUIKit.Catalog.Shop;
using KitforgeLabs.MobileUIKit.Catalog.Toasts;
using KitforgeLabs.MobileUIKit.Catalog.Tutorial;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using KitforgeLabs.MobileUIKit.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogAllDemo
{
    /// <summary>
    /// Single-scene master demo wiring every catalog popup, screen and HUD into one buttons-per-element host.
    /// Demonstrates the canonical buyer path: PopupManager.Show&lt;T&gt;(dto) for popups,
    /// UIManager.Push&lt;T&gt;(dto) for screens, ToastManager.Show&lt;T&gt;(dto) for toasts.
    /// Theme dropdown reskins all spawned elements via UIManager/PopupManager/ToastManager.SetTheme().
    /// </summary>
    public sealed class CatalogAllDemoHost : MonoBehaviour
    {
        [Serializable]
        public struct ThemeOption
        {
            public string Name;
            public UIThemeConfig Theme;
        }

        [Header("Kit references (wired by Build Catalog_All_Demo)")]
        [SerializeField] private UIManager _uiManager;
        [SerializeField] private PopupManager _popupManager;
        [SerializeField] private ToastManager _toastManager;
        [SerializeField] private UIServices _services;

        [Header("Theme dropdown — Default / Casual / Premium")]
        [SerializeField] private TMP_Dropdown _themeDropdown;
        [SerializeField] private List<ThemeOption> _themes = new();

        [Header("Group A — Pure UI buttons")]
        [SerializeField] private Button _btnConfirm;
        [SerializeField] private Button _btnPause;
        [SerializeField] private Button _btnTutorial;
        [SerializeField] private Button _btnToast;

        [Header("Group B — Currency buttons")]
        [SerializeField] private Button _btnReward;
        [SerializeField] private Button _btnShop;
        [SerializeField] private Button _btnNotEnough;

        [Header("Group C — Progression buttons")]
        [SerializeField] private Button _btnDailyLogin;
        [SerializeField] private Button _btnLevelComplete;
        [SerializeField] private Button _btnGameOver;

        [Header("Group D — Player Data buttons")]
        [SerializeField] private Button _btnSettings;

        [Header("Group E — Screens buttons")]
        [SerializeField] private Button _btnLoadingScreen;
        [SerializeField] private Button _btnMainMenuScreen;

        [Header("HUD test — drive currency from buttons")]
        [SerializeField] private Button _btnAddCoins;
        [SerializeField] private Button _btnAddGems;
        [SerializeField] private Button _btnSpendCoins;

        private void OnEnable()
        {
            BuildThemeDropdown();
            HookButton(_btnConfirm, ShowConfirm);
            HookButton(_btnPause, ShowPause);
            HookButton(_btnTutorial, ShowTutorial);
            HookButton(_btnToast, ShowToast);
            HookButton(_btnReward, ShowReward);
            HookButton(_btnShop, ShowShop);
            HookButton(_btnNotEnough, ShowNotEnough);
            HookButton(_btnDailyLogin, ShowDailyLogin);
            HookButton(_btnLevelComplete, ShowLevelComplete);
            HookButton(_btnGameOver, ShowGameOver);
            HookButton(_btnSettings, ShowSettings);
            HookButton(_btnLoadingScreen, PushLoadingScreen);
            HookButton(_btnMainMenuScreen, PushMainMenuScreen);
            HookButton(_btnAddCoins, AddCoins);
            HookButton(_btnAddGems, AddGems);
            HookButton(_btnSpendCoins, SpendCoins);
        }

        private void OnDisable()
        {
            UnhookButton(_btnConfirm, ShowConfirm);
            UnhookButton(_btnPause, ShowPause);
            UnhookButton(_btnTutorial, ShowTutorial);
            UnhookButton(_btnToast, ShowToast);
            UnhookButton(_btnReward, ShowReward);
            UnhookButton(_btnShop, ShowShop);
            UnhookButton(_btnNotEnough, ShowNotEnough);
            UnhookButton(_btnDailyLogin, ShowDailyLogin);
            UnhookButton(_btnLevelComplete, ShowLevelComplete);
            UnhookButton(_btnGameOver, ShowGameOver);
            UnhookButton(_btnSettings, ShowSettings);
            UnhookButton(_btnLoadingScreen, PushLoadingScreen);
            UnhookButton(_btnMainMenuScreen, PushMainMenuScreen);
            UnhookButton(_btnAddCoins, AddCoins);
            UnhookButton(_btnAddGems, AddGems);
            UnhookButton(_btnSpendCoins, SpendCoins);
            if (_themeDropdown != null) _themeDropdown.onValueChanged.RemoveListener(OnThemeChanged);
        }

        private static void HookButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.AddListener(action);
        }

        private static void UnhookButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveListener(action);
        }

        private void BuildThemeDropdown()
        {
            if (_themeDropdown == null) return;
            _themeDropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>(_themes.Count);
            for (var i = 0; i < _themes.Count; i++) options.Add(new TMP_Dropdown.OptionData(_themes[i].Name));
            _themeDropdown.AddOptions(options);
            _themeDropdown.onValueChanged.RemoveListener(OnThemeChanged);
            _themeDropdown.onValueChanged.AddListener(OnThemeChanged);
        }

        private void OnThemeChanged(int index)
        {
            if (index < 0 || index >= _themes.Count) return;
            var theme = _themes[index].Theme;
            if (theme == null) return;
            if (_uiManager != null) _uiManager.SetTheme(theme);
            if (_popupManager != null) _popupManager.SetTheme(theme);
            if (_toastManager != null) _toastManager.SetTheme(theme);
        }

        // ── Group A spawners ────────────────────────────────────────────────

        private void ShowConfirm()
        {
            _popupManager?.Show<ConfirmPopup>(new ConfirmPopupData
            {
                Title = "Confirm action",
                Message = "Spend 100 coins to continue?",
                ConfirmLabel = "Yes",
                CancelLabel = "No",
            });
        }

        private void ShowPause()
        {
            _popupManager?.Show<PausePopup>(new PausePopupData
            {
                Title = "Paused",
                Subtitle = "Tap Resume to continue",
                ShowResume = true,
                ShowRestart = true,
                ShowSettings = true,
                ShowHome = true,
            });
        }

        private void ShowTutorial()
        {
            _popupManager?.Show<TutorialPopup>(new TutorialPopupData
            {
                Steps = new List<TutorialStep>
                {
                    new TutorialStep { Title = "How to play", Body = "Drag a card to merge." },
                    new TutorialStep { Title = "Combo", Body = "Match three to score." },
                },
            });
        }

        private void ShowToast()
        {
            _toastManager?.Show<NotificationToast>(new NotificationToastData
            {
                Message = "Daily reward unlocked",
                Severity = ToastSeverity.Info,
                DurationOverride = 2.5f,
            });
        }

        // ── Group B spawners ────────────────────────────────────────────────

        private void ShowReward()
        {
            _popupManager?.Show<RewardPopup>(new RewardPopupData
            {
                Title = "Reward!",
                Kind = RewardKind.Coins,
                Amount = 100,
            });
        }

        private void ShowShop()
        {
            _popupManager?.Show<ShopPopup>(new ShopPopupData
            {
                Title = "Shop",
            });
        }

        private void ShowNotEnough()
        {
            _popupManager?.Show<NotEnoughCurrencyPopup>(new NotEnoughCurrencyPopupData
            {
                Currency = CurrencyType.Coins,
                Required = 500,
                Missing = 425,
            });
        }

        // ── Group C spawners ────────────────────────────────────────────────

        private void ShowDailyLogin()
        {
            _popupManager?.Show<DailyLoginPopup>(new DailyLoginPopupData
            {
                Title = "Daily login",
                CurrentDay = 3,
            });
        }

        private void ShowLevelComplete()
        {
            _popupManager?.Show<LevelCompletePopup>(new LevelCompletePopupData
            {
                Title = "Level Cleared",
                Stars = 3,
                Score = 12340,
            });
        }

        private void ShowGameOver()
        {
            _popupManager?.Show<GameOverPopup>(new GameOverPopupData
            {
                Title = "Game Over",
                Score = 4280,
                ContinueMode = ContinueMode.Ad,
            });
        }

        // ── Group D spawners ────────────────────────────────────────────────

        private void ShowSettings()
        {
            _popupManager?.Show<SettingsPopup>(new SettingsPopupData
            {
                Title = "Settings",
            });
        }

        // ── Group E spawners (screens via UIManager.Push) ───────────────────

        private void PushLoadingScreen()
        {
            _uiManager?.Push<LoadingScreen>(new LoadingScreenData
            {
                Title = "Loading...",
                MinDisplaySeconds = 1.5f,
            });
        }

        private void PushMainMenuScreen()
        {
            _uiManager?.Push<MainMenuScreen>(new MainMenuScreenData
            {
                Title = "Main Menu",
            });
        }

        // ── HUD test buttons (drive IEconomyService) ────────────────────────

        private void AddCoins() => _services?.Economy?.Add(CurrencyType.Coins, 100);
        private void AddGems() => _services?.Economy?.Add(CurrencyType.Gems, 5);
        private void SpendCoins() => _services?.Economy?.Spend(CurrencyType.Coins, 30);
    }
}
