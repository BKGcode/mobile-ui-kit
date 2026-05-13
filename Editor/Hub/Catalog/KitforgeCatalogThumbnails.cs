using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Hub.Catalog
{
    internal static class KitforgeCatalogThumbnails
    {
        private const string ScreenshotsFolder = "Packages/com.kitforgelabs.mobile-ui-kit/Documentation~/Screenshots";

        private static readonly Dictionary<Type, Texture2D> Cache = new Dictionary<Type, Texture2D>();
        private static bool _hookRegistered;

        public static Texture2D LoadFor(KitforgeCatalogEntry entry)
        {
            if (entry == null || entry.ComponentType == null) return null;
            EnsureCleanupHookRegistered();
            if (Cache.TryGetValue(entry.ComponentType, out var cached) && cached != null) return cached;
            var tex = LoadFromDisk(entry.ComponentType.Name);
            Cache[entry.ComponentType] = tex;
            return tex;
        }

        private static Texture2D LoadFromDisk(string typeName)
        {
            var dir = Path.GetFullPath(ScreenshotsFolder);
            if (!Directory.Exists(dir)) return null;
            var direct = Path.Combine(dir, typeName + ".png");
            if (File.Exists(direct)) return CreateTextureFromFile(direct);
            var matches = Directory.GetFiles(dir, "*_" + typeName + ".png");
            if (matches.Length == 0) return null;
            return CreateTextureFromFile(matches[0]);
        }

        private static Texture2D CreateTextureFromFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, name = "kf_thumb_" + Path.GetFileNameWithoutExtension(filePath) };
            if (tex.LoadImage(bytes)) return tex;
            UnityEngine.Object.DestroyImmediate(tex);
            return null;
        }

        private static void EnsureCleanupHookRegistered()
        {
            if (_hookRegistered) return;
            _hookRegistered = true;
            AssemblyReloadEvents.beforeAssemblyReload += DisposeAll;
        }

        private static void DisposeAll()
        {
            foreach (var tex in Cache.Values)
            {
                if (tex != null) UnityEngine.Object.DestroyImmediate(tex);
            }
            Cache.Clear();
        }
    }
}
