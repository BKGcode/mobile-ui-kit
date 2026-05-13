using System;
using System.Collections;
using KitforgeLabs.UIKit.Bootstrap;
using KitforgeLabs.UIKit.Catalog.Confirm;
using KitforgeLabs.UIKit.Catalog.DailyLogin;
using KitforgeLabs.UIKit.Catalog.GameOver;
using KitforgeLabs.UIKit.Catalog.LevelComplete;
using KitforgeLabs.UIKit.Catalog.NotEnough;
using KitforgeLabs.UIKit.Catalog.Pause;
using KitforgeLabs.UIKit.Catalog.Reward;
using KitforgeLabs.UIKit.Catalog.Screens;
using KitforgeLabs.UIKit.Catalog.Settings;
using KitforgeLabs.UIKit.Catalog.Shop;
using KitforgeLabs.UIKit.Catalog.Toasts;
using KitforgeLabs.UIKit.Catalog.Tutorial;
using KitforgeLabs.UIKit.Core;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using KitforgeLabs.UIKit.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.UIKit.Demo
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-50)]
    public sealed class DemoMenuController : MonoBehaviour
    {
        [Serializable]
        private struct ManagerRefs
        {
            public PopupManager Popups;
            public UIManager Screens;
            public ToastManager Toasts;
            public KitforgeThemeBinder ThemeBinder;
        }

        [Serializable]
        private struct ThemeCycle
        {
            public UIThemeConfig Default;
            public UIThemeConfig Casual;
            public UIThemeConfig Premium;
        }

        [SerializeField] private ManagerRefs _managers;
        [SerializeField] private ThemeCycle _themes;
        [SerializeField] private Transform _overlayParent;

        private int _themeIndex;
        private MainMenuScreen _mainMenu;

        private void Reset()
        {
            var root = GetComponentInParent<KitforgeThemeBinder>(true);
            if (root == null) return;
            _managers.Popups = root.GetComponentInChildren<PopupManager>(true);
            _managers.Screens = root.GetComponentInChildren<UIManager>(true);
            _managers.Toasts = root.GetComponentInChildren<ToastManager>(true);
            _managers.ThemeBinder = root;
            _overlayParent = root.transform;
        }

        private void Start()
        {
            if (!ValidateRefs()) return;
            PushMainMenu();
            BuildOverlayPanel();
        }

        private bool ValidateRefs()
        {
            if (_managers.Popups != null && _managers.Screens != null && _managers.Toasts != null) return true;
            Debug.LogError("[DemoMenuController] Manager references missing. Re-bake the demo scene from Tools → KitforgeLabs → Test → Bake Demo Scene.", this);
            return false;
        }

        private void PushMainMenu()
        {
            if (_managers.Screens == null) return;
            var dto = new MainMenuScreenData { Title = "Demo", ShowPlayButton = true, ShowSettingsButton = true, ShowShopButton = true, ShowDailyButton = true };
            _mainMenu = _managers.Screens.Push<MainMenuScreen>(dto);
            if (_mainMenu == null) return;
            _mainMenu.OnPlayRequested += HandlePlay;
            _mainMenu.OnSettingsRequested += () => _managers.Popups.Show<SettingsPopup>(BuildSettingsData());
            _mainMenu.OnShopRequested += () => _managers.Popups.Show<ShopPopup>(new ShopPopupData { Title = "Shop", Category = ShopCategoryFilter.All });
            _mainMenu.OnDailyRequested += () => _managers.Popups.Show<DailyLoginPopup>(BuildDailyLoginData());
        }

        private void HandlePlay()
        {
            _managers.Screens.Push<LoadingScreen>(new LoadingScreenData { Title = "Loading…", Subtitle = "Demo gameplay starting", InitialProgress = 0.2f, MinDisplaySeconds = 1.5f, ShowProgressBar = true, ShowSpinner = true });
            StartCoroutine(PlayDemoFlow());
        }

        private IEnumerator PlayDemoFlow()
        {
            yield return new WaitForSeconds(1.8f);
            _managers.Screens.Pop();
            _managers.Popups.Show<LevelCompletePopup>(BuildLevelCompleteData());
        }

        private void BuildOverlayPanel()
        {
            var parent = ResolveOverlayParent();
            if (parent == null) return;
            var canvas = parent.GetComponentInParent<Canvas>(true);
            if (canvas == null) return;
            BuildQuickSpawnColumn(canvas.transform);
            BuildThemeCycleButton(canvas.transform);
        }

        private Transform ResolveOverlayParent() => _overlayParent != null ? _overlayParent : transform;

        private void BuildQuickSpawnColumn(Transform canvasRoot)
        {
            var panel = CreatePanel("DemoQuickSpawn", canvasRoot, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(170f, 460f), new Vector2(95f, 0f));
            AddVerticalLayout(panel);
            AddPanelTitle(panel.transform, "Quick Spawn");
            CreateButton(panel.transform, "Confirm", () => _managers.Popups.Show<ConfirmPopup>(BuildConfirmData()));
            CreateButton(panel.transform, "Tutorial", () => _managers.Popups.Show<TutorialPopup>(BuildTutorialData()));
            CreateButton(panel.transform, "Reward", () => _managers.Popups.Show<RewardPopup>(BuildRewardData()));
            CreateButton(panel.transform, "Pause", () => _managers.Popups.Show<PausePopup>(BuildPauseData()));
            CreateButton(panel.transform, "Level Complete", () => _managers.Popups.Show<LevelCompletePopup>(BuildLevelCompleteData()));
            CreateButton(panel.transform, "Game Over", () => _managers.Popups.Show<GameOverPopup>(BuildGameOverData()));
            CreateButton(panel.transform, "Not Enough", () => _managers.Popups.Show<NotEnoughCurrencyPopup>(BuildNotEnoughData()));
            CreateButton(panel.transform, "Toast", () => _managers.Toasts.Show<NotificationToast>(new NotificationToastData { Message = "Saved!", Severity = ToastSeverity.Success }));
        }

        private void BuildThemeCycleButton(Transform canvasRoot)
        {
            var panel = CreatePanel("DemoThemeCycle", canvasRoot, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(170f, 70f), new Vector2(-95f, -60f));
            CreateButton(panel.transform, "Cycle Theme", CycleTheme);
        }

        private void CycleTheme()
        {
            if (_managers.ThemeBinder == null) return;
            _themeIndex = (_themeIndex + 1) % 3;
            var next = _themeIndex switch { 0 => _themes.Default, 1 => _themes.Casual, _ => _themes.Premium };
            if (next != null) _managers.ThemeBinder.SetTheme(next);
        }

        private GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 offset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);
            return go;
        }

        private void AddVerticalLayout(GameObject panel)
        {
            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private void AddPanelTitle(Transform parent, string text)
        {
            var go = new GameObject("Title", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 16f;
            label.color = Color.white;
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 24f;
        }

        private void CreateButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);
            go.GetComponent<LayoutElement>().preferredHeight = 38f;
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
            AttachButtonLabel(go.transform, label);
        }

        private void AttachButtonLabel(Transform parent, string text)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 14f;
            label.color = Color.white;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private SettingsPopupData BuildSettingsData() => new SettingsPopupData
        {
            Title = "Settings",
            ShowMusicSlider = true,
            ShowSfxSlider = true,
            ShowLanguagePicker = true,
            ShowNotificationsToggle = true,
            ShowHapticsToggle = true,
            LanguageOptions = new[]
            {
                new LanguageOption { Code = "en", DisplayName = "English" },
                new LanguageOption { Code = "es", DisplayName = "Español" }
            }
        };

        private ConfirmPopupData BuildConfirmData() => new ConfirmPopupData
        {
            Title = "Quit?",
            Message = "Progress will be lost.",
            ConfirmLabel = "Yes",
            CancelLabel = "Stay",
            Tone = ConfirmTone.Destructive,
            ShowCancel = true,
            CloseOnBackdrop = false
        };

        private TutorialPopupData BuildTutorialData()
        {
            var data = new TutorialPopupData();
            data.Steps.Add(new TutorialStep { Title = "Welcome", Body = "This is the Kitforge UI Kit demo." });
            data.Steps.Add(new TutorialStep { Title = "Catalog", Body = "10 popups, 2 screens, 1 toast, 4 HUDs." });
            data.Steps.Add(new TutorialStep { Title = "Theme", Body = "Three themes ship in. Cycle them from the top-right button." });
            return data;
        }

        private RewardPopupData BuildRewardData() => new RewardPopupData
        {
            Title = "Daily Bonus!",
            Kind = RewardKind.Coins,
            Amount = 250,
            ClaimLabel = "Claim",
            CloseOnBackdrop = false
        };

        private PausePopupData BuildPauseData() => new PausePopupData
        {
            Title = "Paused",
            ShowResume = true,
            ShowRestart = true,
            ShowSettings = true,
            ShowQuit = true,
            ShowMusicToggle = true,
            ShowSoundToggle = true,
            MusicOn = true,
            SoundOn = true
        };

        private LevelCompletePopupData BuildLevelCompleteData() => new LevelCompletePopupData
        {
            Title = "Level Complete!",
            LevelLabel = "Level 1-3",
            Stars = 3,
            Score = 1250,
            BestScore = 1200,
            IsNewBest = true,
            ShowNext = true,
            ShowRetry = true,
            Rewards = new[] { new RewardPopupData { Title = "Bonus", Kind = RewardKind.Coins, Amount = 100 } }
        };

        private GameOverPopupData BuildGameOverData() => new GameOverPopupData
        {
            Title = "Game Over",
            Subtitle = "Better luck next time",
            Score = 850,
            ContinueMode = ContinueMode.Ad,
            ContinueCurrency = CurrencyType.Gems,
            ContinueAmount = 5,
            MaxContinuesPerSession = 1,
            ShowRestart = true,
            ShowMainMenu = true
        };

        private NotEnoughCurrencyPopupData BuildNotEnoughData() => new NotEnoughCurrencyPopupData
        {
            Currency = CurrencyType.Gems,
            Required = 100,
            Missing = 20,
            Title = "Not enough gems",
            Message = "Buy more or watch an ad.",
            ShowBuyMore = true,
            ShowWatchAd = true,
            ShowDecline = true
        };

        private DailyLoginPopupData BuildDailyLoginData()
        {
            var entries = new DailyLoginRewardEntry[7];
            for (var i = 0; i < 7; i++)
            {
                var amount = (i + 1) * 100;
                entries[i] = new DailyLoginRewardEntry
                {
                    Label = $"Day {i + 1}",
                    IsBigReward = i == 6,
                    AllowDouble = i == 6,
                    Rewards = new[] { new RewardPopupData { Title = $"Day {i + 1}", Kind = RewardKind.Coins, Amount = amount } }
                };
            }
            return new DailyLoginPopupData { Title = "Daily Reward", RewardEntries = entries, CurrentDay = 3 };
        }
    }
}
