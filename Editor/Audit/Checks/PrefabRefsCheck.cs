using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class PrefabRefsCheck : IUIKitCheck
    {
        private const string RefsFieldName = "_refs";

        public string Name => "Refs";
        public UIKitCheckScope Scope => UIKitCheckScope.Catalog;

        public bool AppliesTo(UIKitAuditTarget target)
        {
            return target.IsCatalog && target.ComponentType != null;
        }

        public void Run(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            var component = prefabRoot.GetComponent(target.ComponentType);
            if (component == null) { AddMissingComponent(report, target); return; }
            var so = new SerializedObject(component);
            var refsProp = so.FindProperty(RefsFieldName);
            if (refsProp == null) { ReportNoRefs(report); return; }
            report.RefsCheckRan = true;
            IterateRefs(refsProp, report);
        }

        private static void AddMissingComponent(UIKitAuditTargetReport report, UIKitAuditTarget target)
        {
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "Refs",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Error,
                Message = $"Component '{target.TypeShortName}' missing on prefab root",
                ChildPath = string.Empty,
                ComponentType = target.TypeShortName
            });
        }

        private static void ReportNoRefs(UIKitAuditTargetReport report)
        {
            report.RefsCheckRan = false;
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "Refs",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Info,
                Message = "No '_refs' field on component — Refs check skipped",
                ChildPath = string.Empty,
                ComponentType = string.Empty
            });
        }

        private static void IterateRefs(SerializedProperty refsProp, UIKitAuditTargetReport report)
        {
            var iterator = refsProp.Copy();
            var end = refsProp.GetEndProperty();
            iterator.NextVisible(true);
            while (!SerializedProperty.EqualContents(iterator, end))
            {
                ValidateRefField(iterator, report);
                if (!iterator.NextVisible(false)) break;
            }
        }

        private static void ValidateRefField(SerializedProperty field, UIKitAuditTargetReport report)
        {
            if (field.propertyType != SerializedPropertyType.ObjectReference) return;
            if (field.objectReferenceValue != null) return;
            var msg = field.objectReferenceInstanceIDValue != 0
                ? "Broken reference (target was deleted but field still wired)"
                : "Unassigned reference (Refs field is null in prefab)";
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "Refs",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Error,
                Message = msg,
                ChildPath = "_refs." + field.name,
                ComponentType = ResolveExpectedType(field)
            });
        }

        private static string ResolveExpectedType(SerializedProperty field)
        {
            var typeString = field.type;
            if (string.IsNullOrEmpty(typeString)) return string.Empty;
            return typeString.Replace("PPtr<$", string.Empty).Replace(">", string.Empty);
        }
    }
}
