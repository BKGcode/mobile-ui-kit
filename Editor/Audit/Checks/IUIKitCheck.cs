using UnityEngine;

namespace KitforgeLabs.MobileUIKit.Editor.Audit
{
    public interface IUIKitCheck
    {
        string Name { get; }
        UIKitCheckScope Scope { get; }
        bool AppliesTo(UIKitAuditTarget target);
        void Run(UIKitAuditTarget target, GameObject prefabRoot, UIKitAuditTargetReport report, UIKitAuditOptions options);
    }
}
