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
        private const string GroupCDailyLoginPath = "Assets/Catalog_GroupC_Demo/Prefabs/DailyLoginPopup.prefab";
        private const string ProgressionTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryProgressionService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string TimeServiceTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryTimeService, KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string DemoHostTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupE.GroupEDemoHost, KitforgeLabs.MobileUIKit.Samples.CatalogGroupE";
        private const string InstructionsText = "DEMO: Right-click GroupEDemoHost in Hierarchy → ContextMenu (Boot Demo / Show MainMenu / Trigger DailyLogin). Button clicks log to Console (Window → General → Console).";

        private static readonly Color BgDark = new Color(0.10f, 0.12f, 0.16f, 1f);
        private static readonly Color BgMainMenu = new Color(0.12f, 0.16f, 0.24f, 1f);
        private static readonly Color BarTrack = new Color(0.25f, 0.28f, 0.35f, 1f);
        private static readonly Color BarFill = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color BtnPlay = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color BtnSecondary = new Color(0.22f, 0.26f, 0.36f, 1f);
        private static readonly Color DotRed = new Color(0.92f, 0.30f, 0.30f, 1f);
        private static readonly Color TextLight = Color.white;
        private static readonly Color BackdropColor = new Color(0f, 0f, 0f, 0.55f);

        [MenuItem("Tools/Kitforge/UI Kit/Build Group E Sample")]
        public static void BuildAll()
        {
            EnsureFolders();
            if (!CheckTheme()) return;
            if (!CheckGroupCBuilt()) return;
            BuildLoadingScreen();
            BuildMainMenuScreen();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildDemoScene();
            EditorUtility.DisplayDialog("Kitforge UI Kit",
                $"Group E built at {OutputRoot}.\n\n2 prefabs + 1 scene generated.\nOpen GroupE_BootDemo.unity → Play → right-click Demo → Boot Demo.",
                "OK");
        }

        private static bool CheckTheme()
        {
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath) != null) return true;
            return EditorUtility.DisplayDialog(
                "Bootstrap Defaults missing",
                $"No Theme found at {DefaultThemePath}.\n\nRun 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first, or proceed without a Theme reference.",
                "Proceed", "Cancel");
        }

        private static bool CheckGroupCBuilt()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(GroupCDailyLoginPath) != null) return true;
            EditorUtility.DisplayDialog(
                "Group C not built",
                $"DailyLoginPopup.prefab not found at:\n{GroupCDailyLoginPath}\n\nImport 'Catalog — Group C — Progression' via Package Manager, then run Tools/Kitforge/UI Kit/Build Group C Sample first.",
                "OK");
            return false;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) AssetDatabase.CreateFolder("Assets", "Catalog_GroupE_Demo");
            if (!AssetDatabase.IsValidFolder(PrefabsFolder)) AssetDatabase.CreateFolder(OutputRoot, "Prefabs");
        }

        // ── Prefab builders ─────────────────────────────────────────────────

        private static LoadingScreen BuildLoadingScreen()
        {
            var root = CreateRoot("LoadingScreen");
            AddImage(root, BgDark);
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
            title = CreateLabel(content.transform, "TitleLabel", "Loading...", 40, FontStyles.Bold, 800f, 56f);
            subtitle = CreateLabel(content.transform, "SubtitleLabel", "", 28, FontStyles.Normal, 800f, 40f);
            spinner = CreateSpinner(content.transform);
        }

        private static Image CreateSpinner(Transform parent)
        {
            var go = CreateChild(parent, "SpinnerImage");
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 80f;
            le.preferredHeight = 80f;
            var img = AddImage(go, TextLight);
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
            var track = AddImage(bar, BarTrack);
            track.raycastTarget = false;
            return (track, BuildProgressFill(bar.transform));
        }

        private static Image BuildProgressFill(Transform parent)
        {
            var go = CreateChild(parent, "ProgressFill");
            StretchInside(go.GetComponent<RectTransform>());
            var img = AddImage(go, BarFill);
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
            AddImage(root, BgMainMenu);
            var panel = CreatePanel(root.transform, "Panel", 800f, 1400f);
            var title = CreateText(panel.transform, "TitleLabel", "", 48, FontStyles.Bold);
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
            var play = BuildMenuButton(group.transform, "PlayButton", "Play", BtnPlay);
            var settings = BuildMenuButton(group.transform, "SettingsButton", "Settings", BtnSecondary);
            var shop = BuildMenuButton(group.transform, "ShopButton", "Shop", BtnSecondary);
            var daily = BuildMenuButton(group.transform, "DailyButton", "Daily Login", BtnSecondary);
            return (play, settings, shop, daily);
        }

        private static GameObject CreateButtonGroup(Transform parent)
        {
            var go = CreateChild(parent, "ButtonGroup");
            PositionCentered(go.GetComponent<RectTransform>(), 0f, -100f, 700f, 500f);
            ApplyVerticalLayout(go, 20f);
            return go;
        }

        private static Button BuildMenuButton(Transform parent, string name, string label, Color bg)
        {
            var go = CreateChild(parent, name);
            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = 680f;
            le.preferredHeight = 100f;
            le.minHeight = 88f;
            var img = AddImage(go, bg);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var text = CreateText(go.transform, "Label", label, 32, FontStyles.Bold);
            StretchInside(text.GetComponent<RectTransform>());
            text.alignment = TextAlignmentOptions.Center;
            text.color = TextLight;
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
            AddImage(go, DotRed);
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
            var (screenRoot, popupRoot, toastRoot, backdropGO) = BuildUICanvas(scene);
            BuildEventSystem(scene);
            var (uiManager, popupManager, router, toastManager) = BuildManagers(scene);
            var services = BuildServicesGO(scene);
            WireAllManagers(uiManager, popupManager, router, toastManager, services, theme, screenRoot, popupRoot, toastRoot, backdropGO);
            BuildDemoHost(scene, uiManager, popupManager, services);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static (Transform screenRoot, Transform popupRoot, Transform toastRoot, GameObject backdrop) BuildUICanvas(Scene scene)
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
            return (screenRoot, popupRoot, toastRoot, backdrop);
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
            var text = CreateText(go.transform, "InstructionsText", InstructionsText, 26, FontStyles.Normal);
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

        // ── Helpers ─────────────────────────────────────────────────────────

        private static GameObject CreateRoot(string name)
        {
            var go = new GameObject(name);
            StretchInside(go.AddComponent<RectTransform>());
            go.AddComponent<CanvasGroup>();
            return go;
        }

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

        private static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Image AddImage(GameObject go, Color color)
        {
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, int size, FontStyles style)
        {
            var go = CreateChild(parent, name);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = TextLight;
            return tmp;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string name, string text, int size, FontStyles style, float width, float height)
        {
            var label = CreateText(parent, name, text, size, style);
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

        private static void StretchInside(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
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

        private static T SaveAsPrefab<T>(GameObject root, string path) where T : Component
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab.GetComponent<T>();
        }
    }
}
