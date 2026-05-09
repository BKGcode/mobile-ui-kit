using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public static class UIKitAuditService
    {
        private static List<IUIKitCheck> _cachedChecks;

        public static UIKitAuditRunReport AuditAll(UIKitAuditOptions options)
        {
            options ??= new UIKitAuditOptions();
            var targets = ResolveTargets(options);
            return AuditTargets(targets, options);
        }

        public static UIKitAuditRunReport AuditOne(UIKitAuditTarget target, UIKitAuditOptions options)
        {
            options ??= new UIKitAuditOptions();
            return AuditTargets(new List<UIKitAuditTarget> { target }, options);
        }

        public static void InvalidateCache()
        {
            _cachedChecks = null;
        }

        private static List<UIKitAuditTarget> ResolveTargets(UIKitAuditOptions options)
        {
            var all = UIKitAuditDiscovery.DiscoverAll();
            return UIKitAuditDiscovery.Filter(all, options.Dimensions);
        }

        private static UIKitAuditRunReport AuditTargets(IList<UIKitAuditTarget> targets, UIKitAuditOptions options)
        {
            var run = NewRun(options);
            var theme = ResolveTheme(options);
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < targets.Count; i++) AuditOneInternal(targets, i, run, theme, options);
            sw.Stop();
            run.ElapsedMs = sw.ElapsedMilliseconds;
            return run;
        }

        private static void AuditOneInternal(IList<UIKitAuditTarget> targets, int i, UIKitAuditRunReport run, UIThemeConfig theme, UIKitAuditOptions options)
        {
            var target = targets[i];
            options.Progress?.Invoke(i, targets.Count, target.Label);
            var report = NewTargetReport(target);
            var sw = Stopwatch.StartNew();
            try { AuditTargetByKind(target, report, theme, options); }
            catch (Exception e) { AddFatalException(report, e); }
            sw.Stop();
            report.ElapsedMs = sw.ElapsedMilliseconds;
            run.Targets.Add(report);
        }

        private static void AuditTargetByKind(UIKitAuditTarget target, UIKitAuditTargetReport report, UIThemeConfig theme, UIKitAuditOptions options)
        {
            if (target.Kind == UIKitAuditTargetKind.Scene) AuditScene(target, report, options);
            else AuditPrefab(target, report, theme, options);
        }

        private static void AuditScene(UIKitAuditTarget target, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            try { EditorSceneManager.OpenScene(target.AssetPath, OpenSceneMode.Single); }
            catch (Exception e) { AddSceneOpenFailure(report, e); return; }
            RunChecksForTarget(target, null, report, options);
        }

        private static void AuditPrefab(UIKitAuditTarget target, UIKitAuditTargetReport report, UIThemeConfig theme, UIKitAuditOptions options)
        {
            var root = TryLoadPrefabContents(target, report);
            if (root == null) return;
            try { RunChecksForTarget(target, root, report, options); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            if (target.IsCatalog && (options.Dimensions & UIKitAuditDimensions.Visual) != 0) PrefabSnapshotCapture.Capture(target, theme, report);
        }

        private static GameObject TryLoadPrefabContents(UIKitAuditTarget target, UIKitAuditTargetReport report)
        {
            try { return PrefabUtility.LoadPrefabContents(target.AssetPath); }
            catch (Exception e) { AddLoadFailure(report, e); return null; }
        }

        private static void RunChecksForTarget(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            var checks = GetChecks();
            for (var i = 0; i < checks.Count; i++) InvokeCheck(checks[i], target, prefabRoot, report, options);
        }

        private static void InvokeCheck(IUIKitCheck check, UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            if (!check.AppliesTo(target)) return;
            try { check.Run(target, prefabRoot, report, options); }
            catch (Exception e) { AddCheckException(report, check, e); }
        }

        private static void AddCheckException(UIKitAuditTargetReport report, IUIKitCheck check, Exception e)
        {
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = check.Name,
                Scope = check.Scope,
                Severity = UIKitAuditSeverity.Error,
                Message = $"Threw {e.GetType().Name}: {e.Message}"
            });
        }

        private static void AddFatalException(UIKitAuditTargetReport report, Exception e)
        {
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "AuditFatal",
                Scope = UIKitCheckScope.Prefab,
                Severity = UIKitAuditSeverity.Error,
                Message = $"Threw {e.GetType().Name}: {e.Message}"
            });
        }

        private static void AddLoadFailure(UIKitAuditTargetReport report, Exception e)
        {
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "LoadPrefab",
                Scope = UIKitCheckScope.Prefab,
                Severity = UIKitAuditSeverity.Error,
                Message = $"PrefabUtility.LoadPrefabContents threw {e.GetType().Name}: {e.Message}"
            });
        }

        private static void AddSceneOpenFailure(UIKitAuditTargetReport report, Exception e)
        {
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "OpenScene",
                Scope = UIKitCheckScope.Scene,
                Severity = UIKitAuditSeverity.Error,
                Message = $"EditorSceneManager.OpenScene threw {e.GetType().Name}: {e.Message}"
            });
        }

        private static UIThemeConfig ResolveTheme(UIKitAuditOptions options)
        {
            if (options.SnapshotTheme != null) return options.SnapshotTheme;
            var guids = AssetDatabase.FindAssets("t:UIThemeConfig");
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<UIThemeConfig>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static UIKitAuditRunReport NewRun(UIKitAuditOptions options)
        {
            return new UIKitAuditRunReport
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                UnityVersion = Application.unityVersion,
                GitCommit = ResolveGitCommit(),
                Trigger = options.Trigger.ToString(),
                Scope = options.Scope ?? "All"
            };
        }

        private static UIKitAuditTargetReport NewTargetReport(UIKitAuditTarget target)
        {
            return new UIKitAuditTargetReport
            {
                Kind = target.Kind,
                Label = target.Label,
                AssetPath = target.AssetPath,
                Group = target.Group,
                TypeShortName = target.TypeShortName,
                TypeFullName = target.TypeFullName,
                IsCatalog = target.IsCatalog
            };
        }

        private static string ResolveGitCommit()
        {
            try { return RunGitRevParse(); }
            catch { return "unknown"; }
        }

        private static string RunGitRevParse()
        {
            var psi = new ProcessStartInfo("git", "rev-parse --short HEAD")
            {
                WorkingDirectory = Path.GetDirectoryName(Application.dataPath),
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (var p = Process.Start(psi))
            {
                if (p == null) return "unknown";
                var output = p.StandardOutput.ReadToEnd().Trim();
                p.WaitForExit(2000);
                return string.IsNullOrEmpty(output) ? "unknown" : output;
            }
        }

        private static List<IUIKitCheck> GetChecks()
        {
            if (_cachedChecks != null) return _cachedChecks;
            _cachedChecks = new List<IUIKitCheck>();
            var types = TypeCache.GetTypesDerivedFrom<IUIKitCheck>();
            for (var i = 0; i < types.Count; i++) TryInstantiateCheck(types[i]);
            return _cachedChecks;
        }

        private static void TryInstantiateCheck(Type type)
        {
            if (type.IsAbstract || type.IsInterface) return;
            if (type.GetConstructor(Type.EmptyTypes) == null) return;
            try { _cachedChecks.Add((IUIKitCheck)Activator.CreateInstance(type)); }
            catch (Exception e) { Debug.LogWarning($"[UIKitAudit] Failed to instantiate check '{type.Name}': {e.Message}"); }
        }
    }
}
