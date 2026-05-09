using System;
using System.Collections.Generic;
using System.IO;
using KitforgeLabs.MobileUIKit.Core;
using KitforgeLabs.MobileUIKit.HUD;
using KitforgeLabs.MobileUIKit.Toast;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public static class UIKitAuditDiscovery
    {
        private const string CatalogPathPrefix = "Assets/Catalog_";

        public static List<UIKitAuditTarget> DiscoverAll()
        {
            var targets = new List<UIKitAuditTarget>();
            DiscoverScenes(targets);
            DiscoverCatalogPrefabs(targets);
            DiscoverNonCatalogPrefabs(targets);
            targets.Sort(CompareTargets);
            return targets;
        }

        public static List<UIKitAuditTarget> Filter(List<UIKitAuditTarget> targets, UIKitAuditDimensions dims)
        {
            var include = new List<UIKitAuditTarget>();
            for (var i = 0; i < targets.Count; i++) AppendIfDimensionMatch(include, targets[i], dims);
            return include;
        }

        public static List<string> ListCatalogGroups()
        {
            var groups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var all = DiscoverAll();
            for (var i = 0; i < all.Count; i++) if (all[i].IsCatalog) groups.Add(all[i].Group);
            var list = new List<string>(groups);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }

        private static void AppendIfDimensionMatch(List<UIKitAuditTarget> output, UIKitAuditTarget target, UIKitAuditDimensions dims)
        {
            if (target.Kind == UIKitAuditTargetKind.Scene && (dims & UIKitAuditDimensions.Scenes) != 0) { output.Add(target); return; }
            if (target.Kind != UIKitAuditTargetKind.Prefab) return;
            if (target.IsCatalog && (dims & UIKitAuditDimensions.Catalog) != 0) { output.Add(target); return; }
            if (!target.IsCatalog && (dims & UIKitAuditDimensions.PrefabStructural) != 0) output.Add(target);
        }

        private static void DiscoverScenes(List<UIKitAuditTarget> targets)
        {
            var guids = AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets" });
            for (var i = 0; i < guids.Length; i++) TryAddScene(guids[i], targets);
        }

        private static void TryAddScene(string guid, List<UIKitAuditTarget> targets)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith(CatalogPathPrefix)) return;
            targets.Add(new UIKitAuditTarget
            {
                Kind = UIKitAuditTargetKind.Scene,
                AssetPath = path,
                Label = Path.GetFileNameWithoutExtension(path),
                Group = ExtractGroup(path)
            });
        }

        private static void DiscoverCatalogPrefabs(List<UIKitAuditTarget> targets)
        {
            CollectCatalogFromBase<UIModuleBase>(targets);
            CollectCatalogFromBase<UIToastBase>(targets);
            CollectCatalogFromBase<UIHUDBase>(targets);
        }

        private static void CollectCatalogFromBase<TBase>(List<UIKitAuditTarget> targets) where TBase : MonoBehaviour
        {
            var types = TypeCache.GetTypesDerivedFrom<TBase>();
            for (var i = 0; i < types.Count; i++) TryCollectCatalogType(types[i], targets);
        }

        private static void TryCollectCatalogType(Type type, List<UIKitAuditTarget> targets)
        {
            if (type.IsAbstract) return;
            if (Attribute.IsDefined(type, typeof(UIKitAuditIgnoreAttribute), false)) return;
            var prefabPath = FindCatalogPrefabFor(type);
            if (string.IsNullOrEmpty(prefabPath)) return;
            if (TargetAlreadyTracked(targets, prefabPath)) return;
            targets.Add(BuildCatalogTarget(type, prefabPath));
        }

        private static bool TargetAlreadyTracked(List<UIKitAuditTarget> targets, string prefabPath)
        {
            for (var i = 0; i < targets.Count; i++) if (targets[i].AssetPath == prefabPath) return true;
            return false;
        }

        private static string FindCatalogPrefabFor(Type type)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!path.StartsWith(CatalogPathPrefix)) continue;
                if (PrefabHasComponent(path, type)) return path;
            }
            return null;
        }

        private static bool PrefabHasComponent(string path, Type type)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) return false;
            return asset.GetComponent(type) != null;
        }

        private static UIKitAuditTarget BuildCatalogTarget(Type type, string prefabPath)
        {
            return new UIKitAuditTarget
            {
                Kind = UIKitAuditTargetKind.Prefab,
                AssetPath = prefabPath,
                Label = type.Name,
                Group = ExtractGroup(prefabPath),
                TypeFullName = type.FullName,
                TypeShortName = type.Name,
                ComponentType = type,
                PrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath),
                IsCatalog = true
            };
        }

        private static void DiscoverNonCatalogPrefabs(List<UIKitAuditTarget> targets)
        {
            var demoFolders = ResolveDemoFolders(targets);
            for (var i = 0; i < demoFolders.Count; i++) AppendPrefabsInFolder(demoFolders[i], targets);
        }

        private static List<string> ResolveDemoFolders(List<UIKitAuditTarget> targets)
        {
            var folders = new HashSet<string>();
            for (var i = 0; i < targets.Count; i++) AppendDemoFolder(targets[i], folders);
            var list = new List<string>(folders);
            return list;
        }

        private static void AppendDemoFolder(UIKitAuditTarget target, HashSet<string> folders)
        {
            if (target.Kind != UIKitAuditTargetKind.Scene) return;
            var dir = Path.GetDirectoryName(target.AssetPath);
            if (dir == null) return;
            var sub = dir.Replace('\\', '/') + "/Prefabs";
            if (AssetDatabase.IsValidFolder(sub)) folders.Add(sub);
        }

        private static void AppendPrefabsInFolder(string folder, List<UIKitAuditTarget> targets)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            for (var i = 0; i < guids.Length; i++) TryAddNonCatalogPrefab(guids[i], targets);
        }

        private static void TryAddNonCatalogPrefab(string guid, List<UIKitAuditTarget> targets)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (TargetAlreadyTracked(targets, path)) return;
            targets.Add(new UIKitAuditTarget
            {
                Kind = UIKitAuditTargetKind.Prefab,
                AssetPath = path,
                Label = Path.GetFileNameWithoutExtension(path),
                Group = ExtractGroup(path),
                PrefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path),
                IsCatalog = false
            });
        }

        private static string ExtractGroup(string assetPath)
        {
            var dir = Path.GetDirectoryName(assetPath);
            if (dir == null) return "Unknown";
            var parts = dir.Replace('\\', '/').Split('/');
            for (var i = 0; i < parts.Length; i++)
            {
                if (parts[i].StartsWith("Catalog_")) return parts[i].Substring("Catalog_".Length);
            }
            return "Unknown";
        }

        private static int CompareTargets(UIKitAuditTarget a, UIKitAuditTarget b)
        {
            if (a.Kind != b.Kind) return a.Kind == UIKitAuditTargetKind.Scene ? -1 : 1;
            var byGroup = string.Compare(a.Group, b.Group, StringComparison.OrdinalIgnoreCase);
            if (byGroup != 0) return byGroup;
            return string.Compare(a.Label, b.Label, StringComparison.OrdinalIgnoreCase);
        }
    }
}
