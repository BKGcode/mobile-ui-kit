using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class MissingScriptCheck : IUIKitCheck
    {
        public string Name => "MissingScript";
        public UIKitCheckScope Scope => UIKitCheckScope.Prefab;

        public bool AppliesTo(UIKitAuditTarget target)
        {
            return true;
        }

        public void Run(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            if (target.Kind == UIKitAuditTargetKind.Scene) ScanScene(report);
            else if (prefabRoot != null) ScanPrefab(prefabRoot, report);
        }

        private static void ScanScene(UIKitAuditTargetReport report)
        {
            var scene = EditorSceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++) ScanRecursive(roots[i].transform, report, UIKitCheckScope.Scene);
        }

        private static void ScanPrefab(GameObject prefabRoot, UIKitAuditTargetReport report)
        {
            ScanRecursive(prefabRoot.transform, report, UIKitCheckScope.Prefab);
        }

        private static void ScanRecursive(Transform t, UIKitAuditTargetReport report, UIKitCheckScope scope)
        {
            CheckGameObject(t.gameObject, report, scope);
            for (var i = 0; i < t.childCount; i++) ScanRecursive(t.GetChild(i), report, scope);
        }

        private static void CheckGameObject(GameObject go, UIKitAuditTargetReport report, UIKitCheckScope scope)
        {
            var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count <= 0) return;
            report.Findings.Add(BuildFinding(go, count, scope));
        }

        private static UIKitAuditFinding BuildFinding(GameObject go, int count, UIKitCheckScope scope)
        {
            return new UIKitAuditFinding
            {
                CheckName = "MissingScript",
                Scope = scope,
                Severity = UIKitAuditSeverity.Error,
                Message = $"{count} missing script(s) on '{go.name}'",
                ChildPath = HierarchyPath.Of(go.transform),
                ComponentType = string.Empty
            };
        }
    }
}
