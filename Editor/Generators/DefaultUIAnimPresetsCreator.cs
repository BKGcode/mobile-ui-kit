using KitforgeLabs.MobileUIKit.Animation;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Generators
{
    public static class DefaultUIAnimPresetsCreator
    {
        private const string FolderRoot = "Assets";
        private const string FolderSettings = "Settings";
        private const string FolderPresets = "UIAnimPresets";
        private const string FolderPath = "Assets/Settings/UIAnimPresets";

        [MenuItem("Tools/Kitforge/Create Default UIAnimPresets")]
        public static void CreateAll()
        {
            EnsureFolder();
            CreatePreset(UIAnimStyle.Snappy,     0.20f, UIAnimEase.OutBack,    0.15f, UIAnimEase.InQuad,     1.05f);
            CreatePreset(UIAnimStyle.Bouncy,     0.45f, UIAnimEase.OutElastic, 0.25f, UIAnimEase.InBack,     1.15f);
            CreatePreset(UIAnimStyle.Playful,    0.40f, UIAnimEase.OutBack,    0.20f, UIAnimEase.InQuad,     1.10f);
            CreatePreset(UIAnimStyle.Punchy,     0.10f, UIAnimEase.OutQuad,    0.10f, UIAnimEase.InQuad,     1.20f);
            CreatePreset(UIAnimStyle.Smooth,     0.35f, UIAnimEase.OutCubic,   0.25f, UIAnimEase.InOutSine,  1.00f);
            CreatePreset(UIAnimStyle.Elegant,    0.50f, UIAnimEase.OutSine,    0.40f, UIAnimEase.InSine,     1.00f);
            CreatePreset(UIAnimStyle.Juicy,      0.40f, UIAnimEase.OutElastic, 0.20f, UIAnimEase.InQuad,     1.15f);
            CreatePreset(UIAnimStyle.Soft,       0.50f, UIAnimEase.InOutSine,  0.35f, UIAnimEase.InOutSine,  1.00f);
            CreatePreset(UIAnimStyle.Mechanical, 0.05f, UIAnimEase.Linear,     0.05f, UIAnimEase.Linear,     1.00f);
            CreatePreset(UIAnimStyle.Cinematic,  0.60f, UIAnimEase.OutCubic,   0.30f, UIAnimEase.OutCubic,   1.05f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("UI Anim Presets", $"10 default presets created at {FolderPath}.", "OK");
        }

        private static void EnsureFolder()
        {
            var settings = $"{FolderRoot}/{FolderSettings}";
            if (!AssetDatabase.IsValidFolder(settings)) AssetDatabase.CreateFolder(FolderRoot, FolderSettings);
            if (!AssetDatabase.IsValidFolder(FolderPath)) AssetDatabase.CreateFolder(settings, FolderPresets);
        }

        private static void CreatePreset(UIAnimStyle style, float showDuration, UIAnimEase showEase,
            float hideDuration, UIAnimEase hideEase, float overshoot)
        {
            var assetPath = $"{FolderPath}/UIAnimPreset_{style}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<UIAnimPreset>(assetPath);
            if (existing != null)
            {
                ApplyValues(existing, style, showDuration, showEase, hideDuration, hideEase, overshoot);
                return;
            }
            var asset = ScriptableObject.CreateInstance<UIAnimPreset>();
            AssetDatabase.CreateAsset(asset, assetPath);
            ApplyValues(asset, style, showDuration, showEase, hideDuration, hideEase, overshoot);
        }

        private static void ApplyValues(UIAnimPreset asset, UIAnimStyle style, float showDuration, UIAnimEase showEase,
            float hideDuration, UIAnimEase hideEase, float overshoot)
        {
            var so = new SerializedObject(asset);
            so.FindProperty("_style").enumValueIndex = (int)style;
            so.FindProperty("_showDuration").floatValue = showDuration;
            so.FindProperty("_showEase").enumValueIndex = (int)showEase;
            so.FindProperty("_hideDuration").floatValue = hideDuration;
            so.FindProperty("_hideEase").enumValueIndex = (int)hideEase;
            so.FindProperty("_scaleOvershoot").floatValue = overshoot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
