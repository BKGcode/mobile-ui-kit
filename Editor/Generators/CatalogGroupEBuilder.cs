using KitforgeLabs.MobileUIKit.Catalog.Screens;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using KitforgeLabs.MobileUIKit.Toast;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static KitforgeLabs.MobileUIKit.Editor.Generators.CatalogGroupBuilderShared;

namespace KitforgeLabs.MobileUIKit.Editor.Generators
{
    public static class CatalogGroupEBuilder
    {
        private const string OutputRoot = "Assets/Catalog_GroupE_Demo";
        private const string PrefabsFolder = OutputRoot + "/Prefabs";
        private const string LoadingPath = PrefabsFolder + "/LoadingScreen.prefab";
        private const string MainMenuPath = PrefabsFolder + "/MainMenuScreen.prefab";
        private const string ScenePath = OutputRoot + "/GroupE_BootDemo.unity";
        private const string DefaultThemePath = "Assets/Settings/UI/UIThemeConfig_Default.asset";
        private const string CasualThemePath = "Assets/Catalog_M4_ThemePresets_Demo/Themes/Theme_Casual.asset";
        private const string PremiumThemePath = "Assets/Catalog_M4_ThemePresets_Demo/Themes/Theme_Premium.asset";
        private const string GroupCDailyLoginPath = "Assets/Catalog_GroupC_Demo/Prefabs/DailyLoginPopup.prefab";
        private const string ProgressionTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryProgressionService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string TimeServiceTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryTimeService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string DemoHostTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupE.GroupEDemoHost, KitforgeLabs.MobileUIKit.Samples.CatalogGroupE";
        private const string ThemeSwitcherTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupE.ThemeSwitcherEScreens, KitforgeLabs.MobileUIKit.Samples.CatalogGroupE";
        private const string InstructionsText = "DEMO: Right-click GroupEDemoHost in Hierarchy → ContextMenu (Boot Demo / Show MainMenu / Trigger DailyLogin). Button clicks log to Console (Window → General → Console).";

        private static readonly Color BgDark = new Color(0.10f, 0.12f, 0.16f, 1f);
        private static readonly Color BgMainMenu = new Color(0.12f, 0.16f, 0.24f, 1f);
        private static readonly Color BarTrack = new Color(0.25f, 0.28f, 0.35f, 1f);
        private static readonly Color BarFill = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color BtnPlay = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color BtnSecondary = new Color(0.22f, 0.26f, 0.36f, 1f);
        private static readonly Color DotRed = new Color(0.92f, 0.30f, 0.30f, 1f);

        [MenuItem("Tools/Kitforge/UI Kit/Build Group E Sample")]
        public static void BuildAll() => BuildAllInternal(true);

        public static bool BuildAllForAudit() => BuildAllInternal(false);

        private static bool BuildAllInternal(bool interactive)
        {
            EnsureFolders("Catalog_GroupE_Demo");
            if (!CheckTheme(interactive)) return false;
            if (!CheckGroupCBuilt(interactive)) return false;
            BuildLoadingScreen();
            BuildMainMenuScreen();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildDemoScene();
            if (interactive) EditorUtility.DisplayDialog("Kitforge UI Kit",
                $"Group E built at {OutputRoot}.\n\n2 prefabs + 1 scene generated.\nOpen GroupE_BootDemo.unity → Play → right-click Demo → Boot Demo.",
                "OK");
            return true;
        }

