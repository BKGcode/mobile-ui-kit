using System;
using System.Collections.Generic;
using KitforgeLabs.MobileUIKit.Catalog.Toasts;
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
    public static class CatalogAllDemoBuilder
    {
        private const string OutputRoot = "Assets/Catalog_All_Demo";
        private const string ScenePath = OutputRoot + "/AllDemo.unity";
        private const string DefaultThemePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset";
        private const string CasualThemePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Casual.asset";
        private const string PremiumThemePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Premium.asset";
        private const string GroupBStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupB";
        private const string GroupCStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupC";
        private const string GroupDStubAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogGroupD";
        private const string SampleAsmdef = "KitforgeLabs.MobileUIKit.Samples.CatalogAllDemo";
        private const string HostTypeName = "KitforgeLabs.MobileUIKit.Samples.CatalogAllDemo.CatalogAllDemoHost, " + SampleAsmdef;

        private static readonly Color HUDPanelColor = new Color(0f, 0f, 0f, 0.55f);
        private static readonly Color UIBgColor = new Color(0.10f, 0.12f, 0.16f, 1f);
        private static readonly Color ButtonColor = new Color(0.20f, 0.55f, 0.95f, 1f);

        private static readonly PrefabSpec[] PopupPrefabs =
        {
            new("ConfirmPopup", "Assets/Catalog_GroupA_Demo/Prefabs/ConfirmPopup.prefab"),
            new("PausePopup", "Assets/Catalog_GroupA_Demo/Prefabs/PausePopup.prefab"),
            new("TutorialPopup", "Assets/Catalog_GroupA_Demo/Prefabs/TutorialPopup.prefab"),
            new("RewardPopup", "Assets/Catalog_GroupB_Demo/Prefabs/RewardPopup.prefab"),
            new("ShopPopup", "Assets/Catalog_GroupB_Demo/Prefabs/ShopPopup.prefab"),
            new("NotEnoughCurrencyPopup", "Assets/Catalog_GroupB_Demo/Prefabs/NotEnoughCurrencyPopup.prefab"),
            new("DailyLoginPopup", "Assets/Catalog_GroupC_Demo/Prefabs/DailyLoginPopup.prefab"),
            new("LevelCompletePopup", "Assets/Catalog_GroupC_Demo/Prefabs/LevelCompletePopup.prefab"),
            new("GameOverPopup", "Assets/Catalog_GroupC_Demo/Prefabs/GameOverPopup.prefab"),
            new("SettingsPopup", "Assets/Catalog_GroupD_Demo/Prefabs/SettingsPopup.prefab"),
        };

        private static readonly PrefabSpec[] ScreenPrefabs =
        {
            new("LoadingScreen", "Assets/Catalog_GroupE_Demo/Prefabs/LoadingScreen.prefab"),
            new("MainMenuScreen", "Assets/Catalog_GroupE_Demo/Prefabs/MainMenuScreen.prefab"),
        };

        private static readonly PrefabSpec[] ToastPrefabs =
        {
            new("NotificationToast", "Assets/Catalog_GroupA_Demo/Prefabs/NotificationToast.prefab"),
        };

        [MenuItem("Tools/Kitforge/UI Kit/Build Catalog_All_Demo")]
        public static void BuildAll() => BuildAllInternal(true);

        public static bool BuildAllForAudit() => BuildAllInternal(false);

        private static bool BuildAllInternal(bool interactive)
        {
            if (!CheckDependencies(interactive)) return false;
            EnsureFolders();
            BuildDemoScene();
            if (interactive) EditorUtility.DisplayDialog(
                "Kitforge UI Kit — Catalog_All_Demo",
                $"Generated {ScenePath}.\n\nOpen the scene → Play → click any popup button. Use the Theme dropdown to validate the skin-it-once contract across Default / Casual / Premium.",
                "OK");
            return true;
        }

        private static bool CheckDependencies(bool interactive)
        {
            var missing = new List<string>();
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath) == null) missing.Add(DefaultThemePath);
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(CasualThemePath) == null) missing.Add(CasualThemePath);
            if (AssetDatabase.LoadAssetAtPath<UIThemeConfig>(PremiumThemePath) == null) missing.Add(PremiumThemePath);
            for (var i = 0; i < PopupPrefabs.Length; i++)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(PopupPrefabs[i].Path) == null) missing.Add(PopupPrefabs[i].Path);
            for (var i = 0; i < ScreenPrefabs.Length; i++)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(ScreenPrefabs[i].Path) == null) missing.Add(ScreenPrefabs[i].Path);
            for (var i = 0; i < ToastPrefabs.Length; i++)
                if (AssetDatabase.LoadAssetAtPath<GameObject>(ToastPrefabs[i].Path) == null) missing.Add(ToastPrefabs[i].Path);
            if (missing.Count == 0) return true;
            var msg = $"Missing prerequisites — import + build the catalog group samples and M4.1 first.\n\nMissing assets:\n• {string.Join("\n• ", missing)}\n\nSee README in Samples/Catalog — All — Single-import master demo for the full prerequisite list.";
            if (!interactive) { Debug.LogError($"[CatalogAllDemoBuilder] {msg}"); return false; }
            EditorUtility.DisplayDialog("Catalog_All_Demo prerequisites missing", msg, "OK");
            return false;
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder(OutputRoot)) AssetDatabase.CreateFolder("Assets", "Catalog_All_Demo");
        }

        private static void BuildDemoScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneFactory.CreateMainCamera(scene);
            BuildEventSystem(scene);
            var defaultTheme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            var casualTheme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(CasualThemePath);
            var premiumTheme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(PremiumThemePath);
            var canvasGO = BuildCanvas(scene);
            var screenRoot = CreateStretchChild(canvasGO.transform, "ScreenRoot");
            var popupRoot = CreateStretchChild(canvasGO.transform, "PopupRoot");
            var toastRoot = CreateStretchChild(canvasGO.transform, "ToastRoot");
            var services = BuildServices(scene);
            var (uiManager, popupManager, toastManager) = BuildManagers(scene, defaultTheme, services, screenRoot, popupRoot, toastRoot);
            var (themeDropdown, buttonRefs) = BuildSidebarAndDropdown(canvasGO.transform);
            BuildHost(scene, uiManager, popupManager, toastManager, services, themeDropdown, buttonRefs, defaultTheme, casualTheme, premiumTheme);
            EditorSceneManager.SaveScene(scene, ScenePath);
        }

        private static GameObject BuildCanvas(Scene scene)
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
            return canvasGO;
        }

        private static (UIManager ui, PopupManager popup, ToastManager toast) BuildManagers(Scene scene, UIThemeConfig theme, UIServices services, RectTransform screenRoot, RectTransform popupRoot, RectTransform toastRoot)
        {
            var uiHost = new GameObject("UIManager");
            SceneManager.MoveGameObjectToScene(uiHost, scene);
            var ui = uiHost.AddComponent<UIManager>();
            var popupHost = new GameObject("PopupManager");
            SceneManager.MoveGameObjectToScene(popupHost, scene);
            var popup = popupHost.AddComponent<PopupManager>();
            var toastHost = new GameObject("ToastManager");
            SceneManager.MoveGameObjectToScene(toastHost, scene);
            var toast = toastHost.AddComponent<ToastManager>();
            WireManagerArrays(ui, popup, toast, theme, services, screenRoot, popupRoot, toastRoot);
            return (ui, popup, toast);
        }

        private static void WireManagerArrays(UIManager ui, PopupManager popup, ToastManager toast, UIThemeConfig theme, UIServices services, RectTransform screenRoot, RectTransform popupRoot, RectTransform toastRoot)
        {
            var uiSO = new SerializedObject(ui);
            uiSO.FindProperty("_themeConfig").objectReferenceValue = theme;
            uiSO.FindProperty("_services").objectReferenceValue = services;
            uiSO.FindProperty("_screenRoot").objectReferenceValue = screenRoot;
            var screensProp = uiSO.FindProperty("_screenPrefabs");
            screensProp.arraySize = ScreenPrefabs.Length;
            for (var i = 0; i < ScreenPrefabs.Length; i++)
                screensProp.GetArrayElementAtIndex(i).objectReferenceValue = LoadComponent(ScreenPrefabs[i].Path, typeof(UIModuleBase));
            uiSO.ApplyModifiedPropertiesWithoutUndo();

            var popupSO = new SerializedObject(popup);
            popupSO.FindProperty("_themeConfig").objectReferenceValue = theme;
            popupSO.FindProperty("_services").objectReferenceValue = services;
            popupSO.FindProperty("_popupRoot").objectReferenceValue = popupRoot;
            var popupsProp = popupSO.FindProperty("_popupPrefabs");
            popupsProp.arraySize = PopupPrefabs.Length;
            for (var i = 0; i < PopupPrefabs.Length; i++)
                popupsProp.GetArrayElementAtIndex(i).objectReferenceValue = LoadComponent(PopupPrefabs[i].Path, typeof(UIModuleBase));
            popupSO.ApplyModifiedPropertiesWithoutUndo();

            var toastSO = new SerializedObject(toast);
            toastSO.FindProperty("_themeConfig").objectReferenceValue = theme;
            toastSO.FindProperty("_services").objectReferenceValue = services;
            toastSO.FindProperty("_toastRoot").objectReferenceValue = toastRoot;
            var toastsProp = toastSO.FindProperty("_toastPrefabs");
            toastsProp.arraySize = ToastPrefabs.Length;
            for (var i = 0; i < ToastPrefabs.Length; i++)
                toastsProp.GetArrayElementAtIndex(i).objectReferenceValue = LoadComponent(ToastPrefabs[i].Path, typeof(UIToastBase));
            toastSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UnityEngine.Object LoadComponent(string prefabPath, Type componentType)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) return null;
            return prefab.GetComponent(componentType);
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
            var shop = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupB.InMemoryShopDataProvider, {GroupBStubAsmdef}");
            var time = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryTimeService, {GroupCStubAsmdef}");
            var progression = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupC.InMemoryProgressionService, {GroupCStubAsmdef}");
            var playerData = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupD.InMemoryPlayerDataService, {GroupDStubAsmdef}");
            var localization = TryAddComponent(host, $"KitforgeLabs.MobileUIKit.Samples.CatalogGroupD.InMemoryLocalizationService, {GroupDStubAsmdef}");
            if (economy == null || ads == null || time == null || progression == null)
                Debug.LogWarning("[CatalogAllDemoBuilder] Some Group B / C / D stubs not found. Import all 5 group samples + M4.1 (Package Manager → Samples) so their asmdefs compile, then re-run this builder.");
            if (progression != null && economy != null)
            {
                var progSO = new SerializedObject(progression);
                var prop = progSO.FindProperty("_economyServiceRef");
                if (prop != null) { prop.objectReferenceValue = economy; progSO.ApplyModifiedPropertiesWithoutUndo(); }
            }
            var so = new SerializedObject(services);
            if (economy != null) so.FindProperty("_economyServiceRef").objectReferenceValue = economy;
            if (ads != null) so.FindProperty("_adsServiceRef").objectReferenceValue = ads;
            if (shop != null) so.FindProperty("_shopDataProviderRef").objectReferenceValue = shop;
            if (time != null) so.FindProperty("_timeServiceRef").objectReferenceValue = time;
            if (progression != null) so.FindProperty("_progressionServiceRef").objectReferenceValue = progression;
            if (playerData != null) so.FindProperty("_playerDataServiceRef").objectReferenceValue = playerData;
            if (localization != null) so.FindProperty("_localizationServiceRef").objectReferenceValue = localization;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static MonoBehaviour TryAddComponent(GameObject host, string assemblyQualifiedName)
        {
            var t = Type.GetType(assemblyQualifiedName);
            return t != null ? (MonoBehaviour)host.AddComponent(t) : null;
        }

        private static (TMP_Dropdown dropdown, ButtonRefs buttons) BuildSidebarAndDropdown(Transform canvasParent)
        {
            var topBar = CreateChild(canvasParent, "TopBar");
            var topRT = topBar.GetComponent<RectTransform>();
            topRT.anchorMin = new Vector2(0f, 1f);
            topRT.anchorMax = new Vector2(1f, 1f);
            topRT.pivot = new Vector2(0.5f, 1f);
            topRT.anchoredPosition = new Vector2(0f, -16f);
            topRT.sizeDelta = new Vector2(-32f, 120f);
            var topBg = topBar.AddComponent<Image>();
            topBg.color = HUDPanelColor;
            topBg.raycastTarget = false;
            var label = CreateLabel(topBar.transform, "Theme:", 28, FontStyles.Bold);
            AnchorTopLeft(label.GetComponent<RectTransform>(), 24f, -24f, 200f, 48f);
            var dropdown = BuildDropdown(topBar.transform);

            var sidebar = CreateChild(canvasParent, "Sidebar");
            var sideRT = sidebar.GetComponent<RectTransform>();
            sideRT.anchorMin = new Vector2(1f, 0f);
            sideRT.anchorMax = new Vector2(1f, 1f);
            sideRT.pivot = new Vector2(1f, 0.5f);
            sideRT.anchoredPosition = new Vector2(-16f, -84f);
            sideRT.sizeDelta = new Vector2(440f, -200f);
            var sideBg = sidebar.AddComponent<Image>();
            sideBg.color = HUDPanelColor;
            sideBg.raycastTarget = false;
            var layout = sidebar.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            var refs = new ButtonRefs
            {
                Confirm = AddSidebarButton(sidebar.transform, "Group A — Confirm"),
                Pause = AddSidebarButton(sidebar.transform, "Group A — Pause"),
                Tutorial = AddSidebarButton(sidebar.transform, "Group A — Tutorial"),
                Toast = AddSidebarButton(sidebar.transform, "Group A — Toast"),
                Reward = AddSidebarButton(sidebar.transform, "Group B — Reward"),
                Shop = AddSidebarButton(sidebar.transform, "Group B — Shop"),
                NotEnough = AddSidebarButton(sidebar.transform, "Group B — Not Enough"),
                DailyLogin = AddSidebarButton(sidebar.transform, "Group C — Daily Login"),
                LevelComplete = AddSidebarButton(sidebar.transform, "Group C — Level Complete"),
                GameOver = AddSidebarButton(sidebar.transform, "Group C — Game Over"),
                Settings = AddSidebarButton(sidebar.transform, "Group D — Settings"),
                LoadingScreen = AddSidebarButton(sidebar.transform, "Group E — Loading Screen"),
                MainMenuScreen = AddSidebarButton(sidebar.transform, "Group E — Main Menu Screen"),
                AddCoins = AddSidebarButton(sidebar.transform, "HUD — Add 100 coins"),
                AddGems = AddSidebarButton(sidebar.transform, "HUD — Add 5 gems"),
                SpendCoins = AddSidebarButton(sidebar.transform, "HUD — Spend 30 coins"),
            };
            return (dropdown, refs);
        }

        private static Button AddSidebarButton(Transform parent, string label)
        {
            var go = CreateChild(parent, label);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 56f;
            var img = go.AddComponent<Image>();
            img.color = ButtonColor;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var text = CreateLabel(go.transform, label, 22, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;
            StretchInside(text.GetComponent<RectTransform>());
            return btn;
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

        private static void BuildBackground(Transform parent)
        {
            var go = CreateChild(parent, "Background");
            StretchInside(go.GetComponent<RectTransform>());
            var img = go.AddComponent<Image>();
            img.color = UIBgColor;
            img.raycastTarget = false;
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

        private static void BuildEventSystem(Scene scene)
        {
            var go = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static void BuildHost(Scene scene, UIManager ui, PopupManager popup, ToastManager toast, UIServices services, TMP_Dropdown dropdown, ButtonRefs buttons, UIThemeConfig defaultTheme, UIThemeConfig casualTheme, UIThemeConfig premiumTheme)
        {
            var go = new GameObject("Demo");
            SceneManager.MoveGameObjectToScene(go, scene);
            var hostType = Type.GetType(HostTypeName);
            if (hostType == null)
            {
                Debug.LogWarning("[CatalogAllDemoBuilder] CatalogAllDemoHost type not found. Make sure 'Catalog — All — Single-import master demo' sample is imported (Package Manager → Samples) so its asmdef compiles.");
                return;
            }
            var host = go.AddComponent(hostType);
            var so = new SerializedObject(host);
            so.FindProperty("_uiManager").objectReferenceValue = ui;
            so.FindProperty("_popupManager").objectReferenceValue = popup;
            so.FindProperty("_toastManager").objectReferenceValue = toast;
            so.FindProperty("_services").objectReferenceValue = services;
            so.FindProperty("_themeDropdown").objectReferenceValue = dropdown;

            var themesProp = so.FindProperty("_themes");
            themesProp.arraySize = 3;
            WireThemeOption(themesProp.GetArrayElementAtIndex(0), "Default", defaultTheme);
            WireThemeOption(themesProp.GetArrayElementAtIndex(1), "Casual", casualTheme);
            WireThemeOption(themesProp.GetArrayElementAtIndex(2), "Premium", premiumTheme);

            so.FindProperty("_btnConfirm").objectReferenceValue = buttons.Confirm;
            so.FindProperty("_btnPause").objectReferenceValue = buttons.Pause;
            so.FindProperty("_btnTutorial").objectReferenceValue = buttons.Tutorial;
            so.FindProperty("_btnToast").objectReferenceValue = buttons.Toast;
            so.FindProperty("_btnReward").objectReferenceValue = buttons.Reward;
            so.FindProperty("_btnShop").objectReferenceValue = buttons.Shop;
            so.FindProperty("_btnNotEnough").objectReferenceValue = buttons.NotEnough;
            so.FindProperty("_btnDailyLogin").objectReferenceValue = buttons.DailyLogin;
            so.FindProperty("_btnLevelComplete").objectReferenceValue = buttons.LevelComplete;
            so.FindProperty("_btnGameOver").objectReferenceValue = buttons.GameOver;
            so.FindProperty("_btnSettings").objectReferenceValue = buttons.Settings;
            so.FindProperty("_btnLoadingScreen").objectReferenceValue = buttons.LoadingScreen;
            so.FindProperty("_btnMainMenuScreen").objectReferenceValue = buttons.MainMenuScreen;
            so.FindProperty("_btnAddCoins").objectReferenceValue = buttons.AddCoins;
            so.FindProperty("_btnAddGems").objectReferenceValue = buttons.AddGems;
            so.FindProperty("_btnSpendCoins").objectReferenceValue = buttons.SpendCoins;

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

        // ── Records ─────────────────────────────────────────────────────────

        private readonly struct PrefabSpec
        {
            public readonly string Name;
            public readonly string Path;
            public PrefabSpec(string name, string path) { Name = name; Path = path; }
        }

        private struct ButtonRefs
        {
            public Button Confirm, Pause, Tutorial, Toast;
            public Button Reward, Shop, NotEnough;
            public Button DailyLogin, LevelComplete, GameOver;
            public Button Settings;
            public Button LoadingScreen, MainMenuScreen;
            public Button AddCoins, AddGems, SpendCoins;
        }
    }
}
