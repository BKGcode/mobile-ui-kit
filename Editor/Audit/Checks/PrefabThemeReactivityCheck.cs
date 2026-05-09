using KitforgeLabs.MobileUIKit.Theme;
using UnityEditor;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class PrefabThemeReactivityCheck : IUIKitCheck
    {
        public string Name => "ThemeReactivity";
        public UIKitCheckScope Scope => UIKitCheckScope.Catalog;

        public bool AppliesTo(UIKitAuditTarget target)
        {
            return target.IsCatalog;
        }

        public void Run(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            report.ThemeCheckRan = true;
            var themedImages = prefabRoot.GetComponentsInChildren<ThemedImage>(true);
            var themedTexts = prefabRoot.GetComponentsInChildren<ThemedText>(true);
            report.ThemedImageCount = themedImages.Length;
            report.ThemedTextCount = themedTexts.Length;
            if (themedImages.Length == 0 && themedTexts.Length == 0) { AddNoReactivityFinding(report); return; }
            report.ThemeReactivityPass = true;
            ValidateThemedImages(themedImages, report);
            ValidateThemedTexts(themedTexts, report);
        }

        private static void AddNoReactivityFinding(UIKitAuditTargetReport report)
        {
            report.ThemeReactivityPass = false;
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "ThemeReactivity",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Warning,
                Message = "No ThemedImage or ThemedText components found — prefab will not respond to theme swap",
                ChildPath = string.Empty,
                ComponentType = "ThemedImage|ThemedText"
            });
        }

        private static void ValidateThemedImages(ThemedImage[] images, UIKitAuditTargetReport report)
        {
            for (var i = 0; i < images.Length; i++) ValidateThemedImage(images[i], report);
        }

        private static void ValidateThemedImage(ThemedImage themed, UIKitAuditTargetReport report)
        {
            var so = new SerializedObject(themed);
            var imageProp = so.FindProperty("_image");
            if (imageProp == null || imageProp.objectReferenceValue != null) return;
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "ThemeReactivity",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Error,
                Message = "ThemedImage._image is unassigned — slot will not paint",
                ChildPath = ResolvePath(themed.transform),
                ComponentType = "ThemedImage"
            });
        }

        private static void ValidateThemedTexts(ThemedText[] texts, UIKitAuditTargetReport report)
        {
            for (var i = 0; i < texts.Length; i++) ValidateThemedText(texts[i], report);
        }

        private static void ValidateThemedText(ThemedText themed, UIKitAuditTargetReport report)
        {
            var so = new SerializedObject(themed);
            var textProp = so.FindProperty("_text");
            if (textProp == null || textProp.objectReferenceValue != null) return;
            report.Findings.Add(new UIKitAuditFinding
            {
                CheckName = "ThemeReactivity",
                Scope = UIKitCheckScope.Catalog,
                Severity = UIKitAuditSeverity.Error,
                Message = "ThemedText._text is unassigned — slot will not paint",
                ChildPath = ResolvePath(themed.transform),
                ComponentType = "ThemedText"
            });
        }

        private static string ResolvePath(Transform t)
        {
            var path = t.name;
            var parent = t.parent;
            while (parent != null) { path = parent.name + "/" + path; parent = parent.parent; }
            return path;
        }
    }
}