        private static bool CheckTheme(bool interactive)
        {
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath) != null) return true;
            if (!interactive) { Debug.LogError($"[CatalogGroupEBuilder] Theme missing at {DefaultThemePath}. Run 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first."); return false; }
            return EditorUtility.DisplayDialog(
                "Bootstrap Defaults missing",
                $"No Theme found at {DefaultThemePath}.\n\nRun 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first, or proceed without a Theme reference.",
                "Proceed", "Cancel");
        }

        private static bool CheckGroupCBuilt(bool interactive)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(GroupCDailyLoginPath) != null) return true;
            if (!interactive) { Debug.LogError($"[CatalogGroupEBuilder] Group C not built. DailyLoginPopup missing at {GroupCDailyLoginPath}. Run Build Group C Sample first."); return false; }
            EditorUtility.DisplayDialog(
                "Group C not built",
                $"DailyLoginPopup.prefab not found at:\n{GroupCDailyLoginPath}\n\nImport 'Catalog — Group C — Progression' via Package Manager, then run Tools/Kitforge/UI Kit/Build Group C Sample first.",
                "OK");
            return false;
        }

        // ── Prefab builders ─────────────────────────────────────────────────

        private static LoadingScreen BuildLoadingScreen()
        {
            var root = CreateRoot("LoadingScreen");
            AddThemedImage(root, BgDark, ThemeSpriteSlot.None, ThemeColorSlot.BackgroundDark);
            BuildLoadingContent(root.transform, out var title, out var subtitle, out var spinner);
            var (track, fill) = BuildProgressBar(root.transform);
            root.AddComponent<UIAnim_LoadingScreen>();
            var screen = root.AddComponent<LoadingScreen>();
            WireLoadingRefs(screen, title, subtitle, spinner, track, fill);
            return SaveAsPrefab<LoadingScreen>(root, LoadingPath);
        }

        private static void BuildLoadingContent(Transform parent, out TextMeshProUGUI title, out TextMeshProUGUI subtitle, out Image spinner)
        {
            var content = CreateChild(parent, "Content");
            PositionCentered(content.GetComponent<RectTransform>(), 0f, 60f, 800f, 280f);
            ApplyVerticalLayout(content, 16f);
            title = CreateLabel(content.transform, "TitleLabel", "Loading...", 40, FontStyles.Bold, 800f, 56f, ThemeBuilderSlots.DarkBgHeading);
            subtitle = CreateLabel(content.transform, "SubtitleLabel", "", 28, FontStyles.Normal, 800f, 40f, ThemeBuilderSlots.DarkBgBody);
            spinner = CreateSpinner(content.transform);
        }

        private static Image CreateSpinner(Transform parent)
        {
            var go = CreateChild(parent, "SpinnerImage");
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 80f;
            le.preferredHeight = 80f;
            var img = AddImage(go, TextLightColor);
            img.raycastTarget = false;
            return img;
        }

        private static (Image track, Image fill) BuildProgressBar(Transform parent)
        {
            var bar = CreateChild(parent, "ProgressBar");
            var rt = bar.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0f);
            rt.anchorMax = new Vector2(0.9f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 100f);
            rt.sizeDelta = new Vector2(0f, 20f);
            var track = AddThemedImage(bar, BarTrack, ThemeSpriteSlot.None, ThemeColorSlot.MutedColor);
            track.raycastTarget = false;
            return (track, BuildProgressFill(bar.transform));
        }

        private static Image BuildProgressFill(Transform parent)
        {
            var go = CreateChild(parent, "ProgressFill");
            StretchInside(go.GetComponent<RectTransform>());
            var img = AddThemedImage(go, BarFill, ThemeSpriteSlot.None, ThemeColorSlot.LoadingBarColor);
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 0f;
            img.raycastTarget = false;
            return img;
        }

        private static void WireLoadingRefs(LoadingScreen screen, TextMeshProUGUI title, TextMeshProUGUI subtitle, Image spinner, Image track, Image fill)
        {
            var so = new SerializedObject(screen);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.SubtitleLabel").objectReferenceValue = subtitle;
            so.FindProperty("_refs.SpinnerImage").objectReferenceValue = spinner;
            so.FindProperty("_refs.ProgressBarTrack").objectReferenceValue = track;
            so.FindProperty("_refs.ProgressBarFill").objectReferenceValue = fill;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static MainMenuScreen BuildMainMenuScreen()
        {
            var root = CreateRoot("MainMenuScreen");
            AddThemedImage(root, BgMainMenu, ThemeSpriteSlot.None, ThemeColorSlot.BackgroundDark);
            var panel = CreatePanel(root.transform, "Panel", 800f, 1400f);
            var title = CreateThemedText(panel.transform, "TitleLabel", "", 48, FontStyles.Bold, ThemeBuilderSlots.DarkBgHeading);
            AnchorTopStretch(title.GetComponent<RectTransform>(), -60f, 60f);
            title.alignment = TextAlignmentOptions.Center;
            var logo = CreateLogoSlot(panel.transform);
            var (play, settings, shop, daily) = BuildMainMenuButtons(panel.transform);
            var dot = BuildDailyDot(daily.gameObject.transform);
            var animator = root.AddComponent<UIAnim_MainMenuScreen>();
            var screen = root.AddComponent<MainMenuScreen>();
            WireMainMenuScreenRefs(screen, animator, panel, title, logo, play, settings, shop, daily, dot);
            return SaveAsPrefab<MainMenuScreen>(root, MainMenuPath);
        }

        private static Image CreateLogoSlot(Transform parent)
        {
            var go = CreateChild(parent, "LogoImage");
            AnchorTopStretch(go.GetComponent<RectTransform>(), -30f, 120f);
            var img = AddImage(go, new Color(1f, 1f, 1f, 0f));
            img.raycastTarget = false;
            img.preserveAspect = true;
            return img;
        }

        private static (Button play, Button settings, Button shop, Button daily) BuildMainMenuButtons(Transform panel)
        {
            var group = CreateButtonGroup(panel);
            var play = BuildMenuButton(group.transform, "PlayButton", "Play", BtnPlay, ThemeColorSlot.PrimaryColor);
            var settings = BuildMenuButton(group.transform, "SettingsButton", "Settings", BtnSecondary, ThemeColorSlot.MutedColor);
            var shop = BuildMenuButton(group.transform, "ShopButton", "Shop", BtnSecondary, ThemeColorSlot.MutedColor);
            var daily = BuildMenuButton(group.transform, "DailyButton", "Daily Login", BtnSecondary, ThemeColorSlot.MutedColor);
            return (play, settings, shop, daily);
        }

        private static GameObject CreateButtonGroup(Transform parent)
        {
            var go = CreateChild(parent, "ButtonGroup");
            PositionCentered(go.GetComponent<RectTransform>(), 0f, -100f, 700f, 500f);
            ApplyVerticalLayout(go, 20f);
            return go;
        }

        private static Button BuildMenuButton(Transform parent, string name, string label, Color bg, ThemeColorSlot bgSlot)
        {
            var go = CreateChild(parent, name);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 680f;
            le.preferredHeight = 100f;
            le.minHeight = 88f;
            var img = AddThemedImage(go, bg, ThemeSpriteSlot.None, bgSlot);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var text = CreateThemedText(go.transform, "Label", label, 32, FontStyles.Bold, ThemeBuilderSlots.DarkBgBody);
            StretchInside(text.GetComponent<RectTransform>());
            text.alignment = TextAlignmentOptions.Center;
            return btn;
        }

        private static GameObject BuildDailyDot(Transform parent)
        {
            var go = CreateChild(parent, "DailyIndicatorDot");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(-10f, -10f);
            rt.sizeDelta = new Vector2(24f, 24f);
            AddThemedImage(go, DotRed, ThemeSpriteSlot.None, ThemeColorSlot.DangerColor);
            return go;
        }

        private static void WireMainMenuScreenRefs(MainMenuScreen screen, UIAnim_MainMenuScreen animator, GameObject panel, TextMeshProUGUI title, Image logo, Button play, Button settings, Button shop, Button daily, GameObject dot)
        {
            var so = new SerializedObject(screen);
            so.FindProperty("_refs.TitleLabel").objectReferenceValue = title;
            so.FindProperty("_refs.LogoImage").objectReferenceValue = logo;
            so.FindProperty("_refs.PlayButton").objectReferenceValue = play;
            so.FindProperty("_refs.SettingsButton").objectReferenceValue = settings;
            so.FindProperty("_refs.ShopButton").objectReferenceValue = shop;
            so.FindProperty("_refs.DailyButton").objectReferenceValue = daily;
            so.FindProperty("_refs.DailyIndicatorDot").objectReferenceValue = dot;
            so.ApplyModifiedPropertiesWithoutUndo();
            WireMainMenuAnimator(animator, panel, new[] { play, settings, shop, daily });
        }

        private static void WireMainMenuAnimator(UIAnim_MainMenuScreen animator, GameObject panel, Button[] buttons)
        {
            var so = new SerializedObject(animator);
            so.FindProperty("_panel").objectReferenceValue = panel.GetComponent<RectTransform>();
            var stagger = so.FindProperty("_staggerItems");
            stagger.arraySize = buttons.Length;
            for (var i = 0; i < buttons.Length; i++)
                stagger.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i].GetComponent<RectTransform>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Demo scene ──────────────────────────────────────────────────────

        private static void BuildDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            EditorSceneFactory.CreateMainCamera(scene);
            var (canvasRoot, screenRoot, popupRoot, toastRoot, backdropGO) = BuildUICanvas(scene);
            BuildEventSystem(scene);
            var (uiManager, popupManager, router, toastManager) = BuildManagers(scene);
            var services = BuildServicesGO(scene);
            WireAllManagers(uiManager, popupManager, router, toastManager, services, theme, screenRoot, popupRoot, toastRoot, backdropGO);
            BuildDemoHost(scene, uiManager, popupManager, services);
            BuildThemeSwitcher(scene, canvasRoot, uiManager, popupManager, toastManager);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static (Transform canvasRoot, Transform screenRoot, Transform popupRoot, Transform toastRoot, GameObject backdrop) BuildUICanvas(Scene scene)
        {
            var canvasGO = new GameObject("UICanvas");
            SceneManager.MoveGameObjectToScene(canvasGO, scene);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();
            var screenRoot = CreateStretchChild(canvasGO.transform, "Screens");
            var popupRoot = CreateStretchChild(canvasGO.transform, "Popups");
            var backdrop = CreateBackdropIn(popupRoot);
            var toastRoot = CreateStretchChild(canvasGO.transform, "Toasts");
            BuildInstructionsPanel(canvasGO.transform);
            return (canvasGO.transform, screenRoot, popupRoot, toastRoot, backdrop);
        }

        private static void BuildInstructionsPanel(Transform parent)
        {
            var go = CreateChild(parent, "Instructions");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -16f);
            rt.sizeDelta = new Vector2(-32f, 160f);
            var bg = AddImage(go, new Color(0f, 0f, 0f, 0.55f));
            bg.raycastTarget = false;
            var text = CreateThemedText(go.transform, "InstructionsText", InstructionsText, 26, FontStyles.Normal, ThemeBuilderSlots.DarkBgBody);
            StretchInside(text.GetComponent<RectTransform>());
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
        }

        private static GameObject CreateBackdropIn(Transform parent)
        {
            var go = CreateChild(parent, "Backdrop");
            StretchInside(go.GetComponent<RectTransform>());
            var img = AddImage(go, BackdropColor);
            img.raycastTarget = true;
            go.SetActive(false);
            return go;
        }

        private static void BuildEventSystem(Scene scene)
        {
            var go = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static (UIManager, PopupManager, UIRouter, ToastManager) BuildManagers(Scene scene)
        {
            var go = new GameObject("UIRoot");
            SceneManager.MoveGameObjectToScene(go, scene);
            var ui = go.AddComponent<UIManager>();
            var pm = go.AddComponent<PopupManager>();
            var router = go.AddComponent<UIRouter>();
            var toast = go.AddComponent<ToastManager>();
            return (ui, pm, router, toast);
        }

        private static UIServices BuildServicesGO(Scene scene)
        {
            var go = new GameObject("UIServices");
            SceneManager.MoveGameObjectToScene(go, scene);
            var services = go.AddComponent<UIServices>();
            TryAttachProgressionService(go, services);
            TryAttachTimeService(go, services);
            return services;
        }

        private static void TryAttachProgressionService(GameObject host, UIServices services)
        {
            var progType = System.Type.GetType(ProgressionTypeName);
            if (progType == null)
            {
                Debug.LogWarning("[CatalogGroupEBuilder] InMemoryProgressionService not found. Import 'Catalog — Group C — Progression' sample (Package Manager → Samples).");
                return;
            }
            var prog = (MonoBehaviour)host.AddComponent(progType);
            var so = new SerializedObject(services);
            so.FindProperty("_progressionServiceRef").objectReferenceValue = prog;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void TryAttachTimeService(GameObject host, UIServices services)
        {
            var timeType = System.Type.GetType(TimeServiceTypeName);
            if (timeType == null)
            {
                Debug.LogWarning("[CatalogGroupEBuilder] InMemoryTimeService not found. Import 'Catalog — Group C — Progression' sample first.");
                return;
            }
            var svc = (MonoBehaviour)host.AddComponent(timeType);
            var so = new SerializedObject(services);
            so.FindProperty("_timeServiceRef").objectReferenceValue = svc;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireAllManagers(UIManager ui, PopupManager pm, UIRouter router, ToastManager toast, UIServices services, UIThemeConfig theme, Transform screenRoot, Transform popupRoot, Transform toastRoot, GameObject backdrop)
        {
            WireUIManager(ui, theme, services, screenRoot);
            WirePopupManager(pm, theme, services, popupRoot, backdrop);
            WireUIRouter(router, ui, pm);
            WireToastManager(toast, theme, services, toastRoot);
        }

        private static void WireUIManager(UIManager ui, UIThemeConfig theme, UIServices services, Transform screenRoot)
        {
            var loadingPrefab = AssetDatabase.LoadAssetAtPath<LoadingScreen>(LoadingPath);
            var mainMenuPrefab = AssetDatabase.LoadAssetAtPath<MainMenuScreen>(MainMenuPath);
            var so = new SerializedObject(ui);
            so.FindProperty("_themeConfig").objectReferenceValue = theme;
            so.FindProperty("_services").objectReferenceValue = services;
            so.FindProperty("_screenRoot").objectReferenceValue = screenRoot;
            var prefabs = so.FindProperty("_screenPrefabs");
            prefabs.arraySize = 2;
            prefabs.GetArrayElementAtIndex(0).objectReferenceValue = loadingPrefab;
            prefabs.GetArrayElementAtIndex(1).objectReferenceValue = mainMenuPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WirePopupManager(PopupManager pm, UIThemeConfig theme, UIServices services, Transform popupRoot, GameObject backdrop)
        {
            var dailyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GroupCDailyLoginPath);
            var so = new SerializedObject(pm);
            so.FindProperty("_themeConfig").objectReferenceValue = theme;
            so.FindProperty("_services").objectReferenceValue = services;
            so.FindProperty("_popupRoot").objectReferenceValue = popupRoot;
            so.FindProperty("_backdrop").objectReferenceValue = backdrop;
            if (dailyPrefab != null)
            {
                so.FindProperty("_popupPrefabs").arraySize = 1;
                so.FindProperty("_popupPrefabs").GetArrayElementAtIndex(0).objectReferenceValue = dailyPrefab.GetComponent<UIModuleBase>();
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireUIRouter(UIRouter router, UIManager ui, PopupManager pm)
        {
            var so = new SerializedObject(router);
            so.FindProperty("_uiManager").objectReferenceValue = ui;
            so.FindProperty("_popupManager").objectReferenceValue = pm;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireToastManager(ToastManager toast, UIThemeConfig theme, UIServices services, Transform toastRoot)
        {
            var so = new SerializedObject(toast);
            so.FindProperty("_themeConfig").objectReferenceValue = theme;
            so.FindProperty("_services").objectReferenceValue = services;
            so.FindProperty("_toastRoot").objectReferenceValue = toastRoot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildDemoHost(Scene scene, UIManager ui, PopupManager pm, UIServices services)
        {
            var go = new GameObject("Demo");
            SceneManager.MoveGameObjectToScene(go, scene);
            var demoType = System.Type.GetType(DemoHostTypeName);
            if (demoType == null)
            {
                Debug.LogWarning("[CatalogGroupEBuilder] GroupEDemoHost type not found. Make sure the Group E sample is imported (Package Manager → Samples → Catalog — Group E — Screens).");
                return;
            }
            var demo = go.AddComponent(demoType);
            WireDemoHost(demo, ui, pm, services);
        }

        private static void WireDemoHost(Component demo, UIManager ui, PopupManager pm, UIServices services)
        {
            var so = new SerializedObject(demo);
            so.FindProperty("_uiManager").objectReferenceValue = ui;
            so.FindProperty("_popupManager").objectReferenceValue = pm;
            so.FindProperty("_services").objectReferenceValue = services;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ── Theme switcher (M4.X — manual swap E2E for screens) ─────────────

        private static void BuildThemeSwitcher(Scene scene, Transform canvasRoot, UIManager ui, PopupManager pm, ToastManager toast)
        {
            var go = new GameObject("ThemeSwitcher");
            SceneManager.MoveGameObjectToScene(go, scene);
            var dropdown = BuildThemeDropdown(canvasRoot);
            var switcherType = System.Type.GetType(ThemeSwitcherTypeName);
            if (switcherType == null)
            {
                Debug.LogWarning("[CatalogGroupEBuilder] ThemeSwitcherEScreens type not found. Make sure the Group E sample is imported (Package Manager → Samples → Catalog — Group E — Screens).");
                return;
            }
            var switcher = go.AddComponent(switcherType);
            WireThemeSwitcher(switcher, dropdown, ui, pm, toast);
        }

        private static TMP_Dropdown BuildThemeDropdown(Transform canvasRoot)
        {
            var resources = new TMP_DefaultControls.Resources();
            var dropdownGO = TMP_DefaultControls.CreateDropdown(resources);
            dropdownGO.name = "ThemeDropdown";
            dropdownGO.transform.SetParent(canvasRoot, false);
            var rt = dropdownGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-24f, -200f);
            rt.sizeDelta = new Vector2(360f, 80f);
            return dropdownGO.GetComponent<TMP_Dropdown>();
        }

        private static void WireThemeSwitcher(Component switcher, TMP_Dropdown dropdown, UIManager ui, PopupManager pm, ToastManager toast)
        {
            var defaultTheme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var casual = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(CasualThemePath);
            var premium = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(PremiumThemePath);
            var so = new SerializedObject(switcher);
            so.FindProperty("_uiManager").objectReferenceValue = ui;
            so.FindProperty("_popupManager").objectReferenceValue = pm;
            so.FindProperty("_toastManager").objectReferenceValue = toast;
            so.FindProperty("_themeDropdown").objectReferenceValue = dropdown;
            WireThemeOptions(so.FindProperty("_themes"), defaultTheme, casual, premium);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireThemeOptions(SerializedProperty themesProp, UIThemeConfig defaultTheme, UIThemeConfig casual, UIThemeConfig premium)
        {
            var hasPair = casual != null && premium != null;
            themesProp.arraySize = hasPair ? 3 : 1;
            WireThemeOption(themesProp.GetArrayElementAtIndex(0), "Default", defaultTheme);
            if (hasPair)
            {
                WireThemeOption(themesProp.GetArrayElementAtIndex(1), "Casual", casual);
                WireThemeOption(themesProp.GetArrayElementAtIndex(2), "Premium", premium);
                return;
            }
            Debug.LogWarning("[CatalogGroupEBuilder] Theme_Casual.asset / Theme_Premium.asset not found at Assets/Catalog_M4_ThemePresets_Demo/Themes/. Run 'Tools/Kitforge/UI Kit/Build M4.1 — Theme Presets' first for full theme swap coverage. Falling back to Default-only dropdown.");
        }

        private static void WireThemeOption(SerializedProperty option, string name, UIThemeConfig theme)
        {
            option.FindPropertyRelative("Name").stringValue = name;
            option.FindPropertyRelative("Theme").objectReferenceValue = theme;
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static GameObject CreatePanel(Transform parent, string name, float width, float height)
        {
            var go = CreateChild(parent, name);
            PositionCentered(go.GetComponent<RectTransform>(), 0f, 0f, width, height);
            return go;
        }

        private static Transform CreateStretchChild(Transform parent, string name)
        {
            var go = CreateChild(parent, name);
            StretchInside(go.GetComponent<RectTransform>());
            return go.transform;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int size, FontStyles style, float width, float height, TextThemeSlot slot)
        {
            var label = CreateThemedText(parent, name, text, size, style, slot);
            var le = label.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = width;
            le.preferredHeight = height;
            return label;
        }

        private static void ApplyVerticalLayout(GameObject go, float spacing)
        {
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
        }

        private static void PositionCentered(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void AnchorTopStretch(RectTransform rt, float y, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(-40f, height);
        }

    }
}
