using System.IO;
using System.Security.Cryptography;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.Theme;
using KitforgeLabs.MobileUIKit.Toast;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public static class PrefabSnapshotCapture
    {
        public const string SnapshotFolderRelative = "Assets/Editor/QASnapshots";
        public const int Width = 1080;
        public const int Height = 1920;

        public static void EnsureFolder()
        {
            if (AssetDatabase.IsValidFolder(SnapshotFolderRelative)) return;
            EnsureAssetFolderRecursive(SnapshotFolderRelative);
            AssetDatabase.Refresh();
        }

        public static void Clean()
        {
            if (!AssetDatabase.IsValidFolder(SnapshotFolderRelative)) return;
            AssetDatabase.DeleteAsset(SnapshotFolderRelative);
            EnsureFolder();
        }

        public static bool Capture(UIKitAuditTarget target, UIThemeConfig theme, UIKitAuditTargetReport report)
        {
            if (target.PrefabAsset == null) return false;
            EnsureFolder();
            var setup = SetupSnapshotScene();
            try { return CaptureWithSetup(setup, target, theme, report); }
            finally { TeardownSnapshotScene(setup); }
        }

        private static SnapshotSetup SetupSnapshotScene()
        {
            var previousActive = EditorSceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);
            var canvas = CreateCanvas();
            var camera = CreateCamera(canvas);
            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { name = "AuditRT" };
            camera.targetTexture = rt;
            SceneManager.MoveGameObjectToScene(canvas.gameObject, scene);
            SceneManager.MoveGameObjectToScene(camera.gameObject, scene);
            return new SnapshotSetup { Scene = scene, PreviousActive = previousActive, Canvas = canvas, Camera = camera, RenderTexture = rt };
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject("AuditCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Width, Height);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static Camera CreateCamera(Canvas canvas)
        {
            var go = new GameObject("AuditCamera", typeof(Camera));
            var camera = go.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = Height * 0.5f;
            camera.transform.position = new Vector3(0f, 0f, -100f);
            canvas.worldCamera = camera;
            canvas.planeDistance = 50f;
            return camera;
        }

        private static bool CaptureWithSetup(SnapshotSetup setup, UIKitAuditTarget target, UIThemeConfig theme, UIKitAuditTargetReport report)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(target.PrefabAsset, setup.Canvas.transform);
            if (instance == null) { ReportSnapshotFailure(report, "PrefabUtility.InstantiatePrefab returned null"); return false; }
            ApplyThemeToInstance(instance, theme);
            ForceVisibility(instance);
            setup.Camera.Render();
            var bytes = ReadRenderTexturePng(setup.RenderTexture);
            Object.DestroyImmediate(instance);
            return WriteSnapshot(target, bytes, report);
        }

        private static void ForceVisibility(GameObject instance)
        {
            var groups = instance.GetComponentsInChildren<CanvasGroup>(true);
            for (var i = 0; i < groups.Length; i++) { groups[i].alpha = 1f; groups[i].interactable = true; groups[i].blocksRaycasts = true; }
            var rect = instance.GetComponent<RectTransform>();
            if (rect != null) rect.localScale = Vector3.one;
        }

        private static void ApplyThemeToInstance(GameObject instance, UIThemeConfig theme)
        {
            if (theme == null) return;
            var module = instance.GetComponent<UIModuleBase>();
            if (module != null) { module.Initialize(theme, null); return; }
            var toast = instance.GetComponent<UIToastBase>();
            if (toast != null) { toast.Initialize(theme, null); return; }
            ApplyThemeFallback(instance, theme);
        }

        private static void ApplyThemeFallback(GameObject instance, UIThemeConfig theme)
        {
            var themed = instance.GetComponentsInChildren<IThemedElement>(true);
            for (var i = 0; i < themed.Length; i++) themed[i].ApplyTheme(theme);
        }

        private static byte[] ReadRenderTexturePng(RenderTexture rt)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            tex.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            var bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            return bytes;
        }

        private static bool WriteSnapshot(UIKitAuditTarget target, byte[] bytes, UIKitAuditTargetReport report)
        {
            if (bytes == null || bytes.Length == 0) { ReportSnapshotFailure(report, "PNG encode produced 0 bytes"); return false; }
            if (!SnapshotValidation.LooksRendered(bytes, out var diagnostic)) { ReportSnapshotFailure(report, diagnostic); return false; }
            var path = Path.Combine(SnapshotFolderRelative, target.Label + ".png").Replace('\\', '/');
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path);
            FillSnapshotReport(report, path, bytes);
            return true;
        }

        private static void FillSnapshotReport(UIKitAuditTargetReport report, string path, byte[] bytes)
        {
            report.SnapshotCaptured = true;
            report.SnapshotPath = path;
            report.SnapshotWidth = Width;
            report.SnapshotHeight = Height;
            report.SnapshotSha256 = ComputeSha256(bytes);
        }

        private static void ReportSnapshotFailure(UIKitAuditTargetReport report, string diagnostic)
        {
            report.SnapshotCaptured = false;
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "Snapshot",
                Scope = UIKitCheckScope.Visual,
                Severity = UIKitAuditSeverity.Error,
                Message = "Snapshot capture failed: " + diagnostic,
                ChildPath = string.Empty,
                ComponentType = string.Empty
            });
        }

        private static string ComputeSha256(byte[] bytes)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(bytes);
                var sb = new System.Text.StringBuilder(hash.Length * 2);
                for (var i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
        }

        private static void TeardownSnapshotScene(SnapshotSetup setup)
        {
            if (setup.Camera != null) setup.Camera.targetTexture = null;
            if (setup.RenderTexture != null) Object.DestroyImmediate(setup.RenderTexture);
            if (setup.Scene.IsValid()) EditorSceneManager.CloseScene(setup.Scene, true);
            if (setup.PreviousActive.IsValid()) EditorSceneManager.SetActiveScene(setup.PreviousActive);
        }

        private static void EnsureAssetFolderRecursive(string path)
        {
            var parts = path.Split('/');
            var built = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = built + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(built, parts[i]);
                built = next;
            }
        }

        private struct SnapshotSetup
        {
            public Scene Scene;
            public Scene PreviousActive;
            public Canvas Canvas;
            public Camera Camera;
            public RenderTexture RenderTexture;
        }
    }
}
