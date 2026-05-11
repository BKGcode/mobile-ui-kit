using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Hub.Catalog
{
    public static class KitforgeCatalogPrefabResolver
    {
        public static string ResolvePrefabPath(KitforgeCatalogEntry entry)
        {
            var folder = $"Assets/Catalog_Group{entry.Group}_Demo/Prefabs";
            if (!AssetDatabase.IsValidFolder(folder)) return null;
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { folder });
            return PickBestMatch(guids, entry);
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
