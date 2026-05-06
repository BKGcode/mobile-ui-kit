using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.GameOver;
using KitforgeLabs.MobileUIKit.Catalog.LevelComplete;
using KitforgeLabs.MobileUIKit.Catalog.Reward;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Samples.CatalogM41ThemePresets
{
    public class ThemeSwitcherSample : MonoBehaviour
    {
        [Serializable]
        private struct ThemeOption
        {
            public string Name;
            public UIThemeConfig Theme;
        }

        [SerializeField] private RectTransform _popupParent;
        [SerializeField] private UIServices _services;
        [SerializeField] private TMP_Dropdown _themeDropdown;
        [SerializeField] private Button _showGameOverButton;
        [SerializeField] private Button _showLevelCompleteButton;
        [SerializeField] private GameObject _gameOverPrefab;
        [SerializeField] private GameObject _levelCompletePrefab;
        [SerializeField] private List<ThemeOption> _themes = new List<ThemeOption>();

        private UIThemeConfig _activeTheme;
        private UIModuleBase _activeInstance;
        private string _activePopupKind;

        private void Awake()
        {
            if (_themes.Count == 0)
            {
                Debug.LogError("[ThemeSwitcherSample] No themes assigned. Run 'Tools/Kitforge/UI Kit/Build M4.1 — Theme Presets'.", this);
                return;
            }
            _activeTheme = _themes[0].Theme;
        }

        private void Start()
        {
            PopulateDropdown();
            HookButtons();
        }

        private void PopulateDropdown()
        {
            if (_themeDropdown == null) return;
            _themeDropdown.ClearOptions();
            var options = new List<string>();
            foreach (var t in _themes) options.Add(t.Name);
            _themeDropdown.AddOptions(options);
            _themeDropdown.onValueChanged.AddListener(OnDropdownChanged);
            _themeDropdown.value = 0;
        }

        private void HookButtons()
        {
            if (_showGameOverButton != null) _showGameOverButton.onClick.AddListener(ShowGameOver);
            if (_showLevelCompleteButton != null) _showLevelCompleteButton.onClick.AddListener(ShowLevelComplete);
        }

        private void OnDropdownChanged(int index)
        {
            if (index < 0 || index >= _themes.Count) return;
            _activeTheme = _themes[index].Theme;
            if (_activeInstance != null) ReopenActive();
        }

        private void ReopenActive()
        {
            var kind = _activePopupKind;
            DismissActive();
            if (kind == "GameOver") ShowGameOver();
            else if (kind == "LevelComplete") ShowLevelComplete();
        }

        private void DismissActive()
        {
            if (_activeInstance == null) return;
            Destroy(_activeInstance.gameObject);
            _activeInstance = null;
            _activePopupKind = null;
        }

        [ContextMenu("Show GameOver")]
        public void ShowGameOver()
        {
            DismissActive();
            var instance = SpawnPopup<GameOverPopup>(_gameOverPrefab, "GameOverPopup");
            if (instance == null) return;
            instance.Bind(BuildGameOverData());
            instance.OnContinueWithAdRequested += () => Debug.Log("[ThemeSwitcherSample] GameOver Continue Ad");
            instance.OnContinueWithCurrencyRequested += (c, a) => Debug.Log($"[ThemeSwitcherSample] GameOver Continue {a} {c}");
            instance.OnRestartRequested += () => Debug.Log("[ThemeSwitcherSample] GameOver Restart");
            instance.OnMainMenuRequested += () => Debug.Log("[ThemeSwitcherSample] GameOver MainMenu");
            instance.OnDismissed += HandleDismissed;
            instance.OnShow();
            _activeInstance = instance;
            _activePopupKind = "GameOver";
        }

        [ContextMenu("Show LevelComplete")]
        public void ShowLevelComplete()
        {
            DismissActive();
            var instance = SpawnPopup<LevelCompletePopup>(_levelCompletePrefab, "LevelCompletePopup");
            if (instance == null) return;
            instance.Bind(BuildLevelCompleteData());
            instance.OnNextRequested += _ => Debug.Log("[ThemeSwitcherSample] LevelComplete Next");
            instance.OnRetryRequested += _ => Debug.Log("[ThemeSwitcherSample] LevelComplete Retry");
            instance.OnMainMenuRequested += _ => Debug.Log("[ThemeSwitcherSample] LevelComplete MainMenu");
            instance.OnDismissed += HandleDismissed;
            instance.OnShow();
            _activeInstance = instance;
            _activePopupKind = "LevelComplete";
        }

        private void HandleDismissed()
        {
            _activeInstance = null;
            _activePopupKind = null;
        }

        private T SpawnPopup<T>(GameObject prefab, string popupName) where T : UIModuleBase
        {
            if (prefab == null)
            {
                Debug.LogError($"[ThemeSwitcherSample] {popupName} prefab missing. Run 'Tools/Kitforge/UI Kit/Build M4.1 — Theme Presets' (and 'Build Group C Sample' first).", this);
                return null;
            }
            var go = Instantiate(prefab, _popupParent, false);
            var inst = go.GetComponent<T>();
            if (inst == null)
            {
                Debug.LogError($"[ThemeSwitcherSample] {popupName} prefab missing the {typeof(T).Name} component on its root.", this);
                Destroy(go);
                return null;
            }
            inst.Initialize(_activeTheme, _services);
            return inst;
        }

        private static GameOverPopupData BuildGameOverData() => new GameOverPopupData
        {
            Title = "Game Over",
            Subtitle = "You ran out of moves",
            Score = 12450,
            ContinueMode = ContinueMode.AdOrCurrency,
            ContinueCurrency = CurrencyType.Gems,
            ContinueAmount = 5,
            ShowRestart = true,
            ShowMainMenu = true,
            CloseOnBackdrop = false
        };

        private static LevelCompletePopupData BuildLevelCompleteData() => new LevelCompletePopupData
        {
            Title = "Level Complete!",
            LevelLabel = "Level 12",
            Stars = 3,
            Score = 24800,
            BestScore = 21500,
            IsNewBest = true,
            ShowNext = true,
            ShowRetry = true,
            ShowMainMenu = true,
            Rewards = Array.Empty<RewardPopupData>()
        };
    }
}
