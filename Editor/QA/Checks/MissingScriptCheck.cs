using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public sealed class MissingScriptCheck : IQAIntegrityCheck
    {
        public string Name => "MissingScript";
        public bool SupportsScene => true;
        public bool SupportsPrefab => true;

        public void RunOnScene(string scenePath, QASuiteReport report)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) ScanRecursive(roots[i].transform, scenePath, null, report);
        }

        public void RunOnPrefab(GameObject prefabRoot, string prefabPath, QASuiteReport report)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(prefabPath);
            ScanRecursive(prefabRoot.transform, prefabPath, asset, report);
        }

        private static void ScanRecursive(Transform t, string targetPath, Object asset, QASuiteReport report)
        {
            CheckGameObject(t.gameObject, targetPath, asset, report);
            for (var i = 0; i < t.childCount; i++) ScanRecursive(t.GetChild(i), targetPath, asset, report);
        }

        private static void CheckGameObject(GameObject go, string targetPath, Object asset, QASuiteReport report)
        {
            var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count <= 0) return;
            report.Add(BuildEntry(go, count, targetPath, asset));
        }

        private static QAEntry BuildEntry(GameObject go, int count, string targetPath, Object asset)
        {
            return new QAEntry
            {
                TargetPath = targetPath,
                TargetLabel = Path.GetFileNameWithoutExtension(targetPath),
                CheckName = "MissingScript",
                Severity = QASeverity.Error,
                Message = $"{count} missing script(s) on '{go.name}'",
                OwnerHierarchy = QAHierarchy.Of(go.transform),
                OwnerAsset = asset
            };
        }
    }
}
