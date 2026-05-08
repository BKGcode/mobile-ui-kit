using System.IO;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.QA
{
    public sealed class MissingReferenceCheck : IQAIntegrityCheck
    {
        public string Name => "MissingReference";
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
            var components = t.GetComponents<MonoBehaviour>();
            for (var i = 0; i < components.Length; i++) ScanComponent(components[i], t, targetPath, asset, report);
            for (var i = 0; i < t.childCount; i++) ScanRecursive(t.GetChild(i), targetPath, asset, report);
        }

        private static void ScanComponent(MonoBehaviour mb, Transform owner, string targetPath, Object asset, QASuiteReport report)
        {
            if (mb == null) return;
            var so = new SerializedObject(mb);
            var prop = so.GetIterator();
            while (prop.NextVisible(true)) Inspect(prop, mb, owner, targetPath, asset, report);
        }

        private static void Inspect(SerializedProperty prop, MonoBehaviour mb, Transform owner, string targetPath, Object asset, QASuiteReport report)
        {
            if (prop.propertyType != SerializedPropertyType.ObjectReference) return;
            if (prop.objectReferenceValue != null) return;
            if (prop.objectReferenceInstanceIDValue == 0) return;
            report.Add(BuildEntry(mb, prop, owner, targetPath, asset));
        }

        private static QAEntry BuildEntry(MonoBehaviour mb, SerializedProperty prop, Transform owner, string targetPath, Object asset)
        {
            return new QAEntry
            {
                TargetPath = targetPath,
                TargetLabel = Path.GetFileNameWithoutExtension(targetPath),
                CheckName = "MissingReference",
                Severity = QASeverity.Error,
                Message = $"{mb.GetType().Name}.{prop.propertyPath} -> broken reference (instanceID set, asset missing)",
                OwnerHierarchy = QAHierarchy.Of(owner),
                OwnerAsset = asset
            };
        }
    }
}
