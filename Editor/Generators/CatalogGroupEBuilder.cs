using KitforgeLabs.UIKit.Catalog.Screens;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static KitforgeLabs.UIKit.Editor.Generators.CatalogGroupBuilderShared;

namespace KitforgeLabs.UIKit.Editor.Generators
{
    internal static class CatalogGroupEBuilder
    {
        private const string DefaultThemePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset";

        private static string LoadingPath => $"{ResolvePrefabsFolder()}/LoadingScreen.prefab";
        private static string MainMenuPath => $"{ResolvePrefabsFolder()}/MainMenuScreen.prefab";

        private static readonly Color BgDark = new Color(0.10f, 0.12f, 0.16f, 1f);
        private static readonly Color BgMainMenu = new Color(0.12f, 0.16f, 0.24f, 1f);
        private static readonly Color BarTrack = new Color(0.25f, 0.28f, 0.35f, 1f);
        private static readonly Color BarFill = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color BtnPlay = new Color(0.20f, 0.55f, 0.95f, 1f);
        private static readonly Color BtnSecondary = new Color(0.22f, 0.26f, 0.36f, 1f);
        private static readonly Color DotRed = new Color(0.92f, 0.30f, 0.30f, 1f);

        public static void BuildAll()
        {
            EnsurePrefabsFolder(ResolvePrefabsFolder());
            BuildLoadingScreen();
            BuildMainMenuScreen();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
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
    }
}
