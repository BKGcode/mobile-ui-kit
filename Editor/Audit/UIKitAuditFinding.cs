using System;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    [Serializable]
    public sealed class UIKitAuditFinding
    {
        public string CheckName;
        public UIKitCheckScope Scope;
        public UIKitAuditSeverity Severity;
        public string Message;
        public string ChildPath;
        public string ComponentType;
    }
}
