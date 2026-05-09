using System;
using KitforgeLabs.MobileUIKit.Services;
using KitforgeLabs.MobileUIKit.Theme;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Editor.Generators
{
    public static class CatalogM41ThemeBuilder
    {
        private const string OutputRoot = "Assets/Catalog_M4_ThemePresets_Demo";
        private const string ThemesFolder = OutputRoot + "/Themes";
        private const string CasualPath = ThemesFolder + "/Theme_Casual.asset";
        private const string PremiumPath = ThemesFolder + "/Theme_Premium.asset";
        private const string ScenePath = OutputRoot + "/ThemePresetsDemo.unity";
        private const string DefaultThemePath = "Assets/Settings/UI/UIThemeConfig_Default.asset";
        private const string GameOverPrefabPath = "Assets/Catalog_GroupC_Demo/Prefabs/GameOverPopup.prefab";
        private const string LevelCompletePrefabPath = "Assets/Catalog_GroupC_Demo/Prefabs/LevelCompletePopup.prefab";
        private const string GroupBStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupB";
        private const string GroupCStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string SampleAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogM41ThemePresets";
        private const string SwitcherTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogM41ThemePresets.ThemeSwitcherSample, " + SampleAsmdef;
        private const string InstructionsText = "M4.1 — Theme Presets demo. Use the dropdown to switch theme; press a button to spawn the popup. Same prefab + 3 themes = 3 distinct visual identities (skin-it-once contract).";

        private static readonly Color HUDPanelColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color UIBgColor = new Color(0.10f, 0.12f, 0.16f, 1f);

        [MenuItem("Tools/Kitforge/UI Kit/Build M4.1 — Theme Presets")]
        public static void BuildAll() => BuildAllInternal(true);

        public static bool BuildAllForAudit() => BuildAllInternal(false);

        private static bool BuildAllInternal(bool interactive)
        {
            if (!CheckDependencies(interactive)) return false;
            EnsureFolders();
            var defaultTheme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            BuildThemeAsset(defaultTheme, CasualPath, CasualPalette);
            BuildThemeAsset(defaultTheme, PremiumPath, PremiumPalette);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildDemoScene();
            if (interactive) EditorUtility.DisplayDialog(
                "Kitforge UI Kit — M4.1 Theme Presets",
                $"Generated under {OutputRoot}:\n• Theme_Casual.asset (bright/saturated)\n• Theme_Premium.asset (dark/desaturated)\n• ThemePresetsDemo.unity\n\nOpen the scene → Play → use the Theme dropdown + buttons to validate the skin-it-once contract.",
                "OK");
            return true;
        }

        private static bool CheckDependencies(bool interactive)
        {
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath) == null)
            {
                if (!interactive) { Debug.LogError($"[CatalogM41ThemeBuilder] Theme missing at {DefaultThemePath}. Run 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first."); return false; }
                EditorUtility.DisplayDialog("Bootstrap Defaults missing", $"No Theme found at {DefaultThemePath}.\n\nRun 'Tools/Kitforge/UI Kit/Bootstrap Defaults' first.", "OK");
                return false;
            }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(GameOverPrefabPath) == null
                || AssetDatabase.LoadAssetAtPath<GameObject>(LevelCompletePrefabPath) == null)
            {
                if (!interactive) { Debug.LogError($"[CatalogM41ThemeBuilder] Group C prefabs missing under Assets/Catalog_GroupC_Demo/Prefabs/. Run Build Group C Sample first."); return false; }
                EditorUtility.DisplayDialog(
                    "Group C not built",
                    $"GameOver / LevelComplete prefabs missing under Assets/Catalog_GroupC_Demo/Prefabs/.\n\nImport 'Catalog — Group C — Progression' via Package Manager, then run Tools/Kitforge/UI Kit/Build Group C Sample first.",
                    "OK");
                return false;
            }
            return true;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) AssetDatabase.CreateFolder("Assets", "Catalog_M4_ThemePresets_Demo");
            if (!AssetDatabase.IsValidFolder(ThemesFolder)) AssetDatabase.CreateFolder(OutputRoot, "Themes");
        }

        // ── Theme cloning ───────────────────────────────────────────────────

        private static UIThemeConfig BuildThemeAsset(UIThemeConfig source, string path, ColorPalette palette)
        {
            var sourcePath = AssetDatabase.GetAssetPath(source);
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(path) != null) AssetDatabase.DeleteAsset(path);
            AssetDatabase.CopyAsset(sourcePath, path);
            var asset = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(path);
            var so = new SerializedObject(asset);
            ApplyPalette(so, palette);
            so.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static void ApplyPalette(SerializedObject so, ColorPalette p)
        {
            so.FindProperty("_primaryColor").colorValue = p.Primary;
            so.FindProperty("_secondaryColor").colorValue = p.Secondary;
            so.FindProperty("_accentColor").colorValue = p.Accent;
            so.FindProperty("_backgroundDark").colorValue = p.BackgroundDark;
            so.FindProperty("_backgroundLight").colorValue = p.BackgroundLight;
            so.FindProperty("_textPrimary").colorValue = p.TextPrimary;
            so.FindProperty("_textSecondary").colorValue = p.TextSecondary;
            so.FindProperty("_successColor").colorValue = p.Success;
            so.FindProperty("_warningColor").colorValue = p.Warning;
            so.FindProperty("_dangerColor").colorValue = p.Danger;
            so.FindProperty("_failureColor").colorValue = p.Failure;
            so.FindProperty("_tertiaryColor").colorValue = p.Tertiary;
            so.FindProperty("_mutedColor").colorValue = p.Muted;
            so.FindProperty("_textOnPrimary").colorValue = p.TextOnPrimary;
            so.FindProperty("_textOnAccent").colorValue = p.TextOnAccent;
            so.FindProperty("_loadingBarColor").colorValue = p.LoadingBar;
        }

        // ── Demo scene ──────────────────────────────────────────────────────

        private static void BuildDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var defaultTheme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var casual = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(CasualPath);
            var premium = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(PremiumPath);
            EditorSceneFactory.CreateMainCamera(scene);
            BuildEventSystem(scene);
            var (canvas, popupRoot, dropdown, gameOverButton, levelCompleteButton) = BuildUICanvas(scene);
            var services = BuildServices(scene);
            BuildDemoHost(scene, popupRoot, services, dropdown, gameOverButton, levelCompleteButton, defaultTheme, casual, premium);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static (GameObject canvas, RectTransform popupRoot, TMP_Dropdown dropdown, Button gameOverBtn, Button levelCompleteBtn) BuildUICanvas(Scene scene)
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
            BuildBackground(canvasGO.transform);
            var popupRoot = CreateStretchChild(canvasGO.transform, "PopupRoot");
            var (dropdown, goBtn, lcBtn) = BuildHUDPanel(canvasGO.transform);
            BuildInstructions(canvasGO.transform);
            return (canvasGO, popupRoot, dropdown, goBtn, lcBtn);
        }

        private static void BuildBackground(Transform parent)
        {
            var go = CreateChild(parent, "Background");
            StretchInside(go.GetComponent<RectTransform>());
            var img = go.AddComponent<Image>();
            img.color = UIBgColor;
            img.raycastTarget = false;
        }

        private static (TMP_Dropdown dropdown, Button gameOverBtn, Button levelCompleteBtn) BuildHUDPanel(Transform parent)
        {
            var panel = CreateChild(parent, "HUDPanel");
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -16f);
            rt.sizeDelta = new Vector2(-32f, 280f);
            var bg = panel.AddComponent<Image>();
            bg.color = HUDPanelColor;
            bg.raycastTarget = false;
            var label = CreateLabel(panel.transform, "Theme:", 28, FontStyles.Bold);
            AnchorTopLeft(label.GetComponent<RectTransform>(), 24f, -24f, 200f, 48f);
            var dropdown = BuildDropdown(panel.transform);
            var gameOverBtn = BuildButton(panel.transform, "ShowGameOverButton", "Show GameOver");
            AnchorBottomLeft(gameOverBtn.GetComponent<RectTransform>(), 24f, 24f, 460f, 96f);
            var lcBtn = BuildButton(panel.transform, "ShowLevelCompleteButton", "Show LevelComplete");
            AnchorBottomRight(lcBtn.GetComponent<RectTransform>(), -24f, 24f, 460f, 96f);
            return (dropdown, gameOverBtn, lcBtn);
        }

        private static TMP_Dropdown BuildDropdown(Transform parent)
        {
            var resources = new TMP_DefaultControls.Resources();
            var dropdownGO = TMP_DefaultControls.CreateDropdown(resources);
            dropdownGO.name = "ThemeDropdown";
            dropdownGO.transform.SetParent(parent, false);
            var rt = dropdownGO.GetComponent<RectTransform>();
            AnchorTopLeft(rt, 220f, -24f, 480f, 80f);
            return dropdownGO.GetComponent<TMP_Dropdown>();
        }

        private static Button BuildButton(Transform parent, string name, string label)
        {
            var go = CreateChild(parent, name);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.20f, 0.55f, 0.95f, 1f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var text = CreateLabel(go.transform, label, 30, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            StretchInside(text.GetComponent<RectTransform>());
            return btn;
        }

        private static TextMeshProUGUI CreateLabel(Transform parent, string text, int size, FontStyles style)
        {
            var go = CreateChild(parent, "Label");
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            return tmp;
        }

        private static void BuildInstructions(Transform parent)
        {
            var go = CreateChild(parent, "Instructions");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 24f);
            rt.sizeDelta = new Vector2(-32f, 140f);
            var bg = go.AddComponent<Image>();
            bg.color = HUDPanelColor;
            bg.raycastTarget = false;
            var text = CreateLabel(go.transform, InstructionsText, 22, FontStyles.Normal);
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            StretchInside(text.GetComponent<RectTransform>());
        }

        private static void BuildEventSystem(Scene scene)
        {
            var go = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static UIServices BuildServices(Scene scene)
        {
            var go = new GameObject("UIServices");
            SceneManager.MoveGameObjectToScene(go, scene);
            var services = go.AddComponent<UIServices>();
            TryAttachStubs(go, services);
            return services;
        }

        private static void TryAttachStubs(GameObject host, UIServices services)
        {
            var economy = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryEconomyService, {GroupBStubAsmdef}");
            var ads = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryAdsService, {GroupBStubAsmdef}");
            var time = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryTimeService, {GroupCStubAsmdef}");
            var progression = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryProgressionService, {GroupCStubAsmdef}");
            if (economy == null || ads == null || time == null || progression == null)
                Debug.LogWarning("[CatalogM41ThemeBuilder] Some Group B/C stubs not found. Import both samples (Package Manager → Samples) so their asmdefs compile, then re-run this builder.");
            if (progression != null && economy != null)
            {
                var progSO = new SerializedObject(progression);
                var prop = progSO.FindProperty("_economyServiceRef");
                if (prop != null) { prop.objectReferenceValue = economy; progSO.ApplyModifiedPropertiesWithoutUndo(); }
            }
            var so = new SerializedObject(services);
            if (economy != null) so.FindProperty("_economyServiceRef").objectReferenceValue = economy;
            if (ads != null) so.FindProperty("_adsServiceRef").objectReferenceValue = ads;
            if (time != null) so.FindProperty("_timeServiceRef").objectReferenceValue = time;
            if (progression != null) so.FindProperty("_progressionServiceRef").objectReferenceValue = progression;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static MonoBehaviour TryAddComponent(GameObject host, string assemblyQualifiedName)
        {
            var t = Type.GetType(assemblyQualifiedName);
            return t != null ? (MonoBehaviour)host.AddComponent(t) : null;
        }

        private static void BuildDemoHost(Scene scene, RectTransform popupRoot, UIServices services, TMP_Dropdown dropdown, Button gameOverBtn, Button levelCompleteBtn, UIThemeConfig defaultTheme, UIThemeConfig casual, UIThemeConfig premium)
        {
            var go = new GameObject("Demo");
            SceneManager.MoveGameObjectToScene(go, scene);
            var switcherType = Type.GetType(SwitcherTypeName);
            if (switcherType == null)
            {
                Debug.LogWarning("[CatalogM41ThemeBuilder] ThemeSwitcherSample type not found. Make sure 'Catalog — M4.1 — Theme Presets' sample is imported (Package Manager → Samples) so its asmdef compiles.");
                return;
            }
            var switcher = go.AddComponent(switcherType);
            var gameOverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GameOverPrefabPath);
            var levelCompletePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelCompletePrefabPath);
            var so = new SerializedObject(switcher);
            so.FindProperty("_popupParent").objectReferenceValue = popupRoot;
            so.FindProperty("_services").objectReferenceValue = services;
            so.FindProperty("_themeDropdown").objectReferenceValue = dropdown;
            so.FindProperty("_showGameOverButton").objectReferenceValue = gameOverBtn;
            so.FindProperty("_showLevelCompleteButton").objectReferenceValue = levelCompleteBtn;
            so.FindProperty("_gameOverPrefab").objectReferenceValue = gameOverPrefab;
            so.FindProperty("_levelCompletePrefab").objectReferenceValue = levelCompletePrefab;
            var themesProp = so.FindProperty("_themes");
            themesProp.arraySize = 3;
            WireThemeOption(themesProp.GetArrayElementAtIndex(0), "Default", defaultTheme);
            WireThemeOption(themesProp.GetArrayElementAtIndex(1), "Casual", casual);
            WireThemeOption(themesProp.GetArrayElementAtIndex(2), "Premium", premium);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireThemeOption(SerializedProperty option, string name, UIThemeConfig theme)
        {
            option.FindPropertyRelative("Name").stringValue = name;
            option.FindPropertyRelative("Theme").objectReferenceValue = theme;
        }

        // ── Layout helpers ──────────────────────────────────────────────────

        private static GameObject CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            return go;
        }

        private static RectTransform CreateStretchChild(Transform parent, string name)
        {
            var go = CreateChild(parent, name);
            var rt = go.GetComponent<RectTransform>();
            StretchInside(rt);
            return rt;
        }

        private static void StretchInside(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void AnchorTopLeft(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void AnchorBottomLeft(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        private static void AnchorBottomRight(RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
        }

        // ── Palettes ────────────────────────────────────────────────────────

        private struct ColorPalette
        {
            public Color Primary, Secondary, Tertiary, Accent, Muted;
            public Color BackgroundDark, BackgroundLight;
            public Color TextPrimary, TextSecondary, TextOnPrimary, TextOnAccent;
            public Color Success, Warning, Danger, Failure, LoadingBar;
        }

        private static readonly ColorPalette CasualPalette = new ColorPalette
        {
            Primary = Hex("#FF7B2B"), Secondary = Hex("#00B5A8"), Tertiary = Hex("#8DC93B"),
            Accent = Hex("#FFC93B"), Muted = Hex("#B5AAA0"),
            BackgroundDark = Hex("#2E1B4D"), BackgroundLight = Hex("#FFF7E8"),
            TextPrimary = Hex("#3D2817"), TextSecondary = Hex("#7A5A40"),
            TextOnPrimary = Hex("#FFFFFF"), TextOnAccent = Hex("#2A1A0F"),
            Success = Hex("#4CDB4C"), Warning = Hex("#FFA830"),
            Danger = Hex("#FF4D5E"), Failure = Hex("#D62828"),
            LoadingBar = Hex("#FFC93B")
        };

        private static readonly ColorPalette PremiumPalette = new ColorPalette
        {
            Primary = Hex("#1A8B6F"), Secondary = Hex("#3B5C7A"), Tertiary = Hex("#8A6A3B"),
            Accent = Hex("#C9A55A"), Muted = Hex("#3A3F4A"),
            BackgroundDark = Hex("#0D1117"), BackgroundLight = Hex("#1F232B"),
            TextPrimary = Hex("#E8E0CE"), TextSecondary = Hex("#A89C82"),
            TextOnPrimary = Hex("#E8E0CE"), TextOnAccent = Hex("#1A1410"),
            Success = Hex("#4A9D8A"), Warning = Hex("#C97C3B"),
            Danger = Hex("#A53048"), Failure = Hex("#7A1A1A"),
            LoadingBar = Hex("#C9A55A")
        };

        private static Color Hex(string hex)
        {
            return ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;
        }
    }
}
