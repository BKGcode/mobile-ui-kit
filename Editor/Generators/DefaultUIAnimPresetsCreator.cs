using KitforgeLabs.MobileUIKit.Animation;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Generators
{
    public static class DefaultUIAnimPresetsCreator
    {
        private const string FolderAssets = "Assets";
        private const string FolderSettings = "Settings";
        private const string FolderPresetsName = "UIAnimPresets";
        private const string FolderPresets = "Assets/Settings/UIAnimPresets";
        private const string FolderUIName = "UI";
        private const string FolderUI = "Assets/Settings/UI";
        private const string DefaultPresetName = "Playful";
        private const string DefaultThemePath = "Assets/Settings/UI/UIThemeConfig_Default.asset";

        [MenuItem("Tools/Kitforge/Create Default UI Theme + Presets")]
        public static void CreateAll()
        {
            EnsureFolders();
            CreatePresets();
            var theme = EnsureDefaultTheme();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = theme;
            EditorGUIUtility.PingObject(theme);
            EditorUtility.DisplayDialog(
                "Kitforge UI Kit",
                $"10 presets at {FolderPresets}.\n1 default Theme at {DefaultThemePath} (Playful preset wired).\n\nAssign this Theme to your UIManager / PopupManager / ToastManager.",
                "OK");
        }

        private static void EnsureFolders()
        {
            var settings = $"{FolderAssets}/{FolderSettings}";
            if (!AssetDatabase.IsValidFolder(settings)) AssetDatabase.CreateFolder(FolderAssets, FolderSettings);
            if (!AssetDatabase.IsValidFolder(FolderPresets)) AssetDatabase.CreateFolder(settings, FolderPresetsName);
            if (!AssetDatabase.IsValidFolder(FolderUI)) AssetDatabase.CreateFolder(settings, FolderUIName);
        }

        private static void CreatePresets()
        {
            CreatePreset("Snappy",     0.20f, UIAnimEase.OutBack,    0.15f, UIAnimEase.InQuad,     1.05f);
            CreatePreset("Bouncy",     0.45f, UIAnimEase.OutElastic, 0.25f, UIAnimEase.InBack,     1.15f);
            CreatePreset("Playful",    0.40f, UIAnimEase.OutBack,    0.20f, UIAnimEase.InQuad,     1.10f);
            CreatePreset("Punchy",     0.10f, UIAnimEase.OutQuad,    0.10f, UIAnimEase.InQuad,     1.20f);
            CreatePreset("Smooth",     0.35f, UIAnimEase.OutCubic,   0.25f, UIAnimEase.InOutSine,  1.00f);
            CreatePreset("Elegant",    0.50f, UIAnimEase.OutSine,    0.40f, UIAnimEase.InSine,     1.00f);
            CreatePreset("Juicy",      0.40f, UIAnimEase.OutElastic, 0.20f, UIAnimEase.InQuad,     1.15f);
            CreatePreset("Soft",       0.50f, UIAnimEase.InOutSine,  0.35f, UIAnimEase.InOutSine,  1.00f);
            CreatePreset("Mechanical", 0.05f, UIAnimEase.Linear,     0.05f, UIAnimEase.Linear,     1.00f);
            CreatePreset("Cinematic",  0.60f, UIAnimEase.OutCubic,   0.30f, UIAnimEase.OutCubic,   1.05f);
        }

        private static void CreatePreset(string name, float showDuration, UIAnimEase showEase,
            float hideDuration, UIAnimEase hideEase, float overshoot)
        {
            var assetPath = $"{FolderPresets}/UIAnimPreset_{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<UIAnimPreset>(assetPath);
            var asset = existing != null ? existing : CreateAsset(assetPath);
            ApplyValues(asset, showDuration, showEase, hideDuration, hideEase, overshoot);
        }

        private static UIAnimPreset CreateAsset(string assetPath)
        {
            var asset = ScriptableObject.CreateInstance<UIAnimPreset>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        private static void ApplyValues(UIAnimPreset asset, float showDuration, UIAnimEase showEase,
            float hideDuration, UIAnimEase hideEase, float overshoot)
        {
            var so = new SerializedObject(asset);
            so.FindProperty("_showDuration").floatValue = showDuration;
            so.FindProperty("_showEase").enumValueIndex = (int)showEase;
            so.FindProperty("_hideDuration").floatValue = hideDuration;
            so.FindProperty("_hideEase").enumValueIndex = (int)hideEase;
            so.FindProperty("_scaleOvershoot").floatValue = overshoot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static UIThemeConfig EnsureDefaultTheme()
        {
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(DefaultThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<UIThemeConfig>();
                AssetDatabase.CreateAsset(theme, DefaultThemePath);
            }
            WireDefaultPreset(theme);
            return theme;
        }

        private static void WireDefaultPreset(UIThemeConfig theme)
        {
            var presetPath = $"{FolderPresets}/UIAnimPreset_{DefaultPresetName}.asset";
            var preset = AssetDatabase.LoadAssetAtPath<UIAnimPreset>(presetPath);
            if (preset == null) return;
            var so = new SerializedObject(theme);
            var prop = so.FindProperty("_defaultAnimPreset");
            if (prop == null || prop.objectReferenceValue == preset) return;
            prop.objectReferenceValue = preset;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
