using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public static class QASuiteBuilders
    {
        private const string BuilderMenuPrefix = "Tools/Kitforge/UI Kit/Build";

        public static List<string> DiscoverBuilderMenuPaths()
        {
            var paths = new List<string>();
            var methods = TypeCache.GetMethodsWithAttribute<MenuItem>();
            foreach (var m in methods) AppendMatching(m, paths);
            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        private static void AppendMatching(MethodInfo method, List<string> paths)
        {
            var attrs = method.GetCustomAttributes<MenuItem>(false);
            foreach (var attr in attrs) TryAppend(attr, paths);
        }

        private static void TryAppend(MenuItem attr, List<string> paths)
        {
            if (string.IsNullOrEmpty(attr.menuItem)) return;
            if (!attr.menuItem.StartsWith(BuilderMenuPrefix)) return;
            if (paths.Contains(attr.menuItem)) return;
            paths.Add(attr.menuItem);
        }

        public static int RebuildAll(Action<int, int, string> onProgress)
        {
            var paths = DiscoverBuilderMenuPaths();
            for (var i = 0; i < paths.Count; i++) RebuildOne(paths[i], i, paths.Count, onProgress);
            return paths.Count;
        }

        private static void RebuildOne(string path, int index, int total, Action<int, int, string> onProgress)
        {
            onProgress?.Invoke(index + 1, total, path);
            var ok = EditorApplication.ExecuteMenuItem(path);
            if (!ok) Debug.LogWarning($"[QASuite] ExecuteMenuItem failed: {path}");
        }
    }
}
