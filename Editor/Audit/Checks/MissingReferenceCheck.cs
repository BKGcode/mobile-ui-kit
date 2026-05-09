using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class MissingReferenceCheck : IUIKitCheck
    {
        public string Name => "MissingReference";
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
            var components = t.GetComponents<MonoBehaviour>();
            for (var i = 0; i < components.Length; i++) ScanComponent(components[i], t, report, scope);
            for (var i = 0; i < t.childCount; i++) ScanRecursive(t.GetChild(i), report, scope);
        }

        private static void ScanComponent(MonoBehaviour mb, Transform owner, UIKitAuditTargetReport report, UIKitCheckScope scope)
        {
            if (mb == null) return;
            var so = new SerializedObject(mb);
            var prop = so.GetIterator();
            while (prop.NextVisible(true)) Inspect(prop, mb, owner, report, scope);
        }

        private static void Inspect(SerializedProperty prop, MonoBehaviour mb, Transform owner, UIKitAuditTargetReport report, UIKitCheckScope scope)
        {
            if (prop.propertyType != SerializedPropertyType.ObjectReference) return;
            if (prop.objectReferenceValue != null) return;
            if (prop.objectReferenceInstanceIDValue == 0) return;
            report.Findings.Add(BuildFinding(mb, prop, owner, scope));
        }

        private static UIKitAuditFinding BuildFinding(MonoBehaviour mb, SerializedProperty prop, Transform owner, UIKitCheckScope scope)
        {
            return new UIKitAuditFinding
            {
                CheckName = "MissingReference",
                Scope = scope,
                Severity = UIKitAuditSeverity.Error,
                Message = $"{mb.GetType().Name}.{prop.propertyPath} -> broken reference (instanceID set, asset missing)",
                ChildPath = HierarchyPath.Of(owner),
                ComponentType = mb.GetType().Name
            };
        }
    }
}
