using System.Collections.Generic;
using System.IO;
using KitforgeLabs.UIKit.Core;
using KitforgeLabs.UIKit.HUD;
using KitforgeLabs.UIKit.Toast;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Generators
{
    internal static class KitforgeCatalogWireTool
    {
        private const string KitforgeRootPath = "Packages/com.kitforgelabs.mobile-ui-kit/Runtime/Bootstrap/KitforgeRoot.prefab";

        [MenuItem("KitforgeLabs/UI Kit/Maintenance/Wire Catalog Into KitforgeRoot (Dev)")]
        public static void WireAll()
        {
            var prefabsFolder = CatalogGroupBuilderShared.ResolvePrefabsFolder();
            if (!Directory.Exists(prefabsFolder)) { Debug.LogError($"[KitforgeCatalogWireTool] '{prefabsFolder}' does not exist. Run Regenerate Catalog Prefabs first."); return; }
            if (AssetDatabase.LoadAssetAtPath<GameObject>(KitforgeRootPath) == null) { Debug.LogError($"[KitforgeCatalogWireTool] KitforgeRoot.prefab not found at '{KitforgeRootPath}'."); return; }
            CollectPrefabs(prefabsFolder, out var popups, out var screens, out var toasts);
            var root = PrefabUtility.LoadPrefabContents(KitforgeRootPath);
            try
            {
                WireModuleRegistry(root.GetComponentInChildren<PopupManager>(true), "_popupPrefabs", popups);
                WireModuleRegistry(root.GetComponentInChildren<UIManager>(true), "_screenPrefabs", screens);
                WireToastRegistry(root.GetComponentInChildren<ToastManager>(true), "_toastPrefabs", toasts);
                PrefabUtility.SaveAsPrefabAsset(root, KitforgeRootPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
            EditorUtility.DisplayDialog(
                "KitforgeLabs · UI Kit",
                $"KitforgeRoot wired:\n• {popups.Count} popups\n• {screens.Count} screens\n• {toasts.Count} toasts\n\nServices: KitforgeRoot ships with empty Service refs — UIServices auto-instantiates Null Object defaults on Awake so the kit boots out-of-box. Wire your production services on UIServices for live data.\n\nHUD prefabs are not registry-wired (scene drag-drop only).\n\nCommit Runtime/Catalog/Prefabs/* and Runtime/Bootstrap/KitforgeRoot.prefab.",
                "OK");
        }

        private static void CollectPrefabs(string folder, out List<UIModuleBase> popups, out List<UIModuleBase> screens, out List<UIToastBase> toasts)
        {
            popups = new List<UIModuleBase>();
            screens = new List<UIModuleBase>();
            toasts = new List<UIToastBase>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                SortPrefab(go, popups, screens, toasts);
            }
        }

        private static void SortPrefab(GameObject go, List<UIModuleBase> popups, List<UIModuleBase> screens, List<UIToastBase> toasts)
        {
            var toast = go.GetComponent<UIToastBase>();
            if (toast != null) { toasts.Add(toast); return; }
            if (go.GetComponent<UIHUDBase>() != null) return;
            var module = go.GetComponent<UIModuleBase>();
            if (module == null) return;
            if (IsScreen(module)) screens.Add(module);
            else popups.Add(module);
        }

        private static bool IsScreen(UIModuleBase module)
        {
            var typeName = module.GetType().FullName ?? string.Empty;
            return typeName.Contains(".Catalog.Screens.");
        }

        private static void WireModuleRegistry(Component manager, string fieldName, IList<UIModuleBase> prefabs)
        {
            if (manager == null) { Debug.LogWarning($"[KitforgeCatalogWireTool] Manager not found for field '{fieldName}'. Skipped."); return; }
            var so = new SerializedObject(manager);
            var array = so.FindProperty(fieldName);
            if (array == null) { Debug.LogWarning($"[KitforgeCatalogWireTool] '{fieldName}' not found on {manager.GetType().Name}. Skipped."); return; }
            array.arraySize = prefabs.Count;
            for (var i = 0; i < prefabs.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireToastRegistry(Component manager, string fieldName, IList<UIToastBase> prefabs)
        {
            if (manager == null) { Debug.LogWarning($"[KitforgeCatalogWireTool] Manager not found for field '{fieldName}'. Skipped."); return; }
            var so = new SerializedObject(manager);
            var array = so.FindProperty(fieldName);
            if (array == null) { Debug.LogWarning($"[KitforgeCatalogWireTool] '{fieldName}' not found on {manager.GetType().Name}. Skipped."); return; }
            array.arraySize = prefabs.Count;
            for (var i = 0; i < prefabs.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
