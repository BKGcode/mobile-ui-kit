using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public sealed class QATarget
    {
        public enum Kind
        {
            Scene,
            Prefab
        }

        public Kind TargetKind;
        public string AssetPath;
        public string Label;
    }

    public static class QASuiteRunner
    {
        private const string ScenePathPrefix = "Assets/Catalog_";
        private static List<IQAIntegrityCheck> _cachedChecks;

        public static List<QATarget> DiscoverTargets()
        {
            var targets = new List<QATarget>();
            var demoFolders = new HashSet<string>();
            DiscoverScenes(targets, demoFolders);
            DiscoverPrefabs(targets, demoFolders);
            return targets;
        }

        private static void DiscoverScenes(List<QATarget> targets, HashSet<string> demoFolders)
        {
            var guids = AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets" });
            for (var i = 0; i < guids.Length; i++) TryAddSceneTarget(guids[i], targets, demoFolders);
        }

        private static void TryAddSceneTarget(string guid, List<QATarget> targets, HashSet<string> demoFolders)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith(ScenePathPrefix)) return;
            targets.Add(new QATarget { TargetKind = QATarget.Kind.Scene, AssetPath = path, Label = Path.GetFileNameWithoutExtension(path) });
            demoFolders.Add(Path.GetDirectoryName(path).Replace('\\', '/'));
        }

        private static void DiscoverPrefabs(List<QATarget> targets, HashSet<string> demoFolders)
        {
            var folders = ResolvePrefabFolders(demoFolders);
            if (folders.Count == 0) return;
            var guids = AssetDatabase.FindAssets("t:Prefab", folders.ToArray());
            for (var i = 0; i < guids.Length; i++) AddPrefabTarget(guids[i], targets);
        }

        private static List<string> ResolvePrefabFolders(HashSet<string> demoFolders)
        {
            var folders = new List<string>();
            foreach (var d in demoFolders)
            {
                var sub = d + "/Prefabs";
                if (AssetDatabase.IsValidFolder(sub)) folders.Add(sub);
            }
            return folders;
        }

        private static void AddPrefabTarget(string guid, List<QATarget> targets)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            targets.Add(new QATarget { TargetKind = QATarget.Kind.Prefab, AssetPath = path, Label = Path.GetFileNameWithoutExtension(path) });
        }

        public static QASuiteReport RunAll(IList<QATarget> targets)
        {
            var report = new QASuiteReport();
            for (var i = 0; i < targets.Count; i++) RunOne(targets[i], report);
            return report;
        }

        public static void RunOne(QATarget target, QASuiteReport report)
        {
            var checks = GetChecks();
            if (target.TargetKind == QATarget.Kind.Scene) RunSceneTarget(target, checks, report);
            else RunPrefabTarget(target, checks, report);
        }

        private static void RunSceneTarget(QATarget target, IList<IQAIntegrityCheck> checks, QASuiteReport report)
        {
            try { EditorSceneManager.OpenScene(target.AssetPath, OpenSceneMode.Single); }
            catch (Exception e) { ReportException("OpenScene", target, e, report); return; }
            for (var i = 0; i < checks.Count; i++) InvokeSceneCheck(checks[i], target, report);
        }

        private static void InvokeSceneCheck(IQAIntegrityCheck check, QATarget target, QASuiteReport report)
        {
            if (!check.SupportsScene) return;
            try { check.RunOnScene(target.AssetPath, report); }
            catch (Exception e) { ReportException(check, target, e, report); }
        }

        private static void RunPrefabTarget(QATarget target, IList<IQAIntegrityCheck> checks, QASuiteReport report)
        {
            GameObject root;
            try { root = PrefabUtility.LoadPrefabContents(target.AssetPath); }
            catch (Exception e) { ReportException("LoadPrefab", target, e, report); return; }
            try
            {
                for (var i = 0; i < checks.Count; i++) InvokePrefabCheck(checks[i], root, target, report);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void InvokePrefabCheck(IQAIntegrityCheck check, GameObject root, QATarget target, QASuiteReport report)
        {
            if (!check.SupportsPrefab) return;
            try { check.RunOnPrefab(root, target.AssetPath, report); }
            catch (Exception e) { ReportException(check, target, e, report); }
        }

        private static void ReportException(IQAIntegrityCheck check, QATarget target, Exception e, QASuiteReport report)
        {
            ReportException(check.Name, target, e, report);
        }

        private static void ReportException(string source, QATarget target, Exception e, QASuiteReport report)
        {
            report.Add(new QAEntry
            {
                TargetPath = target.AssetPath,
                TargetLabel = target.Label,
                CheckName = source,
                Severity = QASeverity.Error,
                Message = $"Threw {e.GetType().Name}: {e.Message}",
                OwnerHierarchy = string.Empty,
                OwnerAsset = AssetDatabase.LoadMainAssetAtPath(target.AssetPath)
            });
        }

        private static IList<IQAIntegrityCheck> GetChecks()
        {
            if (_cachedChecks != null) return _cachedChecks;
            _cachedChecks = new List<IQAIntegrityCheck>();
            var types = TypeCache.GetTypesDerivedFrom<IQAIntegrityCheck>();
            for (var i = 0; i < types.Count; i++) TryInstantiateCheck(types[i]);
            return _cachedChecks;
        }

        private static void TryInstantiateCheck(Type t)
        {
            if (t.IsAbstract || t.IsInterface) return;
            try { _cachedChecks.Add((IQAIntegrityCheck)Activator.CreateInstance(t)); }
            catch (Exception e) { Debug.LogWarning($"[QASuite] Failed to instantiate check '{t.Name}': {e.Message}"); }
        }

        public static void InvalidateCache()
        {
            _cachedChecks = null;
        }
    }
}
