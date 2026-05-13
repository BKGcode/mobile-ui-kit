using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace KitforgeLabs.UIKit.Editor.Maintenance
{
    internal static class KitforgeCatalogPrefabsRegenerator
    {
        [MenuItem("Tools/KitforgeLabs/Test/Regenerate Catalog Prefabs")]
        public static void RegenerateAll()
        {
            var folder = CatalogGroupBuilderShared.ResolvePrefabsFolder();
            if (!ConfirmWritable(folder)) return;
            CatalogGroupABuilder.BuildAll();
            CatalogGroupBBuilder.BuildAll();
            CatalogGroupCBuilder.BuildAll();
            CatalogGroupDBuilder.BuildAll();
            CatalogGroupEBuilder.BuildAll();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "KitforgeLabs · UI Kit",
                $"17 catalog prefabs regenerated at {folder}.\n\nNext step: KitforgeLabs → UI Kit → Maintenance → Wire Catalog Into KitforgeRoot (Dev).",
                "OK");
        }

        private static bool ConfirmWritable(string folder)
        {
            var pkg = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(folder);
            if (pkg == null) return true;
            if (pkg.source == UnityEditor.PackageManager.PackageSource.Embedded || pkg.source == PackageSource.Local) return true;
            Debug.LogError($"[KitforgeCatalogPrefabsRegenerator] Target folder '{folder}' belongs to a package installed from '{pkg.source}' which is read-only. Embed the package (move to Packages/ folder) or reference it via 'file:...' in manifest.json, then re-run this menu.");
            return false;
        }
    }
}
