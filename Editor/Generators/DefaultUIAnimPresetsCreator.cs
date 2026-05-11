using System.IO;
using KitforgeLabs.UIKit.Animation;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Generators
{
    public static class DefaultUIAnimPresetsCreator
    {
        private const string PresetsFolder = "Assets/KitforgeLabs/UI Kit/Settings/UIAnimPresets";

        [MenuItem("KitforgeLabs/UI Kit/Bootstrap Defaults")]
        public static void CreateAll()
        {
            EnsureFolder();
            CreatePresets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "KitforgeLabs · UI Kit",
                $"10 UIAnimPreset assets are ready at {PresetsFolder}.\n\nDrop KitforgeRoot.prefab in your scene, then drag any preset onto UIThemeConfig._defaultAnimPreset, or create your own UIThemeConfig via Assets → Create → KitforgeLabs → UI Kit → Theme.",
                "OK");
        }

        private static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(PresetsFolder)) return;
            Directory.CreateDirectory(PresetsFolder);
            AssetDatabase.Refresh();
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
            var assetPath = $"{PresetsFolder}/UIAnimPreset_{name}.asset";
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
    }
}
