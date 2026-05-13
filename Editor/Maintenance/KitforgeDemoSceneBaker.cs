using System.IO;
using KitforgeLabs.UIKit.Bootstrap;
using KitforgeLabs.UIKit.Core;
using KitforgeLabs.UIKit.Demo;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Services.Demo;
using KitforgeLabs.UIKit.Theme;
using KitforgeLabs.UIKit.Toast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Maintenance
{
    public static class KitforgeDemoSceneBaker
    {
        private const string DestinationScenePath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Demo/KitforgeDemoScene.unity";
        private const string KitforgeRootPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab";
        private const string ThemeDefaultPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset";
        private const string ThemeCasualPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Casual.asset";
        private const string ThemePremiumPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Premium.asset";

        [MenuItem("Tools/KitforgeLabs/Test/Bake Demo Scene")]
        public static void Bake()
        {
            if (!ValidateWritable()) return;
            var prefab = LoadKitforgeRoot();
            if (prefab == null) return;
            var themes = LoadThemes();
            if (themes == null) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateMainCamera();
            var root = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            AttachDemoComponents(root, themes);
            EnsureDirectory(DestinationScenePath);
            EditorSceneManager.SaveScene(scene, DestinationScenePath);
            AssetDatabase.ImportAsset(DestinationScenePath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[KitforgeDemoSceneBaker] Demo scene baked at {DestinationScenePath}. Commit the .unity asset to ship it with the package.");
        }

        private static void CreateMainCamera()
        {
            var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 1f, -10f);
            var cam = camGO.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.13f, 0.17f, 1f);
            cam.orthographic = false;
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
        }

        private static bool ValidateWritable()
        {
            var rootAssetPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.AssetPathToGUID(KitforgeRootPath));
            if (string.IsNullOrEmpty(rootAssetPath))
            {
                Debug.LogError("[KitforgeDemoSceneBaker] KitforgeRoot.prefab not found. Verify the package is installed.");
                return false;
            }
            var fullPath = Path.GetFullPath(rootAssetPath);
            if (fullPath.Replace("\\", "/").Contains("/Library/PackageCache/"))
            {
                Debug.LogError("[KitforgeDemoSceneBaker] Package is installed via git URL (read-only). Embed it locally before baking — edit Packages/manifest.json to use 'file:' path.");
                return false;
            }
            return true;
        }

        private static GameObject LoadKitforgeRoot()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(KitforgeRootPath);
            if (prefab == null) Debug.LogError($"[KitforgeDemoSceneBaker] KitforgeRoot.prefab missing at {KitforgeRootPath}.");
            return prefab;
        }

        private static UIThemeConfig[] LoadThemes()
        {
            var themes = new[]
            {
                AssetDatabase.LoadAssetAtPath<UIThemeConfig>(ThemeDefaultPath),
                AssetDatabase.LoadAssetAtPath<UIThemeConfig>(ThemeCasualPath),
                AssetDatabase.LoadAssetAtPath<UIThemeConfig>(ThemePremiumPath)
            };
            for (var i = 0; i < themes.Length; i++)
            {
                if (themes[i] != null) continue;
                Debug.LogError($"[KitforgeDemoSceneBaker] Theme preset missing at slot {i}. Verify Runtime/Theme/Presets/ assets.");
                return null;
            }
            return themes;
        }

        private static void AttachDemoComponents(GameObject root, UIThemeConfig[] themes)
        {
            var uiServices = root.GetComponentInChildren<UIServices>(true);
            var binder = root.GetComponent<KitforgeThemeBinder>();
            var popups = root.GetComponentInChildren<PopupManager>(true);
            var screens = root.GetComponentInChildren<UIManager>(true);
            var toasts = root.GetComponentInChildren<ToastManager>(true);
            AttachServicesBootstrap(root, uiServices);
            AttachMenuController(root, popups, screens, toasts, binder, themes);
        }

        private static void AttachServicesBootstrap(GameObject root, UIServices uiServices)
        {
            var bootstrapGO = new GameObject("DemoServicesBootstrap");
            bootstrapGO.transform.SetParent(root.transform, false);
            var bootstrap = bootstrapGO.AddComponent<DemoServicesBootstrap>();
            var so = new SerializedObject(bootstrap);
            so.FindProperty("_uiServices").objectReferenceValue = uiServices;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AttachMenuController(GameObject root, PopupManager popups, UIManager screens, ToastManager toasts, KitforgeThemeBinder binder, UIThemeConfig[] themes)
        {
            var menuGO = new GameObject("DemoMenuController");
            menuGO.transform.SetParent(root.transform, false);
            var menu = menuGO.AddComponent<DemoMenuController>();
            var so = new SerializedObject(menu);
            so.FindProperty("_managers.Popups").objectReferenceValue = popups;
            so.FindProperty("_managers.Screens").objectReferenceValue = screens;
            so.FindProperty("_managers.Toasts").objectReferenceValue = toasts;
            so.FindProperty("_managers.ThemeBinder").objectReferenceValue = binder;
            so.FindProperty("_themes.Default").objectReferenceValue = themes[0];
            so.FindProperty("_themes.Casual").objectReferenceValue = themes[1];
            so.FindProperty("_themes.Premium").objectReferenceValue = themes[2];
            so.FindProperty("_overlayParent").objectReferenceValue = root.transform;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureDirectory(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath);
            if (string.IsNullOrEmpty(dir) || AssetDatabase.IsValidFolder(dir)) return;
            var fullDir = Path.GetFullPath(dir);
            if (!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);
            AssetDatabase.Refresh();
        }
    }
}
