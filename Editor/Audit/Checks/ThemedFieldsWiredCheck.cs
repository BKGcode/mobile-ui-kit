using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class ThemedFieldsWiredCheck : IUIKitCheck
    {
        public string Name => "ThemedFieldsWired";
        public UIKitCheckScope Scope => UIKitCheckScope.Prefab;

        public bool AppliesTo(UIKitAuditTarget target)
        {
            return target.Kind == UIKitAuditTargetKind.Prefab;
        }

        public void Run(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            ValidateThemedImages(prefabRoot, report);
            ValidateThemedTexts(prefabRoot, report);
        }

        private static void ValidateThemedImages(GameObject prefabRoot, UIKitAuditTargetReport report)
        {
            var components = prefabRoot.GetComponentsInChildren<ThemedImage>(true);
            for (var i = 0; i < components.Length; i++) ValidateBackingField(components[i], "_image", nameof(ThemedImage), report);
        }

        private static void ValidateThemedTexts(GameObject prefabRoot, UIKitAuditTargetReport report)
        {
            var components = prefabRoot.GetComponentsInChildren<ThemedText>(true);
            for (var i = 0; i < components.Length; i++) ValidateBackingField(components[i], "_text", nameof(ThemedText), report);
        }

        private static void ValidateBackingField(Component themed, string backingFieldName, string componentTypeName, UIKitAuditTargetReport report)
        {
            var so = new SerializedObject(themed);
            var prop = so.FindProperty(backingFieldName);
            if (prop != null && prop.objectReferenceValue != null) return;
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "ThemedFieldsWired",
                Scope = UIKitCheckScope.Prefab,
                Severity = UIKitAuditSeverity.Error,
                Message = $"{componentTypeName}.{backingFieldName} is null — Theme will not apply at runtime",
                ChildPath = HierarchyPath.Of(themed.transform),
                ComponentType = componentTypeName
            });
        }
    }
}
