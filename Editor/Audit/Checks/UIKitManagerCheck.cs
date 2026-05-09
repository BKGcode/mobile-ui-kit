using KitforgeLabs.MobileUIKit.Editor.Validation;
using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public sealed class UIKitManagerCheck : IUIKitCheck
    {
        public string Name => "UIKitManager";
        public UIKitCheckScope Scope => UIKitCheckScope.Scene;

        public bool AppliesTo(UIKitAuditTarget target)
        {
            return target.Kind == UIKitAuditTargetKind.Scene;
        }

        public void Run(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options)
        {
            var validation = UIKitValidator.ValidateActiveScene();
            for (var i = 0; i < validation.Errors.Count; i++) report.Findings.Add(BuildFinding(validation.Errors[i], UIKitAuditSeverity.Error));
            for (var i = 0; i < validation.Warnings.Count; i++) report.Findings.Add(BuildFinding(validation.Warnings[i], UIKitAuditSeverity.Warning));
        }

        private static UIKitAuditFinding BuildFinding(string message, UIKitAuditSeverity severity)
        {
            return new UIKitAuditFinding
            {
                CheckName = "UIKitManager",
                Scope = UIKitCheckScope.Scene,
                Severity = severity,
                Message = message,
                ChildPath = string.Empty,
                ComponentType = string.Empty
            };
        }
    }
}
