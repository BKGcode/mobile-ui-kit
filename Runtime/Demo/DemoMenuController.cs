using System;
using System.Reflection;
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
using KitforgeLabs.UIKit.HUD;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using KitforgeLabs.UIKit.Toast;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        [Serializable]
        private struct HudPrefabs
        {
            public GameObject Coins;
            public GameObject Gems;
            public GameObject Energy;
            public GameObject Timer;
        }

        [SerializeField] private ManagerRefs _managers;
        [SerializeField] private ThemeCycle _themes;
        [SerializeField] private HudPrefabs _hudPrefabs;
        [SerializeField] private Transform _overlayParent;

        private static readonly FieldInfo HUDServicesField = typeof(UIHUDBase)
            .GetField("_services", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly string[] ThemeNames = { "Default", "Casual", "Premium" };

        private int _themeIndex;
        private TMP_Text _themeChipLabel;
        private UIServices _services;

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
            var canvas = ResolveOverlayCanvas();
            if (canvas == null) { Debug.LogError("[DemoMenuController] No Canvas under KitforgeRoot.", this); return; }
            _services = (_overlayParent != null ? _overlayParent : transform).GetComponentInChildren<UIServices>(true);
            BuildTopBar(canvas.transform);
            BuildHudRow(canvas.transform);
            BuildCatalogPanel(canvas.transform);
            BuildBottomCta(canvas.transform);
        }

        private bool ValidateRefs()
        {
            if (_managers.Popups != null && _managers.Screens != null && _managers.Toasts != null) return true;
            Debug.LogError("[DemoMenuController] Manager refs missing. Re-bake via Tools → KitforgeLabs → Test → Bake Demo Scene.", this);
            return false;
        }

        private Canvas ResolveOverlayCanvas()
        {
            var probe = _overlayParent != null ? _overlayParent : transform;
            var canvas = probe.GetComponentInChildren<Canvas>(true);
            return canvas != null ? canvas : probe.GetComponentInParent<Canvas>(true);
        }

        private void BuildTopBar(Transform canvasRoot)
        {
            var bar = CreateStretchPanel("DemoTopBar", canvasRoot, top: true, height: 140f, color: new Color(0f, 0f, 0f, 0.55f));
            CreateTitleBlock(bar.transform);
            CreateThemeChip(bar.transform);
        }

        private void CreateTitleBlock(Transform parent)
        {
            var titleGO = CreateText(parent, "Title", "Kitforge UI Kit · Demo", 32f, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
            var rt = (RectTransform)titleGO.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0.65f, 1f);
            rt.offsetMin = new Vector2(40f, 16f);
            rt.offsetMax = new Vector2(0f, -36f);
            var hint = CreateText(parent, "Hint", "Pick any element below to preview.", 18f, FontStyles.Normal, TextAlignmentOptions.Left, new Color(1f, 1f, 1f, 0.65f));
            var hrt = (RectTransform)hint.transform;
            hrt.anchorMin = new Vector2(0f, 0f);
            hrt.anchorMax = new Vector2(0.65f, 1f);
            hrt.offsetMin = new Vector2(40f, 16f);
            hrt.offsetMax = new Vector2(0f, -76f);
        }

        private void CreateThemeChip(Transform parent)
        {
            var chip = CreateAnchoredButton("ThemeChip", parent,
                anchor: new Vector2(1f, 0.5f), pivot: new Vector2(1f, 0.5f),
                size: new Vector2(260f, 80f), offset: new Vector2(-32f, 0f),
                bgColor: new Color(1f, 1f, 1f, 0.18f),
                label: $"Theme: {ThemeNames[_themeIndex]}  ↻",
                onClick: CycleTheme,
                outLabel: out _themeChipLabel);
        }

        private void CycleTheme()
        {
            if (_managers.ThemeBinder == null) return;
            _themeIndex = (_themeIndex + 1) % 3;
            var next = ThemeAt(_themeIndex);
            if (next == null) return;
            _managers.ThemeBinder.SetTheme(next);
            ReapplyThemeToOverlays(next);
            if (_themeChipLabel != null) _themeChipLabel.text = $"Theme: {ThemeNames[_themeIndex]}  ↻";
        }

        private UIThemeConfig ThemeAt(int index) => index switch
        {
            0 => _themes.Default,
            1 => _themes.Casual,
            _ => _themes.Premium
        };

        private void ReapplyThemeToOverlays(UIThemeConfig theme)
        {
            if (_overlayParent == null) return;
            var elements = _overlayParent.GetComponentsInChildren<IThemedElement>(true);
            for (var i = 0; i < elements.Length; i++) elements[i].ApplyTheme(theme);
        }

        private void BuildHudRow(Transform canvasRoot)
        {
            if (_services == null) return;
            var row = CreateStretchPanel("DemoHudRow", canvasRoot, top: true, height: 130f, color: new Color(0f, 0f, 0f, 0.30f));
            var rowRT = (RectTransform)row.transform;
            rowRT.anchoredPosition = new Vector2(0f, -140f);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            ConfigureHudLayout(layout);
            InstantiateHud(_hudPrefabs.Coins, row.transform);
            InstantiateHud(_hudPrefabs.Gems, row.transform);
            InstantiateHud(_hudPrefabs.Energy, row.transform);
            InstantiateHud(_hudPrefabs.Timer, row.transform);
        }

        private static void ConfigureHudLayout(HorizontalLayoutGroup layout)
        {
            layout.padding = new RectOffset(28, 28, 14, 14);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void InstantiateHud(GameObject prefab, Transform parent)
        {
            if (prefab == null || _services == null) return;
            var holder = new GameObject("DemoHudHolder") { hideFlags = HideFlags.HideAndDontSave };
            holder.SetActive(false);
            holder.transform.SetParent(parent, false);
            var hudGO = (GameObject)Instantiate(prefab, holder.transform, false);
            var hud = hudGO.GetComponent<UIHUDBase>();
            if (hud != null && HUDServicesField != null) HUDServicesField.SetValue(hud, _services);
            hudGO.transform.SetParent(parent, false);
            Destroy(holder);
        }

        private void BuildCatalogPanel(Transform canvasRoot)
        {
            var panel = CreateAnchoredPanel("DemoCatalog", canvasRoot,
                anchor: new Vector2(0.5f, 0.5f), pivot: new Vector2(0.5f, 0.5f),
                size: new Vector2(940f, 1200f), offset: new Vector2(0f, -40f),
                color: new Color(0f, 0f, 0f, 0.30f));
            var layout = panel.AddComponent<VerticalLayoutGroup>();
            ConfigureCatalogLayout(layout);
            BuildScreensSection(panel.transform);
            BuildOnboardingSection(panel.transform);
            BuildEconomySection(panel.transform);
            BuildGameflowSection(panel.transform);
            BuildSystemSection(panel.transform);
            BuildToastsSection(panel.transform);
        }

        private static void ConfigureCatalogLayout(VerticalLayoutGroup layout)
        {
            layout.padding = new RectOffset(28, 28, 28, 28);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private void BuildScreensSection(Transform parent)
        {
            CreateSectionHeader(parent, "Screens");
            var row = CreateRow(parent, "Screens");
            CreateCatalogButton(row.transform, "Main Menu", "Title + 4 CTAs + theme-aware logo", PushMainMenu);
            CreateCatalogButton(row.transform, "Loading", "Animated bar + spinner + min display", PushLoading);
        }

        private void BuildOnboardingSection(Transform parent)
        {
            CreateSectionHeader(parent, "Onboarding");
            var row = CreateRow(parent, "Onboarding");
            CreateCatalogButton(row.transform, "Tutorial", "Multi-step intro with progress dots", () => _managers.Popups.Show<TutorialPopup>(BuildTutorialData()));
            CreateCatalogButton(row.transform, "Daily Login", "7-day calendar with big-reward day", () => _managers.Popups.Show<DailyLoginPopup>(BuildDailyLoginData()));
        }

        private void BuildEconomySection(Transform parent)
        {
            CreateSectionHeader(parent, "Economy");
            var row = CreateRow(parent, "Economy");
            CreateCatalogButton(row.transform, "Shop", "Grid of items with category filter", () => _managers.Popups.Show<ShopPopup>(new ShopPopupData { Title = "Shop", Category = ShopCategoryFilter.All }));
            CreateCatalogButton(row.transform, "Reward", "Single-reward claim, optional double", () => _managers.Popups.Show<RewardPopup>(BuildRewardData()));
            CreateCatalogButton(row.transform, "Not Enough", "Soft paywall: buy / ad / decline", () => _managers.Popups.Show<NotEnoughCurrencyPopup>(BuildNotEnoughData()));
        }

        private void BuildGameflowSection(Transform parent)
        {
            CreateSectionHeader(parent, "Gameflow");
            var row = CreateRow(parent, "Gameflow");
            CreateCatalogButton(row.transform, "Pause", "Resume / restart / settings / quit", () => _managers.Popups.Show<PausePopup>(BuildPauseData()));
            CreateCatalogButton(row.transform, "Level Complete", "Stars, score, new-best, next/retry", () => _managers.Popups.Show<LevelCompletePopup>(BuildLevelCompleteData()));
            CreateCatalogButton(row.transform, "Game Over", "Continue (ad/gems), restart, menu", () => _managers.Popups.Show<GameOverPopup>(BuildGameOverData()));
        }

        private void BuildSystemSection(Transform parent)
        {
            CreateSectionHeader(parent, "System");
            var row = CreateRow(parent, "System");
            CreateCatalogButton(row.transform, "Confirm", "Yes / No with destructive tone", () => _managers.Popups.Show<ConfirmPopup>(BuildConfirmData()));
            CreateCatalogButton(row.transform, "Settings", "Sliders, toggles, language picker", () => _managers.Popups.Show<SettingsPopup>(BuildSettingsData()));
        }

        private void BuildToastsSection(Transform parent)
        {
            CreateSectionHeader(parent, "Toasts");
            var row = CreateRow(parent, "Toasts");
            CreateCatalogButton(row.transform, "Success", "Themed accent + auto-dismiss", () => ShowToast("Saved!", ToastSeverity.Success));
            CreateCatalogButton(row.transform, "Info", "Informative neutral message", () => ShowToast("Tip: open Pause popup", ToastSeverity.Info));
            CreateCatalogButton(row.transform, "Warning", "Warning yellow accent", () => ShowToast("Low energy", ToastSeverity.Warning));
            CreateCatalogButton(row.transform, "Error", "Failure red accent", () => ShowToast("Network unavailable", ToastSeverity.Error));
        }

        private void PushMainMenu()
        {
            _managers.Screens.Push<MainMenuScreen>(new MainMenuScreenData
            {
                Title = "Demo Game",
                ShowPlayButton = true,
                ShowSettingsButton = true,
                ShowShopButton = true,
                ShowDailyButton = true
            });
        }

        private void PushLoading()
        {
            _managers.Screens.Push<LoadingScreen>(new LoadingScreenData
            {
                Title = "Loading…",
                Subtitle = "Demo gameplay",
                InitialProgress = 0.2f,
                MinDisplaySeconds = 1.5f,
                ShowProgressBar = true,
                ShowSpinner = true
            });
        }

        private void ShowToast(string message, ToastSeverity severity)
        {
            _managers.Toasts.Show<NotificationToast>(new NotificationToastData { Message = message, Severity = severity });
        }

        private GameObject CreateRow(Transform parent, string suffix)
        {
            var row = new GameObject($"Row_{suffix}", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var hl = row.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 12f;
            hl.childAlignment = TextAnchor.MiddleLeft;
            hl.childControlWidth = true;
            hl.childControlHeight = true;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = false;
            row.AddComponent<LayoutElement>().preferredHeight = 110f;
            return row;
        }

        private void CreateSectionHeader(Transform parent, string title)
        {
            var go = CreateText(parent, $"Header_{title}", title.ToUpperInvariant(), 18f, FontStyles.Bold, TextAlignmentOptions.Left, new Color(1f, 1f, 1f, 0.55f));
            var label = go.GetComponent<TMP_Text>();
            label.characterSpacing = 4f;
            go.AddComponent<LayoutElement>().preferredHeight = 32f;
        }

        private void CreateCatalogButton(Transform parent, string label, string desc, Action onClick)
        {
            var go = new GameObject($"Btn_{label}", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.16f);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
            AttachCatalogTitleLabel(go.transform, label);
            AttachCatalogDescLabel(go.transform, desc);
        }

        private void AttachCatalogTitleLabel(Transform parent, string text)
        {
            var go = CreateText(parent, "Title", text, 18f, FontStyles.Bold, TextAlignmentOptions.BottomLeft, Color.white);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(14f, 0f);
            rt.offsetMax = new Vector2(-14f, -6f);
        }

        private void AttachCatalogDescLabel(Transform parent, string text)
        {
            var go = CreateText(parent, "Desc", text, 13f, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Color(1f, 1f, 1f, 0.66f));
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.offsetMin = new Vector2(14f, 6f);
            rt.offsetMax = new Vector2(-14f, 0f);
            var lbl = go.GetComponent<TMP_Text>();
            lbl.enableWordWrapping = true;
        }

        private void BuildBottomCta(Transform canvasRoot)
        {
            var bar = CreateStretchPanel("DemoBottomCta", canvasRoot, top: false, height: 120f, color: new Color(0f, 0f, 0f, 0.55f));
            CreateAnchoredButton("HubCta", bar.transform,
                anchor: new Vector2(0.5f, 0.5f), pivot: new Vector2(0.5f, 0.5f),
                size: new Vector2(560f, 80f), offset: Vector2.zero,
                bgColor: new Color(0.20f, 0.55f, 0.95f, 1f),
                label: "How to use this in my game  →",
                onClick: OpenHub,
                outLabel: out _);
        }

        private void OpenHub()
        {
#if UNITY_EDITOR
            EditorApplication.ExecuteMenuItem("KitforgeLabs/UI Kit/Hub");
#else
            Debug.Log("[DemoMenuController] Open KitforgeLabs → UI Kit → Hub from the Editor menu.");
#endif
        }

        private GameObject CreateStretchPanel(string name, Transform parent, bool top, float height, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            var edgeY = top ? 1f : 0f;
            rt.anchorMin = new Vector2(0f, edgeY);
            rt.anchorMax = new Vector2(1f, edgeY);
            rt.pivot = new Vector2(0.5f, edgeY);
            rt.sizeDelta = new Vector2(0f, height);
            rt.anchoredPosition = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private GameObject CreateAnchoredPanel(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 offset, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
            go.GetComponent<Image>().color = color;
            return go;
        }

        private GameObject CreateAnchoredButton(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 offset, Color bgColor, string label, Action onClick, out TMP_Text outLabel)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            ApplyAnchored((RectTransform)go.transform, anchor, pivot, size, offset);
            go.GetComponent<Image>().color = bgColor;
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
            outLabel = AttachStretchedLabel(go.transform, label);
            return go;
        }

        private static void ApplyAnchored(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 size, Vector2 offset)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
        }

        private TMP_Text AttachStretchedLabel(Transform parent, string text)
        {
            var go = CreateText(parent, "Label", text, 18f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return go.GetComponent<TMP_Text>();
        }

        private GameObject CreateText(Transform parent, string name, string text, float size, FontStyles style, TextAlignmentOptions align, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontStyle = style;
            label.alignment = align;
            label.color = color;
            return go;
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
            data.Steps.Add(new TutorialStep { Title = "Theme", Body = "Three themes ship in. Cycle them from the top-right chip." });
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
