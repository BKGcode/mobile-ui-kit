using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Hub.Catalog
{
    public static class KitforgeCatalogPrefabResolver
    {
        private const string UpmPrefabsFolder = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Catalog/Prefabs";

        public static string ResolvePrefabPath(KitforgeCatalogEntry entry)
        {
            var folder = ResolveFolder();
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder)) return null;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            return PickBestMatch(guids, entry);
        }

        private static string ResolveFolder()
        {
            if (AssetDatabase.IsValidFolder(UpmPrefabsFolder)) return UpmPrefabsFolder;
            foreach (var guid in AssetDatabase.FindAssets("KitforgeLabs.UIKit.Catalog t:asmdef"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("/KitforgeLabs.UIKit.Catalog.asmdef")) continue;
                var dir = Path.GetDirectoryName(path).Replace('\\', '/');
                return $"{dir}/Prefabs";
            }
            return null;
        }

        public static GameObject LoadPrefab(KitforgeCatalogEntry entry)
        {
            var path = ResolvePrefabPath(entry);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        private static string PickBestMatch(string[] guids, KitforgeCatalogEntry entry)
        {
            string firstComponentMatch = null;
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null || go.GetComponent(entry.ComponentType) == null) continue;
                if (FilenameMatchesDisplay(path, entry.DisplayName)) return path;
                firstComponentMatch ??= path;
            }
            return firstComponentMatch;
        }

        private static bool FilenameMatchesDisplay(string path, string displayName)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            return string.Equals(fileName, displayName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
