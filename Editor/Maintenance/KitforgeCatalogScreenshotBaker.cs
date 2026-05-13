using System.IO;
using System.Reflection;
using KitforgeLabs.UIKit.Animation;
using KitforgeLabs.UIKit.Catalog.Confirm;
using KitforgeLabs.UIKit.Catalog.DailyLogin;
using KitforgeLabs.UIKit.Catalog.GameOver;
using KitforgeLabs.UIKit.Catalog.LevelComplete;
using KitforgeLabs.UIKit.Catalog.NotEnough;
using KitforgeLabs.UIKit.Catalog.Pause;
using KitforgeLabs.UIKit.Catalog.Reward;
using KitforgeLabs.UIKit.Catalog.Settings;
using KitforgeLabs.UIKit.Catalog.Shop;
using KitforgeLabs.UIKit.Catalog.Toasts;
using KitforgeLabs.UIKit.Catalog.Tutorial;
using KitforgeLabs.UIKit.Core;
using KitforgeLabs.UIKit.Editor.Hub.Catalog;
using KitforgeLabs.UIKit.Services;
using KitforgeLabs.UIKit.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace KitforgeLabs.UIKit.Editor.Maintenance
{
    public static class KitforgeCatalogScreenshotBaker
    {
        private const string OutputDirectory = "Packages/com.kitforgelabs.mobile-ui-kit/Documentation~/Screenshots";
        private const string ThemeDefaultPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Theme/Presets/Theme_Default.asset";
        private const string PrefabsFolder = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Catalog/Prefabs";
        private const int Width = 1080;
        private const int Height = 1920;

        private static readonly MethodInfo BindUntypedMethod = typeof(UIModuleBase).GetMethod("BindUntyped", BindingFlags.Instance | BindingFlags.NonPublic);

        [MenuItem("KitforgeLabs/UI Kit/Maintenance/Bake Catalog Screenshots (Dev)")]
        public static void Bake()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureOutputDirectory();
            var theme = AssetDatabase.LoadAssetAtPath<UIThemeConfig>(ThemeDefaultPath);
            if (theme == null) { Debug.LogError($"[KitforgeCatalogScreenshotBaker] Theme not found at {ThemeDefaultPath}."); return; }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var rig = BuildRig();
            try
            {
                foreach (var entry in KitforgeCatalogRegistry.All)
                {
                    if (entry.Pattern == KitforgeSpawnPattern.HUD) continue;
                    CaptureEntry(entry, rig, theme);
                }
                Debug.Log($"[KitforgeCatalogScreenshotBaker] Done. PNGs at {OutputDirectory}/ (commit to ship).");
            }
            finally
            {
                Object.DestroyImmediate(rig.RigRoot);
                if (rig.RenderTex != null) Object.DestroyImmediate(rig.RenderTex);
            }
        }

        private static void EnsureOutputDirectory()
        {
            var fullDir = Path.GetFullPath(OutputDirectory);
            if (!Directory.Exists(fullDir)) Directory.CreateDirectory(fullDir);
        }

        private struct CaptureRig
        {
            public GameObject RigRoot;
            public Camera Cam;
            public Canvas Canvas;
            public RenderTexture RenderTex;
        }

        private static CaptureRig BuildRig()
        {
            var rigRoot = new GameObject("BakeRig");
            var camGO = new GameObject("BakeCamera", typeof(Camera));
            camGO.transform.SetParent(rigRoot.transform, false);
            camGO.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camGO.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.13f, 0.17f, 1f);
            cam.orthographic = false;
            cam.fieldOfView = 60f;
            var canvasGO = new GameObject("BakeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(rigRoot.transform, false);
            ConfigureCanvas(canvasGO, cam);
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { name = "KitforgeBakeRT" };
            cam.targetTexture = rt;
            return new CaptureRig { RigRoot = rigRoot, Cam = cam, Canvas = canvasGO.GetComponent<Canvas>(), RenderTex = rt };
        }

        private static void ConfigureCanvas(GameObject canvasGO, Camera cam)
        {
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10f;
            canvas.sortingOrder = 0;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Width, Height);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static void CaptureEntry(KitforgeCatalogEntry entry, CaptureRig rig, UIThemeConfig theme)
        {
            var prefab = LoadPrefab(entry);
            if (prefab == null) { Debug.LogWarning($"[KitforgeCatalogScreenshotBaker] Prefab missing for {entry.DisplayName}. Skipped."); return; }
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rig.Canvas.transform);
            try
            {
                ResetTransformForCapture(instance);
                BindData(instance, entry, theme);
                Canvas.ForceUpdateCanvases();
                rig.Cam.Render();
                SaveRenderTextureAsPng(rig.RenderTex, $"{OutputDirectory}/{IndexFor(entry):00}_{entry.ComponentType.Name}.png");
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static GameObject LoadPrefab(KitforgeCatalogEntry entry)
        {
            var typeName = entry.ComponentType.Name;
            var path = $"{PrefabsFolder}/{typeName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null) return prefab;
            var alt = $"{PrefabsFolder}/{entry.DisplayName.Replace(" ", string.Empty)}.prefab";
            return AssetDatabase.LoadAssetAtPath<GameObject>(alt);
        }

        private static void ResetTransformForCapture(GameObject instance)
        {
            instance.SetActive(true);
            var rt = instance.transform as RectTransform;
            if (rt == null) return;
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            var cg = instance.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }

        private static void BindData(GameObject instance, KitforgeCatalogEntry entry, UIThemeConfig theme)
        {
            var module = instance.GetComponent<UIModuleBase>();
            if (module == null) return;
            module.Initialize(theme, null);
            var data = BuildMockData(entry);
            if (data != null && BindUntypedMethod != null) BindUntypedMethod.Invoke(module, new[] { data });
        }

        private static int IndexFor(KitforgeCatalogEntry entry)
        {
            var i = 0;
            foreach (var e in KitforgeCatalogRegistry.All)
            {
                i++;
                if (e == entry) return i;
            }
            return 0;
        }

        private static object BuildMockData(KitforgeCatalogEntry entry)
        {
            var name = entry.ComponentType.Name;
            switch (name)
            {
                case nameof(ConfirmPopup): return new ConfirmPopupData { Title = "Quit?", Message = "Progress will be lost.", ConfirmLabel = "Yes", CancelLabel = "Stay", Tone = ConfirmTone.Destructive };
                case nameof(PausePopup): return new PausePopupData { Title = "Paused", ShowResume = true, ShowRestart = true, ShowSettings = true, ShowQuit = true };
                case nameof(TutorialPopup): return BuildTutorialData();
                case nameof(RewardPopup): return new RewardPopupData { Title = "Daily Bonus!", Kind = RewardKind.Coins, Amount = 250, ClaimLabel = "Claim" };
                case nameof(ShopPopup): return new ShopPopupData { Title = "Shop", Category = ShopCategoryFilter.All };
                case nameof(NotEnoughCurrencyPopup): return new NotEnoughCurrencyPopupData { Currency = CurrencyType.Gems, Required = 100, Missing = 20, Title = "Not enough gems", ShowDecline = true };
                case nameof(DailyLoginPopup): return BuildDailyLoginData();
                case nameof(LevelCompletePopup): return new LevelCompletePopupData { Title = "Level Complete!", LevelLabel = "Level 1-3", Stars = 3, Score = 1250, BestScore = 1200, IsNewBest = true, ShowNext = true, ShowRetry = true };
                case nameof(GameOverPopup): return new GameOverPopupData { Title = "Game Over", Subtitle = "Better luck next time", Score = 850, ContinueMode = ContinueMode.Ad, ShowRestart = true, ShowMainMenu = true };
                case nameof(SettingsPopup): return new SettingsPopupData { Title = "Settings", ShowMusicSlider = true, ShowSfxSlider = true, ShowLanguagePicker = true, ShowNotificationsToggle = true, ShowHapticsToggle = true };
                case nameof(NotificationToast): return new NotificationToastData { Message = "Saved!", Severity = ToastSeverity.Success };
                default: return null;
            }
        }

        private static TutorialPopupData BuildTutorialData()
        {
            var d = new TutorialPopupData();
            d.Steps.Add(new TutorialStep { Title = "Welcome", Body = "Discover the catalog in 3 steps." });
            d.Steps.Add(new TutorialStep { Title = "Theme", Body = "Cycle the theme from top-right." });
            d.Steps.Add(new TutorialStep { Title = "Spawn", Body = "Quick spawn any popup from the side panel." });
            return d;
        }

        private static DailyLoginPopupData BuildDailyLoginData()
        {
            var entries = new DailyLoginRewardEntry[7];
            for (var i = 0; i < 7; i++)
            {
                entries[i] = new DailyLoginRewardEntry
                {
                    Label = $"Day {i + 1}",
                    IsBigReward = i == 6,
                    AllowDouble = i == 6,
                    Rewards = new[] { new RewardPopupData { Title = $"Day {i + 1}", Kind = RewardKind.Coins, Amount = (i + 1) * 100 } }
                };
            }
            return new DailyLoginPopupData { Title = "Daily Reward", RewardEntries = entries, CurrentDay = 3 };
        }

        private static void SaveRenderTextureAsPng(RenderTexture rt, string assetPath)
        {
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previous;
            var bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            File.WriteAllBytes(Path.GetFullPath(assetPath), bytes);
        }
    }
}
